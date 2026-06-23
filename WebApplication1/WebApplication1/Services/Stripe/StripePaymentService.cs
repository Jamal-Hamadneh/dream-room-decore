using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Exceptions.Payments;
using WebApplication1.Models;
using WebApplication1.Options;

namespace WebApplication1.Services;

public interface IStripePaymentService
{
    StripeConfigResponse GetConfig();
    Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request, CancellationToken cancellationToken = default);
    Task<SyncPaymentIntentResponse> SyncPaymentIntentAsync(int userId, SyncPaymentIntentRequest request, CancellationToken cancellationToken = default);
    Task HandleWebhookAsync(string json, string? stripeSignature, CancellationToken cancellationToken = default);
}

public class StripePaymentService(ApplicationDbContext context, IOptions<StripeOptions> options, IWebHostEnvironment environment) : IStripePaymentService
{
    private readonly StripeOptions _options = options.Value;
    private readonly Dictionary<string, string> _localEnv = LoadLocalEnv(Path.Combine(environment.ContentRootPath, ".env"));

    public StripeConfigResponse GetConfig()
    {
        var publishableKey = PublishableKey;
        return new StripeConfigResponse
        {
            PublishableKey = publishableKey,
            Currency = Currency,
            IsConfigured = !string.IsNullOrWhiteSpace(SecretKey) && !string.IsNullOrWhiteSpace(publishableKey)
        };
    }

    public async Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(int userId, CreatePaymentIntentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var order = await context.Orders
            .Include(order => order.Payment)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId && order.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"Order '{request.OrderId}' was not found.");

        if (order.PaymentStatus == "paid")
        {
            throw new StripePaymentException("This order is already paid.");
        }

        StripeConfiguration.ApiKey = SecretKey;
        var paymentIntentService = new PaymentIntentService();
        PaymentIntent paymentIntent;

        if (!string.IsNullOrWhiteSpace(order.StripePaymentIntentId))
        {
            paymentIntent = await paymentIntentService.GetAsync(order.StripePaymentIntentId, cancellationToken: cancellationToken);
        }
        else
        {
            paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(order.TotalPrice),
                Currency = Currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString(),
                    ["userId"] = userId.ToString()
                }
            }, cancellationToken: cancellationToken);

            order.StripePaymentIntentId = paymentIntent.Id;
        }

        var payment = order.Payment;
        if (payment is null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Payments.Add(payment);
        }

        payment.StripePaymentIntentId = paymentIntent.Id;
        payment.Amount = order.TotalPrice;
        payment.Currency = Currency;
        payment.Status = ToLocalPaymentStatus(paymentIntent.Status);
        order.PaymentStatus = ToOrderPaymentStatus(paymentIntent.Status);
        order.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new CreatePaymentIntentResponse
        {
            OrderId = order.Id,
            PaymentId = payment.Id,
            PaymentIntentId = paymentIntent.Id,
            ClientSecret = paymentIntent.ClientSecret,
            Amount = order.TotalPrice,
            Currency = Currency,
            Status = payment.Status,
            PublishableKey = PublishableKey
        };
    }

    public async Task<SyncPaymentIntentResponse> SyncPaymentIntentAsync(int userId, SyncPaymentIntentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        StripeConfiguration.ApiKey = SecretKey;

        var paymentIntent = await new PaymentIntentService().GetAsync(request.PaymentIntentId, cancellationToken: cancellationToken);
        var order = await context.Orders
            .Include(order => order.Payment)
            .FirstOrDefaultAsync(order => order.StripePaymentIntentId == request.PaymentIntentId && order.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Payment intent was not found for the current user.");

        var payment = await UpdateLocalPaymentAsync(order, paymentIntent, cancellationToken);

        return new SyncPaymentIntentResponse
        {
            OrderId = order.Id,
            PaymentId = payment.Id,
            PaymentIntentId = paymentIntent.Id,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.Status,
            StripeStatus = paymentIntent.Status
        };
    }

    public async Task HandleWebhookAsync(string json, string? stripeSignature, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var webhookSecret = WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new StripePaymentException("Stripe webhook secret is not configured.");
        }

        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return;
        }

        if (stripeEvent.Type is not ("payment_intent.succeeded" or "payment_intent.payment_failed" or "payment_intent.canceled"))
        {
            return;
        }

        var order = await context.Orders
            .Include(order => order.Payment)
            .FirstOrDefaultAsync(order => order.StripePaymentIntentId == paymentIntent.Id, cancellationToken);

        if (order is null)
        {
            return;
        }

        await UpdateLocalPaymentAsync(order, paymentIntent, cancellationToken);
    }

    private async Task<Payment> UpdateLocalPaymentAsync(Order order, PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var payment = order.Payment ?? await context.Payments.FirstOrDefaultAsync(payment => payment.OrderId == order.Id, cancellationToken);
        if (payment is null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Payments.Add(payment);
        }

        payment.StripePaymentIntentId = paymentIntent.Id;
        payment.Amount = FromStripeAmount(paymentIntent.AmountReceived > 0 ? paymentIntent.AmountReceived : paymentIntent.Amount);
        payment.Currency = paymentIntent.Currency;
        payment.Status = ToLocalPaymentStatus(paymentIntent.Status);
        order.PaymentStatus = ToOrderPaymentStatus(paymentIntent.Status);
        order.Status = order.PaymentStatus == "paid" && order.Status == "pending" ? "processing" : order.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    private string SecretKey => FirstValue(_options.SecretKey, _localEnv, "STRIPE_SECRET_KEY");
    private string PublishableKey => FirstValue(_options.PublishableKey, _localEnv, "STRIPE_PUBLISHABLE_KEY");
    private string WebhookSecret => FirstValue(_options.WebhookSecret, _localEnv, "STRIPE_WEBHOOK_SECRET");
    private string Currency => string.IsNullOrWhiteSpace(_options.Currency) ? "usd" : _options.Currency.ToLowerInvariant();

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new StripePaymentException("Stripe secret key is not configured.");
        }
    }

    private static long ToStripeAmount(decimal amount) => (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    private static decimal FromStripeAmount(long amount) => amount / 100m;

    private static string ToLocalPaymentStatus(string stripeStatus) => stripeStatus switch
    {
        "succeeded" => "succeeded",
        "canceled" => "failed",
        _ => "pending"
    };

    private static string ToOrderPaymentStatus(string stripeStatus) => stripeStatus switch
    {
        "succeeded" => "paid",
        "canceled" => "failed",
        _ => "pending"
    };

    private static Dictionary<string, string> LoadLocalEnv(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return new Dictionary<string, string>();
        }

        return System.IO.File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#'))
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
    }

    private static string FirstValue(string configuredValue, Dictionary<string, string> localEnv, string key)
    {
        if (!string.IsNullOrWhiteSpace(configuredValue))
        {
            return configuredValue;
        }

        return localEnv.TryGetValue(key, out var value) ? value : string.Empty;
    }
}

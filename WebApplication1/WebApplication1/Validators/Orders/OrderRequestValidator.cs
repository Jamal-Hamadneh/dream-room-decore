using FluentValidation;
using WebApplication1.Contracts.Requests;

namespace WebApplication1.Validators;

public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.AddressId).GreaterThan(0);
        RuleFor(x => x.TotalPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).Must(x => x is "pending" or "processing" or "shipped" or "delivered" or "cancelled");
        RuleFor(x => x.PaymentStatus).Must(x => x is "pending" or "paid" or "failed");
    }
}

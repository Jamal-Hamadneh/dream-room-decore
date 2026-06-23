using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Payments;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IPaymentService : ICrudService<PaymentRequest, PaymentResponse>;

public class PaymentService(IPaymentRepository repository, ICrudMapper<Payment, PaymentRequest, PaymentResponse> mapper, ApplicationDbContext context)
    : CrudService<Payment, PaymentRequest, PaymentResponse>(repository, mapper), IPaymentService
{
    public override async Task<PaymentResponse> CreateAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await context.Payments.AnyAsync(payment => payment.OrderId == request.OrderId, cancellationToken);
        if (exists)
        {
            throw new OrderPaymentAlreadyExistsException(request.OrderId);
        }

        return await base.CreateAsync(request, cancellationToken);
    }
}

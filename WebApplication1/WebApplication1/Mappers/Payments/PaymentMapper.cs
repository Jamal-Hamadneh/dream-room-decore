using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class PaymentMapper : ICrudMapper<Payment, PaymentRequest, PaymentResponse>
{
    public partial Payment ToEntity(PaymentRequest request);
    public partial void UpdateEntity([MappingTarget] Payment entity, PaymentRequest request);
    public partial PaymentResponse ToResponse(Payment entity);
}

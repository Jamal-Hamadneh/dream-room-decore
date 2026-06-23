using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/payments")]
public class PaymentsController(IPaymentService service, IValidator<PaymentRequest> validator) : CrudController<PaymentRequest, PaymentResponse>(service, validator);

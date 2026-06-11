using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/orders")]
public class OrdersController(IOrderService service, IValidator<OrderRequest> validator) : CrudController<OrderRequest, OrderResponse>(service, validator);

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/order-items")]
public class OrderItemsController(IOrderItemService service, IValidator<OrderItemRequest> validator) : CrudController<OrderItemRequest, OrderItemResponse>(service, validator);

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/cart-items")]
public class CartItemsController(ICartItemService service, IValidator<CartItemRequest> validator) : CrudController<CartItemRequest, CartItemResponse>(service, validator);

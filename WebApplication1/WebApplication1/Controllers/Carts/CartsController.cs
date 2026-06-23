using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/cart")]
public class CartsController(ICartService service, IValidator<CartRequest> validator) : CrudController<CartRequest, CartResponse>(service, validator);

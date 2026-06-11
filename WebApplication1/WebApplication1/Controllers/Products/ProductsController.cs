using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/products")]
public class ProductsController(IProductService service, IValidator<ProductRequest> validator) : CrudController<ProductRequest, ProductResponse>(service, validator);

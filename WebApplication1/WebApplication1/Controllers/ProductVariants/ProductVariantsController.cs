using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/product-variants")]
public class ProductVariantsController(IProductVariantService service, IValidator<ProductVariantRequest> validator) : CrudController<ProductVariantRequest, ProductVariantResponse>(service, validator);

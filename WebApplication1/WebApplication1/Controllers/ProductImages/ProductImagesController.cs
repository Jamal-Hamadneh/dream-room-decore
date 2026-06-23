using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/product-images")]
public class ProductImagesController(IProductImageService service, IValidator<ProductImageRequest> validator) : CrudController<ProductImageRequest, ProductImageResponse>(service, validator);

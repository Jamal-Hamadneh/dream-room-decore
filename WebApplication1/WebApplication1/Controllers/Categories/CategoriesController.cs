using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/categories")]
public class CategoriesController(ICategoryService service, IValidator<CategoryRequest> validator) : CrudController<CategoryRequest, CategoryResponse>(service, validator);

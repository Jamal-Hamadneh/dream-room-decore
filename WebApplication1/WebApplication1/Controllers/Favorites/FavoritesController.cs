using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/favorites")]
public class FavoritesController(IFavoriteService service, IValidator<FavoriteRequest> validator) : CrudController<FavoriteRequest, FavoriteResponse>(service, validator);

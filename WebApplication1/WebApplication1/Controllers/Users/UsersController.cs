using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/users")]
public class UsersController(IUserService service, IValidator<UserRequest> validator) : CrudController<UserRequest, UserResponse>(service, validator);

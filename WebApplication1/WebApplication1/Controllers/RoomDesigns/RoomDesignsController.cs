using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/room-designs")]
public class RoomDesignsController(IRoomDesignService service, IValidator<RoomDesignRequest> validator) : CrudController<RoomDesignRequest, RoomDesignResponse>(service, validator);

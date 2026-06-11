using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/room-furniture-placements")]
public class RoomFurniturePlacementsController(IRoomFurniturePlacementService service, IValidator<RoomFurniturePlacementRequest> validator) : CrudController<RoomFurniturePlacementRequest, RoomFurniturePlacementResponse>(service, validator);

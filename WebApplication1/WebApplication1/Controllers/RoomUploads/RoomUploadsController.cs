using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/room-uploads")]
public class RoomUploadsController(IRoomUploadService service, IValidator<RoomUploadRequest> validator) : CrudController<RoomUploadRequest, RoomUploadResponse>(service, validator);

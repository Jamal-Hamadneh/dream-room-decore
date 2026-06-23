using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/addresses")]
public class AddressesController(IAddressService service, IValidator<AddressRequest> validator) : CrudController<AddressRequest, AddressResponse>(service, validator);

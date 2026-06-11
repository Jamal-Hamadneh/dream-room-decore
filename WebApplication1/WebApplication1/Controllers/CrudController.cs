using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
public abstract class CrudController<TRequest, TResponse>(
    ICrudService<TRequest, TResponse> service,
    IValidator<TRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await service.GetByIdAsync(id, cancellationToken);
        return response is null ? throw new NotFoundException($"Resource with id '{id}' was not found.") : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<TResponse>> Create(TRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        var response = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = GetResponseId(response) }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TResponse>> Update(int id, TRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        var response = await service.UpdateAsync(id, request, cancellationToken);
        return response is null ? throw new NotFoundException($"Resource with id '{id}' was not found.") : Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await service.DeleteAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Resource with id '{id}' was not found.");
        }

        return NoContent();
    }

    private static int GetResponseId(TResponse response)
    {
        var id = typeof(TResponse).GetProperty("Id")?.GetValue(response);
        return id is int value ? value : 0;
    }
}

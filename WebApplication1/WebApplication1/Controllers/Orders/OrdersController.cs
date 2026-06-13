using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[Route("api/orders")]
public class OrdersController(IOrderService service, IValidator<OrderRequest> validator) : CrudController<OrderRequest, OrderResponse>(service, validator)
{
    public override async Task<ActionResult<List<OrderResponse>>> GetAll(int? page, int? limit, string? sort, CancellationToken cancellationToken)
    {
        if (page is null && limit is null && sort is null)
        {
            return await base.GetAll(page, limit, sort, cancellationToken);
        }

        return Ok(await service.GetPagedAsync(page ?? 1, limit ?? 5, sort, cancellationToken));
    }
}

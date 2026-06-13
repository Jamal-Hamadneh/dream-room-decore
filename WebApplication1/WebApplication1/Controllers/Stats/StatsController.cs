using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("api/stats")]
public class StatsController(IStatsService statsService) : ControllerBase
{
    [HttpGet("summary")]
    public Task<StatsSummaryResponse> GetSummary(CancellationToken cancellationToken)
    {
        return statsService.GetSummaryAsync(cancellationToken);
    }

    [HttpGet("revenue")]
    public Task<MonthlyRevenueResponse> GetRevenue([FromQuery] int? year, CancellationToken cancellationToken)
    {
        return statsService.GetMonthlyRevenueAsync(year ?? DateTime.UtcNow.Year, cancellationToken);
    }

    [HttpGet("categories")]
    public Task<List<CategorySalesResponse>> GetCategories(CancellationToken cancellationToken)
    {
        return statsService.GetCategorySalesAsync(cancellationToken);
    }
}

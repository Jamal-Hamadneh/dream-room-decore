namespace WebApplication1.Contracts.Responses;

public class StatsSummaryResponse
{
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

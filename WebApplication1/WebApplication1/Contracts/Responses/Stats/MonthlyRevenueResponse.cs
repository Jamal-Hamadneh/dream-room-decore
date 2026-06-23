namespace WebApplication1.Contracts.Responses;

public class MonthlyRevenueResponse
{
    public int Year { get; set; }
    public List<MonthlyRevenuePoint> Months { get; set; } = new();
}

public class MonthlyRevenuePoint
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

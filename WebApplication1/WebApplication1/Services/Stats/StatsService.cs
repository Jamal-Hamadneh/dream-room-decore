using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;

namespace WebApplication1.Services;

public interface IStatsService
{
    Task<StatsSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<MonthlyRevenueResponse> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default);
    Task<List<CategorySalesResponse>> GetCategorySalesAsync(CancellationToken cancellationToken = default);
}

public class StatsService(ApplicationDbContext context) : IStatsService
{
    private const string PaidStatus = "paid";

    public async Task<StatsSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var paidOrders = context.Orders.AsNoTracking().Where(order => order.PaymentStatus == PaidStatus);

        var orderCount = await paidOrders.CountAsync(cancellationToken);
        var totalRevenue = orderCount == 0 ? 0 : await paidOrders.SumAsync(order => order.TotalPrice, cancellationToken);
        var customerCount = await context.Users.AsNoTracking().CountAsync(user => user.Role == "customer", cancellationToken);

        return new StatsSummaryResponse
        {
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            CustomerCount = customerCount,
            AverageOrderValue = orderCount == 0 ? 0 : totalRevenue / orderCount
        };
    }

    public async Task<MonthlyRevenueResponse> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default)
    {
        var monthlyTotals = await context.Orders
            .AsNoTracking()
            .Where(order => order.PaymentStatus == PaidStatus && order.CreatedAt.Year == year)
            .GroupBy(order => order.CreatedAt.Month)
            .Select(group => new { Month = group.Key, Revenue = group.Sum(order => order.TotalPrice) })
            .ToListAsync(cancellationToken);

        var months = Enumerable.Range(1, 12)
            .Select(month => new MonthlyRevenuePoint
            {
                Month = month,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month),
                Revenue = monthlyTotals.FirstOrDefault(total => total.Month == month)?.Revenue ?? 0
            })
            .ToList();

        return new MonthlyRevenueResponse { Year = year, Months = months };
    }

    public async Task<List<CategorySalesResponse>> GetCategorySalesAsync(CancellationToken cancellationToken = default)
    {
        var categoryTotals = await context.OrderItems
            .AsNoTracking()
            .Where(item => item.Order.PaymentStatus == PaidStatus)
            .GroupBy(item => new { item.Product.CategoryId, item.Product.Category.Name })
            .Select(group => new { group.Key.CategoryId, group.Key.Name, Revenue = group.Sum(item => item.Price * item.Quantity) })
            .ToListAsync(cancellationToken);

        var totalRevenue = categoryTotals.Sum(category => category.Revenue);

        return categoryTotals
            .OrderByDescending(category => category.Revenue)
            .Select(category => new CategorySalesResponse
            {
                CategoryId = category.CategoryId,
                CategoryName = category.Name,
                Revenue = category.Revenue,
                Percentage = totalRevenue == 0 ? 0 : Math.Round(category.Revenue / totalRevenue * 100, 2)
            })
            .ToList();
    }
}

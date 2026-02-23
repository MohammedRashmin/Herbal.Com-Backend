using Web.Com.DTOs.Admin;
using Web.Com.Repositories.Interfaces.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Services.Implementations.Admin;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var totalOrders = await _dashboardRepository.GetTotalOrdersCountAsync();
        var pendingOrders = await _dashboardRepository.GetPendingOrdersCountAsync();
        var paidOrders = await _dashboardRepository.GetPaidOrdersAsync();
        var totalSales = paidOrders.Sum(o => o.TotalAmount);
        
        var totalCustomers = await _dashboardRepository.GetTotalCustomersCountAsync();
        var newCustomersThisMonth = await _dashboardRepository.GetNewCustomersCountAsync(startOfMonth);

        var totalProducts = await _dashboardRepository.GetTotalProductsCountAsync();
        var lowStockCount = await _dashboardRepository.GetLowStockProductsCountAsync(10);

        return new DashboardSummaryDto
        {
            TotalSales = totalSales,
            TotalRevenue = totalSales,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomersThisMonth,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockCount
        };
    }

    public async Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync()
    {
        var products = await _dashboardRepository.GetLowStockProductsAsync(10);
        return products.Select(p => new LowStockProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Stock = p.Stock,
            CategoryName = p.Category?.Name ?? "Uncategorized"
        });
    }
}

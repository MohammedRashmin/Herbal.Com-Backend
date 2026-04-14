using Web.Com.DTOs.Admin;

namespace Web.Com.Services.Interfaces.Admin;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync();
    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year);
    Task<IEnumerable<CategoryOrdersDto>> GetOrdersByCategoryAsync();
}

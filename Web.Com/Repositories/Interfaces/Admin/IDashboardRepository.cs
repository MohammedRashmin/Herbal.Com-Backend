using Web.Com.Entities;

namespace Web.Com.Repositories.Interfaces.Admin;

public interface IDashboardRepository
{
    Task<int> GetTotalOrdersCountAsync();
    Task<int> GetPendingOrdersCountAsync();
    Task<IEnumerable<Order>> GetPaidOrdersAsync();
    Task<int> GetTotalCustomersCountAsync();
    Task<int> GetNewCustomersCountAsync(DateTime since);
    Task<int> GetTotalProductsCountAsync();
    Task<int> GetLowStockProductsCountAsync(int threshold);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
}

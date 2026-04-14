using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Repositories.Interfaces.Admin;

namespace Web.Com.Repositories.Implementations.Admin;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<int> GetPendingOrdersCountAsync()
    {
        return await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
    }

    public async Task<IEnumerable<Order>> GetPaidOrdersAsync()
    {
        return await _context.Orders
            .Where(o => o.PaymentStatus == "Paid" || o.PaymentStatus == "COD")
            .ToListAsync();
    }

    public async Task<int> GetTotalCustomersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetNewCustomersCountAsync(DateTime since)
    {
        return await _context.Users.CountAsync(u => u.CreatedAt >= since);
    }

    public async Task<int> GetTotalProductsCountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<int> GetLowStockProductsCountAsync(int threshold)
    {
        return await _context.Products.CountAsync(p => p.Stock <= threshold);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Stock <= threshold)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year)
    {
        var raw = await _context.Orders
            .Where(o => o.OrderDate.Year == year && (o.PaymentStatus == "Paid" || o.PaymentStatus == "COD"))
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new { Month = g.Key, Sales = g.Sum(o => o.TotalAmount) })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return raw.Select(x => new MonthlySalesDto
        {
            Month = new DateTime(year, x.Month, 1).ToString("MMM"),
            Sales = x.Sales
        });
    }

    public async Task<IEnumerable<CategoryOrdersDto>> GetOrdersByCategoryAsync()
    {
        return await _context.OrderItems
            .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
            .GroupBy(oi => oi.Product.Category != null ? oi.Product.Category.Name : "Uncategorized")
            .Select(g => new CategoryOrdersDto
            {
                Category = g.Key,
                Orders = g.Count()
            })
            .OrderByDescending(x => x.Orders)
            .ToListAsync();
    }
}

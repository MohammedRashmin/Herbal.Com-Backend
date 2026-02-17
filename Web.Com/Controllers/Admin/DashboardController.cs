using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Com.Data;
using Web.Com.DTOs.Admin;
using Web.Com.Entities;
using Web.Com.Helpers.Constants;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var totalOrders = await _context.Orders.CountAsync();
        var pendingOrders = await _context.Orders
            .CountAsync(o => o.Status == OrderStatus.Pending);
        
        var paidOrders = await _context.Orders
            .Where(o => o.PaymentStatus == "Paid" || o.PaymentStatus == "COD")
            .ToListAsync();

        var totalSales = paidOrders.Sum(o => o.TotalAmount);
        
        var totalCustomers = await _context.Users.CountAsync();
        var newCustomersThisMonth = await _context.Users
            .CountAsync(u => u.CreatedAt >= startOfMonth);

        var totalProducts = await _context.Products.CountAsync();
        var lowStockProducts = await _context.Products
            .CountAsync(p => p.Stock <= 10);

        return Ok(new DashboardSummaryDto
        {
            TotalSales = totalSales,
            TotalRevenue = totalSales,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newCustomersThisMonth,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts
        });
    }

    [HttpGet("inventory/low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStockProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Stock <= 10)
            .OrderBy(p => p.Stock)
            .Select(p => new LowStockProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Stock = p.Stock,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(products);
    }
}

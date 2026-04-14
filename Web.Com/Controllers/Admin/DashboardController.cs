using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.DTOs.Admin;
using Web.Com.Services.Interfaces.Admin;

namespace Web.Com.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("inventory/low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockProductDto>>> GetLowStockProducts()
    {
        var products = await _dashboardService.GetLowStockProductsAsync();
        return Ok(products);
    }

    [HttpGet("charts/monthly-sales")]
    public async Task<ActionResult<IEnumerable<MonthlySalesDto>>> GetMonthlySales([FromQuery] int? year)
    {
        var result = await _dashboardService.GetMonthlySalesAsync(year ?? DateTime.UtcNow.Year);
        return Ok(result);
    }

    [HttpGet("charts/orders-by-category")]
    public async Task<ActionResult<IEnumerable<CategoryOrdersDto>>> GetOrdersByCategory()
    {
        var result = await _dashboardService.GetOrdersByCategoryAsync();
        return Ok(result);
    }
}

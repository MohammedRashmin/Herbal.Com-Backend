namespace Web.Com.DTOs.Admin;

public class DashboardSummaryDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
}

public class LowStockProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

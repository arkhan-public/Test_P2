using InventorySystem.Models;

namespace InventorySystem.Services.Interfaces
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<SalesOrder>> GetAllSalesOrdersAsync();
        Task<SalesOrder?> GetSalesOrderByIdAsync(int id);
        Task<(bool Success, string Message, SalesOrder? Order)> CreateSalesOrderAsync(SalesOrder salesOrder, List<SalesOrderItem> items);
        Task<bool> CompleteSalesOrderAsync(int id);
        Task<bool> CancelSalesOrderAsync(int id);
    }
}

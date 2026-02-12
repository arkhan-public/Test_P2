using InventorySystem.Models;

namespace InventorySystem.Services.Interfaces
{
    public interface IStockService
    {
        Task<bool> AddStockAsync(int productId, int quantity, string notes);
        Task<bool> RemoveStockAsync(int productId, int quantity, string notes);
        Task<bool> AdjustStockAsync(int productId, int quantity, string notes);
        Task<IEnumerable<StockTransaction>> GetAllTransactionsAsync();
        Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(int productId);
        Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 50);
        Task<bool> HasSufficientStockAsync(int productId, int requiredQuantity);
    }
}

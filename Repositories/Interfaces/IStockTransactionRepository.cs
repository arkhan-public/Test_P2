using InventorySystem.Models;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Interfaces
{
    public interface IStockTransactionRepository
    {
        Task AddAsync(StockTransaction tx);
    }
}
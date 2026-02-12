using InventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Interfaces
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrder>> GetAllWithItemsAsync();
        Task<SalesOrder?> GetByIdWithItemsAsync(int id);
        Task AddAsync(SalesOrder order);
        Task AddItemAsync(SalesOrderItem item);
        Task DeleteAsync(SalesOrder order);
    }
}
using InventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllWithItemsAsync();
        Task<PurchaseOrder?> GetByIdWithItemsAsync(int id);
        Task AddAsync(PurchaseOrder order);
        Task AddItemAsync(PurchaseOrderItem item);
        Task DeleteAsync(PurchaseOrder order);
    }
}
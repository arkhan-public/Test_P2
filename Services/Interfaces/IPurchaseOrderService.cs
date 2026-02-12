using InventorySystem.Models;

namespace InventorySystem.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllPurchaseOrdersAsync();
        Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id);
        Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder, List<PurchaseOrderItem> items);
        Task<bool> CompletePurchaseOrderAsync(int id);
        Task<bool> CancelPurchaseOrderAsync(int id);
        Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId);
        Task<bool> DeletePurchaseOrderAsync(int id);
    }
}

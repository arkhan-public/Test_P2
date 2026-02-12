using System.Linq;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Repositories.Interfaces;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseOrderRepository _purchaseRepo;
        private readonly IStockTransactionRepository _stockRepo;
        private readonly ILogger<PurchaseOrderService> _logger;

        public PurchaseOrderService(
            ApplicationDbContext context,
            IPurchaseOrderRepository purchaseRepo,
            IStockTransactionRepository stockRepo,
            ILogger<PurchaseOrderService> logger)
        {
            _context = context;
            _purchaseRepo = purchaseRepo;
            _stockRepo = stockRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                return await _purchaseRepo.GetAllWithItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all purchase orders");
                throw;
            }
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id)
        {
            try
            {
                return await _purchaseRepo.GetByIdWithItemsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order with ID {PurchaseOrderId}", id);
                throw;
            }
        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder purchaseOrder, List<PurchaseOrderItem> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                purchaseOrder.OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                purchaseOrder.OrderDate = DateTime.Now;
                purchaseOrder.Status = PurchaseOrderStatus.Pending;

                decimal total = 0;
                foreach (var item in items)
                {
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    total += item.TotalPrice;
                }
                purchaseOrder.TotalAmount = total;

                await _purchaseRepo.AddAsync(purchaseOrder);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    item.PurchaseOrderId = purchaseOrder.Id;
                    await _purchaseRepo.AddItemAsync(item);
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Purchase order created: {OrderNumber}", purchaseOrder.OrderNumber);
                return purchaseOrder;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating purchase order");
                throw;
            }
        }

        public async Task<bool> CompletePurchaseOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseOrder = await _purchaseRepo.GetByIdWithItemsAsync(id);

                if (purchaseOrder == null || purchaseOrder.Status != PurchaseOrderStatus.Pending)
                {
                    _logger.LogWarning("Purchase order not found or already processed: {PurchaseOrderId}", id);
                    return false;
                }

                // Update stock for each item
                foreach (var item in purchaseOrder.Items)
                {
                    var product = item.Product;
                    if (product == null) continue;

                    product.QuantityInStock += item.Quantity;
                    product.UpdatedAt = DateTime.Now;
                    _context.Products.Update(product);

                    // Create stock transaction
                    var tx = new StockTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = TransactionType.Purchase,
                        Quantity = item.Quantity,
                        BalanceAfter = product.QuantityInStock,
                        Reference = $"PO: {purchaseOrder.OrderNumber}",
                        Notes = "Purchase order completed",
                        TransactionDate = DateTime.Now
                    };
                    await _stockRepo.AddAsync(tx);
                }

                purchaseOrder.Status = PurchaseOrderStatus.Completed;
                purchaseOrder.CompletedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Purchase order completed: {OrderNumber}", purchaseOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing purchase order ID {PurchaseOrderId}", id);
                return false;
            }
        }

        public async Task<bool> CancelPurchaseOrderAsync(int id)
        {
            try
            {
                var purchaseOrder = await _purchaseRepo.GetByIdWithItemsAsync(id);
                if (purchaseOrder == null || purchaseOrder.Status != PurchaseOrderStatus.Pending)
                {
                    return false;
                }

                purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Purchase order cancelled: {OrderNumber}", purchaseOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling purchase order ID {PurchaseOrderId}", id);
                return false;
            }
        }

        public async Task<bool> DeletePurchaseOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseOrder = await _purchaseRepo.GetByIdWithItemsAsync(id);
                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Attempted to delete non-existing purchase order {Id}", id);
                    return false;
                }

                if (purchaseOrder.Status == PurchaseOrderStatus.Completed)
                {
                    _logger.LogWarning("Attempted to delete completed purchase order {Id}", id);
                    return false;
                }

                await _purchaseRepo.DeleteAsync(purchaseOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Purchase order deleted: {OrderNumber}", purchaseOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting purchase order ID {PurchaseOrderId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId)
        {
            try
            {
                var all = await _purchaseRepo.GetAllWithItemsAsync();
                return all.Where(po => po.SupplierId == supplierId)
                          .OrderByDescending(po => po.OrderDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders for supplier ID {SupplierId}", supplierId);
                throw;
            }
        }
    }
}

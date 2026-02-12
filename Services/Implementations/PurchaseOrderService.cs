using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseOrderService> _logger;

        public PurchaseOrderService(ApplicationDbContext context, ILogger<PurchaseOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            try
            {
                return await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product)
                    .OrderByDescending(po => po.OrderDate)
                    .ToListAsync();
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
                return await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product)
                    .FirstOrDefaultAsync(po => po.Id == id);
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
                // Generate order number
                purchaseOrder.OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                purchaseOrder.OrderDate = DateTime.Now;
                purchaseOrder.Status = PurchaseOrderStatus.Pending;

                // Calculate total
                decimal total = 0;
                foreach (var item in items)
                {
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    total += item.TotalPrice;
                }
                purchaseOrder.TotalAmount = total;

                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // Add items
                foreach (var item in items)
                {
                    item.PurchaseOrderId = purchaseOrder.Id;
                    _context.PurchaseOrderItems.Add(item);
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
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product)
                    .FirstOrDefaultAsync(po => po.Id == id);

                if (purchaseOrder == null || purchaseOrder.Status != PurchaseOrderStatus.Pending)
                {
                    _logger.LogWarning("Purchase order not found or already processed: {PurchaseOrderId}", id);
                    return false;
                }

                // Update stock for each item
                foreach (var item in purchaseOrder.Items)
                {
                    var product = item.Product;
                    product.QuantityInStock += item.Quantity;
                    product.UpdatedAt = DateTime.Now;

                    // Create stock transaction
                    var stockTransaction = new StockTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = TransactionType.Purchase,
                        Quantity = item.Quantity,
                        BalanceAfter = product.QuantityInStock,
                        Reference = $"PO: {purchaseOrder.OrderNumber}",
                        Notes = $"Purchase order completed",
                        TransactionDate = DateTime.Now
                    };
                    _context.StockTransactions.Add(stockTransaction);
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
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
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

        public async Task<IEnumerable<PurchaseOrder>> GetPurchaseOrdersBySupplierAsync(int supplierId)
        {
            try
            {
                return await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product)
                    .Where(po => po.SupplierId == supplierId)
                    .OrderByDescending(po => po.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders for supplier ID {SupplierId}", supplierId);
                throw;
            }
        }
    }
}

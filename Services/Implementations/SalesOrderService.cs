using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Repositories.Interfaces;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISalesOrderRepository _salesRepo;
        private readonly IStockTransactionRepository _stockRepo;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(
            ApplicationDbContext context,
            ISalesOrderRepository salesRepo,
            IStockTransactionRepository stockRepo,
            ILogger<SalesOrderService> logger)
        {
            _context = context;
            _salesRepo = salesRepo;
            _stockRepo = stockRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllSalesOrdersAsync()
        {
            try
            {
                return await _salesRepo.GetAllWithItemsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sales orders");
                throw;
            }
        }

        public async Task<SalesOrder?> GetSalesOrderByIdAsync(int id)
        {
            try
            {
                return await _salesRepo.GetByIdWithItemsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales order with ID {SalesOrderId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message, SalesOrder? Order)> CreateSalesOrderAsync(SalesOrder salesOrder, List<SalesOrderItem> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate product availability (do not change product stock here)
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        return (false, $"Product with ID {item.ProductId} not found", null);

                    if (product.QuantityInStock < item.Quantity)
                        return (false, $"Insufficient stock for {product.Name}. Available: {product.QuantityInStock}, Required: {item.Quantity}", null);
                }

                // Prepare order
                salesOrder.OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                salesOrder.OrderDate = DateTime.Now;
                salesOrder.Status = SalesOrderStatus.Pending;

                decimal total = 0;
                foreach (var item in items)
                {
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    total += item.TotalPrice;
                }
                salesOrder.TotalAmount = total;

                await _salesRepo.AddAsync(salesOrder);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    item.SalesOrderId = salesOrder.Id;
                    await _salesRepo.AddItemAsync(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sales order created (Pending): {OrderNumber}", salesOrder.OrderNumber);
                return (true, "Sales order created successfully", salesOrder);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating sales order");
                return (false, "An error occurred while creating the sales order", null);
            }
        }

        public async Task<bool> CompleteSalesOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _salesRepo.GetByIdWithItemsAsync(id);

                if (salesOrder == null || salesOrder.Status != SalesOrderStatus.Pending)
                {
                    _logger.LogWarning("Sales order not found or not pending: {SalesOrderId}", id);
                    return false;
                }

                // Re-check stock & prepare changes
                foreach (var item in salesOrder.Items)
                {
                    var product = item.Product;
                    if (product == null)
                    {
                        _logger.LogWarning("Product not loaded for item in order {SalesOrderId}", id);
                        return false;
                    }

                    if (product.QuantityInStock < item.Quantity)
                    {
                        _logger.LogWarning("Insufficient stock for product {ProductId} when completing order {SalesOrderId}", product.Id, id);
                        return false;
                    }
                }

                // Deduct stock and add transactions
                foreach (var item in salesOrder.Items)
                {
                    var product = item.Product!;
                    product.QuantityInStock -= item.Quantity;
                    product.UpdatedAt = DateTime.Now;
                    _context.Products.Update(product);

                    var tx = new StockTransaction
                    {
                        ProductId = product.Id,
                        TransactionType = TransactionType.Sale,
                        Quantity = -item.Quantity,
                        BalanceAfter = product.QuantityInStock,
                        Reference = $"SO: {salesOrder.OrderNumber}",
                        Notes = "Sales order completed",
                        TransactionDate = DateTime.Now
                    };
                    await _stockRepo.AddAsync(tx);
                }

                salesOrder.Status = SalesOrderStatus.Completed;
                salesOrder.CompletedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sales order completed: {OrderNumber}", salesOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing sales order ID {SalesOrderId}", id);
                return false;
            }
        }

        public async Task<bool> CancelSalesOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _salesRepo.GetByIdWithItemsAsync(id);
                if (salesOrder == null || salesOrder.Status == SalesOrderStatus.Cancelled)
                    return false;

                if (salesOrder.Status == SalesOrderStatus.Completed)
                {
                    foreach (var item in salesOrder.Items)
                    {
                        var product = item.Product;
                        if (product == null) continue;

                        product.QuantityInStock += item.Quantity;
                        product.UpdatedAt = DateTime.Now;
                        _context.Products.Update(product);

                        var tx = new StockTransaction
                        {
                            ProductId = product.Id,
                            TransactionType = TransactionType.Return,
                            Quantity = item.Quantity,
                            BalanceAfter = product.QuantityInStock,
                            Reference = $"SO: {salesOrder.OrderNumber} (Cancelled)",
                            Notes = "Stock returned due to order cancellation",
                            TransactionDate = DateTime.Now
                        };
                        await _stockRepo.AddAsync(tx);
                    }
                }

                salesOrder.Status = SalesOrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sales order cancelled: {OrderNumber}", salesOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling sales order ID {SalesOrderId}", id);
                return false;
            }
        }

        // New: Delete sales order (only Pending or Cancelled)
        public async Task<bool> DeleteSalesOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _salesRepo.GetByIdWithItemsAsync(id);
                if (salesOrder == null)
                {
                    _logger.LogWarning("Attempted to delete non-existing sales order {Id}", id);
                    return false;
                }

                if (salesOrder.Status == SalesOrderStatus.Completed)
                {
                    _logger.LogWarning("Attempted to delete completed sales order {Id}", id);
                    return false;
                }

                await _salesRepo.DeleteAsync(salesOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sales order deleted: {OrderNumber}", salesOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting sales order ID {SalesOrderId}", id);
                return false;
            }
        }
    }
}

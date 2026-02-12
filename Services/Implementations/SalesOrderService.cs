using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesOrderService> _logger;

        public SalesOrderService(ApplicationDbContext context, ILogger<SalesOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllSalesOrdersAsync()
        {
            try
            {
                return await _context.SalesOrders
                    .Include(so => so.Items)
                        .ThenInclude(soi => soi.Product)
                    .OrderByDescending(so => so.OrderDate)
                    .ToListAsync();
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
                return await _context.SalesOrders
                    .Include(so => so.Items)
                        .ThenInclude(soi => soi.Product)
                    .FirstOrDefaultAsync(so => so.Id == id);
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
                // Check stock availability for all items first
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        return (false, $"Product with ID {item.ProductId} not found", null);
                    }

                    if (product.QuantityInStock < item.Quantity)
                    {
                        return (false, $"Insufficient stock for {product.Name}. Available: {product.QuantityInStock}, Required: {item.Quantity}", null);
                    }
                }

                // Generate order number
                salesOrder.OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                salesOrder.OrderDate = DateTime.Now;
                salesOrder.Status = SalesOrderStatus.Pending;

                // Calculate total
                decimal total = 0;
                foreach (var item in items)
                {
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    total += item.TotalPrice;
                }
                salesOrder.TotalAmount = total;

                _context.SalesOrders.Add(salesOrder);
                await _context.SaveChangesAsync();

                // Add items and deduct stock
                foreach (var item in items)
                {
                    item.SalesOrderId = salesOrder.Id;
                    _context.SalesOrderItems.Add(item);

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.QuantityInStock -= item.Quantity;
                        product.UpdatedAt = DateTime.Now;

                        // Create stock transaction
                        var stockTransaction = new StockTransaction
                        {
                            ProductId = product.Id,
                            TransactionType = TransactionType.Sale,
                            Quantity = -item.Quantity,
                            BalanceAfter = product.QuantityInStock,
                            Reference = $"SO: {salesOrder.OrderNumber}",
                            Notes = $"Sales order",
                            TransactionDate = DateTime.Now
                        };
                        _context.StockTransactions.Add(stockTransaction);
                    }
                }

                await _context.SaveChangesAsync();

                // Mark as completed immediately
                salesOrder.Status = SalesOrderStatus.Completed;
                salesOrder.CompletedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Sales order created: {OrderNumber}", salesOrder.OrderNumber);
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
            try
            {
                var salesOrder = await _context.SalesOrders.FindAsync(id);
                if (salesOrder == null || salesOrder.Status != SalesOrderStatus.Pending)
                {
                    return false;
                }

                salesOrder.Status = SalesOrderStatus.Completed;
                salesOrder.CompletedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Sales order completed: {OrderNumber}", salesOrder.OrderNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sales order ID {SalesOrderId}", id);
                return false;
            }
        }

        public async Task<bool> CancelSalesOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var salesOrder = await _context.SalesOrders
                    .Include(so => so.Items)
                        .ThenInclude(soi => soi.Product)
                    .FirstOrDefaultAsync(so => so.Id == id);

                if (salesOrder == null || salesOrder.Status == SalesOrderStatus.Cancelled)
                {
                    return false;
                }

                // Restore stock if order was completed
                if (salesOrder.Status == SalesOrderStatus.Completed)
                {
                    foreach (var item in salesOrder.Items)
                    {
                        var product = item.Product;
                        product.QuantityInStock += item.Quantity;
                        product.UpdatedAt = DateTime.Now;

                        // Create return transaction
                        var stockTransaction = new StockTransaction
                        {
                            ProductId = product.Id,
                            TransactionType = TransactionType.Return,
                            Quantity = item.Quantity,
                            BalanceAfter = product.QuantityInStock,
                            Reference = $"SO: {salesOrder.OrderNumber} (Cancelled)",
                            Notes = "Stock returned due to order cancellation",
                            TransactionDate = DateTime.Now
                        };
                        _context.StockTransactions.Add(stockTransaction);
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
    }
}

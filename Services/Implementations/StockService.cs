using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockService> _logger;

        public StockService(ApplicationDbContext context, ILogger<StockService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddStockAsync(int productId, int quantity, string notes)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID {ProductId}", productId);
                    return false;
                }

                product.QuantityInStock += quantity;
                product.UpdatedAt = DateTime.Now;

                var transaction = new StockTransaction
                {
                    ProductId = productId,
                    TransactionType = TransactionType.Adjustment,
                    Quantity = quantity,
                    BalanceAfter = product.QuantityInStock,
                    Notes = notes,
                    Reference = "Manual Stock Addition",
                    TransactionDate = DateTime.Now
                };

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock added: {Quantity} units to product ID {ProductId}", quantity, productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock to product ID {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> RemoveStockAsync(int productId, int quantity, string notes)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID {ProductId}", productId);
                    return false;
                }

                if (product.QuantityInStock < quantity)
                {
                    _logger.LogWarning("Insufficient stock for product ID {ProductId}. Available: {Available}, Requested: {Requested}",
                        productId, product.QuantityInStock, quantity);
                    return false;
                }

                product.QuantityInStock -= quantity;
                product.UpdatedAt = DateTime.Now;

                var transaction = new StockTransaction
                {
                    ProductId = productId,
                    TransactionType = TransactionType.Adjustment,
                    Quantity = -quantity,
                    BalanceAfter = product.QuantityInStock,
                    Notes = notes,
                    Reference = "Manual Stock Removal",
                    TransactionDate = DateTime.Now
                };

                _context.StockTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock removed: {Quantity} units from product ID {ProductId}", quantity, productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock from product ID {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> AdjustStockAsync(int productId, int quantity, string notes)
        {
            try
            {
                if (quantity > 0)
                {
                    return await AddStockAsync(productId, quantity, notes);
                }
                else if (quantity < 0)
                {
                    return await RemoveStockAsync(productId, Math.Abs(quantity), notes);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock for product ID {ProductId}", productId);
                return false;
            }
        }

        public async Task<IEnumerable<StockTransaction>> GetAllTransactionsAsync()
        {
            try
            {
                return await _context.StockTransactions
                    .Include(st => st.Product)
                    .OrderByDescending(st => st.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all stock transactions");
                throw;
            }
        }

        public async Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(int productId)
        {
            try
            {
                return await _context.StockTransactions
                    .Include(st => st.Product)
                    .Where(st => st.ProductId == productId)
                    .OrderByDescending(st => st.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for product ID {ProductId}", productId);
                throw;
            }
        }

        public async Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 50)
        {
            try
            {
                return await _context.StockTransactions
                    .Include(st => st.Product)
                    .OrderByDescending(st => st.TransactionDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions");
                throw;
            }
        }

        public async Task<bool> HasSufficientStockAsync(int productId, int requiredQuantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                return product != null && product.QuantityInStock >= requiredQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock availability for product ID {ProductId}", productId);
                throw;
            }
        }
    }
}

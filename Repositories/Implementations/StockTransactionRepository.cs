using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Repositories.Interfaces;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Implementations
{
    public class StockTransactionRepository : IStockTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockTransaction tx)
        {
            await _context.StockTransactions.AddAsync(tx);
        }
    }
}
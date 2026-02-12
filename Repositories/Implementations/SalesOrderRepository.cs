using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Implementations
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllWithItemsAsync()
        {
            return await _context.SalesOrders
                .Include(so => so.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<SalesOrder?> GetByIdWithItemsAsync(int id)
        {
            return await _context.SalesOrders
                .Include(so => so.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task AddAsync(SalesOrder order)
        {
            await _context.SalesOrders.AddAsync(order);
        }

        public async Task AddItemAsync(SalesOrderItem item)
        {
            await _context.SalesOrderItems.AddAsync(item);
        }

        public Task DeleteAsync(SalesOrder order)
        {
            if (order.Items != null && order.Items.Any())
            {
                _context.SalesOrderItems.RemoveRange(order.Items);
            }

            _context.SalesOrders.Remove(order);
            return Task.CompletedTask;
        }
    }
}
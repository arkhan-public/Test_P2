using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Repositories.Implementations
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllWithItemsAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdWithItemsAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task AddAsync(PurchaseOrder order)
        {
            await _context.PurchaseOrders.AddAsync(order);
        }

        public async Task AddItemAsync(PurchaseOrderItem item)
        {
            await _context.PurchaseOrderItems.AddAsync(item);
        }

        public Task DeleteAsync(PurchaseOrder order)
        {
            if (order.Items != null && order.Items.Any())
            {
                _context.PurchaseOrderItems.RemoveRange(order.Items);
            }

            _context.PurchaseOrders.Remove(order);
            return Task.CompletedTask;
        }
    }
}
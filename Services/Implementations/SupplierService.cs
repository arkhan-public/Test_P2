using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ApplicationDbContext context, ILogger<SupplierService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            try
            {
                return await _context.Suppliers
                    .Include(s => s.Products)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all suppliers");
                throw;
            }
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            try
            {
                return await _context.Suppliers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
                throw;
            }
        }

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                supplier.CreatedAt = DateTime.Now;
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Supplier created: {SupplierName}", supplier.Name);
                return supplier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier: {SupplierName}", supplier.Name);
                throw;
            }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                _context.Entry(supplier).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Supplier updated: {SupplierName}", supplier.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID {SupplierId}", supplier.Id);
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                {
                    return false;
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Supplier deleted: {SupplierName}", supplier.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID {SupplierId}", id);
                return false;
            }
        }

        public async Task<bool> SupplierExistsAsync(int id)
        {
            try
            {
                return await _context.Suppliers.AnyAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if supplier exists with ID {SupplierId}", id);
                throw;
            }
        }
    }
}

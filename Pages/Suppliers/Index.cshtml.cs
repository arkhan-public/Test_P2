using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Suppliers
{
    public class IndexModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public IndexModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public List<Supplier> Suppliers { get; set; } = new();

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var allSuppliers = await _supplierService.GetAllSuppliersAsync();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Suppliers = allSuppliers
                    .Where(s => s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                               (s.Email != null && s.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            else
            {
                Suppliers = allSuppliers.ToList();
            }
        }
    }
}

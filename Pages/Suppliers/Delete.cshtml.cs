using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Suppliers
{
    public class DeleteModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public DeleteModel(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [BindProperty]
        public Supplier Supplier { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _supplierService.GetSupplierByIdAsync(id.Value);
            if (supplier == null)
            {
                return NotFound();
            }

            Supplier = supplier;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _supplierService.DeleteSupplierAsync(Supplier.Id);
            return RedirectToPage("./Index");
        }
    }
}

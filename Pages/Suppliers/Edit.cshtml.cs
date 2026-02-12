using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Suppliers
{
    public class EditModel : PageModel
    {
        private readonly ISupplierService _supplierService;

        public EditModel(ISupplierService supplierService)
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _supplierService.UpdateSupplierAsync(Supplier);
            return RedirectToPage("./Index");
        }
    }
}

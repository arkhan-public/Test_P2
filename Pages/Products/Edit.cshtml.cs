using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;

        public EditModel(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        public SelectList Categories { get; set; } = null!;
        public SelectList Suppliers { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            await _productService.UpdateProductAsync(Product);
            return RedirectToPage("./Index");
        }

        private async Task LoadDropdownsAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var suppliers = await _supplierService.GetAllSuppliersAsync();

            Categories = new SelectList(categories, "Id", "Name");
            Suppliers = new SelectList(suppliers, "Id", "Name");
        }
    }
}

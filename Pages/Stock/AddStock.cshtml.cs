using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Stock
{
    public class AddStockModel : PageModel
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;

        public AddStockModel(
            IStockService stockService,
            IProductService productService)
        {
            _stockService = stockService;
            _productService = productService;
        }

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string? Notes { get; set; }

        public SelectList Products { get; set; } = null!;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            await LoadProductsAsync();

            if (productId.HasValue && productId > 0)
            {
                ProductId = productId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                ErrorMessage = "Please fill in all required fields with valid values.";
                return Page();
            }

            if (ProductId <= 0)
            {
                await LoadProductsAsync();
                ErrorMessage = "Please select a product.";
                return Page();
            }

            if (Quantity <= 0)
            {
                await LoadProductsAsync();
                ErrorMessage = "Quantity must be greater than zero.";
                return Page();
            }

            // Verify product exists
            var product = await _productService.GetProductByIdAsync(ProductId);
            if (product == null)
            {
                await LoadProductsAsync();
                ErrorMessage = "Selected product does not exist.";
                return Page();
            }

            try
            {
                var success = await _stockService.AddStockAsync(ProductId, Quantity, Notes ?? string.Empty);

                if (success)
                {
                    return RedirectToPage("./Overview", new { success = true, productName = product.Name });
                }
                else
                {
                    await LoadProductsAsync();
                    ErrorMessage = "Failed to add stock. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                await LoadProductsAsync();
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadProductsAsync()
        {
            var products = await _productService.GetAllProductsAsync();
            var productList = products
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} (SKU: {p.SKU})",
                    Disabled = false
                })
                .ToList();

            Products = new SelectList(productList, "Value", "Text");
        }
    }
}

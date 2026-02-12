using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Stock
{
    public class OverviewModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IStockService _stockService;

        public OverviewModel(
            IProductService productService,
            IStockService stockService)
        {
            _productService = productService;
            _stockService = stockService;
        }

        public List<Product> Products { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterType { get; set; }

        public async Task OnGetAsync()
        {
            // Get all products
            var allProducts = (await _productService.GetAllProductsAsync()).ToList();
            TotalProducts = allProducts.Count;
            TotalStock = (await _productService.GetTotalStockQuantityAsync());

            // Calculate counts
            LowStockCount = allProducts.Count(p => p.QuantityInStock > 0 && p.QuantityInStock <= p.MinimumStockLevel);
            OutOfStockCount = allProducts.Count(p => p.QuantityInStock == 0);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                allProducts = allProducts
                    .Where(p => p.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                (p.SKU != null && p.SKU.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(FilterType))
            {
                if (FilterType == "low")
                {
                    allProducts = allProducts
                        .Where(p => p.QuantityInStock > 0 && p.QuantityInStock <= p.MinimumStockLevel)
                        .ToList();
                }
                else if (FilterType == "out")
                {
                    allProducts = allProducts
                        .Where(p => p.QuantityInStock == 0)
                        .ToList();
                }
            }

            Products = allProducts;
        }
    }
}

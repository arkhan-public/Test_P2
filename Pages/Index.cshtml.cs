using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IStockService _stockService;

        public IndexModel(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IStockService stockService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _stockService = stockService;
        }

        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalStock { get; set; }
        public List<Product> LowStockProducts { get; set; } = new();
        public List<StockTransaction> RecentTransactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalProducts = await _productService.GetTotalProductCountAsync();
            TotalCategories = (await _categoryService.GetAllCategoriesAsync()).Count();
            TotalSuppliers = (await _supplierService.GetAllSuppliersAsync()).Count();
            TotalStock = await _productService.GetTotalStockQuantityAsync();

            LowStockProducts = (await _productService.GetLowStockProductsAsync()).ToList();
            RecentTransactions = (await _stockService.GetRecentTransactionsAsync(10)).ToList();
        }
    }
}

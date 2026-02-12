using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;
using System.Text.Json;

namespace InventorySystem.Pages.SalesOrders
{
    public class CreateModel : PageModel
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly IProductService _productService;

        public CreateModel(ISalesOrderService salesOrderService, IProductService productService)
        {
            _salesOrderService = salesOrderService;
            _productService = productService;
        }

        [BindProperty]
        public SalesOrder SalesOrder { get; set; } = new();

        [BindProperty]
        public string? ItemsJson { get; set; }

        public List<Product> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Initialize order with current date
            SalesOrder.OrderDate = DateTime.Now;
            SalesOrder.Status = SalesOrderStatus.Pending;

            // Load all products for the dropdown
            Products = (await _productService.GetAllProductsAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate that we have items
            if (string.IsNullOrWhiteSpace(ItemsJson))
            {
                ModelState.AddModelError("", "Please add at least one product to the order.");
                Products = (await _productService.GetAllProductsAsync()).ToList();
                return Page();
            }

            // Parse items from JSON
            var items = new List<SalesOrderItem>();
            try
            {
                var itemsData = JsonSerializer.Deserialize<List<dynamic>>(ItemsJson);
                if (itemsData == null || itemsData.Count == 0)
                {
                    ModelState.AddModelError("", "Please add at least one product to the order.");
                    Products = (await _productService.GetAllProductsAsync()).ToList();
                    return Page();
                }

                foreach (var itemData in itemsData)
                {
                    var productId = int.Parse(itemData.GetProperty("productId").ToString());
                    var quantity = int.Parse(itemData.GetProperty("quantity").ToString());
                    var unitPrice = decimal.Parse(itemData.GetProperty("unitPrice").ToString());

                    // Validate product exists
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product == null)
                    {
                        ModelState.AddModelError("", $"Product not found.");
                        Products = (await _productService.GetAllProductsAsync()).ToList();
                        return Page();
                    }

                    // Validate stock availability
                    if (product.QuantityInStock < quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {product.Name}. Available: {product.QuantityInStock}, Requested: {quantity}");
                        Products = (await _productService.GetAllProductsAsync()).ToList();
                        return Page();
                    }

                    items.Add(new SalesOrderItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * quantity
                    });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error parsing order items: " + ex.Message);
                Products = (await _productService.GetAllProductsAsync()).ToList();
                return Page();
            }

            // Validate customer details
            if (!ModelState.IsValid)
            {
                Products = (await _productService.GetAllProductsAsync()).ToList();
                return Page();
            }

            // Generate order number
            SalesOrder.OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Calculate total amount
            SalesOrder.TotalAmount = items.Sum(i => i.TotalPrice);
            SalesOrder.OrderDate = DateTime.Now;
            SalesOrder.Status = SalesOrderStatus.Pending;

            // Create sales order with items
            var result = await _salesOrderService.CreateSalesOrderAsync(SalesOrder, items);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Sales order {result.Order?.OrderNumber} created successfully!";
                return RedirectToPage("./Index");
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                Products = (await _productService.GetAllProductsAsync()).ToList();
                return Page();
            }
        }
    }
}

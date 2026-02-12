using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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

        // Bind only the form fields we need (follow PurchaseOrder pattern)
        [BindProperty]
        [StringLength(100)]
        public string? CustomerName { get; set; }

        [BindProperty]
        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [BindProperty]
        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [BindProperty]
        [StringLength(500)]
        public string? Notes { get; set; }

        // items posted as Items[index].ProductId, Items[index].UnitPrice, Items[index].Quantity
        [BindProperty]
        public List<SalesOrderItemInput> Items { get; set; } = new();

        public List<Product> AllProducts { get; set; } = new();

        public SelectList ProductsSelect { get; set; } = null!;

        public async Task OnGetAsync()
        {
            AllProducts = (await _product_service_getall()).ToList();
            ProductsSelect = new SelectList(AllProducts, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Basic server-side validation
            if (Items == null || Items.Count == 0 || Items.All(i => i.ProductId <= 0 || i.Quantity <= 0))
            {
                ModelState.AddModelError(string.Empty, "Please add at least one product with quantity.");
            }

            if (!ModelState.IsValid)
            {
                AllProducts = (await _product_service_getall()).ToList();
                ProductsSelect = new SelectList(AllProducts, "Id", "Name");
                return Page();
            }

            // validate products and availability
            var orderItems = new List<SalesOrderItem>();
            decimal total = 0m;

            foreach (var input in Items)
            {
                if (input.ProductId <= 0 || input.Quantity <= 0) continue;

                var product = await _productService.GetProductByIdAsync(input.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, $"Product with ID {input.ProductId} not found.");
                    AllProducts = (await _product_service_getall()).ToList();
                    ProductsSelect = new SelectList(AllProducts, "Id", "Name");
                    return Page();
                }

                if (product.QuantityInStock < input.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Insufficient stock for {product.Name}. Available: {product.QuantityInStock}, Requested: {input.Quantity}");
                    AllProducts = (await _product_service_getall()).ToList();
                    ProductsSelect = new SelectList(AllProducts, "Id", "Name");
                    return Page();
                }

                var unitPrice = input.UnitPrice >= 0 ? input.UnitPrice : product.UnitPrice;
                var totalPrice = unitPrice * input.Quantity;

                orderItems.Add(new SalesOrderItem
                {
                    ProductId = input.ProductId,
                    Quantity = input.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });

                total += totalPrice;
            }

            if (orderItems.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please add at least one valid product with quantity.");
                AllProducts = (await _product_service_getall()).ToList();
                ProductsSelect = new SelectList(AllProducts, "Id", "Name");
                return Page();
            }

            // Build SalesOrder entity server-side and generate OrderNumber
            var salesOrder = new SalesOrder
            {
                OrderNumber = $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                CustomerPhone = CustomerPhone,
                OrderDate = DateTime.Now,
                Status = SalesOrderStatus.Pending,
                Notes = Notes,
                TotalAmount = total
            };

            // Create and handle tuple result properly
            var (success, message, createdOrder) = await _salesOrderService.CreateSalesOrderAsync(salesOrder, orderItems);

            if (success)
            {
                var orderNumber = createdOrder?.OrderNumber ?? salesOrder.OrderNumber;
                TempData["SuccessMessage"] = $"Sales order {orderNumber} created successfully!";
                return RedirectToPage("./Index");
            }
            else
            {
                // show service message
                ModelState.AddModelError(string.Empty, message ?? "Unable to create sales order.");
                AllProducts = (await _product_service_getall()).ToList();
                ProductsSelect = new SelectList(AllProducts, "Id", "Name");
                return Page();
            }
        }

        // helper wrappers
        private Task<IEnumerable<Product>> _product_service_getall() => _productService.GetAllProductsAsync();

        public class SalesOrderItemInput
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}

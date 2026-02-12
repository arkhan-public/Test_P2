using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Pages.PurchaseOrders
{
    public class CreateModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public CreateModel(
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService)
        {
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
        }

        [BindProperty]
        public int SupplierId { get; set; }

        [BindProperty]
        [StringLength(500)]
        public string? Notes { get; set; }

        [BindProperty]
        public List<PurchaseOrderItemInput> Items { get; set; } = new();

        public SelectList Suppliers { get; set; } = null!;
        public List<Product> AllProducts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || SupplierId == 0 || Items.Count == 0)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            try
            {
                // Generate order number
                var orderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                // Create purchase order
                var purchaseOrder = new PurchaseOrder
                {
                    OrderNumber = orderNumber,
                    SupplierId = SupplierId,
                    OrderDate = DateTime.Now,
                    Status = PurchaseOrderStatus.Pending,
                    Notes = Notes,
                    TotalAmount = 0 // Will be calculated
                };

                // Create order items
                var orderItems = new List<PurchaseOrderItem>();
                decimal totalAmount = 0;

                foreach (var item in Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        var unitPrice = item.UnitPrice > 0 ? item.UnitPrice : product.UnitPrice;
                        var totalPrice = unitPrice * item.Quantity;

                        orderItems.Add(new PurchaseOrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = totalPrice
                        });

                        totalAmount += totalPrice;
                    }
                }

                if (orderItems.Count == 0)
                {
                    ModelState.AddModelError("Items", "Please add at least one product with quantity.");
                    await LoadDropdownsAsync();
                    return Page();
                }

                purchaseOrder.TotalAmount = totalAmount;

                // Save to database
                await _purchaseOrderService.CreatePurchaseOrderAsync(purchaseOrder, orderItems);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating purchase order: {ex.Message}");
                await LoadDropdownsAsync();
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            AllProducts = (await _productService.GetAllProductsAsync()).ToList();

            Suppliers = new SelectList(suppliers, "Id", "Name");
        }

        public class PurchaseOrderItemInput
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}

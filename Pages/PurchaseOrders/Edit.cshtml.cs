using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.PurchaseOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            ApplicationDbContext context,
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService,
            IProductService productService,
            ILogger<EditModel> logger)
        {
            _context = context;
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public PurchaseOrder PurchaseOrder { get; set; } = new();

        [BindProperty]
        public List<PurchaseOrderItemInput> Items { get; set; } = new();

        public SelectList Suppliers { get; set; } = null!;
        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id.Value);
            if (order == null) return NotFound();

            if (order.Status != PurchaseOrderStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending purchase orders can be edited.";
                return RedirectToPage("./Details", new { id = order.Id });
            }

            PurchaseOrder = order;

            // Make sure the SelectList is constructed with the current selected value
            var suppliers = (await _supplier_service_getall()).ToList();
            Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);

            Products = (await _product_service_getall()).ToList();

            Items = PurchaseOrder.Items
                .Select(i => new PurchaseOrderItemInput
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Log bound supplier id for troubleshooting
            _logger.LogDebug("Edit POST PurchaseOrder.Id={Id} SupplierId={SupplierId}", PurchaseOrder.Id, PurchaseOrder.SupplierId);

            // Server-side validation: require a supplier
            if (PurchaseOrder.SupplierId <= 0)
            {
                ModelState.AddModelError("PurchaseOrder.SupplierId", "Please select a supplier.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PurchaseOrders/Edit POST ModelState invalid. Errors: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                TempData["ErrorMessage"] = "Validation failed. Please fix the errors and try again.";

                // repopulate suppliers/products so the page can render correctly
                var suppliers = (await _supplier_service_getall()).ToList();
                Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            var existing = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == PurchaseOrder.Id);

            if (existing == null) return NotFound();

            if (existing.Status != PurchaseOrderStatus.Pending)
            {
                ModelState.AddModelError(string.Empty, "Only pending orders can be edited.");
                var suppliers = (await _supplier_service_getall()).ToList();
                Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            // Validate and build new items list
            var newItems = new List<PurchaseOrderItem>();
            decimal total = 0m;

            if (Items == null || Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please have at least one order item.");
                var suppliers = (await _supplier_service_getall()).ToList();
                Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            foreach (var input in Items)
            {
                if (input.ProductId <= 0 || input.Quantity <= 0)
                {
                    continue; // skip invalid / empty rows
                }

                var product = await _productService.GetProductByIdAsync(input.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, $"Product with ID {input.ProductId} not found.");
                    var suppliers = (await _supplier_service_getall()).ToList();
                    Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                    Products = (await _product_service_getall()).ToList();
                    return Page();
                }

                var unitPrice = input.UnitPrice >= 0 ? input.UnitPrice : product.UnitPrice;
                var totalPrice = unitPrice * input.Quantity;

                newItems.Add(new PurchaseOrderItem
                {
                    ProductId = input.ProductId,
                    Quantity = input.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });

                total += totalPrice;
            }

            if (newItems.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please add at least one product with quantity.");
                var suppliers = (await _supplier_service_getall()).ToList();
                Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            // Update order header
            existing.SupplierId = PurchaseOrder.SupplierId;

            // Apply latest date on save (server time)
            existing.OrderDate = DateTime.Now;

            existing.Notes = PurchaseOrder.Notes;
            existing.TotalAmount = total;

            try
            {
                // Remove old items
                if (existing.Items.Any())
                {
                    _context.PurchaseOrderItems.RemoveRange(existing.Items);
                    await _context.SaveChangesAsync();
                }

                // Add new items
                foreach (var ni in newItems)
                {
                    ni.PurchaseOrderId = existing.Id;
                    _context.PurchaseOrderItems.Add(ni);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Purchase order updated.";
                return RedirectToPage("./Details", new { id = existing.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating purchase order {PurchaseOrderId}", existing.Id);
                TempData["ErrorMessage"] = "An error occurred while saving changes. See server logs for details.";
                var suppliers = (await _supplier_service_getall()).ToList();
                Suppliers = new SelectList(suppliers, "Id", "Name", PurchaseOrder.SupplierId);
                Products = (await _product_service_getall()).ToList();
                return Page();
            }
        }

        private Task<IEnumerable<Supplier>> _supplier_service_getall() => _supplierService.GetAllSuppliersAsync();
        private Task<IEnumerable<Product>> _product_service_getall() => _productService.GetAllProductsAsync();

        public class PurchaseOrderItemInput
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
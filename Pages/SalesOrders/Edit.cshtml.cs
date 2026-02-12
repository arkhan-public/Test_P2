using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.SalesOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISalesOrderService _salesOrderService;
        private readonly IProductService _productService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            ApplicationDbContext context,
            ISalesOrderService salesOrderService,
            IProductService productService,
            ILogger<EditModel> logger)
        {
            _context = context;
            _salesOrderService = salesOrderService;
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        public SalesOrder SalesOrder { get; set; } = new();

        // Input model for items in the form
        [BindProperty]
        public List<SalesOrderItemInput> Items { get; set; } = new();

        public List<Product> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var order = await _salesOrderService.GetSalesOrderByIdAsync(id.Value);
            if (order == null) return NotFound();

            _logger.LogDebug("Loaded SalesOrder Id={Id} OrderNumber={OrderNumber} Phone='{Phone}'", order.Id, order.OrderNumber, order.CustomerPhone);

            // Only allow editing pending orders
            if (order.Status != SalesOrderStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending sales orders can be edited.";
                return RedirectToPage("./Details", new { id = order.Id });
            }

            SalesOrder = order;
            if (string.IsNullOrWhiteSpace(SalesOrder.CustomerPhone) && !string.IsNullOrWhiteSpace(order.CustomerPhone))
            {
                SalesOrder.CustomerPhone = order.CustomerPhone;
            }

            Products = (await _product_service_getall()).ToList();

            Items = SalesOrder.Items
                .Select(i => new SalesOrderItemInput
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList();

            if (Items.Count == 0) Items.Add(new SalesOrderItemInput());

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var keysToRemove = ModelState.Keys.Where(k => k.StartsWith("SalesOrder.Items") || k.StartsWith("SalesOrder.Items[")).ToList();
            foreach (var k in keysToRemove) ModelState.Remove(k);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("SalesOrders/Edit POST ModelState invalid. Errors: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            var existing = await _context.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == SalesOrder.Id);

            if (existing == null) return NotFound();

            if (existing.Status != SalesOrderStatus.Pending)
            {
                ModelState.AddModelError(string.Empty, "Only pending orders can be edited.");
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            // Validate and build new items
            var newItems = new List<SalesOrderItem>();
            decimal total = 0m;

            if (Items == null || Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please add at least one order item.");
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            foreach (var input in Items)
            {
                if (input.ProductId <= 0 || input.Quantity <= 0) continue;

                var product = await _productService.GetProductByIdAsync(input.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError(string.Empty, $"Product with ID {input.ProductId} not found.");
                    Products = (await _product_service_getall()).ToList();
                    return Page();
                }

                // Validate availability now so edited order is reasonable
                if (product.QuantityInStock < input.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Insufficient stock for {product.Name}. Available: {product.QuantityInStock}, Requested: {input.Quantity}");
                    Products = (await _product_service_getall()).ToList();
                    return Page();
                }

                var unitPrice = input.UnitPrice >= 0 ? input.UnitPrice : product.UnitPrice;
                var totalPrice = unitPrice * input.Quantity;

                newItems.Add(new SalesOrderItem
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
                ModelState.AddModelError(string.Empty, "Please add at least one valid product with quantity.");
                Products = (await _product_service_getall()).ToList();
                return Page();
            }

            // Update order header fields
            existing.CustomerName = SalesOrder.CustomerName;
            existing.CustomerEmail = SalesOrder.CustomerEmail;
            existing.CustomerPhone = SalesOrder.CustomerPhone;
            // Apply latest date on save (server time)
            existing.OrderDate = DateTime.Now;
            existing.Notes = SalesOrder.Notes;
            existing.TotalAmount = total;

            try
            {
                // Replace items: delete old, insert new
                if (existing.Items.Any())
                {
                    _context.SalesOrderItems.RemoveRange(existing.Items);
                    await _context.SaveChangesAsync();
                }

                foreach (var ni in newItems)
                {
                    ni.SalesOrderId = existing.Id;
                    _context.SalesOrderItems.Add(ni);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Sales order updated.";
                return RedirectToPage("./Details", new { id = existing.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating sales order {SalesOrderId}", existing.Id);
                ModelState.AddModelError(string.Empty, "Error saving changes. See logs for details.");
                Products = (await _product_service_getall()).ToList();
                return Page();
            }
        }

        private Task<IEnumerable<Product>> _product_service_getall() => _productService.GetAllProductsAsync();

        public class SalesOrderItemInput
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
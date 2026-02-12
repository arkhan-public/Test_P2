using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.PurchaseOrders
{
    public class IndexModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public IndexModel(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public List<PurchaseOrder> PurchaseOrders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var orders = await _purchaseOrderService.GetAllPurchaseOrdersAsync();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (Enum.TryParse<PurchaseOrderStatus>(StatusFilter, out var status))
                {
                    PurchaseOrders = orders.Where(o => o.Status == status).ToList();
                }
                else
                {
                    PurchaseOrders = orders.ToList();
                }
            }
            else
            {
                PurchaseOrders = orders.ToList();
            }

            // Sort by order date descending
            PurchaseOrders = PurchaseOrders.OrderByDescending(o => o.OrderDate).ToList();
        }
    }
}

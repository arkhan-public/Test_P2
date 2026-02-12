using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Pages.SalesOrders
{
    public class IndexModel : PageModel
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ISalesOrderService salesOrderService, ILogger<IndexModel> logger)
        {
            _salesOrderService = salesOrderService;
            _logger = logger;
        }

        public IList<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var list = (await _salesOrderService.GetAllSalesOrdersAsync()).ToList();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                // filter by known statuses: Pending, Completed, Cancelled
                if (StatusFilter.Equals("Pending", System.StringComparison.OrdinalIgnoreCase))
                    list = list.Where(s => s.Status == SalesOrderStatus.Pending).ToList();
                else if (StatusFilter.Equals("Completed", System.StringComparison.OrdinalIgnoreCase))
                    list = list.Where(s => s.Status == SalesOrderStatus.Completed).ToList();
                else if (StatusFilter.Equals("Cancelled", System.StringComparison.OrdinalIgnoreCase))
                    list = list.Where(s => s.Status == SalesOrderStatus.Cancelled).ToList();
            }

            SalesOrders = list;
        }

        // Handler to complete an order from the index list
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            if (id <= 0) return BadRequest();

            var ok = await _salesOrderService.CompleteSalesOrderAsync(id);
            return RedirectToPage("./Index", new { statusFilter = StatusFilter });
        }
    }
}

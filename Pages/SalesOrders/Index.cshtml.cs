using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.SalesOrders
{
    public class IndexModel : PageModel
    {
        private readonly ISalesOrderService _salesOrderService;

        public IndexModel(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        public List<SalesOrder> SalesOrders { get; set; } = new();

        public async Task OnGetAsync()
        {
            SalesOrders = (await _salesOrderService.GetAllSalesOrdersAsync()).ToList();
        }
    }
}

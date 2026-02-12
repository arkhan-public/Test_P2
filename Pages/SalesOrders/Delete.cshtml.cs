using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.SalesOrders
{
    public class DeleteModel : PageModel
    {
        private readonly ISalesOrderService _salesOrderService;

        public DeleteModel(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        public SalesOrder? SalesOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var so = await _salesOrderService.GetSalesOrderByIdAsync(id.Value);
            if (so == null) return NotFound();

            // Do not allow deleting completed orders
            if (so.Status == SalesOrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Completed sales orders cannot be deleted.";
                return RedirectToPage("./Details", new { id = id.Value });
            }

            SalesOrder = so;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var success = await _salesOrderService.DeleteSalesOrderAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Sales order deleted successfully.";
                return RedirectToPage("./Index");
            }

            TempData["ErrorMessage"] = "Failed to delete sales order.";
            return RedirectToPage("./Details", new { id });
        }
    }
}

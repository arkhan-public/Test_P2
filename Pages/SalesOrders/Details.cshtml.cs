using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.SalesOrders
{
    public class DetailsModel : PageModel
    {
        private readonly ISalesOrderService _salesOrderService;

        public DetailsModel(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        public SalesOrder? SalesOrder { get; set; }

        [BindProperty]
        public string? Action { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            SalesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id.Value);

            if (SalesOrder == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            SalesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id.Value);

            if (SalesOrder == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(Action))
            {
                return Page();
            }

            bool success = false;

            if (Action == "complete")
            {
                success = await _salesOrderService.CompleteSalesOrderAsync(id.Value);
                if (success)
                {
                    TempData["SuccessMessage"] = "Sales order marked as completed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to complete the sales order.";
                }
            }
            else if (Action == "cancel")
            {
                success = await _salesOrderService.CancelSalesOrderAsync(id.Value);
                if (success)
                {
                    TempData["SuccessMessage"] = "Sales order cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel the sales order.";
                }
            }

            if (success)
            {
                return RedirectToPage("./Index");
            }
            else
            {
                // Reload the sales order to show current state
                SalesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id.Value);
                return Page();
            }
        }
    }
}

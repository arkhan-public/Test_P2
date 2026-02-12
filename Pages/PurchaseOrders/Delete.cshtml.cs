using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.PurchaseOrders
{
    public class DeleteModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public DeleteModel(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public PurchaseOrder? PurchaseOrder { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var po = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id.Value);
            if (po == null) return NotFound();

            // Do not allow deleting completed orders
            if (po.Status == PurchaseOrderStatus.Completed)
            {
                TempData["ErrorMessage"] = "Completed purchase orders cannot be deleted.";
                return RedirectToPage("./Details", new { id = id.Value });
            }

            PurchaseOrder = po;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var success = await _purchaseOrderService.DeletePurchaseOrderAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Purchase order deleted successfully.";
                return RedirectToPage("./Index");
            }

            TempData["ErrorMessage"] = "Failed to delete purchase order.";
            return RedirectToPage("./Details", new { id });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.PurchaseOrders
{
    public class DetailsModel : PageModel
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public DetailsModel(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        public PurchaseOrder PurchaseOrder { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id.Value);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            PurchaseOrder = purchaseOrder;

            // Check for success message from TempData
            if (TempData.ContainsKey("SuccessMessage"))
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            try
            {
                var success = await _purchaseOrderService.CompletePurchaseOrderAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Purchase order completed successfully!";
                    return RedirectToPage(new { id = id });
                }
                else
                {
                    ErrorMessage = "Failed to complete the purchase order.";
                    var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
                    if (purchaseOrder != null)
                    {
                        PurchaseOrder = purchaseOrder;
                    }
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error completing purchase order: {ex.Message}";
                var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
                if (purchaseOrder != null)
                {
                    PurchaseOrder = purchaseOrder;
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            try
            {
                var success = await _purchaseOrderService.CancelPurchaseOrderAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Purchase order cancelled successfully!";
                    return RedirectToPage(new { id = id });
                }
                else
                {
                    ErrorMessage = "Failed to cancel the purchase order.";
                    var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
                    if (purchaseOrder != null)
                    {
                        PurchaseOrder = purchaseOrder;
                    }
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error cancelling purchase order: {ex.Message}";
                var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
                if (purchaseOrder != null)
                {
                    PurchaseOrder = purchaseOrder;
                }
                return Page();
            }
        }
    }
}

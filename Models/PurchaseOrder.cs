using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InventorySystem.Models
{
    public enum PurchaseOrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        // Validate the foreign-key only (database stores SupplierId)
        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Prevent model binding/validation on the navigation property
        [ForeignKey("SupplierId")]
        [ValidateNever]
        public virtual Supplier Supplier { get; set; } = null!;

        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}

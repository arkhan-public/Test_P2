using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models
{
    public enum SalesOrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public class SalesOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Navigation property
        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }
}

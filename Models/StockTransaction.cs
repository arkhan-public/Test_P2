using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models
{
    public enum TransactionType
    {
        Purchase,
        Sale,
        Adjustment,
        Return
    }

    public class StockTransaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        public int Quantity { get; set; }

        public int BalanceAfter { get; set; }

        [StringLength(500)]
        public string? Reference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}

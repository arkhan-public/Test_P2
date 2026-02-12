using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models
{
    public class StockEntry
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.Now;

        // This is used for manual stock adjustments
        // Not stored separately, creates a StockTransaction
    }
}

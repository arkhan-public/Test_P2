using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using InventorySystem.Models;
using InventorySystem.Services.Interfaces;

namespace InventorySystem.Pages.Stock
{
    public class TransactionsModel : PageModel
    {
        private readonly IStockService _stockService;

        public TransactionsModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        public List<StockTransaction> Transactions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ProductId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProductSearch { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TransactionType { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterDate { get; set; }

        public async Task OnGetAsync()
        {
            List<StockTransaction> transactions;

            // Get transactions based on whether we're filtering by product ID
            if (ProductId.HasValue && ProductId > 0)
            {
                transactions = (await _stockService.GetTransactionsByProductAsync(ProductId.Value)).ToList();
            }
            else
            {
                transactions = (await _stockService.GetAllTransactionsAsync()).ToList();
            }

            // Apply product name search filter
            if (!string.IsNullOrWhiteSpace(ProductSearch))
            {
                transactions = transactions
                    .Where(t => t.Product != null &&
                                t.Product.Name.Contains(ProductSearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply transaction type filter
            if (!string.IsNullOrWhiteSpace(TransactionType))
            {
                if (Enum.TryParse<TransactionType>(TransactionType, out var transactionType))
                {
                    transactions = transactions
                        .Where(t => t.TransactionType == transactionType)
                        .ToList();
                }
            }

            // Apply date filter
            if (FilterDate.HasValue)
            {
                var filterDate = FilterDate.Value.Date;
                transactions = transactions
                    .Where(t => t.TransactionDate.Date == filterDate)
                    .ToList();
            }

            // Sort by date descending (most recent first)
            Transactions = transactions
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }
    }
}

using System;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Application.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public TransactionType Type { get; set; } = TransactionType.Expense;
        public int CategoryId { get; set; }
        
        // Category detailed info flattened for simple API consumption
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;

        // Wallet info
        public int WalletId { get; set; }
        public string WalletName { get; set; } = string.Empty;
    }
}

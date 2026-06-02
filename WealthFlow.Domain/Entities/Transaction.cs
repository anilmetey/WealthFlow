using System;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public TransactionType Type { get; set; } = TransactionType.Expense;
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int WalletId { get; set; }
        public Wallet? Wallet { get; set; }
    }
}

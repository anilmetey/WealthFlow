using System.Collections.Generic;

namespace WealthFlow.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Color { get; set; } = "#6366F1";
        public string Icon { get; set; } = "fa-wallet";

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}

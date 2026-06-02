namespace WealthFlow.Application.DTOs
{
    public class WalletTransferDto
    {
        public int FromWalletId { get; set; }
        public int ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "Cüzdanlar Arası Transfer";
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;

namespace WealthFlow.Application.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<WalletDto>> GetAllWalletsAsync();
        Task<WalletDto?> GetByIdAsync(int id);
        Task<WalletDto> CreateWalletAsync(WalletDto dto);
        Task UpdateWalletAsync(WalletDto dto);
        Task DeleteWalletAsync(int id);
        Task<bool> TransferFundsAsync(WalletTransferDto dto);
    }
}

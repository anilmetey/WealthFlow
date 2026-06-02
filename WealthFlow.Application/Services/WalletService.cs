using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Enums;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WalletDto>> GetAllWalletsAsync()
        {
            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            return _mapper.Map<IEnumerable<WalletDto>>(wallets);
        }

        public async Task<WalletDto?> GetByIdAsync(int id)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(id);
            return _mapper.Map<WalletDto?>(wallet);
        }

        public async Task<WalletDto> CreateWalletAsync(WalletDto dto)
        {
            var wallet = _mapper.Map<Wallet>(dto);
            await _unitOfWork.Wallets.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<WalletDto>(wallet);
        }

        public async Task UpdateWalletAsync(WalletDto dto)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(dto.Id);
            if (wallet != null)
            {
                _mapper.Map(dto, wallet);
                _unitOfWork.Wallets.Update(wallet);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteWalletAsync(int id)
        {
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(id);
            if (wallet != null)
            {
                _unitOfWork.Wallets.Delete(wallet);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> TransferFundsAsync(WalletTransferDto dto)
        {
            var fromWallet = await _unitOfWork.Wallets.GetByIdAsync(dto.FromWalletId);
            var toWallet = await _unitOfWork.Wallets.GetByIdAsync(dto.ToWalletId);

            if (fromWallet == null || toWallet == null || fromWallet.Balance < dto.Amount)
            {
                return false;
            }

            // 1. Bakiyeleri Güncelle
            fromWallet.Balance -= dto.Amount;
            toWallet.Balance += dto.Amount;

            _unitOfWork.Wallets.Update(fromWallet);
            _unitOfWork.Wallets.Update(toWallet);

            // 2. Çıkış İşlemini Kaydet (Kategori: Diğer)
            var otherCategory = await _unitOfWork.Categories.GetByNameAsync("Diğer");
            int otherCatId = otherCategory?.Id ?? 1;

            var expenseTx = new Transaction
            {
                Description = $"Cüzdan Transferi -> {toWallet.Name}",
                Amount = dto.Amount,
                Date = DateTime.Today,
                Type = TransactionType.Expense,
                CategoryId = otherCatId,
                WalletId = fromWallet.Id
            };
            await _unitOfWork.Transactions.AddAsync(expenseTx);

            // 3. Giriş İşlemini Kaydet
            var incomeTx = new Transaction
            {
                Description = $"Cüzdan Transferi <- {fromWallet.Name}",
                Amount = dto.Amount,
                Date = DateTime.Today,
                Type = TransactionType.Income,
                CategoryId = otherCatId,
                WalletId = toWallet.Id
            };
            await _unitOfWork.Transactions.AddAsync(incomeTx);

            // 4. Audit Log Ekle
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                Action = "Bakiye Transferi",
                Details = $"'{fromWallet.Name}' cüzdanından '{toWallet.Name}' cüzdanına {dto.Amount:N2} ₺ transfer edildi."
            });

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}

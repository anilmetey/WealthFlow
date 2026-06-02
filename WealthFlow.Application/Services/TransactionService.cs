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
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            return _mapper.Map<TransactionDto?>(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(string? searchTerm, int? categoryId, TransactionType? type)
        {
            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(searchTerm, categoryId, type);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<TransactionDto> CreateTransactionAsync(TransactionDto dto)
        {
            var transaction = _mapper.Map<Transaction>(dto);
            await _unitOfWork.Transactions.AddAsync(transaction);

            // Cüzdan Bakiyesini Güncelle
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId);
            if (wallet != null)
            {
                if (transaction.Type == TransactionType.Income)
                {
                    wallet.Balance += transaction.Amount;
                }
                else
                {
                    wallet.Balance -= transaction.Amount;
                }
                _unitOfWork.Wallets.Update(wallet);
            }
            
            // Log ekle
            var category = await _unitOfWork.Categories.GetByIdAsync(transaction.CategoryId);
            var typeLabel = transaction.Type == TransactionType.Income ? "Gelir" : "Gider";
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                Action = "İşlem Eklendi",
                Details = $"'{transaction.Description}' açıklamalı, {transaction.Amount:N2} ₺ tutarında {typeLabel} işlemi '{wallet?.Name ?? "Bilinmeyen"}' cüzdanına eklendi. (Kategori: {category?.Name ?? "Kategorisiz"})"
            });

            await _unitOfWork.SaveChangesAsync();
            
            // Reload to fetch category relationship if needed
            var createdTransaction = await _unitOfWork.Transactions.GetByIdAsync(transaction.Id);
            return _mapper.Map<TransactionDto>(createdTransaction ?? transaction);
        }

        public async Task UpdateTransactionAsync(TransactionDto dto)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(dto.Id);
            if (transaction != null)
            {
                // Eski cüzdanın bakiyesini geri al
                var oldWallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId);
                if (oldWallet != null)
                {
                    if (transaction.Type == TransactionType.Income)
                    {
                        oldWallet.Balance -= transaction.Amount;
                    }
                    else
                    {
                        oldWallet.Balance += transaction.Amount;
                    }
                    _unitOfWork.Wallets.Update(oldWallet);
                }

                var oldDesc = transaction.Description;
                var oldAmount = transaction.Amount;
                _mapper.Map(dto, transaction);

                // Yeni cüzdanın bakiyesini güncelle
                var newWallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId);
                if (newWallet != null)
                {
                    if (transaction.Type == TransactionType.Income)
                    {
                        newWallet.Balance += transaction.Amount;
                    }
                    else
                    {
                        newWallet.Balance -= transaction.Amount;
                    }
                    _unitOfWork.Wallets.Update(newWallet);
                }

                _unitOfWork.Transactions.Update(transaction);
                
                await _unitOfWork.AuditLogs.AddAsync(new AuditLog
                {
                    Action = "İşlem Güncellendi",
                    Details = $"'{oldDesc}' açıklamalı işlem güncellendi. Yeni Açıklama: '{transaction.Description}', Yeni Tutar: {transaction.Amount:N2} ₺."
                });

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            if (transaction != null)
            {
                // Cüzdan bakiyesinden geri düş
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId);
                if (wallet != null)
                {
                    if (transaction.Type == TransactionType.Income)
                    {
                        wallet.Balance -= transaction.Amount;
                    }
                    else
                    {
                        wallet.Balance += transaction.Amount;
                    }
                    _unitOfWork.Wallets.Update(wallet);
                }

                var typeLabel = transaction.Type == TransactionType.Income ? "Gelir" : "Gider";
                await _unitOfWork.AuditLogs.AddAsync(new AuditLog
                {
                    Action = "İşlem Silindi",
                    Details = $"'{transaction.Description}' açıklamalı, {transaction.Amount:N2} ₺ tutarındaki {typeLabel} işlemi silindi."
                });

                _unitOfWork.Transactions.Delete(transaction);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}

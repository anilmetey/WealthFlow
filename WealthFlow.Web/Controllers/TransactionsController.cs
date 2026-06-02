using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

namespace WealthFlow.Web.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IWalletService _walletService;

        public TransactionsController(ITransactionService transactionService, ICategoryService categoryService, IWalletService walletService)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;
            _walletService = walletService;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, TransactionType? type)
        {
            var transactions = await _transactionService.GetFilteredTransactionsAsync(searchTerm, categoryId, type);
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedType = type;

            return View(transactions);
        }

        // GET: Transactions/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            var wallets = await _walletService.GetAllWalletsAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            ViewBag.WalletId = new SelectList(wallets, "Id", "Name");
            return View(new TransactionDto());
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionDto dto)
        {
            if (ModelState.IsValid)
            {
                await _transactionService.CreateTransactionAsync(dto);
                TempData["SuccessMessage"] = "İşlem başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            
            var categories = await _categoryService.GetAllAsync();
            var wallets = await _walletService.GetAllWalletsAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", dto.CategoryId);
            ViewBag.WalletId = new SelectList(wallets, "Id", "Name", dto.WalletId);
            return View(dto);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetByIdAsync(id.Value);
            if (transaction == null)
            {
                return NotFound();
            }

            var categories = await _categoryService.GetAllAsync();
            var wallets = await _walletService.GetAllWalletsAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", transaction.CategoryId);
            ViewBag.WalletId = new SelectList(wallets, "Id", "Name", transaction.WalletId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _transactionService.UpdateTransactionAsync(dto);
                TempData["SuccessMessage"] = "İşlem başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetAllAsync();
            var wallets = await _walletService.GetAllWalletsAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", dto.CategoryId);
            ViewBag.WalletId = new SelectList(wallets, "Id", "Name", dto.WalletId);
            return View(dto);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Domain.Enums;

namespace WealthFlow.Web.Controllers.Api
{
    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionsApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionsApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var list = transactions.ToList();
            var today = DateTime.Today;

            // Bu ayki işlemleri sorgula
            var currentMonthTransactions = list
                .Where(t => t.Date.Month == today.Month && t.Date.Year == today.Year && t.Type == TransactionType.Expense)
                .ToList();

            var predefinedSubs = new List<SubscriptionItemDto>();

            // Netflix
            var netflix = currentMonthTransactions.FirstOrDefault(t => t.Description.Contains("Netflix"));
            if (netflix != null)
            {
                predefinedSubs.Add(new SubscriptionItemDto
                {
                    Name = "Netflix Premium",
                    Amount = netflix.Amount,
                    NextPaymentDate = new DateTime(today.Year, today.Month, 10).AddMonths(1).ToString("dd.MM.yyyy"),
                    WalletName = netflix.Wallet?.Name ?? "Kredi Kartı",
                    CategoryColor = netflix.Category?.Color ?? "#EF4444",
                    Icon = "fa-tv",
                    BrandClass = "sub-netflix"
                });
            }

            // Spotify
            var spotify = currentMonthTransactions.FirstOrDefault(t => t.Description.Contains("Spotify"));
            if (spotify != null)
            {
                predefinedSubs.Add(new SubscriptionItemDto
                {
                    Name = "Spotify Aile",
                    Amount = spotify.Amount,
                    NextPaymentDate = new DateTime(today.Year, today.Month, 10).AddMonths(1).ToString("dd.MM.yyyy"),
                    WalletName = spotify.Wallet?.Name ?? "Kredi Kartı",
                    CategoryColor = spotify.Category?.Color ?? "#EC4899",
                    Icon = "fa-music",
                    BrandClass = "sub-spotify"
                });
            }

            // YouTube Premium
            var youtube = currentMonthTransactions.FirstOrDefault(t => t.Description.Contains("YouTube"));
            if (youtube != null)
            {
                predefinedSubs.Add(new SubscriptionItemDto
                {
                    Name = "YouTube Premium",
                    Amount = youtube.Amount,
                    NextPaymentDate = new DateTime(today.Year, today.Month, 10).AddMonths(1).ToString("dd.MM.yyyy"),
                    WalletName = youtube.Wallet?.Name ?? "Kredi Kartı",
                    CategoryColor = youtube.Category?.Color ?? "#EC4899",
                    Icon = "fa-play-circle",
                    BrandClass = "sub-youtube"
                });
            }

            // Fiber Internet
            var internet = currentMonthTransactions.FirstOrDefault(t => t.Description.Contains("İnternet") || t.Description.Contains("Fiber"));
            if (internet != null)
            {
                predefinedSubs.Add(new SubscriptionItemDto
                {
                    Name = "Fiber İnternet",
                    Amount = internet.Amount,
                    NextPaymentDate = new DateTime(today.Year, today.Month, 7).AddMonths(1).ToString("dd.MM.yyyy"),
                    WalletName = internet.Wallet?.Name ?? "Banka Hesabı",
                    CategoryColor = internet.Category?.Color ?? "#EF4444",
                    Icon = "fa-wifi",
                    BrandClass = "sub-internet"
                });
            }

            return Ok(predefinedSubs);
        }

        [HttpPost("simulate-eta")]
        public async Task<IActionResult> SimulateEta([FromBody] SimulationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Geçersiz istek.");
            }

            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = transactions.ToList();

            var today = DateTime.Today;

            // 1. Son 3 aylık birikim ortalamasını bul (Tasarruf Hızı)
            decimal totalSavingsPast3Months = 0;
            int monthsWithDataCount = 0;
            for (int k = 0; k < 3; k++)
            {
                var checkDate = today.AddMonths(-k);
                var mTrans = transactionList.Where(t => t.Date.Month == checkDate.Month && t.Date.Year == checkDate.Year).ToList();
                var mInc = mTrans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var mExp = mTrans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                totalSavingsPast3Months += (mInc - mExp);
                monthsWithDataCount++;
            }
            decimal avgMonthlySavings = monthsWithDataCount > 0 ? totalSavingsPast3Months / monthsWithDataCount : 0;

            // İptal edilen aboneliklerin toplam tutarını hesapla
            decimal cancelledTotal = 0;
            if (request.CancelledNames != null)
            {
                foreach (var name in request.CancelledNames)
                {
                    if (name.Contains("Netflix")) cancelledTotal += 220;
                    else if (name.Contains("Spotify")) cancelledTotal += 85;
                    else if (name.Contains("YouTube")) cancelledTotal += 120;
                    else if (name.Contains("İnternet")) cancelledTotal += 650;
                }
            }

            var newSavingsSpeed = avgMonthlySavings + cancelledTotal;

            // 2. Tüm hedefler için yeni ETA hesapla
            var goals = await _unitOfWork.Goals.GetGoalsWithCategoriesAsync();
            var goalList = goals.ToList();

            var simulationResults = new List<SimulatedGoalEtaDto>();
            var culture = new System.Globalization.CultureInfo("tr-TR");

            foreach (var g in goalList)
            {
                var remaining = g.TargetAmount - g.CurrentAmount;
                if (remaining <= 0) continue;

                // Orijinal ETA
                double originalMonths = avgMonthlySavings > 50 ? (double)(remaining / avgMonthlySavings) : 9999;
                var originalEtaStr = "Belirsiz";
                if (avgMonthlySavings > 50)
                {
                    if (originalMonths > 120) originalEtaStr = "10+ Yıl";
                    else originalEtaStr = today.AddMonths((int)Math.Ceiling(originalMonths)).ToString("MMMM yyyy", culture);
                }

                // Yeni ETA
                double newMonths = newSavingsSpeed > 50 ? (double)(remaining / newSavingsSpeed) : 9999;
                var newEtaStr = "Belirsiz";
                if (newSavingsSpeed > 50)
                {
                    if (newMonths > 120) newEtaStr = "10+ Yıl";
                    else newEtaStr = today.AddMonths((int)Math.Ceiling(newMonths)).ToString("MMMM yyyy", culture);
                }

                int monthsSaved = 0;
                if (avgMonthlySavings > 50 && newSavingsSpeed > 50 && originalMonths < 120 && newMonths < 120)
                {
                    monthsSaved = (int)Math.Max(0, Math.Ceiling(originalMonths) - Math.Ceiling(newMonths));
                }

                simulationResults.Add(new SimulatedGoalEtaDto
                {
                    GoalId = g.Id,
                    GoalTitle = g.Title,
                    OriginalEta = originalEtaStr,
                    NewEta = newEtaStr,
                    OriginalMonths = originalMonths,
                    NewMonths = newMonths,
                    MonthsSaved = monthsSaved
                });
            }

            return Ok(new
            {
                avgMonthlySavings = avgMonthlySavings,
                cancelledTotal = cancelledTotal,
                newSavingsSpeed = newSavingsSpeed,
                results = simulationResults
            });
        }
    }

    public class SubscriptionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string NextPaymentDate { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-receipt";
        public string BrandClass { get; set; } = string.Empty;
    }

    public class SimulationRequestDto
    {
        public List<string> CancelledNames { get; set; } = new();
    }

    public class SimulatedGoalEtaDto
    {
        public int GoalId { get; set; }
        public string GoalTitle { get; set; } = string.Empty;
        public string OriginalEta { get; set; } = string.Empty;
        public string NewEta { get; set; } = string.Empty;
        public double OriginalMonths { get; set; }
        public double NewMonths { get; set; }
        public int MonthsSaved { get; set; }
    }
}

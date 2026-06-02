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
    [Route("api/advanced-finances")]
    public class AdvancedFinancesApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdvancedFinancesApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("tax-bracket")]
        public async Task<IActionResult> GetTaxBracket()
        {
            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = transactions.ToList();

            var monthlyIncomes = transactionList
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => new { t.Date.Month, t.Date.Year })
                .Select(g => g.Sum(t => t.Amount))
                .ToList();

            decimal avgMonthlyIncome = monthlyIncomes.Any() ? monthlyIncomes.Average() : 0;
            decimal estAnnualIncome = avgMonthlyIncome * 12;

            decimal taxRate = 0;
            decimal taxDue = 0;

            if (estAnnualIncome > 0)
            {
                // Simulated Progressive tax bracket calculation (based on Turkey income tax brackets)
                if (estAnnualIncome > 3000000) { taxRate = 40; taxDue = estAnnualIncome * 0.40m; }
                else if (estAnnualIncome > 870000) { taxRate = 35; taxDue = estAnnualIncome * 0.35m; }
                else if (estAnnualIncome > 230000) { taxRate = 27; taxDue = estAnnualIncome * 0.27m; }
                else if (estAnnualIncome > 110000) { taxRate = 20; taxDue = estAnnualIncome * 0.20m; }
                else { taxRate = 15; taxDue = estAnnualIncome * 0.15m; }
            }

            return Ok(new
            {
                avgMonthlyIncome = avgMonthlyIncome,
                estAnnualIncome = estAnnualIncome,
                taxRate = taxRate,
                taxDue = taxDue,
                bracketDetails = new[] {
                    new { name = "1. Dilim (15%)", limit = "110.000 ₺'ye kadar" },
                    new { name = "2. Dilim (20%)", limit = "110.000 ₺ - 230.000 ₺" },
                    new { name = "3. Dilim (27%)", limit = "230.000 ₺ - 870.000 ₺" },
                    new { name = "4. Dilim (35%)", limit = "870.000 ₺ - 3.000.000 ₺" },
                    new { name = "5. Dilim (40%)", limit = "3.000.000 ₺ ve üzeri" }
                }
            });
        }

        [HttpGet("debt-plan")]
        public async Task<IActionResult> GetDebtPlan()
        {
            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            var walletList = wallets.ToList();
            var debtWallets = walletList.Where(w => w.Balance < 0).ToList();

            decimal totalDebt = Math.Abs(debtWallets.Sum(w => w.Balance));

            // Fetch average monthly savings rate to estimate payoff time
            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = transactions.ToList();
            var today = DateTime.Today;

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
            if (avgMonthlySavings < 50) avgMonthlySavings = 3000; // Fallback to avoid division by zero or negative speed

            double monthsToPayoff = totalDebt > 0 ? (double)(totalDebt / avgMonthlySavings) : 0;

            // Generate two methods: Snowball vs Avalanche
            var snowballSteps = new List<object>();
            var avalancheSteps = new List<object>();

            if (totalDebt > 0)
            {
                // Snowball: Sort by balance ascending (smallest debt first)
                var sortedSnowball = debtWallets.OrderBy(w => Math.Abs(w.Balance)).ToList();
                decimal currentSavingsAlloc = avgMonthlySavings;
                foreach (var w in sortedSnowball)
                {
                    decimal amount = Math.Abs(w.Balance);
                    double months = (double)(amount / currentSavingsAlloc);
                    snowballSteps.Add(new {
                        walletName = w.Name,
                        amount = amount,
                        months = Math.Ceiling(months),
                        strategy = "Küçük Bakiyeyi Hızla Eritme"
                    });
                }

                // Avalanche: Sort by simulated interest rate (highest first)
                // We'll simulate interest rates for wallets: Credit Card = 60% APR, others = 45% APR
                var sortedAvalanche = debtWallets
                    .Select(w => new { Wallet = w, InterestRate = w.Name.Contains("Kart") ? 60 : 45 })
                    .OrderByDescending(x => x.InterestRate)
                    .ToList();
                
                foreach (var item in sortedAvalanche)
                {
                    decimal amount = Math.Abs(item.Wallet.Balance);
                    double months = (double)(amount / currentSavingsAlloc);
                    avalancheSteps.Add(new {
                        walletName = item.Wallet.Name,
                        amount = amount,
                        interestRate = item.InterestRate,
                        months = Math.Ceiling(months),
                        strategy = "Yüksek Faiz Koruması (En Ucuz Yol)"
                    });
                }
            }

            return Ok(new
            {
                totalDebt = totalDebt,
                avgMonthlySavings = avgMonthlySavings,
                monthsToPayoff = Math.Ceiling(monthsToPayoff),
                debtItems = debtWallets.Select(w => new { name = w.Name, balance = Math.Abs(w.Balance), color = w.Color }),
                snowballSteps = snowballSteps,
                avalancheSteps = avalancheSteps
            });
        }

        [HttpGet("fire-calculator")]
        public async Task<IActionResult> GetFireCalculator()
        {
            var transactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = transactions.ToList();

            var monthlyExpenses = transactionList
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Date.Month, t.Date.Year })
                .Select(g => g.Sum(t => t.Amount))
                .ToList();

            decimal avgMonthlyExpense = monthlyExpenses.Any() ? monthlyExpenses.Average() : 0;
            decimal annualExpense = avgMonthlyExpense * 12;
            decimal fireNumber = annualExpense * 25; // Rule of 25

            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            decimal totalAssets = wallets.Sum(w => w.Balance);
            double progressPercent = fireNumber > 0 ? (double)(totalAssets / fireNumber) * 100 : 0;

            // Fetch savings rate to calculate estimated time
            var today = DateTime.Today;
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

            // Estimate years: with compound growth (assume 35% inflation/investment rate)
            double yearsToFire = 0;
            if (avgMonthlySavings > 50 && fireNumber > totalAssets)
            {
                decimal currentWealth = totalAssets;
                decimal annualSavings = avgMonthlySavings * 12;
                decimal targetWealth = fireNumber;
                decimal rate = 0.35m; // 35% compound yield
                int y = 0;
                while (currentWealth < targetWealth && y < 100)
                {
                    currentWealth = (currentWealth + annualSavings) * (1 + rate);
                    y++;
                }
                yearsToFire = y;
            }

            return Ok(new
            {
                avgMonthlyExpense = avgMonthlyExpense,
                annualExpense = annualExpense,
                fireNumber = fireNumber,
                totalAssets = totalAssets,
                progressPercent = progressPercent,
                avgMonthlySavings = avgMonthlySavings,
                yearsToFire = yearsToFire
            });
        }

        [HttpGet("bes-simulator")]
        public IActionResult SimulateBes([FromQuery] decimal monthlyPayment = 2000, [FromQuery] double growthRate = 35, [FromQuery] int years = 10)
        {
            decimal monthlyGrowth = (decimal)(growthRate / 100.0 / 12.0);
            decimal totalGovContribution = 0;
            decimal totalPrincipal = 0;
            decimal currentWealth = 0;
            
            var projections = new List<object>();

            for (int month = 1; month <= years * 12; month++)
            {
                decimal payment = monthlyPayment;
                decimal govContrib = payment * 0.30m; // 30% Government contribution

                totalGovContribution += govContrib;
                totalPrincipal += payment;

                // Add contribution and compound growth
                currentWealth = (currentWealth + payment + govContrib) * (1 + monthlyGrowth);

                if (month % 12 == 0) // Track annual data
                {
                    projections.Add(new
                    {
                        year = month / 12,
                        totalPrincipal = totalPrincipal,
                        totalGovContribution = totalGovContribution,
                        estimatedWealth = currentWealth
                    });
                }
            }

            return Ok(new
            {
                monthlyPayment = monthlyPayment,
                growthRate = growthRate,
                years = years,
                totalGovContribution = totalGovContribution,
                totalPrincipal = totalPrincipal,
                estimatedWealth = currentWealth,
                projections = projections
            });
        }

        [HttpGet("currency-exposure")]
        public async Task<IActionResult> GetCurrencyExposure()
        {
            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            var walletList = wallets.ToList();
            var positiveWallets = walletList.Where(w => w.Balance > 0).ToList();
            decimal totalAssets = positiveWallets.Sum(w => w.Balance);

            decimal goldAssets = positiveWallets
                .Where(w => w.Name.Contains("altın", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("gold", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("gau", StringComparison.OrdinalIgnoreCase))
                .Sum(w => w.Balance);

            decimal usdAssets = positiveWallets
                .Where(w => w.Name.Contains("dolar", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("usd", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("euro", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("eur", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("döviz", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("gbp", StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains("sterlin", StringComparison.OrdinalIgnoreCase))
                .Sum(w => w.Balance);

            decimal tryAssets = totalAssets - goldAssets - usdAssets;
            if (tryAssets < 0) tryAssets = 0;

            double hedgeRatio = totalAssets > 0 ? (double)((goldAssets + usdAssets) / totalAssets) * 100 : 0;

            return Ok(new
            {
                totalAssets = totalAssets,
                tryAssets = tryAssets,
                usdAssets = usdAssets,
                goldAssets = goldAssets,
                hedgeRatio = hedgeRatio,
                riskExposure = 100 - hedgeRatio,
                recommendation = hedgeRatio < 50 
                    ? "TL varlık oranınız yüksek. Alım gücünüzü enflasyona karşı korumak amacıyla döviz korumalı fon, altın veya Eurobond enstrümanlarının payını en az %50 seviyesine yükseltmeniz önerilir." 
                    : "Hedge oranınız dengeli. Portföyünüz döviz kurlarındaki dalgalanmalara karşı iyi bir tampona sahip."
            });
        }
    }
}

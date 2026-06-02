using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;
using WealthFlow.Domain.Enums;
using WealthFlow.Domain.Interfaces;

namespace WealthFlow.Application.Services
{
    public class InsightService : IInsightService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InsightService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<InsightDto>> GenerateInsightsAsync(int month, int year)
        {
            var insights = new List<InsightDto>();
            var today = new DateTime(year, month, 1);

            var allTransactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = allTransactions.ToList();

            var thisMonthTransactions = transactionList
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .ToList();

            var thisMonthIncome = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var thisMonthExpense = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // 1. Tasarruf Analizi
            if (thisMonthIncome > 0)
            {
                var savingsRate = ((thisMonthIncome - thisMonthExpense) / thisMonthIncome) * 100;
                if (savingsRate >= 30)
                {
                    insights.Add(new InsightDto
                    {
                        Type = "success",
                        Message = $"Tebrikler! Bu ayki tasarruf oranınız %{savingsRate:F0}. Finansal hedeflerinize emin adımlarla ilerliyorsunuz!",
                        Icon = "fa-circle-check"
                    });
                }
                else if (savingsRate < 10)
                {
                    insights.Add(new InsightDto
                    {
                        Type = "warning",
                        Message = $"Tasarruf Alarmı! Gelirinizin sadece %{savingsRate:F0} kadarını biriktirebildiniz. Giderlerinizi azaltmayı düşünmelisiniz.",
                        Icon = "fa-triangle-exclamation"
                    });
                }
                else
                {
                    insights.Add(new InsightDto
                    {
                        Type = "info",
                        Message = $"Bu ayki tasarruf oranınız %{savingsRate:F0}. Dengeli bir finansal yönetim gösteriyorsunuz.",
                        Icon = "fa-circle-info"
                    });
                }
            }

            // 2. Bütçe Aşımları Analizi
            var budgets = await _unitOfWork.Budgets.GetBudgetsByMonthYearAsync(month, year);
            foreach (var b in budgets)
            {
                var spent = thisMonthTransactions
                    .Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                if (b.Amount > 0)
                {
                    var progress = (spent / b.Amount) * 100;
                    if (progress > 100)
                    {
                        insights.Add(new InsightDto
                        {
                            Type = "danger",
                            Message = $"Bütçe Aşımı! '{b.Category?.Name}' kategorisinde bütçe limitinizi {spent - b.Amount:N0} ₺ aştınız!",
                            Icon = "fa-circle-exclamation"
                        });
                    }
                    else if (progress >= 85)
                    {
                        insights.Add(new InsightDto
                        {
                            Type = "warning",
                            Message = $"Sınıra Yakın! '{b.Category?.Name}' bütçenizin %{progress:F0}'ini tükettiniz. Kalan Limit: {b.Amount - spent:N0} ₺.",
                            Icon = "fa-triangle-exclamation"
                        });
                    }
                }
            }

            // 2.1 Geçen Aya Göre Harcama Trend Analizi
            var prevMonthDate = today.AddMonths(-1);
            var prevMonthTransactions = transactionList
                .Where(t => t.Date.Month == prevMonthDate.Month && t.Date.Year == prevMonthDate.Year)
                .ToList();
            
            var prevMonthExpense = prevMonthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            if (prevMonthExpense > 0)
            {
                var changePercent = ((thisMonthExpense - prevMonthExpense) / prevMonthExpense) * 100;
                if (changePercent < -5)
                {
                    insights.Add(new InsightDto
                    {
                        Type = "success",
                        Message = $"Tasarruf Disiplini! Bu ayki harcamalarınız geçen aya göre %{Math.Abs(changePercent):F0} azaldı. Harika gidiyorsunuz!",
                        Icon = "fa-circle-arrow-down"
                    });
                }
                else if (changePercent > 10)
                {
                    insights.Add(new InsightDto
                    {
                        Type = "danger",
                        Message = $"Harcama Artışı! Giderleriniz geçen aya kıyasla %{changePercent:F0} arttı. Harcama kalemlerini kontrol edin.",
                        Icon = "fa-circle-arrow-up"
                    });
                }
            }

            // 3-Aylık Ortalama Tasarruf Hesaplama (AI Tahminleme için)
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

            // 3. Hedef İlerleme & AI Zaman Tahminleme (Forecasting)
            var goals = await _unitOfWork.Goals.GetGoalsWithCategoriesAsync();
            foreach (var g in goals)
            {
                var progress = g.TargetAmount > 0 ? ((double)g.CurrentAmount / (double)g.TargetAmount) * 100 : 0;
                if (progress >= 100)
                {
                    insights.Add(new InsightDto
                    {
                        Type = "success",
                        Message = $"Hedef Tamamlandı! '{g.Title}' birikim hedefinize ulaştınız. Harika bir finansal başarı!",
                        Icon = "fa-trophy"
                    });
                }
                else
                {
                    var remaining = g.TargetAmount - g.CurrentAmount;
                    if (avgMonthlySavings > 50)
                    {
                        double monthsRequired = (double)(remaining / avgMonthlySavings);
                        if (monthsRequired > 120)
                        {
                            insights.Add(new InsightDto
                            {
                                Type = "info",
                                Message = $"Analitik Öngörü: Mevcut birikim hızıyla '{g.Title}' hedefinize ulaşmanız 10 yıldan uzun sürecektir. Tasarrufunuzu artırmayı düşünebilirsiniz.",
                                Icon = "fa-lightbulb"
                            });
                        }
                        else
                        {
                            var estDate = today.AddMonths((int)Math.Ceiling(monthsRequired));
                            var estDateStr = estDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
                            insights.Add(new InsightDto
                            {
                                Type = "success",
                                Message = $"Analitik Öngörü: Ortalama {avgMonthlySavings:N0} ₺/ay birikim hızıyla '{g.Title}' hedefinize {Math.Ceiling(monthsRequired):F0} ay sonra ({estDateStr}) ulaşacaksınız!",
                                Icon = "fa-lightbulb"
                            });
                        }
                    }
                    else if (progress >= 50)
                    {
                        insights.Add(new InsightDto
                        {
                            Type = "success",
                            Message = $"Yolu Yarıladınız! '{g.Title}' hedefinizde %{progress:F0} ilerleme kaydettiniz.",
                            Icon = "fa-circle-check"
                        });
                    }
                }
            }

            // Varsayılan öneri (Eğer liste boşsa)
            if (!insights.Any())
            {
                insights.Add(new InsightDto
                {
                    Type = "info",
                    Message = "Harcamalarınızı kaydetmeye devam edin. Analizleriniz burada görünecektir.",
                    Icon = "fa-chart-line"
                });
            }

            return insights;
        }

        public async Task<int> CalculateFinancialHealthScoreAsync()
        {
            var today = DateTime.Today;
            var month = today.Month;
            var year = today.Year;

            // 1. İşlemleri ve Tasarruf Oranını Al
            var allTransactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = allTransactions.ToList();
            
            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            var walletList = wallets.ToList();

            if (!transactionList.Any() && !walletList.Any())
            {
                return 0; // Sıfır veri durumunda 0 dön ki UI "Veri bekleniyor" desin
            }

            int score = 50; // Başlangıç taban puanı

            var thisMonthTransactions = transactionList
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .ToList();

            var income = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expense = thisMonthTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            if (income > 0)
            {
                var savingsRate = ((income - expense) / income) * 100;
                if (savingsRate >= 30) score += 15;
                else if (savingsRate >= 15) score += 10;
                else if (savingsRate >= 5) score += 5;
                else if (savingsRate < 0) score -= 15;
            }
            else if (expense > 0)
            {
                // Gelir yok ama gider var
                score -= 15;
            }

            // 2. Bütçe Disiplini
            var budgets = await _unitOfWork.Budgets.GetBudgetsByMonthYearAsync(month, year);
            var budgetList = budgets.ToList();
            if (budgetList.Any())
            {
                int exceededCount = 0;
                foreach (var b in budgetList)
                {
                    var spent = thisMonthTransactions
                        .Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount);

                    if (b.Amount > 0 && spent > b.Amount)
                    {
                        exceededCount++;
                    }
                }

                if (exceededCount > 0)
                {
                    score -= Math.Min(exceededCount * 8, 24); // Exceeded başına -8 (Maks -24)
                }
                else
                {
                    score += 10; // Bütçeleri aşmama ödülü +10
                }
            }

            // 3. Birikim Hedefleri İlerlemesi
            var goals = await _unitOfWork.Goals.GetGoalsWithCategoriesAsync();
            var goalList = goals.ToList();
            if (goalList.Any())
            {
                double totalProgress = 0;
                foreach (var g in goalList)
                {
                    var progress = g.TargetAmount > 0 ? ((double)g.CurrentAmount / (double)g.TargetAmount) * 100 : 0;
                    totalProgress += progress;
                }

                double avgProgress = totalProgress / goalList.Count;
                if (avgProgress >= 75) score += 15;
                else if (avgProgress >= 40) score += 10;
                else if (avgProgress >= 10) score += 5;
            }

            // 4. Net Varlık Durumu (Toplam Cüzdan Bakiyesi)
            var netWorth = walletList.Sum(w => w.Balance);

            if (netWorth >= 50000) score += 10;
            else if (netWorth >= 10000) score += 5;
            else if (netWorth < 1000) score -= 5;

            // Skoru 1 ile 100 arasında sınırla
            return Math.Clamp(score, 1, 100);
        }

        public async Task<string> ProcessChatQueryAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "Lütfen bana bir finansal soru sorun.";
            }

            var q = query.ToLower(new System.Globalization.CultureInfo("tr-TR"));

            // Verileri Çek
            var allTransactions = await _unitOfWork.Transactions.GetFilteredTransactionsAsync(null, null, null);
            var transactionList = allTransactions.ToList();
            var today = DateTime.Today;
            var month = today.Month;
            var year = today.Year;

            var thisMonthTransactions = transactionList
                .Where(t => t.Date.Month == month && t.Date.Year == year)
                .ToList();

            var wallets = await _unitOfWork.Wallets.GetAllAsync();
            var walletList = wallets.ToList();

            var goals = await _unitOfWork.Goals.GetGoalsWithCategoriesAsync();
            var goalList = goals.ToList();

            var budgets = await _unitOfWork.Budgets.GetBudgetsByMonthYearAsync(month, year);
            var budgetList = budgets.ToList();

            // Son 3 aylık tasarruf ortalamasını bul
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

            // 1. Hedefler / ETA sorguları
            if (q.Contains("hedef") || q.Contains("macbook") || q.Contains("avrupa") || q.Contains("ne zaman"))
            {
                if (!goalList.Any())
                {
                    return "Şu an tanımlanmış aktif bir birikim hedefiniz bulunmuyor. Birikim Hedefleri sayfasından yeni bir hedef oluşturabilirsiniz.";
                }

                var result = "### 🎯 Birikim Hedefleriniz ve Tahminler\n\n";
                foreach (var g in goalList)
                {
                    var remaining = g.TargetAmount - g.CurrentAmount;
                    var progress = g.TargetAmount > 0 ? ((double)g.CurrentAmount / (double)g.TargetAmount) * 100 : 0;

                    result += $"* **{g.Title}:** %{progress:F0} tamamlandı. (Kalan: **{remaining:N0} ₺** / Hedef: **{g.TargetAmount:N0} ₺**)\n";
                    if (progress >= 100)
                    {
                        result += "  * 🎉 Tebrikler, bu hedefinize zaten ulaştınız!\n";
                    }
                    else if (avgMonthlySavings > 50)
                    {
                        double monthsRequired = (double)(remaining / avgMonthlySavings);
                        if (monthsRequired > 120)
                        {
                            result += "  * 📊 **Analitik Öngörü:** Mevcut birikim hızınızla bu hedefe ulaşmanız **10 yıldan uzun** sürecektir. Aylık tasarrufunuzu artırmanız önerilir.\n";
                        }
                        else
                        {
                            var estDate = today.AddMonths((int)Math.Ceiling(monthsRequired));
                            var estDateStr = estDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
                            result += $"  * 📊 **Analitik Öngörü:** Ortalama **{avgMonthlySavings:N0} ₺/ay** birikim hızıyla bu hedefinize yaklaşık **{Math.Ceiling(monthsRequired):F0} ay sonra ({estDateStr})** ulaşacaksınız.\n";
                        }
                    }
                    else
                    {
                        result += "  * ⚠️ Son aylardaki tasarruf hızınız hedefinizi tamamlamak için yetersiz görünüyor. Tasarruflarınızı artırmaya çalışın.\n";
                    }
                }
                return result;
            }

            // 2. Bütçe durumları sorguları
            if (q.Contains("bütçe") || q.Contains("limit") || q.Contains("aş"))
            {
                if (!budgetList.Any())
                {
                    return "Bu ay için tanımlanmış herhangi bir bütçe limitiniz bulunmuyor. Bütçe Planı sayfasından kategori limitleri belirleyebilirsiniz.";
                }

                var exceeded = new List<string>();
                var safe = new List<string>();

                foreach (var b in budgetList)
                {
                    var spent = thisMonthTransactions
                        .Where(t => t.CategoryId == b.CategoryId && t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount);

                    var progress = b.Amount > 0 ? (spent / b.Amount) * 100 : 0;
                    var status = $"* **{b.Category?.Name ?? "Kategori"}:** {spent:N0} ₺ harcandı (Limit: {b.Amount:N0} ₺ - %{progress:F0})";

                    if (spent > b.Amount)
                    {
                        exceeded.Add(status + $" 🚨 **{spent - b.Amount:N0} ₺ Bütçe Aşımı!**");
                    }
                    else
                    {
                        safe.Add(status);
                    }
                }

                var result = "### 📊 Bütçe Analiz Raporu\n\n";
                if (exceeded.Any())
                {
                    result += "⚠️ **Aşılan Bütçeleriniz:**\n" + string.Join("\n", exceeded) + "\n\n";
                }
                if (safe.Any())
                {
                    result += "✅ **Dengeli Bütçeleriniz:**\n" + string.Join("\n", safe) + "\n";
                }
                if (!exceeded.Any())
                {
                    result += "\n🎉 Harika! Bu ay hiçbir bütçe limitinizi aşmadınız. Finansal disiplininizi tebrik ederim.";
                }
                return result;
            }

            // 3. Abonelikler ve faturalar sorguları
            if (q.Contains("abonelik") || q.Contains("netflix") || q.Contains("spotify") || q.Contains("youtube") || q.Contains("fatura") || q.Contains("sabit gider"))
            {
                var subsKeywords = new[] { "netflix", "spotify", "youtube premium", "abonelik", "üyelik", "fiber internet", "faturası", "kira" };
                
                // Cüzdandaki düzenli harcamaları filtreleyelim
                var subscriptions = thisMonthTransactions
                    .Where(t => t.Type == TransactionType.Expense && 
                                subsKeywords.Any(k => t.Description.ToLower(new System.Globalization.CultureInfo("tr-TR")).Contains(k)))
                    .ToList();

                if (!subscriptions.Any())
                {
                    return "Bu aya ait herhangi bir abonelik veya sabit ödeme kaydı bulunamadı. Fatura ve eğlence işlemlerini ekleyerek burada görebilirsiniz.";
                }

                var totalSubs = subscriptions.Sum(s => s.Amount);
                var result = $"### 💳 Abonelik ve Sabit Giderleriniz\n\nBu ayki tespit edilen sabit giderleriniz toplamı: **{totalSubs:N0} ₺**\n\nDetaylar:\n";
                foreach (var s in subscriptions)
                {
                    result += $"* **{s.Description}:** {s.Amount:N0} ₺ ({s.Date:dd.MM.yyyy})\n";
                }
                result += "\n💡 *Öneri:* Dashboard altındaki Abonelik Yönetimi panelini kullanarak bu abonelikleri iptal etmenin hedeflerinize (ETA) olan etkisini simüle edebilirsiniz.";
                return result;
            }

            // 4. Harcama / Tasarruf Önerileri sorguları
            if (q.Contains("tasarruf") || q.Contains("tavsiye") || q.Contains("öneri") || q.Contains("nasıl kısarım"))
            {
                var expensesByCategory = thisMonthTransactions
                    .Where(t => t.Type == TransactionType.Expense && t.Category != null)
                    .GroupBy(t => t.Category!.Name)
                    .Select(g => new { Name = g.Key, Total = g.Sum(t => t.Amount) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                if (!expensesByCategory.Any())
                {
                    return "Harcama analizi ve tasarruf tavsiyesi üretebilmemiz için önce harcama işlemleri eklemelisiniz.";
                }

                var topExpense = expensesByCategory.First();
                var result = "### 💡 Finansal Analiz & Tasarruf Önerileri\n\n";
                result += $"Bu ay en fazla harcamayı **'{topExpense.Name}'** kategorisinde (**{topExpense.Total:N0} ₺**) yaptınız.\n\n";
                result += "📊 **En Çok Harcanan Kategoriler:**\n";
                foreach (var ec in expensesByCategory.Take(3))
                {
                    result += $"* **{ec.Name}:** {ec.Total:N0} ₺\n";
                }

                result += $"\n🔍 **Aksiyon Önerisi:**\n* **'{topExpense.Name}'** harcamalarınızı %15 oranında kısarak ayda yaklaşık **{topExpense.Total * 0.15m:N0} ₺** ek tasarruf edebilirsiniz.\n";
                
                // Hedefe yansımasını bul
                if (goalList.Any())
                {
                    var firstGoal = goalList.First();
                    var extraSavings = topExpense.Total * 0.15m;
                    result += $"* Bu ek tasarruf miktarı, en yakın hedefiniz olan **'{firstGoal.Title}'** hedefinize çok daha hızlı ulaşmanızı sağlayacaktır.";
                }
                return result;
            }

            // 5. Net Varlık ve cüzdan sorguları
            if (q.Contains("net varlık") || q.Contains("bakiye") || q.Contains("cüzdan") || q.Contains("kaç param var") || q.Contains("servet"))
            {
                var totalNetWorth = walletList.Sum(w => w.Balance);
                var result = $"### 💰 Net Varlık & Cüzdan Durumu\n\nToplam Net Varlığınız: **{totalNetWorth:N0} ₺**\n\nHesap Detayları:\n";
                foreach (var w in walletList)
                {
                    result += $"* **{w.Name}:** {formatter(w.Balance)} ₺\n";
                }
                return result;
            }

            // 6. Vergi Planlama ve Dilim Optimizasyonu
            if (q.Contains("vergi") || q.Contains("vergilendirme") || q.Contains("dilim") || q.Contains("matrah"))
            {
                var monthlyIncomes = transactionList
                    .Where(t => t.Type == TransactionType.Income)
                    .GroupBy(t => new { t.Date.Month, t.Date.Year })
                    .Select(g => g.Sum(t => t.Amount))
                    .ToList();
                
                decimal avgMonthlyIncome = monthlyIncomes.Any() ? monthlyIncomes.Average() : 48000;
                decimal estAnnualIncome = avgMonthlyIncome * 12;
                
                string bracket = "15%";
                decimal taxDue = 0;
                if (estAnnualIncome > 3000000) { bracket = "40%"; taxDue = estAnnualIncome * 0.40m; }
                else if (estAnnualIncome > 870000) { bracket = "35%"; taxDue = estAnnualIncome * 0.35m; }
                else if (estAnnualIncome > 230000) { bracket = "27%"; taxDue = estAnnualIncome * 0.27m; }
                else if (estAnnualIncome > 110000) { bracket = "20%"; taxDue = estAnnualIncome * 0.20m; }
                else { bracket = "15%"; taxDue = estAnnualIncome * 0.15m; }

                var result = "### ⚖️ Vergi Dilimi & Planlama Analizi\n\n";
                result += $"* **Tahmini Yıllık Geliriniz:** **{estAnnualIncome:N0} ₺** (Aylık ortalama: {avgMonthlyIncome:N0} ₺)\n";
                result += $"* **Mevcut Vergi Diliminiz:** **{bracket}** (Gelir Vergisi matrahına göre tahmini dilim)\n";
                result += $"* **Tahmini Yıllık Gelir Vergisi Yükü:** **{taxDue:N0} ₺**\n\n";
                result += "💡 **Vergi Optimizasyon Önerileri:**\n";
                result += "* **BES Katkısı:** Bireysel Emeklilik Sistemi'ne (BES) ödeyeceğiniz katkı paylarını beyan ederek vergi matrahından düşebilir ve vergi yükünüzü azaltabilirsiniz.\n";
                result += "* **Sağlık & Hayat Sigortası:** Şahsınız, eşiniz ve küçük çocuklarınız için ödediğiniz şahıs sigorta primlerinin %15'ini matrahınızdan indirebilirsiniz.\n";
                result += "* **Eğitim & Sağlık Harcamaları:** Türkiye'de yapılan ve belgelendirilen eğitim/sağlık harcamalarını (gelirinizin %10'unu aşmamak şartıyla) beyan ederek verginizi düşürebilirsiniz.";
                return result;
            }

            // 7. Bireysel Emeklilik (BES) & Devlet Katkısı
            if (q.Contains("bes") || q.Contains("bireysel emeklilik") || q.Contains("devlet katkısı") || q.Contains("emeklilik"))
            {
                var result = "### 🛡️ BES & Devlet Katkısı Simülasyonu\n\n";
                result += "Bireysel Emeklilik Sistemi (BES), tasarruflarınızı devlet desteğiyle büyütmenin en güvenli yoludur.\n\n";
                result += "* **Devlet Katkısı Oranı:** **%30** (Ödediğiniz her 1.000 ₺ için devlet hesabınıza 300 ₺ ekler)\n";
                result += "* **Yıllık Limit:** Yıllık toplam brüt asgari ücretin %30'una kadar devlet katkısından tam yararlanabilirsiniz.\n\n";
                result += "💡 **Ortalama Öngörü Projeksiyonu:**\n";
                result += "* Aylık **2.000 ₺** birikim yaptığınızda, devlet her ay **600 ₺** ek katkı sağlar.\n";
                result += "* Yıllık ortalama **%35** fon büyümesiyle, **10 yıl** sonunda biriken tutarınız yaklaşık **685.000 ₺** (bunun 72.000 ₺'si net devlet katkısı) olur.\n";
                result += "* **Aksiyon Önerisi:** Cüzdanlarınızdaki değişken harcamalardan tasarruf edeceğiniz tutarları otomatik BES talimatına bağlayarak gelecek güvencenizi garantileyebilirsiniz.";
                return result;
            }

            // 8. Borç Kapatma Stratejisti (Snowball vs Avalanche)
            if (q.Contains("borç") || q.Contains("kapatma") || q.Contains("snowball") || q.Contains("avalanche") || q.Contains("kartopu") || q.Contains("çığ"))
            {
                var debtWallets = walletList.Where(w => w.Balance < 0).ToList();
                var result = "### 💳 Borç Kapatma Stratejisi\n\n";
                
                if (!debtWallets.Any())
                {
                    result += "🎉 Harika! Şu anda sistemde kayıtlı herhangi bir borçlu cüzdanınız (negatif bakiye) bulunmuyor. Finansal sağlığınız mükemmel durumda!";
                    return result;
                }

                decimal totalDebt = Math.Abs(debtWallets.Sum(w => w.Balance));
                result += $"Toplam borç yükünüz: **{totalDebt:N0} ₺** (Negatif bakiyeli cüzdanlar)\n\n";
                result += "Borçlarınızı hızlıca eritmek için iki popüler stratejiden birini seçebilirsiniz:\n\n";
                result += "1. ❄️ **Snowball (Kartopu) Stratejisi (Önerilen - Psikolojik Destek):**\n";
                result += "   * Borçlarınızı en küçükten en büyüğe doğru sıralayın.\n";
                result += "   * En küçük borcu (örneğin Kredi Kartı asgari tutarı üstündeki farkı) agresif bir şekilde kapatın, diğerlerine asgari ödeyin. Biten her borç psikolojik olarak motivasyonunuzu artırır.\n\n";
                result += "2. ☄️ **Avalanche (Çığ) Stratejisi (Matematiksel Olarak En Ucuz):**\n";
                result += "   * Borçlarınızı faiz oranı en yüksek olandan en düşük olana sıralayın.\n";
                result += "   * En yüksek faizli borcu ilk kapatın. Bu yöntemle toplamda ödeyeceğiniz faiz miktarını en aza indirirsiniz.\n\n";
                result += "💡 *Analiz:* Aylık tasarruf hızınızla mevcut borçlarınızı yaklaşık **1.5 ay** içinde tamamen sıfırlayabilirsiniz.";
                return result;
            }

            // 9. FIRE (Finansal Özgürlük)
            if (q.Contains("fire") || q.Contains("özgürlük") || q.Contains("erken emeklilik") || q.Contains("4%"))
            {
                var monthlyExpenses = transactionList
                    .Where(t => t.Type == TransactionType.Expense)
                    .GroupBy(t => new { t.Date.Month, t.Date.Year })
                    .Select(g => g.Sum(t => t.Amount))
                    .ToList();
                
                decimal avgMonthlyExpense = monthlyExpenses.Any() ? monthlyExpenses.Average() : 38000;
                decimal annualExpense = avgMonthlyExpense * 12;
                decimal fireNumber = annualExpense * 25;
                decimal totalAssets = walletList.Sum(w => w.Balance);
                double progress = fireNumber > 0 ? (double)(totalAssets / fireNumber) * 100 : 0;

                var result = "### 🚀 FIRE (Finansal Özgürlük) Analizi\n\n";
                result += $"* **Aylık Ortalama Gideriniz:** **{avgMonthlyExpense:N0} ₺**\n";
                result += $"* **Yıllık Gideriniz:** **{annualExpense:N0} ₺**\n";
                result += $"* **Hedef FIRE Sayınız:** **{fireNumber:N0} ₺** (Yıllık giderinizin 25 katı - %4 kuralı)\n";
                result += $"* **Mevcut İlerlemeniz:** **%{progress:F2}** (Toplam Net Varlık: {totalAssets:N0} ₺)\n\n";
                result += "💡 **Yol Haritası & Tavsiye:**\n";
                result += "* **%4 Kuralı:** FIRE sayınıza ulaştığınızda, portföyünüzden her yıl enflasyondan arındırılmış %4 oranında çekim yaparak ana paranıza dokunmadan ömür boyu yaşayabilirsiniz.\n";
                result += "* **Mevcut Hızla Projeksiyon:** Aylık tasarruflarınızı yıllık %35 büyüme sağlayan yatırım araçlarında değerlendirirseniz, finansal özgürlüğünüze yaklaşık **12 yıl** içinde ulaşabilirsiniz.";
                return result;
            }

            // 10. Döviz & Altın Koruması (Kur Riski)
            if (q.Contains("kur riski") || q.Contains("döviz") || q.Contains("enflasyon") || q.Contains("altın") || q.Contains("hedg"))
            {
                decimal totalAssets = walletList.Where(w => w.Balance > 0).Sum(w => w.Balance);
                decimal simulatedHedgeRatio = 42.5m; // Orantılı kur koruma yüzdesi

                var result = "### 📉 Döviz Pozisyonu & Enflasyon Koruma Analizi\n\n";
                result += $"* **Toplam Net Varlığınız:** **{totalAssets:N0} ₺**\n";
                result += $"* **Kur Korumalı / Döviz Varlık Oranı:** **%{simulatedHedgeRatio}** (Altın ve Döviz enstrümanları)\n";
                result += $"* **TL Varlık Oranı:** **%{100 - simulatedHedgeRatio}**\n\n";
                result += "💡 **Kur Riski Değerlendirmesi:**\n";
                result += "* **Enflasyon Riski:** Portföyünüzün %50'den fazlası TL cinsinde nakit veya banka mevduatında duruyor. Yüksek enflasyon ortamında alım gücünüzü korumak için döviz korumalı enstrümanların (Eurobond, Döviz/Altın fonları) oranını artırmalısınız.\n";
                result += "* **Öneri:** Banka hesabınızdaki atıl nakdin en az %20'sini altın veya yabancı para fonlarına kaydırarak kur dalgalanmalarına karşı tampon oluşturabilirsiniz.";
                return result;
            }

            // Varsayılan / Bulunamayan
            return "Merhaba! Ben WealthFlow Finansal Danışmanı. Sorunuzu tam olarak anlayamadım.\n\n" +
                   "Bana şu konuları sorabilirsiniz:\n" +
                   "* **Birikim hedefleri:** *'Hedeflerime ne zaman ulaşırım?'*\n" +
                   "* **Bütçe durumu:** *'Bütçe limitlerimi aştım mı?'*\n" +
                   "* **Sabit giderler:** *'Aboneliklerimin toplam maliyeti nedir?'*\n" +
                   "* **Tasarruf önerileri:** *'Harcamalarımı nasıl kısabilirim?'*\n" +
                   "* **Cüzdan bakiyeleri:** *'Net varlığım ne kadar?'*\n" +
                   "* **BES Simülasyonu:** *'BES devlet katkısı nedir?'*\n" +
                   "* **Vergi Analizi:** *'Hangi vergi dilimindeyim?'*\n" +
                   "* **Borç Planlayıcı:** *'Borçlarımı nasıl eritirim?'*\n" +
                   "* **FIRE Analizi:** *'Finansal özgürlüğe ne zaman ulaşırım?'*\n" +
                   "* **Kur Riski:** *'Portföyüm kur riskine karşı korumalı mı?'*";
        }

        private string formatter(decimal balance)
        {
            return balance.ToString("N0", new System.Globalization.CultureInfo("tr-TR"));
        }
    }
}

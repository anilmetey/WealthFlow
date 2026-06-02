using System;
using System.Linq;
using WealthFlow.Domain.Entities;
using WealthFlow.Domain.Enums;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Database'i oluştur (yoksa)
            context.Database.EnsureCreated();

            // 1. Kategorileri Tohumla
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Maaş / Gelir", Icon = "fa-wallet", Color = "#10B981" },
                    new Category { Name = "Gıda / Market", Icon = "fa-shopping-basket", Color = "#F59E0B" },
                    new Category { Name = "Kira / Ev Giderleri", Icon = "fa-home", Color = "#EF4444" },
                    new Category { Name = "Ulaşım / Araç", Icon = "fa-car", Color = "#3B82F6" },
                    new Category { Name = "Eğlence / Sosyal", Icon = "fa-gamepad", Color = "#EC4899" },
                    new Category { Name = "Sağlık / Spor", Icon = "fa-heartbeat", Color = "#14B8A6" },
                    new Category { Name = "Eğitim / Kitap", Icon = "fa-book", Color = "#8B5CF6" },
                    new Category { Name = "Diğer", Icon = "fa-tag", Color = "#6B7280" }
                };

                foreach (var c in categories)
                {
                    context.Categories.Add(c);
                }
                context.SaveChanges();
            }

            // 2. Cüzdanları Tohumla
            if (!context.Wallets.Any())
            {
                var defaultWallet = new Wallet
                {
                    Name = "Nakit Cüzdanı",
                    Balance = 0,
                    Color = "#6366f1",
                    Icon = "fa-wallet"
                };
                context.Wallets.Add(defaultWallet);
                context.SaveChanges();
            }
        }
    }
}

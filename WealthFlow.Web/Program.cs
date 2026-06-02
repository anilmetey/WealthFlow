using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using WealthFlow.Application.Interfaces;
using WealthFlow.Application.Mappings;
using WealthFlow.Application.Services;
using WealthFlow.Application.Validation;
using WealthFlow.Domain.Interfaces;
using WealthFlow.Infrastructure.Contexts;
using WealthFlow.Infrastructure.Data;
using WealthFlow.Infrastructure.Repositories;
using WealthFlow.Web.Middleware;

// Serilog Yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog entegrasyonu
    builder.Host.UseSerilog();

    // MVC Controller'ları ve Görünümleri
    builder.Services.AddControllersWithViews();

    // Cookie Authentication Kaydı
    builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.Name = "WealthFlow.Auth";
        });


    // Swagger API Dökümantasyonu
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "WealthFlow API", Version = "v1" });
    });

    // SQLite DB Context Kaydı
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    // UnitOfWork Kaydı
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Servis Katmanı Kayıtları
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IBudgetService, BudgetService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<IFinancialGoalService, FinancialGoalService>();
    builder.Services.AddScoped<IWalletService, WalletService>();
    builder.Services.AddScoped<IInsightService, InsightService>();

    // AutoMapper Kaydı (Application katmanındaki profilleri otomatik tarar)
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

    // FluentValidation Kaydı (Assembly taraması yaparak hedefler vb. tüm validatorleri kaydeder)
    builder.Services.AddValidatorsFromAssemblyContaining<TransactionValidator>();

    var app = builder.Build();

    // SQLite Veritabanı ve Seed verileri kontrolü
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            DbInitializer.Initialize(context);
            Log.Information("Veritabanı başarıyla tohumlandı.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Veritabanı tohumlanırken kritik bir hata oluştu.");
        }
    }

    // Global Exception Handling Middleware Entegrasyonu
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Swagger UI Aktifleştirme (Geliştirme aşamasında API testleri için)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WealthFlow API v1");
    });

    // MVC Route Yapısı
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik bir şekilde durduruldu.");
}
finally
{
    Log.CloseAndFlush();
}

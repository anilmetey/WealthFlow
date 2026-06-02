using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WealthFlow.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace WealthFlow.Web.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;

        public BudgetsController(IBudgetService budgetService, ICategoryService categoryService)
        {
            _budgetService = budgetService;
            _categoryService = categoryService;
        }

        // GET: Budgets
        public async Task<IActionResult> Index(int? month, int? year)
        {
            var today = DateTime.Today;
            var targetMonth = month ?? today.Month;
            var targetYear = year ?? today.Year;

            var budgets = await _budgetService.GetBudgetsByMonthYearAsync(targetMonth, targetYear);
            var categories = await _categoryService.GetAllAsync();

            // Sadece bu ay bütçesi olmayan kategorileri listele (yeni ekleme için)
            var budgetList = budgets.ToList();
            var categoriesWithoutBudget = categories
                .Where(c => !budgetList.Any(b => b.CategoryId == c.Id))
                .OrderBy(c => c.Name)
                .ToList();

            ViewBag.CategoryId = new SelectList(categoriesWithoutBudget, "Id", "Name");
            ViewBag.CategoryCount = categoriesWithoutBudget.Count;
            ViewBag.AllCategories = new SelectList(categories.OrderBy(c => c.Name).ToList(), "Id", "Name");
            ViewBag.SelectedMonth = targetMonth;
            ViewBag.SelectedYear = targetYear;

            return View(budgets);
        }
    }
}

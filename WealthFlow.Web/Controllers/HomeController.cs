using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace WealthFlow.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;

        public HomeController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            // Kategorileri alarak Layout'taki Hızlı İşlem Modalı için ViewBag'e yükleyelim
            var categories = await _categoryService.GetAllAsync();
            ViewBag.CategoriesList = categories;
            return View();
        }

        public IActionResult AuditLogs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

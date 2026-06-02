using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WealthFlow.Application.DTOs;
using WealthFlow.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;

namespace WealthFlow.Web.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            PopulateIconsAndColors();
            return View(new CategoryDto());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(dto);
                TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            PopulateIconsAndColors();
            return View(dto);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryService.GetByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            PopulateIconsAndColors();
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(dto);
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            PopulateIconsAndColors();
            return View(dto);
        }

        private void PopulateIconsAndColors()
        {
            ViewBag.AvailableIcons = new string[]
            {
                "fa-shopping-basket", "fa-home", "fa-car", "fa-gamepad", "fa-heartbeat", "fa-book",
                "fa-tag", "fa-utensils", "fa-plane", "fa-gift", "fa-film", "fa-tshirt", "fa-dumbbell",
                "fa-wallet", "fa-laptop", "fa-coffee", "fa-wrench", "fa-briefcase", "fa-graduation-cap"
            };

            ViewBag.AvailableColors = new string[]
            {
                "#EF4444", "#F59E0B", "#10B981", "#3B82F6", "#EC4899", "#8B5CF6", "#6366F1", "#14B8A6",
                "#6B7280", "#059669", "#DC2626", "#D97706", "#2563EB", "#C084FC", "#F472B6", "#4B5563"
            };
        }
    }
}

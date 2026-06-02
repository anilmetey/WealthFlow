using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WealthFlow.Domain.Entities;
using WealthFlow.Infrastructure.Contexts;

namespace WealthFlow.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password, string passwordConfirm)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Tüm alanlar zorunludur.");
                return View();
            }

            if (password != passwordConfirm)
            {
                ModelState.AddModelError(string.Empty, "Şifreler eşleşmiyor.");
                return View();
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            if (existingUser)
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta adresi zaten kullanımda.");
                return View();
            }

            var user = new User
            {
                Name = name.Trim(),
                Email = email.Trim(),
                PasswordHash = HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kayıt başarıyla tamamlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "E-posta ve şifre zorunludur.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Hatalı e-posta veya şifre.");
                return View();
            }

            var initials = GetInitials(user.Name);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Avatar", initials)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    ViewBag.UserName = user.Name;
                    ViewBag.UserEmail = user.Email;
                    return View();
                }
            }
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string name, string email, string? currentPassword, string? newPassword, string? newPasswordConfirm)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.UserName = name;
            ViewBag.UserEmail = email;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Ad Soyad ve E-posta alanları zorunludur.");
                return View();
            }

            // E-posta benzersizlik kontrolü
            var emailExists = await _context.Users.AnyAsync(u => u.Id != user.Id && u.Email.ToLower() == email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
                return View();
            }

            user.Name = name.Trim();
            user.Email = email.Trim();

            // Şifre değiştirme talebi varsa
            if (!string.IsNullOrWhiteSpace(currentPassword) || !string.IsNullOrWhiteSpace(newPassword))
            {
                if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    ModelState.AddModelError(string.Empty, "Şifre değiştirmek için mevcut şifrenizi ve yeni şifrenizi girmelisiniz.");
                    return View();
                }

                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Mevcut şifreniz hatalı.");
                    return View();
                }

                if (newPassword != newPasswordConfirm)
                {
                    ModelState.AddModelError(string.Empty, "Yeni şifreler eşleşmiyor.");
                    return View();
                }

                user.PasswordHash = HashPassword(newPassword);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Giriş yapmış kullanıcının claims bilgilerini güncelle
            var initials = GetInitials(user.Name);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Avatar", initials)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        // Hash Helper
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "U";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }
    }
}

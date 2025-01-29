//ProfileController.cs- Edit and Del operation
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synergy.Data;
using Synergy.Models;

namespace Synergy.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var profile = _context.Profiles
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(profile);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Profile profile)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                profile.UserId = userId;

                _context.Add(profile);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        public IActionResult Edit()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Profile profile)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (profile.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profile);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(profile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }

        private bool ProfileExists(int id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }

        public IActionResult Delete()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);

            if (profile != null)
            {
                _context.Profiles.Remove(profile);
                _context.SaveChanges();

                // Clear cookies and sign out the user
                await HttpContext.SignOutAsync("Cookies");
                HttpContext.Response.Cookies.Delete(".AspNetCore.Cookies");

                return RedirectToAction("Index", "Home");
            }

            return NotFound();
        }



    }
}
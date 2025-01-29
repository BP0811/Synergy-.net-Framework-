using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synergy.Data;
using Synergy.Models;
using Synergy.Models.ViewModels;

namespace Synergy.Controllers
{
    [Authorize] // Only logged-in users can access
    public class MatchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show potential matches (other users)
        public IActionResult Index()
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Get current user's profile
            var userProfile = _context.Profiles
                .FirstOrDefault(p => p.UserId == currentUserId);

            if (userProfile == null)
            {
                return RedirectToAction("Create", "Profile");
            }

            // Get potential matches (excluding already matched users)
            var potentialMatches = _context.Profiles
                .Include(p => p.User)
                .Where(p => p.UserId != currentUserId &&
                           !_context.Matches.Any(m =>
                               m.UserId == currentUserId &&
                               m.MatchUserId == p.UserId))
                .ToList();

            return View(potentialMatches);
        }

        // Show user's matches
        public IActionResult MyMatches()
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Get mutual matches (where both users liked each other)
            var mutualMatches = _context.Matches
                .Include(m => m.MatchUser)
                .ThenInclude(u => u.Profile)
                .Where(m => m.UserId == currentUserId &&
                           _context.Matches.Any(m2 =>
                               m2.UserId == m.MatchUserId &&
                               m2.MatchUserId == currentUserId))
                .ToList();

            return View(mutualMatches);
        }

        // Like a user
        [HttpPost]
        public IActionResult Like(int matchUserId)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Check if already liked
            var existingMatch = _context.Matches
                .FirstOrDefault(m => m.UserId == currentUserId &&
                                   m.MatchUserId == matchUserId);

            if (existingMatch == null)
            {
                var match = new Match
                {
                    UserId = currentUserId,
                    MatchUserId = matchUserId,
                    MatchDate = DateTime.Now
                };

                _context.Matches.Add(match);
                _context.SaveChanges();

                // Check if it's a mutual match
                var mutualMatch = _context.Matches
                    .Any(m => m.UserId == matchUserId &&
                             m.MatchUserId == currentUserId);

                if (mutualMatch)
                {
                    TempData["MatchAlert"] = "Congratulations! You have a new match!";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Unlike a user
        [HttpPost]
        public IActionResult Unlike(int matchUserId)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var match = _context.Matches
                .FirstOrDefault(m => m.UserId == currentUserId &&
                                   m.MatchUserId == matchUserId);

            if (match != null)
            {
                _context.Matches.Remove(match);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(MyMatches));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Userstories.Data;
using Userstories.Models;

namespace Userstories.Controllers
{
    [Authorize(AuthenticationSchemes = "InstructorScheme")]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public class AchievementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AchievementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Achievements
        public async Task<IActionResult> InstructorIndex()
        {
            var achievements = await _context.Achievement.Include(a => a.Learner).Include(a => a.Badge).ToListAsync();
            return View(achievements);
        }

        // GET: Add Achievement
        public IActionResult AddAchievement()
        {
            return View();
        }

        // POST: Add Achievement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAchievement(Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                // Check if LearnerID and BadgeID exist
                var learnerExists = await _context.Learner.AnyAsync(l => l.LearnerID == achievement.LearnerID);
                var badgeExists = await _context.Badge.AnyAsync(b => b.BadgeID == achievement.BadgeID);

                if (!learnerExists || !badgeExists)
                {
                    Console.WriteLine("Invalid LearnerID or BadgeID.");
                    ModelState.AddModelError(string.Empty, "Invalid LearnerID or BadgeID.");
                    return View(achievement);
                }

                achievement.date_earned = DateTime.Now;
                _context.Achievement.Add(achievement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(InstructorIndex));
            }

            return View(achievement);
        }

        // GET: My Achievements
        [Authorize(AuthenticationSchemes = "LearnerScheme")]
        public async Task<IActionResult> MyAchievements()
        {
            var learnerId = int.Parse(User.Identity.Name); // Assuming LearnerID is stored in the Name claim

            var achievements = await _context.Achievement
                .Where(a => a.LearnerID == learnerId)
                .Include(a => a.Badge)
                .ToListAsync();

            return View(achievements);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Userstories.Data;
using Userstories.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;

namespace Userstories.Controllers
{
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public class QuestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quests
        public async Task<IActionResult> LearnerIndex()
        {
            var quests = await _context.Collaborative.ToListAsync();
            return View(quests);
        }

        // GET: Check Participants
        public async Task<IActionResult> CheckParticipants(int questId)
        {
            var participants = new List<LearnerParticipationViewModel>();

            var questIdParam = new SqlParameter("@QuestID", questId);
            var commandText = "EXEC GetQuestParticipants @QuestID";

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(questIdParam);

                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        participants.Add(new LearnerParticipationViewModel
                        {
                            LearnerID = result.GetInt32(0),
                            FirstName = result.IsDBNull(1) ? null : result.GetString(1),
                            LastName = result.IsDBNull(2) ? null : result.GetString(2),
                            CompletionStatus = result.IsDBNull(3) ? null : result.GetString(3)
                        });
                    }
                }
            }

            return View(participants);
        }

        // POST: Participate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Participate(int questId)
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToAction("Login", "Learner");
            }

            if (!int.TryParse(User.Identity.Name, out var learnerId))
            {
                return BadRequest("Invalid LearnerID.");
            }

            // Check if the learner has already participated in the quest
            var existingParticipation = await _context.LearnerCollaboration
                .FirstOrDefaultAsync(lc => lc.QuestID == questId && lc.LearnerID == learnerId);

            if (existingParticipation != null)
            {
                // Optionally, you can add a message to inform the learner
                TempData["ErrorMessage"] = "You have already participated in this quest.";
                return RedirectToAction("MyQuests");
            }

            var learnerCollaboration = new LearnerCollaboration
            {
                QuestID = questId,
                LearnerID = learnerId,
                completion_status = "In Progress" // Default status
            };

            _context.LearnerCollaboration.Add(learnerCollaboration);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyQuests");
        }

        // GET: MyQuests
        public async Task<IActionResult> MyQuests()
        {
            var learnerId = int.Parse(User.Identity.Name); // Assuming LearnerID is stored in the Name claim

            var quests = await _context.LearnerCollaboration
                .Where(lc => lc.LearnerID == learnerId)
                .Include(lc => lc.Collaborative)
                .Select(lc => new
                {
                    lc.Collaborative,
                    lc.completion_status
                })
                .ToListAsync();

            var viewModel = quests.Select(q => new LearnerQuestViewModel
            {
                Quest = q.Collaborative,
                CompletionStatus = q.completion_status
            }).ToList();

            return View(viewModel);
        }
    
        // GET: Add Quest
        public IActionResult AddQuest()
        {
            return View();
        }

        // POST: Add Quest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuest(Collaborative quest)
        {
            if (ModelState.IsValid)
            {
                _context.Collaborative.Add(quest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(InstructorIndex));
            }

            return View(quest);
        }
        // GET: Update Deadline
        public async Task<IActionResult> UpdateDeadline(int id)
        {
            var quest = await _context.Collaborative.FindAsync(id);
            if (quest == null)
            {
                return NotFound();
            }
            return View(quest);
        }

// POST: Update Deadline
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeadline(int id, DateTime newDeadline)
        {
            var quest = await _context.Collaborative.FindAsync(id);
            if (quest == null)
            {
                return NotFound();
            }

            quest.deadline = newDeadline;
            _context.Update(quest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InstructorIndex));
        }
        // GET: Instructor Quests
        public async Task<IActionResult> InstructorIndex(string criteria)
        {
            var quests = _context.Collaborative.AsQueryable();

            if (!string.IsNullOrEmpty(criteria))
            {
                criteria = criteria ?? string.Empty;
                quests = quests.Where(q => (q.criteria ?? string.Empty).Contains(criteria));
                ViewData["Criteria"] = criteria;
            }

            return View(await quests.ToListAsync());
        }

        // POST: Delete Quest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            var quest = await _context.Collaborative.FindAsync(id);
            if (quest == null)
            {
                return NotFound();
            }

            _context.Collaborative.Remove(quest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InstructorIndex));
        }
        // GET: Instructor Quests
        public async Task<IActionResult> AdminIndex(string criteria)
        {
            var quests = _context.Collaborative.AsQueryable();

            if (!string.IsNullOrEmpty(criteria))
            {
                criteria = criteria ?? string.Empty;
                quests = quests.Where(q => (q.criteria ?? string.Empty).Contains(criteria));
                ViewData["Criteria"] = criteria;
            }

            return View(await quests.ToListAsync());
        }
        // POST: Delete Quest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminDeleteQuest(int id)
        {
            var quest = await _context.Collaborative.FindAsync(id);
            if (quest == null)
            {
                return NotFound();
            }

            _context.Collaborative.Remove(quest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
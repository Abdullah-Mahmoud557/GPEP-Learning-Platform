using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Userstories.Data;
using Userstories.Models;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Userstories.Controllers;

public class LearnerController : Controller
{
    private readonly ApplicationDbContext _context;

    public LearnerController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(Learner learner)
    {

        if (!await _context.EmailExistsAsync(learner.Email))
        {
            try
            {


                // Save the learner to the database
                _context.Learner.Add(learner);
                await _context.SaveChangesAsync();

                // Call Login directly after successful signup
                return await Login(learner.Email, learner.Password);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }
        }
       
         ModelState.AddModelError(string.Empty, "Email already exists.");
          return View();
    }



    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var learner = await _context.Learner.FirstOrDefaultAsync(l => l.Email == email && l.Password == password);
        if (learner != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, learner.LearnerID.ToString()), // Store LearnerID in the Name claim
                new Claim(ClaimTypes.Email, learner.Email) // Optional: store email
            };

            var claimsIdentity = new ClaimsIdentity(claims, "LearnerScheme");
            await HttpContext.SignInAsync("LearnerScheme", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Dashboard", new { id = learner.LearnerID });
        }

        ModelState.AddModelError("", "Invalid credentials.");
        return View();
    }


    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public IActionResult UploadImage()
    {
        return View();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> UploadImage(int id, IFormFile image)
    {
        if (image != null && image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var learner = await _context.Learner.FindAsync(id);
                if (learner != null)
                {
                    learner.ImageData = memoryStream.ToArray();
                    _context.Update(learner);
                    await _context.SaveChangesAsync();
                }
            }
        }

        return RedirectToAction("Dashboard", new { id });
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> UnsetImage(int id)
    {
        var learner = await _context.Learner.FindAsync(id);
        if (learner != null)
        {
            learner.ImageData = null;
            _context.Update(learner);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard", new { id });
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> Profiles()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid user state. LearnerID is missing or invalid.");
        }

        try
        {
            var profiles = await _context.PersonalizationProfiles
                .Where(p => p.LearnerID == learnerId)
                .ToListAsync();
            return View(profiles);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
        }

        return View();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public IActionResult CreateProfile()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public IActionResult CreateProfile(PersonalizationProfiles model)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        try
        {
            model.LearnerID = learnerId;

            // Ensure ProfileID is not set explicitly
            model.ProfileID = 0;

            _context.PersonalizationProfiles.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Profiles));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> EditProfile(int profileId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var profile = await _context.PersonalizationProfiles
            .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> DeleteProfile(int profileId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var profile = await _context.PersonalizationProfiles
            .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

        if (profile == null)
        {
            return NotFound("Profile not found.");
        }

        _context.PersonalizationProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Profiles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> EditProfile(int profileId, PersonalizationProfiles model)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        if (learnerId != model.LearnerID || profileId != model.ProfileID)
        {
            return BadRequest("Invalid LearnerID or ProfileID.");
        }

        var profile = await _context.PersonalizationProfiles
            .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

        if (profile == null)
        {
            return NotFound();
        }

        // Update the profile
        profile.PreferredContentType = model.PreferredContentType;
        profile.EmotionalState = model.EmotionalState;
        profile.PersonalityType = model.PersonalityType;

        await _context.SaveChangesAsync();

        // Redirect to prevent form resubmission
        return RedirectToAction(nameof(Profiles));
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> Dashboard(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        var learner = await _context.Learner
            .Include(l => l.PersonalizationProfiles) // Include profiles
            .FirstOrDefaultAsync(l => l.LearnerID == id);

        if (learner == null)
        {
            return NotFound();
        }

        return View("LearnerDashboard", learner); // Update the view if necessary
    }

// GET: Confirm Account Deletion
    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public IActionResult ConfirmAccountDeletion()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("ChooseRole", "Account");
        }

        return View();
    }

// POST: Remove the Learner's Account
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> RemoveAccount()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var learner = await _context.Learner.FindAsync(learnerId);

        if (learner == null)
        {
            return NotFound("Learner account not found.");
        }

        _context.Learner.Remove(learner);
        await _context.SaveChangesAsync();

        // Log the user out after deleting their account
        await HttpContext.SignOutAsync("LearnerScheme"); // Use "LearnerScheme" explicitly

        return View("~/Views/Account/ChooseRole.cshtml"); // Redirect to a confirmation page
    }

// GET: Account Deleted Confirmation
    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public IActionResult AccountDeleted()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("ChooseRole", "Account");
        }

        return View();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> ViewProfile()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var learner = await _context.Learner.FirstOrDefaultAsync(l => l.LearnerID == learnerId);

        if (learner == null)
        {
            return NotFound("Learner profile not found.");
        }

        return View(learner); // Pass the learner model to the view
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> EditProfileDetails()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var learner = await _context.Learner.FirstOrDefaultAsync(l => l.LearnerID == learnerId);

        if (learner == null)
        {
            return NotFound("Learner profile not found.");
        }

        return View(learner); // Display the EditProfileDetails.cshtml view
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> EditProfileDetails(Learner model)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID in claims.");
        }

        var learner = await _context.Learner.FirstOrDefaultAsync(l => l.LearnerID == learnerId);

        if (learner == null)
        {
            return NotFound("Learner profile not found.");
        }

        // Update learner properties
        learner.FirstName = model.FirstName;
        learner.LastName = model.LastName;
        learner.Gender = model.Gender;
        learner.BirthDate = model.BirthDate;
        learner.Country = model.Country;
        learner.CulturalBackground = model.CulturalBackground;

        await _context.SaveChangesAsync();

        return RedirectToAction("ViewProfile"); // Redirect back to the profile view
    }

    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> Dashboard2()
    {
        var learnerid = int.Parse(User.Identity.Name);
        return RedirectToAction("Dashboard", new { id = learnerid });
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> MyGoals()
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID.");
        }

        var myGoals = await _context.LearnersGoals
            .Include(lg => lg.LearningGoal)
            .Where(lg => lg.LearnerID == learnerId)
            .Select(lg => lg.LearningGoal)
            .ToListAsync();

        return View(myGoals); // Pass the learner's goals to the view
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> AvailableGoals()
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID.");
        }

        var myGoalIds = await _context.LearnersGoals
            .Where(lg => lg.LearnerID == learnerId)
            .Select(lg => lg.GoalID)
            .ToListAsync();

        var availableGoals = await _context.LearningGoals
            .Where(g => !myGoalIds.Contains(g.ID))
            .ToListAsync();

        return View(availableGoals); // Pass the available goals to the view
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> AddGoal(int goalId)
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner");
        }

        if (!int.TryParse(User.Identity.Name, out var learnerId))
        {
            return BadRequest("Invalid LearnerID.");
        }

        var goalExists = await _context.LearningGoals.AnyAsync(g => g.ID == goalId);
        if (!goalExists)
        {
            return NotFound("Goal not found.");
        }

        var learnerGoal = new LearnersGoals
        {
            LearnerID = learnerId,
            GoalID = goalId
        };

        _context.LearnersGoals.Add(learnerGoal);
        await _context.SaveChangesAsync();

        return RedirectToAction("MyGoals");
    }

    // GET: View Learning Paths for the Current Learner
    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> ViewLearningPaths()
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner"); // Redirect unauthenticated users
        }

        try
        {
            var learnerId = int.Parse(User.Identity.Name); // Retrieve LearnerID from the Name claim

            // Fetch paths for the authenticated learner
            var paths = await _context.LearningPath
                .Where(path => path.LearnerID == learnerId)
                .Select(path => new LearningPath
                {
                    PathID = path.PathID,
                    LearnerID = path.LearnerID,
                    ProfileID = path.ProfileID,
                    CompletionStatus = path.CompletionStatus ?? "N/A",
                    CustomContent = path.CustomContent ?? "N/A",
                    AdaptiveRules = path.AdaptiveRules ?? "N/A"
                })
                .ToListAsync();

            return View(paths); // Pass the learner's paths to the view
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }
        
    }
    // View taken assessments
    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> ViewTakenAssessments()
    {
        var learnerId = int.Parse(User.Identity.Name); // Retrieve learner ID from the authenticated user
        var takenAssessments = await _context.TakenAssessments
            .Include(ta => ta.Assessment)
            .Where(ta => ta.LearnerID == learnerId)
            .Select(ta => new
            {
                ta.AssessmentID,
                ta.ScoredPoints,
                ta.Assessment.Title,
                ta.Assessment.Description,
                ta.Assessment.TotalMarks,
                ta.Assessment.PassingMarks,
                ta.Assessment.Type
            })
            .ToListAsync();

        return View(takenAssessments);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> AssessmentBreakdown(int assessmentId)
    {
        var learnerId = int.Parse(User.Identity.Name);

        var breakdown = await _context.TakenAssessments
            .Include(ta => ta.Assessment)
            .Where(ta => ta.LearnerID == learnerId && ta.AssessmentID == assessmentId)
            .Select(ta => new
            {
                ta.ScoredPoints,
                TotalMarks = ta.Assessment.TotalMarks,
                Percentage = (ta.ScoredPoints * 100.0 / ta.Assessment.TotalMarks),
                Performance = ta.ScoredPoints >= ta.Assessment.PassingMarks ? "Pass" : "Fail"
            })
            .FirstOrDefaultAsync();

        if (breakdown == null)
        {
            return RedirectToAction("ErrorPage", "Home", new { message = "Assessment details not found." });
        }

        return View(breakdown);
    }
    [HttpGet]
    public async Task<IActionResult> Courses(int id)
    {
        var learnerIdParam = new SqlParameter("@LearnerID", id);
        var courses = await _context.Course.ToListAsync();           
        ViewBag.LearnerID = id;
        return View(courses);
    }
        [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> ViewNotifications()
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner");
        }

        int learnerId;
        if (!int.TryParse(User.Identity.Name, out learnerId))
        {
            return RedirectToAction("Login", "Learner");
        }

        // Fetch notifications for the learner
        var notifications = await _context.ReceivedNotification
            .Where(rn => rn.LearnerID == learnerId)
            .Include(rn => rn.Notification)
            .Select(rn => new
            {
                rn.NotificationID,
                rn.Notification.Timestamp,
                rn.Notification.Message,
                rn.Notification.UrgencyLevel,
                rn.ReadStatus
            })
            .ToListAsync();

        // if (!notifications.Any())
        // {
        //     TempData["Message"] = "No notifications found.";
        //     return RedirectToAction("Dashboard2");
        // }

        ViewBag.LearnerID = learnerId;
        return View(notifications);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
        {
            return RedirectToAction("Login", "Learner");
        }

        int learnerId = int.Parse(User.Identity.Name); // Assuming LearnerID is stored in the Name claim

        // Fetch the notification record
        var receivedNotification = await _context.ReceivedNotification
            .FirstOrDefaultAsync(rn => rn.NotificationID == notificationId && rn.LearnerID == learnerId);

        if (receivedNotification == null)
        {
            TempData["Message"] = "Notification not found.";
            return RedirectToAction("ViewNotifications");
        }

        // Mark as read
        receivedNotification.ReadStatus = true;
        await _context.SaveChangesAsync();

        TempData["Message"] = "Notification marked as read.";
        return RedirectToAction("ViewNotifications");
    }
    
    [HttpGet]
    [Authorize(AuthenticationSchemes = "LearnerScheme")]
    public async Task<IActionResult> ViewForums()
    {
        // Fetch all discussion forums
        var forums = await _context.DiscussionForum.ToListAsync();
        return View(forums);
    } 
    
[HttpGet]
[Authorize(AuthenticationSchemes = "LearnerScheme")]
public async Task<IActionResult> ViewPosts(int forumId)
{
    // Fetch posts from both learners and instructors
    var learnerPosts = await _context.LearnerDiscussion
        .Where(ld => ld.ForumID == forumId)
        .Select(ld => new
        {
            AuthorType = "Learner",
            AuthorID = ld.LearnerID,
            Post = ld.Post,
            Time = ld.Time
        })
        .ToListAsync();

    var instructorPosts = await _context.InstructorDiscussion
        .Where(id => id.ForumID == forumId)
        .Select(id => new
        {
            AuthorType = "Instructor",
            AuthorID = id.InstructorID,
            Post = id.Post,
            Time = id.Time
        })
        .ToListAsync();

    // Combine the posts into one collection
    var posts = learnerPosts
        .Union(instructorPosts)
        .OrderBy(p => p.Time) // Order posts by time
        .ToList();

    ViewBag.ForumId = forumId;
    return View(posts);
}

[HttpPost]
[Authorize(AuthenticationSchemes = "LearnerScheme")]
public async Task<IActionResult> PostAsLearner(int forumId, string postContent)
{
    if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
    {
        return Unauthorized("You need to be logged in as a learner to post.");
    }

    if (!int.TryParse(User.Identity.Name, out int learnerId))
    {
        return BadRequest("Invalid Learner ID.");
    }

    if (string.IsNullOrWhiteSpace(postContent))
    {
        TempData["Message"] = "Post content cannot be empty.";
        return RedirectToAction("ViewPosts", new { forumId });
    }

    var post = new LearnerDiscussion
    {
        ForumID = forumId,
        LearnerID = learnerId,
        Post = postContent,
        Time = DateTime.Now
    };

    _context.LearnerDiscussion.Add(post);
    await _context.SaveChangesAsync();

    TempData["Message"] = "Post added successfully!";
    return RedirectToAction("ViewPosts", new { forumId });
}




}
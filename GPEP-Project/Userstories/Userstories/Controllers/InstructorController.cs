using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Userstories.Data;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Userstories.Models;


namespace Userstories.Controllers
{
    public class InstructorController : Controller
    {

        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                if (await _context.EmailExistsAsync(instructor.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(instructor);
                }

                // Save the instructor to the database
                _context.Instructor.Add(instructor);
                await _context.SaveChangesAsync();

                // Call the Login method to log in the instructor
                return await Login(instructor.Email, instructor.Password);
            }

            return View(instructor);
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
            var emailParam = new SqlParameter("@Email", email);
            var passwordParam = new SqlParameter("@Password", password);
            var isInstructorParam = new SqlParameter("@IsInstructor", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC Is_Instructor @Email, @Password, @IsInstructor OUTPUT",
                emailParam, passwordParam, isInstructorParam);

            if ((bool)isInstructorParam.Value)
            {
                var instructor = await _context.Instructor.FirstOrDefaultAsync(i => i.Email == email);
                if (instructor != null)
                {
                    // Create authentication claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,
                            instructor.InstructorID.ToString()), // Store InstructorID in the Name claim
                        new Claim(ClaimTypes.Email, instructor.Email) // Optional: store email
                    };

                    // Create the identity and principal
                    var claimsIdentity = new ClaimsIdentity(claims, "InstructorScheme");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user
                    await HttpContext.SignInAsync("InstructorScheme", claimsPrincipal);

                    return RedirectToAction("Dashboard", new { id = instructor.InstructorID });
                }
            }

            ModelState.AddModelError("", "Invalid instructor credentials.");
            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    var instructor = await _context.Instructor.FindAsync(id);
                    if (instructor != null)
                    {
                        instructor.ImageData = memoryStream.ToArray();
                        _context.Update(instructor);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction("Dashboard", new { id });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> UnsetImage(int id)
        {
            var instructor = await _context.Instructor.FindAsync(id);
            if (instructor != null)
            {
                instructor.ImageData = null;
                _context.Update(instructor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", new { id });
        }

        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> Dashboard(int id)
        {
            var instructor = await _context.Instructor.FindAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View("InstructorDashboard", instructor);
        }

        // GET: View Instructor Profile
        [HttpGet]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> ViewProfile()
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToAction("Login", "Instructor"); // Redirect unauthenticated users
            }

            try
            {
                var instructorId = int.Parse(User.Identity.Name); // Retrieve InstructorID from the Name claim
                var instructor = await _context.Instructor.FirstOrDefaultAsync(i => i.InstructorID == instructorId);

                if (instructor == null)
                {
                    return NotFound("Instructor profile not found.");
                }

                return View(instructor); // Pass the instructor model to the view
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        // GET: Edit Instructor Profile
        [HttpGet]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> EditProfileDetails()
        {
            var instructorId = int.Parse(User.Identity.Name); // Retrieve InstructorID from the authenticated user
            var instructor = await _context.Instructor.FirstOrDefaultAsync(i => i.InstructorID == instructorId);

            if (instructor == null)
            {
                return NotFound("Instructor profile not found.");
            }

            return View(instructor); // Display the EditProfileDetails.cshtml view
        }

        // POST: Save Edited Profile Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> EditProfileDetails(Instructor model)
        {
            var instructorId = int.Parse(User.Identity.Name);
            var instructor = await _context.Instructor.FirstOrDefaultAsync(i => i.InstructorID == instructorId);

            if (instructor == null)
            {
                return NotFound("Instructor profile not found.");
            }

            // Update instructor properties
            instructor.FirstName = model.FirstName;
            instructor.LastName = model.LastName;
            instructor.LatestQualification = model.LatestQualification;
            instructor.ExpertiseArea = model.ExpertiseArea;

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewProfile"); // Redirect back to the profile view
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public IActionResult CreateLearningGoal()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> CreateLearningGoal(LearningGoal model)
        {
            if (ModelState.IsValid)
            {
                // Prepare parameters for the stored procedure
                var idParam = new SqlParameter("@GoalID", model.ID);
                var statusParam = new SqlParameter("@Status", model.Status ?? (object)DBNull.Value);
                var deadlineParam = new SqlParameter("@Deadline", model.Deadline);
                var descriptionParam = new SqlParameter("@Description", model.Description ?? (object)DBNull.Value);

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC NewGoal @GoalID, @Status, @Deadline, @Description",
                        idParam, statusParam, deadlineParam, descriptionParam
                    );

                    return RedirectToAction("ViewGoals"); // Redirect to the ViewGoals action
                }
                catch (Exception ex)
                {
                    // Add the exception message to the model state
                    ModelState.AddModelError("", $"An error occurred while creating the goal: {ex.Message}");
                    return View();
                }
            }

            // Add the exception message to the model state
            ModelState.AddModelError("", "check the goal id");
            return View();




        }


        [HttpGet]
        public async Task<IActionResult> ViewGoals()
        {
            var goals = await _context.LearningGoals.ToListAsync();
            return View(goals); // Pass the list of goals to the view
        }

        // GET: Edit a goal
        [HttpGet]
        public async Task<IActionResult> EditGoal(int id)
        {
            var goal = await _context.LearningGoals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

// POST: Save edits
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGoal(LearningGoal model)
        {
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewGoals");
            }

            return View(model);
        }

// GET: Delete confirmation page
        [HttpGet]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var goal = await _context.LearningGoals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

// POST: Delete the goal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGoalConfirmed(int id)
        {
            var goal = await _context.LearningGoals.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }

            _context.LearningGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewGoals");
        }

        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> Dashboard2()
        {
            var instructorId = int.Parse(User.Identity.Name);
            return RedirectToAction("Dashboard", new { id = instructorId });
        }

        // GET: View all learning paths

        // GET: Create learning path
        [HttpGet]
        public IActionResult CreateLearningPath()
        {
            return View(); // Corresponds to CreateLearningPath.cshtml
        }

        // POST: Create learning path
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLearningPath(LearningPath model)
        {

            try
            {
                // Check if the personalization profile exists
                var personalizationProfileExists = await _context.PersonalizationProfiles
                    .AnyAsync(pp => pp.LearnerID == model.LearnerID && pp.ProfileID == model.ProfileID);

                if (!personalizationProfileExists)
                {
                    ModelState.AddModelError("",
                        "A personalization profile with the specified Learner ID and Profile ID does not exist.");
                    return View(model);
                }

                // Check for duplicate learning path
                var existingPath = await _context.LearningPath
                    .FirstOrDefaultAsync(lp => lp.LearnerID == model.LearnerID && lp.ProfileID == model.ProfileID);



                // Validate required fields
                if (string.IsNullOrEmpty(model.CompletionStatus))
                {
                    ModelState.AddModelError("CompletionStatus", "Completion status is required.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.CustomContent))
                {
                    ModelState.AddModelError("CustomContent", "Custom content is required.");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.AdaptiveRules))
                {
                    ModelState.AddModelError("AdaptiveRules", "Adaptive rules are required.");
                    return View(model);
                }

                // Add the new learning path
                var newPath = new LearningPath
                {
                    LearnerID = model.LearnerID,
                    ProfileID = model.ProfileID,
                    CompletionStatus = model.CompletionStatus,
                    CustomContent = model.CustomContent,
                    AdaptiveRules = model.AdaptiveRules
                };

                _context.LearningPath.Add(newPath);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Learning path created successfully.";
                return RedirectToAction("ViewLearningPaths");
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }


            // If we reach this point, redisplay the form with validation errors
            return View(model);
        }


        // GET: Edit learning path
        [HttpGet]
        public async Task<IActionResult> EditLearningPath(int id)
        {
            var path = await _context.LearningPath.FindAsync(id);
            if (path == null)
            {
                return NotFound();
            }

            return View(path);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearningPath(int id, LearningPath model)
        {
            if (id != model.PathID)
            {
                return BadRequest();
            }


            var path = await _context.LearningPath.FindAsync(id);
            if (path == null)
            {
                return NotFound();
            }

            // Update the path properties
            path.CompletionStatus = model.CompletionStatus;
            path.CustomContent = model.CustomContent;
            path.AdaptiveRules = model.AdaptiveRules;

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewLearningPaths");
        }



        // POST: Delete learning path directly
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLearningPath(int id)
        {
            var path = await _context.LearningPath.FindAsync(id);

            if (path == null)
            {
                return NotFound("Learning path not found.");
            }

            // Remove the learning path
            _context.LearningPath.Remove(path);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewLearningPaths");
        }



        [HttpGet]
        public async Task<IActionResult> ViewLearningPaths()
        {
            var paths = await _context.LearningPath
                .Select(path => new LearningPath
                {
                    PathID = path.PathID,
                    LearnerID = path.LearnerID,
                    ProfileID = path.ProfileID,
                    CompletionStatus = path.CompletionStatus ?? "N/A",
                    CustomContent = path.CustomContent ?? "N/A",
                    AdaptiveRules = path.AdaptiveRules ?? "N/A"
                }).ToListAsync();

            return View(paths);
        }

        [HttpGet]
        public async Task<IActionResult> ViewForums()
        {
            var forums = await _context.DiscussionForum.ToListAsync();
            return View(forums);
        }

        [HttpGet]
        public IActionResult CreateForum()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForum(DiscussionForum model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Timestamp = DateTime.Now;
                    model.LastActive = DateTime.Now;

                    _context.DiscussionForum.Add(model);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("ViewForums");
                }
            }
            catch (SqlException ex)
            {

                ModelState.AddModelError("", $"check the course and model id : {ex.Message}");

                return View();
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", $"check the course and model id : {ex.Message}");

                return View();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditForum(int id)
        {
            var forum = await _context.DiscussionForum.FindAsync(id);
            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForum(int id, DiscussionForum model)
        {
            if (id != model.ForumID)
            {
                return BadRequest();
            }

            var forum = await _context.DiscussionForum.FindAsync(id);
            if (forum == null)
            {
                return NotFound();
            }

            forum.Title = model.Title;
            forum.Description = model.Description;
            forum.LastActive = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewForums");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForum(int id)
        {
            var forum = await _context.DiscussionForum.FindAsync(id);
            if (forum == null)
            {
                return NotFound();
            }

            _context.DiscussionForum.Remove(forum);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewForums");
        }






        //////////

        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Course.ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Modules(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var modules = await _context.Modules
                .Where(m => m.CourseID == id)
                .ToListAsync();
            if (modules == null)
            {
                return NotFound();
            }

            return View(modules);
        }

        //
        // public async Task<IActionResult> Activities(int? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     var activities = await _context.Learning_activities
        //         .Where(a => a.ModuleID == id)
        //         .ToListAsync();
        //     if (activities == null)
        //     {
        //         return NotFound();
        //     }
        //
        //     return View(activities);
        // }
        public async Task<IActionResult> Activities(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activities = await _context.Learning_activities
                .Where(a => a.ModuleID == id)
                .ToListAsync();

            if (activities == null)
            {
                return NotFound();
            }

            var module = await _context.Modules.FirstOrDefaultAsync(m => m.ModuleID == id);
            if (module != null)
            {
                ViewBag.CourseID = module.CourseID;
            }

            return View(activities);
        }

        [HttpGet]
        public IActionResult AddActivity(int courseId, int moduleId)
        {
            ViewBag.CourseID = courseId;
            ViewBag.ModuleID = moduleId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddActivity(int courseId, int moduleId, string activityType,
            string instructionDetails, int maxPoints)
        {
            var courseParam = new SqlParameter("@CourseID", courseId);
            var moduleParam = new SqlParameter("@ModuleID", moduleId);
            var activityTypeParam = new SqlParameter("@ActivityType", activityType);
            var instructionDetailsParam = new SqlParameter("@InstructionDetails", instructionDetails);
            var maxPointsParam = new SqlParameter("@MaxPoints", maxPoints);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC NewActivity @CourseID, @ModuleID, @ActivityType, @InstructionDetails, @MaxPoints",
                courseParam, moduleParam, activityTypeParam, instructionDetailsParam, maxPointsParam);

            return RedirectToAction("Modules", new { id = courseId });
        }

        // GET: View all courses
        [HttpGet]
        public async Task<IActionResult> ViewCourses()
        {
            var courses = await _context.Course.ToListAsync();
            return View(courses);
        }

        // GET: Create a new course
        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        // POST: Create a new course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Course.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewCourses");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Edit a course
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Save course edits
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course model)
        {
            if (id != model.CourseID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var course = await _context.Course.FindAsync(id);
                    if (course == null)
                    {
                        return NotFound();
                    }

                    course.Title = model.Title;
                    course.LearningObjective = model.LearningObjective;
                    course.CreditPoints = model.CreditPoints;
                    course.DifficultyLevel = model.DifficultyLevel;
                    course.Description = model.Description;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("ViewCourses");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // POST: Delete a course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Check if there are enrolled learners
            var enrollmentsExist = await _context.Course_enrollment
                .AnyAsync(e => e.CourseID == id);
            if (enrollmentsExist)
            {
                ModelState.AddModelError("", "Cannot delete the course as learners are enrolled.");
                return RedirectToAction("ViewCourses");
            }

            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewCourses");
        }

public async Task<IActionResult> ViewAssessments()
{
    try
    {
        var assessments = await _context.Assessments.ToListAsync();

        if (assessments == null || !assessments.Any())
        {
            TempData["Message"] = "No assessments found.";
            return RedirectToAction("Dashboard2");
        }

        return View("ViewAssessments", assessments);
    }
    catch (Exception ex)
    {
        TempData["Message"] = $"An error occurred: {ex.Message}";
        return RedirectToAction("Dashboard2");
    }
}


        [HttpGet]
        public async Task<IActionResult> EditScore(int learnerId, int assessmentId)
        {
            var takenAssessment = await _context.TakenAssessments
                .Include(ta => ta.Assessment)
                .FirstOrDefaultAsync(ta => ta.LearnerID == learnerId && ta.AssessmentID == assessmentId);

            if (takenAssessment == null)
            {
                return NotFound("The taken assessment record could not be found.");
            }

            return View(takenAssessment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditScore(int learnerId, int assessmentId, int scoredPoints)
        {
            var takenAssessment = await _context.TakenAssessments
                .FirstOrDefaultAsync(ta => ta.LearnerID == learnerId && ta.AssessmentID == assessmentId);

            if (takenAssessment == null)
            {
                return NotFound("The taken assessment record could not be found.");
            }

            takenAssessment.ScoredPoints = scoredPoints;

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewAssessments");
        }



        [HttpGet]
        public async Task<IActionResult> Breakdown(int id)
        {
            var breakdown = await _context.TakenAssessments
                .Include(ta => ta.Assessment)
                .Where(ta => ta.AssessmentID == id)
                .Select(ta => new
                {
                    ta.LearnerID,
                    ta.AssessmentID,
                    ta.ScoredPoints,
                    TotalMarks = ta.Assessment.TotalMarks,
                    Percentage = (ta.ScoredPoints / (float)ta.Assessment.TotalMarks) * 100
                })
                .ToListAsync();

            // Calculate the average score
            var averageScore = breakdown.Any()
                ? breakdown.Average(b => b.ScoredPoints)
                : 0;

            ViewBag.AverageScore = averageScore; // Pass the average score to the view

            return View(breakdown);
        }



        [HttpGet]
        public IActionResult CreateAssessment()
        {
            return View(); // Corresponding view: CreateAssessment.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssessment(Assessments model)
        {
            try

            {
                _context.Assessments.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewAssessments");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(model); // Re-display the form if validation fails
        }

        [HttpGet]
        public IActionResult AddAssessment(int courseId, int moduleId)
        {
            var assessment = new Assessments
            {
                CourseID = courseId,
                ModuleID = moduleId
            };

            ViewBag.CourseID = courseId;
            return View("AddAssessment", assessment); // Use the updated view name
        }

        [HttpGet]
        public async Task<IActionResult> ViewModuleAssessments(int moduleId, int courseId)
        {
            var assessments = await _context.Assessments
                .Where(a => a.ModuleID == moduleId && a.CourseID == courseId)
                .ToListAsync();

            if (!assessments.Any())
            {
                TempData["Message"] = "No assessments found for this module.";
                return RedirectToAction("Modules", new { id = courseId });
            }

            ViewBag.ModuleID = moduleId;
            ViewBag.CourseID = courseId;

            return View("ViewModuleAssessments", assessments); // New view for module-specific assessments
        }

        [HttpGet]
        public async Task<IActionResult> HighestScoreAssessments(int courseId)
        {
            var highestScoreAssessments = await _context.Assessments
                .Where(a => a.CourseID == courseId)
                .GroupBy(a => a.CourseID)
                .Select(g => g.OrderByDescending(a => a.TotalMarks).FirstOrDefault())
                .ToListAsync();

            if (!highestScoreAssessments.Any())
            {
                TempData["Message"] = "No assessments found for this course.";
                return RedirectToAction("ViewCourses");
            }

            ViewBag.CourseID = courseId;
            return View("HighestScoreAssessments", highestScoreAssessments); // New view for highest-score assessments
        }

        [HttpGet]
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

            // Pass the forum details and posts to the view
            ViewBag.ForumId = forumId;
            return View(posts);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> PostAsInstructor(int forumId, string postContent)
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
            {
                return Unauthorized("You need to be logged in as an instructor to post.");
            }

            if (!int.TryParse(User.Identity.Name, out int instructorId))
            {
                return BadRequest("Invalid Instructor ID.");
            }

            if (string.IsNullOrWhiteSpace(postContent))
            {
                TempData["Message"] = "Post content cannot be empty.";
                return RedirectToAction("ViewPosts", new { forumId });
            }

            var post = new InstructorDiscussion
            {
                ForumID = forumId,
                InstructorID = instructorId,
                Post = postContent,
                Time = DateTime.Now
            };

            _context.InstructorDiscussion.Add(post);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Post added successfully!";
            return RedirectToAction("ViewPosts", new { forumId });
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> SingleCourse(int id)
        {
            if (!User.Identity.IsAuthenticated || string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToAction("Login", "Instructor");
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                TempData["Message"] = "Course not found.";
                return RedirectToAction("ViewCourses");
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> ShowLearners()
        {
            var learners = await _context.Learner.ToListAsync();
            return View(learners);
        }

        [HttpGet]
        public async Task<IActionResult> SearchLearnerById(int learnerId)
        {
            var learner = await _context.Learner
                .FirstOrDefaultAsync(l => l.LearnerID == learnerId);

            if (learner == null)
            {
                TempData["Message"] = "Learner not found.";
                return RedirectToAction("ShowLearners");
            }

            var learners = new List<Learner> { learner };
            return View("ShowLearners", learners);
        }

        [HttpGet]
        public async Task<IActionResult> CompletedCourses(int learnerId)
        {
            var completedCourses = await _context.Course_enrollment
                .Where(ce => ce.LearnerID == learnerId && ce.Status == "Completed")
                .Include(ce => ce.Course)
                .Select(ce => ce.Course)
                .ToListAsync();

            ViewBag.LearnerID = learnerId;
            return View("CompletedCourses", completedCourses);
        }

        public IActionResult ConfirmAccountDeletion()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("ChooseRole", "Account");
            }

            return View();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public IActionResult SendNotification1(int learnerId)
        {
            var model = new NotificationViewModel
            {
                LearnerID = learnerId
            };
          return View(model);
  
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "InstructorScheme")]
        public async Task<IActionResult> SendNotification(NotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with validation errors
                return View(model);
            }

            var learner = await _context.Learner.FindAsync(model.LearnerID);
            if (learner == null)
            {
                TempData["Message"] = "Learner not found.";
                return RedirectToAction("ShowLearners");
            }

            var notification = new Notification
            {
                Timestamp = DateTime.Now,
                Message = model.Message,
                UrgencyLevel = model.UrgencyLevel
            };
            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            var receivedNotification = new ReceivedNotification
            {
                NotificationID = notification.ID,
                LearnerID = model.LearnerID,
                ReadStatus = false
            };
            _context.ReceivedNotification.Add(receivedNotification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Notification sent successfully.";
            return RedirectToAction("ShowLearners");
        }
    }
}


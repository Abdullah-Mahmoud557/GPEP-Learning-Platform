using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Userstories.Data;
using Userstories.Models;

namespace Userstories.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Course.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Modules)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CourseID,Title,LearningObjective,CreditPoints,DifficultyLevel,Description")]
            Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("CourseID,Title,LearningObjective,CreditPoints,DifficultyLevel,Description")]
            Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
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

            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.CourseID == id);
        }
[HttpGet]
        public async Task<IActionResult> EnrolledCourses(int learnerId)
        {
            var learnerIdParam = new SqlParameter("@LearnerID", learnerId);
            var courses = await _context.Course
                .FromSqlRaw("EXEC EnrolledCourses @LearnerID", learnerIdParam)
                .ToListAsync();

            ViewBag.LearnerID = learnerId;
            return View("/Views/Learner/Courses.cshtml", courses);
        }

        [HttpGet]
        public async Task<IActionResult> AllTheCourses(int learnerId)
        {
            var courses = await _context.Course.ToListAsync();
            ViewBag.LearnerID = learnerId;
            return View("/Views/Learner/Courses.cshtml", courses);
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
            return View("/Views/Learner/Courses.cshtml", completedCourses);
        }

        [HttpGet]
        public async Task<IActionResult> Show(int courseId, int LearnerID)
        {
            var course = await _context.Course.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.LearnerID = LearnerID;

            return View("/Views/Learner/SingleCourse.cshtml", course);
        }
                [HttpGet]
        public async Task<IActionResult> CourseModules(int courseId, int LearnerID)
        {
            var modules = await _context.Modules
                .Where(m => m.CourseID == courseId)
                .ToListAsync();

            if (modules == null || !modules.Any())
            {
                modules = new List<Modules>();
            }
            ViewBag.LearnerID = LearnerID;
            ViewBag.CourseID = courseId;
            return View("/Views/Learner/CourseModules.cshtml",modules);
        }
      [HttpPost]
public async Task<IActionResult> Enroll(int courseId, int learnerId)
{
    var learnerIdParam = new SqlParameter("@LearnerID", learnerId);
    var courseIdParam = new SqlParameter("@CourseID", courseId);
    var notifyParam = new SqlParameter
    {
        ParameterName = "@result",
        SqlDbType = SqlDbType.Int,
        Direction = ParameterDirection.Output
    };

    await _context.Database.ExecuteSqlRawAsync("EXEC Courseregister @LearnerID, @CourseID, @result OUTPUT", learnerIdParam, courseIdParam, notifyParam);
    var courses = await _context.Course.ToListAsync();
        ViewBag.notify =  notifyParam.Value ;
        ViewBag.LearnerID = learnerId;
    return View("/Views/Learner/Courses.cshtml", courses);
}
        [HttpGet]
        public async Task<IActionResult> Prerequisites(int learnerId, int courseId)
        {
            var learnerExists = await _context.Learner.AnyAsync(l => l.LearnerID == learnerId);
            if (!learnerExists)
            {
                return Json(new { message = "No such learner exists." });
            }
            var learnerIdParam = new SqlParameter("@LearnerID", learnerId);
            var courseIdParam = new SqlParameter("@CourseID", courseId);

            var resultMessage = new SqlParameter
            {
                ParameterName = "@ResultMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = -1, // Use -1 for MAX size
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC Prerequisites @LearnerID, @CourseID, @ResultMessage OUTPUT",
                learnerIdParam, courseIdParam, resultMessage);

            var completionStatusMessage = new List<string>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC GetCourseCompletionStatus @LearnerID, @CourseID";
                command.Parameters.Add(learnerIdParam);
                command.Parameters.Add(courseIdParam);

                _context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var courseTitle = result["CourseTitle"].ToString();
                        var completionStatus = result["CompletionStatus"].ToString();
                        if (!completionStatus.Equals("completed", StringComparison.OrdinalIgnoreCase))
                        {
                            completionStatus = "Not completed ❌";
                        }
                        else
                        {
                            completionStatus = "Completed ✔️";
                        }

                        completionStatusMessage.Add($"{courseTitle}: {completionStatus}");
                    }
                }
            }

            var finalMessage = resultMessage.Value.ToString();
            if (completionStatusMessage.Any())
            {
                finalMessage += $" Completion Status:{Environment.NewLine}{string.Join(Environment.NewLine, completionStatusMessage)}";
            }

            return Json(new { message = finalMessage });
        }
        [HttpGet]
        public async Task<IActionResult> Modules(int courseId, int learnerId)
        {
            var modules = await _context.Modules
                .Where(m => m.CourseID == courseId)
                .ToListAsync();

            ViewBag.LearnerID = learnerId;
            ViewBag.CourseID = courseId;
            return View(modules);
        }
        

        [HttpGet]
        public async Task<IActionResult> ShowActivities(int? id,int courseId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activities = await _context.Learning_activities
                .Where(a => a.ModuleID == id)
                .ToListAsync();

            if (activities == null || !activities.Any())
            {
                return NotFound();
            }
              
            return View("/Views/Learner/showActivities.cshtml", activities);
        }        
        [HttpGet]
        
        public async Task<IActionResult> HighestScoreAssessments(int courseId,int learnerId)
        {
            var highestScoreAssessments = await _context.Assessments
                .Where(a => a.CourseID == courseId)
                .GroupBy(a => a.CourseID)
                .Select(g => g.OrderByDescending(a => a.TotalMarks).FirstOrDefault())
                .ToListAsync();

            if (!highestScoreAssessments.Any())
            {
                TempData["Message"] = "No assessments found for this course.";
                return RedirectToAction("AllTheCourses",learnerId );
            }
            ViewBag.LearnerID = learnerId;
            ViewBag.CourseID = courseId;
            return View("/Views/Learner/highstAssesment.cshtml", highestScoreAssessments);
        }
        [HttpGet
        ]
      public async Task<IActionResult> Details2(int courseId, int moduleId)
{
    var activities = await _context.Learning_activities
        .Where(a => a.ModuleID == moduleId && a.CourseID == courseId)
        .ToListAsync();

    ViewBag.CourseID = courseId;
    return View("/Views/Modules/Details2.cshtml", activities);
}
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Userstories.Data;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Userstories.Models;

namespace Userstories.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(Admin admin, string secretCode)
        {
            if (ModelState.IsValid)
            {
                // Check if the secret code is valid using the stored procedure
                var codeParam = new SqlParameter("@Code", secretCode);
                var isValidParam = new SqlParameter("@IsValid", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync("EXEC Is_Valid_SecretCode @Code, @IsValid OUTPUT", codeParam, isValidParam);

                // If the secret code is not valid, show an error
                if (!(bool)isValidParam.Value)
                {
                    ModelState.AddModelError("", "Invalid secret code.");
                    return View(admin);
                }

                // Check if the email already exists
                if (await _context.EmailExistsAsync(admin.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(admin);
                }

                // Add admin to the database
                _context.Admin.Add(admin);
                await _context.SaveChangesAsync();

                // Automatically log in the admin after successful signup
                return await Login(admin.Email, admin.Password);
            }

            return View(admin);
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
            var isAdminParam = new SqlParameter("@IsAdmin", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC Is_Admin @Email, @Password, @IsAdmin OUTPUT", emailParam, passwordParam, isAdminParam);

            if ((bool)isAdminParam.Value)
            {
                var admin = await _context.Admin.FirstOrDefaultAsync(a => a.Email == email);
                if (admin != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.AdminID.ToString()), // Store AdminID in Name claim
                        new Claim(ClaimTypes.Email, admin.Email) // Optional: Store email
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "AdminScheme");
                    await HttpContext.SignInAsync("AdminScheme", new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Dashboard", new { id = admin.AdminID });
                }
            }

            ModelState.AddModelError("", "Invalid admin credentials.");
            return View();
        }



        [HttpGet]
        public IActionResult UploadImage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    var admin = await _context.Admin.FindAsync(id);
                    if (admin != null)
                    {
                        admin.ImageData = memoryStream.ToArray();
                        _context.Update(admin);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Dashboard", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> UnsetImage(int id)
        {
            var admin = await _context.Admin.FindAsync(id);
            if (admin != null)
            {
                admin.ImageData = null;
                _context.Update(admin);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Dashboard", new { id });
        }

        public async Task<IActionResult> Dashboard(int id)
        {
            var admin = await _context.Admin.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            return View("AdminDashboard", admin);
        }
        
        // GET: View all personalization profiles
        [HttpGet]
        public async Task<IActionResult> ManageProfiles()
        {
            var profiles = await _context.PersonalizationProfiles
                .Include(p => p.Learner) // Include related learner data
                .ToListAsync();
            return View(profiles);
        }

        // POST: Delete a personalization profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile(int learnerId, int profileId)
        {
            var profile = await _context.PersonalizationProfiles
                .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            _context.PersonalizationProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageProfiles));
        }
        [HttpGet]
        public async Task<IActionResult> ManageInstructors()
        {
            var instructors = await _context.Instructor.ToListAsync();
            return View(instructors);
        }

        // POST: Delete an instructor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructor.FindAsync(id);

            if (instructor == null)
            {
                return NotFound("Instructor not found.");
            }

            _context.Instructor.Remove(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageInstructors));
        }
        [HttpGet]
        public async Task<IActionResult> ManageLearners()
        {
            var learners = await _context.Learner.ToListAsync();
            return View(learners);
        }

        // POST: Delete a learner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLearner(int id)
        {
            var learner = await _context.Learner.FindAsync(id);

            if (learner == null)
            {
                return NotFound("Learner not found.");
            }

            _context.Learner.Remove(learner);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageLearners));
        }
        [Authorize(AuthenticationSchemes = "AdminScheme")]
        [HttpGet]
        public async Task<IActionResult> ViewProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!int.TryParse(User.Identity.Name, out var adminId))
            {
                return BadRequest("Invalid AdminID in claims.");
            }

            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.AdminID == adminId);

            if (admin == null)
            {
                return NotFound("Admin profile not found.");
            }

            return View(admin);
        }
       

        [Authorize(AuthenticationSchemes = "AdminScheme")]
        [HttpGet]
        public async Task<IActionResult> EditProfileDetails()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                return Unauthorized("User is not authenticated.");
            }

            if (!int.TryParse(User.Identity.Name, out var adminId))
            {
                return BadRequest("Invalid AdminID in claims.");
            }

            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.AdminID == adminId);

            if (admin == null)
            {
                return NotFound("Admin profile not found.");
            }

            return View(admin);
        }

        
        [Authorize(AuthenticationSchemes = "AdminScheme")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileDetails(Admin model, IFormFile ImageFile)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Admin");
            }

            if (!int.TryParse(User.Identity.Name, out var adminId))
            {
                return BadRequest("Invalid AdminID in claims.");
            }

            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.AdminID == adminId);

            if (admin == null)
            {
                return NotFound("Admin profile not found.");
            }

            // Update admin properties
            admin.Email = model.Email;
            admin.Password = model.Password;

            // Check if a new image file is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ImageFile.CopyToAsync(memoryStream);
                    admin.ImageData = memoryStream.ToArray();
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewProfile"); // Redirect back to the profile view
        }
        [Authorize(AuthenticationSchemes = "AdminScheme")]
        public async Task<IActionResult> Dashboard2()
        {
            var iD = int.Parse(User.Identity.Name);
            return RedirectToAction("Dashboard", new { id = iD });
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
            }catch (SqlException ex)
            {
                
                ModelState.AddModelError("", $"check the course and model id : {ex.Message}");
               
                return View();
            }catch (Exception ex)
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



    }
}

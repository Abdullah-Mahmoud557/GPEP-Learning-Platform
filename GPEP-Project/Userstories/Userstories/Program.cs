using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Userstories.Data;
using Userstories.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Generic default scheme
})
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
})
.AddCookie("LearnerScheme", options =>
{
    options.LoginPath = "/Learner/Login";
    options.AccessDeniedPath = "/Learner/AccessDenied";
})
.AddCookie("InstructorScheme", options =>
{
    options.LoginPath = "/Instructor/Login";
    options.AccessDeniedPath = "/Instructor/AccessDenied";
});

// Add authorization services (needed for [Authorize] attributes)
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Apply default authentication scheme based on controller route
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;

    if (!string.IsNullOrEmpty(path))
    {
        if (path.StartsWith("/Learner"))
        {
            context.Items["AuthenticationScheme"] = "LearnerScheme";
        }
        else if (path.StartsWith("/Admin"))
        {
            context.Items["AuthenticationScheme"] = "AdminScheme";
        }
        else if (path.StartsWith("/Instructor"))
        {
            context.Items["AuthenticationScheme"] = "InstructorScheme";
        }
    }

    await next();
});

// Redirect root URL to the welcome page
app.MapGet("/", context =>
{
    context.Response.Redirect("/html/welcome.html");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Instructor}/{action=Dashboard}/{id?}");

app.Run();

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Added for List<SelectListItem>
using Trackademic.Core.Interfaces;
using Trackademic.Core.Models;
using Trackademic.Data.Data;
using Trackademic.Data.Repositories;
using Trackademic.Services.Implementations;
//using Trackademic.WebApp.Data;  // Database Seeder For Testing

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<TrackademicDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Authentication (Cookie Configuration)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Error";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// Authorization Services (Define the Rules)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireTeacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("RequireStudent", policy => policy.RequireRole("Student"));
});

// Simple Razor Pages Registration (Avoids Auth Errors)
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Set Login as Homepage
        options.Conventions.AddPageRoute("/Account/Login", "");

        // SECURE THE FOLDERS
        options.Conventions.AuthorizeFolder("/Admin", "RequireAdmin");
        options.Conventions.AuthorizeFolder("/Teachers", "RequireTeacher");
        options.Conventions.AuthorizeFolder("/Student", "RequireStudent");

        // ALLOW ANONYMOUS ACCESS
        options.Conventions.AllowAnonymousToPage("/Account/Login");
        options.Conventions.AllowAnonymousToPage("/Account/ForgotPassword");
        options.Conventions.AllowAnonymousToPage("/Error");
    });

// Register All Application Services
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();


// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Added Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed the database (for testing purposes)
/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TrackademicDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// The order is critical: Routing -> Session -> Authentication -> Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
//app.MapFallbackToPage("/Account/Login"); //it might override specific 404s

app.Run();
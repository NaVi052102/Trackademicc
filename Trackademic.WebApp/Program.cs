using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Trackademic.Core.Interfaces;
using Trackademic.Data.Data;
using Trackademic.Data.Repositories;
using Trackademic.Services.Implementations;
using Trackademic.Core.Models;
using System.Collections.Generic; // Added for List<SelectListItem>

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<TrackademicDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 1. Add Authentication (Cookie Configuration) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Error";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

// --- 2. Simple Razor Pages Registration (Avoids Auth Errors) ---
builder.Services.AddRazorPages();

// --- 3. Register All Application Services ---
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
// Add other repository registrations here
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); 
// ...etc...

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGradeService, GradeService>(); // FIX for Grades View
// ...etc...

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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
app.MapFallbackToPage("/Index");

app.Run();
using Microsoft.EntityFrameworkCore;
using Trackademic.Core.Interfaces;
using Trackademic.Core.Models;
using Trackademic.Data.Data; // Assuming your DbContext is here
using System.Threading.Tasks;

namespace Trackademic.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly TrackademicDbContext _context;

        // Constructor for Dependency Injection
        public AuthService(TrackademicDbContext context)
        {
            _context = context;
        }

        // Implements the LoginAsync method from IAuthService
        // Returns Task<User?> to allow for a null return if login fails.
        public async Task<User?> LoginAsync(string username, string password, string userType)
        {
            // 1. Find the user by their Username AND ensure the role matches the button pressed.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.UserType == userType);

            if (user == null)
            {
                // User not found or the provided role was incorrect
                return null;
            }

            // 2. Check the password
            // IMPORTANT: This is a placeholder for a secure check. 
            // In production, you must use a library like BCrypt.Net to hash and verify the password_hash.
            if (user.PasswordHash == password)
            {
                // Password is correct
                return user;
            }

            // Password was wrong
            return null;
        }
    }
}
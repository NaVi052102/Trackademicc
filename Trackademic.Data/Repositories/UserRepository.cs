using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trackademic.Core.Interfaces;
using Trackademic.Core.Models;
using Trackademic.Data.Data;

namespace Trackademic.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TrackademicDbContext _context;

        public UserRepository(TrackademicDbContext context)
        {
            _context = context;
        }

        // This is the implementation of the method from IUserRepository
        public async Task<IEnumerable<Trackademic.Core.Models.User>> GetUsers()
        {
            // Use .Select() to convert Data.Models.User -> Core.Models.User
            return await _context.Users
                .Select(u => new Trackademic.Core.Models.User
                {
                    Id = u.Id,
                    Username = u.Username,
                    PasswordHash = u.PasswordHash,
                    UserType = u.UserType
                    // Add any other properties here that exist in both classes
                })
                .ToListAsync();
        }

        // Add other required methods here...
        // public async Task<User> GetUserById(long id) { ... }
        // public async Task<User> GetUserByUsername(string username) { ... }
        // public void AddUser(User user) { ... }
    }
}
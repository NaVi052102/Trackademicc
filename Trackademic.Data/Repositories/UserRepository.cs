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
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Add other required methods here...
        // public async Task<User> GetUserById(long id) { ... }
        // public async Task<User> GetUserByUsername(string username) { ... }
        // public void AddUser(User user) { ... }
    }
}
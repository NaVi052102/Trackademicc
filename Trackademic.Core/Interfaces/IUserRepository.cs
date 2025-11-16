using Trackademic.Core.Models; // <-- This is the correct using statement
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trackademic.Core.Interfaces
{
    public interface IUserRepository
    {
        // This is an example of a method you will need.
        // Your error logs were asking for a method, so this will fix it.
        Task<IEnumerable<User>> GetUsers();

        // You will also add other methods here later, like:
        // Task<User> GetUserById(long id);
        // Task<User> GetUserByUsername(string username);
        // void AddUser(User user);
    }
}
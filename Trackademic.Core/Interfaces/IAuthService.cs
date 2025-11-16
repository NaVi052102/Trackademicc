using Trackademic.Core.Models;
using System.Threading.Tasks;

namespace Trackademic.Core.Interfaces
{
    public interface IAuthService
    {
        // This now correctly accepts a string
        Task<User?> LoginAsync(string username, string password, string userType);
    }
}
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trackademic.Core.Interfaces;
using Trackademic.Core.Models;
using Trackademic.Data.Data;

namespace Trackademic.Data.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly TrackademicDbContext _context;

        public ClassRepository(TrackademicDbContext context)
        {
            _context = context;
        }

        // Add methods from IClassRepository here later...
        // public async Task<Class> GetClassById(long id) { ... }
    }
}
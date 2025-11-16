using Trackademic.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trackademic.Core.Interfaces
{
    public interface IGradeService
    {
        // This method is required by your OnGet() logic
        List<Grade> GetGradesByStudentAndSemester(int studentId, string semester, string schoolYear);
    }
}
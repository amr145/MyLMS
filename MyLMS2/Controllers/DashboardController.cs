using Microsoft.AspNetCore.Mvc;
using MyLMS2.Data;
using MyLMS2.ViewModels;
using Microsoft.AspNetCore.Identity;
using MyLMS2.Models;
using System.Linq;
using System.Threading.Tasks;   // مهم عشان async/await

namespace MyLMS2.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // جلب عدد الطلاب والإنستراكتورز بشكل Async
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");

            var studentsCount = students.Count;
            var instructorsCount = instructors.Count;

            // جلب عدد الكورسات
            var coursesCount = _context.Courses.Count();

            var viewModel = new DashboardViewModel
            {
                StudentsCount = studentsCount,
                InstructorsCount = instructorsCount,
                CoursesCount = coursesCount
            };

            return View(viewModel);
        }
    }
}

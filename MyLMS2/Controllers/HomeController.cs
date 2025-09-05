using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyLMS2.Models;
using MyLMS2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyLMS2.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MyLMS2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Dashboard");

                if (await _userManager.IsInRoleAsync(user, "Instructor"))
                    return RedirectToAction("InstructorHome", "Home");

                if (await _userManager.IsInRoleAsync(user, "Student"))
                    return RedirectToAction("StudentHome", "Home");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ========================= Instructor Home =========================
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorHome()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            
            var coursesCount = await _context.Courses
                                    .Where(c => c.InstructorId == user.Id)
                                    .CountAsync();

           
            var latestCourses = await _context.Courses
                                    .Where(c => c.InstructorId == user.Id)
                                    .OrderByDescending(c => c.Id) 
                                    .Take(3)
                                    .ToListAsync();

            var viewModel = new InstructorHomeViewModel
            {
                CoursesCount = coursesCount,
                LatestCourses = latestCourses
            };

            return View(viewModel);
        }

        // ========================= Student Home =========================
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentHome()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index");

            
            var enrolledCoursesCount = await _context.Enrollments
                                        .Where(e => e.StudentId == user.Id)
                                        .CountAsync();

            
            var latestCourses = await _context.Enrollments
                                        .Where(e => e.StudentId == user.Id)
                                        .Include(e => e.Course)
                                        .OrderByDescending(e => e.Id) 
                                        .Take(3)
                                        .Select(e => e.Course)
                                        .ToListAsync();

            var viewModel = new StudentHomeViewModel
            {
                EnrolledCoursesCount = enrolledCoursesCount,
                LatestCourses = latestCourses
            };

            return View(viewModel);
        }
    }
}

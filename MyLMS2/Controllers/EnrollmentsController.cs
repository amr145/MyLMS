using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;

namespace MyLMS2.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments => يوجه حسب Role
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminIndex");

            if (User.IsInRole("Instructor"))
                return RedirectToAction("InstructorIndex");

            if (User.IsInRole("Student"))
                return RedirectToAction("StudentIndex");

            return RedirectToAction("AdminIndex"); // fallback
        }

        // 👨‍💼 Admin: يشوف كل الكورسات + الدكاترة + الطلاب
        public async Task<IActionResult> AdminIndex()
        {
            var data = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Include(e => e.Student)
                .ToListAsync();

            return View(data);
        }

        // 👨‍🏫 Instructor: يشوف الكورسات اللي هو بيدرسها + الطلاب المسجلين فيها
        public async Task<IActionResult> InstructorIndex()
        {
            var userId = User.Identity.Name; // أو لو عندك UserId استخدمه

            var data = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Include(e => e.Student)
                .Where(e => e.Course.Instructor.UserName == userId)
                .ToListAsync();

            return View(data);
        }

        // 🎓 Student: يشوف المواد اللي واخدها + اسم الدكتور بتاعها
        public async Task<IActionResult> StudentIndex()
        {
            var userId = User.Identity.Name; // أو لو عندك UserId استخدمه

            var data = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Include(e => e.Student)
                .Where(e => e.Student.UserName == userId)
                .ToListAsync();

            return View(data);
        }
    }
}

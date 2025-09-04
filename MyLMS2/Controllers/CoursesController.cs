using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MyLMS2.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;


        public CoursesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Courses
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                
                var allCourses = _context.Courses.Include(c => c.Instructor);
                return View(await allCourses.ToListAsync());
            }
            else if (User.IsInRole("Student"))
            {
                var userId = _userManager.GetUserId(User);

                var studentCourses = _context.Enrollments
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)                
                    .ThenInclude(c => c.Instructor)    
                .Select(e => e.Course);               


                return View(await studentCourses.ToListAsync());
            }
            else if (User.IsInRole("Instructor"))
            {
                var userId = _userManager.GetUserId(User);

                var instructorCourses = _context.Courses
                    .Where(c => c.InstructorId == userId);

                return View(await instructorCourses.ToListAsync());
            }

            return View(new List<Course>());
        }



        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id == null) return NotFound();


            var course = await _context.Courses

             .Include(c => c.Instructor)
             .Include(c => c.Enrollments)
             //.ThenInclude(e => e.Student)   
             .FirstOrDefaultAsync(m => m.Id == id);




            if (course == null) return NotFound();

          


            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {

           
            var instructors = (from user in _context.Users
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               where role.Name == "Instructor"
                               select user).ToList();

            ViewData["InstructorId"] = new SelectList(instructors, "Id", "UserName");


            return View();
        }

        // POST: Courses/Create


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,InstructorId")] Course course)
        {
 
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

        }


        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {


            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // instructors فقط
            var instructors = (from user in _context.Users
                               join ur in _context.UserRoles on user.Id equals ur.UserId
                               join r in _context.Roles on ur.RoleId equals r.Id
                               where r.Name == "Instructor"
                               select user).ToList();

            ViewData["InstructorId"] = new SelectList(instructors, "Id", "UserName", course.InstructorId);

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,InstructorId")] Course course)
        {
            if (id != course.Id) return NotFound();

            
            
                var existingCourse = await _context.Courses.FindAsync(id);
                if (existingCourse == null) return NotFound();


                existingCourse.Title = course.Title;
                existingCourse.Description = course.Description;
                existingCourse.InstructorId = course.InstructorId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            

            // 👇 لو ModelState فيه مشكلة (Validation Error)
            var instructors = (from user in _context.Users
                               join ur in _context.UserRoles on user.Id equals ur.UserId
                               join r in _context.Roles on ur.RoleId equals r.Id
                               where r.Name == "Instructor"
                               select user).ToList();

            ViewData["InstructorId"] = new SelectList(instructors, "Id", "UserName", course.InstructorId);

            return View(course);

        }



        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();


            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (course == null) return NotFound();


            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course != null)
            {
                
                _context.Enrollments.RemoveRange(course.Enrollments);

                
                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }


        // GET: Courses/Assign
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            return View(courses);
        }

        // GET: Courses/AssignStudents/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudents(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

           
            var students = (from user in _context.Users
                            join ur in _context.UserRoles on user.Id equals ur.UserId
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where r.Name == "Student"
                            select user).ToList();

            
            var selected = course.Enrollments?
                .Select(e => e.StudentId)
                .ToList() ?? new List<string>();

            ViewBag.Students = students;
            ViewBag.SelectedStudents = selected;

            return View(course);
        }

        // POST: Courses/AssignStudents/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudents(int id, string[] selectedStudents)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            selectedStudents = selectedStudents ?? new string[0];

            
            var existing = course.Enrollments.Select(e => e.StudentId).ToList();

            
            var toAdd = selectedStudents.Except(existing);
            foreach (var studentId in toAdd)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    CourseId = id,
                    StudentId = studentId
                });
            }

           
            var toRemove = existing.Except(selectedStudents);
            var enrollmentsToRemove = course.Enrollments
                .Where(e => toRemove.Contains(e.StudentId))
                .ToList();

            _context.Enrollments.RemoveRange(enrollmentsToRemove);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        




    }
}

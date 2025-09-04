using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;

namespace MyLMS2.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Courses.Include(c => c.Instructor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null) return NotFound();


            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (course == null) return NotFound();

          


            return View(course);
        }

        // GET: Courses/Create
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,InstructorId")] Course course)
        {

                
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            


        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            ViewData["InstructorId"] = new SelectList(_context.Users, "Id", "UserName", course.InstructorId);

            
            return View(course);
        }

        // POST: Courses/Edit/5


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,InstructorId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }


           
                var existingCourse = await _context.Courses.FindAsync(course.Id);
                if (existingCourse == null)
                {
                    return NotFound();
                }

                
                existingCourse.Title = course.Title;
                existingCourse.Description = course.Description;
                existingCourse.InstructorId = course.InstructorId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           
        }


        // GET: Courses/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
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
        public async Task<IActionResult> Assign()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            return View(courses);
        }

        // GET: Courses/AssignStudents/5
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

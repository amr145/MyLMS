using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLMS2.Controllers
{
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ModulesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var modules = await _context.Modules
                .Include(m => m.Course)
                .Where(m => m.Course.InstructorId == userId)
                .ToListAsync();

            return View(modules);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CourseMaterials(int courseId)
        {
            var modules = await _context.Modules
                .Include(m => m.Course)
                .Where(m => m.CourseId == courseId)
                .ToListAsync();

            if (!modules.Any())
            {
                TempData["ErrorMessage"] = "No materials available for this course.";
                return RedirectToAction("Index", "Courses");
            }

            return View(modules);
        }

        public async Task<IActionResult> Details(int id)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            return View(module);
        }


        [Authorize(Roles = "Instructor")]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);

            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => c.InstructorId == userId),
                "Id", "Title"
            );

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(Module module, IFormFile? pdfFile, IFormFile? wordFile, IFormFile? pptFile, IFormFile? audioFile)
        {
            
            ModelState.Remove("pdfFile");
            ModelState.Remove("wordFile");
            ModelState.Remove("pptFile");
            ModelState.Remove("audioFile");
            ModelState.Remove("videoFile");
            ModelState.Remove("Course"); 

            if (!ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                ViewData["CourseId"] = new SelectList(
                    _context.Courses.Where(c => c.InstructorId == userId),
                    "Id", "Title", module.CourseId
                );
                return View(module);
            }

            module.PdfPath = SaveFiles(pdfFile, "pdfs");
            module.WordPath = SaveFiles(wordFile, "words");
            module.PptPath = SaveFiles(pptFile, "ppts");
            module.AudioPath = SaveFiles(audioFile, "audios");

            _context.Add(module);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Module created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => c.InstructorId == userId),
                "Id", "Title", module.CourseId
            );

            return View(module);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id,Module module,IFormFile? pdfFile,IFormFile? wordFile,IFormFile? pptFile,IFormFile? audioFile,IFormFile? videoFile)
        {
            if (id != module.Id) return NotFound();

            ModelState.Remove("pdfFile");
            ModelState.Remove("wordFile");
            ModelState.Remove("pptFile");
            ModelState.Remove("audioFile");
            ModelState.Remove("videoFile");
            ModelState.Remove("Course");

            if (!ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                ViewData["CourseId"] = new SelectList(
                    _context.Courses.Where(c => c.InstructorId == userId),
                    "Id", "Title", module.CourseId
                );
                return View(module);
            }

            var existing = await _context.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (existing is null) return NotFound();

            module.PdfPath = pdfFile != null ? SaveFiles(pdfFile, "pdfs") : existing.PdfPath;
            module.WordPath = wordFile != null ? SaveFiles(wordFile, "words") : existing.WordPath;
            module.PptPath = pptFile != null ? SaveFiles(pptFile, "ppts") : existing.PptPath;
            module.AudioPath = audioFile != null ? SaveFiles(audioFile, "audios") : existing.AudioPath;
            module.VideoPath = videoFile != null ? SaveFiles(videoFile, "videos") : existing.VideoPath;

            _context.Update(module);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Module updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            return View(module);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module != null)
            {
                DeleteFiles(module);
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyMaterials(int? courseId)
        {
            var userId = _userManager.GetUserId(User);

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.StudentId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var query = _context.Modules
                .Include(m => m.Course)
                .Where(m => enrolledCourses.Contains(m.CourseId));

            if (courseId.HasValue)
            {
                query = query.Where(m => m.CourseId == courseId.Value);
            }

            var modules = await query.ToListAsync();

            return View(modules);
        }

        private string SaveFiles(IFormFile file, string folderName)
        {
            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);

                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return $"/uploads/{folderName}/{file.FileName}";
            }
            return null;
        }

        private void DeleteFiles(Module module)
        {
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            void deleteFile(string filePath)
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    var fullPath = Path.Combine(webRootPath, filePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
            }

            deleteFile(module.PdfPath);
            deleteFile(module.WordPath);
            deleteFile(module.PptPath);
            deleteFile(module.AudioPath);
            deleteFile(module.VideoPath);
        }
    }
}

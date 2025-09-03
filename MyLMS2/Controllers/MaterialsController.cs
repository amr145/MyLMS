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
    public class MaterialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MaterialsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var materials = await _context.Materials
                .Include(m => m.Course)
                .Where(m => m.Course.InstructorId == userId) 
                .ToListAsync();

            return View(materials);
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
        public async Task<IActionResult> Create(Material material, IFormFile pdfFile, IFormFile wordFile, IFormFile pptFile, IFormFile audioFile)
        {
            if (ModelState.IsValid)
            {
                material.PdfPath = SaveFiles(pdfFile, "pdfs");
                material.WordPath = SaveFiles(wordFile, "words");
                material.PptPath = SaveFiles(pptFile, "ppts");
                material.AudioPath = SaveFiles(audioFile, "audios");

                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => c.InstructorId == userId),
                "Id", "Title", material.CourseId
            );

            return View(material);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => c.InstructorId == userId),
                "Id", "Title", material.CourseId
            );

            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id, Material material, IFormFile pdfFile, IFormFile wordFile, IFormFile pptFile, IFormFile audioFile)
        {
            if (id != material.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMaterial = await _context.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                    if (existingMaterial == null) return NotFound();

                    material.PdfPath = pdfFile != null ? SaveFiles(pdfFile, "pdfs") : existingMaterial.PdfPath;
                    material.WordPath = wordFile != null ? SaveFiles(wordFile, "words") : existingMaterial.WordPath;
                    material.PptPath = pptFile != null ? SaveFiles(pptFile, "ppts") : existingMaterial.PptPath;
                    material.AudioPath = audioFile != null ? SaveFiles(audioFile, "audios") : existingMaterial.AudioPath;

                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Materials.Any(e => e.Id == material.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => c.InstructorId == userId),
                "Id", "Title", material.CourseId
            );

            return View(material);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            return View(material);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                DeleteFiles(material);
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyMaterials()
        {
            var userId = _userManager.GetUserId(User);

            var enrolledCourses = await _context.Enrollments
                .Where(e => e.StudentId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();

            var materials = await _context.Materials
                .Include(m => m.Course)
                .Where(m => enrolledCourses.Contains(m.CourseId))
                .ToListAsync();

            return View(materials);
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

        private void DeleteFiles(Material material)
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

            deleteFile(material.PdfPath);
            deleteFile(material.WordPath);
            deleteFile(material.PptPath);
            deleteFile(material.AudioPath);
            deleteFile(material.VideoPath);
        }
    }
}









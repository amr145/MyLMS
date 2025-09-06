using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;

namespace MyLMS2.Controllers
{
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Quizzes
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes.Include(q => q.Course).ToListAsync();
            return View(quizzes);
        }

        // GET: Quizzes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .Include(q => q.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quiz == null) return NotFound();
            return View(quiz);
        }

        // GET: Quizzes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CourseId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses, "Id", "Title");
            return View();
        }

        // POST: Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Duration,CourseId")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CourseId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses, "Id", "Title", quiz.CourseId);
            return View(quiz);
        }

        // POST: Quizzes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Quizzes/BuildQuiz
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> BuildQuiz(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            return View(quiz);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuildQuiz(int quizId, Quiz quizFromForm)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            if (quizFromForm.Questions != null)
            {
                foreach (var q in quizFromForm.Questions)
                {
                    var newQuestion = new Question
                    {
                        Text = q.Text,
                        QuizId = quizId,
                        AnswerOptions = new List<AnswerOption>()
                    };

                    if (q.AnswerOptions != null)
                    {
                        foreach (var opt in q.AnswerOptions)
                        {
                            if (!string.IsNullOrWhiteSpace(opt.Text))
                            {
                                newQuestion.AnswerOptions.Add(new AnswerOption
                                {
                                    Text = opt.Text,
                                    IsCorrect = opt.IsCorrect
                                });
                            }
                        }
                    }

                    _context.Questions.Add(newQuestion);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = quizId });
        }

        // GET: Quizzes/TakeQuiz
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            return View(quiz);
        }

        // POST: Quizzes/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, int> answers)
        {
            // مثال: يمكن هنا حفظ النتائج في جدول StudentScores
            // key = QuestionId, value = AnswerOptionId

            // بعد الحفظ
            return RedirectToAction("Details", new { id = quizId });
        }
    }
}

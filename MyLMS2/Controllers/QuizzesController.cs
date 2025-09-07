using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;
using System.Text.Json;

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
            IQueryable<Quiz> quizzesQuery = _context.Quizzes.Include(q => q.Course);

            if (User.IsInRole("Student"))
            {
                // جلب ID الدورات التي سجل فيها الطالب
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                var studentCourseIds = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync();

                quizzesQuery = quizzesQuery
                    .Where(q => q.CourseId.HasValue && studentCourseIds.Contains(q.CourseId.Value));
            }
            else if (User.IsInRole("Instructor"))
            {
                // جلب ID الدورات التي يدرسها المعلم
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                var instructorCourseIds = await _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                quizzesQuery = quizzesQuery
                    .Where(q => q.CourseId.HasValue && instructorCourseIds.Contains(q.CourseId.Value));
            }

            var quizzes = await quizzesQuery.ToListAsync();
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

            // جلب قائمة الطلاب من Session
            List<string> studentsTaken = new List<string>();
            if (HttpContext.Session.GetString($"QuizStudents_{id}") != null)
            {
                studentsTaken = JsonSerializer.Deserialize<List<string>>(HttpContext.Session.GetString($"QuizStudents_{id}"));
            }

            ViewBag.StudentsTaken = studentsTaken;

            return View(quiz);
        }

        // GET: Quizzes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CourseId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Courses, "Id", "Title");
            return View();
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
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // حذف سجلات StudentAnswers المرتبطة بكل AnswerOption
            var answerOptionIds = quiz.Questions.SelectMany(q => q.AnswerOptions.Select(a => a.Id)).ToList();
            var studentAnswers = _context.StudentAnswers
                .Where(sa => answerOptionIds.Contains(sa.AnswerOptionId));
            _context.StudentAnswers.RemoveRange(studentAnswers);

            // حذف الأسئلة (AnswerOptions سيتم حذفها تلقائيًا إذا كانت Cascade Delete مفعلة)
            _context.Questions.RemoveRange(quiz.Questions);

            // حذف الـ Quiz
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Quizzes/TakeQuiz
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            if (HttpContext.Session.GetInt32($"QuizTaken_{id}") == 1)
            {
                ViewBag.AlreadyTaken = true;
                ViewBag.Score = HttpContext.Session.GetInt32($"QuizScore_{id}") ?? 0;
                ViewBag.Total = quiz.Questions.Count;
            }

            return View(quiz);
        }

        // POST: Quizzes/SubmitQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public IActionResult SubmitQuiz(int quizId, Dictionary<int, int> answers)
        {
            var quiz = _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefault(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            if (HttpContext.Session.GetInt32($"QuizTaken_{quizId}") == 1)
                return RedirectToAction("TakeQuiz", new { id = quizId });

            int score = 0;
            foreach (var q in quiz.Questions)
            {
                if (answers.TryGetValue(q.Id, out int selectedOptionId))
                {
                    var selectedOption = q.AnswerOptions.FirstOrDefault(a => a.Id == selectedOptionId);
                    if (selectedOption != null && selectedOption.IsCorrect)
                        score++;
                }
            }

            HttpContext.Session.SetInt32($"QuizTaken_{quizId}", 1);
            HttpContext.Session.SetInt32($"QuizScore_{quizId}", score);

            // حفظ الطلاب الذين أخذوا الاختبار
            List<string> studentsTaken;
            if (HttpContext.Session.GetString($"QuizStudents_{quizId}") != null)
            {
                studentsTaken = JsonSerializer.Deserialize<List<string>>(HttpContext.Session.GetString($"QuizStudents_{quizId}"));
            }
            else
            {
                studentsTaken = new List<string>();
            }

            var studentName = User.Identity.Name ?? "Unknown";
            if (!studentsTaken.Contains(studentName))
                studentsTaken.Add(studentName);

            HttpContext.Session.SetString($"QuizStudents_{quizId}", JsonSerializer.Serialize(studentsTaken));

            return RedirectToAction("TakeQuiz", new { id = quizId });
        }
    }
}

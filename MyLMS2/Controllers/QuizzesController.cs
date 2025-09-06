using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLMS2.Data;
using MyLMS2.Models;

namespace MyLMS2.Controllers
{
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
            IQueryable<Quiz> quizzesQuery = _context.Quizzes
                .Include(q => q.Course)
                    .ThenInclude(c => c.Enrollments) // مهم للطلاب
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions);

            if (User.IsInRole("Admin"))
            {
                // الأدمن يشوف كل الكويزات
            }
            else if (User.IsInRole("Instructor"))
            {
                var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                quizzesQuery = quizzesQuery.Where(q => q.Course.InstructorId == instructorId);
            }
            else if (User.IsInRole("Student"))
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                quizzesQuery = quizzesQuery.Where(q => q.Course.Enrollments.Any(e => e.StudentId == studentId));
            }

            var quizzes = await quizzesQuery.ToListAsync();
            return View(quizzes);
        }


        // GET: Create - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.CourseId = new SelectList(_context.Courses, "Id", "Title");
            return View();
        }

        // POST: Create - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Duration,CourseId")] Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = quiz.Id });
        }

        // GET: Edit - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", quiz.CourseId);
            return View(quiz);
        }

        // POST: Edit - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Duration,CourseId")] Quiz quiz)
        {
            if (id != quiz.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", quiz.CourseId);
                return View(quiz);
            }

            try
            {
                _context.Update(quiz);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Quizzes.Any(q => q.Id == id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Details), new { id = quiz.Id });
        }

        // GET: Details - All roles
        [Authorize(Roles = "Admin,Instructor,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                    .ThenInclude(c => c.Enrollments) // عشان نجيب الطلاب المسجلين
                        .ThenInclude(e => e.Student)
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // للعرض للادمن والانستركتور
            if (User.IsInRole("Admin") || User.IsInRole("Instructor"))
            {
                var studentsScores = new List<StudentScoreViewModel>();

                foreach (var enrollment in quiz.Course.Enrollments)
                {
                    var studentId = enrollment.StudentId;
                    var student = enrollment.Student;

                    // جلب إجابات الطالب على هذا الكويز
                    var studentAnswers = await _context.StudentAnswers
                        .Where(sa => sa.StudentId == studentId && quiz.Questions.Select(q => q.Id).Contains(sa.QuestionId))
                        .Include(sa => sa.AnswerOption)
                        .ToListAsync();

                    int correctCount = studentAnswers.Count(sa => sa.AnswerOption.IsCorrect);
                    int totalQuestions = quiz.Questions.Count;

                    studentsScores.Add(new StudentScoreViewModel
                    {
                        StudentName = student.UserName,
                        Score = correctCount,
                        Total = totalQuestions,
                        Percentage = totalQuestions > 0 ? (int)((correctCount / (double)totalQuestions) * 100) : 0
                    });
                }

                ViewBag.StudentsScores = studentsScores;
            }

            return View(quiz);
        }


        // AddQuestion GET - Instructor only
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddQuestion(int quizId)
        {
            var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == quizId);
            if (!quizExists) return NotFound();

            ViewBag.QuizId = quizId;
            return View(new Question { QuizId = quizId });
        }

        // AddQuestion POST - Instructor only
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion([Bind("Text,QuizId")] Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("AddAnswerOption", new { questionId = question.Id });
        }

        // AddAnswerOption GET - Instructor only
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddAnswerOption(int questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question == null) return NotFound();

            var options = await _context.AnswerOptions
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();

            ViewBag.QuestionId = questionId;
            ViewBag.Options = options;

            return View(new AnswerOption { QuestionId = questionId });
        }

        // AddAnswerOption POST - Instructor only
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnswerOption([Bind("Text,IsCorrect,QuestionId")] AnswerOption option)
        {
            if (string.IsNullOrWhiteSpace(option.Text))
            {
                ViewBag.QuestionId = option.QuestionId;
                return View(option);
            }

            _context.AnswerOptions.Add(option);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddAnswerOption), new { questionId = option.QuestionId });
        }

        // TakeQuiz GET - Student only
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> TakeQuiz(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب الكويز مع الأسئلة والخيارات
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // تحقق إذا الطالب حل الكويز قبل كده
            bool alreadyTaken = await _context.StudentAnswers
                .AnyAsync(sa => sa.StudentId == studentId && quiz.Questions.Select(q => q.Id).Contains(sa.QuestionId));

            if (alreadyTaken)
            {
                // لو حلّه قبل كده، يحولّه على صفحة Details أو Result
                return RedirectToAction("Result", new { id = id });
            }

            return View(quiz);
        }

        // SubmitQuiz POST - Student only
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int quizId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(studentId)) return Challenge();

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            // تحقق إذا الطالب حل الكويز قبل كده
            bool alreadyTaken = await _context.StudentAnswers
                .AnyAsync(sa => sa.StudentId == studentId && quiz.Questions.Select(q => q.Id).Contains(sa.QuestionId));

            if (alreadyTaken)
            {
                // لو حلّه قبل كده، يحوله على صفحة النتيجة
                return RedirectToAction(nameof(Result), new { id = quizId });
            }

            var form = Request.Form;
            var submissions = new List<(int QuestionId, int AnswerOptionId)>();

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("answers[") && !key.Contains("]."))
                {
                    var inside = key.Substring("answers[".Length);
                    var closeIndex = inside.IndexOf(']');
                    if (closeIndex > 0 && int.TryParse(inside.Substring(0, closeIndex), out int qid))
                    {
                        var val = form[key].ToString();
                        if (int.TryParse(val, out int optId)) submissions.Add((qid, optId));
                    }
                }
            }

            var studentAnswersToAdd = new List<StudentAnswer>();
            int correctCount = 0;

            foreach (var s in submissions)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == s.QuestionId);
                if (question == null) continue;

                var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.Id == s.AnswerOptionId);
                if (selectedOption == null) continue;

                if (selectedOption.IsCorrect) correctCount++;

                studentAnswersToAdd.Add(new StudentAnswer
                {
                    QuestionId = s.QuestionId,
                    AnswerOptionId = s.AnswerOptionId,
                    StudentId = studentId
                });
            }

            if (studentAnswersToAdd.Any())
            {
                _context.StudentAnswers.AddRange(studentAnswersToAdd);
                await _context.SaveChangesAsync();
            }

            var totalQuestions = quiz.Questions?.Count() ?? 0;
            TempData["QuizScore"] = correctCount;
            TempData["QuizTotal"] = totalQuestions;

            return RedirectToAction(nameof(Result), new { id = quizId });
        }

        // Result GET - Student only
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Result(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qn => qn.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب إجابات الطالب لهذا الكويز من قاعدة البيانات
            var studentAnswers = await _context.StudentAnswers
                .Where(sa => sa.StudentId == studentId && quiz.Questions.Select(q => q.Id).Contains(sa.QuestionId))
                .Include(sa => sa.AnswerOption)
                .ToListAsync();

            int score = studentAnswers.Count(sa => sa.AnswerOption.IsCorrect);
            int total = quiz.Questions?.Count() ?? 0;

            ViewBag.Score = score;
            ViewBag.Total = total;
            ViewBag.Percentage = total > 0 ? (int)((score / (double)total) * 100) : 0;

            return View(quiz);
        }

    }
}

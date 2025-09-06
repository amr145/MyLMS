namespace MyLMS2.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // وصف الكويز
        public string? Description { get; set; }

        // مدة الكويز بالدقايق
        public int Duration { get; set; }

        // الكورس اللي يخص الكويز
        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // نص السؤال

        // Foreign Key للـ Quiz
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public ICollection<AnswerOption> AnswerOptions { get; set; }
    }

    public class AnswerOption
    {
        public int Id { get; set; }
        public string Text { get; set; } // نص الاختيار
        public bool IsCorrect { get; set; } // يحدد إذا كان الاختيار صح

        // Foreign Key للسؤال
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }

    // علشان نخزن إجابات الطلاب
    public class StudentAnswer
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int AnswerOptionId { get; set; }
        public AnswerOption AnswerOption { get; set; }

        public string StudentId { get; set; }
        public User Student { get; set; }
    }
}

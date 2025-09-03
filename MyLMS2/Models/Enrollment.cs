namespace MyLMS2.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        // الطالب
        public string StudentId { get; set; }
        public User Student { get; set; }

        // الكورس
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
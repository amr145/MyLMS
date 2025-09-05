using MyLMS2.Models;

namespace MyLMS2.ViewModels
{
    public class InstructorHomeViewModel
    {
        public int CoursesCount { get; set; }   // عدد الكورسات اللي بيدرّسها
        public List<Course> LatestCourses { get; set; } // آخر 3 كورسات
    }
}

using MyLMS2.Models;

namespace MyLMS2.ViewModels
{
    public class StudentHomeViewModel
    {
        public int EnrolledCoursesCount { get; set; }  // عدد الكورسات اللي مشترك فيها
        public List<Course> LatestCourses { get; set; } // آخر 3 كورسات
    }
}

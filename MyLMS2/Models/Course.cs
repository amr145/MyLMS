using System.Collections.Generic;

namespace MyLMS2.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // المدرس
        public string InstructorId { get; set; }
        public User Instructor { get; set; }


        // Navigation: الكورس يحتوي على Modules
        public ICollection<Module> Modules { get; set; }

        // Navigation: الكورس يحتوي على طلاب مسجلين
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
using System.Collections.Generic;

namespace MyLMS2.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }


        public string InstructorId { get; set; }
        public User Instructor { get; set; }



        
        public ICollection<Module> Modules { get; set; }

        

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
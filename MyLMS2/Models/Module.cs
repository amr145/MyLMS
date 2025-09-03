namespace MyLMS2.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        // الكورس اللي الموديول تبعه
        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
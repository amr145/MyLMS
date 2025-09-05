namespace MyLMS2.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        public string? PdfPath { get; set; }
        public string? WordPath { get; set; }
        public string? PptPath { get; set; }
        public string? AudioPath { get; set; }
        public string? VideoPath { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
using System.Reflection;

namespace Userstories.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string? Title { get; set; }
        public string? LearningObjective { get; set; }
        public int? CreditPoints { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? Description { get; set; }
        public ICollection<Modules>? Modules { get; set; }
        public ICollection<Course_Prerequisites>? CoursePrerequisites { get; set; }
        public ICollection<Course_enrollment>? CourseEnrollments { get; set; }
    }
}

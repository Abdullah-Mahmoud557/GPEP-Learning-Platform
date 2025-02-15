namespace Userstories.Models
{
    public class Modules
    {
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string? Title { get; set; }
        public string? Difficulty { get; set; }
        public string? ContentURL { get; set; }
        public Course? Course { get; set; }
        public ICollection<Learning_activities>? LearningActivities { get; set; }
        public ICollection<Assessments> Assessments { get; set; }
      
    }
}

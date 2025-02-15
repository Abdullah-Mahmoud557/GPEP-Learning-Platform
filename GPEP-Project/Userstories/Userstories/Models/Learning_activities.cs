namespace Userstories.Models
{
    public class Learning_activities
    {
        public int ActivityID { get; set; }
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string? ActivityType { get; set; }
        public string? InstructionDetails { get; set; }
        public int? MaxPoints { get; set; }
        public Modules? Modules { get; set; }
    }
}

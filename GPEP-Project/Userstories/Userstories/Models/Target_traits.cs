namespace Userstories.Models
{
    public class Target_traits
    {
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string? Trait { get; set; }
        public Modules? Modules { get; set; }
    }
}

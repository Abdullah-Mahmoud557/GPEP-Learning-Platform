namespace Userstories.Models
{
    public class Course_Prerequisites
    {
        public int CourseID { get; set; }
        public int PreRequisiteID { get; set; }
        public Course? Course { get; set; }
        public Course? PreRequisite { get; set; }
    }
}

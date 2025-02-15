namespace Userstories.Models
{
    public class Course_enrollment
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public int LearnerID { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string? Status { get; set; }
        public Course? Course { get; set; }
        public Learner? Learner { get; set; }
    }
}

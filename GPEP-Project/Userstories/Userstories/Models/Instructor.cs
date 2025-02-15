namespace Userstories.Models
{
    public class Instructor
    {
        public int InstructorID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[]? ImageData { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LatestQualification { get; set; }
        public string? ExpertiseArea { get; set; }
    }
}

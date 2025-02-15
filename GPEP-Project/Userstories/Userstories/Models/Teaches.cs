using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models
{
    public class Teaches
    {
        public int InstructorID { get; set; }

        public int CourseID { get; set; }

        public Instructor? Instructor { get; set; }

        public Course? Course { get; set; }
    }
}

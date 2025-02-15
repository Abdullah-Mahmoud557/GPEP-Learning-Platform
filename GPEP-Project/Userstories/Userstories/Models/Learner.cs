using System;
using System.ComponentModel.DataAnnotations;

namespace Userstories.Models
{
    public class Learner
    {
        public int LearnerID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        public byte[]? ImageData { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public string? Country { get; set; }

        public string? CulturalBackground { get; set; }
        public ICollection<PersonalizationProfiles> PersonalizationProfiles { get; set; }

    }
}

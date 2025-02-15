using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models
{
    [Table("InstructorDiscussion")] // Matches the database table name
    public class InstructorDiscussion
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        public int ForumID { get; set; } // Foreign Key to DiscussionForum

        [Key]
        [Column(Order = 2)]
        [Required]
        public int InstructorID { get; set; } // Foreign Key to Instructor

        [Key]
        [Column(Order = 3)]
        [Required]
        [StringLength(500)]
        public string Post { get; set; } // Post content with max length of 500 characters

        [Column("time")]
        public DateTime? Time { get; set; } // Timestamp of the post

        // Navigation properties
        [ForeignKey("ForumID")]
        public DiscussionForum DiscussionForum { get; set; }

        [ForeignKey("InstructorID")]
        public Instructor Instructor { get; set; }
    }
}
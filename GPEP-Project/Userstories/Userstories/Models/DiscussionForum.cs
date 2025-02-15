
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models
{
    [Table("Discussion_forum")] // Matches the table name in the database
    public class DiscussionForum
    {
        [Key]
        [Column("forumID")] // Matches the column name in the table
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ForumID { get; set; }

        [Required]
        [Column("ModuleID")] // Matches the column name in the table
        public int ModuleID { get; set; }

        [Required]
        [Column("CourseID")] // Matches the column name in the table
        public int CourseID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("title")] // Matches the column name in the table
        public string Title { get; set; }

        [Column("last_active")] // Matches the column name in the table
        public DateTime LastActive { get; set; }

        [Column("timestamp")] // Matches the column name in the table
        public DateTime Timestamp { get; set; }

        [StringLength(50)]
        [Column("description")] // Matches the column name in the table
        public string Description { get; set; }
    }
}

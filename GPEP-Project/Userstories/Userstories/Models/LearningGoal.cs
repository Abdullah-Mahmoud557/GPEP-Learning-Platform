using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models
{
    [Table("Learning_goal")] // Matches the database table name
    public class LearningGoal
    {
        [Key]
        [Column("ID")] // Maps to the ID column in the table
        public int ID { get; set; }

        [Column("status", TypeName = "VARCHAR(MAX)")] // Maps to the status column
        public string Status { get; set; }

        [Column("deadline", TypeName = "DATETIME")] // Maps to the deadline column
        public DateTime Deadline { get; set; }

        [Column("description", TypeName = "VARCHAR(MAX)")] // Maps to the description column
        public string Description { get; set; }
    }
}
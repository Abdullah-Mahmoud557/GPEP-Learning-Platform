using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;

[Table("ReceivedNotification")] // Matches the database table name
public class ReceivedNotification
{
    [Key]
    [Required]
    public int NotificationID { get; set; } // Matches NotificationID, references Notification

    [Key]
    [Required]
    public int LearnerID { get; set; } // Matches LearnerID, references Learner

    [Required]
    [Column("ReadStatus")]
    public bool ReadStatus { get; set; } = false; // Matches ReadStatus (BIT), default to false

    // Navigation properties for relationships
    public Notification? Notification { get; set; } // Navigation property for Notification
    public Learner? Learner { get; set; } // Navigation property for Learner
}
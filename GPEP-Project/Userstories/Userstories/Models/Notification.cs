using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;

[Table("Notification")] // Matches the database table name
public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; } // Primary Key for Notification

    [Required(ErrorMessage = "Timestamp is required.")]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; } // Matches timestamp (DATETIME)

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(500)]
    [Column("message")]
    public string? Message { get; set; } // Matches message (VARCHAR(500))

    [Required(ErrorMessage = "Urgency Level is required.")]
    [StringLength(50)]
    [Column("urgency_level")]
    public string? UrgencyLevel { get; set; } // Matches urgency_level (VARCHAR(50))

    // Navigation properties for relationships
    public ICollection<ReceivedNotification>? ReceivedNotifications { get; set; } // Navigation property for ReceivedNotification
}
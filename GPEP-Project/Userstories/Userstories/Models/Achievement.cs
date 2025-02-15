namespace Userstories.Models
{
    public class Achievement
    {
        public int AchievementID { get; set; }
        public int LearnerID { get; set; }
        public int BadgeID { get; set; }
        public string? description { get; set; }
        public DateTime? date_earned { get; set; }
        public string? type { get; set; }

        public Learner? Learner { get; set; }
        public Badge? Badge { get; set; }
    }
}
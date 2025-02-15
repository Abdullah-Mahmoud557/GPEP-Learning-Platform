namespace Userstories.Models
{
    public class Badge
    {
        public int BadgeID { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? criteria { get; set; }
        public int? points { get; set; }
    }
}
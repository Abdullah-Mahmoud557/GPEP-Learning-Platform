using System.ComponentModel.DataAnnotations;

namespace Userstories.Models
{
    public class Collaborative
    {
        public int QuestID { get; set; }
        [DataType(DataType.Date)]
        public DateTime? deadline { get; set; }
        public int? max_num_participants { get; set; }
        public string? difficulty_level { get; set; }
        public string? criteria { get; set; }
        public string? description { get; set; }
        public string? title { get; set; }
    }
}
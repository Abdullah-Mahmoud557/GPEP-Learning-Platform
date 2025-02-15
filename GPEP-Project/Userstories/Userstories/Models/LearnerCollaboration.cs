namespace Userstories.Models
{
    public class LearnerCollaboration
    {
        public int QuestID { get; set; }
        public int LearnerID { get; set; }
        public string? completion_status { get; set; }

        public Collaborative? Collaborative { get; set; }
        public Learner? Learner { get; set; }
    }
}
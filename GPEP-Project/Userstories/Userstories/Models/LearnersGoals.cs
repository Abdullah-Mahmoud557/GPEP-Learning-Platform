using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models
{
    public class LearnersGoals
    {
        
        public int GoalID { get; set; }

       
        public int LearnerID { get; set; }

        [ForeignKey("GoalID")]
        public LearningGoal LearningGoal { get; set; }

        [ForeignKey("LearnerID")]
        public Learner Learner { get; set; }
    }
}
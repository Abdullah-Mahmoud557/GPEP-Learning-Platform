using Microsoft.EntityFrameworkCore;
using Userstories.Models;

namespace Userstories.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admin { get; set; }
        public DbSet<Learner> Learner { get; set; }
        public DbSet<Instructor> Instructor { get; set; }
        public DbSet<SecretCode> SecretCode { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<Course_Prerequisites> Course_Prerequisites { get; set; }
        public DbSet<Course_enrollment> Course_enrollment { get; set; }
        public DbSet<Teaches> Teaches { get; set; }
        public DbSet<Modules> Modules { get; set; }
        public DbSet<Target_traits> TargetTraits { get; set; }
        public DbSet<ModuleContent> ModuleContent { get; set; }
        public DbSet<Learning_activities> Learning_activities { get; set; }
        public DbSet<PersonalizationProfiles> PersonalizationProfiles { get; set; } = default!;
        public DbSet<LearningGoal> LearningGoals { get; set; }
        public DbSet<LearnersGoals> LearnersGoals { get; set; }
        public DbSet<LearningPath> LearningPath { get; set; }
        public DbSet<DiscussionForum> DiscussionForum { get; set; }
        public DbSet<Assessments> Assessments { get; set; }
        public DbSet<TakenAssessments> TakenAssessments { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<ReceivedNotification> ReceivedNotification { get; set; }
        public DbSet<InstructorDiscussion> InstructorDiscussion { get; set; } // Add this DbSet
        public DbSet<LearnerDiscussion> LearnerDiscussion { get; set; }
        
        public DbSet<Collaborative> Collaborative { get; set; }
        public DbSet<LearnerCollaboration> LearnerCollaboration { get; set; }
        public DbSet<Badge> Badge { get; set; }
        public DbSet<Achievement> Achievement { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Admin Configuration
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.AdminID);

            // Learner Configuration
            modelBuilder.Entity<Learner>()
                .HasKey(l => l.LearnerID);

            // Instructor Configuration
            modelBuilder.Entity<Instructor>()
                .HasKey(i => i.InstructorID);

            // SecretCode Configuration
            modelBuilder.Entity<SecretCode>()
                .HasKey(sc => sc.Code);

            // Course and Course_Prerequisites Configuration
            modelBuilder.Entity<Course>()
                .HasMany(c => c.CoursePrerequisites)
                .WithOne(cp => cp.Course)
                .HasForeignKey(cp => cp.CourseID);

            modelBuilder.Entity<Course_Prerequisites>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.CoursePrerequisites)
                .HasForeignKey(cp => cp.CourseID);

            modelBuilder.Entity<Course_Prerequisites>()
                .HasOne(cp => cp.PreRequisite)
                .WithMany()
                .HasForeignKey(cp => cp.PreRequisiteID);

            modelBuilder.Entity<Course_Prerequisites>()
                .HasKey(cp => new { cp.CourseID, cp.PreRequisiteID });
            // Course_enrollment Configuration
            modelBuilder.Entity<Course_enrollment>()
                .HasKey(ce => ce.EnrollmentID);

            // Teaches Configuration
            modelBuilder.Entity<Teaches>()
                .HasKey(t => new { t.InstructorID, t.CourseID });

            // Modules Configuration
            modelBuilder.Entity<Modules>()
                .HasKey(m => new { m.ModuleID, m.CourseID });

            // Target_traits Configuration
            modelBuilder.Entity<Target_traits>()
                .HasKey(tt => new { tt.ModuleID, tt.CourseID });

            // ModuleContent Configuration
            modelBuilder.Entity<ModuleContent>()
                .HasKey(mc => new { mc.ModuleID, mc.CourseID });

            // Learning_activities Configuration
            modelBuilder.Entity<Learning_activities>()
                .HasKey(la => la.ActivityID);

            modelBuilder.Entity<Learning_activities>()
                .HasOne(la => la.Modules)
                .WithMany(m => m.LearningActivities)
                .HasForeignKey(la => new { la.ModuleID, la.CourseID });



            modelBuilder.Entity<PersonalizationProfiles>()
                .HasKey(p => new { p.LearnerID, p.ProfileID }); // Define composite key

            modelBuilder.Entity<PersonalizationProfiles>()
                .HasOne(p => p.Learner) // Define relationship
                .WithMany(l => l.PersonalizationProfiles)
                .HasForeignKey(p => p.LearnerID)
                .OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<LearnersGoals>()
                .HasKey(lg => new { lg.GoalID, lg.LearnerID });

            modelBuilder.Entity<LearningPath>()
                .HasOne(lp => lp.PersonalizationProfiles)
                .WithMany()
                .HasForeignKey(lp => new { lp.LearnerID, lp.ProfileID })
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Assessments
            modelBuilder.Entity<Assessments>()
                .HasOne(a => a.Module)
                .WithMany(m => m.Assessments)
                .HasForeignKey(a => new { a.ModuleID, a.CourseID })
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TakenAssessments
            modelBuilder.Entity<TakenAssessments>(entity =>
            {
                entity.ToTable("Taken_assessments");
                entity.HasKey(e => new { e.LearnerID, e.AssessmentID }); // Composite Key

                entity.Property(e => e.ScoredPoints)
                    .HasColumnName("Scored_points")
                    .IsRequired();

                // Foreign Key Relationships
                entity.HasOne(e => e.Learner)
                    .WithMany()
                    .HasForeignKey(e => e.LearnerID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.TakenAssessments)
                    .HasForeignKey(e => e.AssessmentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .IsRequired();

                entity.Property(e => e.Message)
                    .HasColumnName("message")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.UrgencyLevel)
                    .HasColumnName("urgency_level")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configure ReceivedNotification
            modelBuilder.Entity<ReceivedNotification>(entity =>
            {
                entity.ToTable("ReceivedNotification");
                entity.HasKey(e => new { e.NotificationID, e.LearnerID }); // Composite Key

                entity.Property(e => e.ReadStatus)
                    .HasColumnName("ReadStatus")
                    .HasDefaultValue(false)
                    .IsRequired();

                // Foreign Key Relationships
                entity.HasOne(e => e.Notification)
                    .WithMany(n => n.ReceivedNotifications)
                    .HasForeignKey(e => e.NotificationID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Learner)
                    .WithMany()
                    .HasForeignKey(e => e.LearnerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // InstructorDiscussion Configuration
            modelBuilder.Entity<InstructorDiscussion>(entity =>
            {
                entity.ToTable("InstructorDiscussion"); // Maps to the database table

                // Composite Primary Key
                entity.HasKey(e => new { e.ForumID, e.InstructorID, e.Post });

                // Properties
                entity.Property(e => e.Post)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Time)
                    .HasColumnName("time");

                // Relationships
                entity.HasOne(e => e.DiscussionForum)
                    .WithMany()
                    .HasForeignKey(e => e.ForumID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Instructor)
                    .WithMany()
                    .HasForeignKey(e => e.InstructorID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // LearnerDiscussion Configuration
            modelBuilder.Entity<LearnerDiscussion>(entity =>
            {
                entity.ToTable("LearnerDiscussion"); // Maps to the database table

                // Composite Primary Key
                entity.HasKey(e => new { e.ForumID, e.LearnerID, e.Post });

                // Properties
                entity.Property(e => e.Post)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Time)
                    .HasColumnName("time");

                // Relationships
                entity.HasOne(e => e.DiscussionForum)
                    .WithMany()
                    .HasForeignKey(e => e.ForumID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Learner)
                    .WithMany()
                    .HasForeignKey(e => e.LearnerID)
                    .OnDelete(DeleteBehavior.Cascade);

            });
            // Collaborative Configuration
            modelBuilder.Entity<Collaborative>()
                .HasKey(c => c.QuestID);

            // LearnerCollaboration Configuration
            modelBuilder.Entity<LearnerCollaboration>()
                .HasKey(lc => new { lc.QuestID, lc.LearnerID });

            modelBuilder.Entity<LearnerCollaboration>()
                .HasOne(lc => lc.Collaborative)
                .WithMany()
                .HasForeignKey(lc => lc.QuestID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearnerCollaboration>()
                .HasOne(lc => lc.Learner)
                .WithMany()
                .HasForeignKey(lc => lc.LearnerID)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
            
            // Badge Configuration
            modelBuilder.Entity<Badge>()
                .HasKey(b => b.BadgeID);
            
            // Achievement Configuration
            modelBuilder.Entity<Achievement>()
                .HasKey(a => a.AchievementID);
            
            // Configure relationships for Achievement
            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.Learner)
                .WithMany()
                .HasForeignKey(a => a.LearnerID);

            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.Badge)
                .WithMany()
                .HasForeignKey(a => a.BadgeID);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await Admin.AnyAsync(a => a.Email == email) ||
                   await Learner.AnyAsync(l => l.Email == email) ||
                   await Instructor.AnyAsync(i => i.Email == email);
        }
    }
}

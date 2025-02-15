create table dbo.Admin
(
    AdminID   int identity
        primary key,
    Email     nvarchar(256) not null
        unique,
    Password  nvarchar(256) not null,
    CreatedAt datetime2 default getdate(),
    ImageData varbinary(max)
)
go

create table dbo.Badge
(
    BadgeID     int identity
        primary key,
    title       varchar(100),
    description varchar(255),
    criteria    varchar(255),
    points      int
)
go
INSERT INTO dbo.Badge (title, description, criteria, points)
VALUES ('Beginner Programmer', 'Awarded for completing an introductory programming course', 'Complete an introductory programming course', 10),
         ('Data Structures Expert', 'Awarded for mastering data structures and algorithms', 'Complete an advanced data structures and algorithms course', 20),
         ('Machine Learning Specialist', 'Awarded for excelling in machine learning concepts', 'Complete an advanced machine learning course', 30);
go
create table dbo.Collaborative
(
    QuestID              int identity
        primary key,
    deadline             datetime2,
    max_num_participants int,
    difficulty_level     varchar(max),
    criteria             varchar(max),
    description          varchar(max),
    title                varchar(max)
)
go
INSERT INTO dbo.Collaborative (deadline, max_num_participants, difficulty_level, criteria, description, title)
VALUES ('2022-12-31', 5, 'Intermediate', 'Must have completed an intermediate course', 'This quest will challenge you to apply your skills in a real-world scenario.', 'Data Analysis Challenge'),
       ('2022-11-30', 3, 'Advanced', 'Must have completed an advanced course', 'This quest will test your problem-solving abilities and creativity.', 'Machine Learning Project'),
       ('2022-10-31', 2, 'Beginner', 'Must have completed a beginner course', 'This quest will introduce you to basic concepts and help you build a foundation.', 'Programming Exercise'),
        ('2022-09-30', 4, 'Intermediate', 'Must have completed an intermediate course', 'This quest will challenge you to think critically and collaborate with others.', 'Group Project'),
        ('2022-08-31', 3, 'Advanced', 'Must have completed an advanced course', 'This quest will push you to explore new ideas and innovate.', 'Research Challenge');
go
create table dbo.Course
(
    CourseID          int identity
        primary key,
    Title             varchar(100),
    LearningObjective varchar(250),
    CreditPoints      int,
    DifficultyLevel   varchar(50),
    Description       varchar(500)
)
go
INSERT INTO dbo.Course (Title, LearningObjective, CreditPoints, DifficultyLevel, Description)
VALUES ('Introduction to Programming', 'Learn the basics of programming', 3, 'Beginner', 'This course will introduce you to the basics of programming and help you understand the core concepts of coding.'),
       ('Data Structures and Algorithms', 'Understand data structures and algorithms', 4, 'Intermediate', 'This course will cover advanced topics in data structures and algorithms to help you improve your problem-solving skills.'),
       ('Machine Learning Fundamentals', 'Learn the basics of machine learning', 5, 'Advanced', 'This course will provide an introduction to machine learning and cover the fundamental concepts and techniques used in the field.'),
            ('Web Development Basics', 'Understand the basics of web development', 3, 'Beginner', 'This course will teach you the basics of web development, including HTML, CSS, and JavaScript.'),
            ('Database Management', 'Learn about database management', 4, 'Intermediate', 'This course will cover the principles of database management and teach you how to design and implement databases.');
go
create table dbo.Course_Prerequisites
(
    CourseID       int not null
        references dbo.Course
            on update cascade on delete cascade,
    PreRequisiteID int not null
        references dbo.Course,
    primary key (CourseID, PreRequisiteID)
)
go
INSERT INTO dbo.Course_Prerequisites (CourseID, PreRequisiteID)
VALUES (2, 1),
       (3, 2),
         (4, 3),
         (5, 4);
go
create table dbo.Instructor
(
    InstructorID        int identity
        primary key,
    Email               nvarchar(256) not null
        unique,
    Password            nvarchar(256) not null,
    CreatedAt           datetime2 default getdate(),
    ImageData           varbinary(max),
    FirstName           nvarchar(100),
    LastName            nvarchar(100),
    LatestQualification varchar(100),
    ExpertiseArea       varchar(100)
)
go

create table dbo.Learner
(
    LearnerID          int identity
        primary key,
    Email              nvarchar(256) not null
        unique,
    Password           nvarchar(256) not null,
    CreatedAt          datetime2 default getdate(),
    ImageData          varbinary(max),
    FirstName          varchar(20),
    LastName           varchar(20),
    Gender             char,
    BirthDate          date,
    Country            varchar(20),
    CulturalBackground varchar(100)
)
go

create table dbo.Achievement
(
    AchievementID int identity
        primary key,
    LearnerID     int
        references dbo.Learner
            on update cascade on delete cascade,
    BadgeID       int
        references dbo.Badge
            on update cascade on delete cascade,
    description   varchar(max),
    date_earned   datetime2,
    type          varchar(max)
)
go
create table dbo.Course_enrollment
(
    EnrollmentID   int identity
        primary key,
    CourseID       int
        references dbo.Course
            on update cascade on delete cascade,
    LearnerID      int
        references dbo.Learner
            on update cascade on delete cascade,
    CompletionDate date,
    EnrollmentDate date,
    Status         varchar(50)
)
go

create table dbo.LearnerCollaboration
(
    QuestID           int not null
        references dbo.Collaborative
            on update cascade on delete cascade,
    LearnerID         int not null
        references dbo.Learner
            on update cascade on delete cascade,
    completion_status varchar(50),
    primary key (QuestID, LearnerID)
)
go

create table dbo.Learning_goal
(
    ID          int not null
        primary key,
    status      varchar(max),
    deadline    datetime,
    description varchar(max)
)
go

create table dbo.LearnersGoals
(
    GoalID    int not null
        references dbo.Learning_goal
            on update cascade on delete cascade,
    LearnerID int not null
        references dbo.Learner
            on update cascade on delete cascade,
    primary key (GoalID, LearnerID)
)
go

create table dbo.Modules
(
    ModuleID   int identity,
    CourseID   int not null
        references dbo.Course
            on update cascade on delete cascade,
    Title      varchar(100),
    Difficulty varchar(50),
    ContentURL varchar(250),
    primary key (ModuleID, CourseID)
)
go
INSERT INTO dbo.Modules (CourseID, Title, Difficulty, ContentURL)
VALUES (1, 'Introduction to Programming', 'Beginner', 'https://www.example.com/intro-programming'),
       (2, 'Data Structures and Algorithms', 'Intermediate', 'https://www.example.com/data-structures'),
       (3, 'Machine Learning Fundamentals', 'Advanced', 'https://www.example.com/machine-learning'),
         (4, 'Web Development Basics', 'Beginner', 'https://www.example.com/web-dev-basics'),
         (5, 'Database Management', 'Intermediate', 'https://www.example.com/database-management');
go
create table dbo.Assessments
(
    ID            int identity
        primary key,
    ModuleID      int,
    CourseID      int,
    type          varchar(50),
    total_marks   int,
    passing_marks int,
    criteria      varchar(250),
    weightage     float,
    description   varchar(500),
    title         varchar(100),
    foreign key (ModuleID, CourseID) references dbo.Modules
        on update cascade on delete cascade
)
go

create table dbo.Discussion_forum
(
    forumID     int identity
        primary key,
    ModuleID    int,
    CourseID    int,
    title       varchar(50),
    last_active datetime,
    timestamp   datetime,
    description varchar(50),
    foreign key (ModuleID, CourseID) references dbo.Modules
        on update cascade on delete cascade
)
go

create table dbo.InstructorDiscussion
(
    ForumID      int          not null
        references dbo.Discussion_forum
            on update cascade on delete cascade,
    InstructorID int          not null
        references dbo.Instructor
            on update cascade on delete cascade,
    Post         varchar(500) not null,
    time         datetime,
    primary key (ForumID, InstructorID, Post)
)
go

create table dbo.LearnerDiscussion
(
    ForumID   int          not null
        references dbo.Discussion_forum
            on update cascade on delete cascade,
    LearnerID int          not null
        references dbo.Learner
            on update cascade on delete cascade,
    Post      varchar(500) not null,
    time      datetime,
    primary key (ForumID, LearnerID, Post)
)
go

create table dbo.Learning_activities
(
    ActivityID         int identity
        primary key,
    ModuleID           int,
    CourseID           int,
    ActivityType       varchar(50),
    InstructionDetails nvarchar(max),
    MaxPoints          int,
    foreign key (ModuleID, CourseID) references dbo.Modules
        on update cascade on delete cascade
)
go

create table dbo.ModuleContent
(
    ModuleID    int          not null,
    CourseID    int          not null,
    ContentType varchar(250) not null,
    primary key (ModuleID, CourseID, ContentType),
    foreign key (ModuleID, CourseID) references dbo.Modules
        on update cascade on delete cascade
)
go

create table dbo.Notification
(
    ID            int identity
        primary key,
    timestamp     datetime,
    message       varchar(500),
    urgency_level varchar(50)
)
go

create table dbo.PersonalizationProfiles
(
    LearnerID              int not null
        references dbo.Learner
            on update cascade on delete cascade,
    ProfileID              int identity,
    preferred_content_type varchar(50),
    emotional_state        varchar(50),
    personality_type       varchar(50),
    primary key (LearnerID, ProfileID)
)
go

create table dbo.Learning_path
(
    pathID            int identity
        primary key,
    LearnerID         int,
    ProfileID         int,
    completion_status varchar(50),
    custom_content    varchar(max),
    adaptive_rules    varchar(max),
    foreign key (LearnerID, ProfileID) references dbo.PersonalizationProfiles
        on update cascade on delete cascade
)
go

create table dbo.Pathreview
(
    InstructorID int not null
        references dbo.Instructor
            on update cascade on delete cascade,
    PathID       int not null
        references dbo.Learning_path
            on update cascade on delete cascade,
    feedback     varchar(500),
    primary key (InstructorID, PathID)
)
go

create table dbo.ReceivedNotification
(
    NotificationID int not null
        references dbo.Notification
            on update cascade on delete cascade,
    LearnerID      int not null
        references dbo.Learner
            on update cascade on delete cascade,
    ReadStatus     bit default 0,
    primary key (NotificationID, LearnerID)
)
go

create table dbo.SecretCodes
(
    Code nvarchar(20) not null
        primary key
)
go
INSERT INTO dbo.SecretCodes (Code)
VALUES ('SECRET123'),
       ('ADMIN456');
create table dbo.Taken_assessments
(
    LearnerID     int not null
        references dbo.Learner
            on update cascade on delete cascade,
    AssessmentID  int not null
        references dbo.Assessments
            on update cascade on delete cascade,
    Scored_points int,
    primary key (LearnerID, AssessmentID)
)
go

create table dbo.Target_traits
(
    ModuleID int          not null,
    CourseID int          not null,
    Trait    varchar(100) not null,
    primary key (ModuleID, CourseID, Trait),
    foreign key (ModuleID, CourseID) references dbo.Modules
        on update cascade on delete cascade
)
go

create table dbo.Teaches
(
    InstructorID int not null
        references dbo.Instructor
            on update cascade on delete cascade,
    CourseID     int not null
        references dbo.Course
            on update cascade on delete cascade,
    primary key (InstructorID, CourseID)
)
go




CREATE   PROCEDURE AddGoal
    @LearnerID INT,
    @GoalID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID) AND EXISTS (SELECT 1 FROM Learning_goal WHERE ID = @GoalID)
        BEGIN
            INSERT INTO LearnersGoals (GoalID, LearnerID)
            VALUES (@GoalID, @LearnerID);
            PRINT 'Goal added successfully.';
        END
    ELSE
        BEGIN
            PRINT 'Error: LearnerID or GoalID does not exist.';
        END
END;
go

/*
20) Access a breakdown of my assessment scores to identify strengths and weaknesses.
Signature:
Name: AssessmentAnalysis.
Input: @LearnerID int.
Output: Table with detailed score breakdowns and performance analysis.
*/
CREATE   PROCEDURE AssessmentAnalysis
@LearnerID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID)
        BEGIN
            SELECT
                a.ID AS AssessmentID,
                a.title AS AssessmentTitle,
                a.type AS AssessmentType,
                a.total_marks AS TotalMarks,
                ta.Scored_points AS ScoredPoints,
                (ta.Scored_points * 100.0 / a.total_marks) AS Percentage,
                CASE
                    WHEN (ta.Scored_points * 100.0 / a.total_marks) >= 90 THEN 'Excellent'
                    WHEN (ta.Scored_points * 100.0 / a.total_marks) >= 75 THEN 'Good'
                    WHEN (ta.Scored_points * 100.0 / a.total_marks) >= 50 THEN 'Average'
                    ELSE 'Needs Improvement'
                    END AS Performance
            FROM Assessments a
                     JOIN Taken_assessments ta ON a.ID = ta.AssessmentID
            WHERE ta.LearnerID = @LearnerID
            ORDER BY a.ID;
        END
    ELSE
        BEGIN
            PRINT 'Error: LearnerID does not exist.';
        END
END;
go

-- 20. Generate analytics on assessment scores across various modules or courses (average of scores).
CREATE   PROCEDURE AssessmentAnalytics (@CourseID INT, @ModuleID INT)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Course WHERE CourseID = @CourseID)
        AND EXISTS (SELECT 1 FROM Modules WHERE ModuleID = @ModuleID)
        BEGIN
            SELECT subquery.average_score, a2.*
            FROM (SELECT AVG(ta.Scored_points) AS average_score, a2.ID AS AssessmentID
                  FROM Assessments a2
                           JOIN Taken_assessments ta ON a2.ID = ta.AssessmentID
                  WHERE a2.CourseID = @CourseID AND a2.ModuleID = @ModuleID
                  GROUP BY a2.ID) AS subquery
                     JOIN Assessments a2 ON subquery.AssessmentID = a2.ID;
        END
    ELSE
        BEGIN
            PRINT 'Course or Module not found.';
        END
END;
go

-- 10. AssessmentNot: Send a notification about an upcoming assessment deadline.
CREATE   PROCEDURE AssessmentNot
(@NotificationID INT, @Timestamp DATETIME, @Message VARCHAR(MAX), @UrgencyLevel VARCHAR(50), @LearnerID INT)
AS
BEGIN
    DECLARE @confirmation VARCHAR(MAX);

    IF EXISTS (SELECT 1 FROM Notification WHERE ID = @NotificationID)
        BEGIN
            SET @confirmation = 'Notification already exists.';
        END
    ELSE
        BEGIN
            INSERT INTO Notification (Timestamp, Message, urgency_level)
            VALUES (@Timestamp, @Message, @UrgencyLevel);
            SET @confirmation = 'Notification inserted successfully.';
        END

    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID)
        BEGIN
            IF EXISTS (SELECT 1 FROM ReceivedNotification WHERE LearnerID = @LearnerID AND NotificationID = @NotificationID)
                BEGIN
                    SET @confirmation += ' Notification already sent to the learner.';
                END
            ELSE
                BEGIN
                    INSERT INTO ReceivedNotification (LearnerID, NotificationID)
                    VALUES (@LearnerID, @NotificationID);
                    SET @confirmation += ' Notification sent to learner successfully.';
                END
        END
    ELSE
        BEGIN
            SET @confirmation += ' Learner does not exist, notification not sent.';
        END

    SELECT @confirmation AS Confirmation;
END;
go

/*
11) View all the assessments I took and its grades for a certain module.
Signature:
Name: AssessmentsList.
Input: @courseID int, @ModuleID int.
Output: A table containing all the assessments and their grades.
*/
----------------------------------------
CREATE   PROC AssessmentsList
    @LearnerID INT,
    @courseID INT,
    @ModuleID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Assessments WHERE CourseID = @courseID AND ModuleID = @ModuleID) and Exists (select 1 from Learner l where l.LearnerID = @LearnerID)
        BEGIN
            SELECT
                a.ID AS AssessmentID,
                a.title AS AssessmentTitle,
                a.type AS AssessmentType,
                a.total_marks AS TotalMarks,
                ta.Scored_points AS Grade
            FROM
                Assessments a
                    JOIN
                Taken_Assessments ta ON a.ID = ta.AssessmentID
            WHERE
                a.CourseID = @courseID AND
                a.ModuleID = @ModuleID
              and ta.LearnerID = @LearnerID;
        END
    ELSE
        BEGIN
            if Exists (select 1 from Learner l where l.LearnerID = @LearnerID)
                PRINT 'No assessments found for the given CourseID and ModuleID and LearnerID.';
            else
                PRINT 'No such learner found';
        END
END;
go

CREATE PROCEDURE CheckEmailExists
    @Email NVARCHAR(256),
    @Exists BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Learner WHERE Email = @Email)
        BEGIN
            SET @Exists = 1;
            RETURN;
        END

    IF EXISTS (SELECT 1 FROM Instructor WHERE Email = @Email)
        BEGIN
            SET @Exists = 1;
            RETURN;
        END

    IF EXISTS (SELECT 1 FROM Admin WHERE Email = @Email)
        BEGIN
            SET @Exists = 1;
            RETURN;
        END

    SET @Exists = 0;
END
go

------------
--Procedures
-- Procedure to check if a learner has completed the prerequisites for a course
CREATE   PROCEDURE CheckPrerequisites
    @LearnerID INT,
    @CourseID INT
AS
BEGIN
    DECLARE @UnmetPrereqsCount INT;
    SELECT @UnmetPrereqsCount = COUNT(*)
    FROM Course_Prerequisites cp
             LEFT JOIN Course_enrollment ce ON cp.PreRequisiteID = ce.CourseID AND ce.LearnerID = @LearnerID
    WHERE cp.CourseID = @CourseID AND (ce.CourseID IS NULL OR ce.Status != 'Completed');

    IF @UnmetPrereqsCount > 0
        BEGIN
            PRINT 'Not all prerequisites are completed.';
        END
    ELSE
        BEGIN
            PRINT 'All prerequisites are completed.';
        END
END;
go

-- Procedure to list details of previously taken courses
CREATE   PROCEDURE CompletedCourses
@LearnerID INT
AS
BEGIN
    SELECT c.*
    FROM Course_enrollment ce
             JOIN Course c ON ce.CourseID = c.CourseID
    WHERE ce.LearnerID = @LearnerID AND ce.Status = 'Completed';
END;
go

CREATE   PROCEDURE Courseregister
    @LearnerID INT,
    @CourseID INT,
    @result INT OUTPUT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID) AND EXISTS (SELECT 1 FROM Course WHERE CourseID = @CourseID)
        BEGIN
            IF EXISTS (SELECT 1 FROM Course_enrollment WHERE CourseID = @CourseID AND LearnerID = @LearnerID)
                BEGIN
                    SET @result = 1;
                END
            ELSE
                BEGIN
                    DECLARE @UnmetPrereqsCount INT;
                    SELECT @UnmetPrereqsCount = COUNT(*)
                    FROM Course_Prerequisites cp
                             LEFT JOIN Course_enrollment ce ON cp.PrerequisiteID = ce.CourseID AND ce.LearnerID = @LearnerID
                    WHERE cp.CourseID = @CourseID AND (ce.CourseID IS NULL OR ce.status != 'completed');

                    IF @UnmetPrereqsCount > 0
                        BEGIN
                            SET @result = 2;
                        END
                    ELSE
                        BEGIN
                            SET @result = 3;
                            INSERT INTO Course_enrollment (CourseID, LearnerID, EnrollmentDate, status)
                            VALUES (@CourseID, @LearnerID, GETDATE(), 'In Progress');
                        END
                END
        END
    ELSE
        BEGIN
            SET @result = 4;
        END
END;
go

CREATE   PROCEDURE CurrentPath
@LearnerID INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID)
        BEGIN
            SELECT *
            FROM Learning_path
            WHERE LearnerID = @LearnerID;
        END
    ELSE
        BEGIN
            PRINT 'Error: LearnerID does not exist.';
        END
END;
go

CREATE   PROCEDURE DeleteCourseIfNoEnrollment
@CourseID INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Course_enrollment WHERE CourseID = @CourseID)
        BEGIN
            DELETE FROM Course WHERE CourseID = @CourseID;
            PRINT 'Course deleted successfully.';
        END
    ELSE
        BEGIN
            PRINT 'Cannot delete the course. There are learners enrolled in it.';
        END
END;
go

CREATE    PROCEDURE EnrolledCourses
@LearnerID INT
AS

BEGIN
    SELECT c.*
    FROM Course_enrollment ce
             JOIN Course c ON ce.CourseID = c.CourseID
    WHERE ce.LearnerID = @LearnerID;


END;
go

CREATE   PROCEDURE GetCourseCompletionStatus
    @LearnerID INT,
    @CourseID INT
AS
BEGIN
    SELECT
        c.Title AS CourseTitle,
        ce.status AS CompletionStatus
    FROM
        Course c
            INNER JOIN
        Course_Prerequisites cp ON cp.PreRequisiteID = c.CourseID
            LEFT JOIN
        Course_enrollment ce ON ce.CourseID = cp.PrerequisiteID AND ce.LearnerID = @LearnerID
    WHERE cp.CourseID = @CourseID;
END;
go

CREATE PROCEDURE GetPrerequisitesStatus
    @CourseID INT,
    @LearnerID INT
AS
BEGIN
    DECLARE @PrerequisitesStatus TABLE (
                                           PrerequisiteCourse NVARCHAR(255),
                                           CompletionStatus NVARCHAR(50)
                                       );

    INSERT INTO @PrerequisitesStatus (PrerequisiteCourse, CompletionStatus)
    SELECT c.Title AS PrerequisiteCourse,
           ISNULL(ce.Status, 'Not Enrolled') AS CompletionStatus
    FROM Course_Prerequisites cp
             LEFT JOIN Course c ON cp.PrerequisiteID = c.CourseID
             LEFT JOIN Course_enrollment ce ON cp.PrerequisiteID = ce.CourseID AND ce.LearnerID = @LearnerID
    WHERE cp.CourseID = @CourseID;

    SELECT PrerequisiteCourse, CompletionStatus FROM @PrerequisitesStatus;
END
go

CREATE PROCEDURE GetQuestParticipants
@QuestID INT
AS
BEGIN
    SELECT l.LearnerID, l.FirstName, l.LastName, lc.completion_status
    FROM LearnerCollaboration lc
             JOIN Learner l ON lc.LearnerID = l.LearnerID
    WHERE lc.QuestID = @QuestID;
END;
go

-- 9. GradeUpdate: Update an assessment grade for a learner.
CREATE   PROCEDURE GradeUpdate (@LearnerID INT, @AssessmentID INT, @points INT)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Taken_assessments WHERE LearnerID = @LearnerID AND AssessmentID = @AssessmentID)
        BEGIN
            UPDATE Taken_assessments
            SET Scored_points = @points
            WHERE LearnerID = @LearnerID AND AssessmentID = @AssessmentID;
            SELECT 'Grade updated successfully' AS Confirmation;
        END
    ELSE
        BEGIN
            SELECT 'Learner has not taken this assessment' AS Confirmation;
        END
END;
go

-- 7. Highestgrade: View the assessment with the highest maximum points for each course.
CREATE   PROCEDURE Highestgrade
AS
BEGIN
    SELECT a1.*
    FROM Assessments a1
    WHERE a1.total_marks = (
        SELECT MAX(a2.total_marks)
        FROM Assessments a2
        WHERE a2.CourseID = a1.CourseID
    );
END;
go

CREATE   PROCEDURE Is_Admin
    @Email NVARCHAR(256),
    @Password NVARCHAR(256),
    @IsAdmin BIT OUTPUT
AS
BEGIN
    SET @IsAdmin = 0;
    IF EXISTS (SELECT * FROM Admin WHERE Email = @Email AND Password = @Password)
        SET @IsAdmin = 1;
END;
go

CREATE   PROCEDURE Is_Instructor
    @Email NVARCHAR(256),
    @Password NVARCHAR(256),
    @IsInstructor BIT OUTPUT
AS
BEGIN
    SET @IsInstructor = 0;
    IF EXISTS (SELECT * FROM Instructor WHERE Email = @Email AND Password = @Password)
        SET @IsInstructor = 1;
END;
go

CREATE   PROCEDURE Is_Learner
    @Email NVARCHAR(256),
    @Password NVARCHAR(256),
    @IsLearner BIT OUTPUT
AS
BEGIN
    SET @IsLearner = 0;
    IF EXISTS (SELECT * FROM Learner WHERE Email = @Email AND Password = @Password)
        SET @IsLearner = 1;
END;
go

CREATE   PROCEDURE Is_Valid_SecretCode
    @Code NVARCHAR(20),
    @IsValid BIT OUTPUT
AS
BEGIN
    SET @IsValid = 0;
    IF EXISTS (SELECT * FROM SecretCodes WHERE Code = @Code)
        SET @IsValid = 1;
END;
go

CREATE   PROCEDURE LearnerInfo (@LearnerID INT)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID)
        BEGIN
            SELECT * FROM PersonalizationProfiles WHERE LearnerID = @LearnerID;
        END
    ELSE
        BEGIN
            SELECT 'Learner not found' AS Message;
        END
END;
go

CREATE   PROCEDURE NewActivity
    @CourseID INT,
    @ModuleID INT,
    @ActivityType VARCHAR(50),
    @InstructionDetails NVARCHAR(MAX),
    @MaxPoints INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Modules WHERE ModuleID = @ModuleID AND CourseID = @CourseID)
        BEGIN
            INSERT INTO Learning_activities (ModuleID, CourseID, ActivityType, InstructionDetails, MaxPoints)
            VALUES (@ModuleID, @CourseID, @ActivityType, @InstructionDetails, @MaxPoints);
        END
    ELSE
        BEGIN
            PRINT 'Invalid ModuleID or CourseID.';
        END
END;
go

-- 11. NewGoal: Define a new learning goal for learners.
CREATE   PROCEDURE NewGoal (@GoalID INT, @Status VARCHAR(MAX), @Deadline DATETIME, @Description VARCHAR(MAX))
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Learning_goal WHERE ID = @GoalID)
        BEGIN
            INSERT INTO Learning_goal (ID, Status, deadline, Description)
            VALUES (@GoalID, @Status, @Deadline, @Description);
        END
    ELSE
        BEGIN
            PRINT 'GoalID already exists.';
        END
END;
go

-- 5. NewPath: Add a new learning path for a learner.
CREATE   PROCEDURE NewPath
(@LearnerID INT, @ProfileID INT, @CompletionStatus VARCHAR(50), @CustomContent VARCHAR(MAX), @AdaptiveRules VARCHAR(MAX))
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Learner WHERE LearnerID = @LearnerID)
        BEGIN
            INSERT INTO Learning_path
            VALUES (@LearnerID, @ProfileID, @CompletionStatus, @CustomContent, @AdaptiveRules);
        END
    ELSE
        BEGIN
            PRINT 'Learner not found.';
        END
END;
go

CREATE   PROCEDURE Prerequisites
    @LearnerID INT,
    @CourseID INT
    ,@ResultMessage varchar(max) OUTPUT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Course WHERE CourseID = @CourseID)
        BEGIN
            IF EXISTS (SELECT 1 FROM Course_enrollment WHERE LearnerID = @LearnerID AND CourseID = @CourseID)
                BEGIN
                    select    @ResultMessage= '  You already registered for this course.';
                END
            ELSE
                BEGIN
                    DECLARE @UnmetPrereqsCount INT;

                    SELECT @UnmetPrereqsCount = COUNT(*)
                    FROM Course_Prerequisites cp
                             LEFT JOIN Course_enrollment ce ON cp.PrerequisiteID = ce.CourseID AND ce.LearnerID = @LearnerID
                    WHERE cp.CourseID = @CourseID AND (ce.CourseID IS NULL OR ce.status != 'completed');

                    IF @UnmetPrereqsCount > 0
                        select  @ResultMessage= 'Not all prerequisites are completed.';
                    ELSE
                        select    @ResultMessage= 'All prerequisites are completed.';
                END
        END
    ELSE
        BEGIN
            select    @ResultMessage= 'Error: CourseID does not exist.';
        END
END;
go

CREATE   PROCEDURE ProfileUpdate
    @learnerID INT,
    @ProfileID INT,
    @PreferedContentType VARCHAR(50),
    @emotional_state VARCHAR(50),
    @PersonalityType VARCHAR(50)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM PersonalizationProfiles WHERE LearnerID = @learnerID AND ProfileID = @ProfileID)
        BEGIN
            UPDATE PersonalizationProfiles
            SET preferred_content_type = @PreferedContentType,
                emotional_state = @emotional_state,
                personality_type = @PersonalityType
            WHERE LearnerID = @learnerID AND ProfileID = @ProfileID;
        END
    ELSE
        BEGIN
            PRINT 'Error: LearnerID or ProfileID does not exist.';
        END
END;
go

CREATE   PROCEDURE ViewScore
    @LearnerID INT,
    @AssessmentID INT,
    @score INT OUTPUT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Taken_assessments WHERE LearnerID = @LearnerID AND AssessmentID = @AssessmentID)
        BEGIN
            SELECT @score = Scored_points
            FROM Taken_assessments
            WHERE LearnerID = @LearnerID AND AssessmentID = @AssessmentID;
        END
    ELSE
        BEGIN
            PRINT 'Error: LearnerID or AssessmentID does not exist.';
        END
END;
go
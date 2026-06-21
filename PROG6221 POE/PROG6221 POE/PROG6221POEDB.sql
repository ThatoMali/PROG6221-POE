-- Create Database
BEGIN
    CREATE DATABASE SecureCoreDB;
    PRINT 'Database SecureCoreDB created successfully.';
END

BEGIN
    PRINT 'Database SecureCoreDB already exists.';
END
GO

-- Use the database
USE SecureCoreDB;
GO

-- Create Tasks Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'tasks')
BEGIN
    CREATE TABLE tasks (
        id INT IDENTITY(1,1) PRIMARY KEY,
        title NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX) NULL,
        reminder_date DATETIME NULL,
        is_completed BIT NOT NULL DEFAULT 0,
        created_at DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table ''tasks'' created successfully.';
END

BEGIN
    PRINT 'Table ''tasks'' already exists.';
END
GO

-- Insert Sample Data
IF NOT EXISTS (SELECT TOP 1 * FROM tasks)
BEGIN
    INSERT INTO tasks (title, description, reminder_date) VALUES
    ('Enable Two-Factor Authentication', 
     'Set up 2FA on all major accounts including email, banking, and social media.', 
     DATEADD(DAY, 7, GETDATE())),
    
    ('Review Privacy Settings', 
     'Check and update privacy settings on social media platforms and online accounts.', 
     DATEADD(DAY, 3, GETDATE())),
    
    ('Update Passwords', 
     'Change passwords for banking, email, and other sensitive accounts. Use strong unique passwords.', 
     DATEADD(DAY, 14, GETDATE())),
    
    ('Install Antivirus Software', 
     'Install and configure reliable antivirus software on all devices.', 
     DATEADD(DAY, 5, GETDATE())),
    
    ('Backup Important Data', 
     'Create a backup of all important files to external drive or cloud storage.', 
     DATEADD(DAY, 10, GETDATE()));
    
    PRINT 'Sample data inserted successfully.';
END
ELSE
BEGIN
    PRINT 'Sample data already exists.';
END
GO

-- Create Indexes for Performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_tasks_reminder_date')
BEGIN
    CREATE INDEX idx_tasks_reminder_date ON tasks(reminder_date);
    PRINT 'Index idx_tasks_reminder_date created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_tasks_is_completed')
BEGIN
    CREATE INDEX idx_tasks_is_completed ON tasks(is_completed);
    PRINT 'Index idx_tasks_is_completed created successfully.';
END
GO

-- Create View for Active Tasks
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'v_active_tasks')
BEGIN
    EXEC('
    CREATE VIEW v_active_tasks AS
    SELECT 
        id,
        title,
        description,
        reminder_date,
        created_at,
        CASE 
            WHEN reminder_date IS NOT NULL AND reminder_date <= GETDATE() THEN ''OVERDUE''
            WHEN reminder_date IS NOT NULL THEN ''PENDING''
            ELSE ''NO REMINDER''
        END AS status,
        DATEDIFF(DAY, GETDATE(), reminder_date) AS days_until_reminder
    FROM tasks
    WHERE is_completed = 0
    ');
    PRINT 'View v_active_tasks created successfully.';
END
GO

-- Create Stored Procedure to Get Task Statistics
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetTaskStatistics')
    DROP PROCEDURE GetTaskStatistics;
GO

CREATE PROCEDURE GetTaskStatistics
AS
BEGIN
    SELECT 
        COUNT(*) AS total_tasks,
        SUM(CASE WHEN is_completed = 0 THEN 1 ELSE 0 END) AS pending_tasks,
        SUM(CASE WHEN is_completed = 1 THEN 1 ELSE 0 END) AS completed_tasks,
        SUM(CASE WHEN reminder_date IS NOT NULL AND is_completed = 0 THEN 1 ELSE 0 END) AS tasks_with_reminders,
        SUM(CASE WHEN reminder_date IS NOT NULL AND reminder_date <= GETDATE() AND is_completed = 0 THEN 1 ELSE 0 END) AS overdue_tasks
    FROM tasks;
END
GO

PRINT 'Stored procedure GetTaskStatistics created successfully.';
GO

-- Create Stored Procedure to Clean Up Old Tasks
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'CleanOldTasks')
    DROP PROCEDURE CleanOldTasks;
GO

CREATE PROCEDURE CleanOldTasks
    @daysToKeep INT = 30
AS
BEGIN
    DELETE FROM tasks 
    WHERE is_completed = 1 
    AND created_at < DATEADD(DAY, -@daysToKeep, GETDATE());
    
    SELECT @@ROWCOUNT AS tasks_deleted;
END
GO

PRINT 'Stored procedure CleanOldTasks created successfully.';
GO

-- Display summary
SELECT '=== Database Setup Complete ===' AS Status;
SELECT COUNT(*) AS TotalTables FROM sys.tables WHERE type = 'U';
SELECT COUNT(*) AS TotalViews FROM sys.views WHERE type = 'V';
SELECT COUNT(*) AS TotalStoredProcedures FROM sys.procedures;
GO

-- Show initial data
SELECT * FROM tasks;
GO
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PROG6221_POE.Services
{
    public class DatabaseHelper : IDisposable
    {
        private string _connectionString;
        private SqlConnection _connection;

        public DatabaseHelper()
        {
            // Use the provided connection string
            _connectionString = @"Data Source=localhost\SQLEXPRESS01;Initial Catalog=""PROG6221POEDB"";Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                // First, create the database if it doesn't exist
                CreateDatabaseIfNotExists();

                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                System.Diagnostics.Debug.WriteLine("Database connection established successfully to PROG6221POEDB.");
                CreateTablesIfNotExists();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init error: {ex.Message}");
                throw;
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                // Use master database to check if PROG6221POEDB exists
                string masterConnectionString = @"Data Source=localhost\SQLEXPRESS01;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

                using (var conn = new SqlConnection(masterConnectionString))
                {
                    conn.Open();
                    string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = 'PROG6221POEDB'";
                    using (var cmd = new SqlCommand(checkDbQuery, conn))
                    {
                        int dbCount = (int)cmd.ExecuteScalar();
                        if (dbCount == 0)
                        {
                            string createDbQuery = "CREATE DATABASE PROG6221POEDB";
                            using (var createCmd = new SqlCommand(createDbQuery, conn))
                            {
                                createCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine("Database PROG6221POEDB created successfully.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateDatabase error: {ex.Message}");
                throw;
            }
        }

        private void CreateTablesIfNotExists()
        {
            try
            {
                string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tasks')
                    BEGIN
                        CREATE TABLE tasks (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            title NVARCHAR(255) NOT NULL,
                            description NVARCHAR(MAX) NULL,
                            reminder_date DATETIME NULL,
                            is_completed BIT NOT NULL DEFAULT 0,
                            created_at DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END";

                using (var cmd = new SqlCommand(createTableQuery, _connection))
                {
                    cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Table 'tasks' created/verified successfully.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateTables error: {ex.Message}");
                throw;
            }
        }

        public bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                string query = @"INSERT INTO tasks (title, description, reminder_date) 
                                VALUES (@title, @description, @reminderDate)";

                using (var cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description ?? "");
                    cmd.Parameters.AddWithValue("@reminderDate", reminderDate ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"AddTask result: {result} rows affected");
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTask error: {ex.Message}");
                return false;
            }
        }

        public List<TaskItem> GetTasks(bool includeCompleted = false)
        {
            var tasks = new List<TaskItem>();
            try
            {
                string query = includeCompleted ?
                    "SELECT * FROM tasks ORDER BY is_completed, reminder_date" :
                    "SELECT * FROM tasks WHERE is_completed = 0 ORDER BY reminder_date";

                using (var cmd = new SqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Title = reader.GetString(reader.GetOrdinal("title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
                            ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("reminder_date")),
                            IsCompleted = reader.GetBoolean(reader.GetOrdinal("is_completed")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                        });
                    }
                }
                System.Diagnostics.Debug.WriteLine($"GetTasks returned {tasks.Count} tasks");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTasks error: {ex.Message}");
            }
            return tasks;
        }

        public bool MarkTaskComplete(int taskId)
        {
            try
            {
                string query = "UPDATE tasks SET is_completed = 1 WHERE id = @id";
                using (var cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"MarkTaskComplete: {result} rows affected");
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkTaskComplete error: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            try
            {
                string query = "DELETE FROM tasks WHERE id = @id";
                using (var cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"DeleteTask: {result} rows affected");
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteTask error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTask(int taskId, string title, string description, DateTime? reminderDate)
        {
            try
            {
                string query = @"UPDATE tasks 
                                SET title = @title, 
                                    description = @description, 
                                    reminder_date = @reminderDate 
                                WHERE id = @id";

                using (var cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description ?? "");
                    cmd.Parameters.AddWithValue("@reminderDate", reminderDate ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"UpdateTask: {result} rows affected");
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTask error: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Status => IsCompleted ? "Completed" : "Pending";
        public string Reminder => ReminderDate?.ToString("yyyy-MM-dd HH:mm") ?? "No reminder";
        public bool IsOverdue => !IsCompleted && ReminderDate.HasValue && ReminderDate.Value <= DateTime.Now;
    }
}
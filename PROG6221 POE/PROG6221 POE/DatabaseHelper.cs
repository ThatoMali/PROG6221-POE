using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace PROG6221_POE
{
    public class DatabaseHelper : IDisposable
    {
        private string _connectionString;
        private MySqlConnection _connection;

        public DatabaseHelper()
        {
            // Update with your MySQL credentials
            _connectionString = "Data Source=localhost\\SQLEXPRESS01;Initial Catalog=\"PROG6221POEDB\";Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS tasks (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(255) NOT NULL,
                        description TEXT,
                        reminder_date DATETIME,
                        is_completed BOOLEAN DEFAULT FALSE,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";

                using (var cmd = new MySqlCommand(createTableQuery, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init error: {ex.Message}");
            }
        }

        public bool AddTask(string title, string description, DateTime? reminderDate)
        {
            try
            {
                string query = @"INSERT INTO tasks (title, description, reminder_date) 
                                VALUES (@title, @description, @reminderDate)";
                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description ?? "");
                    cmd.Parameters.AddWithValue("@reminderDate", reminderDate ?? (object)DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
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
                    "SELECT * FROM tasks WHERE is_completed = FALSE ORDER BY reminder_date";

                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                            ReminderDate = reader.IsDBNull(reader.GetOrdinal("reminder_date")) ? null : reader.GetDateTime("reminder_date"),
                            IsCompleted = reader.GetBoolean("is_completed"),
                            CreatedAt = reader.GetDateTime("created_at")
                        });
                    }
                }
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
                string query = "UPDATE tasks SET is_completed = TRUE WHERE id = @id";
                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    return cmd.ExecuteNonQuery() > 0;
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
                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteTask error: {ex.Message}");
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

        public string Status => IsCompleted ? "✅ Completed" : "⏳ Pending";
        public string Reminder => ReminderDate?.ToString("yyyy-MM-dd HH:mm") ?? "No reminder";
    }
}
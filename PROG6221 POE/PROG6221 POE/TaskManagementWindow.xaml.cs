using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PROG6221_POE.Services;

namespace PROG6221_POE
{
    public partial class TaskManagementWindow : Window
    {
        private DatabaseHelper _dbHelper;
        private List<TaskItem> _tasks;

        public TaskManagementWindow()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            LoadTasks();
        }

        private void LoadTasks()
        {
            _tasks = _dbHelper.GetTasks(true);
            lstTasks.ItemsSource = _tasks;
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTaskTitle.Text.Trim();
            string description = txtTaskDescription.Text.Trim();

            if (string.IsNullOrEmpty(title) || title == "Enter task title...")
            {
                MessageBox.Show("Please enter a task title.", "Validation",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(description) || description == "Enter description...")
                description = "";

            if (_dbHelper.AddTask(title, description, null))
            {
                MessageBox.Show("Task added successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                txtTaskTitle.Text = "Enter task title...";
                txtTaskDescription.Text = "Enter description...";
                LoadTasks();
            }
            else
            {
                MessageBox.Show("Failed to add task. Please try again.", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int taskId)
            {
                if (_dbHelper.MarkTaskComplete(taskId))
                {
                    MessageBox.Show("Task marked as complete!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTasks();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int taskId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this task?",
                                           "Confirm Delete", MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (_dbHelper.DeleteTask(taskId))
                    {
                        MessageBox.Show("Task deleted successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTasks();
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _dbHelper?.Dispose();
            base.OnClosing(e);
        }
    }
}
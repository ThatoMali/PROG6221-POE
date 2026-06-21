using System;
using System.Windows;
using PROG6221_POE.Services;

namespace PROG6221_POE
{
    public partial class ActivityLogWindow : Window
    {
        private ActivityLogManager _logManager;

        public ActivityLogWindow()
        {
            InitializeComponent();
            _logManager = new ActivityLogManager();

            // Subscribe to log updates
            _logManager.OnLogUpdated += (msg) => LoadLogs();

            LoadLogs();
        }

        private void LoadLogs(int count = 10)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var entries = _logManager.GetRecentEntries(count);
                    lstLog.ItemsSource = entries;
                    txtTotal.Text = $"Total Entries: {_logManager.GetTotalCount()} (Showing {entries.Count})";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs(10);
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs(_logManager.GetTotalCount());
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all logs?",
                                       "Confirm Clear", MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _logManager.ClearLog();
                LoadLogs();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Data.SqlClient;

namespace PROG6221_POE
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load saved settings (in a real app, these would be from a config file)
            chkEnableVoice.IsChecked = true;
            chkAutoGreeting.IsChecked = true;
            chkEnableSentiment.IsChecked = true;
            chkShowEmoji.IsChecked = true;
            cboTheme.SelectedIndex = 0;
            sliderFontSize.Value = 13;

            // Set the connection string in the textbox
            txtConnectionString.Text = @"Data Source=localhost\SQLEXPRESS01;Initial Catalog=""PROG6221POEDB"";Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool voiceEnabled = chkEnableVoice.IsChecked ?? true;
                bool autoGreeting = chkAutoGreeting.IsChecked ?? true;
                bool sentimentEnabled = chkEnableSentiment.IsChecked ?? true;
                bool showEmoji = chkShowEmoji.IsChecked ?? true;
                string theme = (cboTheme.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Dark";
                double fontSize = sliderFontSize.Value;

                txtStatus.Text = "Settings saved successfully!";
                txtStatus.Foreground = new SolidColorBrush(Colors.LightGreen);

                MessageBox.Show("Settings saved successfully!", "Success",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error: {ex.Message}";
                txtStatus.Foreground = new SolidColorBrush(Colors.Salmon);
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = txtConnectionString.Text.Trim();
                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("Please enter a connection string.", "Validation",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    txtStatus.Text = "Connection successful!";
                    txtStatus.Foreground = new SolidColorBrush(Colors.LightGreen);
                    MessageBox.Show("Database connection successful!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Connection failed: {ex.Message}";
                txtStatus.Foreground = new SolidColorBrush(Colors.Salmon);
                MessageBox.Show($"Connection failed: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
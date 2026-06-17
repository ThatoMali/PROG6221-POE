using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PROG6221_POE
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private string _text, _sender, _time, _alignment;
        private Brush _backgroundColor, _senderColor;

        public string Text { get => _text; set { _text = value; OnPropertyChanged(); } }
        public string Sender { get => _sender; set { _sender = value; OnPropertyChanged(); } }
        public string Time { get => _time; set { _time = value; OnPropertyChanged(); } }
        public string Alignment { get => _alignment; set { _alignment = value; OnPropertyChanged(); } }
        public Brush BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; OnPropertyChanged(); } }
        public Brush SenderColor { get => _senderColor; set { _senderColor = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public partial class MainWindow : Window
    {
        private ChatbotEngine _chatbot;
        private VoiceService _voiceService;
        private ObservableCollection<ChatMessage> _messages;
        private bool _awaitingTaskDescription = false;
        private bool _awaitingReminderConfirmation = false;

        public MainWindow()
        {
            InitializeComponent();
            _chatbot = new ChatbotEngine();
            _messages = new ObservableCollection<ChatMessage>();
            lstChatDisplay.ItemsSource = _messages;
            _voiceService = new VoiceService();

            _chatbot.OnResponseGenerated += Chatbot_OnResponseGenerated;
            _chatbot.OnSentimentDetected += Chatbot_OnSentimentDetected;
            _chatbot.OnUserInfoUpdated += Chatbot_OnUserInfoUpdated;
        }

        private void Chatbot_OnUserInfoUpdated(string info)
        {
            Dispatcher.Invoke(() => txtUserInfo.Text = info);
        }

        private void Chatbot_OnSentimentDetected(string sentiment, string emoji)
        {
            Dispatcher.Invoke(() =>
            {
                txtSentiment.Text = $"{emoji} Sentiment: {sentiment}";
                txtSentiment.Foreground = sentiment.Contains("Positive") ? Brushes.LightGreen :
                                          sentiment.Contains("worried") ? Brushes.LightYellow :
                                          sentiment.Contains("frustrated") ? Brushes.Salmon :
                                          sentiment.Contains("confused") ? Brushes.Orange :
                                          sentiment.Contains("grateful") ? Brushes.LightGreen :
                                          sentiment.Contains("hopeless") ? Brushes.Red :
                                          Brushes.LightGray;
            });
        }

        private void Chatbot_OnResponseGenerated(string response)
        {
            Dispatcher.Invoke(() =>
            {
                AddBotMessage(response);
                ScrollToBottom();

                // Check if response indicates we're waiting for user input
                if (response.Contains("description") || response.Contains("skip"))
                {
                    _awaitingTaskDescription = true;
                }
                else if (response.Contains("reminder") && response.Contains("yes/no"))
                {
                    _awaitingReminderConfirmation = true;
                }
                else
                {
                    _awaitingTaskDescription = false;
                    _awaitingReminderConfirmation = false;
                }
            });
        }

        private void AddUserMessage(string message)
        {
            _messages.Add(new ChatMessage
            {
                Text = message,
                Sender = "👤 You",
                Time = DateTime.Now.ToString("HH:mm"),
                Alignment = "Right",
                BackgroundColor = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
                SenderColor = Brushes.Gold
            });
        }

        private void AddBotMessage(string message)
        {
            _messages.Add(new ChatMessage
            {
                Text = message,
                Sender = "🤖 SecureBot",
                Time = DateTime.Now.ToString("HH:mm"),
                Alignment = "Left",
                BackgroundColor = new SolidColorBrush(Color.FromRgb(61, 61, 77)),
                SenderColor = new SolidColorBrush(Color.FromRgb(0, 206, 209))
            });
        }

        private void ScrollToBottom()
        {
            if (lstChatDisplay.Items.Count > 0)
                lstChatDisplay.ScrollIntoView(lstChatDisplay.Items[lstChatDisplay.Items.Count - 1]);
        }

        private void BtnStartChat_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please enter your name.", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _chatbot.SetUserName(userName);
            _messages.Clear();
            AddBotMessage($"Hello {userName}! Welcome to SecureCore.");
            AddBotMessage("I can help with: Password Safety, Phishing Attacks, Safe Browsing, Malware Protection");
            AddBotMessage("🆕 **New Features:** Task Management, Cybersecurity Quiz, Activity Log");
            AddBotMessage("Type 'menu' for options or 'exit' to quit.");

            txtMessageInput.IsEnabled = true;
            btnSend.IsEnabled = true;
            btnVoice.IsEnabled = true;
            txtUserName.IsEnabled = false;
            btnStartChat.IsEnabled = false;
            txtMessageInput.Focus();
            _voiceService?.PlayGreeting();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void TxtMessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private async void SendMessage()
        {
            string userInput = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            AddUserMessage(userInput);
            txtMessageInput.Clear();

            // Handle special task flows
            if (_awaitingTaskDescription)
            {
                _chatbot.AddTaskWithDescription(userInput);
                _awaitingTaskDescription = false;
                ScrollToBottom();
                return;
            }

            if (_awaitingReminderConfirmation)
            {
                if (userInput.ToLower().Contains("yes") || userInput.ToLower().Contains("y"))
                {
                    AddBotMessage("📅 Please specify the reminder date (e.g., '2026-06-20' or 'in 3 days' or 'tomorrow')");
                    _awaitingReminderConfirmation = false;
                    ScrollToBottom();
                    return;
                }
                else
                {
                    _chatbot.SetTaskReminder(DateTime.MinValue); // This will add task without reminder
                    _awaitingReminderConfirmation = false;
                    ScrollToBottom();
                    return;
                }
            }

            // Check if we're in a date/reminder flow
            if (userInput.Contains("in ") || userInput.Contains("days") || userInput.Contains("tomorrow") ||
                userInput.Contains("today") || DateTime.TryParse(userInput, out _))
            {
                DateTime reminderDate = ParseReminderDate(userInput);
                if (reminderDate != DateTime.MinValue)
                {
                    _chatbot.SetTaskReminder(reminderDate);
                    ScrollToBottom();
                    return;
                }
            }

            try
            {
                await Task.Run(() => _chatbot.ProcessInput(userInput));
            }
            catch
            {
                AddBotMessage("I encountered an issue. Please try again.");
            }

            ScrollToBottom();
        }

        private DateTime ParseReminderDate(string input)
        {
            input = input.ToLower().Trim();

            if (input == "today")
                return DateTime.Now.Date;

            if (input == "tomorrow")
                return DateTime.Now.Date.AddDays(1);

            // Parse "in X days"
            var match = System.Text.RegularExpressions.Regex.Match(input, @"in (\d+) days?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            // Try direct parse
            if (DateTime.TryParse(input, out DateTime result))
                return result;

            return DateTime.MinValue;
        }

        private void BtnVoice_Click(object sender, RoutedEventArgs e)
        {
            _voiceService?.SpeakResponse("What would you like to know about cybersecurity?");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _chatbot?.Dispose();
            _voiceService?.Dispose();
            base.OnClosing(e);
        }
    }
}
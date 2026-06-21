using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PROG6221_POE.Services;

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
        private int _messageCount = 0;

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

            // Set focus to name input on startup
            this.Loaded += (s, e) => txtUserName.Focus();
        }

        #region Event Handlers

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
            });
        }

        #endregion

        #region UI Methods

        private void AddUserMessage(string message)
        {
            _messageCount++;
            UpdateMessageCount();

            _messages.Add(new ChatMessage
            {
                Text = message,
                Sender = "You",
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
                Sender = "SecureBot",
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

        private void UpdateMessageCount()
        {
            Dispatcher.Invoke(() =>
            {
                txtMessageCount.Text = $"Messages: {_messageCount}";
            });
        }

        #endregion

        #region Button Click Handlers

        private void BtnStartChat_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please enter your name.", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
                txtUserName.Focus();
                return;
            }

            _chatbot.SetUserName(userName);
            _messages.Clear();
            _messageCount = 0;

            AddBotMessage($"Hello {userName}! Welcome to SecureCore.");
            AddBotMessage("I can help with: Password Safety, Phishing Attacks, Safe Browsing, Malware Protection");
            AddBotMessage("New Features: Task Management, Cybersecurity Quiz, Activity Log");
            AddBotMessage("Type 'menu' for options or 'exit' to quit.");
            AddBotMessage("You can also use the toolbar buttons above for quick access to features.");

            txtMessageInput.IsEnabled = true;
            btnSend.IsEnabled = true;
            btnVoice.IsEnabled = true;
            btnClearChat.IsEnabled = true;
            txtUserName.IsEnabled = false;
            btnStartChat.IsEnabled = false;
            txtMessageInput.Focus();
            _voiceService?.PlayGreeting();

            UpdateMessageCount();
        }

        private void BtnClearChat_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Clear all chat messages?", "Confirm",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _messages.Clear();
                _messageCount = 0;
                UpdateMessageCount();

                AddBotMessage("Chat cleared. Type 'menu' for options or continue chatting.");
                ScrollToBottom();
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void BtnVoice_Click(object sender, RoutedEventArgs e)
        {
            _voiceService?.SpeakResponse("What would you like to know about cybersecurity?");
        }

        #endregion

        #region Navigation Button Handlers

        private void BtnOpenTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var taskWindow = new TaskManagementWindow();
                taskWindow.Owner = this;
                taskWindow.ShowDialog();

                AddBotMessage("Task management window closed. You can continue chatting.");
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Task Management: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenQuiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var quizWindow = new QuizWindow();
                quizWindow.Owner = this;
                quizWindow.ShowDialog();

                AddBotMessage("Quiz window closed. You can continue chatting.");
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quiz: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logWindow = new ActivityLogWindow();
                logWindow.Owner = this;
                logWindow.ShowDialog();

                AddBotMessage("Activity log window closed. You can continue chatting.");
                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Activity Log: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Message Sending

        private void TxtMessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private async void SendMessage()
        {
            string userInput = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            AddUserMessage(userInput);
            txtMessageInput.Clear();

            

            try
            {
                await Task.Run(() => _chatbot.ProcessInput(userInput));
            }
            catch (Exception ex)
            {
                AddBotMessage($"I encountered an issue: {ex.Message}. Please try again.");
            }

            ScrollToBottom();
        }

        #endregion

        #region Window Events

        protected override void OnClosing(CancelEventArgs e)
        {
            _chatbot?.Dispose();
            _voiceService?.Dispose();
            base.OnClosing(e);
        }

        #endregion
    }
}
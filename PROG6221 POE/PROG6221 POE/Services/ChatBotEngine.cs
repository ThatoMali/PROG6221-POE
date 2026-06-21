using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PROG6221_POE.Services;

namespace PROG6221_POE
{
    public class ChatbotEngine : IDisposable
    {
        public event Action<string> OnResponseGenerated;
        public event Action<string, string> OnSentimentDetected;
        public event Action<string> OnUserInfoUpdated;

        private DatabaseHelper _dbHelper;
        private QuizManager _quizManager;
        private ActivityLogManager _activityLog;
        private string _userName;
        private string _favoriteTopic;
        private int _messageCount = 0;
        private bool _awaitingFollowUp = false;
        private string _currentTopic = null;
        private bool _quizMode = false;
        private string _pendingTaskTitle = null;
        private bool _awaitingQuizAnswer = false;
        private string _pendingTaskDescription = null;
        private bool _awaitingReminder = false;

        public bool IsAwaitingFollowUp => _awaitingFollowUp;
        public string LastTopic => _currentTopic;
        public bool IsQuizMode => _quizMode;

        public ChatbotEngine()
        {
            InitializeDictionaries();
            _dbHelper = new DatabaseHelper();
            _quizManager = new QuizManager();
            _activityLog = new ActivityLogManager();

            // Subscribe to quiz events
            _quizManager.OnQuestionDisplayed += (q) =>
            {
                _awaitingQuizAnswer = true;
                OnResponseGenerated?.Invoke(q);
            };

            _quizManager.OnAnswerFeedback += (correct, explanation) =>
            {
                string feedback = correct ? "Correct! " : "Incorrect. ";
                OnResponseGenerated?.Invoke(feedback + explanation);
                _activityLog.AddEntry("Quiz Answer", $"{(correct ? "Correct" : "Incorrect")}");
            };

            _quizManager.OnQuizComplete += (score, feedback) =>
            {
                _quizMode = false;
                _awaitingQuizAnswer = false;
                OnResponseGenerated?.Invoke($"Quiz Complete!\n\n{feedback}");
                _activityLog.AddEntry("Quiz Complete", $"Score: {score}/{_quizManager.TotalQuestions}");
            };
        }

        private void InitializeDictionaries()
        {
            _keywordMap = new Dictionary<string, List<string>>
            {
                { "password", new List<string> { "password", "pass", "login", "credentials", "account" } },
                { "phishing", new List<string> { "phish", "scam", "fraud", "fake email", "spoof" } },
                { "privacy", new List<string> { "privacy", "personal data", "data protection", "gdpr" } },
                { "browsing", new List<string> { "browse", "browser", "website", "http", "https", "url" } },
                { "malware", new List<string> { "malware", "virus", "ransomware", "trojan", "spyware" } },
                { "task", new List<string> { "task", "todo", "to-do", "add task", "new task" } },
                { "reminder", new List<string> { "remind", "reminder", "remember" } },
                { "quiz", new List<string> { "quiz", "game", "test", "challenge", "knowledge" } },
                { "log", new List<string> { "log", "history", "activity", "what have you done" } }
            };

            _responsePool = new Dictionary<string, List<string>>
            {
                { "password", new List<string> {
                    "Use a password manager to generate and store unique passwords for each account!",
                    "Enable two-factor authentication whenever possible - it adds an extra layer of security!",
                    "Create passwords with at least 12 characters using uppercase, lowercase, numbers, and symbols!",
                    "Never reuse passwords across different accounts!",
                    "Avoid using personal information like birthdays or pet names in your passwords!"
                }},
                { "phishing", new List<string> {
                    "Always check the sender's email address carefully - scammers use slight variations!",
                    "Hover over links before clicking to see the actual destination URL!",
                    "Legitimate companies never ask for passwords via email!",
                    "Look for spelling errors and urgent language - common phishing tactics!",
                    "When in doubt, contact the company directly using their official website!"
                }},
                { "privacy", new List<string> {
                    "Review app permissions regularly - remove access for apps you don't use!",
                    "Use a VPN when on public Wi-Fi to encrypt your internet traffic!",
                    "Disable location tracking for apps that don't need it!",
                    "Regularly check 'Have I Been Pwned' to see if your data was compromised!",
                    "Use aliases or temporary emails for newsletter signups!"
                }},
                { "browsing", new List<string> {
                    "Look for the padlock icon in the address bar - it means the site uses HTTPS encryption!",
                    "Clear your browser cache and cookies regularly!",
                    "Keep your browser updated for the latest security patches!",
                    "Avoid saving passwords in your browser - use a password manager instead!",
                    "Install reputable ad-blockers and script blockers!"
                }},
                { "malware", new List<string> {
                    "Always download software from official sources, not third-party sites!",
                    "Keep your antivirus software updated and run regular scans!",
                    "Be cautious with email attachments - even from known senders!",
                    "Enable automatic updates for your operating system!",
                    "Back up important data regularly to an external drive or cloud!"
                }}
            };

            _generalTips = new List<string>
            {
                "Keep your software updated - security patches fix known vulnerabilities!",
                "Use unique passwords for every account - credential stuffing attacks are common!",
                "Think before you click - social engineering is the #1 attack vector!",
                "Regular backups can save you from ransomware attacks!",
                "Be skeptical of unsolicited calls asking for personal information!",
                "Lock your computer when stepping away - even for a minute!",
                "Use secure Wi-Fi - public networks can be easily intercepted!",
                "Monitor your bank statements for unauthorized transactions!"
            };

            _sentimentKeywords = new Dictionary<string, string[]>
            {
                { "worried", new string[] { "worried", "concerned", "anxious", "nervous", "scared", "fear", "unsafe", "vulnerable" } },
                { "frustrated", new string[] { "frustrated", "annoyed", "irritated", "fed up", "tired of", "sick of", "exhausted", "stressed" } },
                { "curious", new string[] { "curious", "interested", "want to learn", "tell me", "explain", "how does", "what is", "why do" } },
                { "confused", new string[] { "confused", "don't understand", "unclear", "what does it mean", "too complicated", "complex" } },
                { "grateful", new string[] { "thank", "thanks", "appreciate", "helpful", "useful", "great info", "good to know", "awesome" } },
                { "hopeless", new string[] { "hopeless", "no hope", "useless", "pointless", "why bother", "won't help", "never work", "impossible" } }
            };

            _empatheticResponses = new Dictionary<string, List<string>>
            {
                { "worried", new List<string> {
                    "I completely understand your concern. Many people feel worried about cybersecurity - it's normal! Let me help you feel more secure.",
                    "Your worry is valid. Online threats can be scary, but I'm here to help you stay safe. Here's something practical you can do:",
                    "It's okay to feel worried. The good news is that there are simple steps you can take to protect yourself. Let me share one:",
                    "Feeling concerned is the first step toward better security! Let me give you a tip that will help ease your mind."
                }},
                { "frustrated", new List<string> {
                    "I hear your frustration! Cybersecurity can feel overwhelming at times. Let me simplify this for you:",
                    "It's completely normal to feel frustrated. Let me break this down into something simple you can do right now:",
                    "I understand this can be frustrating. Instead of trying to do everything, let's focus on one small, important step:",
                    "Your frustration is understandable. Let me give you one simple tip that makes a big difference:"
                }},
                { "curious", new List<string> {
                    "That's a great question! I'm glad you're curious about staying safe online. Here's what you should know:",
                    "I love your curiosity! Learning about cybersecurity is the best way to stay protected. Let me explain:",
                    "Great question! Being curious about security is how we all stay safer. Here's the answer:",
                    "I'm happy you asked! Curiosity about cybersecurity is the first step to becoming more secure."
                }},
                { "confused", new List<string> {
                    "I can see this is confusing. Let me explain it more simply:",
                    "Don't worry - cybersecurity terms can be confusing. Let me put this in plain English:",
                    "I understand this is complex. Let me break it down into simple steps:",
                    "You're not alone in finding this confusing. Let me explain it differently:"
                }},
                { "grateful", new List<string> {
                    "You're very welcome! I'm glad I could help. Staying safe online is important. Here's another tip you might like:",
                    "Thank you! I'm happy to help. Since you found that useful, here's something else to keep in mind:",
                    "My pleasure! Knowledge is power when it comes to security. Let me share another helpful tip:",
                    "I appreciate your kind words! Here's another cybersecurity tip you might find valuable:"
                }},
                { "hopeless", new List<string> {
                    "I hear that you're feeling hopeless. Please don't give up - even small steps make a big difference! Let's start with something simple:",
                    "I understand it can feel hopeless when there's so much to worry about. But you don't have to do everything at once. Try this one thing:",
                    "Don't lose hope! Every security expert started somewhere. Let's focus on one small, manageable step:",
                    "I know cybersecurity can feel overwhelming, but you're already taking the right step by learning. Here's an easy win for you:"
                }}
            };
        }

        public void SetUserName(string name)
        {
            _userName = name;
            OnUserInfoUpdated?.Invoke($"User: {name}");
            _activityLog.AddEntry("User Logged In", name);
        }

        public void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            _messageCount++;
            string lowerInput = input.ToLower().Trim();

            // ============================================================
            // QUIZ HANDLING - MUST BE FIRST!
            // ============================================================

            // Check for stop quiz command
            if (lowerInput.Contains("stop quiz") || lowerInput.Contains("end quiz") || lowerInput.Contains("quit quiz"))
            {
                if (_quizMode)
                {
                    _quizManager.StopQuiz();
                    _quizMode = false;
                    _awaitingQuizAnswer = false;
                    OnResponseGenerated?.Invoke("Quiz stopped. You can start a new quiz anytime by typing 'start quiz'.");
                    _activityLog.AddEntry("Quiz Stopped", "User ended the quiz early");
                    return;
                }
                else
                {
                    OnResponseGenerated?.Invoke("There is no active quiz to stop. Type 'start quiz' to begin a new quiz.");
                    return;
                }
            }

            // Check if we're in quiz mode
            if (_quizMode)
            {
                if (_awaitingQuizAnswer)
                {
                    if (IsValidQuizAnswer(input))
                    {
                        _quizManager.SubmitAnswer(input);
                        return;
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke("Please answer with A, B, C, D or 1, 2, 3, 4. Type 'stop quiz' to end the quiz.");
                        var currentQuestion = _quizManager.GetNextQuestion();
                        if (currentQuestion != null)
                        {
                            string display = FormatQuizQuestion(currentQuestion);
                            OnResponseGenerated?.Invoke(display);
                            _awaitingQuizAnswer = true;
                        }
                        return;
                    }
                }
                else
                {
                    OnResponseGenerated?.Invoke("Please wait for the next question...");
                    return;
                }
            }

            // ============================================================
            // TASK REMINDER HANDLING - FIXED!
            // ============================================================

            // Check if we're waiting for a reminder response (yes/no or date)
            if (_awaitingReminder && _pendingTaskTitle != null)
            {
                // FIRST: Try to extract a date from the user's input
                DateTime extractedDate = ParseReminderDate(input);
                if (extractedDate != DateTime.MinValue)
                {
                    // User included the date in their response (e.g., "yes, remind me in 5 days")
                    bool success = _dbHelper.AddTask(_pendingTaskTitle, _pendingTaskDescription ?? "", extractedDate);
                    if (success)
                    {
                        _activityLog.AddEntry("Task Added with Reminder", $"{_pendingTaskTitle} - Reminder: {extractedDate:yyyy-MM-dd HH:mm}");
                        OnResponseGenerated?.Invoke($"Task '{_pendingTaskTitle}' added with reminder for {extractedDate:yyyy-MM-dd HH:mm}!");
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke($"Failed to add task '{_pendingTaskTitle}' with reminder. Please try again.");
                    }
                    _pendingTaskTitle = null;
                    _pendingTaskDescription = null;
                    _awaitingReminder = false;
                    _awaitingFollowUp = false;
                    return;
                }

                // SECOND: Check if user said yes (without a date)
                if (lowerInput == "yes" || lowerInput == "y" ||
                    lowerInput.Contains("yes") || lowerInput.Contains("y"))
                {
                    OnResponseGenerated?.Invoke("Please specify the reminder date (e.g., 'in 3 days', 'tomorrow', or '2026-06-25')");
                    _awaitingReminder = false;
                    // Set a flag to wait for date input
                    return;
                }
                // THIRD: Check if user said no
                else if (lowerInput == "no" || lowerInput == "n" ||
                         lowerInput.Contains("no") || lowerInput.Contains("n"))
                {
                    // Save task without reminder
                    bool success = _dbHelper.AddTask(_pendingTaskTitle, _pendingTaskDescription ?? "", null);
                    if (success)
                    {
                        _activityLog.AddEntry("Task Added", $"{_pendingTaskTitle} (no reminder)");
                        OnResponseGenerated?.Invoke($"Task '{_pendingTaskTitle}' added successfully without a reminder!");
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke($"Failed to add task '{_pendingTaskTitle}'. Please try again.");
                    }
                    _pendingTaskTitle = null;
                    _pendingTaskDescription = null;
                    _awaitingReminder = false;
                    _awaitingFollowUp = false;
                    return;
                }
                else
                {
                    // User said something else - ask again
                    OnResponseGenerated?.Invoke("Please answer yes or no. Would you like to set a reminder? (yes/no)");
                    return;
                }
            }

            // Check if we're waiting for a date (after user said "yes" without a date)
            // This is handled by the date parsing in the _awaitingReminder block above
            // The flag is set to false when asking for date, and the next input will be caught

            // ============================================================
            // TASK DESCRIPTION HANDLING
            // ============================================================

            if (_awaitingFollowUp && _pendingTaskTitle != null)
            {
                if (lowerInput == "skip" || lowerInput == "no")
                {
                    _pendingTaskDescription = "";
                    OnResponseGenerated?.Invoke($"Task '{_pendingTaskTitle}' added! Would you like to set a reminder? (yes/no)");
                    _awaitingReminder = true;
                    _awaitingFollowUp = false;
                    return;
                }
                else
                {
                    _pendingTaskDescription = input;
                    bool success = _dbHelper.AddTask(_pendingTaskTitle, _pendingTaskDescription, null);
                    if (success)
                    {
                        _activityLog.AddEntry("Task Added", $"{_pendingTaskTitle}: {_pendingTaskDescription}");
                        OnResponseGenerated?.Invoke($"Task '{_pendingTaskTitle}' added with description! Would you like to set a reminder? (yes/no)");
                        _awaitingReminder = true;
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke($"Failed to add task '{_pendingTaskTitle}'. Please try again.");
                        _pendingTaskTitle = null;
                    }
                    _awaitingFollowUp = false;
                    return;
                }
            }

            // ============================================================
            // EXIT, MENU, LOG, QUIZ START, TASK COMMANDS
            // ============================================================

            // Check for exit
            if (IsExitCommand(lowerInput))
            {
                HandleExit();
                return;
            }

            // Check for menu
            if (lowerInput.Contains("menu") || lowerInput.Contains("help") || lowerInput == "?")
            {
                ShowMenu();
                return;
            }

            // Check for activity log request
            if (lowerInput.Contains("log") || lowerInput.Contains("activity") || lowerInput.Contains("what have you done"))
            {
                ShowActivityLog();
                return;
            }

            // Check for quiz request
            if (lowerInput.Contains("quiz") || lowerInput.Contains("game") || lowerInput.Contains("test") || lowerInput.Contains("challenge"))
            {
                if (_quizMode)
                {
                    OnResponseGenerated?.Invoke("A quiz is already in progress. Type 'stop quiz' to end it, or continue answering.");
                    return;
                }
                StartQuiz();
                return;
            }

            // Check for task management commands
            if (lowerInput.Contains("add task") || lowerInput.Contains("new task") || lowerInput.Contains("create task") ||
                lowerInput.Contains("remind me") || lowerInput.Contains("set reminder"))
            {
                HandleTaskCommand(lowerInput);
                return;
            }

            // Check for viewing tasks
            if (lowerInput.Contains("show tasks") || lowerInput.Contains("list tasks") || lowerInput.Contains("view tasks") ||
                lowerInput.Contains("my tasks") || lowerInput.Contains("what tasks"))
            {
                ShowTasks();
                return;
            }

            // Check for task completion
            if (lowerInput.Contains("complete task") || lowerInput.Contains("mark complete") || lowerInput.Contains("finish task"))
            {
                HandleTaskCompletion(lowerInput);
                return;
            }

            // Check for task deletion
            if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task"))
            {
                HandleTaskDeletion(lowerInput);
                return;
            }

            // Check for follow-up
            if (_awaitingFollowUp && IsFollowUpRequest(lowerInput))
            {
                HandleFollowUp();
                return;
            }

            // Sentiment detection
            string sentiment = DetectSentiment(lowerInput);
            if (sentiment != "neutral")
            {
                OnSentimentDetected?.Invoke(sentiment, GetSentimentEmoji(sentiment));
                HandleSentimentWithTip(sentiment, lowerInput);
                return;
            }

            // Process by keywords
            string response = ProcessByKeywords(lowerInput);
            OnResponseGenerated?.Invoke(response);
        }

        #region Quiz Methods

        private void StartQuiz()
        {
            _quizManager.StartQuiz();
            _quizMode = true;
            _awaitingQuizAnswer = true;
            _activityLog.AddEntry("Quiz Started", "Cybersecurity quiz initiated");
        }

        private string FormatQuizQuestion(QuizQuestion question)
        {
            string display = $"Question {_quizManager.CurrentQuestionIndex + 1} of {_quizManager.TotalQuestions}: {question.Question}\n\n";
            char optionLetter = 'A';
            foreach (string option in question.Options)
            {
                display += $"{optionLetter}) {option}\n";
                optionLetter++;
            }
            display += $"\nType the letter (A, B, C, D) or number (1, 2, 3, 4) to answer. Type 'stop quiz' to end the quiz.";
            return display;
        }

        private bool IsValidQuizAnswer(string input)
        {
            string trimmed = input.Trim().ToUpper();

            if (trimmed.Length == 1 && char.IsLetter(trimmed[0]))
            {
                char letter = trimmed[0];
                return letter >= 'A' && letter <= 'D';
            }

            if (int.TryParse(trimmed, out int number))
            {
                return number >= 1 && number <= 4;
            }

            return false;
        }

        #endregion

        #region Task Management

        private void HandleTaskCommand(string input)
        {
            if (input.Contains("remind me") || input.Contains("set reminder"))
            {
                string title = ExtractTaskTitle(input);
                if (!string.IsNullOrEmpty(title))
                {
                    _pendingTaskTitle = title;
                    OnResponseGenerated?.Invoke($"I'll set a reminder for '{title}'. What's the description? (or say 'skip' to skip)");
                    _awaitingFollowUp = true;
                    return;
                }
            }

            string taskTitle = ExtractTaskTitle(input);
            if (!string.IsNullOrEmpty(taskTitle))
            {
                _pendingTaskTitle = taskTitle;
                OnResponseGenerated?.Invoke($"Task '{taskTitle}' added! Would you like to add a description? (or say 'skip' to skip)");
                _awaitingFollowUp = true;
                return;
            }

            OnResponseGenerated?.Invoke("I didn't catch that task. Try saying 'Add task: [your task name]'");
        }

        private void ShowTasks()
        {
            var tasks = _dbHelper.GetTasks();
            if (!tasks.Any())
            {
                OnResponseGenerated?.Invoke("You have no pending tasks. Add a task to get started!");
                _activityLog.AddEntry("Viewed Tasks", "No pending tasks found");
                return;
            }

            string taskList = "Your Tasks\n\n";
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                taskList += $"{i + 1}. {task.Status} {task.Title}\n";
                if (!string.IsNullOrEmpty(task.Description))
                    taskList += $"   Description: {task.Description}\n";
                if (task.ReminderDate.HasValue)
                    taskList += $"   Reminder: {task.ReminderDate:yyyy-MM-dd HH:mm}\n";
                taskList += "\n";
            }
            OnResponseGenerated?.Invoke(taskList);
            _activityLog.AddEntry("Viewed Tasks", $"{tasks.Count} tasks displayed");
        }

        private void HandleTaskCompletion(string input)
        {
            var match = Regex.Match(input, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                var tasks = _dbHelper.GetTasks();
                if (index > 0 && index <= tasks.Count)
                {
                    var task = tasks[index - 1];
                    if (_dbHelper.MarkTaskComplete(task.Id))
                    {
                        _activityLog.AddEntry("Task Completed", task.Title);
                        OnResponseGenerated?.Invoke($"Task '{task.Title}' marked as complete! Great job!");
                        return;
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke("Failed to complete task. Please try again.");
                        return;
                    }
                }
            }
            OnResponseGenerated?.Invoke("Please specify which task to complete. Example: 'Complete task 1'");
        }

        private void HandleTaskDeletion(string input)
        {
            var match = Regex.Match(input, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                var tasks = _dbHelper.GetTasks();
                if (index > 0 && index <= tasks.Count)
                {
                    var task = tasks[index - 1];
                    if (_dbHelper.DeleteTask(task.Id))
                    {
                        _activityLog.AddEntry("Task Deleted", task.Title);
                        OnResponseGenerated?.Invoke($"Task '{task.Title}' deleted successfully.");
                    }
                    else
                    {
                        OnResponseGenerated?.Invoke("Failed to delete task. Please try again.");
                    }
                    return;
                }
            }
            OnResponseGenerated?.Invoke("Please specify which task to delete. Example: 'Delete task 1'");
        }

        private string ExtractTaskTitle(string input)
        {
            string[] prefixes = { "add task", "new task", "create task", "remind me to", "set reminder for", "remind me about" };
            string cleaned = input;
            foreach (var prefix in prefixes)
            {
                if (cleaned.Contains(prefix))
                {
                    cleaned = cleaned.Replace(prefix, "").Trim();
                    break;
                }
            }

            if (cleaned.Contains("to "))
                cleaned = cleaned.Substring(cleaned.IndexOf("to ") + 3).Trim();

            if (cleaned.Contains("about "))
                cleaned = cleaned.Substring(cleaned.IndexOf("about ") + 6).Trim();

            cleaned = Regex.Replace(cleaned, @"(tomorrow|today|in \d+ days?|on \w+|\d{4}-\d{2}-\d{2})", "").Trim();

            return string.IsNullOrEmpty(cleaned) ? null : cleaned;
        }

        /// <summary>
        /// Parses various date formats from user input
        /// </summary>
        private DateTime ParseReminderDate(string input)
        {
            string lowerInput = input.ToLower().Trim();

            // Check for "today"
            if (lowerInput == "today")
                return DateTime.Now.Date;

            // Check for "tomorrow"
            if (lowerInput == "tomorrow")
                return DateTime.Now.Date.AddDays(1);

            // Check for "in X days"
            var match = Regex.Match(lowerInput, @"in\s+(\d+)\s+days?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            // Check for "in X hours"
            match = Regex.Match(lowerInput, @"in\s+(\d+)\s+hours?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int hours))
                return DateTime.Now.AddHours(hours);

            // Check for "X days from now"
            match = Regex.Match(lowerInput, @"(\d+)\s+days?\s+from\s+now");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int daysFromNow))
                return DateTime.Now.AddDays(daysFromNow);

            // Check for "in X day" (singular)
            match = Regex.Match(lowerInput, @"in\s+(\d+)\s+day");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int singleDay))
                return DateTime.Now.AddDays(singleDay);

            // Check for specific date format (YYYY-MM-DD, MM/DD/YYYY, etc.)
            if (DateTime.TryParse(input, out DateTime result))
                return result;

            return DateTime.MinValue;
        }

        #endregion

        #region Activity Log

        private void ShowActivityLog()
        {
            string log = _activityLog.GetLogSummary(10);
            OnResponseGenerated?.Invoke(log);
        }

        #endregion

        #region NLP and Keyword Processing

        private string ProcessByKeywords(string input)
        {
            string normalized = NormalizeInput(input);

            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => normalized.Contains(keyword)))
                {
                    _currentTopic = kvp.Key;
                    _awaitingFollowUp = true;

                    if (input.Contains("favorite") || input.Contains("love") || input.Contains("interested in"))
                    {
                        _favoriteTopic = kvp.Key;
                        OnUserInfoUpdated?.Invoke($"Favorite topic: {kvp.Key.ToUpper()}");
                        return PersonalizeResponse($"I see you're interested in {kvp.Key}! That's great! {GetRandomResponse(kvp.Key)}");
                    }

                    return GetRandomResponse(kvp.Key);
                }
            }

            if (input.Contains("tip") || input.Contains("advice") || input.Contains("suggestion"))
            {
                return GetRandomGeneralTip();
            }

            return GetDefaultResponse();
        }

        private string NormalizeInput(string input)
        {
            string normalized = input.ToLower();

            normalized = normalized.Replace("remind me", "reminder");
            normalized = normalized.Replace("set reminder", "reminder");
            normalized = normalized.Replace("add reminder", "reminder");
            normalized = normalized.Replace("add task", "task");
            normalized = normalized.Replace("create task", "task");
            normalized = normalized.Replace("new task", "task");
            normalized = normalized.Replace("take quiz", "quiz");
            normalized = normalized.Replace("start quiz", "quiz");
            normalized = normalized.Replace("play game", "quiz");
            normalized = normalized.Replace("what have you done", "log");
            normalized = normalized.Replace("show activity", "log");

            return normalized;
        }

        #endregion

        #region Sentiment and Follow-up

        private string DetectSentiment(string input)
        {
            foreach (var sentiment in _sentimentKeywords)
            {
                foreach (string keyword in sentiment.Value)
                {
                    if (input.Contains(keyword))
                    {
                        return sentiment.Key;
                    }
                }
            }
            return "neutral";
        }

        private string GetSentimentEmoji(string sentiment)
        {
            switch (sentiment)
            {
                case "worried": return "😟";
                case "frustrated": return "😤";
                case "curious": return "🤔";
                case "confused": return "😕";
                case "grateful": return "😊";
                case "hopeless": return "😞";
                default: return "😐";
            }
        }

        private void HandleSentimentWithTip(string sentiment, string userInput)
        {
            string empatheticResponse = GetEmpatheticResponse(sentiment);
            string tip = GetRelevantTip(userInput);
            string fullResponse = $"{empatheticResponse}\n\n{tip}";
            _awaitingFollowUp = true;
            _currentTopic = ExtractTopicFromInput(userInput);
            _activityLog.AddEntry("Sentiment Detected", $"{sentiment}");
            OnResponseGenerated?.Invoke(fullResponse);
        }

        private string GetEmpatheticResponse(string sentiment)
        {
            Random rand = new Random();
            if (_empatheticResponses.ContainsKey(sentiment) && _empatheticResponses[sentiment].Count > 0)
            {
                string response = _empatheticResponses[sentiment][rand.Next(_empatheticResponses[sentiment].Count)];
                return PersonalizeResponse(response);
            }
            return PersonalizeResponse("I understand how you feel. Let me help you with that!");
        }

        private string GetRelevantTip(string userInput)
        {
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => userInput.Contains(keyword)))
                {
                    return GetRandomResponse(kvp.Key);
                }
            }
            return GetRandomGeneralTip();
        }

        private string ExtractTopicFromInput(string input)
        {
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => input.Contains(keyword)))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private bool IsFollowUpRequest(string input)
        {
            string[] followupPhrases = { "tell me more", "explain more", "more", "another", "elaborate", "continue", "go on", "and then", "what else", "more tips", "another tip" };
            return followupPhrases.Any(phrase => input.Contains(phrase));
        }

        private void HandleFollowUp()
        {
            _awaitingFollowUp = false;
            string response;
            if (_currentTopic != null)
            {
                response = GetRandomResponse(_currentTopic);
                OnResponseGenerated?.Invoke($"Let me share more about that:\n\n{response}");
            }
            else
            {
                response = GetRandomGeneralTip();
                OnResponseGenerated?.Invoke(response);
            }
        }

        #endregion

        #region Helper Methods

        private string GetRandomResponse(string topic)
        {
            Random rand = new Random();
            if (_responsePool.ContainsKey(topic) && _responsePool[topic].Count > 0)
            {
                string response = _responsePool[topic][rand.Next(_responsePool[topic].Count)];
                return PersonalizeResponse(response);
            }
            return GetRandomGeneralTip();
        }

        private string GetRandomGeneralTip()
        {
            Random rand = new Random();
            string tip = _generalTips[rand.Next(_generalTips.Count)];
            return PersonalizeResponse(tip);
        }

        private string PersonalizeResponse(string response)
        {
            if (!string.IsNullOrEmpty(_userName) && new Random().Next(5) == 0)
            {
                return $"{_userName}, {response.ToLower()}";
            }
            return response;
        }

        private bool IsExitCommand(string input)
        {
            string[] exitCommands = { "exit", "quit", "bye", "goodbye", "end", "stop" };
            return exitCommands.Any(cmd => input == cmd);
        }

        private void HandleExit()
        {
            string exitMessage = $"Goodbye {_userName ?? "friend"}! Remember to stay safe online. Stay secure!";
            OnResponseGenerated?.Invoke(exitMessage);
            _activityLog.AddEntry("User Logged Out", _userName ?? "Unknown user");
        }

        private void ShowMenu()
        {
            string menu = "SECURECORE MENU\n\n" +
                         "- Password Safety - Learn about strong passwords\n" +
                         "- Phishing Attacks - Identify scams and fraud\n" +
                         "- Privacy Protection - Keep your data safe\n" +
                         "- Safe Browsing - Browse the web securely\n" +
                         "- Malware Protection - Defend against viruses\n" +
                         "- Task Management - Add, view, complete tasks\n" +
                         "- Quiz - Test your cybersecurity knowledge\n" +
                         "- Activity Log - See what I've done\n\n" +
                         "Just type any topic above, or ask for 'more tips'!";
            OnResponseGenerated?.Invoke(menu);
        }

        private string GetDefaultResponse()
        {
            string[] defaultResponses = {
                "I'm not sure I understand. Try 'password', 'phishing', 'privacy', 'browsing', 'malware', 'task', 'quiz', or 'menu'.",
                $"Ask me about cybersecurity topics like passwords or phishing, {_userName ?? "friend"}!",
                "I specialize in cybersecurity! Try asking about 'password safety' or 'phishing attacks'.",
                "Type 'menu' to see all the topics I can help you with!"
            };
            Random rand = new Random();
            return PersonalizeResponse(defaultResponses[rand.Next(defaultResponses.Length)]);
        }

        public void Dispose()
        {
            _dbHelper?.Dispose();
        }

        #endregion

        // Dictionaries
        private Dictionary<string, List<string>> _keywordMap;
        private Dictionary<string, List<string>> _responsePool;
        private List<string> _generalTips;
        private Dictionary<string, string[]> _sentimentKeywords;
        private Dictionary<string, List<string>> _empatheticResponses;
    }
}
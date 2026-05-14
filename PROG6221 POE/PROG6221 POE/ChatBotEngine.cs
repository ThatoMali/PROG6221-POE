using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG6221_POE
{
    public class ChatbotEngine : IDisposable
    {
        public event Action<string> OnResponseGenerated;
        public event Action<string, string> OnSentimentDetected;
        public event Action<string> OnUserInfoUpdated;

        private Dictionary<string, List<string>> _keywordMap = new Dictionary<string, List<string>>
        {
            { "password", new List<string> { "password", "pass", "login", "credentials" } },
            { "phishing", new List<string> { "phish", "scam", "fraud", "fake email" } },
            { "privacy", new List<string> { "privacy", "personal data", "data protection" } },
            { "browsing", new List<string> { "browse", "browser", "website", "http", "https" } },
            { "malware", new List<string> { "malware", "virus", "ransomware", "trojan" } }
        };

        private Dictionary<string, List<string>> _responsePool = new Dictionary<string, List<string>>
        {
            { "password", new List<string> {
                "🔐 Use a password manager for unique passwords!",
                "🛡️ Enable two-factor authentication whenever possible!",
                "💪 Create strong passwords with 12+ characters!",
                "🚫 Never reuse passwords across accounts!"
            }},
            { "phishing", new List<string> {
                "🎣 Always check sender email addresses carefully!",
                "⚠️ Hover over links before clicking!",
                "📧 Legitimate companies never ask for passwords via email!",
                "🔍 Look for spelling errors and urgent language!"
            }},
            { "privacy", new List<string> {
                "🔒 Review app permissions regularly!",
                "🌐 Use a VPN on public Wi-Fi!",
                "📱 Disable location tracking for unused apps!",
                "🔍 Check 'Have I Been Pwned' for data breaches!"
            }},
            { "browsing", new List<string> {
                "🌐 Look for HTTPS and the padlock icon!",
                "🧹 Clear browser cache regularly!",
                "🔧 Keep your browser updated!",
                "🚫 Avoid saving passwords in browser!"
            }},
            { "malware", new List<string> {
                "💿 Download only from official sources!",
                "🔄 Keep antivirus software updated!",
                "⚠️ Be cautious with email attachments!",
                "💾 Back up important data regularly!"
            }}
        };

        private List<string> _generalTips = new List<string>
        {
            "💡 Keep your software updated!",
            "💡 Use unique passwords for every account!",
            "💡 Think before you click on links!",
            "💡 Back up your data regularly!",
            "💡 Lock your computer when stepping away!",
            "💡 Use secure Wi-Fi networks!"
        };

        private string _userName;
        private bool _awaitingFollowUp = false;
        private string _currentTopic = null;
        private int _messageCount = 0;

        public bool IsAwaitingFollowUp => _awaitingFollowUp;
        public string LastTopic => _currentTopic;

        private string[] _positiveWords = { "great", "good", "thanks", "thank", "helpful", "love", "awesome", "nice" };
        private string[] _negativeWords = { "bad", "terrible", "hate", "confused", "frustrated", "worried" };

        public void SetUserName(string name)
        {
            _userName = name;
            OnUserInfoUpdated?.Invoke($"👤 User: {name}");
        }

        public void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            _messageCount++;
            string lowerInput = input.ToLower();

            if (_awaitingFollowUp && IsFollowUpRequest(lowerInput))
            {
                HandleFollowUp();
                return;
            }

            string sentiment = DetectSentiment(lowerInput);
            if (sentiment != "Neutral")
            {
                OnSentimentDetected?.Invoke(sentiment, sentiment == "Positive" ? "😊" : "😟");
                HandleSentimentResponse(sentiment);
            }

            if (IsExitCommand(lowerInput))
            {
                HandleExit();
                return;
            }

            if (lowerInput.Contains("menu") || lowerInput.Contains("help"))
            {
                ShowMenu();
                return;
            }

            string response = ProcessByKeywords(lowerInput);
            OnResponseGenerated?.Invoke(response);
        }

        private bool IsFollowUpRequest(string input)
        {
            string[] phrases = { "tell me more", "explain more", "more", "another", "elaborate", "continue" };
            return phrases.Any(p => input.Contains(p));
        }

        private void HandleFollowUp()
        {
            _awaitingFollowUp = false;
            string response = _currentTopic != null ? GetRandomResponse(_currentTopic) : GetRandomGeneralTip();
            OnResponseGenerated?.Invoke($"📚 More on that:\n\n{response}");
        }

        private string ProcessByKeywords(string input)
        {
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(k => input.Contains(k)))
                {
                    _currentTopic = kvp.Key;
                    _awaitingFollowUp = true;
                    return GetRandomResponse(kvp.Key);
                }
            }
            if (input.Contains("tip") || input.Contains("advice") || input.Contains("suggestion"))
                return GetRandomGeneralTip();
            return GetDefaultResponse();
        }

        private string GetRandomResponse(string topic)
        {
            var rand = new Random();
            if (_responsePool.ContainsKey(topic) && _responsePool[topic].Count > 0)
            {
                string response = _responsePool[topic][rand.Next(_responsePool[topic].Count)];
                return PersonalizeResponse(response);
            }
            return GetRandomGeneralTip();
        }

        private string GetRandomGeneralTip()
        {
            var rand = new Random();
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

        private string DetectSentiment(string input)
        {
            if (_positiveWords.Any(w => input.Contains(w))) return "Positive";
            if (_negativeWords.Any(w => input.Contains(w))) return "Negative";
            return "Neutral";
        }

        private void HandleSentimentResponse(string sentiment)
        {
            if (sentiment == "Negative")
            {
                OnResponseGenerated?.Invoke("I understand cybersecurity can be concerning. Let me help simplify things for you!");
            }
            else if (sentiment == "Positive")
            {
                OnResponseGenerated?.Invoke("I'm glad you find this helpful! Staying safe online is important.");
            }
        }

        private bool IsExitCommand(string input)
        {
            string[] exits = { "exit", "quit", "bye", "goodbye", "end" };
            return exits.Any(e => input == e);
        }

        private void HandleExit()
        {
            OnResponseGenerated?.Invoke($"Goodbye {_userName ?? "friend"}! Stay safe online! 🔒");
        }

        private void ShowMenu()
        {
            OnResponseGenerated?.Invoke("🔐 **SECURECORE MENU** 🔐\n\n• Password Safety\n• Phishing Attacks\n• Privacy Protection\n• Safe Browsing\n• Malware Protection\n\nType any topic to learn more!");
        }

        private string GetDefaultResponse()
        {
            string[] defaults = {
                "I'm not sure I understand. Try 'password', 'phishing', 'privacy', 'browsing', 'malware', or 'menu'.",
                $"Ask me about cybersecurity topics like passwords or phishing, {_userName ?? "friend"}!",
                "I specialize in cybersecurity! Try asking about 'password safety' or 'phishing attacks'."
            };
            return defaults[new Random().Next(defaults.Length)];
        }

        public void Dispose() { }
    }
}
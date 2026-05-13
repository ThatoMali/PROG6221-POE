using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PROG6221_POE
{
    // Question 2-3, 5-8: Complete Chatbot Engine
    public class ChatbotEngine : IDisposable
    {
        #region Events
        public event Action<string> OnResponseGenerated;
        public event Action<string, string> OnSentimentDetected;
        public event Action<string> OnUserInfoUpdated;
        #endregion

        #region Question 2: Keyword Recognition
        // Keywords mapping to topics
        private readonly Dictionary<string, List<string>> _keywordMap = new Dictionary<string, List<string>>
        {
            { "password", new List<string> { "password", "pass", "credentials", "login", "account" } },
            { "phishing", new List<string> { "phish", "scam", "fraud", "fake email", "spoof" } },
            { "privacy", new List<string> { "privacy", "personal data", "data protection", "gdpr" } },
            { "browsing", new List<string> { "browse", "browser", "website", "http", "https", "url" } },
            { "malware", new List<string> { "malware", "virus", "trojan", "ransomware", "spyware", "worm" } },
            { "tips", new List<string> { "tips", "advice", "recommendation", "guide", "how to" } },
            { "2fa", new List<string> { "2fa", "two factor", "mfa", "multifactor", "authentication" } }
        };
        #endregion

        #region Question 3: Random Responses
        // Multiple responses for variety
        private readonly Dictionary<string, List<string>> _responsePool = new Dictionary<string, List<string>>
        {
            { "password", new List<string> {
                "🔐 Use a password manager to generate and store unique passwords for each account!",
                "🛡️ Enable two-factor authentication whenever possible - it adds an extra layer of security!",
                "💪 Create passwords with at least 12 characters using uppercase, lowercase, numbers, and symbols!",
                "🚫 Never reuse passwords across different accounts - it's like using the same key for your house and car!",
                "🎯 Avoid using personal information like birthdays or pet names in your passwords!"
            }},
            { "phishing", new List<string> {
                "🎣 Always check the sender's email address carefully - scammers use slight variations!",
                "⚠️ Hover over links before clicking to see the actual destination URL!",
                "📧 Legitimate companies never ask for passwords via email!",
                "🔍 Look for spelling errors and urgent language - common phishing tactics!",
                "📱 When in doubt, contact the company directly using their official website!"
            }},
            { "privacy", new List<string> {
                "🔒 Review app permissions regularly - remove access for apps you don't use!",
                "🌐 Use a VPN when on public Wi-Fi to encrypt your internet traffic!",
                "📱 Disable location tracking for apps that don't need it!",
                "🔍 Regularly check 'Have I Been Pwned' to see if your data was compromised!",
                "📧 Use aliases or temporary emails for newsletter signups!"
            }},
            { "browsing", new List<string> {
                "🌐 Look for the padlock icon in the address bar - it means the site uses HTTPS encryption!",
                "🧹 Clear your browser cache and cookies regularly!",
                "🔧 Keep your browser updated for the latest security patches!",
                "🚫 Avoid saving passwords in your browser - use a password manager instead!",
                "🛡️ Install reputable ad-blockers and script blockers!"
            }},
            { "malware", new List<string> {
                "💿 Always download software from official sources, not third-party sites!",
                "🔄 Keep your antivirus software updated and run regular scans!",
                "⚠️ Be cautious with email attachments - even from known senders!",
                "🔧 Enable automatic updates for your operating system!",
                "💾 Back up important data regularly to an external drive or cloud!"
            }},
            { "2fa", new List<string> {
                "📱 Use authenticator apps like Google Authenticator or Authy instead of SMS when possible!",
                "🔑 Store backup codes in a safe place - they're your lifeline if you lose access!",
                "🛡️ Most major platforms (Google, Microsoft, Facebook) offer 2FA - enable it today!",
                "⚡ Biometric 2FA (fingerprint/face ID) combines convenience with security!",
                "🔐 Hardware security keys like YubiKey provide the strongest 2FA protection!"
            }}
        };

        // General tips pool
        private readonly List<string> _generalTips = new List<string>
        {
            "💡 Keep your software updated - security patches fix known vulnerabilities!",
            "💡 Use unique passwords for every account - credential stuffing attacks are common!",
            "💡 Think before you click - social engineering is the #1 attack vector!",
            "💡 Regular backups can save you from ransomware attacks!",
            "💡 Be skeptical of unsolicited calls asking for personal information!",
            "💡 Lock your computer when stepping away - even for a minute!",
            "💡 Use secure Wi-Fi - public networks can be easily intercepted!",
            "💡 Monitor your bank statements for unauthorized transactions!"
        };
        #endregion

        #region Question 5: Memory and Recall
        private string _userName;
        private string _favoriteTopic;
        private string _lastTopic;
        private Dictionary<string, string> _userPreferences = new Dictionary<string, string>();
        private int _messageCount = 0;
        private List<string> _conversationHistory = new List<string>();
        #endregion

        #region Question 4: Conversation Flow
        private bool _awaitingFollowUp = false;
        private string _currentTopic = null;

        public bool IsAwaitingFollowUp => _awaitingFollowUp;
        public string LastTopic => _currentTopic;
        #endregion

        #region Question 6: Sentiment Detection
        private readonly string[] _positiveWords = {
            "great", "good", "awesome", "thanks", "thank", "helpful", "nice", "excellent",
            "love", "like", "appreciate", "cool", "perfect", "amazing", "wonderful"
        };

        private readonly string[] _negativeWords = {
            "bad", "terrible", "awful", "hate", "dislike", "confused", "difficult",
            "hard", "annoying", "frustrated", "worried", "scared", "unsafe"
        };

        private readonly string[] _neutralWords = {
            "okay", "fine", "alright", "maybe", "perhaps", "sometimes"
        };
        #endregion

        #region Question 8: Code Optimization - OOP Principles
        public ChatbotEngine()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Initialization logic
        }

        public void SetUserName(string name)
        {
            _userName = name;
            _userPreferences["name"] = name;
            OnUserInfoUpdated?.Invoke($"👤 User: {name}");
        }

        public void ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            _messageCount++;
            string lowerInput = input.ToLower();
            _conversationHistory.Add(lowerInput);

            // Question 4: Check for follow-up indicators
            if (_awaitingFollowUp && IsFollowUpRequest(lowerInput))
            {
                HandleFollowUp();
                return;
            }

            // Question 5: Store favorite topic if detected pattern
            DetectAndStoreFavoriteTopic(lowerInput);

            // Question 6: Detect sentiment
            string sentiment = DetectSentiment(lowerInput);
            if (sentiment != "Neutral")
            {
                OnSentimentDetected?.Invoke(sentiment, GetSentimentEmoji(sentiment));
                HandleSentimentResponse(sentiment);
            }

            // Question 7: Check for exit/quit
            if (IsExitCommand(lowerInput))
            {
                HandleExit();
                return;
            }

            // Check for menu request
            if (lowerInput.Contains("menu") || lowerInput.Contains("help"))
            {
                ShowMenu();
                return;
            }

            // Question 2: Process by keyword recognition
            string response = ProcessByKeywords(lowerInput);
            OnResponseGenerated?.Invoke(response);
        }

        #region Question 2: Keyword Recognition Logic
        private string ProcessByKeywords(string input)
        {
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => input.Contains(keyword)))
                {
                    _currentTopic = kvp.Key;
                    _awaitingFollowUp = true;

                    // Question 5: Update favorite topic
                    UpdateFavoriteTopic(kvp.Key);

                    // Question 3: Return random response
                    return GetRandomResponse(kvp.Key);
                }
            }

            // Check for general tips request
            if (input.Contains("tip") || input.Contains("advice") || input.Contains("suggestion"))
            {
                return GetRandomGeneralTip();
            }

            // Question 7: Default response for unknown inputs
            return GetDefaultResponse();
        }
        #endregion

        #region Question 3: Random Response Methods
        private string GetRandomResponse(string topic)
        {
            Random rand = new Random();
            if (_responsePool.ContainsKey(topic) && _responsePool[topic].Count > 0)
            {
                return _responsePool[topic][rand.Next(_responsePool[topic].Count)];
            }
            return GetRandomGeneralTip();
        }

        private string GetRandomGeneralTip()
        {
            Random rand = new Random();
            return _generalTips[rand.Next(_generalTips.Count)];
        }
        #endregion

        #region Question 4: Conversation Flow Methods
        private bool IsFollowUpRequest(string input)
        {
            string[] followupPhrases = {
                "tell me more", "explain more", "more", "another",
                "elaborate", "continue", "go on", "and then", "what else",
                "more tips", "another tip", "more information"
            };

            return followupPhrases.Any(phrase => input.Contains(phrase));
        }

        private void HandleFollowUp()
        {
            _awaitingFollowUp = false;
            string response;

            if (_currentTopic != null)
            {
                response = GetRandomResponse(_currentTopic);
                OnResponseGenerated?.Invoke($"📚 Let me share more about that:\n\n{response}");
            }
            else
            {
                response = GetRandomGeneralTip();
                OnResponseGenerated?.Invoke(response);
            }
        }
        #endregion

        #region Question 5: Memory Methods
        private void DetectAndStoreFavoriteTopic(string input)
        {
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => input.Contains(keyword)))
                {
                    if (input.Contains("favorite") || input.Contains("love") || input.Contains("interested in"))
                    {
                        _favoriteTopic = kvp.Key;
                        OnUserInfoUpdated?.Invoke($"⭐ Favorite topic: {kvp.Key.ToUpper()}");
                        OnResponseGenerated?.Invoke($"I see you're interested in {kvp.Key}! That's great - I'll keep that in mind for our future conversations. {GetRandomResponse(kvp.Key)}");
                    }
                }
            }
        }

        private void UpdateFavoriteTopic(string topic)
        {
            if (!string.IsNullOrEmpty(_favoriteTopic) && _favoriteTopic != topic)
            {
                _userPreferences["last_topic"] = topic;
            }
        }

        private void PersonalizeResponse(ref string response)
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                response = response.Replace("user", _userName);
                if (!response.Contains(_userName) && new Random().Next(5) == 0)
                {
                    response = $"{_userName}, {response.ToLower()}";
                }
            }
        }
        #endregion

        #region Question 6: Sentiment Detection
        private string DetectSentiment(string input)
        {
            foreach (string word in _positiveWords)
            {
                if (input.Contains(word))
                    return "Positive";
            }

            foreach (string word in _negativeWords)
            {
                if (input.Contains(word))
                    return "Negative";
            }

            foreach (string word in _neutralWords)
            {
                if (input.Contains(word))
                    return "Neutral";
            }

            return "Neutral";
        }

        private string GetSentimentEmoji(string sentiment)
        {
            switch (sentiment)
            {
                case "Positive": return "😊";
                case "Negative": return "😟";
                default: return "😐";
            }
        }

        private void HandleSentimentResponse(string sentiment)
        {
            if (sentiment == "Negative")
            {
                OnResponseGenerated?.Invoke("I understand cybersecurity can be concerning. Let me help make it simpler for you!");
            }
            else if (sentiment == "Positive")
            {
                OnResponseGenerated?.Invoke("I'm glad you find this helpful! Cybersecurity is important for everyone.");
            }
        }
        #endregion

        #region Question 7: Error Handling and Edge Cases
        private bool IsExitCommand(string input)
        {
            string[] exitCommands = { "exit", "quit", "bye", "goodbye", "end", "stop" };
            return exitCommands.Any(cmd => input == cmd);
        }

        private void HandleExit()
        {
            string exitMessage = $"Goodbye {_userName ?? "friend"}! Remember to stay safe online. " +
                                $"I've shared {_messageCount} security tips with you today. " +
                                $"Visit again for more cybersecurity guidance! 🔒";
            OnResponseGenerated?.Invoke(exitMessage);
        }

        private string GetDefaultResponse()
        {
            string[] defaultResponses = {
                "I'm not sure I understand. Can you try rephrasing? I can help with topics like password safety, phishing, privacy, browsing, malware, or 2FA.",
                "Hmm, I didn't quite catch that. Try asking me about 'password safety', 'phishing attacks', or 'security tips'!",
                "I specialize in cybersecurity topics! Could you ask about passwords, phishing, privacy, safe browsing, malware, or two-factor authentication?",
                $"I want to help, {_userName ?? "friend"}! Try asking for 'security tips' or specific topics like 'password safety'."
            };
            Random rand = new Random();
            return defaultResponses[rand.Next(defaultResponses.Length)];
        }

        private void ShowMenu()
        {
            string menu = "🔐 **SECURECORE MENU** 🔐\n\n" +
                         "1. Password Safety - Learn about strong passwords\n" +
                         "2. Phishing Attacks - Recognize scams\n" +
                         "3. Privacy Protection - Guard your data\n" +
                         "4. Safe Browsing - Browse securely\n" +
                         "5. Malware Protection - Defend against viruses\n" +
                         "6. 2FA/MFA - Extra security layer\n" +
                         "7. General Tips - Quick security advice\n\n" +
                         "Just type any topic above, or ask for 'more tips'!";
            OnResponseGenerated?.Invoke(menu);
        }
        #endregion

        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
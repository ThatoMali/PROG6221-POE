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

        #region Question 2: Keyword Recognition
        private Dictionary<string, List<string>> _keywordMap = new Dictionary<string, List<string>>
        {
            { "password", new List<string> { "password", "pass", "login", "credentials", "account" } },
            { "phishing", new List<string> { "phish", "scam", "fraud", "fake email", "spoof" } },
            { "privacy", new List<string> { "privacy", "personal data", "data protection", "gdpr" } },
            { "browsing", new List<string> { "browse", "browser", "website", "http", "https", "url" } },
            { "malware", new List<string> { "malware", "virus", "ransomware", "trojan", "spyware" } }
        };
        #endregion

        #region Question 3: Random Responses
        private Dictionary<string, List<string>> _responsePool = new Dictionary<string, List<string>>
        {
            { "password", new List<string> {
                "🔐 Use a password manager to generate and store unique passwords for each account!",
                "🛡️ Enable two-factor authentication whenever possible - it adds an extra layer of security!",
                "💪 Create passwords with at least 12 characters using uppercase, lowercase, numbers, and symbols!",
                "🚫 Never reuse passwords across different accounts!",
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
            }}
        };

        private List<string> _generalTips = new List<string>
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
        private int _messageCount = 0;
        #endregion

        #region Question 4: Conversation Flow
        private bool _awaitingFollowUp = false;
        private string _currentTopic = null;

        public bool IsAwaitingFollowUp => _awaitingFollowUp;
        public string LastTopic => _currentTopic;
        #endregion

        #region Question 6: Enhanced Sentiment Detection
        // Sentiment categories with detailed keywords
        private Dictionary<string, string[]> _sentimentKeywords = new Dictionary<string, string[]>
        {
            { "worried", new string[] {
                "worried", "concerned", "anxious", "nervous", "scared", "fear",
                "unsafe", "vulnerable", "exposed", "at risk", "panic", "terrified"
            }},
            { "frustrated", new string[] {
                "frustrated", "annoyed", "irritated", "fed up", "tired of",
                "sick of", "exhausted", "overwhelmed", "stressed"
            }},
            { "curious", new string[] {
                "curious", "interested", "want to learn", "tell me", "explain",
                "how does", "what is", "why do", "I wonder"
            }},
            { "confused", new string[] {
                "confused", "don't understand", "unclear", "what does it mean",
                "too complicated", "complex", "hard to follow"
            }},
            { "grateful", new string[] {
                "thank", "thanks", "appreciate", "helpful", "useful", "great info",
                "good to know", "awesome", "amazing", "love this"
            }},
            { "hopeless", new string[] {
                "hopeless", "no hope", "useless", "pointless", "why bother",
                "won't help", "never work", "impossible"
            }}
        };

        // Empathetic responses based on sentiment
        private Dictionary<string, List<string>> _empatheticResponses = new Dictionary<string, List<string>>
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
        #endregion

        public ChatbotEngine()
        {
        }

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

            // Question 4: Check for follow-up indicators
            if (_awaitingFollowUp && IsFollowUpRequest(lowerInput))
            {
                HandleFollowUp();
                return;
            }

            // Question 6: Detect sentiment FIRST (before keyword processing)
            string sentiment = DetectSentiment(lowerInput);
            if (sentiment != "neutral")
            {
                OnSentimentDetected?.Invoke(sentiment, GetSentimentEmoji(sentiment));

                // Question 6: Provide empathetic response AND then give a tip
                HandleSentimentWithTip(sentiment, lowerInput);
                return;
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

        #region Question 6: Enhanced Sentiment Detection Methods

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
            // Get empathetic response
            string empatheticResponse = GetEmpatheticResponse(sentiment);

            // Then provide a relevant tip based on what they might be asking about
            string tip = GetRelevantTip(userInput);

            // Combine and send
            string fullResponse = $"{empatheticResponse}\n\n{tip}";

            // Question 6 requirement: Share a tip immediately without making user ask again
            // Also set follow-up so they can ask for more
            _awaitingFollowUp = true;
            _currentTopic = ExtractTopicFromInput(userInput);

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

            // Default empathetic response
            return PersonalizeResponse("I understand how you feel. Let me help you with that!");
        }

        private string GetRelevantTip(string userInput)
        {
            // Try to find a topic in their input
            foreach (var kvp in _keywordMap)
            {
                if (kvp.Value.Any(keyword => userInput.Contains(keyword)))
                {
                    return GetRandomResponse(kvp.Key);
                }
            }

            // Default to general tip
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

        #endregion

        #region Question 4: Conversation Flow Methods
        private bool IsFollowUpRequest(string input)
        {
            string[] followupPhrases = {
                "tell me more", "explain more", "more", "another",
                "elaborate", "continue", "go on", "and then", "what else",
                "more tips", "another tip", "more information", "next"
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
                    if (input.Contains("favorite") || input.Contains("love") || input.Contains("interested in"))
                    {
                        _favoriteTopic = kvp.Key;
                        OnUserInfoUpdated?.Invoke($"⭐ Favorite topic: {kvp.Key.ToUpper()}");
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
        #endregion

        #region Question 3: Random Response Methods
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
            if (!string.IsNullOrEmpty(_userName))
            {
                // Occasionally add user name for personalization (about 20% of responses)
                if (new Random().Next(5) == 0)
                {
                    return $"{_userName}, {response.ToLower()}";
                }
            }
            return response;
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
                                $"Stay secure! 🔒";
            OnResponseGenerated?.Invoke(exitMessage);
        }

        private void ShowMenu()
        {
            string menu = "🔐 **SECURECORE MENU** 🔐\n\n" +
                         "• Password Safety\n" +
                         "• Phishing Attacks\n" +
                         "• Privacy Protection\n" +
                         "• Safe Browsing\n" +
                         "• Malware Protection\n\n" +
                         "Just type any topic above, or ask for 'more tips'!";
            OnResponseGenerated?.Invoke(menu);
        }

        private string GetDefaultResponse()
        {
            string[] defaultResponses = {
                "I'm not sure I understand. Try 'password', 'phishing', 'privacy', 'browsing', 'malware', or 'menu'.",
                $"Ask me about cybersecurity topics like passwords or phishing, {_userName ?? "friend"}!",
                "I specialize in cybersecurity! Try asking about 'password safety' or 'phishing attacks'.",
                "Type 'menu' to see all the topics I can help you with!"
            };
            Random rand = new Random();
            return PersonalizeResponse(defaultResponses[rand.Next(defaultResponses.Length)]);
        }
        #endregion

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
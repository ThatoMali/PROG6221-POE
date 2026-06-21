using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG6221_POE.Services
{
    public class QuizManager
    {
        private List<QuizQuestion> _questions;
        private int _currentQuestionIndex;
        private int _score;
        private bool _isActive;
        private bool _waitingForAnswer;
        private bool _quizCompleted;

        public event Action<string> OnQuestionDisplayed;
        public event Action<bool, string> OnAnswerFeedback;
        public event Action<int, string> OnQuizComplete;

        public bool IsActive => _isActive;
        public int TotalQuestions => _questions?.Count ?? 0;
        public int CurrentQuestionIndex => _currentQuestionIndex;
        public int Score => _score;
        public bool IsQuizCompleted => _quizCompleted;

        public QuizManager()
        {
            InitializeQuestions();
            ResetQuiz();
        }

        private void InitializeQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects your information."
                },
                new QuizQuestion
                {
                    Question = "Which of the following is a strong password practice?",
                    Options = new List<string> { "Using your birthday", "Using 'password123'", "Using a mix of uppercase, lowercase, numbers, and symbols", "Using the same password for all accounts" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Strong passwords use a combination of different character types and are unique for each account."
                },
                new QuizQuestion
                {
                    Question = "What does the padlock icon in your browser address bar indicate?",
                    Options = new List<string> { "The website is safe", "The connection is encrypted using HTTPS", "The website is government-approved", "You have antivirus protection" },
                    CorrectAnswerIndex = 1,
                    Explanation = "The padlock icon indicates that your connection to the website is encrypted with HTTPS, protecting your data in transit."
                },
                new QuizQuestion
                {
                    Question = "What is social engineering in cybersecurity?",
                    Options = new List<string> { "A type of software", "Manipulating people to reveal confidential information", "A programming language", "A social media platform" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Social engineering is the psychological manipulation of people to perform actions or divulge confidential information."
                },
                new QuizQuestion
                {
                    Question = "How often should you update your passwords?",
                    Options = new List<string> { "Never", "Every 10 years", "Every 3-6 months, or immediately if compromised", "Only when forced to" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Regular password updates reduce the risk of unauthorized access. Change immediately if you suspect a breach."
                },
                new QuizQuestion
                {
                    Question = "True or False: Public Wi-Fi networks are always safe to use for banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Public Wi-Fi networks are often unsecured. Always use a VPN when accessing sensitive information on public Wi-Fi."
                },
                new QuizQuestion
                {
                    Question = "True or False: Two-factor authentication adds an extra layer of security to your accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 0,
                    Explanation = "Two-factor authentication requires a second verification step, significantly increasing account security."
                },
                new QuizQuestion
                {
                    Question = "True or False: It's safe to click on links in emails from unknown senders if the email looks legitimate.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Never click links from unknown senders. Always verify the sender's email address and hover over links to check the destination."
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string> { "A type of antivirus", "Malware that encrypts your files and demands payment", "A password manager", "A web browser" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Ransomware is malicious software that encrypts files and demands payment for the decryption key."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a sign of a phishing attempt?",
                    Options = new List<string> { "Urgent language demanding immediate action", "Spelling and grammar errors", "Suspicious sender email address", "All of the above" },
                    CorrectAnswerIndex = 3,
                    Explanation = "Phishing emails often contain urgent language, errors, and suspicious email addresses. All are red flags."
                },
                new QuizQuestion
                {
                    Question = "True or False: Using the same password for multiple accounts is safe if the password is strong.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Even strong passwords should not be reused. If one account is compromised, all accounts using that password are at risk."
                },
                new QuizQuestion
                {
                    Question = "What should you do if you suspect your computer has malware?",
                    Options = new List<string> { "Ignore it", "Run an antivirus scan immediately", "Continue using it normally", "Turn it off and never use it again" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Immediately run a full antivirus scan. If malware is detected, follow the antivirus software's removal instructions."
                }
            };
        }

        public void ResetQuiz()
        {
            _currentQuestionIndex = 0;
            _score = 0;
            _isActive = false;
            _waitingForAnswer = false;
            _quizCompleted = false;
        }

        public void StartQuiz()
        {
            ResetQuiz();
            _isActive = true;
            _quizCompleted = false;
            DisplayCurrentQuestion();
        }

        public void StopQuiz()
        {
            if (_isActive)
            {
                _isActive = false;
                _waitingForAnswer = false;
                _quizCompleted = true;
                OnQuizComplete?.Invoke(_score, "Quiz stopped. You can start a new quiz anytime by typing 'start quiz'.");
            }
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuestionIndex < _questions.Count && _isActive)
            {
                var question = _questions[_currentQuestionIndex];
                string display = FormatQuestion(question);
                OnQuestionDisplayed?.Invoke(display);
                _waitingForAnswer = true;
            }
            else if (_currentQuestionIndex >= _questions.Count && _isActive)
            {
                _isActive = false;
                _waitingForAnswer = false;
                _quizCompleted = true;
                string feedback = GetScoreFeedback();
                OnQuizComplete?.Invoke(_score, feedback);
            }
        }

        private string FormatQuestion(QuizQuestion question)
        {
            string display = $"Question {_currentQuestionIndex + 1} of {_questions.Count}: {question.Question}\n\n";
            char optionLetter = 'A';
            foreach (string option in question.Options)
            {
                display += $"{optionLetter}) {option}\n";
                optionLetter++;
            }
            display += $"\nType the letter (A, B, C, D) or number (1, 2, 3, 4) to answer. Type 'stop quiz' to end the quiz.";
            return display;
        }

        public QuizQuestion GetNextQuestion()
        {
            if (_currentQuestionIndex < _questions.Count && _isActive)
            {
                return _questions[_currentQuestionIndex];
            }
            return null;
        }

        public bool SubmitAnswer(string input)
        {
            if (!_isActive || _quizCompleted)
                return false;

            if (!_waitingForAnswer)
                return false;

            var question = _questions[_currentQuestionIndex];
            int selectedIndex = ParseAnswer(input, question.Options.Count);

            if (selectedIndex < 0 || selectedIndex >= question.Options.Count)
            {
                return false;
            }

            bool isCorrect = selectedIndex == question.CorrectAnswerIndex;
            _waitingForAnswer = false;

            if (isCorrect)
                _score++;

            // Show feedback
            string feedback = isCorrect ? "Correct! " : "Incorrect. ";
            feedback += question.Explanation;
            OnAnswerFeedback?.Invoke(isCorrect, feedback);

            // Move to next question
            _currentQuestionIndex++;

            // Display next question
            if (_currentQuestionIndex < _questions.Count && _isActive)
            {
                DisplayCurrentQuestion();
            }
            else if (_currentQuestionIndex >= _questions.Count && _isActive)
            {
                _isActive = false;
                _waitingForAnswer = false;
                _quizCompleted = true;
                string resultFeedback = GetScoreFeedback();
                OnQuizComplete?.Invoke(_score, resultFeedback);
            }

            return isCorrect;
        }

        private int ParseAnswer(string input, int optionCount)
        {
            input = input.Trim().ToUpper();

            // Try parsing as letter (A, B, C, D)
            if (input.Length == 1 && char.IsLetter(input[0]))
            {
                int index = input[0] - 'A';
                if (index >= 0 && index < optionCount)
                    return index;
            }

            // Try parsing as number (1, 2, 3, 4)
            if (int.TryParse(input, out int number))
            {
                int index = number - 1;
                if (index >= 0 && index < optionCount)
                    return index;
            }

            return -1;
        }

        private string GetScoreFeedback()
        {
            double percentage = (_score * 100.0) / _questions.Count;
            if (percentage >= 90)
                return $"Excellent! You got {_score} out of {_questions.Count} correct! You're a cybersecurity expert! Keep sharing your knowledge!";
            else if (percentage >= 70)
                return $"Good job! You got {_score} out of {_questions.Count} correct! You have strong cybersecurity knowledge. Keep learning to become an expert!";
            else if (percentage >= 50)
                return $"Not bad! You got {_score} out of {_questions.Count} correct. Review the topics you missed to strengthen your security awareness.";
            else
                return $"You got {_score} out of {_questions.Count} correct. Keep learning! Cybersecurity is crucial - review the topics and try again!";
        }

        public int GetCurrentProgress()
        {
            return _currentQuestionIndex;
        }

        public bool IsWaitingForAnswer()
        {
            return _waitingForAnswer;
        }
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using PROG6221_POE.Services;

namespace PROG6221_POE
{
    public partial class QuizWindow : Window
    {
        private QuizManager _quizManager;
        private bool _isQuizActive = false;

        public QuizWindow()
        {
            InitializeComponent();
            _quizManager = new QuizManager();

            // Subscribe to events
            _quizManager.OnQuestionDisplayed += DisplayQuestion;
            _quizManager.OnAnswerFeedback += ShowFeedback;
            _quizManager.OnQuizComplete += QuizComplete;
        }

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quizManager.StartQuiz();
            _isQuizActive = true;
            btnStartQuiz.IsEnabled = false;
            txtStatus.Text = "Answering...";
        }

        private void DisplayQuestion(string questionText)
        {
            Dispatcher.Invoke(() =>
            {
                txtQuestion.Text = questionText;
                txtProgress.Text = $"Question {_quizManager.CurrentQuestionIndex + 1} of {_quizManager.TotalQuestions}";
                txtScore.Text = $"Score: {_quizManager.Score}";
                pnlFeedback.Visibility = Visibility.Collapsed;
            });
        }

        private void ShowFeedback(bool isCorrect, string explanation)
        {
            Dispatcher.Invoke(() =>
            {
                pnlFeedback.Visibility = Visibility.Visible;
                txtFeedback.Text = $"{(isCorrect ? "Correct!" : "Incorrect.")} {explanation}";
                txtScore.Text = $"Score: {_quizManager.Score}";
                txtStatus.Text = isCorrect ? "Good Job!" : "Keep Learning!";
            });
        }

        private void QuizComplete(int score, string feedback)
        {
            Dispatcher.Invoke(() =>
            {
                _isQuizActive = false;
                btnStartQuiz.IsEnabled = true;
                txtStatus.Text = "Complete!";
                txtQuestion.Text = $"Quiz Complete!\n\n{feedback}";
                pnlFeedback.Visibility = Visibility.Visible;
                txtFeedback.Text = $"Final Score: {score} out of {_quizManager.TotalQuestions}";
                txtProgress.Text = $"Completed!";
                txtScore.Text = $"Final Score: {score}";
            });
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
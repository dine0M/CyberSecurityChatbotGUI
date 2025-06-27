using System;
using System.Collections.Generic;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace CyberSecurityChatbotGUI
{
    public partial class MainWindow : Window
    {
        private List<TaskItem> taskItems = new List<TaskItem>();
        private List<string> activityLog = new List<string>();
        private string? userName = null;
        private string? userInterest = null;

        private int currentQuizIndex = 0;
        private List<Question> quizQuestions = new List<Question>
        {
            new Question("What should you do if you receive an email asking for your password?",
                "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it", 2),
            new Question("What is phishing?",
                "A fake email attempt to steal info", "A type of fish", "Password", "Antivirus", 0),
            new Question("Should you reuse passwords?",
                "Yes", "No", "Only at work", "Sometimes", 1),
            new Question("How can you protect your online accounts?",
                "Use weak passwords", "Use the same password everywhere", "Enable two-factor authentication", "Ignore updates", 2),
            new Question("What's the best way to identify a scam email?",
                "Check grammar and sender address", "Click all links", "Reply immediately", "Forward to friends", 0),
        };

        // Keyword-based tips dictionary
        private Dictionary<string, List<string>> cybersecurityTips = new Dictionary<string, List<string>>()
        {
            {"password", new List<string> {
                "Use strong, unique passwords for each account. Avoid using personal details.",
                "Consider using a password manager to store your passwords safely.",
                "Change your passwords regularly and never share them."
            }},
            {"scam", new List<string> {
                "Never click on suspicious links or attachments in emails.",
                "Verify the identity of anyone asking for your personal information.",
                "Be cautious of deals that seem too good to be true—they often are."
            }},
            {"privacy", new List<string> {
                "Review privacy settings on your social media and email accounts.",
                "Limit what personal information you share online.",
                "Use encrypted communication tools whenever possible."
            }},
            {"phishing", new List<string> {
                "Be cautious of emails asking for personal information.",
                "Don’t click on links from unknown senders.",
                "Check for poor grammar or suspicious links in emails.",
                "Go directly to websites rather than clicking on email links."
            }},
        };

        // Sentiment response dictionary
        private Dictionary<string, string> sentimentResponses = new Dictionary<string, string>()
        {
            {"worried", "It's completely understandable to feel that way. Let me help ease your concerns."},
            {"frustrated", "I get that this can be frustrating. You're not alone—many face the same issue."},
            {"curious", "Curiosity is great! Let's explore more about this topic together."}
        };

        public MainWindow()
        {
            InitializeComponent();
            PlayVoiceGreeting();
            DisplayWelcomeAscii();
            AppendBotMessage("CyberSecurityBot: Welcome! Ask me anything about cybersecurity.");
            RefreshTaskList();
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            ChatBox.Items.Add("You: " + input);
            UserInput.Clear();
            HandleInput(input);
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuizIndex = 0;
            ShowQuizQuestion();
            LogActivity("Quiz started");
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            var q = quizQuestions[currentQuizIndex];
            int selectedIndex = -1;

            if (OptionA.IsChecked == true) selectedIndex = 0;
            else if (OptionB.IsChecked == true) selectedIndex = 1;
            else if (OptionC.IsChecked == true) selectedIndex = 2;
            else if (OptionD.IsChecked == true) selectedIndex = 3;

            if (selectedIndex == -1)
            {
                AppendBotMessage("CyberSecurityBot: Please select an answer.");
                return;
            }

            if (selectedIndex == q.CorrectIndex)
                AppendBotMessage("CyberSecurityBot: ✅ Correct! Great job.");
            else
                AppendBotMessage($"CyberSecurityBot: ❌ Incorrect. The correct answer is: {q.Options[q.CorrectIndex]}");

            currentQuizIndex++;
            ShowQuizQuestion();
        }

        private void ShowQuizQuestion()
        {
            if (currentQuizIndex >= quizQuestions.Count)
            {
                QuizPanel.Visibility = Visibility.Collapsed;
                AppendBotMessage("CyberSecurityBot: Quiz complete! Well done 🎉");
                LogActivity("Quiz completed");
                return;
            }

            var q = quizQuestions[currentQuizIndex];
            QuizQuestionText.Text = $"Q{currentQuizIndex + 1}: {q.Text}";
            OptionA.Content = $"A) {q.Options[0]}";
            OptionB.Content = $"B) {q.Options[1]}";
            OptionC.Content = $"C) {q.Options[2]}";
            OptionD.Content = $"D) {q.Options[3]}";
            OptionA.IsChecked = false;
            OptionB.IsChecked = false;
            OptionC.IsChecked = false;
            OptionD.IsChecked = false;

            QuizPanel.Visibility = Visibility.Visible;
        }

        private void HandleInput(string input)
        {
            string normalizedInput = NormalizeInput(input);

            // Name recognition
            if (normalizedInput.StartsWith("my name is"))
            {
                userName = input.Substring(10).Trim();
                AppendBotMessage($"CyberSecurityBot: Nice to meet you, {userName}!");
                return;
            }

            // Reminder and task NLP
            if (normalizedInput.Contains("remind me to") || normalizedInput.Contains("add a task") || normalizedInput.Contains("set a reminder") || normalizedInput.Contains("schedule")) 
            {
                string title = ExtractTaskTitle(input);
                DateTime? dueDate = ExtractDate(input);
                var task = new TaskItem { Title = title, Description = title, DueDate = dueDate };
                taskItems.Add(task);
                LogActivity($"Task added: '{title}'");

                if (dueDate.HasValue)
                {
                    AppendBotMessage($"CyberSecurityBot: ⏰ Reminder set for '{title}' on {dueDate.Value.ToShortDateString()}.");
                    LogActivity($"Reminder set for task: '{title}' on {dueDate.Value.ToShortDateString()}");
                }
                else
                {
                    AppendBotMessage($"CyberSecurityBot: 📌 Task added: '{title}'. Would you like to set a reminder for this task?");
                }

                RefreshTaskList();
                return;
            }

            // Start quiz NLP
            if (normalizedInput.Contains("start quiz") || normalizedInput.Contains("quiz me") || normalizedInput.Contains("ask me a question"))
            {
                currentQuizIndex = 0;
                ShowQuizQuestion();
                LogActivity("Quiz started");
                return;
            }

            // Show activity log NLP
            if (normalizedInput.Contains("what have you done") || normalizedInput.Contains("summary") || normalizedInput.Contains("show my tasks") || normalizedInput.Contains("activity log") || normalizedInput.Contains("show activity log"))
            {
                AppendBotMessage("CyberSecurityBot: Here's a summary of recent actions:");
                var recentLogs = activityLog.Count > 10 ? activityLog.GetRange(activityLog.Count - 10, 10) : activityLog;
                int i = 1;
                foreach (var log in recentLogs)
                    AppendBotMessage($"{i++}. {log}");
                return;
            }

            // Sentiment detection
            foreach (var sentiment in sentimentResponses)
            {
                if (normalizedInput.Contains(sentiment.Key))
                {
                    AppendBotMessage("CyberSecurityBot: " + sentiment.Value);
                    return;
                }
            }

            // Keyword recognition and response
            foreach (var entry in cybersecurityTips)
            {
                if (normalizedInput.Contains(entry.Key))
                {
                    string tip = GetRandomTip(entry.Value);

                    if (!string.IsNullOrEmpty(userInterest) && userInterest.ToLower().Contains(entry.Key))
                    {
                        AppendBotMessage($"CyberSecurityBot: As someone interested in {entry.Key}, here's a tip: {tip}");
                    }
                    else
                    {
                        AppendBotMessage($"CyberSecurityBot: {tip}");
                    }
                    return;
                }
            }

            // Fallback response
            AppendBotMessage("CyberSecurityBot: I'm not sure how to help with that. Try asking about tasks, reminders, or quizzes!");
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e) 
        {
            var task = new TaskItem
            {
                Title = TaskTitleInput.Text,
                Description = TaskDescriptionInput.Text,
                DueDate = TaskReminderDate.SelectedDate
            };
            taskItems.Add(task);
            LogActivity($"Task added: '{task.Title}'");
            if (task.DueDate.HasValue)
                LogActivity($"Reminder set for task: '{task.Title}' on {task.DueDate.Value.ToShortDateString()}");
            AppendBotMessage($"CyberSecurityBot: Task '{task.Title}' added.");
            RefreshTaskList();
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selected)
            {
                taskItems.Remove(selected);
                LogActivity($"Task deleted: '{selected.Title}'");
                RefreshTaskList();
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selected)
            {
                LogActivity($"Task completed: '{selected.Title}'");
                AppendBotMessage($"CyberSecurityBot: Great job completing '{selected.Title}'!");
                taskItems.Remove(selected);
                RefreshTaskList();
            }
        }

        private void RefreshTaskList()
        {
            TaskListBox.ItemsSource = null;
            TaskListBox.ItemsSource = taskItems;
        }

        private void AppendBotMessage(string message)
        {
            ChatBox.Items.Add(message);
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                var myPlayer = new SoundPlayer();
                myPlayer.SoundLocation = @"C:\Users\dineo\source\repos\Chatbot\Chatbot\PROG2A.wav.wav"; // Update your audio path here
                myPlayer.Load();
                myPlayer.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Audio error: " + ex.Message);
            }
        }

        private void DisplayWelcomeAscii()
        {
            string ascii = @"  
  /$$$$$$  /$$     /$$ /$$$$$$$  /$$$$$$$$ /$$$$$$$  /$$   /$$ /$$$$$$$$ /$$$$$$$$
 /$$__  $$|  $$   /$$/| $$__  $$| $$_____/| $$__  $$| $$$ | $$| $$_____/|__  $$__/
| $$  \__/ \  $$ /$$/ | $$  \ $$| $$      | $$  \ $$| $$$$| $$| $$         | $$   
| $$        \  $$$$/  | $$$$$$$ | $$$$$   | $$$$$$$/| $$ $$ $$| $$$$$      | $$   
| $$         \  $$/   | $$__  $$| $$__/   | $$__  $$| $$  $$$$| $$__/      | $$   
| $$    $$    | $$    | $$  \ $$| $$      | $$  \ $$| $$\  $$$| $$         | $$   
|  $$$$$$/    | $$    | $$$$$$$/| $$$$$$$$| $$  | $$| $$ \  $$| $$$$$$$$   | $$   
 \______/     |__/    |_______/ |________/|__/  |__/|__/  \__/|________/   |__/   
                                                                                  
                                                                                  
                                                                                  
  /$$$$$$  /$$$$$$$$  /$$$$$$  /$$   /$$ /$$$$$$$  /$$$$$$ /$$$$$$$$ /$$     /$$  
 /$$__  $$| $$_____/ /$$__  $$| $$  | $$| $$__  $$|_  $$_/|__  $$__/|  $$   /$$/  
| $$  \__/| $$      | $$  \__/| $$  | $$| $$  \ $$  | $$     | $$    \  $$ /$$/   
|  $$$$$$ | $$$$$   | $$      | $$  | $$| $$$$$$$/  | $$     | $$     \  $$$$/    
 \____  $$| $$__/   | $$      | $$  | $$| $$__  $$  | $$     | $$      \  $$/     
 /$$  \ $$| $$      | $$    $$| $$  | $$| $$  \ $$  | $$     | $$       | $$      
|  $$$$$$/| $$$$$$$$|  $$$$$$/|  $$$$$$/| $$  | $$ /$$$$$$   | $$       | $$      
 \______/ |________/ \______/  \______/ |__/  |__/|______/   |__/       |__/      
 "; 
            ChatBox.Items.Add(ascii);
        }

        private string ExtractTaskTitle(string input)
        {
            string lower = input.ToLower();
            string title = "Unnamed Task";

            if (lower.Contains("remind me to"))
                title = input.Substring(lower.IndexOf("remind me to") + "remind me to".Length).Trim();
            else if (lower.Contains("add a task to"))
                title = input.Substring(lower.IndexOf("add a task to") + "add a task to".Length).Trim();
            else if (lower.Contains("set a reminder to"))
                title = input.Substring(lower.IndexOf("set a reminder to") + "set a reminder to".Length).Trim();
            else if (lower.Contains("schedule"))
                title = input.Substring(lower.IndexOf("schedule") + "schedule".Length).Trim();

            return !string.IsNullOrEmpty(title) ? char.ToUpper(title[0]) + title.Substring(1) : "Unnamed Task";
        }

        private DateTime? ExtractDate(string input)
        {
            if (input.ToLower().Contains("tomorrow"))
                return DateTime.Today.AddDays(1);
            else if (input.ToLower().Contains("today"))
                return DateTime.Today;

            return null;
        }

        private void LogActivity(string message) 
        {
            string timestamped = $"{DateTime.Now:HH:mm:ss} - {message}";
            activityLog.Add(timestamped);
        }

        private string NormalizeInput(string input)
        {
            // Remove punctuation and lowercase the input
            string cleaned = Regex.Replace(input.ToLower(), @"[^\w\s]", "");
            return cleaned.Trim();
        }

        private string GetRandomTip(List<string> tips)
        {
            Random rand = new Random();
            int index = rand.Next(tips.Count);
            return tips[index];
        }

        private void ShowActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage("CyberSecurityBot: Here's a summary of recent actions:");
            var recentLogs = activityLog.Count > 10 ? activityLog.GetRange(activityLog.Count - 10, 10) : activityLog;
            int i = 1;
            foreach (var log in recentLogs)
                AppendBotMessage($"{i++}. {log}");
        }
    }

    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }

        public override string ToString()
        {
            return $"{Title} - {Description} (Due: {DueDate?.ToShortDateString() ?? "No date"})";
        }
    }

    public class Question
    {
        public string Text { get; set; }
        public string[] Options { get; set; }
        public int CorrectIndex { get; set; }

        public Question(string text, string a, string b, string c, string d, int correct)
        {
            Text = text;
            Options = new[] { a, b, c, d };
            CorrectIndex = correct;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace CyberSecurityChatbotGUI1
{
    public partial class MainWindow : Window
    {
        private List<string> activityLog = new List<string>();
        private Dictionary<string, (string Description, DateTime? Reminder)> tasks = new();
        private DispatcherTimer reminderTimer;
        private Dictionary<string, List<string>> topics;
        private string lastTopic = "";
        private string userName = "";

        public MainWindow()
        {
            InitializeComponent();
            PlayGreetingAudio();
            AskForUserName();
            SetupChatbot();

            reminderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            reminderTimer.Tick += CheckReminders;
            reminderTimer.Start();
        }

        private void PlayGreetingAudio()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("greeting.wav");
                player.Load();
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to play greeting audio: {ex.Message}");
            }
        }

        private void AskForUserName()
        {
            userName = Microsoft.VisualBasic.Interaction.InputBox("Please enter your name:", "User Name");

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = "User";
            }

            Respond($"Hello, {userName}, I am the Cybersecurity Awareness Bot. How may I help you?");
            ShowDefaultOptions();
        }

        private void SetupChatbot()
        {
            topics = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["phishing"] = new List<string> { "Never click on unknown links.", "Check sender addresses carefully." },
                ["password"] = new List<string> { "Use 2FA whenever possible.", "Never reuse passwords." },
                ["privacy"] = new List<string> { "Check app permissions.", "Don't overshare online." },
                ["scam"] = new List<string> { "If it sounds too good to be true, it probably is." },
                ["safe browsing"] = new List<string> { "Use secure (HTTPS) websites.", "Avoid suspicious popups and ads." }
            };
        }

        private void ShowDefaultOptions()
        {
            Respond("Here are some things you can ask me:");
            Respond("- How are you?\n- What is your purpose?\n- Password safety\n- Phishing\n- Scams\n- Safe browsing");
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AppendToChat($"{userName}: {input}");
            HandleInput(input.ToLower());
            UserInput.Clear();
        }

        private void HandleInput(string input)
        {
            if (input.Contains("how are you"))
            {
                Respond("I'm functioning at optimal cybersecurity levels. Thanks for asking!");
                return;
            }

            if (input.Contains("purpose"))
            {
                Respond("My purpose is to help you stay safe online by providing cybersecurity tips and reminders.");
                return;
            }

            if (input.Contains("password"))
            {
                Respond($"Here are some tips about passwords:\n- {string.Join("\n- ", topics["password"])}");
                return;
            }

            if (input.Contains("phishing"))
            {
                Respond($"Here are some tips about phishing:\n- {string.Join("\n- ", topics["phishing"])}");
                return;
            }

            if (input.Contains("scam"))
            {
                Respond($"Scam Awareness:\n- {string.Join("\n- ", topics["scam"])}");
                return;
            }

            if (input.Contains("safe browsing"))
            {
                Respond($"Safe Browsing Tips:\n- {string.Join("\n- ", topics["safe browsing"])}");
                return;
            }

            if (input.Contains("task"))
            {
                Respond("Use the 'Add Task' button to add a cybersecurity task.");
                return;
            }

            if (input.Contains("quiz"))
            {
                StartQuiz();
                return;
            }

            if (input.Contains("log"))
            {
                ShowLog();
                return;
            }

            Respond("I'm still learning. Try using keywords like 'task', 'quiz', or ask about 'phishing' or 'password safety'.");
            ShowDefaultOptions();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var title = Microsoft.VisualBasic.Interaction.InputBox("Enter task title:", "Add Task");
            var desc = Microsoft.VisualBasic.Interaction.InputBox("Enter description:", "Task Description");
            var reminderInput = Microsoft.VisualBasic.Interaction.InputBox("Reminder? (in days or leave blank)", "Set Reminder");

            DateTime? reminderDate = null;
            if (int.TryParse(reminderInput, out int days))
                reminderDate = DateTime.Now.AddDays(days);

            tasks[title] = (desc, reminderDate);
            LogAction($"Task added: {title} - {desc} (Reminder: {reminderDate?.ToShortDateString() ?? "none"})");
            Respond($"Task added: {title}. Reminder set: {reminderDate?.ToShortDateString() ?? "None"}");
        }

        private void CheckReminders(object sender, EventArgs e)
        {
            foreach (var task in tasks)
            {
                if (task.Value.Reminder.HasValue && task.Value.Reminder.Value.Date <= DateTime.Now.Date)
                {
                    Respond($"Reminder: {task.Key} - {task.Value.Description}");
                }
            }
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            StartQuiz();
        }

        private void StartQuiz()
        {
            var questions = new List<(string Q, string[] A, int Correct)>
            {
                ("What should you do if you get a suspicious email?", new[] { "Click it", "Report it", "Ignore", "Reply" }, 1),
                ("True or False: Using '123456' is a good password.", new[] { "True", "False" }, 1),
                ("Which is the safest way to store passwords?", new[] { "In a notebook", "In your browser", "In a password manager", "On sticky notes" }, 2),
                ("What is phishing?", new[] { "Fishing on the internet", "A hacking method", "A scam tricking you to reveal info", "An antivirus software" }, 2),
                ("How often should you update your passwords?", new[] { "Never", "Every few years", "Only when hacked", "Regularly" }, 3),
                ("Which of the following is a strong password?", new[] { "password123", "letmein", "S3cure!P@ssw0rd", "qwerty" }, 2),
                ("What does 2FA stand for?", new[] { "Two-Factor Authentication", "Too Fast Access", "Two-Faced Attack", "Twin Firewall Access" }, 0),
                ("Which one is not a form of malware?", new[] { "Trojan", "Firewall", "Spyware", "Ransomware" }, 1),
                ("Why should software be kept up-to-date?", new[] { "For better graphics", "To prevent boredom", "To fix bugs and patch security holes", "Just because" }, 2),
                ("Which device is safest to use public Wi-Fi on?", new[] { "Any device with VPN", "Any phone", "Laptops", "Smartwatch" }, 0),
            };

            int score = 0;
            foreach (var (q, a, correct) in questions)
            {
                var answer = Microsoft.VisualBasic.Interaction.InputBox($"{q}\n{string.Join("\n", a.Select((x, i) => $"{i + 1}. {x}"))}", "Quiz");
                if (int.TryParse(answer, out int choice) && choice - 1 == correct)
                {
                    Respond("Correct!");
                    score++;
                }
                else
                {
                    Respond($"Wrong. The correct answer was: {a[correct]}");
                }
            }
            Respond($"Quiz done. Score: {score}/{questions.Count}");
            LogAction($"Quiz completed: Score {score}/{questions.Count}");
        }

        private void ShowLog_Click(object sender, RoutedEventArgs e) => ShowLog();

        private void ShowLog()
        {
            string log = string.Join("\n", activityLog.TakeLast(10));
            Respond("Activity Log:\n" + (string.IsNullOrWhiteSpace(log) ? "No actions yet." : log));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Respond(string text)
        {
            AppendToChat($"Bot: {text}");
        }

        private void AppendToChat(string text)
        {
            ChatHistory.Text += text + "\n\n";
        }

        private void LogAction(string action)
        {
            activityLog.Add($"{DateTime.Now:HH:mm:ss} - {action}");
        }
    }
}

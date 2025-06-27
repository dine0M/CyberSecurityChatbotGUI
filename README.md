#CyberSecurity Awareness Chatbot GUI

A WPF-based C# chatbot designed to educate users about cybersecurity through interactive chat, quizzes, task reminders, and activity logging. Built using .NET and WPF, the project simulates a conversational assistant that promotes safer online habits.


#Features

- Interactive Chatbot Interface**
  - Supports keyword-based NLP for basic input recognition
  - Welcomes user with ASCII art and voice greeting

- Cybersecurity Quiz**
  - 5 multiple-choice questions on cybersecurity best practices
  - Immediate feedback on correctness

- Task and Reminder Assistant**
  - Allows users to add, complete, and delete tasks
  - NLP can recognize phrases like “remind me to...” and set tasks
  - Optional due date parsing (e.g., “today”, “tomorrow”)

- Activity Log**
  - Records key user actions: task updates, quiz attempts, NLP interactions
  - Supports command: “show activity log” or “what have you done”

- GitHub Actions Workflow**
  - Integrated CI pipeline for build and test using `.NET`
  - Workflow defined in `.github/workflows/main.yml`



#Technologies Used

Technology:
C#/. NET
WPF(XAML)
GIT & GITHUB
SOUNDPLAYER



#Getting Started

#Prerequisites
- Visual Studio 2022 or newer
- .NET 6 / .NET 7 SDK (adjust according to your setup)
- Windows OS (required for WPF)

#How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/CyberSecurityChatbotGUI.git
   cd CyberSecurityChatbotGUI

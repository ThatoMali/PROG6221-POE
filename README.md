# PROG6221-POE

Cybersecurity Awareness Chatbot (C# Console Application)
Overview

This project is a console-based Cybersecurity Awareness Chatbot developed in C#. The application is designed to educate users on basic cybersecurity practices through an interactive and user-friendly interface. It incorporates multimedia elements, structured code design, and version control using GitHub.
The chatbot provides a personalised experience, responds to basic cybersecurity-related questions, and demonstrates good programming practices such as modular design and input validation.

Features

1. Voice Greeting
Plays a recorded voice message when the application starts.
Audio file is stored in WAV format for compatibility.
Enhances user engagement and provides a welcoming experience.

2. ASCII Art Display
Displays a cybersecurity-themed ASCII logo at startup.
Improves visual presentation of the console application.

3. Personalised User Interaction
Prompts the user to enter their name.
Uses the name in responses to create a personalised experience.

4. Text-Based Chat System
Allows users to interact with the chatbot via console input.
Responds to predefined questions such as:
    “How are you?”
    “What is your purpose?”
    “What can I ask you about?”

5. Cybersecurity Awareness Responses
Provides basic guidance on:
    Password safety
    Phishing awareness
    Safe browsing practices

6. Input Validation
Handles empty or invalid inputs.
Displays a default message when input is not understood.

7. Enhanced Console UI
Uses coloured text for improved readability.
Includes structured layout with headers and spacing.
Simulates a typing effect for a conversational feel.

8. Modular Code Structure
Code is separated into multiple classes:
    Program.cs (entry point)
    BotUI.cs (user interface and display)
    ChatBotEngine.cs (chat logic)
    VoiceGreeting.cs (audio playback)
    Utils.cs (helper methods)
Improves readability and maintainability.

9. GitHub Version Control
Project is managed using GitHub.
Includes multiple meaningful commits reflecting development progress.

10. Continuous Integration (CI)
GitHub Actions workflow configured.
Automatically builds and checks the project on each push.

Technologies Used:
C#
.NET Console Application
System.Media (for audio playback)
GitHub
GitHub Actions (CI)

Project Structure:

CyberSecurityBot/
│
├── Program.cs
├── BotUI.cs
├── ChatBotEngine.cs
├── VoiceGreeting.cs
├── Utils.cs
├── greeting.wav
└── .github/workflows/dotnet.yml

How to Run the Application:

1.Open the project in Visual Studio or any C# compatible IDE.
2.Ensure the greeting.wav file is in the project directory.
3.Build the solution.
4.Run the application.
5.Follow the prompts in the console to interact with the chatbot.

Example Interaction:

Please enter your name: John

Welcome John! How can I help you today?

You: What is phishing?
Bot: Never click suspicious links or download unknown attachments.

GitHub Commits
The project includes at least six meaningful commits, such as:
    Initial project setup
    Added ASCII art and UI enhancements
    Implemented voice greeting feature
    Developed chatbot response system
    Added input validation
    Configured GitHub Actions CI workflow

Conclusion

This project demonstrates the development of a structured and interactive console application in C#. It highlights key programming concepts such as modular design, user interaction, input validation, and basic cybersecurity awareness. Additionally, it showcases the use of version control and continuous integration tools, aligning with modern software development practices


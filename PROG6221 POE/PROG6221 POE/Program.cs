using CyberSecurityBot;
using PROG6221_POE;
using System;

namespace PROG6221_POE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SecureCore Cybersecurity Bot";

            BotUI.ShowHeader();

            VoiceGreeting.PlayGreeting();

            BotUI.WelcomeMessage();

            ChatBotEngine.StartChat();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSession ended. Stay safe online.");
            Console.ResetColor();

            Console.ReadKey();
        }
    }
}
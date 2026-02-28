
using System;
namespace PROG6221_POE

{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Cybersecurity Awareness Bot";

            BotUI.ShowHeader();

            VoiceGreeting.PlayGreeting();

            BotUI.WelcomeMessage();

            ChatBotEngine.StartChat();

            Console.WriteLine("\nThank you for using the Cybersecurity Awareness Bot. Stay safe online!");
            Console.ReadKey();
        }
    }
}


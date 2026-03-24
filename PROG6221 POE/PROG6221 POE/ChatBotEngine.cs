using System;
using System.Collections.Generic;
using System.Text;


using System;

namespace PROG6221_POE
{
    public static class ChatBotEngine
    {
        public static void StartChat()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nPlease enter your name: ");
            Console.ResetColor();

            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Name cannot be empty. Try again: ");
                name = Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nWelcome {name}! How can I help you today?\n");
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("You: ");
                Console.ResetColor();

                string input = Console.ReadLine()?.ToLower();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("I didn’t quite understand that. Could you rephrase?");
                    continue;
                }

                if (input.Contains("exit") || input.Contains("quit"))
                    break;

                Respond(input);
            }
        }

        private static void Respond(string input)
        {
            if (input.Contains("how are you"))
                BotUI.TypeEffect("I'm doing great! Ready to help you stay safe online.");

            else if (input.Contains("purpose"))
                BotUI.TypeEffect("My purpose is to educate users about cybersecurity awareness.");

            else if (input.Contains("ask"))
                BotUI.TypeEffect("You can ask me about passwords, phishing, and safe browsing.");

            else if (input.Contains("password"))
                BotUI.TypeEffect("Use strong passwords with uppercase, lowercase, numbers, and symbols.");

            else if (input.Contains("phishing"))
                BotUI.TypeEffect("Never click suspicious links or download unknown attachments.");

            else if (input.Contains("browsing"))
                BotUI.TypeEffect("Always verify websites and avoid entering sensitive information on untrusted sites.");

            else
                BotUI.TypeEffect("I didn’t quite understand that. Could you rephrase?");
        }
    }
}


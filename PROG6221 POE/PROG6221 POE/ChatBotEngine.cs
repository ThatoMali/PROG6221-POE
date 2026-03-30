using PROG6221_POE;
using System;

namespace PROG6221_POE
{
    public static class ChatBotEngine
    {
        public static void StartChat()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nEnter your name: ");
            Console.ResetColor();

            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Name cannot be empty. Try again: ");
                name = Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nHello {name}, how can I assist you today?\n");
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("You: ");
                Console.ResetColor();

                string input = Console.ReadLine()?.ToLower();

                // INPUT VALIDATION
                if (string.IsNullOrWhiteSpace(input))
                {
                    BotUI.TypeEffect("I didn’t quite understand that. Could you rephrase?");
                    continue;
                }

                // QUIT FUNCTION
                if (input == "exit" || input == "quit" || input == "bye")
                {
                    BotUI.TypeEffect("Goodbye! Stay safe online.");
                    break;
                }

                Respond(input);
            }
        }

        private static void Respond(string input)
        {
            if (input.Contains("how are you"))
                BotUI.TypeEffect("I'm functioning optimally and ready to assist you.");

            else if (input.Contains("purpose"))
                BotUI.TypeEffect("My purpose is to promote cybersecurity awareness and safe online practices.");

            else if (input.Contains("ask"))
                BotUI.TypeEffect("You can ask me about passwords, phishing, or safe browsing.");

            else if (input.Contains("password"))
                BotUI.TypeEffect("Use strong passwords with a mix of letters, numbers, and symbols.");

            else if (input.Contains("phishing"))
                BotUI.TypeEffect("Be cautious of suspicious emails and never click unknown links.");

            else if (input.Contains("browsing"))
                BotUI.TypeEffect("Always ensure websites are secure (https) before entering sensitive data.");

            else
                BotUI.TypeEffect("I didn’t quite understand that. Could you rephrase?");
        }
    }
}
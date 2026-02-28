using System;
using System.Threading;

namespace PROG6221_POE
{
    public static class BotUI
    {
        public static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("===========================================");
            Console.WriteLine("   CYBERSECURITY AWARENESS CHATBOT");
            Console.WriteLine("===========================================\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
   ____        _               ____        _   
  / ___| _   _| |__   ___ _ __| __ )  ___ | |_ 
  \___ \| | | | '_ \ / _ \ '__|  _ \ / _ \| __|
   ___) | |_| | |_) |  __/ |  | |_) | (_) | |_ 
  |____/ \__,_|_.__/ \___|_|  |____/ \___/ \__|
");
            Console.ResetColor();
        }

        public static void WelcomeMessage()
        {
            TypeEffect("\nWelcome to the Cybersecurity Awareness Bot!");
            TypeEffect("I'm here to help you stay safe online.\n");
        }

        public static void TypeEffect(string message)
        {
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(20);
            }
            Console.WriteLine();
        }
    }
}


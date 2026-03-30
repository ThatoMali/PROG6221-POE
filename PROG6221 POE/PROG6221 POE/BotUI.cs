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
   _____                            _____                
  / ____|                          / ____|               
 | (___   ___  ___ _   _ _ __ ___ | |     ___  _ __ ___  
  \___ \ / _ \/ __| | | | '__/ _ \| |    / _ \| '__/ _ \ 
  ____) |  __/ (__| |_| | | |  __/| |___| (_) | | |  __/ 
 |_____/ \___|\___|\__,_|_|  \___| \_____\___/|_|  \___| 
                                                       
                S E C U R E  C O R E                   
");
            Console.ResetColor();
        }

        public static void WelcomeMessage()
        {
            TypeEffect("\nWelcome to the Cybersecurity Awareness Bot!");
            TypeEffect("I'm here to help you stay safe online.\n");
            TypeEffect("Type 'exit', 'quit', or 'bye' anytime to end the session.\n");
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


using PROG6221_POE;
using System;

namespace CyberSecurityBot
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
            Console.WriteLine($"\nHello {name}, welcome to SecureCore.\n");
            Console.ResetColor();

            ShowMenu();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nYou: ");
                Console.ResetColor();

                string input = Console.ReadLine()?.ToLower();

                // Validation
                if (string.IsNullOrWhiteSpace(input))
                {
                    BotUI.TypeEffect("I didn’t quite understand that. Please select one of the options listed.");
                    continue;
                }

                // Quit function
                if (input == "exit" || input == "quit" || input == "bye")
                {
                    BotUI.TypeEffect("Goodbye! Stay safe online.");
                    break;
                }

                Respond(input);
            }
        }

        private static void ShowMenu()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("You can ask me about the following topics:");
            Console.WriteLine("1. Password Safety");
            Console.WriteLine("2. Phishing Attacks");
            Console.WriteLine("3. Safe Browsing");
            Console.WriteLine("4. Malware Protection");
            Console.WriteLine("5. General Cybersecurity Tips");
            Console.WriteLine("Type 'exit' to quit.");
            Console.ResetColor();
        }

        private static void Respond(string input)
        {
            if (input.Contains("password"))
            {
                BotUI.TypeEffect(
                "Password Safety:\n" +
                "A strong password is your first line of defense against cyber attacks. " +
                "Use a combination of uppercase and lowercase letters, numbers, and special characters. " +
                "Avoid using personal information such as names or birthdates. " +
                "It is also important to use different passwords for different accounts and consider using a password manager to store them securely.");
            }
            else if (input.Contains("phishing"))
            {
                BotUI.TypeEffect(
                "Phishing Attacks:\n" +
                "Phishing is a type of cyber attack where attackers trick users into revealing sensitive information. " +
                "This often happens through fake emails or messages that appear to be from trusted sources. " +
                "Always verify the sender, avoid clicking suspicious links, and never provide personal information unless you are sure the source is legitimate.");
            }
            else if (input.Contains("browsing"))
            {
                BotUI.TypeEffect(
                "Safe Browsing:\n" +
                "When browsing the internet, always ensure that websites are secure by checking for 'https' in the URL. " +
                "Avoid downloading files from unknown sources and be cautious when entering sensitive information online. " +
                "Using updated browsers and security tools can help protect you from threats.");
            }
            else if (input.Contains("malware"))
            {
                BotUI.TypeEffect(
                "Malware Protection:\n" +
                "Malware refers to malicious software designed to harm or exploit systems. " +
                "To protect yourself, install reliable antivirus software and keep it updated. " +
                "Avoid downloading untrusted applications and regularly scan your system for threats. " +
                "Keeping your operating system updated also reduces vulnerabilities.");
            }
            else if (input.Contains("tips"))
            {
                BotUI.TypeEffect(
                "General Cybersecurity Tips:\n" +
                "Always keep your software up to date, use strong and unique passwords, and enable two-factor authentication where possible. " +
                "Be cautious of public Wi-Fi networks and avoid accessing sensitive information on unsecured connections. " +
                "Regularly back up important data to prevent loss in case of an attack.");
            }
            else
            {
                BotUI.TypeEffect("I didn’t quite understand that. Please choose one of the listed topics such as 'password' or 'phishing'.");
            }
        }
    }
}
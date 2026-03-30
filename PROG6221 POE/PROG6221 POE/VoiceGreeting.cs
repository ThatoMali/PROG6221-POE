using System;
using System.IO;
using System.Media;

namespace CyberSecurityBot
{
    public static class VoiceGreeting
    {
        public static void PlayGreeting()
        {
            try
            {
                string filePath = "greeting.wav";

                if (File.Exists(filePath))
                {
                    SoundPlayer player = new SoundPlayer(filePath);
                    player.PlaySync();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Voice greeting file not found.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Audio Error: " + ex.Message);
            }
        }
    }
}
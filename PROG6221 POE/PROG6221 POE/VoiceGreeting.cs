using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace ST10447949_POE_Part1
{
    public static class VoiceGreeting
    {
        public static void PlayGreeting()
        {
            try
            {

                SoundPlayer player = new SoundPlayer("C:\\Users\\lab_services_student\\Desktop\\PROG6221-POE\\PROG6221 POE\\PROG6221 POE\\greeting.wav"); player.Play(); // Waits until complete

            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️ Voice greeting not found.");
                Console.ResetColor();
            }
        }
    }
}
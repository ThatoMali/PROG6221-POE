using System;
using System.Threading;

namespace CyberSecurityBot
{
    public static class Utils
    {
        public static void Delay(int ms)
        {
            Thread.Sleep(ms);
        }

        public static void Divider()
        {
            Console.WriteLine("==============================================");
        }
    }
}
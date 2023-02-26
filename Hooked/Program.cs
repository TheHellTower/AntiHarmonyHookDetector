using System;

namespace Hooked
{
    public class Program
    {
        public static bool isCracked() => false;

        static void Main(string[] args) => Console.WriteLine($"Cracked: {isCracked()}");
    }
}
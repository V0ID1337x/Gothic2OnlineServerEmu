using System;
using System.IO;
using RakNet;
using static System.Console;

namespace G2OServerEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Kolory w konsoli
            var consoleColor = ForegroundColor;
            var backgroundColor = BackgroundColor;
            BackgroundColor = ConsoleColor.White;
            ForegroundColor = ConsoleColor.Black;
            WriteLine("Gothic 2 Online Server Emulator by Sative is loading now...");
            WriteLine("For version: {0}.{1}.{2}", Version.Major, Version.Minor, Version.Patch);
            WriteLine();
            ForegroundColor = consoleColor;
            BackgroundColor = backgroundColor;

            if(!File.Exists("RakNet.dll")) {
                Console.WriteLine("RakNet.dll not found!\nPut RakNet.dll in your server emulator directory!");
            }
            else {
                try {
                    var dllCall = new RakString();
                }
                catch {
                    Console.WriteLine("RakNet.dll isssue!\nTake RakNet.dll from original server emulator archive!");
                    Console.ReadKey();
                    return;
                }
                try {
                    new Server().Start().Run();
                }
                catch(Exception e) {
                    Console.WriteLine("Server cannot start!\nException:" + e);
                }
            }
            Console.WriteLine("Bye!");
            Console.ReadKey();
        }
    }
}

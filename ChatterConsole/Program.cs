/*
Startup console application. Handle format and console read/write
Copyright (C) 2020  Trash Bros (BlinkTheThings, Reakain)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;

namespace ChatterConsole
{
    class Program
    {
        static string incommingMessage = string.Empty;
        static List<string> messageHistory = new List<string>();
        static int historyIndex = -1;

        static void Main(string[] args)
        {
            // Print the version number on startup
            Console.Write(string.Format("You are running version {0} of Chatter!\n",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            // Get the local IP Address to use
            string ipAddress = "";
            if (args.Length > 0)
            {
                ipAddress = args[0];
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                Console.Write("Enter the local IP address to use: ");
                ipAddress = Console.ReadLine();
                while (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Console.ReadLine();
                }
            }

            // Get the display name to use
            string displayName = "";
            if (args.Length > 1)
            {
                displayName = args[1];
            }
            if (string.IsNullOrEmpty(displayName))
            {
                Console.Write("Enter your display name: ");
                displayName = Console.ReadLine();
                while (string.IsNullOrEmpty(displayName))
                {
                    displayName = Console.ReadLine();
                }
            }

            // Create a new Chatter client
            var chatterClient = new Chatter.Controller(ipAddress, displayName, port: "1314");

            // Attach a message display handler
            chatterClient.MessageDisplayEventHandler += (sender, m) =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(m + "\n> " + incommingMessage);
            };

            chatterClient.Init();

            // Get messages and send them out
            Console.Write("> ");
            string message = ReadMessage();

            while (!IsQuitMessage(message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    chatterClient.SendMessage(message);
                }

                message = ReadMessage();
            };

            chatterClient.ShutDown();
        }

        private static string ReadMessage()
        {
            incommingMessage = string.Empty;
            var key = Console.ReadKey(true);
            while( key.Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && incommingMessage.Length > 0)
                {
                    incommingMessage = incommingMessage[0..^1];
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    incommingMessage = "";
                    historyIndex = -1;
                }
                else if (!char.IsControl(key.KeyChar) && incommingMessage.Length < 80)
                {
                    incommingMessage += key.KeyChar;
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (messageHistory.Count > 0 && historyIndex < messageHistory.Count - 1)
                    {
                        historyIndex++;
                        incommingMessage = messageHistory[historyIndex];
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (messageHistory.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        incommingMessage = messageHistory[historyIndex];
                    }
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("> " + incommingMessage);

                key = Console.ReadKey(true);
            }

            if (!string.IsNullOrEmpty(incommingMessage) && !messageHistory.Contains(incommingMessage))
            {
                messageHistory.Add(incommingMessage);
            }
            historyIndex = -1;

            return incommingMessage;
        }

        static bool IsQuitMessage(string message)
        {
            string text = message.ToLower().TrimStart('/');
            return (text == Chatter.CommandList.QUIT || text == Chatter.CommandList.QUIT_S);
        }
    }
}

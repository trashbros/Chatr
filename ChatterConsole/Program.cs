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
using System.Net;
using System.Threading.Tasks;

namespace ChatterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
                while(string.IsNullOrEmpty(ipAddress))
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
            if(string.IsNullOrEmpty(displayName))
            {
                Console.Write("Enter your display name: ");
                displayName = Console.ReadLine();
                while (string.IsNullOrEmpty(displayName))
                {
                    displayName = Console.ReadLine();
                }
            }

            // Create a new Chatter client
            //var chatterClient = new Chatter.Client(IPAddress.Parse(ipAddress), IPAddress.Parse("239.255.10.11"), 1314);
            var chatterClient = new Chatter.Controller(ipAddress, displayName);

            // Attach a message handler
            //chatterClient.MessageReceivedEventHandler += (sender, m) =>
            //{
            //    HandleMessagingCalls(m, displayName);
            //};
            chatterClient.MessageDisplayEventHandler += (sender, m) =>
            {
                Console.Write(m + "\n> ");
            };

            // Start task to receive messages
            //Task.Run(() =>
            //{
            //    chatterClient.StartReceiving();
            //});
            chatterClient.Init();

            // Get messages and send them out
            Console.Write("> ");
            string message = Console.ReadLine().Trim();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("> ");

            while (!IsQuitMessage(message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    //chatterClient.Send(displayName + ">" + message);
                    chatterClient.SendMessage(message);
                }

                message = Console.ReadLine().Trim();
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("> ");
            };

            //chatterClient.ShutDown();
        }

        //static void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m, string displayName)
        //{
        //    // Parse out the sender name
        //    string senderName = m.Message.Split('>')[0];
        //    string text = m.Message.Substring(senderName.Length + 1);

        //    //// Check to see if this is a private message
        //    //if (text.StartsWith("/pm "))
        //    //{
        //    //    // Check to see if this private message is to me
        //    //    text = text.Substring(4).Trim();
        //    //    if (text.StartsWith(displayName + " "))
        //    //    {
        //    //        // Display the message
        //    //        text = text.Substring(displayName.Length);
        //    //        Console.Write($"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
        //    //    }
        //    //}
        //    if(text.StartsWith("/"))
        //    {
        //        text = text.TrimStart('/');
        //        HandleCommandText(text, senderName, displayName);
        //    }
        //    else
        //    {
        //        // Display the message
        //        Console.Write($"\n< { senderName }: { text }\n> ");
        //    }
        //}

        //static void HandleCommandText(string message, string senderName, string displayName)
        //{
        //    string command = message.Split(' ')[0];
        //    if(command == "pm")
        //    {
        //        string text = message.Substring(3).Trim();
        //        if (text.StartsWith(displayName + " "))
        //        {
        //            // Display the message
        //            text = text.Substring(displayName.Length);
        //            Console.Write($"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
        //        }
        //    }
        //    else if(command == "users")
        //    {
        //        //chatterClient.Send(displayName + ">" + "/userinfo");
        //    }
        //    else if(command == "userinfo")
        //    {
        //        Console.Write($"\n[{ senderName } Logged In]\n> ");
        //    }
        //}

        static bool IsQuitMessage(string message)
        {
            string text = message.ToLower();
            return (text == "/quit" || text == "/q");
        }
    }
}

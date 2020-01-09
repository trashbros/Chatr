/*
Primary command and message logic handler
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
using System.Net;
using System.Text;

namespace Chatter
{
    public class Controller
    {
        Client chatterClient;

        string LocalIP;

        string DisplayName;

        public event EventHandler<string> MessageDisplayEventHandler;

        public Controller(string localIP, string displayName)
        {
            LocalIP = localIP;
            DisplayName = displayName;
        }

        public void Init()
        {
            // Create a new Chatter client
            chatterClient = new Chatter.Client(IPAddress.Parse(LocalIP), IPAddress.Parse("239.255.10.11"), 1314);

            // Attach a message handler
            chatterClient.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m);
            };

            // Start task to receive messages
            System.Threading.Tasks.Task.Run(() =>
            {
                chatterClient.StartReceiving();
            });
        }

        public void SendMessage(string message)
        {
            // Check to see if this is a command message
            if (message.StartsWith("/"))
            {
                message = message.TrimStart('/');
                HandleOutgoingCommandText(message);
            }
            else
            {
                // Send The message
                chatterClient.Send(DisplayName + ">" + message);
            }
            //chatterClient.Send(DisplayName + ">" + message);
        }

        void HandleMessagingCalls(Chatter.MessageReceivedEventArgs m)
        {
            // Parse out the sender name
            string senderName = m.Message.Split('>')[0];
            string text = m.Message.Substring(senderName.Length + 1);

            // Check to see if this is a command message
            if (text.StartsWith("/"))
            {
                text = text.TrimStart('/');
                HandleIncomingCommandText(text, senderName);
            }
            else
            {
                // Display the message
                MessageDisplayEventHandler?.Invoke(this, $"\n< { senderName }: { text }");
                //Console.Write($"\n< { senderName }: { text }\n> ");
            }
        }

        void HandleIncomingCommandText(string message, string senderName)
        {
            string command = message.Split(' ')[0];
            switch(command)
            {
                // Private message
                case "pm":
                    string text = message.Substring(3).Trim();
                    if (text.StartsWith(DisplayName + " "))
                    {
                        // Display the message
                        text = text.Substring(DisplayName.Length);
                        MessageDisplayEventHandler?.Invoke(this, $"\n< [[PM]{ senderName }: { text.Trim() }]");
                        //Console.Write($"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
                    }
                    break;
                // Active user list request
                case "users":
                    chatterClient.Send(DisplayName + ">" + "/userinfo " + senderName);
                    break;
                // Active user return message
                case "userinfo":
                    text = message.Substring(8).Trim();
                    if (text.StartsWith(DisplayName))
                    {
                        MessageDisplayEventHandler?.Invoke(this, $"\n[{ senderName } Logged In]");
                        //Console.Write($"\n[{ senderName } Logged In]\n> ");
                    }
                    break;
                // Not a valid command
                default:
                    MessageDisplayEventHandler?.Invoke(this, $"\n< { senderName }: /{ message.Trim() }");
                    break;
            }
            //// Private message
            //if (command == "pm")
            //{
            //    string text = message.Substring(3).Trim();
            //    if (text.StartsWith(DisplayName + " "))
            //    {
            //        // Display the message
            //        text = text.Substring(DisplayName.Length);
            //        MessageDisplayEventHandler?.Invoke(this, $"\n< [[PM]{ senderName }: { text.Trim() }]");
            //        //Console.Write($"\n< [[PM]{ senderName }: { text.Trim() }]\n> ");
            //    }
            //}
            //// Active user list request
            //else if (command == "users")
            //{
            //    chatterClient.Send(DisplayName + ">" + "/userinfo " + senderName);
            //}
            //// Active user return message
            //else if (command == "userinfo")
            //{
            //    string text = message.Substring(8).Trim();
            //    if (text.StartsWith(DisplayName))
            //    {
            //        MessageDisplayEventHandler?.Invoke(this, $"\n[{ senderName } Logged In]");
            //        //Console.Write($"\n[{ senderName } Logged In]\n> ");
            //    }
            //}
        }

        void HandleOutgoingCommandText(string message)
        {
            string command = message.Split(' ')[0];
            switch(command)
            {
                // Help request
                case "h":
                case "help":
                    // MAke a string with info on all command options
                    string helptext = $"\nYou are currently connected as { DisplayName } at IP { LocalIP } \n Command syntax and their function is listed below:\n\n";
                    helptext += "/help                           Provides this help documentation\n/h\n";
                    helptext += "/quit                           Quit the application\n/q\n";
                    helptext += "/users                          Get a listing of users currently connected\n";
                    helptext += "/pm [username] [message]        Message ONLY the specified user.\n";
                    helptext += "                                Does NOT inform if user not online\n\n";
                    helptext += "This software is provided under the GNU AGPL3.0 license.\n";
                    helptext += @"The source code can be found at https://github.com/trashbros/Chatter/";
                    helptext += "\n";
                    MessageDisplayEventHandler?.Invoke(this, helptext);
                    break;
                // Not a valid command string
                default:
                    // Send The message
                    chatterClient.Send(DisplayName + ">/" + message);
                    break;
            }

            //if(command == "help" || command == "h")
            //{
            //    // MAke a string with info on all command options
            //    string helptext = $"You are currently connected as { DisplayName } at IP { LocalIP } \n Command syntax and their function is listed below:\n\n";
            //    helptext += "/help                           Provides this help documentation\n/h\n";
            //    helptext += "/quit                           Quit the application\n/q\n";
            //    helptext += "/users                          Get a listing of users currently connected\n";
            //    helptext += "/pm [username] [message]        Message ONLY the specified user.";
            //    helptext += "                                Does NOT inform if user not online";
            //    MessageDisplayEventHandler?.Invoke(this, helptext);
            //}
            //else
            //{

            //}
        }
    }
}

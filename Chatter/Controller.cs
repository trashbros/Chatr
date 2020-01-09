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

        List<string> m_onlineUsers;

        public event EventHandler<string> MessageDisplayEventHandler;

        public Controller(string localIP, string displayName)
        {
            LocalIP = localIP;
            DisplayName = displayName;
            m_onlineUsers = new List<string>();
        }

        public void Init()
        {
            // Create a new Chatter client
            chatterClient = new Client(IPAddress.Parse(LocalIP), IPAddress.Parse("239.255.10.11"), 1314);

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

            m_onlineUsers.Add(DisplayName);

            System.Threading.Thread.Sleep(2000);

            this.SendMessage("/" + CommandList.LOGON);
        }

        public void ShutDown()
        {
            this.SendMessage("/" + CommandList.QUIT);
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
                MessageDisplayEventHandler?.Invoke(this, FormatMessageText($"\n< { senderName }: { text }", (senderName != DisplayName)));
            }
        }

        void HandleIncomingCommandText(string message, string senderName)
        {
            string command = message.Split(' ')[0];
            switch(command.ToLower())
            {
                // Private message
                case CommandList.PM:
                    string text = message.Substring(CommandList.PM.Length + 1).Trim();
                    if (text.StartsWith(DisplayName + " ") || senderName == DisplayName)
                    {
                        // Display the message
                        text = text.Substring(DisplayName.Length);
                        MessageDisplayEventHandler?.Invoke(this, FormatMessageText($"\n< [PM]{ senderName }: { text.Trim() }", (senderName != DisplayName)));
                    }
                    break;
                // Active user return message
                case CommandList.USER_PING:
                    text = message.Substring(CommandList.USER_PING.Length + 1).Trim();
                    if (text.StartsWith(DisplayName))
                    {
                        m_onlineUsers.Add(senderName);
                    }
                    break;
                // User logged off
                case CommandList.LOGOFF:
                    if (senderName != DisplayName)
                    {
                        m_onlineUsers.Remove(senderName);
                        MessageDisplayEventHandler?.Invoke(this, FormatMessageText($"\n< [{ senderName } has logged off!"));
                    }
                    break;
                // User logged on
                case CommandList.LOGON:
                    if (senderName != DisplayName)
                    {
                        m_onlineUsers.Add(senderName);
                        MessageDisplayEventHandler?.Invoke(this, FormatMessageText($"\n< [{ senderName } has logged on!"));
                        chatterClient.Send(DisplayName + ">/" + CommandList.USER_PING + " " + senderName);
                    }
                    break;
                // Not a valid command, just go ahead and display it
                default:
                    MessageDisplayEventHandler?.Invoke(this, $"\n< { senderName }: /{ message.Trim() }");
                    break;
            }
        }

        void HandleOutgoingCommandText(string message)
        {
            string command = message.Split(' ')[0];
            switch(command.ToLower())
            {
                // Help request
                case CommandList.HELP_S:
                case CommandList.HELP:
                    // MAke a string with info on all command options
                    string helptext = $"\nYou are currently connected as { DisplayName } at IP { LocalIP } \n Command syntax and their function is listed below:\n\n";
                    helptext += $"/{CommandList.HELP}                           Provides this help documentation\n/{CommandList.HELP_S}\n";
                    helptext += $"/{CommandList.QUIT}                           Quit the application\n/{CommandList.QUIT_S}\n";
                    helptext += $"/{CommandList.USER_LIST}                          Get a listing of users currently connected\n";
                    helptext += $"/{CommandList.PM} [username] [message]        Message ONLY the specified user.\n";
                    helptext += "                                Does NOT inform if user not online\n\n";
                    helptext += "This software is provided under the GNU AGPL3.0 license.\n";
                    helptext += @"The source code can be found at https://github.com/trashbros/Chatter/";
                    helptext += "\n";
                    MessageDisplayEventHandler?.Invoke(this, helptext);
                    break;
                // Quit command
                case CommandList.QUIT_S:
                case CommandList.QUIT:
                    chatterClient.Send(DisplayName + ">/" + CommandList.LOGOFF);
                    break;
                // Active user list request
                case CommandList.USER_LIST:
                    string userText = "\nActive users are:\n";
                    foreach(var user in m_onlineUsers)
                    {
                        userText += user + "\n";
                    }
                    MessageDisplayEventHandler?.Invoke(this, userText);
                    break;
                // Not a valid command string
                default:
                    // Send The message
                    chatterClient.Send(DisplayName + ">/" + message);
                    break;
            }
        }

        private string FormatMessageText(string message, bool notify = false)
        {
            string formattedString = message + " \n";

            if(notify)
            {
                formattedString = "\x7" + formattedString;
            }

            return formattedString;
        }
    }
}

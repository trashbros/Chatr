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

        string m_localIP;

        string m_displayName;

        string m_multicastIP;
        int m_port;

        List<string> m_onlineUsers;

        public event EventHandler<string> MessageDisplayEventHandler;

        public Controller(string localIP, string displayName, string multicastIP = "239.255.10.11", string port = "1314")
        {
            m_localIP = localIP;
            m_displayName = displayName;
            m_multicastIP = multicastIP;
            m_port = 0;
            if(!IsValidPort(port, out m_port))
            {
                m_port = 1314;
            }
            m_onlineUsers = new List<string>();
        }

        public void Init()
        {
            NewClientConnection();
        }

        private void ConnectClient()
        {
            // Create a new Chatter client
            chatterClient = new Client(IPAddress.Parse(m_localIP), IPAddress.Parse(m_multicastIP), m_port);

            // Attach a message handler
            chatterClient.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m);
            };

            // Start task to receive messages
            _ = chatterClient.StartReceiving();

            m_onlineUsers.Add(m_displayName);

            System.Threading.Thread.Sleep(2000);

            this.SendMessage("/" + CommandList.LOGON);
        }

        public void ShutDown()
        {
            this.SendMessage("/" + CommandList.QUIT);
            chatterClient?.Dispose();
            chatterClient = null;
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
                chatterClient.Send(m_displayName + ">" + message);
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
                DisplayMessage(FormatMessageText($"< { senderName }: { text }", (senderName != m_displayName)));
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
                    if (text.StartsWith(m_displayName + " ") )
                    {
                        // Display the message
                        text = text.Substring(m_displayName.Length + 1);
                        DisplayMessage(FormatMessageText($"< [PM]{ senderName }: { text.Trim() }", true));
                    }
                    else if(senderName == m_displayName)
                    {
                        string name = text.Split(' ')[0];
                        text = text.Substring(name.Length + 1);
                        DisplayMessage(FormatMessageText($"< [PM]{ senderName } to { name }: { text.Trim() }", false));
                    }
                    break;
                // Active user return message
                case CommandList.USER_PING:
                    text = message.Substring(CommandList.USER_PING.Length + 1).Trim();
                    if (text.StartsWith(m_displayName))
                    {
                        m_onlineUsers.Add(senderName);
                    }
                    break;
                // User logged off
                case CommandList.LOGOFF:
                    if (senderName != m_displayName)
                    {
                        m_onlineUsers.Remove(senderName);
                        DisplayMessage(FormatMessageText($"< [{ senderName } has logged off!]"));
                    }
                    break;
                // User logged on
                case CommandList.LOGON:
                    if (senderName != m_displayName)
                    {
                        m_onlineUsers.Add(senderName);
                        DisplayMessage(FormatMessageText($"< [{ senderName } has logged on!]"));
                        chatterClient.Send(m_displayName + ">/" + CommandList.USER_PING + " " + senderName);
                    }
                    break;
                // Notified a user changed their display name
                case CommandList.NAME_CHANGED:
                    string newName = message.Substring(CommandList.NAME_CHANGED.Length + 1);
                    if ( senderName != m_displayName && newName != m_displayName)
                    {
                        m_onlineUsers.Remove(senderName);
                        m_onlineUsers.Add(newName);
                        DisplayMessage(FormatMessageText($"< [{ senderName } has changed to {newName}]"));
                    }
                    break;
                // Not a valid command, just go ahead and display it
                default:
                    DisplayMessage($"< { senderName }: /{ message.Trim() }");
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
                    string helptext = $"\nYou are currently connected as { m_displayName } at IP { m_localIP } \n Command syntax and their function is listed below:\n\n";
                    helptext += $"/{CommandList.HELP}       or      /{CommandList.HELP_S}\n               Provides this help documentation\n";
                    helptext += $"/{CommandList.QUIT}       or      /{CommandList.QUIT_S}\n               Quit the application\n";
                    helptext += $"/{CommandList.USER_LIST}\n               Get a listing of users currently connected\n";
                    helptext += $"/{CommandList.PM} [username] [message]\n               Message ONLY the specified user.\n";
                    helptext += "               Does NOT inform if user not online\n";
                    helptext += $"/{CommandList.CHANGE_NAME} [username]\n               Changes your currently display name\n";
                    helptext += $"/{CommandList.CHANGE_MULTICAST} [IP address]\n               Changes to a different multicast group.\n";
                    helptext += $"/{CommandList.CHANGE_PORT} [Port number]\n               Changes to a different port on the current multicast IP address\n";
                    helptext += "\nThis software is provided under the GNU AGPL3.0 license.\n";
                    helptext += @"The source code can be found at https://github.com/trashbros/Chatter/";
                    helptext += "\n";
                    DisplayMessage(helptext);
                    break;
                // Quit command
                case CommandList.QUIT_S:
                case CommandList.QUIT:
                    chatterClient.Send(m_displayName + ">/" + CommandList.LOGOFF);
                    break;
                // Active user list request
                case CommandList.USER_LIST:
                    string userText = "Active users are:\n";
                    foreach(var user in m_onlineUsers)
                    {
                        userText += user + "\n";
                    }
                    DisplayMessage(userText);
                    break;
                // Change your display name
                case CommandList.CHANGE_NAME:
                    string newName = message.Substring(CommandList.CHANGE_NAME.Length + 1);
                    chatterClient.Send(m_displayName + ">/" + CommandList.NAME_CHANGED + " " + newName);
                    m_displayName = newName;
                    break;
                // Change your multicast ip address
                case CommandList.CHANGE_MULTICAST:
                    var newIP = message.Substring(CommandList.CHANGE_MULTICAST.Length + 1);
                    if (!IsValidIP(newIP))
                    {
                        DisplayMessage("Multicast IP is not valid\n");
                    }
                    else
                    {
                        m_multicastIP = newIP;
                        NewClientConnection();
                    }
                    break;
                // Change your connection port
                case CommandList.CHANGE_PORT:
                    var portString = message.Substring(CommandList.CHANGE_PORT.Length + 1);
                    int newPort = 0;
                    if(!IsValidPort(portString, out newPort))
                    {
                        DisplayMessage("Invalid port number provided!");
                    }
                    else
                    {
                        m_port = newPort;
                        NewClientConnection();
                    }
                    break;
                // Not a valid command string
                default:
                    // Send The message
                    chatterClient.Send(m_displayName + ">/" + message);
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

        private void NewClientConnection()
        {
            // Check that our local IP is good
            if(!IsValidIP(m_localIP))
            {
                DisplayMessage("Invalid client IP provided!\n");
                return;
            }
            // Check that our multicast IP is good
            if(!IsValidIP(m_multicastIP))
            {
                DisplayMessage("Invalid multicast IP provided!\n");
                return;
            }

            if (chatterClient != null)
            {
                ShutDown();
                
            }
            ConnectClient();
            DisplayMessage($"**************\nJoined Multicast Group:\nIP: {m_multicastIP}\nPort: {m_port.ToString()}\n**************\n");
        }

        private bool IsValidIP(string ipAdress)
        {
            IPAddress testAddr;
            if(!IPAddress.TryParse(ipAdress,out testAddr))
            { return false; }
            return true;
        }

        private bool IsValidPort(string portString, out int portNum)
        {
            portNum = 0;
            if(!Int32.TryParse(portString, out portNum))
            {
                return false;
            }

            if(portNum < 0 || portNum > 65535)
            {
                return false;
            }

            return true;
        }

        private void DisplayMessage(string message)
        {
            MessageDisplayEventHandler?.Invoke(this, message);
        }
    }
}

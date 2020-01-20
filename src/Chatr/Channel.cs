/*
Primary channel command and logic control interface.
Creates and connects to client isntance
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

namespace Chatr
{
    public class Channel
    {
        #region Properties

        public string ChannelName { get { return channelSettings.ChannelName; } }
        public string DisplayName { get { return channelSettings.DisplayName; } }
        public string ConnectionIP { get { return channelSettings.ConnectionIP; } }
        public string MulticastIP { get { return channelSettings.MulticastIP; } }
        public string Port { get { return channelSettings.PortString; } }
        public string Password { get { return channelSettings.Password; } }
        public string BaseColor { get { return channelSettings.BaseColor; } }
        public string PMColor { get { return channelSettings.PMColor; } }
        public string SystemMessageColor { get { return channelSettings.SystemMessageColor; } }

        public bool IsConnected
        {
            get
            {
                if (connection != null)
                {
                    return connection.ReceiveStarted;
                }
                return false;
            }
        }

        #endregion Properties

        #region Events

        public event EventHandler<string[]> MessageDisplayEventHandler;

        #endregion Events

        #region Private Member Variables

        private MulticastConnection connection;

        private readonly ChannelSettings channelSettings;

        private readonly List<string> m_onlineUsers;

        #endregion Private Member Variables

        /// <summary>
        /// Class Initialization function. Gets log in info
        /// </summary>
        /// <param name="channelSettings"></param>
        public Channel(ChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;

            m_onlineUsers = new List<string>();
        }

        #region Public Functions

        /// <summary>
        /// Public class for initializing the connection and beginning messaging
        /// </summary>
        public void Init()
        {
            NewClientConnection();
        }

        /// <summary>
        /// Class to call to send a message out. All parsing on the outgoing side goes here.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            // Check to see if this is a command message
            if (message.StartsWith("/", StringComparison.Ordinal))
            {
                message = message.TrimStart('/');
                HandleOutgoingCommandText(message);
            }
            else
            {
                // Send The message
                connection?.Send(channelSettings.DisplayName + ">" + message);
            }
        }

        /// <summary>
        /// Command to call to shutdown the controller and internal connection on program close.
        /// </summary>
        public void ShutDown()
        {
            this.SendMessage("/" + CommandList.QUIT);
            connection?.Dispose();
            connection = null;
        }

        #endregion Public Functions

        /// <summary>
        /// Create a connection with the current channel settings
        /// </summary>
        private void ConnectClient()
        {
            var messageTransform = new PasswordEncryptedMessageTransform(channelSettings.Password, "AES");

            // Create a new connection
            connection = new MulticastConnection(IPAddress.Parse(channelSettings.ConnectionIP), new IPEndPoint(IPAddress.Parse(channelSettings.MulticastIP), channelSettings.Port), messageTransform);

            // Attach a message handler
            connection.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m);
            };

            // Start task to receive messages
            _ = connection.StartReceiving();

            // Add self as an online user
            m_onlineUsers.Add(channelSettings.DisplayName);

            // Wait to make sure we're actually recieving messages
            while (!connection.ReceiveStarted)
            {
                System.Threading.Thread.Sleep(100);
            }

            // Then send the log on command
            this.SendMessage("/" + CommandList.LOGON);
        }

        /// <summary>
        /// Our incoming message handling class the recieves messages from the multicast connection
        /// </summary>
        /// <param name="m"></param>
        private void HandleMessagingCalls(MessageReceivedEventArgs m)
        {
            // Parse out the sender name
            string senderName = m.Message.Split('>')[0];
            string text = m.Message.Substring(senderName.Length + 1);

            // Check to see if this is a command message
            if (text.StartsWith("/", StringComparison.Ordinal))
            {
                // Trim off the starting slash then try to parse the command
                text = text.TrimStart('/');
                HandleIncomingCommandText(text, senderName);
            }
            else
            {
                // Display the message
                DisplayMessage(FormatMessageText($"{ senderName }: { text }", (senderName != channelSettings.DisplayName)), BaseColor);
            }
        }

        /// <summary>
        /// Parses command text of incoming messages to properly format and respond
        /// </summary>
        /// <param name="message"></param>
        /// <param name="senderName"></param>
        private void HandleIncomingCommandText(string message, string senderName)
        {
            string command = message.Split(' ')[0];
            switch (command.ToLower())
            {
                // Private message
                case CommandList.PM:
                    string text = message.Substring(CommandList.PM.Length + 1).Trim();
                    if (text.StartsWith(channelSettings.DisplayName + " ", StringComparison.Ordinal))
                    {
                        // Display the message
                        text = text.Substring(channelSettings.DisplayName.Length + 1);
                        DisplayMessage(FormatMessageText($"[PM]{ senderName }: { text.Trim() }", true), PMColor);
                    }
                    else if (string.Compare(senderName, channelSettings.DisplayName, StringComparison.Ordinal) == 0)
                    {
                        string name = text.Split(' ')[0];
                        text = text.Substring(name.Length + 1);
                        DisplayMessage(FormatMessageText($"[PM]{ senderName } to { name }: { text.Trim() }", false), PMColor);
                    }
                    break;
                // Active user return message
                case CommandList.USER_PING:
                    text = message.Substring(CommandList.USER_PING.Length + 1).Trim();
                    if (text.StartsWith(channelSettings.DisplayName, StringComparison.Ordinal))
                    {
                        m_onlineUsers.Add(senderName);
                    }
                    break;
                // User logged off
                case CommandList.LOGOFF:
                    if (string.Compare(senderName, channelSettings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        m_onlineUsers.Remove(senderName);
                        DisplayMessage(FormatMessageText($"[{ senderName } has logged off!]"), SystemMessageColor);
                    }
                    break;
                // User logged on
                case CommandList.LOGON:
                    if (string.Compare(senderName, channelSettings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        m_onlineUsers.Add(senderName);
                        DisplayMessage(FormatMessageText($"[{ senderName } has logged on!]"), SystemMessageColor);
                        connection?.Send(channelSettings.DisplayName + ">/" + CommandList.USER_PING + " " + senderName);
                    }
                    break;
                // Notified a user changed their display name
                case CommandList.NAME_CHANGED:
                    string newName = message.Substring(CommandList.NAME_CHANGED.Length + 1).Trim();
                    if (string.Compare(senderName, channelSettings.DisplayName, StringComparison.Ordinal) != 0 && string.Compare(newName, channelSettings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        m_onlineUsers.Remove(senderName);
                        m_onlineUsers.Add(newName);
                        DisplayMessage(FormatMessageText($"[{ senderName } has changed to {newName}]"), SystemMessageColor);
                    }
                    break;
                // Not a valid command, just go ahead and display it
                default:
                    DisplayMessage(FormatMessageText($"{ senderName }: /{ message.Trim() }"), BaseColor);
                    break;
            }
        }

        /// <summary>
        /// Parse and handle commands on the outgoing side that would react back to the user.
        /// </summary>
        /// <param name="message"></param>
        private void HandleOutgoingCommandText(string message)
        {
            string command = message.Split(' ')[0];
            switch (command.ToLower())
            {
                // Quit command
                case CommandList.QUIT_S:
                case CommandList.QUIT:
                    connection?.Send(channelSettings.DisplayName + ">/" + CommandList.LOGOFF);
                    m_onlineUsers.Clear();
                    break;
                // Active user list request
                case CommandList.USER_LIST:
                    string userText = "Active users are:\n";
                    foreach (var user in m_onlineUsers)
                    {
                        userText += user + "\n";
                    }
                    DisplayMessage(userText, SystemMessageColor);
                    break;
                // Change your display name
                case CommandList.CHANGE_NAME:
                    string newName = message.Substring(CommandList.CHANGE_NAME.Length + 1);
                    connection?.Send(channelSettings.DisplayName + ">/" + CommandList.NAME_CHANGED + " " + newName);
                    channelSettings.DisplayName = newName;
                    break;
                // Change your multicast ip address
                case CommandList.CHANGE_MULTICAST:
                    var newIP = message.Substring(CommandList.CHANGE_MULTICAST.Length + 1);
                    if (!Helpers.IsValidIP(newIP))
                    {
                        DisplayMessage("Multicast IP is not valid\n", SystemMessageColor);
                    }
                    else
                    {
                        channelSettings.MulticastIP = newIP;
                        NewClientConnection();
                    }
                    break;
                // Change your connection port
                case CommandList.CHANGE_PORT:
                    var portString = message.Substring(CommandList.CHANGE_PORT.Length + 1);
                    channelSettings.PortString = portString;
                    if (channelSettings.PortString != portString)
                    {
                        DisplayMessage("Invalid port number provided!\n", SystemMessageColor);
                    }
                    else
                    {
                        NewClientConnection();
                    }
                    break;
                // Not a valid command string
                default:
                    // Send The message
                    connection?.Send(channelSettings.DisplayName + ">/" + message);
                    break;
            }
        }

        /// <summary>
        /// Message formatter to handle common formatting on messages to display to the user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="notify"></param>
        /// <returns></returns>
        private string FormatMessageText(string message, bool notify = false)
        {
            string formattedString = message + " \n";
            string dateTime = DateTime.Now.ToString();

            formattedString = $"< [{ChannelName}][{dateTime}] " + formattedString;

            if (notify)
            {
                formattedString = $"\x7" + formattedString;
            }

            return formattedString;
        }

        /// <summary>
        /// Disconnect any existing connection and create a new connection
        /// </summary>
        private void NewClientConnection()
        {
            // Check that our local IP is good
            if (!Helpers.IsValidIP(channelSettings.ConnectionIP))
            {
                DisplayMessage("Invalid client IP provided!\n", SystemMessageColor);
                return;
            }
            // Check that our multicast IP is good
            if (!Helpers.IsValidIP(channelSettings.MulticastIP))
            {
                DisplayMessage("Invalid multicast IP provided!\n", SystemMessageColor);
                return;
            }

            if (connection != null)
            {
                ShutDown();
            }
            ConnectClient();
            DisplayMessage($"**************\nJoined Multicast Group:\nIP: {channelSettings.MulticastIP}\nPort: {channelSettings.Port.ToString()}\n**************\n", SystemMessageColor);
        }

        /// <summary>
        /// Wrapper function for invoking the message display event handler
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message, string textColor)
        {
            MessageDisplayEventHandler?.Invoke(this, new string[] { message, textColor });
        }

        public override string ToString()
        {
            string channelDetails = $"\n**Channel connection information:**\n";
            channelDetails += $"\nChannel Name: {channelSettings.ChannelName}\nDisplayName: {channelSettings.DisplayName}\n";
            channelDetails += $"Local IP: {channelSettings.ConnectionIP}\nMutlicast IP: {channelSettings.MulticastIP}\n";
            channelDetails += $"Port: {channelSettings.PortString}\nPassword: {channelSettings.Password}\n\n";

            string userText = "Active users are:\n";
            foreach (var user in m_onlineUsers)
            {
                userText += user + "\n";
            }

            return channelDetails + userText;
        }
    }
}
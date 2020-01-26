/*
Primary channel command and logic control interface.

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

namespace Chatr
{
    public class Channel
    {
        #region Private Fields

        /// <summary>
        /// Collection of online users present in this channel.
        /// </summary>
        private readonly HashSet<string> _onlineUsers;

        /// <summary>
        /// <para>The settings for this channel.</para>
        /// </summary>
        private readonly ChannelSettings _settings;

        /// <summary>
        /// The connection used to send and receive messages.
        /// </summary>
        private MulticastConnection _connection;

        /// <summary>
        /// The message transform used when sending and receiving data.
        /// </summary>
        private IMessageTransform _messageTransform;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Builds a log off message.
        /// </summary>
        /// <returns>The message.</returns>
        private byte[] BuildLogOffMessage()
        {
            return Encoding.UTF8.GetBytes(_settings.DisplayName + ">/" + CommandList.LOGOFF);
        }

        /// <summary>
        /// Builds a log on message.
        /// </summary>
        /// <returns>The message.</returns>
        private byte[] BuildLogOnMessage()
        {
            return Encoding.UTF8.GetBytes(_settings.DisplayName + ">/" + CommandList.LOGON);
        }

        /// <summary>
        /// Builds a rename message.
        /// </summary>
        /// <param name="newName">The new name.</param>
        /// <returns>The message.</returns>
        private byte[] BuildRenameMessage(string newName)
        {
            return Encoding.UTF8.GetBytes(_settings.DisplayName + ">/" + CommandList.NAME_CHANGED + " " + newName);
        }

        /// <summary>
        /// Builds a text message.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The message.</returns>
        private byte[] BuildTextMessage(string text)
        {
            return Encoding.UTF8.GetBytes(_settings.DisplayName + ">" + text);
        }

        /// <summary>
        /// Create a connection with the current channel settings
        /// </summary>
        private void ConnectClient()
        {
            // Create a new message transform
            _messageTransform = new PasswordEncryptedMessageTransform(_settings.Password, "AES");

            // Create a new connection
            _connection = new MulticastConnection(IPAddress.Parse(_settings.ConnectionIP), new IPEndPoint(IPAddress.Parse(_settings.MulticastIP), _settings.Port));

            // Attach a message handler
            _connection.MessageReceivedEventHandler += (sender, m) =>
            {
                HandleMessagingCalls(m);
            };

            // Open the connection
            _connection.Open();

            // Wait for the connection to become active
            while (!_connection.Active)
            {
                System.Threading.Thread.Sleep(100);
            }

            // Add self as an online user
            _onlineUsers.Add(_settings.DisplayName);

            // Send the log on command
            Send(BuildLogOnMessage());
        }

        /// <summary>
        /// Wrapper function for invoking the message display event handler
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message, string textColor)
        {
            MessageDisplayEventHandler?.Invoke(this, new string[] { message, textColor });
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
                    if (text.StartsWith(_settings.DisplayName + " ", StringComparison.Ordinal))
                    {
                        // Display the message
                        text = text.Substring(_settings.DisplayName.Length + 1);
                        DisplayMessage(FormatMessageText($"[PM]{ senderName }: { text.Trim() }", true), PMColor);
                    }
                    else if (string.Compare(senderName, _settings.DisplayName, StringComparison.Ordinal) == 0)
                    {
                        string name = text.Split(' ')[0];
                        text = text.Substring(name.Length + 1);
                        DisplayMessage(FormatMessageText($"[PM]{ senderName } to { name }: { text.Trim() }", false), PMColor);
                    }
                    break;
                // Active user return message
                case CommandList.USER_PING:
                    text = message.Substring(CommandList.USER_PING.Length + 1).Trim();
                    if (text.StartsWith(_settings.DisplayName, StringComparison.Ordinal))
                    {
                        _onlineUsers.Add(senderName);
                    }
                    break;
                // User logged off
                case CommandList.LOGOFF:
                    if (string.Compare(senderName, _settings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        _onlineUsers.Remove(senderName);
                        DisplayMessage(FormatMessageText($"[{ senderName } has logged off!]"), SystemMessageColor);
                    }
                    break;
                // User logged on
                case CommandList.LOGON:
                    if (string.Compare(senderName, _settings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        _onlineUsers.Add(senderName);
                        DisplayMessage(FormatMessageText($"[{ senderName } has logged on!]"), SystemMessageColor);
                    }
                    break;
                // Notified a user changed their display name
                case CommandList.NAME_CHANGED:
                    string newName = message.Substring(CommandList.NAME_CHANGED.Length + 1).Trim();
                    if (string.Compare(senderName, _settings.DisplayName, StringComparison.Ordinal) != 0 && string.Compare(newName, _settings.DisplayName, StringComparison.Ordinal) != 0)
                    {
                        _onlineUsers.Remove(senderName);
                        _onlineUsers.Add(newName);
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
        /// Our incoming message handling class the recieves messages from the multicast connection
        /// </summary>
        /// <param name="m"></param>
        private void HandleMessagingCalls(MessageReceivedEventArgs m)
        {
            string message = null;
            try
            {
                message = Encoding.UTF8.GetString(_messageTransform.Decode(m.Message));
            }
            catch (Exception)
            {
                // Silently ignore invalid message
            }

            if (message != null)
            {
                // Parse out the sender name
                string senderName = message.Split('>')[0];
                string text = message.Substring(senderName.Length + 1);

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
                    DisplayMessage(FormatMessageText($"{ senderName }: { text }", (senderName != _settings.DisplayName)), BaseColor);
                }
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
                // Active user list request
                case CommandList.USER_LIST:
                    string userText = "Active users are:\n";
                    foreach (var user in _onlineUsers)
                    {
                        userText += user + "\n";
                    }
                    DisplayMessage(userText, SystemMessageColor);
                    break;
                // Change your display name
                case CommandList.CHANGE_NAME:
                    string newName = message.Substring(CommandList.CHANGE_NAME.Length + 1);
                    Send(BuildRenameMessage(newName));
                    _settings.DisplayName = newName;
                    break;
                // Change your multicast ip address
                case CommandList.CHANGE_MULTICAST:
                    var newIP = message.Substring(CommandList.CHANGE_MULTICAST.Length + 1);
                    if (!Helpers.IsValidMulticastIP(newIP))
                    {
                        DisplayMessage("Multicast IP is not valid\n", SystemMessageColor);
                    }
                    else
                    {
                        _settings.MulticastIP = newIP;
                        NewClientConnection();
                    }
                    break;
                // Change your connection port
                case CommandList.CHANGE_PORT:
                    var portString = message.Substring(CommandList.CHANGE_PORT.Length + 1);
                    _settings.PortString = portString;
                    if (_settings.PortString != portString)
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
                    Send(BuildTextMessage("/" + message));
                    break;
            }
        }

        /// <summary>
        /// Disconnect any existing connection and create a new connection
        /// </summary>
        private void NewClientConnection()
        {
            // Check that our local IP is good
            if (!Helpers.IsValidIP(_settings.ConnectionIP))
            {
                DisplayMessage("Invalid client IP provided!\n", SystemMessageColor);
                return;
            }
            // Check that our multicast IP is good
            if (!Helpers.IsValidMulticastIP(_settings.MulticastIP))
            {
                DisplayMessage("Invalid multicast IP provided!\n", SystemMessageColor);
                return;
            }

            if (_connection != null)
            {
                Disconnect();
            }
            ConnectClient();
            DisplayMessage($"**************\nJoined Multicast Group:\nIP: {_settings.MulticastIP}\nPort: {_settings.Port.ToString()}\n**************\n", SystemMessageColor);
        }

        /// <summary>
        /// Sends the specified message using the channel connection.
        /// </summary>
        /// <param name="message">The message.</param>
        private void Send(byte[] message)
        {
            _connection?.Send(_messageTransform.Encode(message));
        }

        #endregion Private Methods

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class using the specified settings.
        /// </summary>
        /// <param name="settings">The channel settings.</param>
        public Channel(ChannelSettings settings)
        {
            _settings = settings;
            _onlineUsers = new HashSet<string>();
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when message needs to be displayed.
        /// </summary>
        public event EventHandler<string[]> MessageDisplayEventHandler;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the base text color.
        /// </summary>
        /// <value>The base text color.</value>
        public string BaseColor { get { return _settings.BaseColor; } }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        /// <value>The name of the channel.</value>
        public string ChannelName { get { return _settings.ChannelName; } }

        /// <summary>
        /// Gets the connection IP address.
        /// </summary>
        /// <value>The connection IP address.</value>
        public string ConnectionIP { get { return _settings.ConnectionIP; } }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get { return _settings.DisplayName; } }

        /// <summary>
        /// Gets a value indicating whether this channel is connected.
        /// </summary>
        /// <value><c>true</c> if this channel is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get => _connection?.Active ?? false;
        }

        /// <summary>
        /// Gets the multicast IP address.
        /// </summary>
        /// <value>The multicast IP address.</value>
        public string MulticastIP { get { return _settings.MulticastIP; } }

        /// <summary>
        /// Gets the channel password.
        /// </summary>
        /// <value>The channel password.</value>
        public string Password { get { return _settings.Password; } }

        /// <summary>
        /// Gets the color used for private messages.
        /// </summary>
        /// <value>The color used for private messages.</value>
        public string PMColor { get { return _settings.PMColor; } }

        /// <summary>
        /// Gets the port number.
        /// </summary>
        /// <value>The port number.</value>
        public string Port { get { return _settings.PortString; } }

        /// <summary>
        /// Gets the color used for system messages.
        /// </summary>
        /// <value>The color used for system messages.</value>
        public string SystemMessageColor { get { return _settings.SystemMessageColor; } }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Connect to the channel.
        /// </summary>
        public void Connect()
        {
            NewClientConnection();
        }

        /// <summary>
        /// Disconnect from the channel.
        /// </summary>
        public void Disconnect()
        {
            Send(BuildLogOffMessage());

            _onlineUsers.Clear();

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }

        /// <summary>
        /// Send a message out. All parsing on the outgoing side goes here.
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
                // Send The message as a text message
                Send(BuildTextMessage(message));
            }
        }

        /// <summary>
        /// Converts the channel to a string.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this channel.</returns>
        public override string ToString()
        {
            string channelDetails = $"\n**Channel connection information:**\n";
            channelDetails += $"\nChannel Name: {_settings.ChannelName}\nDisplayName: {_settings.DisplayName}\n";
            channelDetails += $"Local IP: {_settings.ConnectionIP}\nMutlicast IP: {_settings.MulticastIP}\n";
            channelDetails += $"Port: {_settings.PortString}\nPassword: {_settings.Password}\n\n";

            string userText = "Active users are:\n";
            foreach (var user in _onlineUsers)
            {
                userText += user + "\n";
            }

            return channelDetails + userText;
        }

        #endregion Public Methods
    }
}

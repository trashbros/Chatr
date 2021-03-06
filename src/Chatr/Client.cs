﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Chatr
{
    public class Client
    {
        public event EventHandler<string[]> MessageDisplayEventHandler;

        private readonly List<Channel> channelList;

        private GlobalSettings globalSettings;

        private readonly string m_filepath;

        private int activeChannelIndex = -1;

        public Client(string filepath)
        {
            // TODO: Use config
            Configuration config = new Configuration(filepath);
            Console.WriteLine(config);

            channelList = new List<Channel>();
            m_filepath = filepath;
            ParseSettings(m_filepath);
            // TODO: instantiate channels

            SetupChannel();
        }

        public Client(string username, string ipAddress, string filepath)
        {
            channelList = new List<Channel>();
            m_filepath = filepath;
            ParseSettings(m_filepath, true);
            globalSettings.DisplayName = username;
            globalSettings.ConnectionIP = ipAddress;

            SetupChannel();
        }

        public void SetupChannel()
        {
            if(channelList.Count <= 0)
            {
                channelList.Add(new Channel(new ChannelSettings("-cn Main", globalSettings)));
                channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
            }
        }

        public void ShutDown()
        {
            QuitChannels();
            CreateSettingsFile();
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
                // Send a text message
                TextMessage(message);
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
                // Help request
                case CommandList.HELP_S:
                case CommandList.HELP:

                    // Make a string with info on all command options
                    string helptext = "\n";
                    if (activeChannelIndex > -1)
                    {
                        helptext = $"\nYou are currently chatting on {channelList[activeChannelIndex].ChannelName} as {channelList[activeChannelIndex].DisplayName} \n";
                    }
                    else
                    {
                        helptext = "\nYou are not currently chatting on any channel \n\n";
                    }
                    helptext += $@"Command syntax and their function is listed below:

| Command                    | Description                                        |
|----------------------------|----------------------------------------------------|
| /{CommandList.HELP}, /{CommandList.HELP_S}                  | Display this helpful informaion                    |
| /{CommandList.QUIT}, /{CommandList.QUIT_S}                  | Quit applicaiton                                   |
| /clear                     | Clear the console                                  |
| /{CommandList.PM} [username] [message]   | Send a private message to [username]               |
| /{CommandList.USER_LIST}                     | List active users                                  |
| /{CommandList.CHANGE_NAME} [username]           | Change current username to [username]              |
| /{CommandList.CHANGE_MULTICAST} [ip]            | Change Multicast IP address to [ip]                |
| /{CommandList.CHANGE_PORT} [port]               | Change port number to [port]                       |
| /{CommandList.CHANGE_CHANNEL} [channel]         | Change active channel to [channel]                 |
| /{CommandList.ADD_CHANNEL} [channel settings]    | Add a new channel using [channel settings]         |
| /{CommandList.CHANNEL_LIST}                      | Get a listing of connected channels                |
| /{CommandList.CHANNEL_LIST} all                  | Get a listing of ALL channels                      |
| /{CommandList.CHANNEL_INFO} [channel]            | Display information about [channel]                |
| /{CommandList.CONNECT} [channel]         | Connect to [channel]                               |

This software is provided under the GNU AGPL3.0 license.
The source code can be found at https://github.com/trashbros/Chatr/
";

                    DisplayMessage(helptext, globalSettings.SystemMessageColor);
                    break;
                // Quit command
                case CommandList.QUIT_S:
                case CommandList.QUIT:
                    QuitChannels(message);
                    break;
                // Change active channel command
                case CommandList.CHANGE_CHANNEL:
                    string channelname = message.Substring(CommandList.CHANGE_CHANNEL.Length + 1);
                    int newchan = channelList.FindIndex(channel => channel.ChannelName == channelname);
                    if (newchan > -1)
                    {
                        activeChannelIndex = newchan;
                        DisplayMessage($"Connected to channel {channelname} for chatting.\n", globalSettings.SystemMessageColor);
                    }
                    else
                    {
                        DisplayMessage($"No channel with name {channelname} could be found.\n", globalSettings.SystemMessageColor);
                    }
                    break;

                // Change user name
                case CommandList.CHANGE_NAME:
                    if (activeChannelIndex > -1)
                    {
                        string newName = message.Substring(CommandList.CHANGE_NAME.Length + 1).Trim();

                        channelList[activeChannelIndex].SetDisplayName(newName);
                        DisplayMessage($"Display name set to {newName}.\n", channelList[activeChannelIndex].SystemMessageColor);
                    }
                    else
                    {
                        DisplayMessage("Can't set display name: No active channel.\n", globalSettings.SystemMessageColor);
                    }
                    break;

                // Active user list request
                case CommandList.USER_LIST:
                    if (activeChannelIndex > -1)
                    {
                        string userText = "Active users are:\n";

                        foreach (var user in channelList[activeChannelIndex].Users)
                        {
                            userText += user + "\n";
                        }

                        DisplayMessage(userText, channelList[activeChannelIndex].SystemMessageColor);
                    }
                    else
                    {
                        DisplayMessage("Can't display active users: No active channel.\n", globalSettings.SystemMessageColor);
                    }
                    break;

                // Change your connection port
                case CommandList.CHANGE_PORT:
                    if (activeChannelIndex > -1)
                    {
                        var portString = message.Substring(CommandList.CHANGE_PORT.Length + 1);

                        if (Helpers.TryParsePort(portString, out int port))
                        {
                            DisplayMessage("Setting channel port...\n", channelList[activeChannelIndex].SystemMessageColor);
                            channelList[activeChannelIndex].SetPort(port);
                            DisplayMessage($"**************\nJoined Multicast Group:\nIP: {channelList[activeChannelIndex].MulticastIP}\nPort: {channelList[activeChannelIndex].Port.ToString()}\n**************\n", channelList[activeChannelIndex].SystemMessageColor);
                        }
                        else
                        {
                            DisplayMessage($"Can't set port to {portString}: Invalid port number provided!\n", channelList[activeChannelIndex].SystemMessageColor);
                        }
                    }
                    else
                    {
                        DisplayMessage("Can't set port: No active channel.\n", globalSettings.SystemMessageColor);
                    }
                    break;

                // Change your multicast ip address
                case CommandList.CHANGE_MULTICAST:
                    if (activeChannelIndex > -1)
                    {
                        var newIP = message.Substring(CommandList.CHANGE_MULTICAST.Length + 1);

                        if (!Helpers.IsValidMulticastIP(newIP))
                        {
                            DisplayMessage($"Can't set multicast IP to {newIP}: Multicast IP is not valid\n", channelList[activeChannelIndex].SystemMessageColor);
                        }
                        else
                        {
                            DisplayMessage("Setting multicast IP...\n", channelList[activeChannelIndex].SystemMessageColor);
                            channelList[activeChannelIndex].SetMulticastIP(newIP);
                            DisplayMessage($"**************\nJoined Multicast Group:\nIP: {channelList[activeChannelIndex].MulticastIP}\nPort: {channelList[activeChannelIndex].Port.ToString()}\n**************\n", channelList[activeChannelIndex].SystemMessageColor);
                        }
                    }
                    else
                    {
                        DisplayMessage("Can't set multicast IP: No active channel.\n", globalSettings.SystemMessageColor);
                    }
                    break;

                // Send a private message
                case CommandList.PM:
                    string targetUser = message.Split(' ')[1];
                    string text = message.Substring(message.IndexOf(targetUser) + targetUser.Length);
                    PrivateMessage(targetUser, text);
                    break;

                // Add a channel
                case CommandList.ADD_CHANNEL:
                    AddNewChannel(message);
                    break;

                // Edit a channel
                case CommandList.EDIT_CHANNEL:
                    // TODO: Add channel editing
                    break;

                // Get a listing of connected channels
                case CommandList.CHANNEL_LIST:
                    PrintChannelList(message);
                    break;

                // Get info on a specific channel
                case CommandList.CHANNEL_INFO:
                    PrintChannelInfo(message);
                    break;

                // Connect to a channel
                case CommandList.CONNECT:
                    ConnectChannels(message);
                    break;

                // Not a valid command string
                default:
                    // Send it as a text message
                    TextMessage('/' + message);
                    break;
            }
        }

        private void ConnectChannels(string message)
        {
            DisplayMessage($"Looking for channels...\n", globalSettings.SystemMessageColor);
            bool channelfound = false;
            if (message.Trim() == CommandList.CONNECT)
            {
                foreach (var channel in channelList)
                {
                    if (!channel.IsConnected)
                    {
                        DisplayMessage($"Connecting to {channel.ChannelName}...\n", globalSettings.SystemMessageColor);
                        channel.Connect();
                        DisplayMessage($"**************\nJoined Multicast Group:\nIP: {channel.MulticastIP}\nPort: {channel.Port.ToString()}\n**************\n", channel.SystemMessageColor);
                        channelfound = true;
                    }
                }
            }
            else
            {
                var cnames = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < cnames.Length; i++)
                {
                    var chan = channelList.FindIndex(c => c.ChannelName == cnames[i]);
                    if (!channelList[chan].IsConnected)
                    {
                        DisplayMessage($"Connecting to {channelList[chan].ChannelName}...\n", globalSettings.SystemMessageColor);
                        channelList[chan].Connect();
                        DisplayMessage($"**************\nJoined Multicast Group:\nIP: {channelList[chan].MulticastIP}\nPort: {channelList[chan].Port.ToString()}\n**************\n", channelList[chan].SystemMessageColor);
                        channelfound = true;
                    }
                }
            }
            if (!channelfound)
            {
                DisplayMessage($"No channels connected.\n", globalSettings.SystemMessageColor);
            }
        }

        private void QuitChannels(string message)
        {
            if (message.Trim() == CommandList.QUIT || message.Trim() == CommandList.QUIT_S)
            {
                QuitChannels();
            }
            else
            {
                var cnames = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < cnames.Length; i++)
                {
                    var chan = channelList.FindIndex(c => c.ChannelName == cnames[i]);
                    channelList[chan].Disconnect();

                    // TODO: Handle closing channel and switching to others if availabel
                }
            }
        }

        private void QuitChannels()
        {
            foreach (var channel in channelList)
            {
                channel.Disconnect();
            }
        }

        private void AddNewChannel(string message)
        {
            string channelinfo = message.Substring(CommandList.ADD_CHANNEL.Length + 1).Trim();
            AddNewChannel(new ChannelSettings(channelinfo, globalSettings), false);
        }

        private void AddNewChannel(ChannelSettings channelSettings, bool _ /* silent = true */)
        {
            // TODO: Add silent option to limit logging of channel connection stuff

            channelList.Add(new Channel(channelSettings));
            channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
            channelList[channelList.Count - 1].Connect();
        }

        /* TODO: Implement DeleteChannel
        private void DeleteChannel()
        {
            // Entirely remove channel
        }
        */

        private void PrintChannelList(string message)
        {
            bool allChannels = CommandList.CHANNEL_LIST.Length != message.Trim().Length;
            string channelnames = "Channels are:\n";
            foreach (var channel in channelList)
            {
                if (channel.IsConnected || allChannels)
                {
                    channelnames += $"{channel.ChannelName}\n";
                }
            }
            DisplayMessage(channelnames, globalSettings.SystemMessageColor);
        }

        private void PrintChannelInfo(string message)
        {
            string channelName = message.Substring(CommandList.CHANNEL_INFO.Length + 1);
            try
            {
                var channel = channelList.Find(chan => chan.ChannelName == channelName);
                DisplayMessage(channel.ToString(), channel.SystemMessageColor);
            }
            catch
            {
                DisplayMessage("Not a valid channel name.\n", globalSettings.SystemMessageColor);
            }
        }

        /// <summary>
        /// Wrapper function for invoking the message display event handler
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message, string textColor)
        {
            MessageDisplayEventHandler?.Invoke(this, new string[] { message, textColor });
        }

        /// <summary>Parse the settings from your settings file and pass them as commands to add channels and globals</summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="newSettings">if set to <c>true</c> then no settings file already existsW.</param>
        private void ParseSettings(string filepath, bool newSettings = false)
        {
            bool isglobal = true;
            bool hasglobal = false;
            string line;
            string channelInfo = "";
            List<string> channelstrings = new List<string>();

            if (!System.IO.File.Exists(filepath) || newSettings)
            {
                globalSettings = new GlobalSettings();
                return;
            }

            // Read the file and display it line by line.
            System.IO.StreamReader file =
                new System.IO.StreamReader(filepath);
            while ((line = file.ReadLine()) != null)
            {
                // Remove comments
                line = Regex.Replace(line, "#.*$", "");

                // Remove any leading or trailing white space characters
                line = line.Trim();

                // Check for null or empty after removing comments and trimming
                if (string.IsNullOrEmpty(line)) continue;

                if (line == "[GLOBAL]")
                {
                    isglobal = true;
                    hasglobal = true;
                }
                else if (line == "[CHANNEL]")
                {
                    if (!isglobal && !hasglobal)
                    {
                        channelstrings.Add(channelInfo);
                        channelInfo = "";
                    }
                    else if (!isglobal && hasglobal)
                    {
                        //ChannelSettings chanSet = new ChannelSettings(channelInfo, globalSettings);
                        channelList.Add(new Channel(new ChannelSettings(channelInfo, globalSettings)));
                        channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
                        channelInfo = "";
                    }
                    else if (isglobal && !string.IsNullOrWhiteSpace(channelInfo))
                    {
                        isglobal = false;
                        globalSettings = new GlobalSettings(channelInfo);
                        channelInfo = "";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && line != "\n")
                {
                    var args = line.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (args[0])
                    {
                        case "DisplayName":
                            channelInfo += $"-dn {args[1]} ";
                            break;

                        case "ConnectionIP":
                            channelInfo += $"-lip {args[1]} ";
                            break;

                        case "MulticastIP":
                            channelInfo += $"-mip {args[1]} ";
                            break;

                        case "Port":
                            channelInfo += $"-p {args[1]} ";
                            break;

                        case "Password":
                            channelInfo += $"-pw {args[1]} ";
                            break;

                        case "ChannelName":
                            channelInfo += $"-cn {args[1]} ";
                            break;

                        case "BaseColor":
                            channelInfo += $"-bc {args[1]} ";
                            break;

                        case "PMColor":
                            channelInfo += $"-pm {args[1]} ";
                            break;

                        case "SystemMessageColor":
                            channelInfo += $"-sc {args[1]} ";
                            break;

                        default:
                            break;
                    }
                }
            }

            file.Close();

            if (!isglobal && !string.IsNullOrWhiteSpace(channelInfo))
            {
                //ChannelSettings chanSet = SetGlobals(new ChannelSettings(channelInfo, globalSettings));
                channelList.Add(new Channel(new ChannelSettings(channelInfo, globalSettings)));
                channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
            }
            else if (isglobal && !string.IsNullOrWhiteSpace(channelInfo))
            {
                globalSettings = new GlobalSettings(channelInfo);
            }

            if (channelstrings.Count > 0)
            {
                foreach (var chan in channelstrings)
                {
                    channelList.Add(new Channel(new ChannelSettings(chan, globalSettings)));
                    channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
                }
            }
        }

        /// <summary>
        ///   <para>
        ///  Creates the settings file.</para>
        ///   <para>
        ///     <font color="#333333">From the internally held global settings data and the internally held list of channels and their settings data, this generates the .chatrconfig file.</font>
        ///   </para>
        ///   <para>
        ///     <font color="#333333">This class then takes that file data and writes it to the default .chatrconfig location (next to the program .exe) or if a path was provided at startup, it will write the file to that path instead.</font>
        ///   </para>
        ///   <para>
        ///     <font color="#333333">
        ///       <strong>NOTE:</strong> This function is called on shutdown of the client class, and silently overwrites any file at the existing location. This is how the program stores and saves any changes made while running Chatr.</font>
        ///   </para>
        /// </summary>
        private void CreateSettingsFile()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(m_filepath, false);
            file.WriteLine("[GLOBAL]");
            file.WriteLine($"DisplayName = {globalSettings.DisplayName}");
            file.WriteLine($"ConnectionIP = {globalSettings.ConnectionIP}");
            file.WriteLine($"MulticastIP = {globalSettings.MulticastIP}");
            file.WriteLine($"Port = {globalSettings.Port}");
            file.WriteLine($"Password = {globalSettings.Password}");
            file.WriteLine($"BaseColor = {globalSettings.BaseColor}");
            file.WriteLine($"PMColor = {globalSettings.PMColor}");
            file.WriteLine($"SystemMessageColor = {globalSettings.SystemMessageColor}");
            file.WriteLine("");
            foreach (var channel in channelList)
            {
                file.WriteLine("[CHANNEL]");
                file.WriteLine($"ChannelName = {channel.ChannelName}");
                if (channel.DisplayName != globalSettings.DisplayName)
                {
                    file.WriteLine($"DisplayName = {channel.DisplayName}");
                }
                if (channel.ConnectionIP != globalSettings.ConnectionIP)
                {
                    file.WriteLine($"ConnectionIP = {channel.ConnectionIP}");
                }
                if (channel.MulticastIP != globalSettings.MulticastIP)
                {
                    file.WriteLine($"MulticastIP = {channel.MulticastIP}");
                }
                if (channel.Port != globalSettings.PortString)
                {
                    file.WriteLine($"Port = {channel.Port}");
                }
                if (channel.Password != $"{channel.MulticastIP}:{channel.Port}")
                {
                    file.WriteLine($"Password = {channel.Password}");
                }
                if (channel.BaseColor != globalSettings.BaseColor)
                {
                    file.WriteLine($"BaseColor = {channel.BaseColor}");
                }
                if (channel.PMColor != globalSettings.PMColor)
                {
                    file.WriteLine($"PMColor = {channel.PMColor}");
                }
                if (channel.SystemMessageColor != globalSettings.SystemMessageColor)
                {
                    file.WriteLine($"SystemMessageColor = {channel.SystemMessageColor}");
                }
                file.WriteLine("");
            }
            file.Close();
        }

        /// <summary>Send a text message to all users in the channel.</summary>
        /// <param name="message">The message.</param>
        private void TextMessage(string message)
        {
            // Send The message
            if (activeChannelIndex > -1 && channelList[activeChannelIndex].IsConnected)
            {
                channelList[activeChannelIndex]?.SendTextMessageToAll(message);
            }
            else
            {
                DisplayMessage("No channel selected for sending\n", globalSettings.SystemMessageColor);
            }
        }

        /// <summary>Send a private text message to a user in the channel.</summary>
        /// <param name="user"">The target user.</param>
        /// <param name="message">The message.</param>
        private void PrivateMessage(string user, string message)
        {
            // Send The message
            if (activeChannelIndex > -1 && channelList[activeChannelIndex].IsConnected)
            {
                channelList[activeChannelIndex]?.SendTextMessageToOne(user, message);
            }
            else
            {
                DisplayMessage("No channel selected for sending\n", globalSettings.SystemMessageColor);
            }
        }
    }
}
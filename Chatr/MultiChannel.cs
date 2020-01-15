using System;
using System.Collections.Generic;
using System.Text;

namespace Chatr
{
    public class MultiChannel
    {
        public event EventHandler<string> MessageDisplayEventHandler;

        List<Channel> channelList;

        string globalDisplayName = "user";
        string globalConnectionIP = "localhost";
        string defaultPort = "1314";
        string defaultMulticast = "239.255.10.11";

        string m_filepath;

        int activeChannelIndex = -1;

        public MultiChannel(string filepath)
        {
            channelList = new List<Channel>();
            m_filepath = filepath;
            ParseSettings(m_filepath);
            // TODO: instantiate channels
            // TODO: Ask for channel info if no channel exists.
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
                // Send The message
                PassMessage(message);
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
                        helptext = "\nYou are not currently chatting on any channel \n";
                    }
                    helptext += "Command syntax and their function is listed below:\n\n";
                    helptext += $"/{CommandList.HELP}       or      /{CommandList.HELP_S}\n               Provides this help documentation\n";
                    helptext += $"/{CommandList.QUIT}       or      /{CommandList.QUIT_S}\n               Quit the application\n";
                    helptext += $"/{CommandList.USER_LIST}\n               Get a listing of users currently connected\n";
                    helptext += $"/{CommandList.PM} [username] [message]\n               Message ONLY the specified user.\n";
                    helptext += "               Does NOT inform if user not online\n";
                    helptext += $"/{CommandList.CHANGE_NAME} [username]\n               Changes your currently display name\n";
                    helptext += $"/{CommandList.CHANGE_MULTICAST} [IP address]\n               Changes to a different multicast group.\n";
                    helptext += $"/{CommandList.CHANGE_PORT} [Port number]\n               Changes to a different port on the current multicast IP address\n";
                    helptext += "\nThis software is provided under the GNU AGPL3.0 license.\n";
                    helptext += @"The source code can be found at https://github.com/trashbros/Chatr/";
                    helptext += "\n";
                    DisplayMessage(helptext);
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
                        DisplayMessage($"Connected to channel {channelname} for chatting.\n");
                    }
                    else
                    {
                        DisplayMessage($"No channel with name {channelname} could be found.\n");
                    }
                    break;
                // Change global username
                case CommandList.CHANGE_NAME:
                    var text = message.Substring(CommandList.CHANGE_NAME.Length + 1);
                    if(text.Contains(" -g "))
                    {
                        // TODO: Add global username change in here
                    }
                    else
                    {
                        PassMessage($"/" + message);
                    }
                    break;
                // Add a channel
                case CommandList.ADD_CHANNEL:
                    AddNewChannel(message);
                    break;
                // Edit a channel
                case CommandList.EDIT_CHANNEL:
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
                    // Send The message
                    PassMessage($"/" + message);
                    break;
            }
        }

        private void ConnectChannels(string message)
        {
            DisplayMessage($"Looking for channels...\n");
            bool channelfound = false;
            if (message.Trim() == CommandList.CONNECT)
            {
                foreach (var channel in channelList)
                {
                    if(!channel.IsConnected)
                    {
                        DisplayMessage($"Connecting to {channel.ChannelName}...\n");
                        channel.Init();
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
                    if(!channelList[chan].IsConnected)
                    {
                        DisplayMessage($"Connecting to {channelList[chan].ChannelName}...\n");
                        channelList[chan].Init();
                        channelfound = true;
                    }
                }
            }
            if(!channelfound)
            {
                DisplayMessage($"No channels connected.\n");
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
                    channelList[chan].ShutDown();
                    // TODO: Handle closing channel and switching to others if availabel
                }
            }
        }

        private void QuitChannels()
        {
            foreach (var channel in channelList)
            {
                channel.ShutDown();
            }
        }

        private void AddNewChannel(string message)
        {
            string channelinfo = message.Substring(CommandList.ADD_CHANNEL.Length + 1).Trim();
            AddNewChannel(new ChannelSettings(channelinfo), false);
        }

        private void AddNewChannel(ChannelSettings channelSettings, bool silent = true)
        {
            // TODO: Add silent option to limit logging of channel connection stuff
            channelList.Add(new Channel(SetGlobals(channelSettings)));
            channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
            channelList[channelList.Count - 1].Init();
            
        }

        private void DeleteChannel()
        {
            // TODO: Entirely remove channel
        }

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
            DisplayMessage(channelnames);
        }

        private void PrintChannelInfo(string message)
        {
            string channelName = message.Substring(CommandList.CHANNEL_INFO.Length + 1);
            try
            {
                var channelInfo = channelList.Find(channel => channel.ChannelName == channelName);
                DisplayMessage(channelInfo.ToString());
            }
            catch
            {
                DisplayMessage("Not a valid channel name.\n");
            }
        }

        /// <summary>
        /// Wrapper function for invoking the message display event handler
        /// </summary>
        /// <param name="message"></param>
        private void DisplayMessage(string message)
        {
            MessageDisplayEventHandler?.Invoke(this, message);
        }

        /// <summary>
        /// Parse the settings from your settings file and pass them as commands to add channels and globals
        /// </summary>
        /// <param name="filepath"></param>
        private void ParseSettings(string filepath)
        {
            bool isglobal = true;
            string line;
            string channelInfo = "";

            if(!System.IO.File.Exists(filepath))
            {
                return;
            }

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(filepath);
            while ((line = file.ReadLine()) != null)
            {
                if (line == "[GLOBAL]")
                {
                    isglobal = true;
                }
                else if (line == "[CHANNEL]")
                {
                    if (!isglobal)
                    {
                        ChannelSettings chanSet = SetGlobals(new ChannelSettings(channelInfo));
                        channelList.Add(new Channel(chanSet));
                        channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
                        channelInfo = "";
                    }
                    else
                    {
                        isglobal = false;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && line != "\n")
                {
                    var args = line.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (isglobal)
                    {
                        switch (args[0])
                        {
                            case "DisplayName":
                                globalDisplayName = args[1].Replace(' ', '_');
                                break;
                            case "ConnectionIP":
                                globalConnectionIP = args[1];
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
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
                            default:
                                break;
                        }
                    }
                }
            }

            file.Close();

            if(!isglobal && !string.IsNullOrWhiteSpace(channelInfo))
            {
                ChannelSettings chanSet = SetGlobals(new ChannelSettings(channelInfo));
                channelList.Add(new Channel(chanSet));
                channelList[channelList.Count - 1].MessageDisplayEventHandler += (sender, m) => { this.MessageDisplayEventHandler(sender, m); };
            }
        }

        private void CreateSettingsFile()
        {

            System.IO.StreamWriter file = new System.IO.StreamWriter(m_filepath, false);
            file.WriteLine("[GLOBAL]");
            file.WriteLine($"DisplayName = {globalDisplayName}");
            file.WriteLine($"ConnectionIP = {globalConnectionIP}");
            file.WriteLine("");
            foreach(var channel in channelList)
            {
                file.WriteLine("[CHANNEL]");
                file.WriteLine($"ChannelName = {channel.ChannelName}");
                if ( channel.DisplayName != globalDisplayName)
                {
                    file.WriteLine($"DisplayName = {channel.DisplayName}");
                }
                if(channel.ConnectionIP != globalConnectionIP)
                {
                    file.WriteLine($"ConnectionIP = {channel.ConnectionIP}");
                }
                if(channel.MulticastIP != defaultMulticast)
                {
                    file.WriteLine($"MulticastIP = {channel.MulticastIP}");
                }
                if(channel.Port != defaultPort)
                {
                    file.WriteLine($"Port = {channel.Port}");
                }
                if (channel.Password != $"{channel.MulticastIP}:{channel.Port}")
                {
                    file.WriteLine($"Password = {channel.Password}");
                }
                file.WriteLine("");
            }
            file.Close();
        }

        private ChannelSettings SetGlobals(ChannelSettings channelSettings)
        {
            if (string.IsNullOrWhiteSpace(channelSettings.DisplayName))
            {
                channelSettings.DisplayName = globalDisplayName;
            }
            if (string.IsNullOrWhiteSpace(channelSettings.ChannelName))
            {
                // TODO: Handle missing channel name... Generate name? Or abort?
            }
            if (string.IsNullOrWhiteSpace(channelSettings.ConnectionIP))
            {
                channelSettings.ConnectionIP = globalConnectionIP;
            }
            return channelSettings;
        }

        private void PassMessage(string message)
        {
            // Send The message
            if (activeChannelIndex > -1 && channelList[activeChannelIndex].IsConnected)
            {
                channelList[activeChannelIndex]?.SendMessage(message);
            }
            else
            {
                DisplayMessage("No channel selected for sending\n");
            }
        }
    }
}

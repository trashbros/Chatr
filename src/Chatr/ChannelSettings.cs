﻿/*
Storage class for channel setting information
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

namespace Chatr
{
    public class ChannelSettings
    {
        public string ChannelName { get { return m_channelName; } set { m_channelName = value.Replace(' ', '_'); } }
        private string m_channelName;
        public string DisplayName { get { return m_displayName; } set { m_displayName = value.Replace(' ', '_'); } }
        private string m_displayName;
        public string ConnectionIP { get; set; }
        public string MulticastIP { get; set; }

        public string PortString
        {
            get
            {
                return m_port.ToString();
            }
            set
            {
                if (Helpers.TryParsePort(value, out int port))
                {
                    m_port = port;
                }
            }
        }

        public int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                if (Helpers.IsValidPort(value))
                {
                    m_port = value;
                }
            }
        }

        private int m_port = 1314;
        public string Password { get; set; }
        public string BaseColor { get; set; }
        public string PMColor { get; set; }
        public string SystemMessageColor { get; set; }

        public ChannelSettings(GlobalSettings globalSettings)
        {
            SetDefaults(globalSettings);
        }

        private void SetDefaults(GlobalSettings globalSettings)
        {
            m_port = globalSettings.Port;
            MulticastIP = globalSettings.MulticastIP;
            ConnectionIP = globalSettings.ConnectionIP;
            ChannelName = "";
            Password = "";
            DisplayName = globalSettings.DisplayName;
            BaseColor = globalSettings.BaseColor;
            PMColor = globalSettings.PMColor;
            SystemMessageColor = globalSettings.SystemMessageColor;
            if (globalSettings.Password != $"{globalSettings.MulticastIP}:{globalSettings.PortString}")
            {
                Password = globalSettings.Password;
            }
        }

        public ChannelSettings(string channelinfo, GlobalSettings globalSettings)
        {
            SetDefaults(globalSettings);

            string[] channelsplit = channelinfo.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < channelsplit.Length; i++)
            {
                if (channelsplit[i].StartsWith("-", StringComparison.Ordinal))
                {
                    var command = channelsplit[i].TrimStart('-');
                    switch (command)
                    {
                        // Channel name value
                        case "cn":
                        case "channel":
                            ChannelName = channelsplit[i + 1];
                            break;
                        // Display name value
                        case "dn":
                        case "display":
                            DisplayName = channelsplit[i + 1];
                            break;
                        // Connection IP value
                        case "lip":
                        case "localip":
                            ConnectionIP = channelsplit[i + 1];
                            break;
                        // Multicast IP value
                        case "mip":
                        case "multicastip":
                            MulticastIP = channelsplit[i + 1];
                            break;
                        // Port value
                        case "p":
                        case "port":
                            PortString = channelsplit[i + 1];
                            break;
                        // Encryption password value
                        case "pw":
                        case "password":
                            Password = channelsplit[i + 1];
                            break;
                        // Base color value
                        case "bc":
                        case "basecolor":
                            BaseColor = channelsplit[i + 1];
                            break;
                        // PM color value
                        case "pm":
                        case "pmc":
                        case "pmcolor":
                            PMColor = channelsplit[i + 1];
                            break;
                        // System message color value
                        case "sc":
                        case "sysc":
                        case "syscolor":
                        case "systemcolor":
                            SystemMessageColor = channelsplit[i + 1];
                            break;

                        default:
                            break;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Password = $"{MulticastIP}:{Port}";
            }
        }
    }
}
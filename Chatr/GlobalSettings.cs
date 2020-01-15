using System;
using System.Collections.Generic;
using System.Text;

namespace Chatr
{
    public class GlobalSettings
    {
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
                if (Helpers.IsValidPort(value, out int port))
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

        public GlobalSettings()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            m_port = 1314;
            MulticastIP = "239.255.10.11";
            ConnectionIP = "";
            Password = $"{MulticastIP}:{m_port}";
            DisplayName = "";
            BaseColor = "white";
            PMColor = "white";
            SystemMessageColor = "white";
        }

        public GlobalSettings(string channelinfo)
        {
            SetDefaults();

            string[] channelsplit = channelinfo.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < channelsplit.Length; i++)
            {
                if (channelsplit[i].StartsWith("-", StringComparison.Ordinal))
                {
                    var command = channelsplit[i].TrimStart('-');
                    switch (command)
                    {
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
        }
    }
}

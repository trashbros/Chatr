using System;
using System.Collections.Generic;
using System.Text;
using IniUtils;

namespace Chatr
{
    public class Configuration
    {
        private readonly IniFile _iniFile;

        public Dictionary<string, ColorScheme> ColorSchemes = new Dictionary<string, ColorScheme>();
        public Dictionary<string, ConnectionInfo> ConnectionInfos = new Dictionary<string, ConnectionInfo>();
        public Dictionary<string, ChannelInfo> ChannelInfos = new Dictionary<string, ChannelInfo>();
        public string StartupChannel = null;

        public Configuration(string path)
        {
            _iniFile = new IniFile(path);

            ParseSettings();
        }

        private void ParseSettings()
        {
            foreach (var sectionName in _iniFile.SectionNames())
            {
                if (sectionName.StartsWith("ColorScheme"))
                {
                    var name = sectionName.Replace("ColorScheme", "").Trim();
                    ParseColorScheme(name, sectionName);
                }
                else if (sectionName.StartsWith("ConnectionInfo"))
                {
                    var name = sectionName.Replace("ConnectionInfo", "").Trim();
                    ParseConnectionInfo(name, sectionName);
                }
                else if (sectionName.StartsWith("ChannelInfo"))
                {
                    var name = sectionName.Replace("ChannelInfo", "").Trim();
                    ParseChannelInfo(name, sectionName);
                }
                else if (sectionName.StartsWith("System"))
                {
                    ParseSystem(sectionName);
                }
            }
        }

        private void ParseSystem(string sectionName)
        {
            StartupChannel = _iniFile.ReadSetting(sectionName, "StartupChannel", null).Value;
        }

        private void ParseChannelInfo(string name, string sectionName)
        {
            var channelInfo = ChannelInfos.ContainsKey(name) ? ChannelInfos[name] : new ChannelInfo(name);

            var parent = _iniFile.ReadSetting(sectionName, "Parent", null).Value;
            if (!string.IsNullOrEmpty(parent))
            {
                channelInfo.Parent = ChannelInfos.ContainsKey(parent)
                    ? ChannelInfos[parent]
                    : new ChannelInfo(parent);
            }


            channelInfo.ColorSchemeName = _iniFile.ReadSetting(sectionName, "ColorScheme", null).Value;
            channelInfo.ConnectionInfoName = _iniFile.ReadSetting(sectionName, "ConnectionInfo", null).Value;
            channelInfo.DisplayName = _iniFile.ReadSetting(sectionName, "DisplayName", null).Value;
            channelInfo.Password = _iniFile.ReadSetting(sectionName, "Password", null).Value;

            ChannelInfos[name] = channelInfo;

        }

        private void ParseConnectionInfo(string name, string sectionName)
        {
            var connectionInfo = ConnectionInfos.ContainsKey(name) ? ConnectionInfos[name] : new ConnectionInfo(name);

            var parent = _iniFile.ReadSetting(sectionName, "Parent", null).Value;
            if (!string.IsNullOrEmpty(parent))
            {
                connectionInfo.Parent = ConnectionInfos.ContainsKey(parent)
                    ? ConnectionInfos[parent]
                    : new ConnectionInfo(parent);
            }

            connectionInfo.LocalIPAddress = _iniFile.ReadSetting(sectionName, "LocalIPAddress", null).Value;
            connectionInfo.MulticastIPAddress = _iniFile.ReadSetting(sectionName, "MulticastIPAddress", null).Value;
            connectionInfo.MulticastPort = _iniFile.ReadSetting(sectionName, "MulticastPort", null).Value;

            ConnectionInfos[name] = connectionInfo;
        }

        private void ParseColorScheme(string name, string sectionName)
        {
            var colorScheme = ColorSchemes.ContainsKey(name) ? ColorSchemes[name] : new ColorScheme(name);

            var parent = _iniFile.ReadSetting(sectionName, "Parent", null).Value;
            if (!string.IsNullOrEmpty(parent))
            {
                colorScheme.Parent = ColorSchemes.ContainsKey(parent)
                    ? ColorSchemes[parent]
                    : new ColorScheme(parent);
            }

            colorScheme.RecvMsgColor = _iniFile.ReadSetting(sectionName, "RecvMsgColor", null).Value;
            colorScheme.SendMsgColor = _iniFile.ReadSetting(sectionName, "SendMsgColor", null).Value;
            colorScheme.PrivateMsgColor = _iniFile.ReadSetting(sectionName, "PrivateMsgColor", null).Value;
            colorScheme.SystemMsgColor = _iniFile.ReadSetting(sectionName, "SystemMsgColor", null).Value;

            ColorSchemes[name] = colorScheme;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("System:");
            sb.AppendLine($"StartupChannel   {StartupChannel ?? "<None>"}");
            sb.AppendLine();

            sb.AppendLine("Color Schemes:");
            foreach (var colorScheme in ColorSchemes.Values)
            {
                sb.AppendLine(colorScheme.ToString());
            }

            sb.AppendLine("Connection Infos:");
            foreach (var connectionInfo in ConnectionInfos.Values)
            {
                sb.AppendLine(connectionInfo.ToString());
            }

            sb.AppendLine("Channel Infos:");
            foreach (var channelInfo in ChannelInfos.Values)
            {
                sb.AppendLine(channelInfo.ToString());
            }

            return sb.ToString();
        }
    }
}

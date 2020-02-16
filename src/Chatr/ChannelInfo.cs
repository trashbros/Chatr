using System;
using System.Collections.Generic;
using System.Text;

namespace Chatr
{
    public class ChannelInfo
    {
        private readonly dynamic _settingsChain;

        public string ColorSchemeName
        {
            get => _settingsChain.ColorSchemeName;
            set => _settingsChain.ColorSchemeName = value;
        }

        public string OwnColorSchemeName => _settingsChain.OwnColorSchemeName;

        public string ConnectionInfoName
        {
            get => _settingsChain.ConnectionInfoName;
            set => _settingsChain.ConnectionInfoName = value;
        }

        public string OwnConnectionInfoName => _settingsChain.OwnConnectionInfoName;

        public string DisplayName
        {
            get => _settingsChain.DisplayName;
            set => _settingsChain.DisplayName = value;
        }

        public string OwnDisplayName => _settingsChain.OwnDisplayName;

        public string Password
        {
            get => _settingsChain.Password;
            set => _settingsChain.Password = value;
        }

        public string OwnPassword => _settingsChain.OwnPassword;

        private ChannelInfo _parent;

        public ChannelInfo Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                _settingsChain.Parent = value?._settingsChain;
            }
        }

        public string Name { get; }

        public ChannelInfo(string name, ChannelInfo parent = null)
        {
            _settingsChain = new SettingsChain();
            Name = name;
            Parent = parent;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Name                {Name}");
            sb.AppendLine($"Parent Name         {Parent?.Name ?? "<None>"}");
            sb.AppendLine($"ColorScheme         {ColorSchemeName} ({OwnColorSchemeName})");
            sb.AppendLine($"ConnectionInfo      {ConnectionInfoName} ({OwnConnectionInfoName})");
            sb.AppendLine($"Password            {Password} ({OwnPassword})");

            return sb.ToString();
        }
    }
}

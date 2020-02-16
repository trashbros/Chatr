using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Chatr
{
    public class ConnectionInfo
    {
        private readonly dynamic _settingsChain;

        public string LocalIPAddress
        {
            get => _settingsChain.LocalIPAddress;
            set => _settingsChain.LocalIPAddress = value;
        }

        public string OwnLocalIPAddress => _settingsChain.OwnLocalIPAddress;

        public string MulticastIPAddress
        {
            get => _settingsChain.MulticastIPAddress;
            set => _settingsChain.MulticastIPAddress = value;
        }

        public string OwnMulticastIPAddress => _settingsChain.OwnMulticastIPAddress;

        public string MulticastPort
        {
            get => _settingsChain.MulticastPort;
            set => _settingsChain.MulticastPort = value;
        }

        public string OwnMulticastPort => _settingsChain.OwnMulticastPort;

        private ConnectionInfo _parent;
        public ConnectionInfo Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                _settingsChain.Parent = value?._settingsChain;
            }
        }

        public string Name { get; }

        public ConnectionInfo(string name, ConnectionInfo parent = null)
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
            sb.AppendLine($"LocalIPAddress      {LocalIPAddress} ({OwnLocalIPAddress})");
            sb.AppendLine($"MulticastIPAddress  {MulticastIPAddress} ({OwnMulticastIPAddress})");
            sb.AppendLine($"MulticastPort       {MulticastPort} ({OwnMulticastPort})");

            return sb.ToString();
        }
    }
}

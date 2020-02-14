using System;
using System.Collections.Generic;
using System.Text;

namespace Chatr
{
    public class ColorScheme
    {
        private readonly dynamic _settingsChain;

        public string RecvMsgColor
        {
            get => _settingsChain.RecvMsgColor ?? "white";
            set => _settingsChain.RecvMsgColor = value;
        }

        public string OwnRecvMsgColor => _settingsChain.OwnRecvMsgColor;

        public string SendMsgColor
        {
            get => _settingsChain.SendMsgColor ?? "gray";
            set => _settingsChain.SendMsgColor = value;
        }

        public string OwnSendMsgColor => _settingsChain.OwnSendMsgColor;

        public string PrivateMsgColor
        {
            get => _settingsChain.PrivateMsgColor ?? "magenta";
            set => _settingsChain.PrivateMsgColor = value;
        }

        public string OwnPrivateMsgColor => _settingsChain.OwnPrivateMsgColor;

        public string SystemMsgColor
        {
            get => _settingsChain.SysMsgColor ?? "yellow";
            set => _settingsChain.SysMsgColor = value;
        }

        public string OwnSystemMsgColor => _settingsChain.OwnSystemMsgColor;

        private ColorScheme _parent;
        public ColorScheme Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                _settingsChain.Parent = value?._settingsChain;
            }
        }

        public string Name { get; }

        public ColorScheme(string name, ColorScheme parent = null)
        {
            _settingsChain = new SettingsChain();
            Name = name;
            Parent = parent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TrashBros.IniUtils;

namespace Chatr
{
    public class Configuration
    {
        private readonly IniFile _iniFile;

        public Dictionary<string, ColorScheme> ColorSchemes = new Dictionary<string, ColorScheme>();

        public Configuration(string path)
        {
            _iniFile = new IniFile(path);

            CreateDefaults();
            ParseSettings();
        }

        private void CreateDefaults()
        {
            ColorSchemes.Add("Default", new ColorScheme("Default"));
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
            }
        }

        private void ParseColorScheme(string name, string sectionName)
        {
            var colorScheme = ColorSchemes.ContainsKey(name) ? ColorSchemes[name] : new ColorScheme(name);

            var parent = _iniFile.ReadSetting(sectionName, "Parent", null).Value;
            if (!string.IsNullOrEmpty(parent))
            {
                colorScheme.Parent = ColorSchemes.ContainsKey(parent) ? ColorSchemes[parent] : new ColorScheme(parent);
            }

            colorScheme.RecvMsgColor = _iniFile.ReadSetting(sectionName, "RecvMsgColor", null).Value;
            colorScheme.SendMsgColor = _iniFile.ReadSetting(sectionName, "SendMsgColor", null).Value;
            colorScheme.PrivateMsgColor = _iniFile.ReadSetting(sectionName, "PrivateMsgColor", null).Value;
            colorScheme.SystemMsgColor = _iniFile.ReadSetting(sectionName, "SystemMsgColor", null).Value;

            ColorSchemes[name] = colorScheme;
        }
    }
}

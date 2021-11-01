using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MewsToolbox
{
    public class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> iniContent;
        private string FilePath;
        public static IFormatProvider Culture;

        public IniFile(string filePath)
        {
            FilePath = filePath;
            Culture = new CultureInfo("en-US");
            var fileContent = File.ReadAllLines(FilePath);
            iniContent = new Dictionary<string, Dictionary<string, string>>();
            ParseFile(fileContent);
        }

        public Dictionary<string, string> this[string setting]
        {
            get { return iniContent[setting]; }
        }

        private void ParseFile(string[] content)
        {
            string section = "";

            foreach (string line in content)
            {
                if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line)) continue; // This is either a comment or an empty line, we don't count it.
                else if (line.StartsWith("[") && line.EndsWith("]")) // This is a section, we want to create a new sub dictionary
                {
                    string sectionName = line.Substring(1, line.Length - 2);
                    if (string.IsNullOrWhiteSpace(sectionName)) continue; // We don't want to handle an empty value
                    iniContent.Add(sectionName, new Dictionary<string, string>());
                    section = sectionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(section)) continue; // No empty values
                    if (!line.Contains("=")) continue;
                    var substrings = line.Split('=');
                    iniContent[section].Add(substrings[0], substrings[1]);
                }
            }
        }

        private string MakeString()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (KeyValuePair<string, Dictionary<string, string>> pair in iniContent)
            {
                sb.AppendLine("[" + pair.Key + "]");
                foreach (KeyValuePair<string, string> valuePair in pair.Value)
                {
                    sb.AppendLine(valuePair.Key + "=" + valuePair.Value.ToString(Culture));
                }
            }

            return sb.ToString();
        }

        public void AddSetting(string section, string settingName, string defaultValue)
        {
            if (!iniContent.ContainsKey(section)) iniContent.Add(section, new Dictionary<string, string>());
            iniContent[section].Add(settingName, defaultValue);
        }

        public void SaveFile() => SaveFile(FilePath);

        public void SaveFile(string filePath)
        {
            File.WriteAllText(filePath, MakeString());
        }

        public T GetValueOrDefaultTo<T>(string section, string setting, T defaultValue)
        {
            try
            {
                var value = iniContent[section][setting];
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.String:
                        return (T)(object)value;

                    case TypeCode.Boolean:
                        return (T)(object)bool.Parse(value);

                    case TypeCode.Int32:
                        return (T)(object)int.Parse(value);

                    case TypeCode.Decimal:
                        return (T)(object)decimal.Parse(value, Culture);

                    default:
                        return defaultValue;
                }
            }
            catch
            {
                if (!iniContent.ContainsKey(section))
                    iniContent.Add(section, new Dictionary<string, string>());
                if (!iniContent[section].ContainsKey(setting))
                    iniContent[section].Add(setting, defaultValue.ToString());
                SaveFile();
                return defaultValue;
            }
        }
    }
}

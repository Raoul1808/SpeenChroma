using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MewsToolbox
{
    /// <summary>
    /// Wrapper class around the .ini file format
    /// </summary>
    public class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> iniContent;
        private string FilePath;
        public IFormatProvider Culture;

        public int SectionCount => iniContent.Count;

        /// <summary>
        /// Creates a new Ini file handler for the file at the given path. /!\ THE FILE MUST EXIST FIRST. IT WILL NOT BE CREATED AUTOMATICALLY IF MISSING
        /// </summary>
        /// <param name="filePath">The path to the file. Make sure the given path is valid!</param>
        public IniFile(string filePath)
        {
            FilePath = filePath;
            Culture = new CultureInfo("en-US");
            var fileContent = File.ReadAllLines(FilePath);
            iniContent = new Dictionary<string, Dictionary<string, string>>();
            ParseFile(fileContent);
        }

        /// <summary>
        /// Get a section as a string-string dictionary
        /// </summary>
        /// <param name="section">The section to get</param>
        /// <returns>The section as a string-string dictionary</returns>
        public Dictionary<string, string> this[string section]
        {
            get { return iniContent[section]; }
        }

        private void ParseFile(string[] content)
        {
            string section = "";

            foreach (string line in content)
            {
                if (line.StartsWith(";") || line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue; // This is either a comment or an empty line, we don't count it.
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
            var sb = new StringBuilder();

            foreach (var pair in iniContent)
            {
                sb.AppendLine("[" + pair.Key + "]");
                foreach (KeyValuePair<string, string> valuePair in pair.Value)
                {
                    sb.AppendLine(valuePair.Key + "=" + valuePair.Value.ToString(Culture));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Saves the file
        /// </summary>
        public void SaveFile() => SaveFile(FilePath);

        /// <summary>
        /// Saves the file at a certain path
        /// </summary>
        /// <param name="filePath">The path to save the file to</param>
        public void SaveFile(string filePath)
        {
            File.WriteAllText(filePath, MakeString());
        }

        /// <summary>
        /// Reloads the file if any external changes were made
        /// </summary>
        public void ReloadFile()
        {
            iniContent.Clear();
            ParseFile(File.ReadAllLines(FilePath));
        }

        /// <summary>
        /// Safely get a value from the config file. It will automatically be cast to the requested type. If an error occurs or the config entry is missing, it can be created automatically.
        /// </summary>
        /// <typeparam name="T">The type to cast the config entry to. Currently supported: string, bool, int, float, double, decimal</typeparam>
        /// <param name="section">The config section to get the value from</param>
        /// <param name="setting">The config entry to get the value from</param>
        /// <param name="defaultValue">The value returned if the entry was not found or a cast isn't supported. In the first case, this value can be used to make a new config entry</param>
        /// <param name="setIfDoesntExist">If set to true, the config file will be updated with the new value. Default: true</param>
        /// <param name="saveIfDoesntExist">If set to true, the config file with the new value will be immediately saved. Default: true</param>
        /// <returns>The setting you're looking for, or defaultValue if the value is not found</returns>
        public T GetValueOrDefaultTo<T>(string section, string setting, T defaultValue, bool setIfDoesntExist = true, bool saveIfDoesntExist = true)
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

                    case TypeCode.Single:
                        return (T)(object)float.Parse(value, Culture);

                    case TypeCode.Double:
                        return (T)(object)double.Parse(value, Culture);

                    case TypeCode.Decimal:
                        return (T)(object)decimal.Parse(value, Culture);

                    default:
                        return defaultValue;
                }
            }
            catch
            {
                if (setIfDoesntExist)
                    SetValue(section, setting, defaultValue, saveIfDoesntExist);
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a value in the config
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="section">The section to save the value to</param>
        /// <param name="setting">The config entry to save the value to</param>
        /// <param name="value">The value to save</param>
        /// <param name="immediatelySave">If set to true, immediately save the config file afterwards.</param>
        public void SetValue<T>(string section, string setting, T value, bool immediatelySave = true)
        {
            if (!iniContent.ContainsKey(section))
                iniContent.Add(section, new Dictionary<string, string>());
            if (!iniContent[section].ContainsKey(setting))
                iniContent[section].Add(setting, value.ToString());
            else iniContent[section][setting] = value.ToString();
            if (immediatelySave) SaveFile();
        }

        /// <summary>
        /// Checks if a section exists
        /// </summary>
        /// <param name="section">The section to check</param>
        /// <returns>True if the section exists, False otherwise</returns>
        public bool Exists(string section) => iniContent.ContainsKey(section);

        /// <summary>
        /// Checks if a setting exists
        /// </summary>
        /// <param name="section">The section to check</param>
        /// <param name="setting">The setting to check</param>
        /// <returns>True if both the section and the setting exist, False otherwise</returns>
        public bool Exists(string section, string setting) => iniContent.ContainsKey(section) && iniContent[section].ContainsKey(setting);
    }
}

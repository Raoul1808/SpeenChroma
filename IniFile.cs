using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MewsToolbox
{
    public class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> iniContent;
        private string FilePath;

        public IniFile(string filePath)
        {
            FilePath = filePath;
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
                if (line.StartsWith(";")) continue; // This is a comment, we don't want to count it
                else if (line.StartsWith("[") && line.EndsWith("]")) // This is a section, we want to create a new sub dictionary
                {
                    string sectionName = line.Substring(1, line.Length - 2);
                    if (string.IsNullOrWhiteSpace(sectionName)) continue; // We don't want to handle an empty value
                    iniContent.Add(sectionName, new Dictionary<string, string>());
                    section = sectionName;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(section)) continue;
                    var substrings = line.Split('=', 2);
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
                    sb.AppendLine(valuePair.Key + "=" + valuePair.Value);
                }
            }

            return sb.ToString();
        }

        public void MakeNewSection(string sectionName)
        {

        }

        public void SaveFile() => SaveFile(FilePath);

        public void SaveFile(string filePath)
        {
            File.WriteAllText(filePath, MakeString());
        }
    }
}

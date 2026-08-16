using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlexRegistry.Utils
{
    public class IniFileService
    {
        private readonly string _filePath;

        public string FilePath => _filePath;

        public IniFileService(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "");
            }
        }

        public IEnumerable<string> ReadSectionValues(string section)
        {
            var lines = File.ReadAllLines(_filePath);
            bool inSection = false;
            List<string> sections = new List<string>();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith($"[{section}]"))
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]")) break;

                    sections.Add(trimmed.Split('=').LastOrDefault());
                }
            }
            return sections;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadSectionKeyValues(string section)
        {
            var lines = File.ReadAllLines(_filePath);
            bool inSection = false;
            var sections = new List<KeyValuePair<string, string>>();

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith($"[{section}]"))
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]")) break;
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;
                    var splitted = trimmed.Split('=');
                    sections.Add(new KeyValuePair<string, string>(splitted.FirstOrDefault(), splitted.LastOrDefault()));
                }
            }
            return sections;
        }

        public string Read(string section, string key)
        {
            var lines = File.ReadAllLines(_filePath);
            bool inSection = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith($"[{section}]"))
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]")) break;

                    if (trimmed.StartsWith(key))
                    {
                        return trimmed.Split('=').LastOrDefault()?.Trim();
                    }
                }
            }
            return null;
        }

        public void RemoveKey(string section, string key)
        {
            var lines = File.ReadAllLines(_filePath).ToList();
            bool inSection = false;
            bool removed = false;

            for(int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed.StartsWith($"[{section}]"))
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]")) break;

                    if (trimmed.StartsWith(key))
                    {
                        lines.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }
            if (removed)
            {
                File.WriteAllLines(_filePath, lines);
            }
        }
        //TODO Сделать работающую запись при пустой секции
        public void Write(string section, string key, string value)
        {
            var lines = File.ReadAllLines(_filePath).ToList();
            bool sectionExists = false;
            int sectionIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals($"[{section}]"))
                {
                    sectionExists = true;
                    sectionIndex = i;
                    break;
                }
            }

            if (!sectionExists)
            {
                lines.Add($"[{section}]");
                lines.Add($"{key}={value}");
                File.WriteAllLines(_filePath, lines);
                return;
            }

            for (int i = sectionIndex + 1; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith(key + "="))
                {
                    lines[i] = $"{key}={value}";
                    File.WriteAllLines(_filePath, lines);
                    return;
                }

                if (i + 1 == lines.Count || lines[i + 1].Trim().StartsWith("["))
                {
                    lines.Insert(i, $"{key}={value}");
                    File.WriteAllLines(_filePath, lines);
                    return;
                }
                if (string.IsNullOrWhiteSpace(lines[i]) && i < lines.Count)
                {
                    lines[i] = $"{key}={value}";
                    File.WriteAllLines(_filePath, lines);
                    return;
                }
            }
            if (sectionExists)
            {
                lines.Insert(sectionIndex + 1, $"{key}={value}");
                File.WriteAllLines(_filePath, lines);
            }
        }
    }
}

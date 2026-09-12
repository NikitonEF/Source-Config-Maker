using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SourceConfigMaker.Models;

public class ParsedConfig
{
    public Dictionary<string, string> Binds { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Cvars { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Aliases { get; } = new();
    public bool HasUnbindAll { get; set; }
}

public static class ConfigParser
{
    // ИСПРАВЛЕНО: Теперь регулярка проглатывает любые пробелы/табы и опциональные кавычки
    private static readonly Regex BindRegex = new(@"^bind\s+""?([^""\s]+)""?\s+(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UnbindRegex = new(@"^unbind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AliasRegex = new(@"^alias\s+""?([^""\s]+)""?\s+""?([^""]*)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ParsedConfig Parse(string filePath)
    {
        var result = new ParsedConfig();
        if (!File.Exists(filePath)) return result;

        string[] lines = File.ReadAllLines(filePath);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.Equals("unbindall", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("unbindall;", StringComparison.OrdinalIgnoreCase))
            {
                result.Binds.Clear();
                result.HasUnbindAll = true;
                continue;
            }

            int commentIdx = line.IndexOf("//");
            if (commentIdx >= 0) line = line.Substring(0, commentIdx).Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            Match bindMatch = BindRegex.Match(line);
            if (bindMatch.Success)
            {
                string key = bindMatch.Groups[1].Value.ToUpper();
                // Очищаем команду от лишних кавычек, которые могли остаться в захвате
                string val = bindMatch.Groups[2].Value.Trim().Trim('"').Replace("\"", "");

                if (!string.IsNullOrWhiteSpace(key))
                {
                    result.Binds[key] = val;
                }
                continue;
            }

            Match unbindMatch = UnbindRegex.Match(line);
            if (unbindMatch.Success)
            {
                result.Binds[unbindMatch.Groups[1].Value.ToUpper()] = "UNBIND";
                continue;
            }

            if (AliasRegex.IsMatch(line))
            {
                result.Aliases.Add(rawLine.Trim());
                continue;
            }

            var parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                string key = parts[0];
                string value = parts[1].Trim('"');
                result.Cvars[key] = value;
            }
        }

        return result;
    }
}
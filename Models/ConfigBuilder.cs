using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SourceConfigMaker.ViewModels;

namespace SourceConfigMaker.Models;

public static class ConfigBuilder
{
    private static readonly Regex BindRegex = new(@"^bind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UnbindRegex = new(@"^unbind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AliasRegex = new(@"^alias\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CvarKeyRegex = new(@"^([a-zA-Z0-9_]+)(?=\s|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Константа для ровного столбца значений
    private const int ColumnWidth = 28;

    public static async Task<string> BuildAsync(
        KeyboardViewModel keyboard,
        SettingsViewModel settings,
        bool hasUnbindAll,
        bool isCrosshairEnabled,
        string originalFilePath = null)
    {
        var newLines = new List<string>();
        var handledBinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var handledGenerals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var currentBinds = keyboard.Keys.Values
            .Where(k => !string.IsNullOrWhiteSpace(k.UserBind))
            .ToDictionary(k => k.KeyName, k => k.UserBind, StringComparer.OrdinalIgnoreCase);

        var allSettings = settings.MainSettings.Concat(settings.NetSettings)
            .Concat(settings.SoundSettings)
            .Concat(settings.VideoSettings)
            .Concat(settings.CrosshairSettings)
            .ToDictionary(c => c.Name, c => c.Value, StringComparer.OrdinalIgnoreCase);

        bool isNewConfig = string.IsNullOrEmpty(originalFilePath) || !File.Exists(originalFilePath);

        if (isNewConfig)
        {
            BuildNewConfig(newLines, currentBinds, allSettings, hasUnbindAll, isCrosshairEnabled);
        }
        else
        {
            await MergeConfigAsync(originalFilePath, newLines, currentBinds, allSettings, hasUnbindAll, isCrosshairEnabled, handledBinds, handledGenerals);
        }

        AppendAliases(newLines, settings.PresetsVm);

        return string.Join(Environment.NewLine, newLines);
    }

    // Хелперы для красивого выравнивания
    private static string FormatCvar(string key, string value) => $"{key}".PadRight(ColumnWidth) + $"\"{value}\"";
    private static string FormatBind(string key, string value) => $"bind \"{key}\"".PadRight(ColumnWidth) + $"\"{value}\"";

    private static void BuildNewConfig(List<string> newLines, Dictionary<string, string> currentBinds, Dictionary<string, string> allSettings, bool hasUnbindAll, bool isCrosshairEnabled)
    {
        newLines.Add("// Сделано при помощи Source Config Maker\n");

        if (hasUnbindAll) newLines.Add("unbindall\n");

        newLines.Add("// --- ОСНОВНЫЕ НАСТРОЙКИ ---");
        foreach (var kvp in allSettings)
        {
            if (!isCrosshairEnabled && kvp.Key.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase)) continue;

            if (!string.IsNullOrWhiteSpace(kvp.Value))
            {
                newLines.Add(FormatCvar(kvp.Key, kvp.Value));
            }
        }

        newLines.Add("\n// --- БИНДЫ ---");
        foreach (var kvp in currentBinds)
        {
            if (kvp.Value.Equals("UNBIND", StringComparison.OrdinalIgnoreCase))
                newLines.Add($"unbind \"{kvp.Key.ToUpper()}\"");
            else
                newLines.Add(FormatBind(kvp.Key.ToUpper(), kvp.Value));
        }
    }

    private static async Task MergeConfigAsync(
        string originalFilePath,
        List<string> newLines,
        Dictionary<string, string> currentBinds,
        Dictionary<string, string> allSettings,
        bool hasUnbindAll,
        bool isCrosshairEnabled,
        HashSet<string> handledBinds,
        HashSet<string> handledGenerals)
    {
        var originalLines = await File.ReadAllLinesAsync(originalFilePath);
        bool unbindallAdded = false;

        foreach (string rawLine in originalLines)
        {
            string line = rawLine.Trim();

            if (line.Equals("unbindall", StringComparison.OrdinalIgnoreCase))
            {
                if (hasUnbindAll && !unbindallAdded)
                {
                    newLines.Add("unbindall");
                    unbindallAdded = true;
                }
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                if ((line.Contains("Half-Life Config Maker") || line.Contains("Source Config Maker")) && hasUnbindAll && !unbindallAdded)
                {
                    newLines.Add(rawLine);
                    newLines.Add("unbindall");
                    unbindallAdded = true;
                    continue;
                }
                newLines.Add(rawLine);
                continue;
            }

            if (AliasRegex.IsMatch(line)) continue;

            var bindMatch = BindRegex.Match(line);
            if (bindMatch.Success)
            {
                string key = bindMatch.Groups[1].Value.ToUpper();
                if (currentBinds.TryGetValue(key, out string newValue))
                {
                    if (newValue.Equals("UNBIND", StringComparison.OrdinalIgnoreCase))
                        newLines.Add($"unbind \"{key}\"");
                    else
                        newLines.Add(FormatBind(key, newValue));
                    handledBinds.Add(key);
                }
                continue;
            }

            var unbindMatch = UnbindRegex.Match(line);
            if (unbindMatch.Success)
            {
                string key = unbindMatch.Groups[1].Value.ToUpper();
                if (currentBinds.TryGetValue(key, out string newValue))
                {
                    if (newValue.Equals("UNBIND", StringComparison.OrdinalIgnoreCase))
                        newLines.Add($"unbind \"{key}\"");
                    else
                        newLines.Add(FormatBind(key, newValue));
                    handledBinds.Add(key);
                }
                continue;
            }

            var cvarMatch = CvarKeyRegex.Match(line);
            if (cvarMatch.Success)
            {
                string key = cvarMatch.Groups[1].Value;

                if (allSettings.TryGetValue(key, out string newValue))
                {
                    if (!isCrosshairEnabled && key.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(newValue))
                    {
                        newLines.Add(FormatCvar(key, newValue));
                        handledGenerals.Add(key);
                    }
                    continue;
                }
            }

            newLines.Add(rawLine); // Сохраняем чужие скрипты "как есть", чтобы не ломать их структуру
        }

        if (hasUnbindAll && !unbindallAdded)
        {
            newLines.Insert(0, "unbindall");
        }

        bool addedHeader = false;

        foreach (var kvp in allSettings)
        {
            if (!isCrosshairEnabled && kvp.Key.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase)) continue;

            if (!string.IsNullOrWhiteSpace(kvp.Value) && !handledGenerals.Contains(kvp.Key))
            {
                if (!addedHeader) { newLines.Add("\n// --- НОВЫЕ ЗАПИСИ (CONMAKER) ---"); addedHeader = true; }
                newLines.Add(FormatCvar(kvp.Key, kvp.Value));
            }
        }

        foreach (var kvp in currentBinds)
        {
            if (!handledBinds.Contains(kvp.Key))
            {
                if (!addedHeader) { newLines.Add("\n// --- НОВЫЕ ЗАПИСИ (CONMAKER) ---"); addedHeader = true; }
                if (kvp.Value.Equals("UNBIND", StringComparison.OrdinalIgnoreCase))
                    newLines.Add($"unbind \"{kvp.Key.ToUpper()}\"");
                else
                    newLines.Add(FormatBind(kvp.Key.ToUpper(), kvp.Value));
            }
        }
    }

    private static void AppendAliases(List<string> newLines, PresetsViewModel presetsVm)
    {
        presetsVm.SaveChangesCommand.Execute(null);
        string rawAliases = presetsVm.RawText;

        if (!string.IsNullOrWhiteSpace(rawAliases))
        {
            newLines.Add("\n// --- АЛИАСЫ ---");
            string[] aliasLines = rawAliases.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string al in aliasLines)
            {
                if (al.Trim().StartsWith("alias", StringComparison.OrdinalIgnoreCase) || al.Trim().StartsWith("//"))
                    newLines.Add(al.Trim());
                else
                    newLines.Add("alias " + al.Trim());
            }
        }
    }
}
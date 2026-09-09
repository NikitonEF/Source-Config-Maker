using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace conmaker.Models
{
    /// <summary>
    /// Строит итоговый список строк .cfg файла на основе текущего ConfigState.
    /// Не трогает файловую систему — только формирует данные. Запись на диск (и бэкап)
    /// делает вызывающий код (ViewModel/сервис), т.к. это уже IO-забота, а не забота модели.
    /// </summary>
    public static class ConfigWriter
    {
        private static readonly Regex bindRegex = new Regex(@"^bind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex unbindRegex = new Regex(@"^unbind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex aliasRegex = new Regex(@"^alias\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<string> BuildLines(ConfigState state)
        {
            List<string> newLines = new List<string>();

            if (state.isNewConfig)
            {
                BuildFromScratch(state, newLines);
            }
            else
            {
                MergeWithExisting(state, newLines);
            }

            AppendAliases(state, newLines);

            return newLines;
        }

        private static void BuildFromScratch(ConfigState state, List<string> newLines)
        {
            newLines.Add("// Сделано при помощи Half-Life Config Maker\n");
            if (state.hasUnbindAll) newLines.Add("unbindall\n");

            newLines.Add("// --- ОСНОВНЫЕ НАСТРОЙКИ ---");
            foreach (var kvp in state.settingsValues)
            {
                if (!state.enableCrosshair && kvp.Key.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrWhiteSpace(kvp.Value)) newLines.Add($"{kvp.Key} \"{kvp.Value}\"");
            }

            newLines.Add("\n// --- БИНДЫ ---");
            foreach (var kvp in state.bindings)
            {
                if (kvp.Value == "UNBIND") newLines.Add($"unbind \"{kvp.Key.ToUpper()}\"");
                else newLines.Add($"bind \"{kvp.Key.ToUpper()}\" \"{kvp.Value}\"");
            }
        }

        private static void MergeWithExisting(ConfigState state, List<string> newLines)
        {
            HashSet<string> handledBinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> handledGenerals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool unbindallAdded = false;

            foreach (string rawLine in state.originalFileLines)
            {
                string line = rawLine.Trim();

                if (line.Equals("unbindall", StringComparison.OrdinalIgnoreCase))
                {
                    if (state.hasUnbindAll && !unbindallAdded)
                    {
                        newLines.Add("unbindall");
                        unbindallAdded = true;
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                {
                    // Если это шапка "сделано при помощи..." — сразу за ней втыкаем unbindall,
                    // как это делал старый код (чтобы не плодить unbindall где попало).
                    if (line.Contains("Half-Life Config Maker") && state.hasUnbindAll && !unbindallAdded)
                    {
                        newLines.Add(rawLine);
                        newLines.Add("unbindall");
                        unbindallAdded = true;
                        continue;
                    }
                    newLines.Add(rawLine);
                    continue;
                }

                // Старые alias-строки выкидываем — актуальные алиасы допишутся отдельным блоком в конце.
                if (aliasRegex.IsMatch(line)) continue;

                Match bindMatch = bindRegex.Match(line);
                if (bindMatch.Success)
                {
                    string key = bindMatch.Groups[1].Value.ToUpper();
                    if (state.bindings.ContainsKey(key))
                    {
                        newLines.Add(state.bindings[key] == "UNBIND"
                            ? $"unbind \"{key}\""
                            : $"bind \"{key}\" \"{state.bindings[key]}\"");
                        handledBinds.Add(key);
                    }
                    continue;
                }

                Match unbindMatch = unbindRegex.Match(line);
                if (unbindMatch.Success)
                {
                    string key = unbindMatch.Groups[1].Value.ToUpper();
                    if (state.bindings.ContainsKey(key))
                    {
                        newLines.Add(state.bindings[key] == "UNBIND"
                            ? $"unbind \"{key}\""
                            : $"bind \"{key}\" \"{state.bindings[key]}\"");
                        handledBinds.Add(key);
                    }
                    continue;
                }

                bool isGeneral = false;
                foreach (var cat in state.settingsCategories.Values)
                {
                    foreach (var gKey in cat)
                    {
                        if (!line.StartsWith(gKey, StringComparison.OrdinalIgnoreCase)) continue;

                        if (!state.enableCrosshair && gKey.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase))
                        {
                            // Прицел выключен — строку с cl_cross* просто выкидываем из файла.
                            isGeneral = true;
                            break;
                        }

                        if (state.settingsValues.TryGetValue(gKey, out var value) && !string.IsNullOrWhiteSpace(value))
                        {
                            newLines.Add($"{gKey} \"{value}\"");
                            handledGenerals.Add(gKey);
                        }
                        isGeneral = true;
                        break;
                    }
                    if (isGeneral) break;
                }
                if (isGeneral) continue;

                // Всё, что не bind/unbind/alias/cvar из наших категорий — переносим как есть.
                newLines.Add(rawLine);
            }

            if (state.hasUnbindAll && !unbindallAdded)
            {
                newLines.Insert(0, "unbindall");
            }

            // Новые записи, которых не было в исходном файле — дописываем отдельным блоком.
            bool addedHeader = false;
            foreach (var kvp in state.settingsValues)
            {
                if (!state.enableCrosshair && kvp.Key.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(kvp.Value) || handledGenerals.Contains(kvp.Key)) continue;

                if (!addedHeader) { newLines.Add("\n// --- НОВЫЕ ЗАПИСИ (CONMAKER) ---"); addedHeader = true; }
                newLines.Add($"{kvp.Key} \"{kvp.Value}\"");
            }

            foreach (var kvp in state.bindings)
            {
                if (handledBinds.Contains(kvp.Key)) continue;

                if (!addedHeader) { newLines.Add("\n// --- НОВЫЕ ЗАПИСИ (CONMAKER) ---"); addedHeader = true; }
                newLines.Add(kvp.Value == "UNBIND"
                    ? $"unbind \"{kvp.Key.ToUpper()}\""
                    : $"bind \"{kvp.Key.ToUpper()}\" \"{kvp.Value}\"");
            }
        }

        private static void AppendAliases(ConfigState state, List<string> newLines)
        {
            if (state.Aliases.Count == 0) return;

            newLines.Add("\n// --- АЛИАСЫ ---");
            foreach (string al in state.Aliases)
            {
                string trimmed = al.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                newLines.Add(trimmed.StartsWith("alias", StringComparison.OrdinalIgnoreCase)
                    ? trimmed
                    : "alias " + trimmed);
            }
        }
    }
}
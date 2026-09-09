using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace conmaker.Models
{
    /// <summary>
    /// Один пункт чеклиста: обязательная команда, забинжена ли она (после разворачивания
    /// алиасов) и на каких клавишах (сырые имена клавиш, без локализации/форматирования).
    /// </summary>
    public class ChecklistItem
    {
        public string Command { get; }
        public bool IsBound => Keys.Count > 0;
        public List<string> Keys { get; }

        public ChecklistItem(string command, List<string> keys)
        {
            Command = command;
            Keys = keys;
        }
    }

    /// <summary>
    /// Строит чеклист "обязательных" команд (движение, стрельба, оружие) и на каких клавишах
    /// они реально оказываются после разворачивания алиасов. Порт логики BtnChecklist_Click,
    /// без UI: без Form, без MessageBox, без локализации текста "НЕ НАЗНАЧЕНО".
    /// </summary>
    public static class ChecklistBuilder
    {
        public static readonly string[] RequiredCommands =
        {
            "+forward", "+back", "+moveleft", "+moveright", "+jump", "+duck",
            "+attack", "+attack2", "+reload", "+use",
            "weapon_crowbar", "weapon_9mmhandgun", "weapon_357", "weapon_9mmAR",
            "weapon_shotgun", "weapon_crossbow", "weapon_rpg", "weapon_gauss",
            "weapon_egon", "weapon_hornetgun", "weapon_satchel", "weapon_tripmine",
            "weapon_handgrenade", "weapon_snark"
        };

        private static readonly Regex aliasRegex = new Regex(@"^alias\s+""?([^""\s]+)""?\s+""?([^""]*)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<ChecklistItem> Build(ConfigState state)
        {
            Dictionary<string, List<string>> cmdToKeys = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> currentAliases = ParseAliases(state.Aliases);

            void AddToKeys(string rawVal, string keyName)
            {
                var parts = rawVal.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> expandedParts = new List<string>();

                foreach (var p in parts)
                {
                    string cleanP = p.Trim();
                    if (currentAliases.TryGetValue(cleanP, out var expansion))
                        expandedParts.AddRange(expansion.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    expandedParts.Add(cleanP);
                }

                foreach (var p in expandedParts)
                {
                    string clean = p.Trim();
                    if (!cmdToKeys.TryGetValue(clean, out var keys))
                        cmdToKeys[clean] = keys = new List<string>();
                    if (!keys.Contains(keyName)) keys.Add(keyName);
                }
            }

            // Дефолтные биндинги учитываем только там, где юзер их не переопределил
            // и только если не стоит unbindall (тогда дефолты движка не действуют).
            if (!state.hasUnbindAll)
            {
                foreach (var kvp in state.defaultBindings)
                {
                    bool overridden = state.bindings.ContainsKey(kvp.Key) && !string.IsNullOrWhiteSpace(state.bindings[kvp.Key]);
                    if (!overridden)
                        AddToKeys(kvp.Value, kvp.Key);
                }
            }

            foreach (var kvp in state.bindings)
            {
                if (kvp.Value != "UNBIND" && !string.IsNullOrWhiteSpace(kvp.Value))
                    AddToKeys(kvp.Value, kvp.Key);
            }

            List<ChecklistItem> result = new List<ChecklistItem>();
            foreach (string cmd in RequiredCommands)
            {
                List<string> keys = cmdToKeys.TryGetValue(cmd, out var found) ? found : new List<string>();
                result.Add(new ChecklistItem(cmd, keys));
            }
            return result;
        }

        private static Dictionary<string, string> ParseAliases(List<string> aliasLines)
        {
            Dictionary<string, string> currentAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in aliasLines)
            {
                Match m = aliasRegex.Match(l.Trim());
                if (m.Success) currentAliases[m.Groups[1].Value] = m.Groups[2].Value;
            }
            return currentAliases;
        }
    }
}
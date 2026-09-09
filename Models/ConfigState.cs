using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace conmaker.Models
{
    public class ConfigState
    {
        public event Action? ConfigLoaded;
        public event Action? ConfigReset;

        public Dictionary<string, string> defaultBindings { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"TAB", "+showscores"}, {"ENTER", "+attack"}, {"ESC", "cancelselect"}, {"ESCAPE", "escape"}, {"SPACE", "+jump"},
            {"'", "+moveup"}, {"+", "sizeup"}, {",", "+moveleft"}, {"-", "sizedown"}, {".", "+moveright"},
            {"/", "+movedown"}, {"1", "slot1"}, {"2", "slot2"}, {"3", "slot3"},
            {"4", "slot4"}, {"5", "slot5"}, {";", "+mlook"}, {"=", "sizeup"},
            {"[", "invprev"}, {"]", "invnext"}, {"`", "toggleconsole"}, {"~", "toggleconsole"},
            {"A", "+moveleft"}, {"C", "+movedown"}, {"D", "+moveright"}, {"E", "+use"},
            {"F", "impulse 100"}, {"K", "+voicerecord"}, {"Q", "lastinv"}, {"R", "+reload"}, {"S", "+back"}, {"T", "impulse 201"},
            {"U", "messagemode2"}, {"V", "+moveup"}, {"W", "+forward"}, {"Y", "messagemode"},
            {"UPARROW", "+forward"}, {"DOWNARROW", "+back"},
            {"LEFTARROW", "+left"}, {"RIGHTARROW", "+right"}, {"ALT", "+strafe"}, {"CTRL", "+duck"},
            {"SHIFT", "+speed"}, {"F5", "snapshot"}, {"F6", "save quick"}, {"F7", "load quick"},
            {"F10", "quit prompt"}, {"INS", "+klook"}, {"PGDN", "+lookdown"}, {"PGUP", "+lookup"},
            {"END", "centerview"}, {"MWHEELDOWN", "invnext"}, {"MWHEELUP", "invprev"}, {"MOUSE1", "+attack"}, {"MOUSE2", "+attack2"},
            {"PAUSE", "pause"}
        };

        public Dictionary<string, string> bindings { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, string[]> settingsCategories { get; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "ОСНОВНОЕ", new string[] { "name", "default_fov", "fps_max", "fps_override", "cl_bob", "cl_hidecorpses", "m_rawinput", "m_filter", "zoom_sensitivity_ratio", "cl_autojump", "cl_autorecord" } },
            { "СЕТЬ", new string[] { "rate", "cl_updaterate", "cl_cmdrate", "ex_interp", "cl_dlmax", "cl_lc", "cl_lw", "cl_cmdbackup", "cl_timeout", "cl_resend", "cl_latency" } },
            { "ЗВУК", new string[] { "volume", "hisound", "bgmvolume", "MP3Volume", "suitvolume", "voice_enable", "voice_scale", "ambient_level", "room_off", "s_a3d", "s_eax" } },
            { "ВИДЕО", new string[] { "gamma", "brightness", "r_drawviewmodel", "gl_vsync", "cl_forceenemymodels", "cl_forceteammatemodels", "hud_fastswitch", "net_graph" } },
            { "ПРИЦЕЛ", new string[] { "cl_cross", "cl_cross_size", "cl_cross_color", "cl_cross_thickness", "cl_cross_gap", "cl_cross_dot_size", "cl_cross_alpha" } },
        };
        public Dictionary<string, string> settingsValues { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public List<string> popularCommands { get; } = new List<string>
        {
            "weapon_crowbar", "weapon_9mmhandgun", "weapon_357", "weapon_shotgun",
            "weapon_crossbow", "weapon_rpg", "weapon_gauss", "weapon_egon",
            "weapon_snark", "weapon_tripmine", "weapon_satchel", "weapon_hornetgun",
            "weapon_9mmAR", "+attack", "+attack2", "+jump", "+duck", "+forward", "+back",
            "+moveleft", "+moveright", "+use", "+reload", "drop", "invnext", "invprev",
            "say", "say_team", "say_close", "play_close", "stopsound", "agstart", "agpause",
            "spectate", "retry", "customtimer", "+showscores", "-showscores", "loadauthid", "unloadauthid",
            "cancelselect", "escape", "+moveup", "sizeup", "sizedown", "+movedown", "+mlook", "toggleconsole",
            "+voicerecord", "messagemode2", "messagemode", "+left", "+right", "snapshot", "+strafe",
            "save quick", "load quick", "+klook", "+lookdown", "+lookup", "centerview", "pause", "exec",
            "slot1", "slot2", "slot3", "slot4", "slot5", "impulse 100", "impulse 201", "lastinv", "quit prompt"
        };

        public string currentFilePath { get; set; } = "";
        public bool isNewConfig { get; set; } = false;
        public bool hasUnbindAll { get; set; } = false;
        public bool enableCrosshair { get; set; } = false;
        public List<string> originalFileLines { get; set; } = new();
        public List<string> Aliases { get; } = new();

        private static readonly Regex bindRegex = new Regex(@"^bind\s+""?([^""\s]+)""?\s+""?([^""]*)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex unbindRegex = new Regex(@"^unbind\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex aliasRegex = new Regex(@"^alias\s+""?([^""\s]+)""?\s+""?([^""]*)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Дефолты, которые накатываются и на новый конфиг, и как "заглушка" при пустом парсинге старого.
        public void SetDefaultSettings()
        {
            foreach (var cat in settingsCategories.Values)
                foreach (var key in cat) settingsValues[key] = "";

            settingsValues["rate"] = "250000";
            settingsValues["ex_interp"] = "0.01";

            settingsValues["cl_cross_size"] = "5";
            settingsValues["cl_cross_color"] = "0 255 0";
            settingsValues["cl_cross_thickness"] = "2";
            settingsValues["cl_cross_gap"] = "3";
        }

        // Аналог BtnNew_Click, но без MessageBox — подтверждение "потерять несохранённые изменения?"
        // теперь дело ConfirmDialogViewModel, а не модели.
        public void NewConfig()
        {
            bindings.Clear();
            Aliases.Clear();
            originalFileLines.Clear();
            SetDefaultSettings();

            hasUnbindAll = false;
            enableCrosshair = false;
            currentFilePath = "";
            isNewConfig = true;

            ConfigReset?.Invoke();
        }
        public event Action? BindingsChanged;
        public void SetUnbindAll(bool value)
        {
            if (hasUnbindAll == value) return;
            hasUnbindAll = value;
            BindingsChanged?.Invoke();
        }

        public void SetCrosshairEnabled(bool value)
        {
            if (enableCrosshair == value) return;
            enableCrosshair = value;
            BindingsChanged?.Invoke();
        }

        public void LoadFromFile(string path)
        {
            bindings.Clear();
            Aliases.Clear();
            originalFileLines.Clear();
            hasUnbindAll = false;
            enableCrosshair = false;
            isNewConfig = false;

            SetDefaultSettings();

            if (!File.Exists(path)) return;

            currentFilePath = path;
            originalFileLines = new List<string>(File.ReadAllLines(path));

            foreach (string rawLine in originalFileLines)
            {
                string line = rawLine.Trim();

                if (line.Equals("unbindall", StringComparison.OrdinalIgnoreCase) || line.StartsWith("unbindall;", StringComparison.OrdinalIgnoreCase))
                {
                    bindings.Clear();
                    hasUnbindAll = true;
                    continue;
                }

                int commentIdx = line.IndexOf("//");
                if (commentIdx >= 0) line = line.Substring(0, commentIdx).Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                Match bindMatch = bindRegex.Match(line);
                if (bindMatch.Success)
                {
                    bindings[bindMatch.Groups[1].Value.ToUpper()] = bindMatch.Groups[2].Value;
                    continue;
                }

                Match unbindMatch = unbindRegex.Match(line);
                if (unbindMatch.Success)
                {
                    bindings[unbindMatch.Groups[1].Value.ToUpper()] = "UNBIND";
                    continue;
                }

                Match aliasMatch = aliasRegex.Match(line);
                if (aliasMatch.Success)
                {
                    Aliases.Add(rawLine.Trim());
                    continue;
                }

                foreach (var cat in settingsCategories.Values)
                {
                    foreach (var sKey in cat)
                    {
                        if (line.StartsWith(sKey, StringComparison.OrdinalIgnoreCase))
                        {
                            string value = line.Substring(sKey.Length).Trim(' ', '\t', '"');
                            settingsValues[sKey] = value;
                            if (sKey.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase))
                                enableCrosshair = true;
                            break;
                        }
                    }
                }
            }

            // Было внутри цикла — баг. Теперь стреляет один раз, после полной загрузки.
            ConfigLoaded?.Invoke();
        }
    }
}
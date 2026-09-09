using System;
using System.Collections.Generic;

namespace conmaker.Models
{
    /// <summary>
    /// Языко-независимый идентификатор вкладки настроек. Текст на экране (RU/EN/...)
    /// подставляется отдельно, слоем локализации — сюда никакие строки для показа не лезут.
    /// </summary>
    public enum SettingsCategory
    {
        Main,
        Network,
        Audio,
        Video,
        Crosshair
    }

    /// <summary>
    /// Статичные данные о раскладке клавиатуры/мыши: какие клавиши есть, как они расположены
    /// в UI, их отображаемые "ярлыки" (эти конкретные подписи вроде "UP"/"PG DN"/"LMB" — это не
    /// перевод языка интерфейса, а условные обозначения клавиш, поэтому они не завязаны на локаль).
    /// Раскладки заданы как логические ряды/столбцы без пиксельных координат — View сам решает,
    /// как их разместить (Grid/UniformGrid), в отличие от старого DrawInterface() с ручным scaleX/scaleY.
    /// </summary>
    public static class KeyboardLayoutData
    {
        public static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            {"UPARROW", "UP"}, {"DOWNARROW", "DOWN"}, {"LEFTARROW", "LEFT"}, {"RIGHTARROW", "RIGHT"},
            {"INS", "INS"}, {"DEL", "DEL"}, {"HOME", "HOME"}, {"END", "END"}, {"PGUP", "PG UP"}, {"PGDN", "PG DN"},
            {"KP_SLASH", "/"}, {"*", "*"}, {"KP_MINUS", "-"}, {"KP_PLUS", "+"}, {"KP_ENTER", "ENTER"},
            {"KP_DEL", ".\nDEL"}, {"KP_INS", "0\nINS"}, {"KP_END", "1\nEND"},
            {"KP_DOWNARROW", "2\nDOWN"}, {"KP_PGDN", "3\nPG DN"}, {"KP_LEFTARROW", "4\nLEFT"},
            {"KP_5", "5"}, {"KP_RIGHTARROW", "6\nRIGHT"}, {"KP_HOME", "7\nHOME"},
            {"KP_UPARROW", "8\nUP"}, {"KP_PGUP", "9\nPG UP"}, {"NUMLOCK", "NUM"}
        };

        // Юникод-символы кастомного шрифта иконок оружия (тот самый hlfont, грузится в UI-слое).
        public static readonly Dictionary<string, string> IconMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"weapon_handgrenade", "\uf000"}, {"weapon_9mmAR", "\uf001"}, {"weapon_rpg", "\uf002"},
            {"weapon_smg1", "\uf003"}, {"weapon_shotgun", "\uf004"}, {"weapon_357", "\uf005"},
            {"weapon_snark", "\uf006"}, {"weapon_crossbow", "\uf007"}, {"weapon_crowbar", "\uf008"},
            {"weapon_gauss", "\uf009"}, {"weapon_egon", "\uf009"}
        };

        // Основной блок клавиатуры, по рядам сверху вниз.
        public static readonly string[][] MainKeys =
        {
            new[] { "ESC", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" },
            new[] { "~", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "BACKSPACE" },
            new[] { "TAB", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" },
            new[] { "CAPSLOCK", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "ENTER" },
            new[] { "SHIFT", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "SHIFT" },
            new[] { "CTRL", "WIN", "ALT", "SPACE", "ALT", "FN", "MENU", "CTRL" }
        };

        // Блок Insert/Home/PgUp + стрелки. "SKIP" — пустая ячейка в сетке (там ничего нет).
        public static readonly string[][] NavKeys =
        {
            new[] { "INS", "HOME", "PGUP" },
            new[] { "DEL", "END", "PGDN" },
            new[] { "SKIP", "SKIP", "SKIP" },
            new[] { "SKIP", "UPARROW", "SKIP" },
            new[] { "LEFTARROW", "DOWNARROW", "RIGHTARROW" }
        };

        // Цифровой блок (NumPad).
        public static readonly string[][] NumKeys =
        {
            new[] { "NUMLOCK", "KP_SLASH", "*", "KP_MINUS" },
            new[] { "KP_HOME", "KP_UPARROW", "KP_PGUP", "KP_PLUS" },
            new[] { "KP_LEFTARROW", "KP_5", "KP_RIGHTARROW", "SKIP" },
            new[] { "KP_END", "KP_DOWNARROW", "KP_PGDN", "KP_ENTER" },
            new[] { "KP_INS", "SKIP", "KP_DEL", "SKIP" }
        };

        // Клавиши мыши: сырой ключ -> короткий ярлык. Порядок/раскладка (где именно рисовать
        // ЛКМ/ПКМ/колесо) — забота View, здесь только список того, что вообще существует.
        public static readonly (string Key, string Label)[] MouseButtons =
        {
            ("MOUSE1", "LMB"),
            ("MOUSE2", "RMB"),
            ("MOUSE3", "MMB"),
            ("MWHEELUP", "MWU"),
            ("MWHEELDOWN", "MWD"),
            ("MOUSE4", "M4"),
            ("MOUSE5", "M5")
        };

        public static string GetDisplayName(string rawKey)
            => DisplayNames.TryGetValue(rawKey, out var name) ? name : rawKey;
    }
}
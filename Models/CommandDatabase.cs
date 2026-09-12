using System;
using System.Collections.Generic;

namespace SourceConfigMaker.Models;

// МОДЕЛЬ (Model): Отвечает исключительно за хранение сырых данных и бизнес-правил.
public class CommandDatabase
{
    // Словарь дефолтных биндов. 
    // StringComparer.OrdinalIgnoreCase заставляет словарь не обращать внимание на регистр букв (TAB == tab).
    // Это экономит память и ускоряет поиск.
    public Dictionary<string, string> DefaultBindings { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        {"TAB", "+showscores"}, {"ENTER", "+attack"}, {"ESC", "cancelselect"}, {"SPACE", "+jump"},
        {"'", "+moveup"}, {"+", "sizeup"}, {",", "+moveleft"}, {"-", "sizedown"}, {".", "+moveright"},
        {"/", "+movedown"}, {"1", "slot1"}, {"2", "slot2"}, {"3", "slot3"},
        {"4", "slot4"}, {"5", "slot5"}, {";", "+mlook"}, {"=", "sizeup"},
        {"[", "invprev"}, {"]", "invnext"}, {"`", "toggleconsole"}, {"~", "toggleconsole"},
        {"A", "+moveleft"}, {"C", "+movedown"}, {"D", "+moveright"}, {"E", "+use"},
        {"F", "impulse 100"}, {"K", "+voicerecord"}, {"Q", "lastinv"}, {"R", "+reload"}, {"S", "+back"}, {"T", "impulse 201"},
        {"U", "messagemode2"}, {"V", "+moveup"}, {"W", "+forward"}, {"Y", "messagemode"},
        {"UP", "+forward"}, {"DOWN", "+back"}, {"LEFT", "+left"}, {"RIGHT", "+right"},
        {"ALT", "+strafe"}, {"CTRL", "+duck"}, {"SHIFT", "+speed"},
        {"F5", "snapshot"}, {"F6", "save quick"}, {"F7", "load quick"}, {"F10", "quit prompt"},
        {"INS", "+klook"}, {"PGDN", "+lookdown"}, {"PGUP", "+lookup"}, {"END", "centerview"},
        {"PAUSE", "pause"}
    };

    // Список команд для автодополнения. 
    // В будущем мы сможем загружать этот список из внешнего текстового файла, не трогая остальной код.
    public List<string> PopularCommands { get; } = new()
    {
        "weapon_crowbar", "weapon_9mmhandgun", "weapon_357", "weapon_shotgun",
        "weapon_crossbow", "weapon_rpg", "weapon_gauss", "weapon_egon",
        "weapon_snark", "weapon_tripmine", "weapon_satchel", "weapon_hornetgun",
        "weapon_9mmAR", "+attack", "+attack2", "+jump", "+duck", "+forward", "+back",
        "+moveleft", "+moveright", "+use", "+reload", "drop", "invnext", "invprev",
        "say", "say_team", "unbind", "unbindall"
    };
}
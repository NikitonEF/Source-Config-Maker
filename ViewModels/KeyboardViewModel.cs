using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SourceConfigMaker.Models;

namespace SourceConfigMaker.ViewModels;

public partial class KeyboardViewModel : ObservableObject
{
    private readonly Action<string> _openEditorAction;
    private readonly CommandDatabase _database;

    public Dictionary<string, KeyViewModel> Keys { get; } = new();

    private readonly Dictionary<string, string> _engineToSafeMap = new(StringComparer.OrdinalIgnoreCase);

    public KeyboardViewModel(Action<string> openEditorAction, CommandDatabase database)
    {
        _openEditorAction = openEditorAction;
        _database = database;

        for (int i = 1; i <= 12; i++) AddKey($"F{i}", $"F{i}", $"F{i}");
        AddKey("Esc", "ESCAPE", "ESC");

        for (int i = 0; i <= 9; i++) AddKey(i.ToString(), i.ToString(), i.ToString());

        string[] chars = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C", "V", "B", "N", "M" };
        foreach (var c in chars) AddKey(c, c, c);

        AddKey("Tilde", "~", "~");
        AddKey("Minus", "-", "-");
        AddKey("Plus", "=", "=");
        AddKey("Backspace", "BACKSPACE", "BACKSPACE");
        AddKey("Tab", "TAB", "TAB");
        AddKey("LBracket", "[", "[");
        AddKey("RBracket", "]", "]");
        AddKey("Backslash", "\\", "\\");
        AddKey("CapsLock", "CAPSLOCK", "CAPS");
        AddKey("Semicolon", ";", ";");
        AddKey("Quote", "'", "'");
        AddKey("Enter", "ENTER", "ENTER");
        AddKey("Shift", "SHIFT", "SHIFT");
        AddKey("Comma", ",", ",");
        AddKey("Dot", ".", ".");
        AddKey("Slash", "/", "/");
        AddKey("Ctrl", "CTRL", "CTRL");
        AddKey("Win", "WIN", "WIN");
        AddKey("Alt", "ALT", "ALT");
        AddKey("Space", "SPACE", "SPACE");
        AddKey("Fn", "FN", "FN");
        AddKey("Menu", "MENU", "MENU");
        AddKey("Cntr", "CNTR", "CNTR");

        AddKey("INS", "INS", "INS");
        AddKey("DEL", "DEL", "DEL");
        AddKey("HOME", "HOME", "HOME");
        AddKey("END", "END", "END");
        AddKey("PGUP", "PGUP", "PG UP");
        AddKey("PGDN", "PGDN", "PG DN");
        AddKey("UP", "UPARROW", "UP");
        AddKey("DOWN", "DOWNARROW", "DOWN");
        AddKey("LEFT", "LEFTARROW", "LEFT");
        AddKey("RIGHT", "RIGHTARROW", "RIGHT");

        AddKey("NumLock", "NUMLOCK", "NUM");
        AddKey("NumDiv", "KP_SLASH", "/");
        AddKey("NumMult", "*", "*");
        AddKey("NumMinus", "KP_MINUS", "-");
        AddKey("NumPlus", "KP_PLUS", "+");
        AddKey("NumEnter", "KP_ENTER", "ENTER");
        AddKey("NumDot", "KP_DEL", ".");
        AddKey("Num0", "KP_INS", "0");
        AddKey("Num1", "KP_END", "1");
        AddKey("Num2", "KP_DOWNARROW", "2");
        AddKey("Num3", "KP_PGDN", "3");
        AddKey("Num4", "KP_LEFTARROW", "4");
        AddKey("Num5", "KP_5", "5");
        AddKey("Num6", "KP_RIGHTARROW", "6");
        AddKey("Num7", "KP_HOME", "7");
        AddKey("Num8", "KP_UPARROW", "8");
        AddKey("Num9", "KP_PGUP", "9");

        AddKey("MOUSE1", "MOUSE1", "LMB");
        AddKey("MOUSE2", "MOUSE2", "RMB");
        AddKey("MOUSE3", "MOUSE3", "MMB");
        AddKey("MWHEELUP", "MWHEELUP", "MWU");
        AddKey("MWHEELDOWN", "MWHEELDOWN", "MWD");
        AddKey("MOUSE4", "MOUSE4", "M4");
        AddKey("MOUSE5", "MOUSE5", "M5");
    }

    private void AddKey(string safeName, string engineName, string dispName)
    {
        _engineToSafeMap[engineName] = safeName;
        string defCmd = _database.DefaultBindings.TryGetValue(engineName.ToUpper(), out var cmd) ? cmd : "";
        Keys[safeName] = new KeyViewModel(engineName, dispName, defCmd);
    }

    [RelayCommand]
    private void EditBind(string keyName) => _openEditorAction?.Invoke(keyName);

    public void ApplyParsedBinds(Dictionary<string, string> parsedBinds)
    {
        ClearBinds();
        foreach (var kvp in parsedBinds)
        {
            if (_engineToSafeMap.TryGetValue(kvp.Key, out var safeName))
            {
                if (Keys.TryGetValue(safeName, out var keyVm))
                {
                    keyVm.UserBind = kvp.Value;
                }
            }
        }
    }

    public void ClearBinds()
    {
        foreach (var keyVm in Keys.Values) keyVm.UserBind = string.Empty;
    }

    public void SetUnbindAllState(bool isActive)
    {
        foreach (var keyVm in Keys.Values)
        {
            keyVm.IsUnbindAllActive = isActive;
        }
    }
    public void SetTheme(bool isDark)
    {
        foreach (var keyVm in Keys.Values)
        {
            keyVm.IsDarkMode = isDark;
        }
    }
}
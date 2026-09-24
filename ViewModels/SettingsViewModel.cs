using Avalonia.Controls; // Нужен для TryFindResource
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SourceConfigMaker.Models;
using SourceConfigMaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;

namespace SourceConfigMaker.ViewModels;

// 3. Полностью обновленный класс CvarItem
public partial class CvarItem : ObservableObject
{
    public string Name { get; }
    public string DefaultValue { get; }
    public string ToolTipKey { get; }
    public float Min { get; }
    public float Max { get; }

    // Свойства для управления видимостью элементов в XAML
    public bool IsTextControl => _controlType == CvarControlType.Text;
    public bool IsBoolControl => _controlType == CvarControlType.Boolean;
    public bool IsSliderControl => _controlType == CvarControlType.Slider;

    private readonly CvarControlType _controlType;

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                // Уведомляем UI, что связанные значения (тумблер/слайдер) тоже изменились
                OnPropertyChanged(nameof(BoolValue));
                OnPropertyChanged(nameof(SliderValue));
            }
        }
    }

    // Прокси-свойство для тумблера (1 = включено, 0 = выключено)
    public bool BoolValue
    {
        get => Value == "1" || Value.Equals("true", StringComparison.OrdinalIgnoreCase);
        set => Value = value ? "1" : "0";
    }

    // Прокси-свойство для ползунка
    // Прокси-свойство для ползунка
    public double SliderValue
    {
        get
        {
            // Принудительно парсим строку с точкой, игнорируя региональные настройки Windows
            if (double.TryParse(Value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return Min;
        }
        set
        {
            // Сохраняем в формат движка (с точкой), независимо от языка ОС
            Value = value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
    // Добавляем свойство для хранения готового перевода
    [ObservableProperty]
    private string _toolTipText = string.Empty;

    // Вызываем это при смене языка
    public void UpdateLanguage()
    {
        if (Avalonia.Application.Current != null &&
            Avalonia.Application.Current.TryFindResource(ToolTipKey, out var value))
        {
            ToolTipText = value?.ToString() ?? string.Empty;
        }
    }

    // Новый конструктор принимает CvarDefinition
    public CvarItem(CvarDefinition def)
    {
        Name = def.Name;
        DefaultValue = def.DefaultValue;
        ToolTipKey = def.ToolTipKey;
        Min = def.Min;
        Max = def.Max;
        _controlType = def.Type;
        Value = def.DefaultValue;
        UpdateLanguage();
    }

    [RelayCommand]
    public void Reset()
    {
        Value = DefaultValue;
    }
}

public partial class CommandChipItem : ObservableObject
{
    public string CommandText { get; }

    [ObservableProperty]
    private bool _isAssigned;

    public CommandChipItem(string commandText)
    {
        CommandText = commandText;
    }
}

// Теперь категория работает через ресурсные ключи
public partial class CommandCategory : ObservableObject
{
    public string ResourceKey { get; }

    [ObservableProperty]
    private string _name = string.Empty;

    public ObservableCollection<CommandChipItem> Commands { get; } = new();

    public CommandCategory(string resourceKey)
    {
        ResourceKey = resourceKey;
        UpdateName();
    }

    public void UpdateName()
    {
        if (Avalonia.Application.Current != null &&
            Avalonia.Application.Current.TryFindResource(ResourceKey, out var value))
        {
            Name = value?.ToString() ?? ResourceKey;
        }
    }
}

public partial class CrosshairPreviewViewModel : ObservableObject
{
    private const double CanvasCenter = 100;

    [ObservableProperty] private double _topBarX;
    [ObservableProperty] private double _topBarY;
    [ObservableProperty] private double _topBarWidth;
    [ObservableProperty] private double _topBarHeight;

    [ObservableProperty] private double _bottomBarX;
    [ObservableProperty] private double _bottomBarY;
    [ObservableProperty] private double _bottomBarWidth;
    [ObservableProperty] private double _bottomBarHeight;

    [ObservableProperty] private double _leftBarX;
    [ObservableProperty] private double _leftBarY;
    [ObservableProperty] private double _leftBarWidth;
    [ObservableProperty] private double _leftBarHeight;

    [ObservableProperty] private double _rightBarX;
    [ObservableProperty] private double _rightBarY;
    [ObservableProperty] private double _rightBarWidth;
    [ObservableProperty] private double _rightBarHeight;

    [ObservableProperty] private double _dotX;
    [ObservableProperty] private double _dotY;
    [ObservableProperty] private double _dotSize;
    [ObservableProperty] private bool _dotVisible;

    [ObservableProperty] private IBrush _crossBrush = Brushes.Lime;

    public void Recalculate(IReadOnlyDictionary<string, string> values, bool isEnabled)
    {
        if (!isEnabled)
        {
            CrossBrush = Brushes.Transparent;
            return;
        }

        int size = ParseInt(values, "cl_cross_size", 5);
        int gap = ParseInt(values, "cl_cross_gap", 3);
        int thick = ParseInt(values, "cl_cross_thickness", 2);
        int dot = ParseInt(values, "cl_cross_dot_size", 0);
        int alpha = Math.Clamp(ParseInt(values, "cl_cross_alpha", 255), 0, 255);

        var (r, g, b) = ParseColor(values.TryGetValue("cl_cross_color", out var c) ? c : "0 255 0");
        CrossBrush = new SolidColorBrush(Color.FromArgb((byte)alpha, r, g, b));

        const double canvasSize = 200;
        const double cx = CanvasCenter, cy = CanvasCenter;

        double maxArm = (canvasSize / 2) - 2;
        double clampedGap = Math.Clamp(gap, 0, maxArm);
        double clampedSize = Math.Clamp(size, 0, maxArm - clampedGap);
        double clampedThick = Math.Clamp(thick, 1, canvasSize / 4);
        double clampedDot = Math.Clamp(dot, 0, canvasSize / 3);

        double Clamp01(double v) => Math.Clamp(v, 0, canvasSize);

        TopBarWidth = clampedThick; TopBarHeight = clampedSize;
        TopBarX = Math.Round(Clamp01(cx - clampedThick / 2.0));
        TopBarY = Math.Round(Clamp01(cy - clampedGap - clampedSize));

        BottomBarWidth = clampedThick; BottomBarHeight = clampedSize;
        BottomBarX = Math.Round(Clamp01(cx - clampedThick / 2.0));
        BottomBarY = Math.Round(Clamp01(cy + clampedGap));

        LeftBarWidth = clampedSize; LeftBarHeight = clampedThick;
        LeftBarX = Math.Round(Clamp01(cx - clampedGap - clampedSize));
        LeftBarY = Math.Round(Clamp01(cy - clampedThick / 2.0));

        RightBarWidth = clampedSize; RightBarHeight = clampedThick;
        RightBarX = Math.Round(Clamp01(cx + clampedGap));
        RightBarY = Math.Round(Clamp01(cy - clampedThick / 2.0));

        DotVisible = clampedDot > 0;
        DotSize = clampedDot;
        DotX = Math.Round(Clamp01(cx - clampedDot / 2.0));
        DotY = Math.Round(Clamp01(cy - clampedDot / 2.0));
    }

    private static int ParseInt(IReadOnlyDictionary<string, string> dict, string key, int fallback)
        => dict.TryGetValue(key, out var s) && int.TryParse(s, out var v) ? v : fallback;

    private static (byte, byte, byte) ParseColor(string colorStr)
    {
        var parts = (colorStr ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int r = 0, g = 255, b = 0;
        if (parts.Length == 3)
        {
            int.TryParse(parts[0], out r);
            int.TryParse(parts[1], out g);
            int.TryParse(parts[2], out b);
        }
        return ((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }
}

public partial class SettingsViewModel : ViewModelBase
{
    private readonly CvarDatabase _db = new();

    private static readonly Dictionary<string, string> CrosshairDefaults = new()
    {
        ["cl_cross_size"] = "5",
        ["cl_cross_color"] = "0 255 0",
        ["cl_cross_thickness"] = "2",
        ["cl_cross_gap"] = "3",
        ["cl_cross_dot_size"] = "0",
        ["cl_cross_alpha"] = "255",
    };

    public PresetsViewModel PresetsVm { get; } = new();

    public ObservableCollection<CvarItem> MainSettings { get; } = new();
    public ObservableCollection<CvarItem> NetSettings { get; } = new();
    public ObservableCollection<CvarItem> SoundSettings { get; } = new();
    public ObservableCollection<CvarItem> VideoSettings { get; } = new();
    public ObservableCollection<CvarItem> CrosshairSettings { get; } = new();

    public CrosshairPreviewViewModel CrosshairPreview { get; } = new();

    [ObservableProperty]
    private bool _isCrosshairEnabled;

    public ObservableCollection<CommandCategory> ChecklistCategories { get; } = new();

    public SettingsViewModel()
    {
        LoadCategory("ОСНОВНОЕ", MainSettings);
        LoadCategory("СЕТЬ", NetSettings);
        LoadCategory("ЗВУК", SoundSettings);
        LoadCategory("ВИДЕО", VideoSettings);
        LoadCategory("ПРИЦЕЛ", CrosshairSettings);

        ApplyCrosshairDefaults();
        RecalculateCrosshairPreview();
        SubscribeCrosshairPreview();
        InitializeChecklist();
    }

    public void UpdateLanguage()
    {
        foreach (var category in ChecklistCategories)
        {
            category.UpdateName();
        }

        // НОВОЕ: Обновляем язык для всех подсказок настроек
        var allSettings = MainSettings.Concat(NetSettings)
                                      .Concat(SoundSettings)
                                      .Concat(VideoSettings)
                                      .Concat(CrosshairSettings);
        foreach (var setting in allSettings)
        {
            setting.UpdateLanguage();
        }
    }

    partial void OnIsCrosshairEnabledChanged(bool value)
    {
        RecalculateCrosshairPreview();
    }

    private void ApplyCrosshairDefaults()
    {
        foreach (var item in CrosshairSettings)
        {
            if (string.IsNullOrWhiteSpace(item.Value) && CrosshairDefaults.TryGetValue(item.Name, out var def))
                item.Value = def;
        }
    }

    private void SubscribeCrosshairPreview()
    {
        foreach (var item in CrosshairSettings)
            item.PropertyChanged += (_, _) => RecalculateCrosshairPreview();
    }

    private void RecalculateCrosshairPreview()
    {
        var values = CrosshairSettings.ToDictionary(c => c.Name, c => c.Value);
        CrosshairPreview.Recalculate(values, IsCrosshairEnabled);
    }

    private void InitializeChecklist()
    {
        var movement = new CommandCategory("Lang_Cat_Movement");
        string[] moveCmds = { "+forward", "+back", "+moveleft", "+moveright", "+jump", "+duck", "+speed", "+strafe", "+mlook", "+klook", "+lookup", "+lookdown" };
        foreach (var cmd in moveCmds) movement.Commands.Add(new CommandChipItem(cmd));

        var weapons = new CommandCategory("Lang_Cat_Weapons");
        string[] wpnCmds = {
            "slot1", "slot2", "slot3", "slot4", "slot5", "invnext", "invprev", "lastinv", "drop",
            "weapon_crowbar", "weapon_9mmhandgun", "weapon_357", "weapon_9mmAR", "weapon_shotgun",
            "weapon_crossbow", "weapon_rpg", "weapon_gauss", "weapon_egon", "weapon_hornetgun",
            "weapon_snark", "weapon_satchel", "weapon_tripmine", "weapon_handgrenade"
        };
        foreach (var cmd in wpnCmds) weapons.Commands.Add(new CommandChipItem(cmd));

        var combat = new CommandCategory("Lang_Cat_Combat");
        string[] combatCmds = { "+attack", "+attack2", "+reload", "+use", "impulse 100", "impulse 201" };
        foreach (var cmd in combatCmds) combat.Commands.Add(new CommandChipItem(cmd));

        var uiComm = new CommandCategory("Lang_Cat_Interface");
        string[] uiCmds = { "+showscores", "toggleconsole", "cancelselect", "pause", "snapshot", "save quick", "load quick", "quit prompt" };
        foreach (var cmd in uiCmds) uiComm.Commands.Add(new CommandChipItem(cmd));

        var voice = new CommandCategory("Lang_Cat_Voice");
        string[] voiceCmds = { "messagemode", "messagemode2", "+voicerecord" };
        foreach (var cmd in voiceCmds) voice.Commands.Add(new CommandChipItem(cmd));

        ChecklistCategories.Add(movement);
        ChecklistCategories.Add(weapons);
        ChecklistCategories.Add(combat);
        ChecklistCategories.Add(uiComm);
        ChecklistCategories.Add(voice);
    }

    private void LoadCategory(string categoryName, ObservableCollection<CvarItem> collection)
    {
        // Теперь база данных отдает готовые, строго типизированные объекты
        if (_db.Categories.TryGetValue(categoryName, out var cvars))
        {
            foreach (var def in cvars)
            {
                collection.Add(new CvarItem(def));
            }
        }
    }

    public void UpdateChecklist(IEnumerable<string> assignedCommands)
    {
        var assignedSet = new HashSet<string>(assignedCommands);

        foreach (var category in ChecklistCategories)
        {
            foreach (var cmd in category.Commands)
            {
                cmd.IsAssigned = assignedSet.Contains(cmd.CommandText);
            }
        }
    }

    public void ApplyParsedCvars(Dictionary<string, string> parsedCvars)
    {
        var allCvars = MainSettings.Concat(NetSettings)
                                   .Concat(SoundSettings)
                                   .Concat(VideoSettings)
                                   .Concat(CrosshairSettings);

        foreach (var cvar in allCvars)
        {
            if (parsedCvars.TryGetValue(cvar.Name, out string? value))
            {
                cvar.Value = value;
            }
            else
            {
                cvar.Value = string.Empty;
            }
        }
    }

    public void ClearCvars()
    {
        var allCvars = MainSettings.Concat(NetSettings)
                                   .Concat(SoundSettings)
                                   .Concat(VideoSettings)
                                   .Concat(CrosshairSettings);
        foreach (var cvar in allCvars)
        {
            cvar.Value = string.Empty;
        }
    }
    [RelayCommand]
    private void ResetAllCvars()
    {
        // Собираем все коллекции кваров, которые у тебя есть во вкладках
        var allSettings = MainSettings
            .Concat(NetSettings)
            .Concat(SoundSettings)
            .Concat(VideoSettings)
            .Concat(CrosshairSettings);

        foreach (var cvar in allSettings)
        {
            cvar.Reset(); // Вызываем метод сброса из CvarItem
        }
    }
}
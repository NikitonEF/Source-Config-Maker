using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SourceConfigMaker.Models;

namespace SourceConfigMaker.ViewModels;

public partial class PresetsViewModel : ViewModelBase
{
    private readonly PresetsDatabase _db = new();
    private bool _isSyncingToRaw;
    private bool _isSyncingFromRaw;
    private DateTime _lastRawUpdate = DateTime.MinValue;

    public ObservableCollection<PresetItem> CurrentPresets { get; } = new();

    [ObservableProperty]
    private string _currentGame = "Half-Life";

    [ObservableProperty]
    private PresetItem? _selectedPreset;

    [ObservableProperty]
    private string _rawText = string.Empty;

    public PresetsViewModel()
    {
        CurrentPresets.CollectionChanged += OnPresetsCollectionChanged;
        LoadPresetsForCurrentGame();
        RebuildRawFromPresets();
    }

    partial void OnSelectedPresetChanged(PresetItem? value)
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    private void LoadPresetsForCurrentGame()
    {
        CurrentPresets.Clear();
        if (_db.GamePresets.TryGetValue(CurrentGame, out var presets))
        {
            foreach (var preset in presets)
                CurrentPresets.Add(preset);
        }
    }

    private void OnPresetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (PresetItem item in e.NewItems)
                item.PropertyChanged += OnPresetItemChanged;

        if (e.OldItems != null)
            foreach (PresetItem item in e.OldItems)
                item.PropertyChanged -= OnPresetItemChanged;

        RebuildRawFromPresets();
    }

    private void OnPresetItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Отслеживаем новый флаг IsPinned, чтобы сразу записывать его на жесткий диск
        if (e.PropertyName is nameof(PresetItem.Name) or nameof(PresetItem.Content) or nameof(PresetItem.IsIncluded) or nameof(PresetItem.IsPinned))
        {
            RebuildRawFromPresets();
            SyncAndSave();
        }
    }

    private void RebuildRawFromPresets()
    {
        if (_isSyncingFromRaw) return;

        _isSyncingToRaw = true;
        var sb = new StringBuilder();
        foreach (var preset in CurrentPresets)
        {
            if (!preset.IsIncluded) continue;

            sb.Append("// ").Append(preset.Name).Append('\n');
            sb.Append(preset.Content.TrimEnd('\r', '\n')).Append('\n');
            sb.Append('\n');
        }
        RawText = sb.Length > 0 ? sb.ToString().TrimEnd('\n') + "\n" : string.Empty;
        _lastRawUpdate = DateTime.UtcNow;
        _isSyncingToRaw = false;
    }

    partial void OnRawTextChanged(string value)
    {
        if (_isSyncingToRaw || (DateTime.UtcNow - _lastRawUpdate).TotalMilliseconds < 200) return;

        _isSyncingFromRaw = true;
        try { ApplyRawText(value); }
        finally { _isSyncingFromRaw = false; }
    }

    private void ApplyRawText(string raw)
    {
        var blocks = ParseBlocks(raw);
        var namesInRaw = new HashSet<string>();

        foreach (var (name, content) in blocks)
        {
            namesInRaw.Add(name);
            var existing = CurrentPresets.FirstOrDefault(p => p.Name == name);

            if (existing != null)
            {
                if (existing.IsBuiltIn) continue;
                if (existing.Content != content) existing.Content = content;
                if (!existing.IsIncluded) existing.IsIncluded = true;
            }
            else
            {
                CurrentPresets.Add(new PresetItem { Name = name, Content = content, IsBuiltIn = false, IsIncluded = true });
            }
        }

        foreach (var preset in CurrentPresets)
        {
            if (!preset.IsBuiltIn && !namesInRaw.Contains(preset.Name))
            {
                if (preset.IsIncluded) preset.IsIncluded = false;
            }
        }

        SyncAndSave();
    }

    private static List<(string Name, string Content)> ParseBlocks(string raw)
    {
        var result = new List<(string, string)>();
        var lines = raw.Replace("\r\n", "\n").Split('\n');
        string? currentName = null;
        var currentContent = new StringBuilder();

        void Flush()
        {
            if (currentName != null)
                result.Add((currentName, currentContent.ToString().Trim('\n', '\r')));
            currentContent.Clear();
        }

        foreach (var line in lines)
        {
            if (line.StartsWith("// "))
            {
                Flush();
                currentName = line.Substring(3).Trim();
            }
            else if (currentName != null)
            {
                currentContent.Append(line).Append('\n');
            }
        }
        Flush();
        return result;
    }

    public void ImportAliases(List<string> rawAliases)
    {
        if (rawAliases == null || rawAliases.Count == 0) return;

        var sb = new StringBuilder();
        foreach (var alias in rawAliases) sb.AppendLine(alias);
        string content = sb.ToString().TrimEnd();

        var existing = CurrentPresets.FirstOrDefault(p => p.Name == "Импортировано из конфига");
        if (existing != null)
        {
            existing.Content = content;
            existing.IsIncluded = true;
        }
        else
        {
            CurrentPresets.Add(new PresetItem { Name = "Импортировано из конфига", Content = content, IsBuiltIn = false, IsIncluded = true });
        }
        SyncAndSave();
    }

    // ИСПРАВЛЕНИЕ: Теперь мы бережем запиненные алиасы
    public void ClearAliases()
    {
        for (int i = CurrentPresets.Count - 1; i >= 0; i--)
        {
            var preset = CurrentPresets[i];

            if (!preset.IsBuiltIn)
            {
                if (preset.IsPinned)
                {
                    // Если запинен — оставляем в библиотеке, но выключаем из нового конфига
                    preset.IsIncluded = false;
                }
                else
                {
                    // Если не запинен — стираем насовсем
                    CurrentPresets.RemoveAt(i);
                }
            }
        }
        SyncAndSave();
    }

    [RelayCommand]
    private void AddCustomPreset()
    {
        var newPreset = new PresetItem { Name = "Новый пресет", Content = "// Введите ваш скрипт...", IsBuiltIn = false, IsIncluded = true, IsPinned = false };
        CurrentPresets.Add(newPreset);
        SelectedPreset = newPreset;
        SyncAndSave();
    }

    [RelayCommand]
    private void DeleteSelectedPreset()
    {
        if (SelectedPreset == null || SelectedPreset.IsBuiltIn) return;
        CurrentPresets.Remove(SelectedPreset);
        SelectedPreset = null;
        SyncAndSave();
    }

    public bool CanSave => SelectedPreset != null && !SelectedPreset.IsBuiltIn;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveChanges()
    {
        RebuildRawFromPresets();
        SyncAndSave();
    }

    private void SyncAndSave()
    {
        _db.GamePresets[CurrentGame] = CurrentPresets.ToList();
        _db.Save();
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SourceConfigMaker.Models;

public partial class PresetItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private bool _isBuiltIn;

    [ObservableProperty]
    private bool _isIncluded = true;

    // НОВОЕ: Флаг закрепления алиаса в библиотеке
    [ObservableProperty]
    private bool _isPinned;
}

public class PresetsDatabase
{
    public Dictionary<string, List<PresetItem>> GamePresets { get; set; } = new();
    private readonly string _savePath;

    public PresetsDatabase()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "SourceConfigMaker");
        Directory.CreateDirectory(folder);
        _savePath = Path.Combine(folder, "presets.json");

        Load();
    }

    public void Load()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                GamePresets = JsonSerializer.Deserialize<Dictionary<string, List<PresetItem>>>(json) ?? new();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] Ошибка чтения JSON: {ex.Message}");
                GamePresets = new();
            }
        }

        if (!GamePresets.ContainsKey("Half-Life"))
        {
            GamePresets["Half-Life"] = new List<PresetItem>
            {
                new PresetItem { Name = "Bunnyhop (Auto-Jump)", Content = "alias +bhop \"alias _special @bhop;@bhop\"\nalias -bhop \"alias _special\"\nalias @bhop \"special;wait;+jump;wait;-jump\"", IsBuiltIn = true, IsIncluded = true },
                new PresetItem { Name = "Double Duck", Content = "alias +doubleduck \"-duck;wait;+duck;wait;-duck;wait;+duck\"\nalias -doubleduck \"-duck\"", IsBuiltIn = true, IsIncluded = true },
                new PresetItem { Name = "Duckroll", Content = "alias +duckroll \"alias _zpecial duckroll;duckroll\"\nalias -duckroll \"alias _zpecial\"\nalias duckroll \"+duck;wait;-duck;wait;zpecial\"", IsBuiltIn = true, IsIncluded = true }
            };
            Save();
        }
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(GamePresets, options);

            using (var fs = new FileStream(_savePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs))
            {
                writer.Write(json);
            }

            Debug.WriteLine($"[OK] Успешно сохранено в: {_savePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CRITICAL] ПРОВАЛ СОХРАНЕНИЯ: {ex.Message}");
        }
    }
}
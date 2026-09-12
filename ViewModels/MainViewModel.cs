using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SourceConfigMaker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceConfigMaker.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly CommandDatabase _database = new();

    public KeyboardViewModel KeyboardVm { get; }
    public SettingsViewModel SettingsVm { get; } = new();
    private string _currentFilePath = string.Empty;

    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private string _editingKeyName = string.Empty;

    [ObservableProperty]
    private string _editingCommandText = string.Empty;

    [ObservableProperty]
    private string _defaultBindHint = string.Empty;

    [ObservableProperty]
    private bool _hasDefaultBind;

    [ObservableProperty]
    private bool _isNewConfigDialogOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CrosshairToggleColor))]
    private bool _isCrosshairEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnbindAllColor))]
    private bool _hasUnbindAll;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeIcon))]
    [NotifyPropertyChangedFor(nameof(ThemeHatColor))]
    [NotifyPropertyChangedFor(nameof(AppBackground))]
    [NotifyPropertyChangedFor(nameof(TitleBarBackground))]
    [NotifyPropertyChangedFor(nameof(TopBarButtonBackground))]
    [NotifyPropertyChangedFor(nameof(TopBarForeground))]
    [NotifyPropertyChangedFor(nameof(KeyboardBackground))]
    [NotifyPropertyChangedFor(nameof(SettingsBackground))]
    [NotifyPropertyChangedFor(nameof(MouseBackground))]
    [NotifyPropertyChangedFor(nameof(CrosshairToggleColor))]
    [NotifyPropertyChangedFor(nameof(UnbindAllColor))]
    private bool _isDarkMode = true;

    public IBrush CrosshairToggleColor => IsCrosshairEnabled ? Brush.Parse("#B43C3C") : TopBarButtonBackground;
    public IBrush UnbindAllColor => HasUnbindAll ? Brush.Parse("#C85050") : TopBarButtonBackground;

    public string ThemeIcon => IsDarkMode ? "☀️" : "🌙";
    public IBrush ThemeHatColor => IsDarkMode ? Brushes.White : Brush.Parse("#1A1A1F");

    public IBrush AppBackground => IsDarkMode ? Brush.Parse("#1E1E23") : Brush.Parse("#F0F0F5");
    public IBrush TitleBarBackground => IsDarkMode ? Brush.Parse("#202025") : Brush.Parse("#D2D2D7");
    public IBrush TopBarButtonBackground => IsDarkMode ? Brush.Parse("#2D2D32") : Brush.Parse("#DCDCE1");
    public IBrush TopBarForeground => IsDarkMode ? Brushes.White : Brushes.Black;

    public IBrush KeyboardBackground => IsDarkMode ? Brush.Parse("#1E1E23") : Brush.Parse("#E6E6EB");
    public IBrush SettingsBackground => IsDarkMode ? Brush.Parse("#1A1A1F") : Brush.Parse("#DCDCE1");
    public IBrush MouseBackground => IsDarkMode ? Brush.Parse("#232328") : Brush.Parse("#D2D2D7");

    public List<string> PopularCommands => _database.PopularCommands;

    public MainViewModel()
    {
        KeyboardVm = new KeyboardViewModel(OpenEditor, _database);
        RefreshBindsChecklist();
    }

    [RelayCommand]
    private async Task OpenConfigAsync(Window parentWindow)
    {
        if (parentWindow == null) return;

        var files = await parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите .cfg файл",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Config Files") { Patterns = new[] { "*.cfg" } } }
        });

        if (files.Count >= 1)
        {
            string filePath = files[0].Path.LocalPath;

            _currentFilePath = filePath; // <--- ВОТ ЭТА СТРОКА ДОБАВЛЕНА

            ParsedConfig configData = ConfigParser.Parse(filePath);

            HasUnbindAll = configData.HasUnbindAll;
            KeyboardVm.SetUnbindAllState(HasUnbindAll);
            IsCrosshairEnabled = configData.Cvars.Keys.Any(k => k.StartsWith("cl_cross", StringComparison.OrdinalIgnoreCase));
            SettingsVm.IsCrosshairEnabled = IsCrosshairEnabled;

            DistributeParsedData(configData);
        }
    }

    [RelayCommand]
    private void PromptNewConfig() => IsNewConfigDialogOpen = true;

    [RelayCommand]
    private void CancelNewConfig() => IsNewConfigDialogOpen = false;

    [RelayCommand]
    private void ConfirmNewConfig()
    {
        KeyboardVm.ClearBinds();
        SettingsVm.ClearCvars();
        SettingsVm.PresetsVm.ClearAliases();

        HasUnbindAll = false;
        KeyboardVm.SetUnbindAllState(false);
        IsCrosshairEnabled = false;
        SettingsVm.IsCrosshairEnabled = false;
        RefreshBindsChecklist();

        _currentFilePath = string.Empty; // <--- ВОТ ЭТА СТРОКА ДОБАВЛЕНА

        IsNewConfigDialogOpen = false;
    }

    [RelayCommand]
    private async Task SaveConfigAsync(Window parentWindow)
    {
        if (parentWindow == null) return;

        string targetPath = _currentFilePath;

        // Если файл еще не был открыт, вызываем диалог "Сохранить как..."
        if (string.IsNullOrEmpty(targetPath))
        {
            var file = await parentWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить конфиг как...",
                DefaultExtension = "cfg",
                SuggestedFileName = "config.cfg",
                FileTypeChoices = new[] { new FilePickerFileType("Config Files") { Patterns = new[] { "*.cfg" } } }
            });

            if (file == null) return;
            targetPath = file.Path.LocalPath;
        }

        // Бэкап старого файла (если он существует)
        if (System.IO.File.Exists(targetPath))
        {
            string backupPath = targetPath + ".backup";
            System.IO.File.Copy(targetPath, backupPath, overwrite: true);
        }

        // Генерируем финальный текст конфига (используем await BuildAsync)
        string configContent = await ConfigBuilder.BuildAsync(
            KeyboardVm,
            SettingsVm,
            HasUnbindAll,
            IsCrosshairEnabled,
            string.IsNullOrEmpty(_currentFilePath) ? null : _currentFilePath);

        // Записываем на диск
        await System.IO.File.WriteAllTextAsync(targetPath, configContent);
        _currentFilePath = targetPath; // Запоминаем путь для будущих сохранений
    }

    [RelayCommand]
    private void ToggleCrosshair()
    {
        IsCrosshairEnabled = !IsCrosshairEnabled;
        SettingsVm.IsCrosshairEnabled = IsCrosshairEnabled;
    }

    [RelayCommand]
    private void ToggleUnbindAll()
    {
        HasUnbindAll = !HasUnbindAll;
        KeyboardVm.SetUnbindAllState(HasUnbindAll);
        RefreshBindsChecklist();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        KeyboardVm.SetTheme(IsDarkMode);

        var app = Avalonia.Application.Current;
        if (app != null)
        {
            app.RequestedThemeVariant = IsDarkMode ? Avalonia.Styling.ThemeVariant.Dark : Avalonia.Styling.ThemeVariant.Light;
        }
    }

    private void DistributeParsedData(ParsedConfig config)
    {
        KeyboardVm.ApplyParsedBinds(config.Binds);
        SettingsVm.ApplyParsedCvars(config.Cvars);
        SettingsVm.PresetsVm.ImportAliases(config.Aliases); // Передаем алиасы
        RefreshBindsChecklist();
    }

    private void OpenEditor(string keyName)
    {
        EditingKeyName = keyName;
        if (KeyboardVm.Keys.TryGetValue(keyName, out var keyVm))
        {
            EditingCommandText = keyVm.UserBind == "UNBIND" ? "" : keyVm.UserBind;
        }

        string cleanKey = keyName.ToUpper();
        if (_database.DefaultBindings.TryGetValue(cleanKey, out var defBind))
        {
            DefaultBindHint = $"(По умолчанию: {defBind})";
            HasDefaultBind = true;
        }
        else HasDefaultBind = false;

        IsEditorOpen = true;
    }

    [RelayCommand]
    private void ApplyDefaultBind()
    {
        string cleanKey = EditingKeyName.ToUpper();
        if (_database.DefaultBindings.TryGetValue(cleanKey, out var defBind)) EditingCommandText = defBind;
    }

    [RelayCommand]
    private void SaveBind(string? cmdParameter)
    {
        if (KeyboardVm.Keys.TryGetValue(EditingKeyName, out var keyVm))
        {
            if (cmdParameter == "UNBIND") keyVm.UserBind = "UNBIND";
            else
            {
                string newCmd = EditingCommandText?.Trim() ?? "";
                if (newCmd.Equals("unbind", StringComparison.OrdinalIgnoreCase)) keyVm.UserBind = "UNBIND";
                else keyVm.UserBind = newCmd;
            }
        }
        RefreshBindsChecklist();
        IsEditorOpen = false;
    }

    [RelayCommand]
    private void CancelEdit() => IsEditorOpen = false;

    public void RefreshBindsChecklist()
    {
        var assignedCommands = KeyboardVm.Keys.Values
            .Where(k => !string.IsNullOrEmpty(k.DisplayCommand))
            .Select(k => k.DisplayCommand);
        SettingsVm.UpdateChecklist(assignedCommands);
    }
}
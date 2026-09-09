using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using conmaker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace conmaker.ViewModels
{
    public partial class TopBarViewModel : ViewModelBase
    {
        private readonly ConfigState _sharedState;
        private readonly IStorageProvider _storage;

        // Диалоги, которые View подключит снаружи (как ShowBindEditorAsync в BindsPanelViewModel).
        public Func<string, string, Task<bool>>? ShowConfirmDialogAsync;
        public Func<ChecklistDialogViewModel, Task>? ShowChecklistAsync;

        // Оповещения об итоге операции — вместо MessageBox.Show из старого кода.
        // View подписывается и сама решает, как показать (toast/snackbar/диалог).
        public event Action<bool, string>? SaveCompleted; // (success, path or error)

        // Общие настройки приложения (тема/язык/иконки) — НЕ дублируются здесь как свои
        // ObservableProperty, а просто прокидываются: и TopBar, и другие VM смотрят на один объект.
        public AppUiState UiState { get; }

        public TopBarViewModel(ConfigState statePointer, IStorageProvider storagePointer, AppUiState uiState)
        {
            _sharedState = statePointer;
            _storage = storagePointer;
            UiState = uiState;

            _sharedState.ConfigLoaded += NotifyConfigDependentPropsChanged;
            _sharedState.ConfigReset += NotifyConfigDependentPropsChanged;
        }

        private void NotifyConfigDependentPropsChanged()
        {
            OnPropertyChanged(nameof(HasUnbindAll));
            OnPropertyChanged(nameof(EnableCrosshair));
        }

        // --- Настройки уровня конфига — зеркалим ConfigState через SetUnbindAll/SetCrosshairEnabled ---
        public bool HasUnbindAll
        {
            get => _sharedState.hasUnbindAll;
            set
            {
                if (_sharedState.hasUnbindAll == value) return;
                _sharedState.SetUnbindAll(value);
                OnPropertyChanged();
            }
        }

        public bool EnableCrosshair
        {
            get => _sharedState.enableCrosshair;
            set
            {
                if (_sharedState.enableCrosshair == value) return;
                _sharedState.SetCrosshairEnabled(value);
                OnPropertyChanged();
            }
        }

        // Пытается подхватить последний открытый конфиг (аналог блока в конце старого Form1()).
        // Вызывается из MainViewModel один раз при старте приложения.
        public void TryRestoreLastConfig()
        {
            string? lastPath = LastConfigPathStore.TryLoad();
            if (lastPath != null)
            {
                _sharedState.LoadFromFile(lastPath);
            }
        }

        [RelayCommand]
        public async Task OpenConfigAsync()
        {
            var files = await _storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите конфиг",
                AllowMultiple = false,
                FileTypeFilter = new[] { CfgFileType, FilePickerFileTypes.All }
            });

            if (files.Count == 0) return;

            string? path = files[0].TryGetLocalPath();
            if (path == null) return;

            _sharedState.LoadFromFile(path);
            LastConfigPathStore.Save(path);
        }

        [RelayCommand]
        private async Task NewConfigAsync()
        {
            if (ShowConfirmDialogAsync == null) return;

            bool userSaidYes = await ShowConfirmDialogAsync(
                "ВНИМАНИЕ",
                "Создать новый конфиг? Все несохраненные изменения будут потеряны.");

            if (!userSaidYes) return;

            // Вся логика сброса теперь живёт в одном месте — ConfigState.NewConfig(),
            // а не дублируется здесь руками (как было раньше).
            _sharedState.NewConfig();
            LastConfigPathStore.Clear();
        }

        [RelayCommand]
        private async Task SaveConfigAsync()
        {
            string? path = _sharedState.currentFilePath;

            if (string.IsNullOrEmpty(path))
            {
                var file = await _storage.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить конфиг",
                    DefaultExtension = "cfg",
                    FileTypeChoices = new[] { CfgFileType, FilePickerFileTypes.All }
                });

                if (file == null) return; // юзер нажал "Отмена"

                path = file.TryGetLocalPath();
                if (path == null) return;
            }

            try
            {
                // Бэкап — как в старом BtnSave_Click.
                if (File.Exists(path))
                    File.Copy(path, path + ".backup", true);

                var lines = ConfigWriter.BuildLines(_sharedState);
                File.WriteAllLines(path, lines);

                _sharedState.currentFilePath = path;
                _sharedState.originalFileLines = new List<string>(lines);
                _sharedState.isNewConfig = false;

                LastConfigPathStore.Save(path);

                SaveCompleted?.Invoke(true, path);
            }
            catch (Exception ex)
            {
                SaveCompleted?.Invoke(false, ex.Message);
            }
        }

        [RelayCommand]
        private async Task ShowChecklistDialogAsync()
        {
            if (ShowChecklistAsync == null) return;

            var vm = new ChecklistDialogViewModel(_sharedState);
            await ShowChecklistAsync(vm);
        }

        // --- Простые тумблеры (UiState — общий, HasUnbindAll/EnableCrosshair — свойства выше) ---

        [RelayCommand]
        private void ToggleTheme() => UiState.IsDarkMode = !UiState.IsDarkMode;

        [RelayCommand]
        private void ToggleLanguage() => UiState.IsEnglish = !UiState.IsEnglish;

        [RelayCommand]
        private void ToggleIcons() => UiState.ShowIcons = !UiState.ShowIcons;

        [RelayCommand]
        private void ToggleUnbindAll() => HasUnbindAll = !HasUnbindAll;

        [RelayCommand]
        private void ToggleCrosshair() => EnableCrosshair = !EnableCrosshair;

        private static readonly FilePickerFileType CfgFileType = new("Config files")
        {
            Patterns = new[] { "*.cfg" }
        };
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace conmaker.ViewModels;

public partial class BindEditorViewModel : ViewModelBase
{
    public string BindKey { get; }

    [ObservableProperty]
    private string _bindCommand;

    // Дефолтный бинд движка для этой клавиши (если есть) — для подсказки "(По умолчанию: ...)"
    public string? DefaultCommand { get; }
    public bool HasDefault => !string.IsNullOrEmpty(DefaultCommand);

    // Список для автодополнения: popularCommands + имена уже объявленных алиасов.
    public IReadOnlyList<string> Suggestions { get; }

    public event Action<bool>? CloseRequested;

    public BindEditorViewModel(string bindKey, string currentCommand, string? defaultCommand, IEnumerable<string> suggestions)
    {
        BindKey = bindKey;
        _bindCommand = currentCommand;
        DefaultCommand = defaultCommand;
        Suggestions = suggestions is IReadOnlyList<string> list ? list : new List<string>(suggestions);
    }

    [RelayCommand]
    private void Save() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    [RelayCommand(CanExecute = nameof(HasDefault))]
    private void UseDefault()
    {
        if (DefaultCommand != null) BindCommand = DefaultCommand;
    }

    // Как в старом коде: кнопка Unbind только подставляет служебный текст в поле,
    // юзер всё ещё жмёт Save, чтобы применить (можно передумать перед сохранением).
    [RelayCommand]
    private void Unbind()
    {
        BindCommand = "unbind";
    }
}
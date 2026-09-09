using CommunityToolkit.Mvvm.Input;
using conmaker.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace conmaker.ViewModels;

public partial class BindsPanelViewModel : ViewModelBase
{
    public Dictionary<string, KeyViewModel> Keys { get; } = new();

    private readonly ConfigState _sharedState;
    private readonly AppUiState _uiState;

    // View показывает диалог редактирования и возвращает true, если юзер нажал Save.
    public Func<BindEditorViewModel, Task<bool>>? ShowBindEditorAsync;

    private static readonly Regex AliasNameRegex = new(@"^alias\s+""?([^""\s]+)""?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public BindsPanelViewModel(ConfigState statePointer, AppUiState uiState)
    {
        _sharedState = statePointer;
        _uiState = uiState;

        _sharedState.ConfigLoaded += RefreshKeys;
        _sharedState.ConfigReset += RefreshKeys;
        _sharedState.BindingsChanged += RefreshKeys;

        BuildKeys();
    }

    // Собираем ВСЕ клавиши физической раскладки, а не только те, у которых есть дефолтный бинд.
    private void BuildKeys()
    {
        void AddLayout(IEnumerable<string[]> rows)
        {
            foreach (var row in rows)
                foreach (var key in row)
                    AddKeyIfMissing(key);
        }

        AddLayout(KeyboardLayoutData.MainKeys);
        AddLayout(KeyboardLayoutData.NavKeys);
        AddLayout(KeyboardLayoutData.NumKeys);

        foreach (var (key, _) in KeyboardLayoutData.MouseButtons)
            AddKeyIfMissing(key);
    }

    private void AddKeyIfMissing(string keyName)
    {
        // "SKIP" — пустая ячейка сетки, не настоящая клавиша.
        // SHIFT/CTRL/ALT встречаются в раскладке дважды (лево/право) — это ОДНА и та же
        // логическая клавиша движка, поэтому у неё должна быть ровно одна KeyViewModel
        // на обе кнопки в UI (как было и в старом коде).
        if (keyName == "SKIP" || Keys.ContainsKey(keyName)) return;

        Keys.Add(keyName, new KeyViewModel(keyName, ResolveCommand(keyName), _uiState));
    }

    // Аналог старого:
    // bindings.ContainsKey(key) ? bindings[key] : (hasUnbindAll ? "" : (defaultBindings.ContainsKey(key) ? defaultBindings[key] : ""))
    private string ResolveCommand(string keyName)
    {
        if (_sharedState.bindings.TryGetValue(keyName, out var custom))
            return custom; // может быть "UNBIND"

        if (!_sharedState.hasUnbindAll && _sharedState.defaultBindings.TryGetValue(keyName, out var def))
            return def;

        return "";
    }

    public void RefreshKeys()
    {
        foreach (var pair in Keys)
            pair.Value.CommandText = ResolveCommand(pair.Key);
    }

    [RelayCommand]
    private async Task EditBindAsync(string keyName)
    {
        if (ShowBindEditorAsync == null) return;

        string currentCmd = ResolveCommand(keyName);
        _sharedState.defaultBindings.TryGetValue(keyName, out var defaultCmd);

        var editorVm = new BindEditorViewModel(keyName, currentCmd, defaultCmd, BuildSuggestions());

        bool saved = await ShowBindEditorAsync(editorVm);
        if (!saved) return;

        ApplyBindResult(keyName, editorVm.BindCommand);
    }

    // Порт логики из старого EditBind(): пусто -> вернуться к дефолту, "unbind" -> явный UNBIND,
    // иначе -> обычная команда.
    private void ApplyBindResult(string keyName, string newCommand)
    {
        string trimmed = (newCommand ?? "").Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            _sharedState.bindings.Remove(keyName);
        else if (trimmed.Equals("unbind", StringComparison.OrdinalIgnoreCase))
            _sharedState.bindings[keyName] = "UNBIND";
        else
            _sharedState.bindings[keyName] = trimmed;

        if (Keys.TryGetValue(keyName, out var vm))
            vm.CommandText = ResolveCommand(keyName);
    }

    private List<string> BuildSuggestions()
    {
        var list = new List<string>(_sharedState.popularCommands);

        foreach (var alias in _sharedState.Aliases)
        {
            var m = AliasNameRegex.Match(alias.Trim());
            if (m.Success) list.Add(m.Groups[1].Value);
        }

        return list;
    }
}
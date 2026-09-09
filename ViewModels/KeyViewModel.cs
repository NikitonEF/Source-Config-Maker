using CommunityToolkit.Mvvm.ComponentModel;
using conmaker.Models;

namespace conmaker.ViewModels;

public partial class KeyViewModel : ViewModelBase
{
    public string KeyName { get; }
    public string DisplayName { get; }

    private readonly AppUiState? _uiState;

    [ObservableProperty]
    private string _commandText;

    public KeyViewModel(string keyName, string initialCommand = "", AppUiState? uiState = null)
    {
        KeyName = keyName;
        DisplayName = KeyboardLayoutData.GetDisplayName(keyName);
        _uiState = uiState;
        _commandText = initialCommand;

        if (_uiState != null)
            _uiState.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AppUiState.ShowIcons))
                    OnPropertyChanged(nameof(IconGlyph));
            };
    }

    public bool IsExplicitlyUnbound => CommandText == "UNBIND";

    // Если ShowIcons включен и в команде есть распознанное оружие — глиф иконки,
    // иначе null (View показывает обычный текст CommandText).
    public string? IconGlyph
    {
        get
        {
            if (_uiState == null || !_uiState.ShowIcons) return null;
            if (string.IsNullOrEmpty(CommandText)) return null;

            foreach (var part in CommandText.Split(';'))
            {
                string clean = part.Trim();
                if (KeyboardLayoutData.IconMap.TryGetValue(clean, out var glyph))
                    return glyph;
            }
            return null;
        }
    }

    partial void OnCommandTextChanged(string value)
    {
        OnPropertyChanged(nameof(IsExplicitlyUnbound));
        OnPropertyChanged(nameof(IconGlyph));
    }
}
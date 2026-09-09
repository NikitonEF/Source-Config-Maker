using CommunityToolkit.Mvvm.ComponentModel;

namespace conmaker.ViewModels;

/// <summary>
/// Настройки уровня приложения — НЕ содержимое .cfg файла (в отличие от ConfigState).
/// Тема, язык интерфейса, показ иконок оружия. Общий объект: любая VM/View может
/// подписаться на изменения напрямую, без похода через TopBarViewModel.
/// </summary>
public partial class AppUiState : ObservableObject
{
    [ObservableProperty]
    private bool _isDarkMode = true;

    [ObservableProperty]
    private bool _isEnglish = true;

    [ObservableProperty]
    private bool _showIcons = false;
}
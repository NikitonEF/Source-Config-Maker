using conmaker.Models;
using Avalonia.Platform.Storage;

namespace conmaker.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConfigState _sharedState;

    public AppUiState UiState { get; }
    public TopBarViewModel TopBarVm { get; }
    public BindsPanelViewModel BindsPanelVm { get; }

    public MainViewModel(IStorageProvider storage)
    {
        _sharedState = new ConfigState();
        _sharedState.SetDefaultSettings();

        UiState = new AppUiState();

        TopBarVm = new TopBarViewModel(_sharedState, storage, UiState);
        BindsPanelVm = new BindsPanelViewModel(_sharedState, UiState);

        TopBarVm.TryRestoreLastConfig();
    }
}
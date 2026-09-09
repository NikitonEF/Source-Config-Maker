using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using conmaker.ViewModels;
using conmaker.Views;

namespace conmaker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            desktop.MainWindow = window;

            var mainVm = new MainViewModel(window.StorageProvider);
            window.DataContext = mainVm;

            // --- Подтверждение (Да/Нет) ---
            mainVm.TopBarVm.ShowConfirmDialogAsync = async (title, message) =>
            {
                var confirmVm = new ConfirmDialogViewModel(title, message);
                var dialog = new ConfirmDialogView { DataContext = confirmVm };

                confirmVm.CloseRequested += result => dialog.Close(result);

                return await dialog.ShowDialog<bool>(window);
            };

            // --- Редактор бинда ---
            // BindsPanelViewModel сам собирает готовую BindEditorViewModel (с дефолтом и
            // автодополнением) и просит её показать — View просто открывает окно и ждёт результат.
            mainVm.BindsPanelVm.ShowBindEditorAsync = async editorVm =>
            {
                var dialog = new BindEditorView { DataContext = editorVm };
                return await dialog.ShowDialog<bool>(window);
            };

            // --- Чеклист команд ---
            mainVm.TopBarVm.ShowChecklistAsync = async checklistVm =>
            {
                var dialog = new ChecklistView { DataContext = checklistVm };
                await dialog.ShowDialog(window);
            };

            base.OnFrameworkInitializationCompleted();
        }
        else
        {
            base.OnFrameworkInitializationCompleted();
        }
    }
}
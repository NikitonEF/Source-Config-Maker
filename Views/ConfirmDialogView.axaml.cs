using Avalonia.Controls;
using conmaker.ViewModels;
using System;

namespace conmaker.Views;
public partial class ConfirmDialogView : Window
{
    public ConfirmDialogView()
    {
        InitializeComponent();
    }

    // Этот метод вызывается автоматически, когда мы кладем ViewModel в DataContext окна
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ConfirmDialogViewModel vm)
        {
            // Подписываемся на событие из ViewModel
            vm.CloseRequested += (bool result) =>
            {
                // Метод Close() встроен в само окно Avalonia.
                // Он закрывает окно и возвращает 'result' туда, откуда окно было вызвано.
                Close(result);
            };
        }
    }
}
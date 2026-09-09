using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using conmaker.ViewModels;
using System;

namespace conmaker.Views
{
    public partial class BindEditorView : Window
    {
        public BindEditorView()
        {
            InitializeComponent();
        }
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is BindEditorViewModel vm)
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
}
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace conmaker.ViewModels
{
    public partial class ConfirmDialogViewModel : ViewModelBase
    {
        // Данные, которые мы покажем на экране
        public string Title { get; }
        public string Message { get; }

        // Событие (крик в пустоту), которое услышит View, чтобы закрыться
        public event Action<bool>? CloseRequested;

        public ConfirmDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        [RelayCommand]
        private void Yes()
        {
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void No()
        {
            CloseRequested?.Invoke(false);
        }
    }
}

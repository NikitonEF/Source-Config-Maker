using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using SourceConfigMaker.ViewModels;
using System;
using System.Linq; // <-- Добавлено для работы с LINQ при поиске файла

namespace SourceConfigMaker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Подписываемся на глобальное событие бросания файла на окно
        AddHandler(DragDrop.DropEvent, Window_Drop);
    }

    // НОВЫЙ МЕТОД: Обработка Drag-and-Drop
    private void Window_Drop(object? sender, DragEventArgs e)
    {
        // Проверяем, что нам скинули именно файлы
        if (e.DataTransfer.TryGetFiles() is { } files)
        {
            // Ищем первый попавшийся файл с расширением .cfg
            var cfgFile = files.FirstOrDefault(f => f.Name.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase));

            if (cfgFile != null && this.DataContext is MainViewModel viewModel)
            {
                // Передаем путь напрямую в нашу ViewModel
                viewModel.LoadConfigFromFile(cfgFile.Path.LocalPath);
            }
        }
    }

    // Обработчик нажатия на нашу кастомную верхнюю панель
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Проверяем, что нажата именно левая кнопка мыши
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // По двойному клику разворачиваем окно на весь экран и обратно
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                // Говорим операционной системе начать перетаскивание окна
                BeginMoveDrag(e);
            }
        }
    }

    private void LanguageComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
        {
            string lang = item.Tag?.ToString() ?? "en";
            var uri = new Uri($"avares://SourceConfigMaker/Assets/Lang/{lang}.axaml");

            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(uri) { Source = uri });
            }

            // Обновляем текст категорий биндов на лету
            if (this.DataContext is MainViewModel mainVm)
            {
                mainVm.SettingsVm.UpdateLanguage();
            }
        }
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
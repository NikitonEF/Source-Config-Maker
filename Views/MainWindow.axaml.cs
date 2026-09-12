using Avalonia.Controls;
using Avalonia.Input;

namespace SourceConfigMaker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Обработчик нажатия на нашу кастомную верхнюю панель
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Проверяем, что нажата именно левая кнопка мыши
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Удобная фича: по двойному клику разворачиваем окно на весь экран и обратно
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
}
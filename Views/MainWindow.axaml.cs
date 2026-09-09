using Avalonia.Controls;
using Avalonia.Input;
namespace conmaker.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var titleBar = this.FindControl<Panel>("TitleBar");
        if (titleBar != null)
            titleBar.PointerPressed += TitleBar_PointerPressed;
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.BeginMoveDrag(e);
    }
}
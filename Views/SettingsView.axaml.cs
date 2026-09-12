using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using SourceConfigMaker.ViewModels;

namespace SourceConfigMaker.Views;

public partial class SettingsView : UserControl
{
    private Point _dragStartPoint;
    private CommandChipItem? _dragSourceChip;
    private IPointer? _capturedPointer;
    private bool _isDragging;
    private DragGhost? _currentGhost; // Локальный экземпляр

    public SettingsView()
    {
        InitializeComponent();

        AliasContentEditor.TextArea.TextView.LineTransformers.Add(new SourceSyntaxColorizer());
        RawEditor.TextArea.TextView.LineTransformers.Add(new SourceSyntaxColorizer());
    }

    private static void InsertSnippet(TextEditor editor, string snippet)
    {
        var offset = editor.CaretOffset;
        editor.Document.Insert(offset, snippet);
        editor.CaretOffset = offset + snippet.Length;
        editor.Focus();
    }

    private void AliasSnippet_Alias_Click(object? sender, RoutedEventArgs e) =>
        InsertSnippet(AliasContentEditor, "alias \"name\" \"command1;command2\"");

    private void AliasSnippet_Bind_Click(object? sender, RoutedEventArgs e) =>
        InsertSnippet(AliasContentEditor, "bind \"key\" \"command\"");

    private void AliasSnippet_Wait_Click(object? sender, RoutedEventArgs e) =>
        InsertSnippet(AliasContentEditor, "wait");

    private void RawSnippet_Alias_Click(object? sender, RoutedEventArgs e) =>
        InsertSnippet(RawEditor, "alias \"name\" \"command1;command2\"");

    private void CommandScroll_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X - e.Delta.Y * 50, scrollViewer.Offset.Y);
            e.Handled = true;
        }
    }

    private void CommandChip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is CommandChipItem chip)
        {
            _dragStartPoint = e.GetPosition(control);
            _dragSourceChip = chip;
            _isDragging = false;

            e.Pointer.Capture(control);
            _capturedPointer = e.Pointer;

            control.PointerMoved += CommandChip_PointerMoved;
            control.PointerReleased += CommandChip_PointerReleased;
        }
    }

    private void CommandChip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSourceChip == null || sender is not Control control) return;

        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        if (!_isDragging)
        {
            var diff = point.Position - _dragStartPoint;
            if (System.Math.Abs(diff.X) <= 3 && System.Math.Abs(diff.Y) <= 3)
                return;

            _isDragging = true;
            _currentGhost = new DragGhost(topLevel, _dragSourceChip.CommandText, e.GetPosition(topLevel));
            e.Handled = true;
            return;
        }

        _currentGhost?.Update(e.GetPosition(topLevel));
        e.Handled = true;
    }

    private void CommandChip_PointerReleased(object? sender, PointerEventArgs e)
    {
        if (sender is not Control control) return;

        control.PointerMoved -= CommandChip_PointerMoved;
        control.PointerReleased -= CommandChip_PointerReleased;

        // Корректно освобождаем ресурсы призрака
        _currentGhost?.Dispose();
        _currentGhost = null;

        if (_isDragging && _dragSourceChip != null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                var pointOnTopLevel = e.GetPosition(topLevel);
                var hit = topLevel.InputHitTest(pointOnTopLevel);
                var targetKey = FindKeyViewModel(hit);

                if (targetKey != null)
                {
                    targetKey.UserBind = _dragSourceChip.CommandText;

                    var window = this.FindAncestorOfType<Window>();
                    if (window?.DataContext is MainViewModel mainVm)
                    {
                        mainVm.RefreshBindsChecklist();
                    }
                }
            }
        }

        _capturedPointer?.Capture(null);
        _capturedPointer = null;
        _dragSourceChip = null;
        _isDragging = false;
    }

    private static KeyViewModel? FindKeyViewModel(IInputElement? element)
    {
        var current = element as Visual;
        while (current != null)
        {
            if (current is Control control && control.DataContext is KeyViewModel kvm)
                return kvm;
            current = current.GetVisualParent();
        }
        return null;
    }
}
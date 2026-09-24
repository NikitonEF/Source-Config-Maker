using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using SourceConfigMaker.ViewModels;

namespace SourceConfigMaker.Views;

// Наша статическая сессия, которая будет хранить перетаскиваемые данные 
// (спасет нас от всей боли с изменением API DataTransfer в Avalonia)
public static class DragSession
{
    public static string? Command { get; set; }
    public static KeyViewModel? Key { get; set; }
}

public partial class SettingsView : UserControl
{
    private Avalonia.Point _dragStartPoint;
    private CommandChipItem? _dragSourceChip;
    private PointerPressedEventArgs? _dragStartEvent; // Сохраняем ивент нажатия

    public SettingsView()
    {
        InitializeComponent();

        Focusable = true;

        AliasContentEditor.TextArea.TextView.LineTransformers.Add(new SourceSyntaxColorizer());
        RawEditor.TextArea.TextView.LineTransformers.Add(new SourceSyntaxColorizer());

        // НОВОЕ: Разрешаем всей панели настроек работать как "корзина" для биндов
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    // НОВОЕ: Если клавишу с биндом бросили на любую часть настроек — очищаем её
    private void OnDrop(object? sender, DragEventArgs e)
    {
        // Проверяем, что притащили именно клавишу с клавиатуры, а не что-то другое
        if (DragSession.Key != null)
        {
            // Стираем кастомный бинд (откат к дефолтной команде игры)
            DragSession.Key.UserBind = string.Empty;

            // Обновляем галочки в интерфейсе, чтобы чип снова стал активным
            var window = this.FindAncestorOfType<Window>();
            if (window?.DataContext is MainViewModel mainVm)
            {
                mainVm.RefreshBindsChecklist();
            }

            e.DragEffects = DragDropEffects.Move; // Говорим системе, что приняли элемент
            e.Handled = true;

            // Очищаем сессию
            DragSession.Key = null;
        }
    }

    private void InnerScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ScrollViewer innerScroll && e.Delta.Y != 0)
        {
            bool canScrollHorizontally = innerScroll.Extent.Width > innerScroll.Viewport.Width;
            if (canScrollHorizontally)
            {
                double newX = innerScroll.Offset.X - (e.Delta.Y * 50);
                innerScroll.Offset = new Avalonia.Vector(newX, innerScroll.Offset.Y);
                e.Handled = true;
            }
        }
    }

    private static void InsertSnippet(TextEditor editor, string snippet)
    {
        var offset = editor.CaretOffset;
        editor.Document.Insert(offset, snippet);
        editor.CaretOffset = offset + snippet.Length;
        editor.Focus();
    }

    private void AliasSnippet_Alias_Click(object? sender, RoutedEventArgs e) => InsertSnippet(AliasContentEditor, "alias \"name\" \"command1;command2\"");
    private void AliasSnippet_Bind_Click(object? sender, RoutedEventArgs e) => InsertSnippet(AliasContentEditor, "bind \"key\" \"command\"");
    private void AliasSnippet_Wait_Click(object? sender, RoutedEventArgs e) => InsertSnippet(AliasContentEditor, "wait");
    private void RawSnippet_Alias_Click(object? sender, RoutedEventArgs e) => InsertSnippet(RawEditor, "alias \"name\" \"command1;command2\"");

    private void CommandScroll_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X - e.Delta.Y * 50, scrollViewer.Offset.Y);
            e.Handled = true;
        }
    }

    // --- ЛОГИКА DRAG & DROP (Источник) ---

    private void CommandChip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is CommandChipItem chip)
        {
            var point = e.GetCurrentPoint(control);
            if (point.Properties.IsLeftButtonPressed)
            {
                _dragStartPoint = point.Position;
                _dragSourceChip = chip;
                _dragStartEvent = e; // Сохраняем событие для DoDragDropAsync

                control.PointerMoved += CommandChip_PointerMoved;
            }
        }
    }
    private async void CommandChip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSourceChip == null || sender is not Control control) return;

        var point = e.GetCurrentPoint(control);
        if (!point.Properties.IsLeftButtonPressed) return;

        var diff = point.Position - _dragStartPoint;

        if (System.Math.Abs(diff.X) > 3 || System.Math.Abs(diff.Y) > 3)
        {
            control.PointerMoved -= CommandChip_PointerMoved;

            var commandText = _dragSourceChip.CommandText;
            DragSession.Command = commandText;
            DragSession.Key = null;
            _dragSourceChip = null;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Создаем призрака
            var ghost = new DragGhost(topLevel, commandText, e.GetPosition(topLevel));

            // "Железобетонная" подписка на движение
            System.EventHandler<DragEventArgs> dragOverHandler = (_, args) => ghost.Update(args.GetPosition(topLevel));
            topLevel.AddHandler(DragDrop.DragOverEvent, dragOverHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

            var dragData = new DataTransfer();
            if (_dragStartEvent != null)
            {
                await DragDrop.DoDragDropAsync(_dragStartEvent, dragData, DragDropEffects.Copy);
            }

            // Убиваем призрака после броска
            topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
            ghost.Dispose();
        }
    }
    private void OnGlobalPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var visual = e.Source as Visual;

        // Проверяем, куда попал клик
        bool isTextInput = visual is TextBox ||
                           visual?.FindAncestorOfType<TextBox>() != null ||
                           visual?.FindAncestorOfType<AvaloniaEdit.TextEditor>() != null;

        // Если кликнули по пустоте, кнопкам, табам или ползункам — жестко отбираем фокус
        if (!isTextInput)
        {
            this.Focus();
        }
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.AddHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed, RoutingStrategies.Tunnel);
    }

    // НОВОЕ: Когда вкладка пропадает (или окно закрывается) — отписываемся (Senior практика)
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        topLevel?.RemoveHandler(InputElement.PointerPressedEvent, OnGlobalPointerPressed);
    }
}
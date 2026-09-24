using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SourceConfigMaker.ViewModels;

namespace SourceConfigMaker.Views;

public partial class KeyboardView : UserControl
{
    private Avalonia.Point _dragStartPoint;
    private KeyViewModel? _dragSourceKey;
    private PointerPressedEventArgs? _dragStartEvent; // Сохраняем ивент нажатия

    public KeyboardView()
    {
        InitializeComponent();

        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);

        // Подписываемся на нативное системное событие Drop
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (e.Source is not Control control || control.DataContext is not KeyViewModel keyVm) return;

        // ПКМ: мгновенный сброс клавиши к дефолту
        if (point.Properties.IsRightButtonPressed)
        {
            if (!string.IsNullOrWhiteSpace(keyVm.UserBind))
            {
                keyVm.UserBind = string.Empty;
                RefreshChecklist();
            }
            e.Handled = true;
            return;
        }

        // ЛКМ: подготавливаемся к переносу клавиши (свапу биндов)
        if (point.Properties.IsLeftButtonPressed && !string.IsNullOrWhiteSpace(keyVm.UserBind) && keyVm.UserBind != "UNBIND")
        {
            _dragStartPoint = point.Position;
            _dragSourceKey = keyVm;
            _dragStartEvent = e; // Сохраняем событие для DoDragDropAsync
        }
    }

    private async void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSourceKey == null) return;

        var point = e.GetCurrentPoint(this);
        if (!point.Properties.IsLeftButtonPressed)
        {
            _dragSourceKey = null;
            return;
        }

        var diff = point.Position - _dragStartPoint;
        if (System.Math.Abs(diff.X) > 3 || System.Math.Abs(diff.Y) > 3)
        {
            var sourceKey = _dragSourceKey;
            DragSession.Key = sourceKey;
            DragSession.Command = null;
            _dragSourceKey = null;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var ghost = new DragGhost(topLevel, sourceKey.UserBind, e.GetPosition(topLevel));

            System.EventHandler<DragEventArgs> dragOverHandler = (_, args) => ghost.Update(args.GetPosition(topLevel));
            topLevel.AddHandler(DragDrop.DragOverEvent, dragOverHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

            var dragData = new DataTransfer();
            if (_dragStartEvent != null)
            {
                await DragDrop.DoDragDropAsync(_dragStartEvent, dragData, DragDropEffects.Move);
                e.Handled = true;
            }

            topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
            ghost.Dispose();
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        // Ищем, на какую клавишу бросили элемент
        var targetKey = FindKeyViewModel(e.Source as IInputElement);

        // Сценарий 1: Притащили новую команду из панели настроек
        if (DragSession.Command != null && targetKey != null)
        {
            targetKey.UserBind = DragSession.Command;
            RefreshChecklist();
            e.Handled = true;
        }
        // Сценарий 2: Перетаскивают уже назначенную клавишу (Свап биндов)
        else if (DragSession.Key != null)
        {
            var sourceKey = DragSession.Key;

            if (targetKey != null && targetKey != sourceKey)
            {
                // Меняем бинды местами
                var temp = targetKey.UserBind;
                targetKey.UserBind = sourceKey.UserBind;
                sourceKey.UserBind = temp;
                RefreshChecklist();
            }
            else if (targetKey == null && IsOverBindsDropZone(e.Source as IInputElement))
            {
                // Бросили клавишу мимо клавиатуры — стираем кастомный бинд
                sourceKey.UserBind = string.Empty;
                RefreshChecklist();
            }
            e.Handled = true;
        }

        // Очищаем сессию после броска
        DragSession.Command = null;
        DragSession.Key = null;
    }

    private void RefreshChecklist()
    {
        var window = this.FindAncestorOfType<Window>();
        if (window?.DataContext is MainViewModel mainVm)
        {
            mainVm.RefreshBindsChecklist();
        }
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

    private static bool IsOverBindsDropZone(IInputElement? element)
    {
        var current = element as Visual;
        while (current != null)
        {
            if (current is Control control && control.Name == "BindsDropZone")
                return true;
            current = current.GetVisualParent();
        }
        return false;
    }
}
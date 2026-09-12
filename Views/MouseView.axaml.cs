using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SourceConfigMaker.ViewModels;

namespace SourceConfigMaker.Views;

public partial class MouseView : UserControl
{
    private Point _dragStartPoint;
    private KeyViewModel? _dragSourceKey;
    private IPointer? _capturedPointer;
    private bool _isDragging;
    private DragGhost? _currentGhost;

    public MouseView()
    {
        InitializeComponent();

        AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);

        if (point.Properties.IsLeftButtonPressed && e.Source is Control control && control.DataContext is KeyViewModel keyVm)
        {
            if (!string.IsNullOrWhiteSpace(keyVm.UserBind) && keyVm.UserBind != "UNBIND")
            {
                _dragStartPoint = point.Position;
                _dragSourceKey = keyVm;
                _isDragging = false;
            }
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragSourceKey == null) return;

        var point = e.GetCurrentPoint(this);

        if (!point.Properties.IsLeftButtonPressed)
        {
            if (!_isDragging)
                _dragSourceKey = null;
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        if (!_isDragging)
        {
            var diff = point.Position - _dragStartPoint;
            if (Math.Abs(diff.X) <= 3 && Math.Abs(diff.Y) <= 3)
                return;

            _isDragging = true;
            _capturedPointer = e.Pointer;
            e.Pointer.Capture(this);

            _currentGhost = new DragGhost(topLevel, _dragSourceKey.UserBind, e.GetPosition(topLevel));
            e.Handled = true;
            return;
        }

        _currentGhost?.Update(e.GetPosition(topLevel));
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        _currentGhost?.Dispose();
        _currentGhost = null;

        if (_isDragging && _dragSourceKey != null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                var pointOnTopLevel = e.GetPosition(topLevel);
                var hit = topLevel.InputHitTest(pointOnTopLevel);
                var targetKey = FindKeyViewModel(hit);

                if (targetKey != null && targetKey != _dragSourceKey)
                {
                    // Логика свапа, как и на клавиатуре
                    var temp = targetKey.UserBind;
                    targetKey.UserBind = _dragSourceKey.UserBind;
                    _dragSourceKey.UserBind = temp;

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
        _dragSourceKey = null;
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
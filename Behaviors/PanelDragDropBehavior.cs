using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Behaviors;

public class PanelDragDropBehavior : Behavior<Control>
{
    private bool _isPressed;
    private Point _pointerStartPosition;
    private PointerPressedEventArgs? _pressedEvent;
    private static readonly DataFormat<HmiPanelViewModel> PanelFormat =
        DataFormat.CreateInProcessFormat<HmiPanelViewModel>("hmi-panel");

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerMoved += OnPointerMoved;
            AssociatedObject.PointerReleased += OnPointerReleased;

            DragDrop.SetAllowDrop(AssociatedObject, true);
            AssociatedObject.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AssociatedObject.AddHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed -= OnPointerPressed;
            AssociatedObject.PointerMoved -= OnPointerMoved;
            AssociatedObject.PointerReleased -= OnPointerReleased;

            AssociatedObject.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
            AssociatedObject.RemoveHandler(DragDrop.DropEvent, OnDrop);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (AssociatedObject?.DataContext is HmiPanelViewModel)
        {
            _isPressed = true;
            _pressedEvent = eventArgs;
            _pointerStartPosition = eventArgs.GetPosition(AssociatedObject);
        }
    }

    private async void OnPointerMoved(object? sender, PointerEventArgs eventArgs)
    {
        if (!_isPressed || AssociatedObject == null)
        {
            return;
        }

        var currentPosition = eventArgs.GetPosition(AssociatedObject);
        var delta = currentPosition - _pointerStartPosition;

        if (Math.Abs(delta.X) > 5 || Math.Abs(delta.Y) > 5)
        {
            _isPressed = false;

            if (_pressedEvent != null && AssociatedObject.DataContext is HmiPanelViewModel sourcePanel)
            {
                var data = new DataTransfer();
                data.Add(DataTransferItem.Create(PanelFormat, sourcePanel));

                await DragDrop.DoDragDropAsync(_pressedEvent, data, DragDropEffects.Move);
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        _isPressed = false;
        _pressedEvent = null;
    }

    private void OnDragOver(object? sender, DragEventArgs eventArgs)
    {
        eventArgs.DragEffects = eventArgs.DataTransfer.Contains(PanelFormat) ? DragDropEffects.Move : DragDropEffects.None;
        eventArgs.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs eventArgs)
    {
        if (eventArgs.DataTransfer.TryGetValue(PanelFormat) is HmiPanelViewModel sourcePanel &&
            AssociatedObject?.DataContext is HmiPanelViewModel targetPanel)
        {
            var mainVm = FindMainViewModel(AssociatedObject);

            if (mainVm != null)
            {
                int oldIndex = mainVm.Panels.IndexOf(sourcePanel);
                int newIndex = mainVm.Panels.IndexOf(targetPanel);

                if (oldIndex != -1 && newIndex != -1)
                {
                    mainVm.MovePanel(oldIndex, newIndex);
                    eventArgs.Handled = true;
                }
            }
        }
    }

    private static MainViewModel? FindMainViewModel(Visual? visual)
    {
        while (visual != null)
        {
            if (visual is StyledElement { DataContext: MainViewModel mainVm })
            {
                return mainVm;
            }

            visual = visual.GetVisualParent();
        }

        return null;
    }
}

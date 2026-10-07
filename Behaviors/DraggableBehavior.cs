using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Behaviors;

public class DraggableBehavior : Behavior<Grid>
{
    private bool _isDragging;
    private Point _pointerStartPosition;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed += OnPointerPressed;
            AssociatedObject.PointerMoved += OnPointerMoved;
            AssociatedObject.PointerReleased += OnPointerReleased;
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
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        var point = eventArgs.GetCurrentPoint(AssociatedObject);
        var canvas = AssociatedObject?.FindLogicalAncestorOfType<Canvas>();

        if (AssociatedObject?.DataContext is HmiElementViewModel elementVm &&
            canvas?.DataContext is MainViewModel mainVm)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _pointerStartPosition = eventArgs.GetPosition(canvas);

                var modifiers = eventArgs.KeyModifiers;
                bool isCtrlDown = modifiers.HasFlag(KeyModifiers.Control);
                bool isShiftDown = modifiers.HasFlag(KeyModifiers.Shift);

                if (isCtrlDown || isShiftDown || !elementVm.IsSelected)
                {
                    mainVm.HandleSelection(elementVm, isCtrlDown, isShiftDown);
                }

                eventArgs.Pointer.Capture(AssociatedObject);
                eventArgs.Handled = true;
            }
            else if (point.Properties.IsRightButtonPressed)
            {
                if (!elementVm.IsSelected)
                {
                    mainVm.HandleSelection(elementVm, false, false);
                }
            }
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs eventArgs)
    {
        if (_isDragging && AssociatedObject?.DataContext is HmiElementViewModel elementVm)
        {
            var canvas = AssociatedObject.FindLogicalAncestorOfType<Canvas>();

            if (canvas != null)
            {
                var currentPosition = eventArgs.GetPosition(canvas);
                var delta = currentPosition - _pointerStartPosition;
                _pointerStartPosition = currentPosition;

                if (canvas.DataContext is MainViewModel mainVm)
                {
                    mainVm.MoveSelectedElements(delta.X, delta.Y);
                }

                eventArgs.Handled = true;
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        if (_isDragging)
        {
            _isDragging = false;
            eventArgs.Pointer.Capture(null);
            eventArgs.Handled = true;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Behaviors;

public class ResizableBehavior : Behavior<Rectangle>
{
    private bool _isResizing;
    private Point _resizeStartPosition;

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
        if (AssociatedObject?.Parent is Grid container && container.DataContext is HmiElementViewModel elementVm)
        {
            _isResizing = true;
            var canvas = container.FindLogicalAncestorOfType<Canvas>();

            if (canvas != null)
            {
                _resizeStartPosition = eventArgs.GetPosition(canvas);
            }

            eventArgs.Pointer.Capture(AssociatedObject);
            eventArgs.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs eventArgs)
    {
        if (_isResizing && AssociatedObject?.Parent is Grid container && container.DataContext is HmiElementViewModel elementVm)
        {
            var canvas = container.FindLogicalAncestorOfType<Canvas>();

            if (canvas != null)
            {
                var currentPosition = eventArgs.GetPosition(canvas);
                var delta = currentPosition - _resizeStartPosition;
                _resizeStartPosition = currentPosition;

                double newWidth = elementVm.Width + delta.X;
                double newHeight = elementVm.Height - delta.Y;
                double newTop = elementVm.Y + delta.Y;

                if (newWidth > 20)
                {
                    elementVm.Width = newWidth;
                }

                if (newHeight > 20)
                {
                    elementVm.Height = newHeight;
                    elementVm.Y = newTop;
                }

                eventArgs.Handled = true;
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        if (_isResizing)
        {
            _isResizing = false;
            eventArgs.Pointer.Capture(null);
            eventArgs.Handled = true;
        }
    }
}

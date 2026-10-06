using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Behaviors;

public class CanvasSelectionBehavior : Behavior<Canvas>
{
    private bool _isSelecting;
    private Point _pointerStartPosition;
    private Rectangle? _selectionBox;

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
        if (eventArgs.Source is Canvas &&
            eventArgs.Source == AssociatedObject && AssociatedObject.DataContext is MainViewModel mainVm)
        {
            _isSelecting = true;
            _pointerStartPosition = eventArgs.GetPosition(AssociatedObject);

            var modifiers = eventArgs.KeyModifiers;
            bool isCtrlDown = modifiers.HasFlag(KeyModifiers.Control);
            bool isShiftDown = modifiers.HasFlag(KeyModifiers.Shift);

            if (!isCtrlDown && !isShiftDown)
            {
                mainVm.ClearSelectionCommand.Execute(null);
            }

            _selectionBox = new Rectangle
            {
                Stroke = Avalonia.Media.Brushes.DodgerBlue,
                StrokeThickness = 1,
                StrokeDashArray = [2, 2],
                Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(30, 30, 144, 255))
            };

            Canvas.SetLeft(_selectionBox, _pointerStartPosition.X);
            Canvas.SetTop(_selectionBox, _pointerStartPosition.Y);
            _selectionBox.Width = 0;
            _selectionBox.Height = 0;

            AssociatedObject.Children.Add(_selectionBox);
            eventArgs.Pointer.Capture(AssociatedObject);
            eventArgs.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs eventArgs)
    {
        if (_isSelecting && _selectionBox != null && AssociatedObject != null)
        {
            var currentPosition = eventArgs.GetPosition(AssociatedObject);

            double x = Math.Min(_pointerStartPosition.X, currentPosition.X);
            double y = Math.Min(_pointerStartPosition.Y, currentPosition.Y);
            double width = Math.Abs(currentPosition.X - _pointerStartPosition.X);
            double height = Math.Abs(currentPosition.Y - _pointerStartPosition.Y);

            Canvas.SetLeft(_selectionBox, x);
            Canvas.SetTop(_selectionBox, y);
            _selectionBox.Width = width;
            _selectionBox.Height = height;

            eventArgs.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs eventArgs)
    {
        if (_isSelecting && AssociatedObject != null && _selectionBox != null)
        {
            _isSelecting = false;

            if (AssociatedObject.DataContext is MainViewModel mainVm)
            {
                double x = Canvas.GetLeft(_selectionBox);
                double y = Canvas.GetTop(_selectionBox);
                var rect = new Rect(x, y, _selectionBox.Width, _selectionBox.Height);

                var modifiers = eventArgs.KeyModifiers;
                bool isCtrlDown = modifiers.HasFlag(KeyModifiers.Control);
                bool isShiftDown = modifiers.HasFlag(KeyModifiers.Shift);

                mainVm.SelectElementsInRectangle(rect, isCtrlDown, isShiftDown);
            }

            AssociatedObject.Children.Remove(_selectionBox);
            _selectionBox = null;

            eventArgs.Pointer.Capture(null);
            eventArgs.Handled = true;
        }
    }
}

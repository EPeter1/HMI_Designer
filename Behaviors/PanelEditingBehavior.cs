using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Behaviors;

public class PanelEditingBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged += OnAssociatedObjectPropertyChanged;
            AssociatedObject.LostFocus += OnLostFocus;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged -= OnAssociatedObjectPropertyChanged;
            AssociatedObject.LostFocus -= OnLostFocus;
        }
    }

    private void OnAssociatedObjectPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.Property == Visual.IsVisibleProperty)
        {
            if (eventArgs.NewValue is bool isVisible && isVisible)
            {
                if (AssociatedObject?.DataContext is HmiPanelViewModel)
                {
                    AssociatedObject.Focus();
                    AssociatedObject.SelectAll();
                }
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs eventArgs)
    {
        if (AssociatedObject?.DataContext is HmiPanelViewModel panelVm)
        {
            if (eventArgs.Key == Key.Enter || eventArgs.Key == Key.Escape)
            {
                panelVm.IsEditing = false;
                eventArgs.Handled = true;
            }
        }
    }

    private void OnLostFocus(object? sender, RoutedEventArgs eventArgs)
    {
        if (AssociatedObject?.DataContext is HmiPanelViewModel panelVm && panelVm.IsEditing)
        {
            panelVm.EndRename("true");
        }
    }
}

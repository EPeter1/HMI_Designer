using Avalonia.Controls;
using Avalonia.Input;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Views;

public partial class CanvasView : UserControl
{
    public CanvasView()
    {
        InitializeComponent();
    }

    private void OnPanelPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (eventArgs.ClickCount == 2)
        {
            if (sender is Control control && control.DataContext is HmiPanelViewModel panelVm)
            {
                panelVm.StartRename();
                eventArgs.Handled = true;
            }
        }
    }
}

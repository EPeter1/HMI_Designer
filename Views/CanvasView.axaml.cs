using Avalonia.Controls;
using Avalonia.Input;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Views;

public partial class CanvasView : UserControl
{
    public CanvasView()
    {
        InitializeComponent();
        MainCanvas.PointerPressed += OnCanvasPointerPressed;
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs eventArgs)
    {
        if (eventArgs.Source == MainCanvas && DataContext is MainViewModel mainVm)
        {
            mainVm.ClearSelection();
        }
    }
}

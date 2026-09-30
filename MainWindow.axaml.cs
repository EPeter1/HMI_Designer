using Avalonia.Controls;

using HmiDesigner.ViewModels;

namespace HmiDesigner;

public partial class MainWindow : Window
{
    private MainViewModel? VM => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}

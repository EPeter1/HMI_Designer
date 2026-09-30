using CommunityToolkit.Mvvm.ComponentModel;

namespace HmiDesigner.ViewModels;

public enum ShapeType
{
    Ellipse,
    Rectangle
}

public partial class HmiElementViewModel : ObservableObject
{
    [ObservableProperty]
    private ShapeType _shapeType = ShapeType.Rectangle;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private double _x = 100;

    [ObservableProperty]
    private double _y = 100;

    [ObservableProperty]
    private double _width = 150;

    [ObservableProperty]
    private double _height = 50;

    [ObservableProperty]
    private string _text = "HMI Button";
}

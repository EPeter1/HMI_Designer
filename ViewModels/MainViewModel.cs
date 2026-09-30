using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HmiDesigner.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private HmiElementViewModel? _clipboardElement;

    [ObservableProperty]
    private HmiElementViewModel? _selectedElement;

    public ObservableCollection<HmiElementViewModel> Elements { get; } = new();

    public MainViewModel()
    {
        var initialElement = new HmiElementViewModel();

        Elements.Add(initialElement);
        SelectedElement = initialElement;
    }

    [RelayCommand]
    public void Add(string shapeTypeString)
    {
        var shapeType = shapeTypeString == "Ellipse" ? ShapeType.Ellipse : ShapeType.Rectangle;

        foreach (var element in Elements)
        {
            element.IsSelected = false;
        }

        var newElement = new HmiElementViewModel
        {
            ShapeType = shapeType,
            IsSelected = true,
            X = 150,
            Y = 150,
            Width = shapeType == ShapeType.Ellipse ? 80 : 150,
            Height = 80,
            Text = "HMI Button"
        };

        Elements.Add(newElement);
        SelectedElement = newElement;
    }

    [RelayCommand]
    public void Delete()
    {
        if (SelectedElement != null)
        {
            Elements.Remove(SelectedElement);
            SelectedElement = null;
        }
    }

    [RelayCommand]
    public void Copy()
    {
        if (SelectedElement != null)
        {
            _clipboardElement = new HmiElementViewModel
            {
                ShapeType = SelectedElement.ShapeType,
                X = SelectedElement.X,
                Y = SelectedElement.Y,
                Width = SelectedElement.Width,
                Height = SelectedElement.Height,
                Text = SelectedElement.Text
            };
        }
    }

    [RelayCommand]
    public void Paste()
    {
        if (_clipboardElement != null)
        {
            var newElement = new HmiElementViewModel
            {
                ShapeType = _clipboardElement.ShapeType,
                X = _clipboardElement.X + 20,
                Y = _clipboardElement.Y + 20,
                Width = _clipboardElement.Width,
                Height = _clipboardElement.Height,
                Text = _clipboardElement.Text
            };

            Elements.Add(newElement);

            foreach (var element in Elements)
            {
                element.IsSelected = element == newElement;
            }

            SelectedElement = newElement;
        }
    }

    [RelayCommand]
    public void ClearSelection()
    {
        foreach (var element in Elements)
        {
            element.IsSelected = false;
        }

        SelectedElement = null;
    }

    public void HandleSelection(HmiElementViewModel hmiElement, bool isCtrlDown, bool isShiftDown)
    {
        if (isCtrlDown)
        {
            hmiElement.IsSelected = !hmiElement.IsSelected;
        }
        else if (isShiftDown)
        {
            hmiElement.IsSelected = true;
        }
        else
        {
            foreach (var element in Elements)
            {
                element.IsSelected = element == hmiElement;
            }
        }

        SelectedElement = Elements.FirstOrDefault(element => element.IsSelected);
    }

    public void MoveSelectedElements(double deltaX, double deltaY)
    {
        foreach (var element in Elements)
        {
            if (element.IsSelected)
            {
                element.X += deltaX;
                element.Y += deltaY;
            }
        }
    }
}

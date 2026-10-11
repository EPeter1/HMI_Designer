using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HmiDesigner.ViewModels;

public partial class HmiPanelViewModel : ObservableObject
{
    private List<HmiElementViewModel> _clipboardElements = new();
    private string _originalPanelName = string.Empty;

    [ObservableProperty]
    private string _panelName = "New Panel";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private HmiElementViewModel? _selectedElement;

    public ObservableCollection<HmiElementViewModel> Elements { get; } = new();

    public HmiPanelViewModel(string name)
    {
        PanelName = name;

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
        var elementsToDelete = Elements.Where(element => element.IsSelected).ToList();

        if (elementsToDelete.Count > 0)
        {
            foreach (var element in elementsToDelete)
            {
                Elements.Remove(element);
            }

            SelectedElement = null;
        }
    }

    [RelayCommand]
    public void Copy()
    {
        _clipboardElements.Clear();

        foreach (var element in Elements.Where(element => element.IsSelected))
        {
            _clipboardElements.Add(new HmiElementViewModel
            {
                ShapeType = element.ShapeType,
                X = element.X,
                Y = element.Y,
                Width = element.Width,
                Height = element.Height,
                Text = element.Text
            });
        }
    }

    [RelayCommand]
    public void Cut()
    {
        Copy();
        Delete();
    }

    [RelayCommand]
    public void Paste()
    {
        if (_clipboardElements.Count == 0)
        {
            return;
        }

        foreach (var element in Elements)
        {
            element.IsSelected = false;
        }

        var pastedElements = new List<HmiElementViewModel>();

        foreach (var clipElement in _clipboardElements)
        {
            var newElement = new HmiElementViewModel
            {
                ShapeType = clipElement.ShapeType,
                X = clipElement.X + 20,
                Y = clipElement.Y + 20,
                Width = clipElement.Width,
                Height = clipElement.Height,
                Text = clipElement.Text,
                IsSelected = true
            };

            Elements.Add(newElement);
            pastedElements.Add(newElement);
        }

        SelectedElement = pastedElements.FirstOrDefault();
    }

    [RelayCommand]
    public void SelectAll()
    {
        foreach (var element in Elements)
        {
            element.IsSelected = true;
        }

        SelectedElement = Elements.FirstOrDefault();
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

    [RelayCommand]
    public void StartRename()
    {
        _originalPanelName = PanelName;
        IsEditing = true;
    }

    [RelayCommand]
    public void EndRename(string shouldCancel)
    {
        if (bool.Parse(shouldCancel))
        {
            PanelName = _originalPanelName;
        }

        IsEditing = false;
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

    public void SelectElementsInRectangle(Avalonia.Rect selectionRect, bool isCtrlDown, bool isShiftDown)
    {
        if (!isCtrlDown && !isShiftDown)
        {
            foreach (var element in Elements)
            {
                element.IsSelected = false;
            }
        }

        foreach (var element in Elements)
        {
            var elementRect = new Avalonia.Rect(element.X, element.Y, element.Width, element.Height);

            if (selectionRect.Contains(elementRect) || selectionRect.Intersects(elementRect))
            {
                element.IsSelected = true;
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

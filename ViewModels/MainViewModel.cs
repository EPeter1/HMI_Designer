using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HmiDesigner.Services;

namespace HmiDesigner.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly IFileService _fileService;
    private HmiElementViewModel? _clipboardElement;

    [ObservableProperty]
    private HmiElementViewModel? _selectedElement;

    [ObservableProperty]
    private string _windowTitle = "HMI Designer - [New Project]*";

    [ObservableProperty]
    private bool _isModified;

    public ObservableCollection<HmiElementViewModel> Elements { get; } = new();

    public MainViewModel(MainViewModelServices services)
    {
        _dialogService = services.DialogService;
        _fileService = services.FileService;

        Elements.CollectionChanged += (sender, eventArgs) =>
        {
            MarkAsModified();

            if (eventArgs.NewItems != null)
            {
                foreach (HmiElementViewModel item in eventArgs.NewItems)
                {
                    item.PropertyChanged += (sender, eventArgs) => MarkAsModified();
                }
            }
        };

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

    [RelayCommand]
    public async Task NewAsync(TopLevel topLevel)
    {
        if (IsModified)
        {
            bool isConfirmed = await _dialogService.ShowConfirmDialogAsync(
                topLevel, 
                "HMI Designer", 
                "Your project has unsaved changes. Would you like to save them?"
            );

            if (isConfirmed)
            {
                await SaveAsync(topLevel);
            }
        }

        Elements.Clear();
        _fileService.Reset();
        IsModified = false;

        WindowTitle = "HMI Designer - [New Project]*";
        SelectedElement = null;
    }

    [RelayCommand]
    public async Task OpenAsync(TopLevel topLevel)
    {
        var filePath = await _dialogService.ShowOpenDialogAsync(topLevel);

        if (!string.IsNullOrEmpty(filePath))
        {
            var loadedElements = await _fileService.LoadAsync(filePath);

            Elements.Clear();
            foreach (var element in loadedElements)
            {
                Elements.Add(element);
            }

            IsModified = false;
            WindowTitle = $"HMI Designer - {Path.GetFileName(filePath)}";
            SelectedElement = Elements.FirstOrDefault();
        }
    }

    [RelayCommand]
    public async Task SaveAsync(TopLevel topLevel)
    {
        if (string.IsNullOrEmpty(_fileService.FilePath))
        {
            await SaveAsAsync(topLevel);
        }
        else
        {
            await SaveToFileAsync(_fileService.FilePath);
        }
    }

    [RelayCommand]
    public async Task SaveAsAsync(TopLevel topLevel)
    {
        var filePath = await _dialogService.ShowSaveDialogAsync(topLevel);

        if (!string.IsNullOrEmpty(filePath))
        {
            await SaveToFileAsync(filePath);
        }
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

    private void MarkAsModified()
    {
        IsModified = true;

        if (!WindowTitle.EndsWith('*'))
        {
            WindowTitle += "*";
        }
    }

    private async Task SaveToFileAsync(string path)
    {
        await _fileService.SaveAsync(path, Elements);
        IsModified = false;
        WindowTitle = $"HMI Designer - {Path.GetFileName(path)}";
    }
}

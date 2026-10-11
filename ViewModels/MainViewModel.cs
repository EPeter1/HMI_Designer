using System;
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
    private int _panelCounter = 1;

    [ObservableProperty]
    private HmiPanelViewModel? _selectedPanel;

    [ObservableProperty]
    private string _windowTitle = "HMI Designer - [New Project]*";

    [ObservableProperty]
    private bool _isModified;

    public ObservableCollection<HmiPanelViewModel> Panels { get; } = new();

    public MainViewModel(MainViewModelServices services)
    {
        _dialogService = services.DialogService;
        _fileService = services.FileService;

        Panels.CollectionChanged += (sender, eventArgs) =>
        {
            MarkAsModified();

            if (eventArgs.NewItems != null)
            {
                foreach (HmiPanelViewModel panel in eventArgs.NewItems)
                {
                    HookPanelEvents(panel);
                }
            }
        };

        var initialPanel = new HmiPanelViewModel($"Panel {_panelCounter++}");
        Panels.Add(initialPanel);
        SelectedPanel = initialPanel;

        HookPanelEvents(initialPanel);
    }

    [RelayCommand]
    public void AddPanel()
    {
        var newPanel = new HmiPanelViewModel($"Panel {_panelCounter++}");
        Panels.Add(newPanel);
        SelectedPanel = newPanel;

        IsModified = true;
    }

    [RelayCommand]
    public void DeletePanel(HmiPanelViewModel panel)
    {
        int index = Panels.IndexOf(panel);
        Panels.Remove(panel);

        if (SelectedPanel == panel)
        {
            SelectedPanel = Panels[Math.Clamp(index - 1, 0, Panels.Count - 1)];
        }

        IsModified = true;
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

        Panels.Clear();
        _panelCounter = 1;

        var newPanel = new HmiPanelViewModel($"Panel {_panelCounter++}");
        Panels.Add(newPanel);
        SelectedPanel = newPanel;

        _fileService.Reset();
        IsModified = false;
        WindowTitle = "HMI Designer - [New Project]*";
    }

    [RelayCommand]
    public async Task OpenAsync(TopLevel topLevel)
    {
        var filePath = await _dialogService.ShowOpenDialogAsync(topLevel);

        if (!string.IsNullOrEmpty(filePath))
        {
            var loadedPanels = await _fileService.LoadAsync(filePath);

            Panels.Clear();
            _panelCounter = 1;

            foreach (var panelData in loadedPanels)
            {
                var panel = new HmiPanelViewModel(panelData.PanelName);
                panel.Elements.Clear();

                foreach (var element in panelData.Elements)
                {
                    panel.Elements.Add(element);
                }

                panel.SelectedElement = panel.Elements.FirstOrDefault();
                
                Panels.Add(panel);
                HookPanelEvents(panel);
                _panelCounter++;
            }

            if (Panels.Count == 0)
            {
                var initialPanel = new HmiPanelViewModel($"Panel {_panelCounter++}");
                Panels.Add(initialPanel);
                HookPanelEvents(initialPanel);
            }

            SelectedPanel = Panels.FirstOrDefault();

            IsModified = false;
            WindowTitle = $"HMI Designer - {Path.GetFileName(filePath)}";
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

    public void MovePanel(int oldIndex, int newIndex)
    {
        if (oldIndex >= 0 && oldIndex < Panels.Count && newIndex >= 0 && newIndex < Panels.Count && oldIndex != newIndex)
        {
            Panels.Move(oldIndex, newIndex);
            SelectedPanel = Panels[newIndex];
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

    private void HookPanelEvents(HmiPanelViewModel panel)
    {
        panel.PropertyChanged += (sender, eventArgs) => MarkAsModified();

        panel.Elements.CollectionChanged += (sender, eventArgs) =>
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

        foreach (var element in panel.Elements)
        {
            element.PropertyChanged += (sender, eventArgs) => MarkAsModified();
        }
    }

    private async Task SaveToFileAsync(string path)
    {
        await _fileService.SaveAsync(path, Panels);

        IsModified = false;
        WindowTitle = $"HMI Designer - {Path.GetFileName(path)}";
    }
}

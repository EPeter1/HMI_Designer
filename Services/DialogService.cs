using System;
using System.IO;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace HmiDesigner.Services;

public class DialogService : IDialogService
{
    public async Task<string?> ShowOpenDialogAsync(TopLevel topLevel)
    {
        string defaultPath = Path.Combine(AppContext.BaseDirectory, "Projects");
        Directory.CreateDirectory(defaultPath);
        var defaultFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(defaultPath);

        var options = new FilePickerOpenOptions
        {
            Title = "Open Project",
            AllowMultiple = false,
            SuggestedStartLocation = defaultFolder,
            FileTypeFilter = [ new FilePickerFileType("HMI Project File") { Patterns = ["*.json"] } ]
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<string?> ShowSaveDialogAsync(TopLevel topLevel)
    {
        string defaultPath = Path.Combine(AppContext.BaseDirectory, "Projects");
        Directory.CreateDirectory(defaultPath);
        var defaultFolder = await topLevel.StorageProvider.TryGetFolderFromPathAsync(defaultPath);

        var options = new FilePickerSaveOptions
        {
            Title = "Save Project",
            DefaultExtension = "json",
            SuggestedStartLocation = defaultFolder,
            FileTypeChoices = [ new FilePickerFileType("HMI Project File") { Patterns = ["*.json"] } ]
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        return file?.Path.LocalPath;
    }

    public async Task<bool> ShowConfirmDialogAsync(TopLevel topLevel, string title, string message)
    {
        var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(title,
            message,
            MsBox.Avalonia.Enums.ButtonEnum.YesNo,
            MsBox.Avalonia.Enums.Icon.Question
        );

        var result = await messageBox.ShowWindowDialogAsync((Window)topLevel);
        return result == MsBox.Avalonia.Enums.ButtonResult.Yes;
    }
}

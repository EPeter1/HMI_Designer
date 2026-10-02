using System.Threading.Tasks;

using Avalonia.Controls;

namespace HmiDesigner.Services;

public interface IDialogService
{
    Task<string?> ShowOpenDialogAsync(TopLevel topLevel);
    Task<string?> ShowSaveDialogAsync(TopLevel topLevel);
    Task<bool> ShowConfirmDialogAsync(TopLevel topLevel, string title, string message);
}

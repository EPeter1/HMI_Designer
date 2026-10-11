using System.Collections.Generic;
using System.Threading.Tasks;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Services;

public interface IFileService
{
    string? FilePath { get; }

    Task SaveAsync(string filePath, IEnumerable<HmiPanelViewModel> panels);
    Task<List<HmiPanelDto>> LoadAsync(string filePath);
    void Reset();
}

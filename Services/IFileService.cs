using System.Collections.Generic;
using System.Threading.Tasks;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Services;

public interface IFileService
{
    string? FilePath { get; }

    Task SaveAsync(string path, IEnumerable<HmiElementViewModel> elements);
    Task<List<HmiElementViewModel>> LoadAsync(string path);
    void Reset();
}

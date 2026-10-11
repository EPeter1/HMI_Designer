using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Services;

public class HmiPanelDto
{
    public string PanelName { get; set; } = "New Panel";
    public List<HmiElementViewModel> Elements { get; set; } = new();
}

public class HmiProjectDto
{
    public List<HmiPanelDto> Panels { get; set; } = new();
}

public class FileService : IFileService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public string? FilePath { get; private set; }

    public async Task SaveAsync(string filePath, IEnumerable<HmiPanelViewModel> panels)
    {
        FilePath = filePath;

        var projectDto = new HmiProjectDto
        {
            Panels = [.. panels.Select(panel => new HmiPanelDto
            {
                PanelName = panel.PanelName,
                Elements = [.. panel.Elements]
            })]
        };

        var jsonString = JsonSerializer.Serialize(projectDto, _jsonOptions);
        await File.WriteAllTextAsync(filePath, jsonString);
    }

    public async Task<List<HmiPanelDto>> LoadAsync(string filePath)
    {
        FilePath = filePath;
        if (!File.Exists(filePath))
        {
            return [];
        }

        var jsonString = await File.ReadAllTextAsync(filePath);
        var projectDto = JsonSerializer.Deserialize<HmiProjectDto>(jsonString);

        return projectDto?.Panels ?? [];
    }

    public void Reset()
    {
        FilePath = null;
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using HmiDesigner.ViewModels;

namespace HmiDesigner.Services;

public class FileService : IFileService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public string? FilePath { get; private set; }

    public async Task SaveAsync(string filePath, IEnumerable<HmiElementViewModel> elements)
    {
        FilePath = filePath;
        var jsonString = JsonSerializer.Serialize(elements, _jsonOptions);

        await File.WriteAllTextAsync(filePath, jsonString);
    }

    public async Task<List<HmiElementViewModel>> LoadAsync(string filePath)
    {
        FilePath = filePath;
        if (!File.Exists(filePath))
        {
            return [];
        }

        var jsonString = await File.ReadAllTextAsync(filePath);
        var elements = JsonSerializer.Deserialize<List<HmiElementViewModel>>(jsonString);

        return elements ?? [];
    }

    public void Reset()
    {
        FilePath = null;
    }
}

using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
namespace NetSupport.Shared.Storage;

public static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        
        // Preserve Arabic characters as-is — do NOT escape to \uXXXX.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static async Task<T?> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, Options);
    }

    public static async Task SaveAsync<T>(string path, T value)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value, Options);
    }
}

using System.Text.Json;

namespace NetSupport.Shared.Storage;

public sealed class TutorConnectionSettings
{
    public string TutorListenUrl { get; set; } = "http://0.0.0.0:5000";
    public string StudentHubUrl { get; set; } = "http://127.0.0.1:5000/tutorHub";

    private static string SettingsPath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetSupportSchool");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "connection-settings.json");
        }
    }

    public static TutorConnectionSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new TutorConnectionSettings();
            }

            var json = File.ReadAllText(SettingsPath);
            var loaded = JsonSerializer.Deserialize<TutorConnectionSettings>(json);
            return loaded ?? new TutorConnectionSettings();
        }
        catch
        {
            return new TutorConnectionSettings();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(SettingsPath, json);
    }
}

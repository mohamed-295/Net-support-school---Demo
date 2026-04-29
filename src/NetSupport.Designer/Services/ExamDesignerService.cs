using NetSupport.Shared.Models;
using NetSupport.Shared.Storage;

namespace NetSupport.Designer.Services;

public sealed class ExamDesignerService
{
    public string BuildPath(string folder, string title)
    {
        Directory.CreateDirectory(folder);

        var fileName = JsonFileStore.NormalizeFileName(title) + ".json";

        return Path.Combine(folder, fileName);
    }

    public Task SaveExamAsync(string path, Exam exam)
    {
        return JsonFileStore.SaveAsync(path, exam, overwrite: true);
    }

    public Task<Exam?> LoadExamAsync(string path)
    {
        return JsonFileStore.LoadAsync<Exam>(path);
    }

    public bool IsValidExam(string title, List<Question> questions)
    {
        return !string.IsNullOrWhiteSpace(title) && questions.Count > 0;
    }
}
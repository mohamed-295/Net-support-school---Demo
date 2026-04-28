using NetSupport.Shared.Models;
using NetSupport.Shared.Storage;

namespace NetSupport.Designer.Services;

public sealed class ExamDesignerService
{
    public Task SaveExamAsync(string path, Exam exam)
    {
        return JsonFileStore.SaveAsync(path, exam);
    }

    public async Task<Exam> LoadExamAsync(string path)
    {
        return await JsonFileStore.LoadAsync<Exam>(path) ?? new Exam();
    }
}

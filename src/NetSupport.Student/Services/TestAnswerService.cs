using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

public sealed class TestAnswerService
{
    private readonly List<StudentAnswer> _answers = new();

    public IReadOnlyList<StudentAnswer> Answers => _answers;

    public void SaveAnswer(StudentAnswer answer)
    {
        _answers.RemoveAll(existing => existing.QuestionId == answer.QuestionId);
        _answers.Add(answer);
    }
}

using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

public sealed class StudentRegistry
{
    private readonly Dictionary<string, StudentInfo> _students = new();

    public IReadOnlyCollection<StudentInfo> Students => _students.Values;

    public void Upsert(StudentInfo student)
    {
        student.LastSeenUtc = DateTime.UtcNow;
        _students[student.StudentId] = student;
    }

    public bool Remove(string studentId)
    {
        return _students.Remove(studentId);
    }
}

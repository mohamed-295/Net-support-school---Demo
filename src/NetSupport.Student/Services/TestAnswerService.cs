using NetSupport.Shared.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;

namespace NetSupport.Student.Services;

public static class TestAnswerService
{
    public static async Task SendProgressAsync(
        string studentId,
        string sessionId,
        int answeredCount,
        int totalQuestions,
        string status = "Testing",
        int remainingSeconds = 0,
        IEnumerable<StudentAnswer>? answers = null)
    {
        if (StudentClient.Connection == null)
        {
            return;
        }

        var progress = new StudentProgress
        {
            StudentId = studentId,
            SessionId = sessionId,
            AnsweredCount = answeredCount,
            TotalQuestions = totalQuestions,
            RemainingSeconds = remainingSeconds,
            Status = status,
            Answers = answers?.ToList() ?? new List<StudentAnswer>()
        };

        await StudentClient.Connection.InvokeAsync("SendProgress", progress);
    }

    public static async Task SubmitAsync(string studentId, string sessionId, List<StudentAnswer> answers)
    {
        if (StudentClient.Connection == null)
        {
            return;
        }

        foreach (var answer in answers)
        {
            answer.StudentId = studentId;
            answer.SessionId = sessionId;
            answer.AnsweredAtUtc = DateTime.UtcNow;
        }

        await StudentClient.Connection.InvokeAsync("SubmitAnswers", studentId, answers);
    }
}
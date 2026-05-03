using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

public sealed class ReportService
{
    public static string CalculateScore(Exam exam, List<StudentAnswer> answers)
    {
        var correctByQuestion = new Dictionary<string, string>();
        foreach (var question in exam.Questions)
        {
            var correctChoice = question.Choices.FirstOrDefault(c => c.IsCorrect);
            if (correctChoice != null)
            {
                correctByQuestion[question.Id] = correctChoice.Id;
            }
        }

        var correctCount = 0;
        foreach (var answer in answers)
        {
            if (correctByQuestion.TryGetValue(answer.QuestionId, out var correctId)
                && string.Equals(answer.ChoiceId, correctId, StringComparison.OrdinalIgnoreCase))
            {
                correctCount++;
            }
        }

        return $"{correctCount}/{exam.Questions.Count}";
    }

    public static string CreateHtmlReport(IEnumerable<ReportRow> rows)
    {
        var tableRows = string.Join(Environment.NewLine, rows.Select(row =>
            $"<tr><td>{row.StudentName}</td><td>{row.Score}</td><td>{row.AnsweredQuestions}</td><td>{row.TotalQuestions}</td></tr>"));

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <title>NetSupport Test Report</title>
            </head>
            <body>
                <h1>NetSupport Test Report</h1>
                <table border="1" cellspacing="0" cellpadding="8">
                    <thead>
                        <tr><th>Student</th><th>Score (correct/total)</th><th>Answered</th><th>Total</th></tr>
                    </thead>
                    <tbody>
                        {{tableRows}}
                    </tbody>
                </table>
            </body>
            </html>
            """;
    }
}

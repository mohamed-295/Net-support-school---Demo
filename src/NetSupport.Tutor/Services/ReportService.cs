using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

public sealed class ReportService
{
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
                        <tr><th>Student</th><th>Score</th><th>Answered</th><th>Total</th></tr>
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

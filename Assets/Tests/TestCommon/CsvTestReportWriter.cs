using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;

internal static class CsvTestReportWriter
{
    private const string DefaultTesterName = "Huỳnh Ngọc Thanh Phước";

    internal static readonly string[] Headers =
    {
        "Mã TC",
        "Tiêu đề testcase",
        "Kết quả mong đợi",
        "Kết quả thực tế",
        "Kiểu chạy (Thủ công / Tự động)",
        "Người kiểm thử",
        "Ngày bắt đầu",
        "Chi tiết bước kiểm thử",
        "Ghi chú",
    };

    internal static string GetTesterName()
    {
        var env = Environment.GetEnvironmentVariable("TESTER_NAME");
        if (!string.IsNullOrWhiteSpace(env))
            return env.Trim();

        return DefaultTesterName;
    }

    internal static string GetReportDirectory()
    {
        // Project root: <project>/Assets -> <project>
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(root, "TestReports");
    }

    internal static string GetReportPath()
    {
        var dir = GetReportDirectory();
        Directory.CreateDirectory(dir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var platform = Application.platform.ToString();
        return Path.Combine(dir, $"BaoCaoKiemThu_{platform}_{timestamp}.csv");
    }

    internal static void WriteHeaderIfNew(string path)
    {
        if (File.Exists(path))
            return;

        // UTF-8 BOM so Excel reads Vietnamese correctly.
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        writer.WriteLine(string.Join(",", QuoteAll(Headers)));
    }

    internal static void AppendRow(string path, IReadOnlyList<string> row)
    {
        WriteHeaderIfNew(path);

        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.WriteLine(string.Join(",", QuoteAll(row)));
    }

    internal static List<string> BuildRow(ITest test, DateTime startTime, object result)
    {
        string id = GetProp(test, TestCaseMetaAttribute.KeyId, defaultValue: test.Name);
        string title = GetProp(test, TestCaseMetaAttribute.KeyTitle, defaultValue: test.Name);
        string expected = GetProp(test, TestCaseMetaAttribute.KeyExpected, defaultValue: "");
        string steps = GetProp(test, TestCaseMetaAttribute.KeySteps, defaultValue: "");
        string runType = GetProp(test, TestCaseMetaAttribute.KeyRunType, defaultValue: "Tự động");
        string notes = GetProp(test, TestCaseMetaAttribute.KeyNotes, defaultValue: "");

        var (statusName, label, message) = GetResultInfo(result);

        string actual;
        if (string.Equals(statusName, nameof(TestStatus.Passed), StringComparison.OrdinalIgnoreCase))
        {
            actual = "Đạt";
        }
        else if (string.Equals(statusName, nameof(TestStatus.Skipped), StringComparison.OrdinalIgnoreCase))
        {
            actual = $"Bỏ qua: {message}";
        }
        else
        {
            var msg = message;
            if (string.IsNullOrWhiteSpace(msg)) msg = label;
            actual = $"Không đạt: {msg}";
        }

        var tester = GetTesterName();
        var start = startTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        return new List<string>
        {
            id,
            title,
            expected,
            actual,
            runType,
            tester,
            start,
            steps,
            notes,
        };
    }

    private static (string statusName, string label, string message) GetResultInfo(object result)
    {
        // Unity's embedded NUnit exposes TestContext.CurrentContext.Result as ResultAdapter,
        // but its surface area varies by version. Use reflection to support both:
        // - ResultState.{Status,Label}
        // - Outcome.{Status,Label}
        // plus Message.
        var status = GetNestedPropertyValue(result, "ResultState", "Status")
                     ?? GetNestedPropertyValue(result, "Outcome", "Status");
        var label = GetNestedPropertyValue(result, "ResultState", "Label")
                    ?? GetNestedPropertyValue(result, "Outcome", "Label");
        var message = GetPropertyValue(result, "Message");

        return (
            status?.ToString() ?? string.Empty,
            label?.ToString() ?? string.Empty,
            message?.ToString() ?? string.Empty
        );
    }

    private static object GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null) return null;
        try
        {
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private static object GetNestedPropertyValue(object obj, string parentProperty, string childProperty)
    {
        var parent = GetPropertyValue(obj, parentProperty);
        if (parent == null) return null;
        return GetPropertyValue(parent, childProperty);
    }

    private static string GetProp(ITest test, string key, string defaultValue)
    {
        try
        {
            var props = test.Properties;
            if (props != null && props.ContainsKey(key))
            {
                var val = props.Get(key);
                if (val != null)
                    return val.ToString();
            }
        }
        catch { }

        return defaultValue;
    }

    private static IEnumerable<string> QuoteAll(IEnumerable<string> values)
    {
        foreach (var v in values)
            yield return Quote(v ?? string.Empty);
    }

    private static string Quote(string value)
    {
        // CSV quoting: wrap in "..." and double internal quotes.
        bool mustQuote = value.Contains(",") || value.Contains("\n") || value.Contains("\r") || value.Contains('"');
        if (!mustQuote)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}

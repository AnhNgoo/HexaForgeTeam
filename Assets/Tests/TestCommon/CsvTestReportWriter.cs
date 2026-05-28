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

    internal const string GroupEnemy = "Enemy";
    internal const string GroupGold = "Gold";
    internal const string GroupPlayer = "Player";
    internal const string GroupOther = "Other";

    internal static readonly string[] Headers =
    {
        "Nhóm",
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

        // Fixed name so the file is easy to import to Excel and persists across runs.
        return Path.Combine(dir, "All.csv");
    }

    internal static string GetGroupReportPath(string group)
    {
        var dir = GetReportDirectory();
        Directory.CreateDirectory(dir);

        var normalized = NormalizeGroup(group);
        return Path.Combine(dir, $"{normalized}.csv");
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

    internal static void WriteHeaderOverwrite(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        // UTF-8 BOM so Excel reads Vietnamese correctly.
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        writer.WriteLine(string.Join(",", QuoteAll(Headers)));
    }

    internal static void StartNewRunOverwrite()
    {
        // Create stable CSV files (easy for Excel import) and overwrite contents each run.
        WriteHeaderOverwrite(GetReportPath());
        WriteHeaderOverwrite(GetGroupReportPath(GroupEnemy));
        WriteHeaderOverwrite(GetGroupReportPath(GroupPlayer));
        WriteHeaderOverwrite(GetGroupReportPath(GroupGold));
        WriteHeaderOverwrite(GetGroupReportPath(GroupOther));
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
        string group = DetermineGroup(test);
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
            group,
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

    private static string DetermineGroup(ITest test)
    {
        if (HasCategory(test, GroupEnemy)) return GroupEnemy;
        if (HasCategory(test, GroupGold)) return GroupGold;

        // Tests currently use Category("Character") for the player suite,
        // but the user wants a stable file name: Player.csv.
        if (HasCategory(test, "Character")) return GroupPlayer;
        if (HasCategory(test, GroupPlayer)) return GroupPlayer;

        var name = (test.FullName ?? test.Name) ?? string.Empty;
        if (name.IndexOf(GroupEnemy, StringComparison.OrdinalIgnoreCase) >= 0) return GroupEnemy;
        if (name.IndexOf(GroupGold, StringComparison.OrdinalIgnoreCase) >= 0) return GroupGold;
        if (name.IndexOf("Player", StringComparison.OrdinalIgnoreCase) >= 0) return GroupPlayer;
        return GroupPlayer;
    }

    private static string NormalizeGroup(string group)
    {
        if (string.IsNullOrWhiteSpace(group))
            return GroupOther;

        var g = group.Trim();
        if (g.Equals("Character", StringComparison.OrdinalIgnoreCase))
            return GroupPlayer;

        if (g.Equals(GroupEnemy, StringComparison.OrdinalIgnoreCase)) return GroupEnemy;
        if (g.Equals(GroupGold, StringComparison.OrdinalIgnoreCase)) return GroupGold;
        if (g.Equals(GroupPlayer, StringComparison.OrdinalIgnoreCase)) return GroupPlayer;

        return GroupOther;
    }

    private static bool HasCategory(ITest test, string category)
    {
        try
        {
            var props = test.Properties;
            if (props == null) return false;
            if (!props.ContainsKey("Category")) return false;

            var values = props.Get("Category");
            if (values is System.Collections.IEnumerable enumerable)
            {
                foreach (var v in enumerable)
                {
                    if (v == null) continue;
                    if (string.Equals(v.ToString(), category, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            else if (values != null)
            {
                return string.Equals(values.ToString(), category, StringComparison.OrdinalIgnoreCase);
            }
        }
        catch { }

        return false;
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

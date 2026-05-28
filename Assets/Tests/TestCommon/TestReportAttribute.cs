using System;
using System.Collections.Concurrent;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

/// <summary>
/// Attach to a test fixture to automatically write a Vietnamese CSV test report row per test.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TestReportAttribute : Attribute, ITestAction
{
    private static readonly ConcurrentDictionary<string, DateTime> StartTimes = new();
    private static readonly ConcurrentDictionary<string, string> ReportPaths = new();

    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
        if (test == null) return;

        var key = test.FullName ?? test.Name;
        StartTimes[key] = DateTime.Now;

        // Ensure one report file per test run (per assembly load) using a lazily created shared path.
        // Keyed by AppDomain-friendly name (fallback to "default").
        var domainKey = AppDomain.CurrentDomain.FriendlyName ?? "default";
        ReportPaths.GetOrAdd(domainKey, _ => CsvTestReportWriter.GetReportPath());
    }

    public void AfterTest(ITest test)
    {
        if (test == null) return;

        var key = test.FullName ?? test.Name;
        if (!StartTimes.TryGetValue(key, out var start))
            start = DateTime.Now;

        var domainKey = AppDomain.CurrentDomain.FriendlyName ?? "default";
        if (!ReportPaths.TryGetValue(domainKey, out var reportPath))
            reportPath = CsvTestReportWriter.GetReportPath();

        var result = TestContext.CurrentContext.Result;
        var row = CsvTestReportWriter.BuildRow(test, start, result);
        CsvTestReportWriter.AppendRow(reportPath, row);

        // Also write to a stable per-suite CSV for Excel import.
        // File names are fixed: Enemy.csv / Player.csv / Gold.csv.
        if (row != null && row.Count > 0)
        {
            var group = row[0];
            var groupPath = CsvTestReportWriter.GetGroupReportPath(group);
            CsvTestReportWriter.AppendRow(groupPath, row);
        }

        if (test.IsSuite == false && test.Parent != null)
        {
            // Light logging so user can find the report.
            UnityEngine.Debug.Log($"[TestReport] Đã ghi CSV: {reportPath}");
        }

        StartTimes.TryRemove(key, out _);
    }
}

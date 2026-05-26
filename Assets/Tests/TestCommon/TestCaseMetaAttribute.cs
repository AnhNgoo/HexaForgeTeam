using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestCaseMetaAttribute : Attribute, IApplyToTest
{
    public const string KeyId = "TC_ID";
    public const string KeyTitle = "TC_Title";
    public const string KeyExpected = "TC_Expected";
    public const string KeySteps = "TC_Steps";
    public const string KeyRunType = "TC_RunType";
    public const string KeyNotes = "TC_Notes";

    public string Id { get; }
    public string Title { get; }
    public string Expected { get; }
    public string Steps { get; }
    public string RunType { get; }
    public string Notes { get; }

    public TestCaseMetaAttribute(
        string id,
        string title,
        string expected = "",
        string steps = "",
        string runType = "Tự động",
        string notes = "")
    {
        Id = id;
        Title = title;
        Expected = expected;
        Steps = steps;
        RunType = runType;
        Notes = notes;
    }

    public void ApplyToTest(Test test)
    {
        test.Properties.Set(KeyId, Id);
        test.Properties.Set(KeyTitle, Title);
        test.Properties.Set(KeyExpected, Expected);
        test.Properties.Set(KeySteps, Steps);
        test.Properties.Set(KeyRunType, RunType);
        test.Properties.Set(KeyNotes, Notes);
    }
}

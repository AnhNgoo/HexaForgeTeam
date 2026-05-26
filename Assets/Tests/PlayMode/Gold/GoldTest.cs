using NUnit.Framework;

[TestReport]
[Category("Smoke")]
public class GoldTest
{
    [Test]
    [Category("P2")]
    [TestCaseMeta(
        id: "GL-SMK-001",
        title: "Smoke: NUnit chạy được (Gold suite)",
        expected: "Test framework hoạt động và không có exception.",
        steps: "1) Chạy test. 2) Verify pass.")]
    public void Smoke_TestFramework_Runs()
    {
        Assert.Pass("Gold smoke test.");
    }
}

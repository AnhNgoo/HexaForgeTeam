using NUnit.Framework;

[Category("Smoke")]
[Category("Gold")]
public class GoldTest
{
    [Test]
    [Category("P2")]
    [Description("TC GL-SMK-001: Smoke test để xác nhận bộ test Gold chạy được và không phát sinh exception.")]
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

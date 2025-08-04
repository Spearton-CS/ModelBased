using System.Diagnostics;

namespace MainTests;

[TestClass, DoNotParallelize]
public class ActiveStackTest
{
    [ClassInitialize]
    public static void Prepare(TestContext testContext)
    {

    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.WriteLine("[TEST_CLEANUP]\r\n\r\n");
    }
}
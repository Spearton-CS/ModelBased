using System.Diagnostics;

namespace MainTests;

[TestClass, DoNotParallelize]
public class PoolTest
{
    [ClassInitialize]
    public static void Prepare()
    {

    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.WriteLine("[TEST_CLEANUP]\r\n\r\n");
    }
}
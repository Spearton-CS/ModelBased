using System.Diagnostics;

namespace MainTests;

[TestClass, DoNotParallelize]
public class PoolTest
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

    [TestMethod]
    public void _01RentRefUnrefCheckReturn()
    {

    }
    [TestMethod]
    public async Task _02RentRefUnrefCheckReturnAsync()
    {

    }

    [TestMethod]
    public void _03FillShadowAndClear()
    {

    }
    [TestMethod]
    public async Task _04FillShadowAndClearAsync()
    {

    }

    [TestMethod]
    public void _05Modify()
    {

    }
    [TestMethod]
    public async Task _06ModifyAsync()
    {

    }

    [TestMethod]
    public void _01Enum()
    {

    }
    [TestMethod]
    public async Task _02EnumAsync()
    {

    }
}
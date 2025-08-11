using ModelBased.Collections.Generic;
using System.Diagnostics;

namespace MainTests;

[TestClass, DoNotParallelize]
public class ActiveStackTest
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static int maxId;
    private static PoolActiveStack<TestDataModel, int> stack;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static int i = 1;

    [ClassInitialize]
    public static void Prepare(TestContext testContext)
    {
        int count = Random.Shared.Next(41, 1002);
        stack = new(Math.Max(count / 100, 5));
        maxId = count - 1;

        Debug.WriteLine($"COUNT={count}; ID_PER_ITEM={stack.ItemCapacity}");
        Debug.WriteLine("PREPARE_END\r\n\r\n");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.WriteLine("[TEST_CLEANUP]\r\n\r\n");
    }


}
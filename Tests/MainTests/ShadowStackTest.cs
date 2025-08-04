using ModelBased.Collections.Generic;
using System.Diagnostics;

namespace MainTests
{
    [TestClass]
    [DoNotParallelize]
    public sealed class ShadowStackTest
    {
        private static long[] ids;
        private static PoolShadowStack<TestDataModel, long> stack;

        [ClassInitialize]
        public static void Prepare(TestContext testContext)
        {
            int capacity = Random.Shared.Next(20, 200);
            stack = new(capacity);
            ids = new long[capacity];
            for (int i = 0; i < capacity; i++)
                Debug.WriteLine((ids[i] = (long)int.MaxValue + i));

            Debug.WriteLine($"Capacity: {capacity}");
            Debug.WriteLine("PREPARE_END\r\n\r\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("[TEST_CLEANUP]\r\n\r\n");
        }

        [TestMethod()]
        public void TestMethod1()
        {
            Debug.WriteLine("ABC");
            stack.Push(TestDataModel.Factory(ids[0]));
        }
        [TestMethod()]
        public void TestMethod2()
        {
            Debug.WriteLine("CBA");
            Debug.WriteLine(stack.Pop(ids[0]));
        }
    }
}
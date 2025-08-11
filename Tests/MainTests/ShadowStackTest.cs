using ModelBased.Collections.Generic;
using System.Diagnostics;

namespace MainTests
{
    [TestClass, DoNotParallelize]
    public sealed class ShadowStackTest
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static int maxId;
        private static PoolShadowStack<TestDataModel, int> stack;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static int i = 1;

        [ClassInitialize]
        public static void Prepare(TestContext testContext)
        {
            int capacity = Random.Shared.Next(20, 200);
            stack = new(capacity);
            maxId = capacity - 1;

            Debug.WriteLine($"Capacity: {capacity}");
            Debug.WriteLine("PREPARE_END\r\n\r\n");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Debug.WriteLine("[TEST_CLEANUP]\r\n\r\n");
        }

        private static IEnumerable<TestDataModel> GenerateEnumerator(int many, bool sync = true)
        {
            int j = many;
            for (; j > 0; i++, --j)
                yield return new()
                {
                    ID = i,
                    Data = $"ADD_{(sync ? "SYNC" : "ASYNC")}_MANY:{i}"
                };
        }

        private static IEnumerable<int> EnumIds1(int many)
        {
            for (int i = 1; i <= many; i++)
                yield return i;
        }

        private static IEnumerable<int> EnumIds2(int many, int initialI)
        {
            for (int i = 0; i < many; i++)
                yield return initialI + i;
        }

        [TestMethod(), Priority(0)]
        public void _01Add_ContainsSync()
        {
            Debug.WriteLine("Add 1 by sync:");
            stack.Push(new()
            {
                ID = 0,
                Data = "ADD_SYNC_0"
            });
            Debug.WriteLine(stack.Contains(0));

            int many = (maxId - 1) / 2;
            Debug.WriteLine($"Add {many} by sync:");
            stack.PushMany(GenerateEnumerator(many));

            bool allTrue = true;
            foreach (bool contains in stack.ContainsMany(EnumIds1(many)))
                if (!contains)
                {
                    Debug.WriteLine(contains);
                    allTrue = false;
                }
            Debug.WriteLineIf(allTrue, "All contains.");
        }
        [TestMethod(), Priority(-1)]
        public async Task _02Add_ContainsAsync()
        {
            Debug.WriteLine("Add 1 by async:");
            await stack.PushAsync(new()
            {
                ID = ++i,
                Data = "ADD_ASYNC_0"
            });
            Debug.WriteLine(stack.Contains(i));

            int many = maxId - i, initialI = i;
            Debug.WriteLine($"Add {many} by async:");
            stack.PushMany(GenerateEnumerator(many, false));

            bool allTrue = true;
            await foreach (bool contains in stack.ContainsManyAsync(EnumIds2(many, initialI)))
                if (!contains)
                {
                    Debug.WriteLine(contains);
                    allTrue = false;
                }
            Debug.WriteLineIf(allTrue, "All contains.");
        }

        [TestMethod(), Priority(-2)]
        public void _03RemoveSync()
        {
            Debug.WriteLine("Remove 1 by sync:");
            TestDataModel? firstModel = stack.Pop(0);
            if (firstModel is not null && firstModel.ID == 0 && firstModel.Data == "ADD_SYNC_0")
                Debug.WriteLine("Equal");
            else
                Debug.WriteLine($"Non-equal: {firstModel}");
            i = 1;

            int many = (maxId - 1) / 2;
            bool allEqual = true;
            Debug.WriteLine($"Remove {many} by sync:");
            foreach (var model in stack.PopMany(EnumIds1(many)))
            {
                if (model is null || model.ID != i || model.Data != $"ADD_SYNC_MANY:{i}")
                {
                    allEqual = false;
                    Debug.WriteLine($"Non-equal [{i}]: {model}");
                }
                i++;
            }
            Debug.WriteLineIf(allEqual, "All equal");
        }

        [TestMethod(), Priority(-3)]
        public async Task _04RemoveAsync()
        {
            Debug.WriteLine("Remove 1 by async:");
            TestDataModel? firstModel = await stack.PopAsync(++i);
            if (firstModel is not null && firstModel.ID == i && firstModel.Data == "ADD_ASYNC_0")
                Debug.WriteLine("Equal");
            else
                Debug.WriteLine($"Non-equal: {firstModel}");
            i++;

            int many = maxId - i;
            bool allEqual = true;
            Debug.WriteLine($"Remove {many} by async:");
            await foreach (var model in stack.PopManyAsync(EnumIds2(many, i)))
            {
                if (model is null || model.ID != i || model.Data != $"ADD_ASYNC_MANY:{i}")
                {
                    allEqual = false;
                    Debug.WriteLine($"Non-equal [{i}]: {model}");
                }
                i++;
            }
            Debug.WriteLineIf(allEqual, "All equal");
        }

        [TestMethod(), Priority(-4)]
        public void _05OverflowSync()
        {
            Debug.WriteLine("Full by sync");
            for (int i = 0; i < maxId; i++)
                stack.Push(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check all by sync:");
            bool allContains = true;
            for (int i = 0; i < maxId; i++)
                if (!stack.Contains(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");

            Debug.WriteLine("Overflow by sync:");
            int max = maxId * 2;
            for (int i = maxId; i < max; i++)
                stack.Push(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check on first by sync:");
            allContains = true; //Use this variable, just remember that's 'allReplaced'
            for (int i = 0; i < maxId; i++)
                if (stack.Contains(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Contains first: {i}");
                }
            Debug.WriteLineIf(allContains, "All replaced");

            Debug.WriteLine("Check overflow by sync:");
            allContains = true;
            for (int i = maxId; i < max; i++)
                if (!stack.Contains(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");
        }

        [TestMethod(), Priority(-5)]
        public async Task _06OverflowAsync()
        {
            Debug.WriteLine("Full by async");
            for (int i = 0; i < maxId; i++)
                await stack.PushAsync(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check all by async:");
            bool allContains = true;
            for (int i = 0; i < maxId; i++)
                if (!await stack.ContainsAsync(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");

            Debug.WriteLine("Overflow by async:");
            int max = maxId * 2;
            for (int i = maxId; i < max; i++)
                await stack.PushAsync(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check on first by async:");
            allContains = true; //Use this variable, just remember that's 'allReplaced'
            for (int i = 0; i < maxId; i++)
                if (await stack.ContainsAsync(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Contains first: {i}");
                }
            Debug.WriteLineIf(allContains, "All replaced");

            Debug.WriteLine("Check overflow by async:");
            allContains = true;
            for (int i = maxId; i < max; i++)
                if (!await stack.ContainsAsync(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");
        }

        [TestMethod(), Priority(-6)]
        public void _07ClearSync()
        {
            Debug.WriteLine("Full by sync");
            for (int i = 0; i < maxId; i++)
                stack.Push(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check all by sync:");
            bool allContains = true;
            for (int i = 0; i < maxId; i++)
                if (!stack.Contains(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");

            Debug.WriteLine($"Clear by sync: {stack.Clear()}");

            Debug.WriteLine("Check on cleaned by async:");
            allContains = true; //Use this variable, just remember that's 'allCleaned'
            for (int i = 0; i < maxId; i++)
                if (stack.Contains(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Contains cleaned: {i}");
                }
            Debug.WriteLineIf(allContains, "All cleaned");
        }

        [TestMethod(), Priority(-7)]
        public async Task _08ClearAsync()
        {
            Debug.WriteLine("Full by sync");
            for (int i = 0; i < maxId; i++)
                await stack.PushAsync(new()
                {
                    ID = i,
                    Data = $"FULL_SYNC:{i}"
                });

            Debug.WriteLine("Check all by sync:");
            bool allContains = true;
            for (int i = 0; i < maxId; i++)
                if (!await stack.ContainsAsync(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Don't contains: {i}");
                }
            Debug.WriteLineIf(allContains, "All contains");

            Debug.WriteLine($"Clear by sync: {await stack.ClearAsync()}");

            Debug.WriteLine("Check on cleaned by async:");
            allContains = true; //Use this variable, just remember that's 'allCleaned'
            for (int i = 0; i < maxId; i++)
                if (await stack.ContainsAsync(i))
                {
                    allContains = false;
                    Debug.WriteLine($"Contains cleaned: {i}");
                }
            Debug.WriteLineIf(allContains, "All cleaned");
        }
    }
}
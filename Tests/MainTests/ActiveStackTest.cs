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
    private static bool fault = false;

    [ClassInitialize]
    public static void Prepare(TestContext testContext)
    {
        int count = Random.Shared.Next(41, 1002);
        stack = new(Math.Max(count / 20, 5));
        maxId = count - 1;

        Debug.WriteLine($"COUNT={count}; ID_PER_ITEM={stack.ItemCapacity}");
        Debug.WriteLine("PREPARE_END\r\n\r\n");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (fault)
            try
            {
                Debug.WriteLine("\r\n[TEST_FAULT]\r\n\r\n");
                stack.RemoveManyIgnore(EnumIds(maxId));
            }
            finally
            {
                fault = false;
            }
        else
            Debug.WriteLine("\r\n[TEST_CLEANUP]\r\n\r\n");
    }

    private static IEnumerable<int> EnumIds(int many, int i = 0)
    {
        for (; i < many; i++)
            yield return i;
    }
    private static IEnumerable<TestDataModel> EnumModels(int many, int i = 0)
    {
        for (; i < many; i++)
            yield return new()
            {
                ID = i,
                Data = $"MODEL_{i}"
            };
    }

    [TestMethod]
    public void _01Add_Contains_RemoveSync()
    {
        int MAX = stack.ItemCapacity * 2;
        stack.AddManyIgnore(EnumModels(MAX));
        int i = 0;
        bool mismatch = false;
        foreach (var model in stack)
        {
            if (model.ID != i)
            {
                Debug.WriteLine($"Mismatch: {i}, but {model.ID}");
                mismatch = true;
            }
            i++;
        }
        if (!mismatch)
            Debug.WriteLine("Enumeration equal to added ids");

        mismatch = false;
        i = 0;
        foreach (bool contains in stack.ContainsMany(EnumIds(MAX)))
        {
            if (!contains)
            {
                Debug.WriteLine($"Don't contains {i}");
                mismatch = true;
            }
            i++;
        }

        if (!mismatch)
            Debug.WriteLine("Containing all added ids");

        stack.RemoveManyIgnore(EnumIds(MAX));

        if (stack.Count == 0)
            Debug.WriteLine("All removed");
        else
            Debug.WriteLine($"Left {stack.Count} items");
    }
    [TestMethod]
    public async Task _02Add_Contains_RemoveAsync()
    {
        int MAX = stack.ItemCapacity * 2;
        await stack.AddManyIgnoreAsync(EnumModels(MAX));
        int i = 0;
        bool mismatch = false;
        await foreach (var model in stack)
        {
            if (model.ID != i)
            {
                Debug.WriteLine($"Mismatch: {i}, but {model.ID}");
                mismatch = true;
            }
            i++;
        }
        if (!mismatch)
            Debug.WriteLine("Enumeration equal to added ids");

        mismatch = false;
        i = 0;
        await foreach (bool contains in stack.ContainsManyAsync(EnumIds(MAX)))
        {
            if (!contains)
            {
                Debug.WriteLine($"Don't contains {i}");
                mismatch = true;
            }
            i++;
        }

        if (!mismatch)
            Debug.WriteLine("Containing all added ids");

        await stack.RemoveManyIgnoreAsync(EnumIds(MAX));

        if (stack.Count == 0)
            Debug.WriteLine("All removed");
        else
            Debug.WriteLine($"Left {stack.Count} items");
    }

    [TestMethod]
    public void _03DefragmentationSync()
    {
        try
        {
            stack.AddManyIgnore(EnumModels(maxId));
            int cap = stack.ItemCapacity;
            Debug.WriteLine(cap);

            foreach (var (Success, Model) in stack.RemoveMany(EnumIds((cap * 2) + 1, cap)))
                if (!Success)
                {
                    if (Model is null)
                        Debug.WriteLine($"Something went wrong... Model is not found!");
                    else
                    {
                        Debug.WriteLine($"Fault! Model is not removed: {Model.Data ?? "no data"} [{Model.ID}]");
                        throw new IOException($"MODEL {Model.ID} IS NOT REMOVED");
                    }
                }
            Debug.WriteLine($"Successfully removed {cap} items in second block (simplest defragmentation)");

            int defragmented = stack.Defragmentation();
            Debug.WriteLine($"Success! Defragmented. Check in debug point [{defragmented}]");

            stack.RemoveManyIgnore(EnumIds(maxId));
            Debug.WriteLine("Stack was cleaned.");
        }
        catch
        {
            fault = true;
            throw;
        }
    }
    [TestMethod]
    public async Task _04DefragmentationAsync()
    {
        try
        {
            await stack.AddManyIgnoreAsync(EnumModels(maxId));
            int cap = stack.ItemCapacity;
            Debug.WriteLine(cap);

            await foreach (var (Success, Model) in stack.RemoveManyAsync(EnumIds((cap * 2) + 1, cap)))
                if (!Success)
                {
                    if (Model is null)
                        Debug.WriteLine($"Something went wrong... Model is not found!");
                    else
                    {
                        Debug.WriteLine($"Fault! Model is not removed: {Model.Data ?? "no data"} [{Model.ID}]");
                        throw new IOException($"MODEL {Model.ID} IS NOT REMOVED");
                    }
                }
            Debug.WriteLine($"Successfully removed {cap} items in second block (simplest defragmentation)");

            int defragmented = await stack.DefragmentationAsync();
            Debug.WriteLine($"Success! Defragmented. Check in debug point [{defragmented}]");

            await stack.RemoveManyIgnoreAsync(EnumIds(maxId));
            Debug.WriteLine("Stack was cleaned.");
        }
        catch
        {
            fault = true;
            throw;
        }
    }

    [TestMethod]
    public void _05ClearSync()
    {
        try
        {
            stack.AddManyIgnore(EnumModels(maxId));
            int cap = stack.ItemCapacity;
            Debug.WriteLine(cap);

            foreach (var (Success, Model) in stack.RemoveMany(EnumIds((cap * 2) + 1, cap - 1)))
                if (!Success)
                {
                    if (Model is null)
                        Debug.WriteLine($"Something went wrong... Model is not found!");
                    else
                    {
                        Debug.WriteLine($"Fault! Model is not removed: {Model.Data ?? "no data"} [{Model.ID}]");
                        throw new IOException($"MODEL {Model.ID} IS NOT REMOVED");
                    }
                }
            Debug.WriteLine($"Successfully removed {cap} items in second block (simplest clear)");

            int cleaned = stack.ClearEmpty();
            Debug.WriteLine($"Success! Cleaned. Check in debug point [{cleaned}, capacity: {cap}]");

            stack.RemoveManyIgnore(EnumIds(maxId));
            Debug.WriteLine("Stack was cleaned.");
        }
        catch
        {
            fault = true;
            throw;
        }
    }
    [TestMethod]
    public async Task _06ClearAsync()
    {
        try
        {
            await stack.AddManyIgnoreAsync(EnumModels(maxId));
            int cap = stack.ItemCapacity;
            Debug.WriteLine(cap);

            await foreach (var (Success, Model) in stack.RemoveManyAsync(EnumIds((cap * 2) + 1, cap - 1)))
                if (!Success)
                {
                    if (Model is null)
                        Debug.WriteLine($"Something went wrong... Model is not found!");
                    else
                    {
                        Debug.WriteLine($"Fault! Model is not removed: {Model.Data ?? "no data"} [{Model.ID}]");
                        throw new IOException($"MODEL {Model.ID} IS NOT REMOVED");
                    }
                }
            Debug.WriteLine($"Successfully removed {cap} items in second block (simplest clear)");

            int cleaned = await stack.ClearEmptyAsync();
            Debug.WriteLine($"Success! Cleaned. Check in debug point [{cleaned}, capacity: {cap}]");

            await stack.RemoveManyIgnoreAsync(EnumIds(maxId));
            Debug.WriteLine("Stack was cleaned.");
        }
        catch
        {
            fault = true;
            throw;
        }
    }

    [TestMethod]
    public void _07RefUnref_CheckSync()
    {
        stack.AddManyIgnore(EnumModels(5));
        stack.TryRefManyIgnore(EnumIds(5));

        bool mismatch = false;
        foreach (var (Refs, Model) in stack.TryRefMany(EnumIds(5)))
            if (Refs != 3)
            {
                Debug.WriteLine($"Refs not equal to 3: {Refs} [{Model?.ID}]");
                mismatch = true;
            }
        if (!mismatch)
            Debug.WriteLine("All refs equal to 3");

        mismatch = false;
        foreach (var (Refs, Model) in stack.TryUnrefMany(EnumIds(5)))
            if (Refs != 2)
            {
                Debug.WriteLine($"Refs not equal to 2: {Refs} [{Model?.ID}]");
                mismatch = true;
            }
        if (!mismatch)
            Debug.WriteLine("All refs equal to 2");

        stack.RemoveManyIgnore(EnumIds(5));
        Debug.WriteLine("Stack was cleaned");
    }
    [TestMethod]
    public async Task _08RefUnref_CheckAsync()
    {
        await stack.AddManyIgnoreAsync(EnumModels(5));
        await stack.TryRefManyIgnoreAsync(EnumIds(5));

        bool mismatch = false;
        await foreach (var (Refs, Model) in stack.TryRefManyAsync(EnumIds(5)))
            if (Refs != 3)
            {
                Debug.WriteLine($"Refs not equal to 3: {Refs} [{Model?.ID}]");
                mismatch = true;
            }
        if (!mismatch)
            Debug.WriteLine("All refs equal to 3");

        mismatch = false;
        await foreach (var (Refs, Model) in stack.TryUnrefManyAsync(EnumIds(5)))
            if (Refs != 2)
            {
                Debug.WriteLine($"Refs not equal to 2: {Refs} [{Model?.ID}]");
                mismatch = true;
            }
        if (!mismatch)
            Debug.WriteLine("All refs equal to 2");

        await stack.RemoveManyIgnoreAsync(EnumIds(5));
        Debug.WriteLine("Stack was cleaned");
    }
}
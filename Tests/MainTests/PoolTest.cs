using ModelBased.Collections.Generic;
using System.Diagnostics;

namespace MainTests;

[TestClass, DoNotParallelize]
public class PoolTest
{
    private static ModelPool<TestDataModel, int> pool;

    [ClassInitialize]
    public static void Prepare(TestContext testContext)
    {
        pool = new(Random.Shared.Next(1, 6) * 20);
        Debug.WriteLine($"Init END. {pool.ShadowCapacity} - shadow cap");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Debug.WriteLine("\r\n[TEST_CLEANUP]\r\n\r\n");
    }

    [TestMethod]
    public void _01RentRefUnrefCheckReturn()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = pool.Rent(i).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");

        Debug.WriteLine("Referencing");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = pool.TryRef(models[i])) != 2)
                Debug.WriteLine($"{i}: refs not equal to 2. {refs}");
        }

        Debug.WriteLine("Returning (Un-Referencing)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = pool.TryReturn(models[i])) != 1)
                Debug.WriteLine($"{i}: refs not equal to 1. {refs}");
        }

        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = pool.TryReturn(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");
    }
    [TestMethod]
    public async Task _02RentRefUnrefCheckReturnAsync()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = (await pool.RentAsync(i)).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");

        Debug.WriteLine("Referencing");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = await pool.TryRefAsync(models[i])) != 2)
                Debug.WriteLine($"{i}: refs not equal to 2. {refs}");
        }

        Debug.WriteLine("Returning (Un-Referencing)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = await pool.TryReturnAsync(models[i])) != 1)
                Debug.WriteLine($"{i}: refs not equal to 1. {refs}");
        }

        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = await pool.TryReturnAsync(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");
    }

    [TestMethod]
    public void _03FillShadowAndClear()
    {
        TestDataModel[] models = new TestDataModel[pool.ShadowCapacity];
        Debug.WriteLine($"Adding ({models.Length})");
        for (int i = 0; i < models.Length; i++)
            models[i] = pool.Rent(i).Model;

        if (pool.Count != models.Length)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add {models.Length}");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");

        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < models.Length; i++)
        {
            int refs;
            if ((refs = pool.TryReturn(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} returned objects unvisible in Count");

        if (pool.ShadowCount != models.Length)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} returned objects visible in ShadowCount");

        Debug.WriteLine("Cleaning");
        int clean;
        if ((clean = pool.ClearShadow()) != models.Length)
            Debug.WriteLine($"INVALID CLEAR COUNT. {clean}, but we have {models.Length}");
        else
            Debug.WriteLine($"All fine, clear count equal to {models.Length}");

        if (pool.ShadowCount != 0)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} cleaned objects unvisible in ShadowCount");
    }
    [TestMethod]
    public async Task _04FillShadowAndClearAsync()
    {
        TestDataModel[] models = new TestDataModel[pool.ShadowCapacity];
        Debug.WriteLine($"Adding ({models.Length})");
        for (int i = 0; i < models.Length; i++)
            models[i] = (await pool.RentAsync(i)).Model;

        if (pool.Count != models.Length)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add {models.Length}");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");

        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < models.Length; i++)
        {
            int refs;
            if ((refs = await pool.TryReturnAsync(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} returned objects unvisible in Count");

        if (pool.ShadowCount != models.Length)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} returned objects visible in ShadowCount");

        Debug.WriteLine("Cleaning");
        int clean;
        if ((clean = await pool.ClearShadowAsync()) != models.Length)
            Debug.WriteLine($"INVALID CLEAR COUNT. {clean}, but we have {models.Length}");
        else
            Debug.WriteLine($"All fine, clear count equal to {models.Length}");

        if (pool.ShadowCount != 0)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed {models.Length}");
        else
            Debug.WriteLine($"All fine, all {models.Length} cleaned objects unvisible in ShadowCount");
    }

    [TestMethod]
    public void _05Modify()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = pool.Rent(i).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");


        Debug.WriteLine("Modify");
        TestDataModel model = new();
        for (int i = 0; i < 15; i++)
        {
            model.ID = i;
            model.Data = $"MODEL_{i}_MOD";

            if (pool.TryModify(models[i], model))
                Debug.WriteLine($"{i} modified: {models[i].Data}");
            else
                Debug.WriteLine($"{i} MODIFY UNSUCCESSFULL: {models[i].Data}");
        }


        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = pool.TryReturn(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");

    }
    [TestMethod]
    public async Task _06ModifyAsync()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = (await pool.RentAsync(i)).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");


        Debug.WriteLine("Modify");
        TestDataModel model = new();
        for (int i = 0; i < 15; i++)
        {
            model.ID = i;
            model.Data = $"MODEL_{i}_MOD";

            if (await pool.TryModifyAsync(models[i], model))
                Debug.WriteLine($"{i} modified: {models[i].Data}");
            else
                Debug.WriteLine($"{i} MODIFY UNSUCCESSFULL: {models[i].Data}");
        }


        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = await pool.TryReturnAsync(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");
    }

    [TestMethod]
    public void _07Enum()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = pool.Rent(i).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");


        bool[] found = new bool[15];
        var en1 = pool.EnumerateIDs();
        while (en1.MoveNext())
            if (en1.Current < 15 && en1.Current >= 0)
                found[en1.Current] = true;
            else
                Debug.WriteLine($"Invalid ID: {en1.Current}");

        if (found.All((x) => x))
            Debug.WriteLine("All IDs found");
        else
            Debug.WriteLine($"Not all IDs found: {found.Count((x) => x)}");

        for (int i = 0; i < 15; i++)
            found[i] = false;


        var en2 = pool.GetEnumerator();
        while (en2.MoveNext())
            if (en2.Current.ID < 15 && en2.Current.ID >= 0)
                found[en2.Current.ID] = true;
            else
                Debug.WriteLine($"Invalid ID: {en2.Current.ID} ({en2.Current.Data})");

        if (found.All((x) => x))
            Debug.WriteLine("All Models found");
        else
            Debug.WriteLine($"Not all Models found: {found.Count((x) => x)}");


        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = pool.TryReturn(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");
    }
    [TestMethod]
    public async Task _08EnumAsync()
    {
        TestDataModel[] models = new TestDataModel[15];
        Debug.WriteLine("Adding (15)");
        for (int i = 0; i < 15; i++)
        {
            models[i] = (await pool.RentAsync(i)).Model;
            models[i].Data = $"MODEL_{i}";
        }

        if (pool.Count != 15)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we add 15");
        else
            Debug.WriteLine("All fine, all 15 rented objects visible in Count");


        bool[] found = new bool[15];
        var en1 = pool.EnumerateIDsAsync();
        while (await en1.MoveNextAsync())
            if (en1.Current < 15 && en1.Current >= 0)
                found[en1.Current] = true;
            else
                Debug.WriteLine($"Invalid ID: {en1.Current}");

        if (found.All((x) => x))
            Debug.WriteLine("All IDs found");
        else
            Debug.WriteLine($"Not all IDs found: {found.Count((x) => x)}");

        for (int i = 0; i < 15; i++)
            found[i] = false;


        var en2 = pool.GetAsyncEnumerator();
        while (await en2.MoveNextAsync())
            if (en2.Current.ID < 15 && en2.Current.ID >= 0)
                found[en2.Current.ID] = true;
            else
                Debug.WriteLine($"Invalid ID: {en2.Current.ID} ({en2.Current.Data})");

        if (found.All((x) => x))
            Debug.WriteLine("All Models found");
        else
            Debug.WriteLine($"Not all Models found: {found.Count((x) => x)}");


        Debug.WriteLine("Returning (finally)");
        for (int i = 0; i < 15; i++)
        {
            int refs;
            if ((refs = await pool.TryReturnAsync(models[i])) != 0)
                Debug.WriteLine($"{i}: refs not equal to 0. {refs}");
        }

        if (pool.Count != 0)
            Debug.WriteLine($"INVALID COUNT. {pool.Count}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects unvisible in Count");

        if (pool.ShadowCount != 15)
            Debug.WriteLine($"INVALID SHADOW COUNT. {pool.ShadowCount}, but we removed 15");
        else
            Debug.WriteLine("All fine, all 15 returned objects visible in ShadowCount");
    }
}
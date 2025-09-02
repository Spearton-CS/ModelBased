using ModelBased.ComponentModel;

B<A, int>.Do();

static class B<TModel, TID>
    where TModel : IDataModel<TModel, TID>
{
    public static void Do()
    {
        Type i = typeof(IReusableModel<,>);
        foreach (var item in typeof(A).GetInterfaces())
        {
            Console.WriteLine(item);
            if (item.IsGenericType)
            {
                if (item.GetGenericTypeDefinition() == i)
                {
                    Console.WriteLine(true);
                    typeof(A).GetMethod("Factory", System.Reflection.BindingFlags.Static, [typeof(int), typeof(A), typeof(CancellationToken)]).Invoke(null, [1, null, CancellationToken.None]);
                }
                else
                    Console.WriteLine(false);
            }
            else
                Console.WriteLine(false);
        }
        Console.ReadKey();
    }
}

class A : IReusableModel<A, int>
{
    public static bool SupportsAsyncFactory => throw new NotImplementedException();

    public int ID => throw new NotImplementedException();

    public static A Factory(int id, A? reuse, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public static A Factory(int id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public static Task<A> FactoryAsync(int id, A? reuse, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public static Task<A> FactoryAsync(int id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public bool EqualsByID(int id)
    {
        throw new NotImplementedException();
    }
}
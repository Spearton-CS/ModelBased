using ModelBased.Collections.Generic;
using ModelBased.ComponentModel;

PoolActiveStack<TestModel, int> pas = new();
for (int i = 0; i < 100; i++)
    pas.Add(new() { ID = i });
int j = 0;
foreach (TestModel model in pas)
{
    Console.Write(model.ID);
    if (++j == 3)
    {
        j = 0;
        Console.WriteLine();
    }
    else
        Console.Write('\t');
}
Console.ReadKey();


class TestModel : IDataModel<TestModel, int>
{
    public int ID { get; set; }

    public static TestModel Factory(int id) => new()
    {
        ID = id
    };

    public bool EqualsByID(int id) => ID == id;
}
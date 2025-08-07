(int Refs, int Model)[] array = new (int, int)[5];
for (int i = 0; i < 5; i++)
    array[i] = (i, i * 5);
for (int i = 0; i < 5; i++)
    Console.WriteLine($"{++array[i].Refs}: {array[i].Refs}");
Console.ReadKey();
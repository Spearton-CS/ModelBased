using ModelBased.ComponentModel;

namespace MainTests
{
    public class TestDataModel : IDataModel<TestDataModel, int>
    {
        public int ID { get; set; }
        public string? Data { get; set; }

        public static TestDataModel Factory(int id) => new()
        {
            ID = id
        };

        public bool EqualsByID(int id) => ID == id;

        public override string ToString() => $"[{ID}]: {Data ?? "null"}";
    }
}
using ModelBased.ComponentModel;

namespace MainTests
{
    public class TestDataModel : IDataModel<TestDataModel, long>
    {
        public long ID { get; set; }
        public string? Data { get; set; }

        public static TestDataModel Factory(long id) => new()
        {
            ID = id
        };

        public bool EqualsByID(long id) => ID == id;

        public override string ToString() => $"[{ID}]: {Data ?? "null"}";
    }
}
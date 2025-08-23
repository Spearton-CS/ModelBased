using ModelBased.ComponentModel;

namespace MainTests
{
    public class TestDataModel : IUpdateableModel<TestDataModel, int>,
        IAsyncUpdateableModel<TestDataModel, int>
    {
        public static bool SupportsAsyncFactory => true;

        public int ID { get; set; }
        public string? Data { get; set; }

        public static TestDataModel Factory(int id) => new()
        {
            ID = id
        };

        public static TestDataModel Factory(int id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return new()
            {
                ID = id
            };
        }

        public static Task<TestDataModel> FactoryAsync(int id, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult<TestDataModel>(new()
            {
                ID = id
            });
        }

        public bool EqualsByID(int id) => ID == id;

        public override string ToString() => $"[{ID}]: {Data ?? "null"}";

        public void Update(TestDataModel other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ID != other.ID)
                throw new InvalidOperationException("ID mismatch");
            Data = other.Data;
        }

        public void Update(IDataModel<int> other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ID != other.ID)
                throw new InvalidOperationException("ID mismatch");
            else if (other is TestDataModel model)
                Data = model.Data;
            else
                throw new InvalidOperationException("Is not TestDataModel");
        }

        public Task UpdateAsync(TestDataModel other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ID != other.ID)
                throw new InvalidOperationException("ID mismatch");
            Data = other.Data;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(IDataModel<int> other, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (ID != other.ID)
                throw new InvalidOperationException("ID mismatch");
            else if (other is TestDataModel model)
            {
                Data = model.Data;
                return Task.CompletedTask;
            }
            else
                throw new InvalidOperationException("Is not TestDataModel");
        }
    }
}
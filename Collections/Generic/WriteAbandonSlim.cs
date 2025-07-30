namespace ModelBased.Collections.Generic
{
    public class WriteAbandonSlim : IDisposable
    {
        protected SemaphoreSlim semaphore = new(1, 1);
        protected volatile bool abandon = true;

        /// <summary>
        /// Returns true, if write is currently abandoned
        /// </summary>
        public virtual bool IsWriteAbandoned => abandon;

        public virtual void AbandonWrite()
        {

        }
        public virtual async Task AbandonWriteAsync()
        {

        }

        public virtual void AllowWrite()
        {

        }

        public bool Disposed { get; protected set; } = false;
        public void Dispose()
        {
            if (Disposed)
                return;
            Dispose(true);
            Disposed = true;
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            semaphore.Dispose();
        }
    }
}
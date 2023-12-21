using System;
using System.Threading;

namespace Synchronization
{
    public static class ReaderWriterLockSlimExt
    {
        public static ReaderLockSlimWrapper TakeReaderLock(TimeSpan timeout)
        {
            var rwlock = new ReaderWriterLockSlim();

            bool taken = false;
            try
            {
                taken = rwlock.TryEnterReadLock(timeout);
                if (taken)
                    return new ReaderLockSlimWrapper(rwlock);
                throw new TimeoutException("Failed to acquire a ReaderWriterLockSlim in time.");
            }
            catch
            {
                if (taken)
                    rwlock.ExitReadLock();
                throw;
            }
        }

        public struct ReaderLockSlimWrapper : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwlock;

            public ReaderLockSlimWrapper(ReaderWriterLockSlim rwlock)
            {
                _rwlock = rwlock;
            }
            public void Dispose()
            {
                _rwlock.ExitReadLock();
            }
        }

        public static WriterLockSlimWrapper TakeWriterLock(TimeSpan timeout)
        {
            var rwlock = new ReaderWriterLockSlim();

            bool taken = false;
            try
            {
                taken = rwlock.TryEnterWriteLock(timeout);
                if (taken)
                    return new WriterLockSlimWrapper(rwlock);
                throw new TimeoutException("Failed to acquire a ReaderWriterLockSlim in time.");
            }
            catch
            {
                if (taken)
                    rwlock.ExitWriteLock();
                throw;
            }
        }

        public struct WriterLockSlimWrapper : IDisposable
        {
            private readonly ReaderWriterLockSlim _rwlock;

            public WriterLockSlimWrapper(ReaderWriterLockSlim rwlock)
            {
                _rwlock = rwlock;
            }
            public void Dispose()
            {
                _rwlock.ExitWriteLock();
            }
        }
    }
}

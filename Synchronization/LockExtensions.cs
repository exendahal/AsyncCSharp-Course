using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization
{
    public static class LockExtensions
    {
        public static Lock Lock(this object obj, TimeSpan timeout)
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(obj, timeout, ref lockTaken);
                if (lockTaken)
                {
                    return new Lock(obj);
                }

                throw new TimeoutException("Failed to acquire sync object.");
            }
            catch
            {
                if (lockTaken)
                {
                    Monitor.Exit(obj);
                }

                throw;
            }
        }
    }

    public struct Lock:IDisposable
    {
        private readonly object _obj;

        public Lock(object obj)
        {
            _obj = obj;
        }
        public void Dispose()
        {
            Monitor.Exit(_obj);
        }
    }
}

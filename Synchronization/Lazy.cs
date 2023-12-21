using System.Threading;

namespace Synchronization
{
    public static class Lazy<T> where T : class, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // if current is null, we need to create new instance
                if (_instance == null)
                {
                    // attempt create, it will only set if previous was null
                    Interlocked.CompareExchange(ref _instance, new T(), (T)null);
                }

                return _instance;
            }
        }
    }
}
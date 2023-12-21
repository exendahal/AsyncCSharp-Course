using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization
{
    public class NightClub
    {
        public static SemaphoreSlim Bouncer { get; set; }

        static void Run(string[] args)
        {
            // Create the semaphore with 3 slots, where 3 are available.
            Bouncer = new SemaphoreSlim(3, 3);

            OpenNightClub();

            Thread.Sleep(20000);

            Console.Read();
        }

        private static void OpenNightClub()
        {
            {
                for (int i = 1; i <= 50; i++)
                {
                    // Let each guest enter on an own thread.
                    var number = i;
                    Task.Run(() => Guest(number));
                }
            }

        }

        private static void Guest(int guestNumber)
        {
            // Wait to enter the nightclub (a semaphore to be released).
            Console.WriteLine("Guest {0} is waiting to entering nightclub.", guestNumber);
            Bouncer.Wait();

            // Do some dancing.
            Console.WriteLine("Guest {0} is doing some dancing.", guestNumber);
            Thread.Sleep(500);

            // Let one guest out (release one semaphore).
            Console.WriteLine("Guest {0} is leaving the nightclub.", guestNumber);
            Bouncer.Release(1);

        }
    }
}

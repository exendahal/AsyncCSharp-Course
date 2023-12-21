using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;

namespace Synchronization
{
    class Program
    {
        static readonly object firstLock = new object();
        static readonly object secondLock = new object();

        static void Main(string[] args)
        {
            Task.Run((Action)Do);

            // Wait until we're fairly sure the other thread // has grabbed firstLock
            Thread.Sleep(500);
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locking secondLock");

            lock (secondLock)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locked secondLock");
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locking firstLock");

                lock (firstLock)
                {
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Locked firstLock");
                }
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Released firstLock");
            }
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}-Released secondLock");

            Console.Read();
        }

        private static void Do()
        {
            Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locking firstLock");
            lock (firstLock)
            {
                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locked firstLock");

                // Wait until we're fairly sure the first thread // has grabbed secondLock
                Thread.Sleep(1000);

                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locking secondLock");
                lock (secondLock)
                {
                    Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Locked secondLock");
                }
                Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Released secondLock");
            }
            Console.WriteLine($"\t\t\t\t{Thread.CurrentThread.ManagedThreadId}-Released firstLock");
        }
    


        private static void Swap(object obj1, object obj2)
        {
            object obj1Ref = Interlocked.Exchange(ref obj1, obj2);
            Interlocked.Exchange(ref obj2, obj1Ref);
            //object tmp = obj1;
            //obj1 = obj2;
            //obj2 = tmp;
        }

        private static void TestCharacter()
        {
            Character c = new Character();
            Character c2 = new Character();

            Swap(c, c2);

            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                Task t1 = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        c.CastArmorSpell(true);
                    }
                });
                tasks.Add(t1);

                Task t2 = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        c.CastArmorSpell(false);
                    }
                });
                tasks.Add(t2);
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine($"Resulting armor = {c.Armor}");
        }
    }
}
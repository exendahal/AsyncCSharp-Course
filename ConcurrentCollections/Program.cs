using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentCollections
{
    public class RemoteBookStock
    {
        public static readonly List<string> Books =
            new List<string> { "Clean Code", "C# in Depth", "C++ for Beginners",
                    "Design Patterns in C#", "Marvel Heroes" };


    }

    public class StockController
    {
        readonly ConcurrentDictionary<string, int> _stock = new ConcurrentDictionary<string, int>();

        public void BuyBook(string item, int quantity)
        {
            _stock.AddOrUpdate(item, quantity, (key, oldValue) => oldValue + quantity);
        }

        public bool TryRemoveBookFromStock(string item)
        {
            if (_stock.TryRemove(item, out int val))
            {
                Console.WriteLine($"How much was removed:{val}");
                return true;
            }
            return false;
        }

        public bool TrySellBook(string item)
        {
            bool success = false;

            _stock.AddOrUpdate(item,
                itemName => { success = false; return 0; },
                (itemName, oldValue) =>
                {
                    if (oldValue == 0)
                    {
                        success = false;
                        return 0;
                    }
                    else
                    {
                        success = true;
                        return oldValue - 1;
                    }
                });
            return success;
        }
        public void DisplayStatus()
        {
            foreach (var pair in _stock)
            {
                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
            }
        }

    }

    public class SalesManager
    {
        public string Name { get; }

        public SalesManager(string id)
        {
            Name = id;
        }

        public void StartWork(StockController stockController, TimeSpan workDay)
        {
            Random rand = new Random((int)DateTime.UtcNow.Ticks);
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < workDay)
            {
                Thread.Sleep(rand.Next(50));
                int generatedNumber = rand.Next(10);
                bool shouldPurchase = generatedNumber % 2 == 0;
                bool shouldRemove = generatedNumber == 9;
                string itemName = RemoteBookStock.Books[rand.Next(RemoteBookStock.Books.Count)];

                if (shouldPurchase)
                {
                    int quantity = rand.Next(9) + 1;
                    stockController.BuyBook(itemName, quantity);
                    DisplayPurchase(itemName, quantity);
                }
                else if (shouldRemove)
                {
                    stockController.TryRemoveBookFromStock(itemName);
                    DisplayRemoveAttempt(itemName);
                }
                else
                {
                    bool success = stockController.TrySellBook(itemName);
                    DisplaySaleAttempt(success, itemName);
                }
            }
            Console.WriteLine("SalesManager {0} finished its work!", Name);
        }
        private void DisplayRemoveAttempt(string itemName)
        {
            Console.WriteLine("Thread {0} {1} removed {2}", Thread.CurrentThread.ManagedThreadId, Name, itemName);
        }

        public void DisplayPurchase(string itemName, int quantity)
        {
            Console.WriteLine("Thread {0}: {1} bought {2} of {3}", Thread.CurrentThread.ManagedThreadId, Name, quantity, itemName);
        }

        public void DisplaySaleAttempt(bool success, string itemName)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(success
                ? $"Thread {threadId}: {Name} sold {itemName}"
                : $"Thread {threadId}: {Name}: Out of stock of {itemName}");
        }

    }

    public class CreditCard
    {
        public decimal Liabilities { get; set; }
        public int Id { get; set; }

        public void Block(CancellationToken ct)
        {
            bool blocked = false;
            for(int i=0; i<3; i++)
            {
                ct.ThrowIfCancellationRequested();

                //connecting to a server
                Console.WriteLine($"Connecting {Id}. Iteration:{i}");
                Thread.Sleep(1000);

                //idiotic condition
                if (i == 3)
                {
                    blocked = true;
                }
            }
            if (blocked)
                Console.WriteLine($"Blocked credit card. ID:{Id}");
        }
    }

    class Program
    {
        private static readonly List<int> largeList = new List<int>(128);

        private static void GenerateList()
        {
            for (int i = 0; i < 100000; i++)
            {
                largeList.Add(i);
            }
        }
        static void BuildImmutableCollectionDemo()
        {
            /*
            var builder = ImmutableList.CreateBuilder<int>();
            foreach (var item in largeList)
            {
                builder.Add(item);
            }
            //builder.AddRange(largeList);
            ImmutableList<int> immutableList = builder.ToImmutable();
            */
            var list = largeList.ToImmutableList();
        }

        static IEnumerable<int> RunLoop1()
        {
            for (int i = 0; i < 100; i++)
            {
                //Console.WriteLine($"ThreadID:{Thread.CurrentThread.ManagedThreadId};Iteration:{i}");
                yield return i;
            }
        }
        static IEnumerable<int> RunLoop2()
        {
            for (int i = 0; i < 100; i++)
            {
                //Console.WriteLine($"ThreadID:{Thread.CurrentThread.ManagedThreadId};Iteration:{i}");
                yield return i;
            }
        }


        static void Main(string[] args)
        {
            List<CreditCard> cards = new List<CreditCard>()
            {
                new CreditCard(){Liabilities=1200, Id=1 },
                new CreditCard(){Liabilities=80, Id=2 },
                new CreditCard(){Liabilities=1100, Id=3 },
                new CreditCard(){Liabilities=100, Id=4 },
                new CreditCard(){Liabilities=3000, Id=5 },
                new CreditCard(){Liabilities=800, Id=6 },
                new CreditCard(){Liabilities=1450, Id=7 },
            };

            var cts = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                try
                {
                    cards.AsParallel().WithCancellation(cts.Token)
                    .ForAll(x =>
                    {
                        if (x.Liabilities > 1000)
                        {
                            x.Block(cts.Token);
                        }
                    });
                }
                catch(OperationCanceledException ex)
                {
                    Console.WriteLine("Cancelled!");
                }
            });

            Thread.Sleep(1500);
            Console.WriteLine("Cancelling");
            cts.Cancel();

            Console.Read();
        }

        private static void PlinqTest()
        {
            IEnumerable<int> numbers = Enumerable.Range(3, 100000 - 3);

            var parallelQuery =
              from n in numbers.AsParallel()
              where Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0)
              select n;

            int[] primes = parallelQuery.ToArray();

            primes.ToList().AsParallel()           // Wraps sequence in ParallelQuery<int>
                    .Where(n => n > 100)   // Outputs another ParallelQuery<int>
                    .AsParallel()           // Unnecessary - and inefficient!
                    .Select(n => n * n);

            parallelQuery =
                from n in numbers.AsParallel().AsOrdered()
                where Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0)
                select n;


            var result = from site in new[]
            {
                "www.engineerspock.com",
                "www.udemy.com",
                "www.reddit.com",
                "www.facebook.com",
                "www.stackoverflow.com",
                "www.pluralsight.com"
            }
            .AsParallel().WithDegreeOfParallelism(6)
                         let p = new System.Net.NetworkInformation.Ping().Send(site)
                         select new
                         {
                             site,
                             Result = p.Status,
                             Time = p.RoundtripTime

                         };
        }

        private static void TestParallelClass()
        {
            //Parallel.Invoke(RunLoop1, RunLoop2);
            //var data = new List<int>();
            //Parallel.Invoke(() => data.AddRange(RunLoop1()),
            //() => data.AddRange(RunLoop2()));

            //ParallelOptions po = new ParallelOptions();
            //po.MaxDegreeOfParallelism = 8;

            //Parallel.For(1, 11, i => { Console.WriteLine($"{i * i * i}"); });
            string sentence =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod " +
                "tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit " +
                "anim id est laborum";

            string[] words = sentence.Split(' ');

            Parallel.ForEach(words, word =>
            {
                Console.WriteLine($"\"{word}\" is of {word.Length} length = " +
                    $"thread {Thread.CurrentThread.ManagedThreadId}");
            });
        }

        private static void TestBlockingCollection()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            ProducerConsumerDemo pcd = new ProducerConsumerDemo();
            Task.Run(() => pcd.Run(cts.Token));

            Console.Read();
            cts.Cancel();
            Console.WriteLine("End of processing");
        }

        private static void TestConcurrentDictionary()
        {
            var controller = new StockController();
            TimeSpan workDay = new TimeSpan(0, 0, 1);

            Task t1 = Task.Run(() => new SalesManager("Bob").StartWork(controller, workDay));
            Task t2 = Task.Run(() => new SalesManager("Alice").StartWork(controller, workDay));
            Task t3 = Task.Run(() => new SalesManager("Rob").StartWork(controller, workDay));

            Task.WaitAll(t1, t2, t3);

            controller.DisplayStatus();
        }

        static void ConcurrentBagDemo()
        {
            var names = new ConcurrentBag<string>();
            names.Add("Bob");
            names.Add("Alice");
            names.Add("Rob");

            Console.WriteLine($"After enqueuing, count = {names.Count}");

            string item1; //= names.Dequeue();
            bool success = names.TryTake(out item1);
            if (success)
                Console.WriteLine("\nRemoving " + item1);
            else
                Console.WriteLine("queue was empty");

            string item2; //=names.Peek();
            success = names.TryPeek(out item2);
            if (success)
                Console.WriteLine("Peeking  " + item2);
            else
                Console.WriteLine("queue was empty");
            Console.WriteLine("Enumerating");
            PrintOutCollection(names);

            Console.WriteLine("\nAfter enumerating, count = " + names.Count);
        }

        static void ConcurrentStackDemo()
        {
            var names = new ConcurrentStack<string>();
            names.Push("Bob");
            names.Push("Alice");
            names.Push("Rob");

            Console.WriteLine($"After enqueuing, count = {names.Count}");

            string item1; //= names.Dequeue();
            bool success = names.TryPop(out item1);
            if (success)
                Console.WriteLine("\nRemoving " + item1);
            else
                Console.WriteLine("queue was empty");

            string item2; //=names.Peek();
            success = names.TryPeek(out item2);
            if (success)
                Console.WriteLine("Peeking  " + item2);
            else
                Console.WriteLine("queue was empty");
            Console.WriteLine("Enumerating");
            PrintOutCollection(names);

            Console.WriteLine("\nAfter enumerating, count = " + names.Count);
        }

        static void ConcurrentQueueDemo()
        {
            var names = new ConcurrentQueue<string>();
            names.Enqueue("Bob");
            names.Enqueue("Alice");
            names.Enqueue("Rob");

            Console.WriteLine($"After enqueuing, count = {names.Count}");

            string item1; //= names.Dequeue();
            bool success = names.TryDequeue(out item1);
            if (success)
                Console.WriteLine("\nRemoving " + item1);
            else
                Console.WriteLine("queue was empty");

            string item2; //=names.Peek();
            success = names.TryPeek(out item2);
            if (success)
                Console.WriteLine("Peeking  " + item2);
            else
                Console.WriteLine("queue was empty");
            Console.WriteLine("Enumerating");
            PrintOutCollection(names);

            Console.WriteLine("\nAfter enumerating, count = " + names.Count);
        }

        static void ImmutableDictionaryDemo()
        {
            var dic = ImmutableDictionary<int, string>.Empty;
            dic = dic.Add(1, "John");
            dic = dic.Add(2, "Alex");
            dic = dic.Add(3, "April");

            // Displays "1-John", "2-Alex", "3-April" in an unpredictable order.
            IterateOverDictionary(dic);

            Console.WriteLine("Changing value of key 2 to Bob");
            dic = dic.SetItem(2, "Bob");

            IterateOverDictionary(dic);

            var april = dic[3];
            Console.WriteLine($"Who is at key 3 = {april}");

            Console.WriteLine("Remove record where key = 2");
            dic = dic.Remove(2);

            IterateOverDictionary(dic);
        }

        private static void IterateOverDictionary(ImmutableDictionary<int, string> dic)
        {
            foreach (var item in dic)
                Console.WriteLine(item.Key + "-" + item.Value);
        }

        static void SetsDemo()
        {
            var hashSet = ImmutableHashSet<int>.Empty;
            hashSet = hashSet.Add(5);
            hashSet = hashSet.Add(10);

            // Displays "5" and "10" in an unpredictable order.
            //(at least in multithreaded scenarios)
            PrintOutCollection(hashSet);

            Console.WriteLine("Remove 5");
            hashSet = hashSet.Remove(5);

            PrintOutCollection(hashSet);

            Console.WriteLine("--- ImmutableSortedSet Demo ---");

            var sortedSet = ImmutableSortedSet<int>.Empty;
            sortedSet = sortedSet.Add(10);
            sortedSet = sortedSet.Add(5);

            PrintOutCollection(sortedSet);

            var smallest = sortedSet[0];
            Console.WriteLine($"Smallest Item:{smallest}");

            Console.WriteLine("Remove 5");
            sortedSet = sortedSet.Remove(5);

            PrintOutCollection(sortedSet);
        }

        static void ListDemo()
        {
            var list = ImmutableList<int>.Empty;
            list = list.Add(2);
            list = list.Add(3);
            list = list.Add(4);
            list = list.Add(5);

            PrintOutCollection(list);

            Console.WriteLine("Remove 4 and then RemoveAt index=2");
            list = list.Remove(4);
            list = list.RemoveAt(2);

            PrintOutCollection(list);

            Console.WriteLine("Insert 1 at 0 and 4 at 3");
            list = list.Insert(0, 1);
            list = list.Insert(3, 4);

            PrintOutCollection(list);
        }

        static void QueueDemo()
        {
            var queue = ImmutableQueue<int>.Empty;
            queue = queue.Enqueue(1);
            queue = queue.Enqueue(2);

            PrintOutCollection(queue);

            int first = queue.Peek();
            Console.WriteLine($"Last item:{first}");

            queue = queue.Dequeue(out first);
            Console.WriteLine($"Last item:{first}; Last After Pop:{queue.Peek()}");
        }

        static void StackDemo()
        {
            var stack = ImmutableStack<int>.Empty;
            stack = stack.Push(1);
            stack = stack.Push(2);

            int last = stack.Peek();
            Console.WriteLine($"Last item:{last}");

            stack = stack.Pop(out last);

            Console.WriteLine($"Last item:{last}; Last after Pop:{stack.Peek()}");

        }

        private static void PrintOutCollection<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }
    }
}

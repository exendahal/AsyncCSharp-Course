using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentCollections
{
    public class ProducerConsumerDemo
    {
        private readonly BlockingCollection<string> _cutleriesToWash =
                new BlockingCollection<string>(new ConcurrentStack<string>(), 10);
        private readonly List<string> _cutleries = new List<string>()
        {
            "Fork",
            "Spoon",
            "Plate",
            "Knife"
        };
        private readonly Random _random = new Random();


        //producer
        public void Eat(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                string nextCutlery = _cutleries[_random.Next(3)];
                _cutleriesToWash.Add(nextCutlery);
                Console.WriteLine($"+ {nextCutlery}");
                Thread.Sleep(500);
            }
        }

        //consumer
        public void Wash(CancellationToken ct)
        {
            foreach (var item in _cutleriesToWash.GetConsumingEnumerable())
            {
                ct.ThrowIfCancellationRequested();
                Console.WriteLine($"- {item}");
                Thread.Sleep(3000);
            }


        }

        public void Run(CancellationToken ct)
        {
            Task t1 = Task.Run(() => Eat(ct), ct);
            Task t2 = Task.Run(() => Wash(ct), ct);

            try
            {
                Task.WaitAll(t1, t2);
            }
            catch (AggregateException ae)
            {
            }
        }
    }
 }

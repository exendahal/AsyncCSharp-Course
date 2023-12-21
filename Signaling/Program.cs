using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Signaling
{
    class Program
    {
        private static CountdownEvent _countdown = new CountdownEvent(3);

        private static readonly Barrier Barrier = new Barrier(participantCount: 0);

        static void Main(string[] args)
        {
            int totalRecords = GetNumberOfRecords();
            
            Task[] tasks = new Task[totalRecords];

            for (int i = 0; i < totalRecords; ++i)
            {
                Barrier.AddParticipant();

                int j = i;
                tasks[j] = Task.Factory.StartNew(() =>
                {
                    GetDataAndStoreData(j);
                });
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Backup completed");

            Console.Read();            
        }

        private static int GetNumberOfRecords()
        {
            return 10;
        }

        private static void GetDataAndStoreData(int index)
        {
            Console.WriteLine("Getting data from server: " + index);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Barrier.SignalAndWait();

            Console.WriteLine("Send data to Backup server: " + index);

            Barrier.SignalAndWait();
        }

        private static void TestCountdown()
        {
            Task.Run(() => { DoWork(); });
            Task.Run(() => { DoWork(); });
            Task.Run(() => { DoWork(); });

            _countdown.Wait();
            Console.WriteLine("All tasks have finished their work!");

            //won't work (return false) if countdown is already equal to zero
            //_countdown.TryAddCount(1);
        }

        private static void DoWork()
        {
            Thread.Sleep(1000);
            Console.WriteLine($"I'm a task with id:{Task.CurrentId}");
            _countdown.Signal();
        }

        private static void TestBankTerminal()
        {
            var bt = new BankTerminal.BankTerminal(new IPEndPoint(new IPAddress(0x2414188f), 8080));

            Task purchaseTask = bt.Purchase(100);
            var firstContinue = purchaseTask.ContinueWith(x => { Console.WriteLine("Operation is done!"); });
            firstContinue.ContinueWith(x =>
            {
                Console.WriteLine("-----------Another Operation-----------");
                Task secondPurchase = bt.Purchase(100);
                secondPurchase.ContinueWith(y => { Console.WriteLine("Operation is Done!"); });
            });
        }
    }
}

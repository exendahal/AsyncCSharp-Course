using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCSharp_Course
{
    public class PrintingInfo
    {
        public int ProcessedNumbers { get; set; }
    }
    class ProgramThreading
    {
        static void Test(string[] args)
        {
            var printInfo = new PrintingInfo();
            Thread t1 = new Thread(() => Print(false, printInfo));
            t1.IsBackground = true;
            t1.Priority = ThreadPriority.Highest;
            t1.Start();

            Console.Read();

            /*
            if (t1.Join(TimeSpan.FromMilliseconds(5000)))
            {
                Console.WriteLine($"Im'sure that spawned thread " +
                                  $"processed that many:{printInfo.ProcessedNumbers}");
            }
            else
            {
                Console.WriteLine("Timed out. Can't process results.");
            }
            //Print(true, printInfo);

            Console.Read();
            */
        }

        private static void Print(bool isEven, PrintingInfo printInfo)
        {
            while (true)
            {
                Thread.Sleep(1000);
            }
            /*
            if (isEven)
            {
                for (int i = 0; i < 10000; i++)
                {
                    if (i % 2 == 0)
                    {
                        printInfo.ProcessedNumbers++;
                        Console.WriteLine(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10000; i++)
                {
                    if (i % 2 != 0)
                    {
                        printInfo.ProcessedNumbers++;
                        Console.WriteLine(i);
                    }
                }
            }
            */
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AsyncCSharp_Course
{
    class Program
    {
        static void WhenAny()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Console.WriteLine("Started t1");
            Console.WriteLine("Started t2");

            var tr = Task.WhenAny(t1, t2);
            tr.ContinueWith(x => { Console.WriteLine($"The id of a task which completed first = {tr.Result.Id}"); });

            Console.WriteLine("After when any");
        }
        static void WaitAny()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Console.WriteLine("Started t1");
            Console.WriteLine("Started t2");

            int result = Task.WaitAny(t1, t2);

            Console.WriteLine($"After wait any. First finished task id={result}");
        }
        static void Wait()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);

            Console.WriteLine("Started t1");

            t1.Wait();

            Console.WriteLine("After wait");
        }
        static void ContinueWhenAll()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);
            var t2 = Task.Run<int>(() => Print(false, cts.Token), cts.Token);

            Task.Factory.ContinueWhenAll(new[] { t1, t2 }, tasks =>
            {
                var t1Task = tasks[0];
                var t2Task = tasks[1];

                Console.WriteLine($"t1Task:{t1Task.Result}, t2Task:{t2Task.Result}");
            });
        }
        static void ContinueWith()
        {
            var cts = new CancellationTokenSource();

            var t1 = Task.Run<int>(() => Print(true, cts.Token), cts.Token);

            Task t2 = t1.ContinueWith(prevTask =>
            {
                Console.WriteLine($"How many numbers were processed by prev. task={prevTask.Result}");
                Task.Run<int>(() => Print(false, cts.Token), cts.Token);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            t1.ContinueWith(t =>
            {
                Console.WriteLine("Finally, we are here!");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
        private static void Delay()
        {
            var t1 = Task.Run(() => Print(true, CancellationToken.None));
            Task t2 = null;

            Console.WriteLine("Started t1");

            Task.Delay(5000).ContinueWith(x =>
            {
                t2 = Task.Run(() => Print(false, CancellationToken.None));
                Console.WriteLine("Started t2");
            });
        }

        private static void TestAggregateException()
        {
            var parent = Task.Factory.StartNew(() =>
            {
                // We'll throw 3 exceptions at once using 3 child tasks: 
                int[] numbers = { 0 };
                var childFactory = new TaskFactory(TaskCreationOptions.AttachedToParent, 
                    TaskContinuationOptions.None);
                childFactory.StartNew(() => 5 / numbers[0]); // Division by zero
                childFactory.StartNew(() => numbers[1]); // Index out of range
                childFactory.StartNew(() => { throw null; }); // Null reference
            });
            try
            {
                parent.Wait();
            }
            catch (AggregateException aex)
            {
                aex.Flatten().Handle(ex =>
                {
                    if (ex is DivideByZeroException)
                    {
                        Console.WriteLine("Divide by zero");
                        return true;
                    }

                    if (ex is IndexOutOfRangeException)
                    {
                        Console.WriteLine("Index out of range");
                        return true;
                    }

                    return false;
                });
            }
        }

        public Task ImportXmlFilesAsync(string dataDirectory, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (FileInfo file in new DirectoryInfo(dataDirectory).GetFiles("*.xml"))
                {
                    XElement doc = XElement.Load(file.FullName);
                    InternalProcessXml(doc, CancellationToken.None);
                }
            }, ct);
        }

        public Task ImportXmlFilesAsync2(string dataDirectory, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (FileInfo file in new DirectoryInfo(dataDirectory).GetFiles("*.xml"))
                {
                    string fileToProcess = file.FullName;
                    Task.Factory.StartNew(_ =>
                    {
                        ct.ThrowIfCancellationRequested();

                        XElement doc = XElement.Load(fileToProcess);
                        InternalProcessXml(doc, ct);
                    }, ct, TaskCreationOptions.AttachedToParent);
                }
            }, ct);
        }


        private void InternalProcessXml(XElement doc, CancellationToken ct)
        {

        }

        private void DumpWebPage(string uri)
        {
            WebClient wc = new WebClient();
            string page = wc.DownloadString(uri);
            Console.WriteLine(page);
        }

        private async void DumpWebPageAsync(string uri)
        {
            WebClient wc = new WebClient();
            string page = await wc.DownloadStringTaskAsync(uri);
            //Task<string> DownloadStringTaskAsync(string address)
            Console.WriteLine(page);
        }

        private void DumpWebPageTaskBased(string uri)
        {
            WebClient webClient = new WebClient();
            Task<string> task = webClient.DownloadStringTaskAsync(uri);
            task.ContinueWith(t => { Console.WriteLine(t.Result); });
        }

        public async void Test()
        {
            Task operation1 = Operation1();
            Task operation2 = Operation2();
            await operation1;
            await operation2;

        }

        private Task Operation()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        private Task Operation1()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        private Task Operation2()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(1000));
        }


        static void Main(string[] args)
        {
            CatchMultipleExceptionsWithAwait();

            Console.Read();
        }

        private static async void CatchMultipleExceptionsWithAwait()
        {
            int[] numbers = {0};

            Task<int> t1 = Task.Run(() => 5 / numbers[0]);
            Task<int> t2 = Task.Run(() => numbers[1]);

            Task<int[]> allTask = Task.WhenAll(t1, t2);
            try
            {
                await allTask;
            }
            catch
            {
                foreach (var ex in allTask.Exception.InnerExceptions)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        static async Task Catcher()
        {
            try
            {
                Task thrower = Thrower();
                await thrower;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex);
            }
        }

        async static Task Thrower()
        {
            await Task.Delay(100);
            throw new InvalidOperationException();
        }

        private static void AttachedToParent()
        {
            Task.Factory.StartNew(() =>
            {
                Task nested = Task.Factory.StartNew(() =>
                    Console.WriteLine("hello world"), TaskCreationOptions.AttachedToParent);
            }).Wait();

            Thread.Sleep(100);
        }

        private static int Print(bool isEven, CancellationToken token)
        {
            Console.WriteLine($"Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}");
            int total = 0;
            if (isEven)
            {
                for (int i = 0; i < 100; i += 2)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancellation Requested");                       
                    }
                    token.ThrowIfCancellationRequested();
                    total++;
                    Console.WriteLine($"Current task id = {Task.CurrentId}. Value={i}");
                }
            }
            else
            {
                for (int i = 1; i < 100; i += 2)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancellation Requested");
                    }
                    token.ThrowIfCancellationRequested();
                    total++;
                    Console.WriteLine($"Current task id = {Task.CurrentId}. Value={i}");
                }
            }

            return total;
        }

        /*
        private static void TokenWaitHandle(CancellationToken token)
        {
            if (token.WaitHandle.WaitOne(2000))
            {
                token.ThrowIfCancellationRequested();
            }
        }
        */
        /*
        private static void RunningTasks()
        {
            Task<int> t1 = Task.Factory.StartNew(() => Print(true), CancellationToken.None,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Task<int> t2 = Task.Factory.StartNew(() => Print(false));

            Console.WriteLine($"The first task processed:{t1.Result}");
            Console.WriteLine($"The second task processed:{t2.Result}");
        }*/
    }
}
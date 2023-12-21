using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCSharp_Course
{
    class Processes
    {
        public void Demo()
        {
            //Process.Start("notepad.exe", "C:\\tmp\\HelloWorld.txt");
            //Process.Start("C:\\tmp\\HelloWorld.txt");

            var app = new Process
            {
                StartInfo =
                {
                    FileName = @"notepad.exe",
                    Arguments = "C:\\tmp\\HelloWorld.txt"
                }
            };
            app.Start();

            app.PriorityClass = ProcessPriorityClass.RealTime;

            //app.WaitForExit();
            Console.WriteLine("No more waiting");


            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                if (p.ProcessName == "notepad")
                {
                    p.Kill();
                }
            }

        }
    }
}

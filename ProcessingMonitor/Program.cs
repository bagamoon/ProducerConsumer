using ProcessingMonitor.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingMonitor
{
    class Program
    {
        private static List<Task> tasks = new List<Task>();
        private static List<MonitorProcess> monitors = new List<MonitorProcess>();

        static void Main(string[] args)
        {
            var cancelTokenSource = new CancellationTokenSource();

            while (true)
            {
                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.C)
                {
                    cancelTokenSource.Cancel();
                }
                else if (input.Key == ConsoleKey.S)
                {
                    cancelTokenSource = new CancellationTokenSource();
                    PrepareThreads(cancelTokenSource);
                }
                else if (input.Key == ConsoleKey.Q)
                {
                    break;
                }
            }
        }

        private static void PrepareThreads(CancellationTokenSource cancelTokenSource)
        {
            string name = $"Monitor";
            MonitorProcess m = new MonitorProcess(name);
            monitors.Add(m);

            var task = Task.Factory.StartNew(() => m.Run(cancelTokenSource), cancelTokenSource.Token,
                                                     TaskCreationOptions.LongRunning, TaskScheduler.Default);
            tasks.Add(task);
        }
    }
}

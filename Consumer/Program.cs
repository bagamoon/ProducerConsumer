using Consumer.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        private static List<Task> tasks = new List<Task>();
        private static List<ConsumerProcess> producers = new List<ConsumerProcess>();
        private static int threadCound = 5;

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
                    PrepareThreads(threadCound, cancelTokenSource);                    
                }
                else if (input.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

        }

        private static void PrepareThreads(int count, CancellationTokenSource cancelTokenSource)
        {
            for (int i = 1; i <= count; i++)
            {
                string name = $"Consumer-{i}";
                ConsumerProcess c = new ConsumerProcess(name);
                producers.Add(c);
                
                var task = Task.Factory.StartNew(() => c.Run(cancelTokenSource), cancelTokenSource.Token,
                                                         TaskCreationOptions.LongRunning, TaskScheduler.Default);
                tasks.Add(task);
            }
        }
    }
}

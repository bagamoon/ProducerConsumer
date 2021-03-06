﻿using Consumer.Process;
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
        private static List<ConsumerProcess> consumers = new List<ConsumerProcess>();        

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
            string name = $"Consumer-{Guid.NewGuid()}";
            ConsumerProcess c = new ConsumerProcess(name);
            consumers.Add(c);

            var task = Task.Factory.StartNew(() => c.Run(cancelTokenSource), cancelTokenSource.Token,
                                                     TaskCreationOptions.LongRunning, TaskScheduler.Default);
            tasks.Add(task);
        }
    }
}

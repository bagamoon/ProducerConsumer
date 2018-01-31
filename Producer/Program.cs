using Producer.Process;
using Producer.DTO;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Producer
{
    class Program
    {       
        private static List<Thread> threads = new List<Thread>();
        private static List<ProducerProcess> producers = new List<ProducerProcess>();
        private static int threadCound = 2;

        static void Main(string[] args)
        {
            PrepareThreads(threadCound);

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
                    DispatchUpdates(cancelTokenSource);
                }
                else if (input.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

        }

        private static void DispatchUpdates(CancellationTokenSource cancelTokenSource)
        {
            var task = Task.Run(() =>
            {
                decimal odds = 1;
                while (true)
                {
                    if (cancelTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(200);

                    odds += 0.01M;
                    foreach (var producer in producers)
                    {
                        var update = new OddsUpdate
                        {
                            OddsId = 1,
                            Odds = odds
                        };

                        producer.EventUpdates.Enqueue(update);
                    }
                }
            }, cancelTokenSource.Token);
        }

        private static void PrepareThreads(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                string name = $"Producer-{i}";
                ProducerProcess p = new ProducerProcess(name);
                producers.Add(p);

                var thread = new Thread(p.Run);
                threads.Add(thread);
                thread.Start();
            }            
        }
    }
}

using Producer.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Cache;

namespace Producer.Process
{
    public class ProducerProcess
    {
        public string Name { get; private set; }

        public ConcurrentQueue<OddsUpdate> EventUpdates;

        ICacheProvider cacheProvider;

        public ProducerProcess(string name)
        {
            Name = name;
            EventUpdates = new ConcurrentQueue<OddsUpdate>();
            cacheProvider = new RedisCacheProvider();
        }

        public void Run()
        {
            while (true)
            {
                Thread.Sleep(1);

                if (EventUpdates.Any())
                {
                    OddsUpdate update;
                    if (EventUpdates.TryDequeue(out update))
                    {
                        Thread.Sleep(new Random().Next(1, 500));

                        cacheProvider.Add(update.OddsId.ToString(), update);

                        Console.WriteLine($"{Name} - updated OddId: {update.OddsId}, Odds: {update.Odds}");
                    }
                }

            }
        }
    }
}

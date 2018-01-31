using CommonLib.Cache;
using Consumer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer.Process
{
    public class ConsumerProcess
    {
        public string Name { get; private set; }

        ICacheProvider cacheProvider;

        public ConsumerProcess(string name)
        {
            Name = name;
            cacheProvider = new RedisCacheProvider();
        }

        public void Run(CancellationTokenSource cancelTokenSource)
        {            
            while (true)
            {
                if (cancelTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(1);

                var allkeys = cacheProvider.GetAllKey();

                var key = allkeys.FirstOrDefault();

                if (key != null)
                {
                    Console.WriteLine($"{Name} - get key: {key}");

                    var success = cacheProvider.Execute(key, () =>
                                {
                                    var update = cacheProvider.Get<OddsUpdate>(key);

                                    Thread.Sleep(new Random().Next(500, 1000));

                                    Console.WriteLine($"{Name} - OddsId: {update.OddsId}, Odds: {update.Odds}");

                                    cacheProvider.DeleteKey(key);
                                }, 2);

                    if (!success)
                    {
                        Console.WriteLine($"{Name} - cannot get the lock with key: {key}");
                    }
                }
            }

        }

    }
}

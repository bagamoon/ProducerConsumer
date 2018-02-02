using Producer.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Cache;
using StackExchange.Redis;
using System.Configuration;
using Newtonsoft.Json;

namespace Producer.Process
{
    public class ProducerProcess
    {
        public string Name { get; private set; }

        public ConcurrentQueue<OddsUpdate> EventUpdates;

        private string Host = ConfigurationManager.AppSettings["RedisHost"];
        private int Port = Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"]);
        private ConnectionMultiplexer _redisConn;        

        public ProducerProcess(string name)
        {
            Name = name;
            EventUpdates = new ConcurrentQueue<OddsUpdate>();

            string connString = $"{Host}:{Port}";
            _redisConn = ConnectionMultiplexer.Connect(connString);
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

                if (EventUpdates.Any())
                {
                    OddsUpdate update;
                    if (EventUpdates.TryDequeue(out update))
                    {
                        Thread.Sleep(new Random().Next(1, 500));

                        string key = $"OddsUpdate-{update.OddsId}";                        

                        var db = _redisConn.GetDatabase();
                        db.ListLeftPush(key, JsonConvert.SerializeObject(update));
                        


                        Console.WriteLine($"{Name} - updated OddId: {update.OddsId}, Odds: {update.Odds}, Datetime: {update.DateUpdated.ToString("mm:ss.fff")}");
                    }
                }

            }
        }
    }
}

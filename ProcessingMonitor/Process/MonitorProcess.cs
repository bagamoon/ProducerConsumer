using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProcessingMonitor.Process
{
    public class MonitorProcess
    {
        public string Name { get; private set; }

        private string Host = ConfigurationManager.AppSettings["RedisHost"];
        private int Port = Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"]);
        private ConnectionMultiplexer _redisConn;

        public MonitorProcess(string name)
        {
            Name = name;

            string connString = $"{Host}:{Port}";
            _redisConn = ConnectionMultiplexer.Connect(connString);
        }

        public void Run(CancellationTokenSource cancelTokenSource)
        {            
            while (true)
            {
                Thread.Sleep(100);

                if (cancelTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                var db = _redisConn.GetDatabase();
                var consumersNames = db.SetMembers("OddsConsumers");

                var key = "OddsUpdate";

                foreach (var consumer in consumersNames)
                {
                    var processingKey = $"{key}-Processing-{consumer}";

                    var nowTicks = DateTime.Now.Ticks;
                    var processingUpdates = db.SortedSetRangeByScore(processingKey, 0, nowTicks - 60 * 10000000);
                    if (processingUpdates.Any())
                    {
                        foreach (var expired in processingUpdates)
                        {                            
                            Console.WriteLine($"put back {consumer} expired processing update, {expired}");
                            db.SortedSetRemove(processingKey, expired);
                            db.ListLeftPush(key, expired);
                        }
                    }

                }
            }

        }

    }    
}
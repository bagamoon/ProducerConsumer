using CommonLib.Cache;
using Consumer.DTO;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer.Process
{
    public class ConsumerProcess
    {
        public string Name { get; private set; }

        private string Host = ConfigurationManager.AppSettings["RedisHost"];
        private int Port = Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"]);
        private ConnectionMultiplexer _redisConn;

        public ConsumerProcess(string name)
        {
            Name = name;

            string connString = $"{Host}:{Port}";
            _redisConn = ConnectionMultiplexer.Connect(connString);                       
        }

        public void Run(CancellationTokenSource cancelTokenSource)
        {
            var db = _redisConn.GetDatabase();
            var processKey = "OddsConsumers";
            db.SetAdd(processKey, Name);

            while (true)
            {
                if (cancelTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(1);                

                var key = "OddsUpdate";
                var updateSerialized = db.ListLeftPop(key);                

                var processingKey = $"{key}-Processing-{Name}";
                var doneKey = $"{key}-Done";

                if (updateSerialized.HasValue)
                {
                    var isDoneAlready = db.SetContains(doneKey, updateSerialized);
                    if (isDoneAlready)
                    {
                        Console.WriteLine($"already done: {updateSerialized}, update was ignored");
                        continue;
                    }

                    var update = JsonConvert.DeserializeObject<OddsUpdate>(updateSerialized);                    

                    var isMoveProcessing = db.SortedSetAdd(processingKey, updateSerialized, update.DateUpdated.Ticks);                    

                    if (isMoveProcessing)
                    {
                        Console.WriteLine($"Processing OddsId: {update.OddsId}, Odds: {update.Odds}, DateUpdated: {update.DateUpdated.ToString("HH:mm:ss.fff")}");
                        Thread.Sleep(new Random().Next(100, 3000));

                        db.SortedSetRemove(processingKey, updateSerialized);
                        db.SetAdd(doneKey, updateSerialized);
                    }                    
                }
            }
        }
    }
}

using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedLock;
using System.Net;

namespace CommonLib.Cache
{
    public class RedisCacheProvider : ICacheProvider
    {
        private const int _expiry = 10;
        private static ConnectionMultiplexer _conn;
        private static IDatabase _database;
        private static IServer _server;
        private static string _host = ConfigurationManager.AppSettings["RedisHost"];
        private static int _port = Convert.ToInt32(ConfigurationManager.AppSettings["RedisPort"]);

        static RedisCacheProvider()
        {            
            string connString = $"{_host}:{_port}";

            var _conn = CreateRedisConnection(connString);            
            _database = _conn.GetDatabase();
            _server = _conn.GetServer(_host, _port);
        }        

        public bool Add<T>(string key, T value, DateTimeOffset? expiresAt = null) where T : class
        {
            var serializedObject = JsonConvert.SerializeObject(value);
            if (expiresAt.HasValue)
            {
                var expiration = expiresAt.Value.Subtract(DateTimeOffset.Now);
                return _database.StringSet(key, serializedObject, expiration);
            }
            return _database.StringSet(key, serializedObject);
        }

        public bool DeleteKey(string key)
        {            
            return _database.KeyDelete(key);
        }

        public T Get<T>(string key) where T : class
        {
            var serializedObject = _database.StringGet(key);
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }

        public IEnumerable<string> GetAllKey()
        {
            return _server.Keys().Select(p => p.ToString());
        }

        public bool ExecuteWithRetry(string key, Action action, double expirySec = 2, double waitSec = 1)
        {
            RedisLockFactory redLockFactory = GetLockFactory();

            using (var redisLock = redLockFactory.Create(key,
                                                         TimeSpan.FromSeconds(expirySec),
                                                         TimeSpan.FromSeconds(waitSec),
                                                         TimeSpan.FromSeconds(0.5)))
            {
                if (redisLock.IsAcquired)
                {
                    action();
                    return true;
                }
            }

            return false;
        }

        public bool Execute(string key, Action action, double expirySec = 2)
        {
            RedisLockFactory redLockFactory = GetLockFactory();

            using (var redisLock = redLockFactory.Create(key, TimeSpan.FromSeconds(expirySec)))
            {
                if (redisLock.IsAcquired)
                {
                    action();
                    return true;
                }
            }

            return false;
        }

        private RedisLockFactory GetLockFactory()
        {
            var endPoint = new List<EndPoint>
            {
                new DnsEndPoint(_host, _port)
            };

            var redLockFactory = new RedisLockFactory(endPoint);
            return redLockFactory;
        }

        private static ConnectionMultiplexer CreateRedisConnection(string connectionString)
        {
            return ConnectionMultiplexer.Connect(connectionString);
        }
    }
}

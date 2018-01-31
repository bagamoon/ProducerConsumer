using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Cache
{
    public class RedisCacheProvider : ICacheProvider
    {
        private const int _expiry = 10;
        private static ConnectionMultiplexer _conn;
        private static IDatabase _database;

        static RedisCacheProvider()
        {
            string host = ConfigurationManager.AppSettings["RedisHost"];
            string port = ConfigurationManager.AppSettings["RedisPort"];
            string connString = $"{host}:{port}";

            var _conn = CreateRedisConnection(connString);            
            _database = _conn.GetDatabase();
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

        private static ConnectionMultiplexer CreateRedisConnection(string connectionString)
        {
            return ConnectionMultiplexer.Connect(connectionString);
        }
    }
}

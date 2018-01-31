using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Cache
{
    public interface ICacheProvider
    {
        bool Add<T>(string key, T value, DateTimeOffset? expiresAt = null) where T : class;

        bool DeleteKey(string key);

        T Get<T>(string key) where T : class;

        IEnumerable<string> GetAllKey();

        bool ExecuteWithRetry(string key, Action action, double expirySec, double waitSec);

        bool Execute(string key, Action action, double expirySec);
    }
}

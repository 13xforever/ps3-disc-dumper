using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ps3DiscDumper.DiscKeyProviders
{
    public interface IDiscKeyProvider
    {
        Task<HashSet<DiscKeyInfo>> EnumerateAsync(string discKeyCachePath, string ProductCode, CancellationToken cancellationToken);
    }
}
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public class SystemInformation
    {
        public virtual async Task<IEnumerable<Models.Storage>> GetStorageInformations()
        {
#if DEBUG
            DebugSystemInformation system = new DebugSystemInformation();
#else
            RaspbianSystemInformation system = new RaspbianSystemInformation();
#endif
            return await system.GetStorageInformations();
        }

        public async virtual Task<Models.Memory> GetMemoryInformations()
        {
#if DEBUG
            DebugSystemInformation system = new DebugSystemInformation();
#else
            RaspbianSystemInformation system = new RaspbianSystemInformation();
#endif
            return await system.GetMemoryInformations();
        }

        public async virtual Task<Models.Cpu> GetCpuInformations()
        {
#if DEBUG
            DebugSystemInformation system = new DebugSystemInformation();
#else
            RaspbianSystemInformation system = new RaspbianSystemInformation();
#endif
            return await system.GetCpuInformations();
        }
    }
}
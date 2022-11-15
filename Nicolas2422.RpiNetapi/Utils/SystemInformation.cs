using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public static class SystemInformation
    {
        public async static Task<IEnumerable<Models.Storage>> GetStorageInformations()
        {
#if DEBUG
            return await DebugSystemInformation.GetStorageInformations();
#else
            return await RaspbianSystemInformation.GetStorageInformations();
#endif
        }

        public async static Task<Models.Memory> GetMemoryInformations()
        {
#if DEBUG
            return await DebugSystemInformation.GetMemoryInformations();
#else
            return await RaspbianSystemInformation.GetMemoryInformations();
#endif
        }

        public async static Task<Models.Cpu> GetCpuInformations()
        {
#if DEBUG
            return await DebugSystemInformation.GetCpuInformations();
#else
            return await RaspbianSystemInformation.GetCpuInformations();
#endif
        }
    }
}
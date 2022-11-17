using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public interface ISystemInformation
    {
        public abstract Task<IEnumerable<Models.Storage>> GetStorageInformations();
        public abstract Task<Models.Memory> GetMemoryInformations();
        public abstract Task<Models.Cpu> GetCpuInformations();
    }
}
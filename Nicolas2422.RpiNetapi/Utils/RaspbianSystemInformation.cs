using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public class RaspbianSystemInformation : SystemInformation
    {
        public override async Task<IEnumerable<Models.Storage>> GetStorageInformations()
        {
            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "df",
                Arguments = "--output=source,fstype,size,used,avail,pcent,target -x tmpfs -x devtmpfs",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            return await Task.Run(() =>
            {
                List<Models.Storage> listStorageInformations = new List<Models.Storage>();

                Regex regexStorageDataExtraction = new Regex(
                    @"(?<Filesystem>[\da-zA-Z\/]+)\s+(?<Type>[\da-zA-Z\/]+)\s+(?<Size>[\d]+)\s+(?<Used>[\d]+)\s+(?<Avail>[\d]+)\s+(?<Percent>[\d]+)\%\s+(?<Mounted>[\da-zA-Z\/]+)",
                    RegexOptions.IgnoreCase);

                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    Match match = regexStorageDataExtraction.Match(line);
                    if (!match.Success)
                        continue;

                    listStorageInformations.Add(
                        new Models.Storage(
                            Filesystem: match.Groups["Filesystem"].Value,
                            Type: match.Groups["Type"].Value,
                            Size: int.Parse(match.Groups["Size"].Value),
                            Used: int.Parse(match.Groups["Used"].Value),
                            Avail: int.Parse(match.Groups["Avail"].Value),
                            Usage: decimal.Parse(match.Groups["Percent"].Value),
                            Mounted: match.Groups["Mounted"].Value
                    ));
                }

                return listStorageInformations;
            });
        }

        public override async Task<Models.Memory> GetMemoryInformations()
        {
            int total = 0, used = 0, free = 0;
            decimal usage = 0;

            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "free",
                Arguments = "",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                Regex regexMemoryDataExtraction = new Regex(
                    @"(?<label>[\da-zA-Z\/\:]+)\s+" +
                    @"(?<total>[\d]+)\s+" +
                    @"(?<used>[\d]+)\s+" +
                    @"(?<free>[\d]+)\s+" +
                    @"(?<shared>[\d]+)\s+" +
                    @"(?<buffcache>[\d]+)\s+" +
                    @"(?<available>[\d]+)",
                    RegexOptions.IgnoreCase);

                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    Match match = regexMemoryDataExtraction.Match(line);
                    if (!match.Success || match.Groups["label"].Value.Trim() != "Mem:")
                        continue;

                    total = int.Parse(match.Groups["total"].Value);
                    used = int.Parse(match.Groups["used"].Value);
                    free = int.Parse(match.Groups["free"].Value);
                    usage = (used * 100) / total;
                }
            });

            return new Models.Memory(total, used, free, usage);
        }

        public override async Task<Models.Cpu> GetCpuInformations()
        {
            Task<(string, string)> task = Task.Run(async () => await GetCpuHardware());
            Task<decimal> temperature = Task.Run(async () => await GetCpuTemperature());
            Task<int> cpuFreq = Task.Run(async () => await GetCpuFrequency());
            Task<decimal> cpuUsage = Task.Run(async () => await GetCpuUsage());

            await Task.WhenAll(task, temperature, cpuFreq, cpuUsage);

            return new Models.Cpu(task.Result.Item1, task.Result.Item2,
                cpuFreq.Result,
                cpuUsage.Result,
                temperature.Result
            );
        }

        private async Task<(string, string)> GetCpuHardware()
        {
            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "cat",
                Arguments = "/proc/cpuinfo",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            return await Task.Run(() =>
            {
                Regex regexCpuInfoDataExtraction = new Regex(
                    @"(?<Key>[\s\S]+):(?<Value>[\s\S]+)",
                    RegexOptions.IgnoreCase);
                string hardware = String.Empty;
                string model = String.Empty;

                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    Match match = regexCpuInfoDataExtraction.Match(line);
                    if (!match.Success)
                        continue;

                    if (match.Groups["Key"].Value.Trim() == "Hardware")
                        hardware = match.Groups["Value"].Value.Trim();
                    else if (match.Groups["Key"].Value.Trim() == "Model")
                        model = match.Groups["Value"].Value.Trim();
                }
                return (hardware, model);
            });
        }

        private async Task<decimal> GetCpuTemperature()
        {
            decimal temperature = 0;
            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "cat",
                Arguments = "/sys/class/thermal/thermal_zone0/temp",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    if (decimal.TryParse(line, out temperature))
                        temperature /= 1000;
                }
            });

            return temperature;
        }

        private async Task<int> GetCpuFrequency()
        {
            int frequency = 0;

            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "cat",
                Arguments = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    int.TryParse(line, out frequency);
                }
            });

            return frequency;
        }

        private async Task<decimal> GetCpuUsage()
        {
            decimal usage = 0;

            /* Run df cmd */
            var psi = new ProcessStartInfo
            {
                FileName = "vmstat",
                Arguments = "1 2",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                Regex regexCpuDataExtraction = new Regex(
                    @"(?<r>[\da-zA-Z\/]+)\s+" +
                    @"(?<b>[\d]+)\s+" +
                    @"(?<swpd>[\d]+)\s+" +
                    @"(?<free>[\d]+)\s+" +
                    @"(?<buff>[\d]+)\s+" +
                    @"(?<cache>[\d]+)\s+" +
                    @"(?<si>[\d]+)\s+" +
                    @"(?<so>[\d]+)\s+" +
                    @"(?<bi>[\d]+)\s+" +
                    @"(?<bo>[\d]+)\s+" +
                    @"(?<in>[\d]+)\s+" +
                    @"(?<cs>[\d]+)\s+" +
                    @"(?<us>[\d]+)\s+" +
                    @"(?<sy>[\d]+)\s+" +
                    @"(?<id>[\d]+)\s+" +
                    @"(?<wa>[\d]+)\s+" +
                    @"(?<st>[\d]+)",
                    RegexOptions.IgnoreCase);

                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;

                    Match match = regexCpuDataExtraction.Match(line);
                    if (!match.Success)
                        continue;

                    usage = 100 - int.Parse(match.Groups["id"].Value);
                }
            });

            return usage;
        }
    }
}
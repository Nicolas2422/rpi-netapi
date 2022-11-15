using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public class DebugSystemInformation : SystemInformation
    {
        public override async Task<IEnumerable<Models.Storage>> GetStorageInformations()
        {
            string standardOutput = @"
Filesystem             Type  1K-blocks       Used     Avail Use% Mounted on
/dev/root              ext4   15019464    6857564   7519768  48% /
/dev/mmcblk0p1         vfat     258095      50632    207464  20% /boot
/dev/mapper/volgrp-lv1 ext4   11050184         28  10457056   1% /media/users/nconvers
/dev/mapper/volgrp-lv3 ext4  102626232     546592  96820376   1% /media/svn
/dev/mapper/volgrp-lv2 ext4   11050184         28  10457056   1% /media/users/abonfils
/dev/mapper/volgrp-lv0 ext4 1650272624 1090654168 486181820  70% /media/nas";

            return await Task.Run(() =>
            {
                List<Models.Storage> listStorageInformations = new List<Models.Storage>();

                Regex regexStorageDataExtraction = new Regex(
                    @"(?<Filesystem>[\da-zA-Z\/]+)\s+(?<Type>[\da-zA-Z\/]+)\s+(?<Size>[\d]+)\s+(?<Used>[\d]+)\s+(?<Avail>[\d]+)\s+(?<Percent>[\d]+)\%\s+(?<Mounted>[\da-zA-Z\/]+)",
                    RegexOptions.IgnoreCase);

                foreach (string line in standardOutput.Split("\r\n"))
                {
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

            await Task.Run(() =>
            {
                string standardOutput = @"
              total        used        free      shared  buff/cache   available
Mem:        3748356      149288     3229316        9996      369752     3463400
Swap:       2097148           0     2097148";

                Regex regexMemoryDataExtraction = new Regex(
                            @"(?<label>[\da-zA-Z\/\:]+)\s+" +
                            @"(?<total>[\d]+)\s+" +
                            @"(?<used>[\d]+)\s+" +
                            @"(?<free>[\d]+)\s+" +
                            @"(?<shared>[\d]+)\s+" +
                            @"(?<buffcache>[\d]+)\s+" +
                            @"(?<available>[\d]+)",
                            RegexOptions.IgnoreCase);

                foreach (string line in standardOutput.Split("\r\n"))
                {
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
            /* Wait and read the cmd return */
            return await Task.Run(() =>
            {
                string standardOutput = @"
processor       : 0
model name      : ARMv7 Processor rev 3 (v7l)
BogoMIPS        : 126.00
Features        : half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32
CPU implementer : 0x41
CPU architecture: 7
CPU variant     : 0x0
CPU part        : 0xd08
CPU revision    : 3

processor       : 1
model name      : ARMv7 Processor rev 3 (v7l)
BogoMIPS        : 126.00
Features        : half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32
CPU implementer : 0x41
CPU architecture: 7
CPU variant     : 0x0
CPU part        : 0xd08
CPU revision    : 3

processor       : 2
model name      : ARMv7 Processor rev 3 (v7l)
BogoMIPS        : 126.00
Features        : half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32
CPU implementer : 0x41
CPU architecture: 7
CPU variant     : 0x0
CPU part        : 0xd08
CPU revision    : 3

processor       : 3
model name      : ARMv7 Processor rev 3 (v7l)
BogoMIPS        : 126.00
Features        : half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt vfpd32 lpae evtstrm crc32
CPU implementer : 0x41
CPU architecture: 7
CPU variant     : 0x0
CPU part        : 0xd08
CPU revision    : 3

Hardware        : BCM2711
Revision        : c03111
Serial          : 1000000021e960a1
Model           : Raspberry Pi 4 Model B Rev 1.1
";

                Regex regexCpuInfoDataExtraction = new Regex(
                    @"(?<Key>[\s\S]+):(?<Value>[\s\S]+)",
                    RegexOptions.IgnoreCase);

                string hardware = String.Empty;
                string model = String.Empty;

                foreach (string line in standardOutput.Split("\r\n"))
                {
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

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                string standardOutput = @"29693";

                foreach (string line in standardOutput.Split("\r\n"))
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    if (decimal.TryParse(line, out temperature))
                        temperature /= 1000.0M;
                }
            });

            return temperature;
        }

        private async Task<int> GetCpuFrequency()
        {
            int frequency = 0;

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                string standardOutput = @"700000";

                foreach (string line in standardOutput.Split("\r\n"))
                {
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

            /* Wait and read the cmd return */
            await Task.Run(() =>
            {
                string standardOutput = @"
procs -----------memory---------- ---swap-- -----io---- -system-- ------cpu-----
 r  b   swpd   free   buff  cache   si   so    bi    bo   in   cs us sy id wa st
 0  0      0 3232260  66732 300236    0    0    24     1  126  147  1  2 97  0  0
 0  0      0 3232304  66732 300236    0    0     0     0  431  474  0  3 97  0  0
";

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

                foreach (string line in standardOutput.Split("\r\n"))
                {
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
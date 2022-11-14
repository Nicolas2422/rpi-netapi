using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Utils
{
    public static class SystemInformation
    {
        public static void RunGetStorageInformationsCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            List<Dictionary<String, Object>> listDictionaryStorageInformations = new List<Dictionary<String, Object>>();

            try
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
                Task.WaitAll(Task.Run(() =>
                {
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

                        Dictionary<string, object> tmp = new Dictionary<string, object>();
                        tmp.Add("Filesystem", match.Groups["Filesystem"].Value);
                        tmp.Add("Type", match.Groups["Type"].Value);
                        tmp.Add("Size", int.Parse(match.Groups["Size"].Value));
                        tmp.Add("Used", int.Parse(match.Groups["Used"].Value));
                        tmp.Add("Avail", int.Parse(match.Groups["Avail"].Value));
                        tmp.Add("Usage", decimal.Parse(match.Groups["Percent"].Value));
                        tmp.Add("Mounted", match.Groups["Mounted"].Value);
                        listDictionaryStorageInformations.Add(tmp);
                    }
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Storages", listDictionaryStorageInformations);
        }

        public static void RunGetCpuInformationsCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            string hardware = String.Empty;
            string model = String.Empty;

            try
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
                Task.WaitAll(Task.Run(() =>
                {
                    Regex regexCpuInfoDataExtraction = new Regex(
                        @"(?<Key>[\s\S]+):(?<Value>[\s\S]+)",
                        RegexOptions.IgnoreCase);
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
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Hardware", hardware);
            dictionaryInformations.Add("Model", model);
        }

        public static void RunGetCpuTemperatureCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            decimal temperature = 0;

            try
            {
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
                Task.WaitAll(Task.Run(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string? line = proc.StandardOutput.ReadLine();
                        if (String.IsNullOrEmpty(line))
                            continue;

                        if (decimal.TryParse(line, out temperature))
                            temperature /= 1000;

                    }
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Temperature", temperature);
        }

        public static void RunGetCpuFrequencyCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            int frequency = 0;

            try
            {
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
                Task.WaitAll(Task.Run(() =>
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string? line = proc.StandardOutput.ReadLine();
                        if (String.IsNullOrEmpty(line))
                            continue;

                        int.TryParse(line, out frequency);
                    }
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Frequency", frequency);
        }

        public static void RunGetCpuUsageCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            decimal usage = 0;

            try
            {
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
                Task.WaitAll(Task.Run(() =>
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
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Usage", usage);
        }

        public static void RunGetMemoryInformationCmd(ref Dictionary<String, Object> dictionaryInformations)
        {
            int total = 0, used = 0, free = 0;
            decimal usage = 0;

            try
            {
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
                Task.WaitAll(Task.Run(() =>
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
                }));
                proc.WaitForExit();
            }
            catch
            {
            }

            dictionaryInformations.Add("Total", total);
            dictionaryInformations.Add("Used", used);
            dictionaryInformations.Add("Free", free);
            dictionaryInformations.Add("Usage", usage);
        }
    }
}
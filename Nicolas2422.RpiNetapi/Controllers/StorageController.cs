using Microsoft.AspNetCore.Mvc;
using Nicolas2422.RpiNetapi.Models;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;

        public StorageController(ILogger<StorageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Storage> Get()
        {
            return BuildStorageInformations();
        }

        /// <summary>
        /// Build storage informations
        /// </summary>
        /// <returns>List of storage</returns>
        private Storage[] BuildStorageInformations()
        {
            Regex regexStorageDataExtraction = new Regex(
                @"(?<Filesystem>[\da-zA-Z\/]+)\s+(?<Type>[\da-zA-Z\/]+)\s+(?<Size>[\d]+)\s+(?<Used>[\d]+)\s+(?<Avail>[\d]+)\s+(?<Percent>[\d\%]+)\s+(?<Mounted>[\da-zA-Z\/]+)",
                RegexOptions.IgnoreCase);

            List<Storage> storages = new List<Storage>();
            foreach (var storageLine in RunStorageCmd())
            {
                Match match = regexStorageDataExtraction.Match(storageLine);
                if (match.Success)
                {
                    storages.Add(new Storage()
                    {
                        Filesystem = match.Groups["Filesystem"].Value,
                        Type = match.Groups["Type"].Value,
                        Size = int.Parse(match.Groups["Size"].Value),
                        Used = int.Parse(match.Groups["Used"].Value),
                        Avail = int.Parse(match.Groups["Avail"].Value),
                        Percent = match.Groups["Percent"].Value,
                        Mounted = match.Groups["Mounted"].Value,
                    });
                }
            }

            return storages.ToArray();
        }

        /// <summary>
        /// Run the df shell comd
        /// </summary>
        /// <returns>List of line return by df cmd</returns>
        private List<string> RunStorageCmd()
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
            List<string> storageLines = new List<string>();
            Task.WaitAll(Task.Run(() =>
            {
                while (!proc.StandardOutput.EndOfStream)
                {
                    string? line = proc.StandardOutput.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                        storageLines.Add(line);
                }
            }));
            proc.WaitForExit();

            return storageLines;
        }
    }
}
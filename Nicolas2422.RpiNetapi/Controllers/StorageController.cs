using Microsoft.AspNetCore.Mvc;
using Nicolas2422.RpiNetapi.Models;
using Nicolas2422.RpiNetapi.Utils;
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
            Dictionary<String, Object> dictionaryInformations = new Dictionary<string, Object>();
            SystemInformation.RunGetStorageInformationsCmd(ref dictionaryInformations);

            List<Storage> storages = new List<Storage>();
            foreach (var item in (List<Dictionary<String, Object>>)dictionaryInformations["Storages"])
            {
                storages.Add(new Storage()
                {
                    Filesystem = (string)item["Filesystem"],
                    Type = (string)item["Type"],
                    Size = (int)item["Size"],
                    Used = (int)item["Used"],
                    Avail = (int)item["Avail"],
                    Usage = (decimal)item["Usage"],
                    Mounted = (string)item["Mounted"],
                });
            }

            return storages.ToArray();
        }
    }
}
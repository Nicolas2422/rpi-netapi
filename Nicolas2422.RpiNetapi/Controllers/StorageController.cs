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
        private readonly ISystemInformation _systemInformation;

        public StorageController(ILogger<StorageController> logger, ISystemInformation systemInformation)
        {
            _logger = logger;
            _systemInformation = systemInformation;
        }

        [HttpGet]
        public async Task<IEnumerable<Storage>> Get() => await _systemInformation.GetStorageInformations();
    }
}
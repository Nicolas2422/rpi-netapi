using Microsoft.AspNetCore.Mvc;
using Nicolas2422.RpiNetapi.Models;
using Nicolas2422.RpiNetapi.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace Nicolas2422.RpiNetapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly ILogger<MemoryController> _logger;

        public MemoryController(ILogger<MemoryController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Memory Get()
        {
            return BuildMemoryInformations();
        }

        /// <summary>
        /// Build cpu informations
        /// </summary>
        /// <returns>List of storage</returns>
        private Memory BuildMemoryInformations()
        {
            Dictionary<String, Object> dictionaryInformations = new Dictionary<string, Object>();
            SystemInformation.RunGetMemoryInformationCmd(ref dictionaryInformations);

            return new Memory()
            {
                Total = (int)dictionaryInformations["Total"],
                Used = (int)dictionaryInformations["Used"],
                Free = (int)dictionaryInformations["Free"],
                Usage = (decimal)dictionaryInformations["Usage"],
            };
        }
    }
}
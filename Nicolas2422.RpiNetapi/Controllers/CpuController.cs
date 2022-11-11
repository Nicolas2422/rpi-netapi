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
    public class CpuController : ControllerBase
    {
        private readonly ILogger<CpuController> _logger;

        public CpuController(ILogger<CpuController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Cpu Get()
        {
            return BuildCpuInformations();
        }

        /// <summary>
        /// Build cpu informations
        /// </summary>
        /// <returns>List of storage</returns>
        private Cpu BuildCpuInformations()
        {
            Dictionary<String, Object> dictionaryInformations = new Dictionary<string, Object>();
            SystemInformation.RunGetCpuInformationsCmd(ref dictionaryInformations);
            SystemInformation.RunGetCpuTemperatureCmd(ref dictionaryInformations);
            SystemInformation.RunGetCpuFrequencyCmd(ref dictionaryInformations);
            SystemInformation.RunGetCpuUsageCmd(ref dictionaryInformations);

            return new Cpu()
            {
                Hardware = (string)dictionaryInformations["Hardware"],
                Model = (string)dictionaryInformations["Model"],
                Temperature = (decimal)dictionaryInformations["Temperature"],
                Frequency = (int)dictionaryInformations["Frequency"],
                Usage = (decimal)dictionaryInformations["Usage"],
            };
        }
    }
}
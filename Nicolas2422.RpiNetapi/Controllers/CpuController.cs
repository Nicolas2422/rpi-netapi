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
        public async Task<Cpu> Get() => await SystemInformation.GetCpuInformations();
    }
}
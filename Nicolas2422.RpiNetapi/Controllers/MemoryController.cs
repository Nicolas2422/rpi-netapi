using Microsoft.AspNetCore.Mvc;
using Nicolas2422.RpiNetapi.Models;
using Nicolas2422.RpiNetapi.Utils;

namespace Nicolas2422.RpiNetapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly ILogger<MemoryController> _logger;
        private readonly SystemInformation _systemInformation;

        public MemoryController(ILogger<MemoryController> logger, SystemInformation systemInformation)
        {
            _logger = logger;
            _systemInformation = systemInformation;
        }

        [HttpGet]
        public async Task<Memory> Get() => await _systemInformation.GetMemoryInformations();
    }
}
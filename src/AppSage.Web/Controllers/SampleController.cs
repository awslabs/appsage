using AppSage.Core.Logging;
using Microsoft.AspNetCore.Mvc;

namespace AppSage.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly IAppSageLogger _logger;

        // IAppSageLogger is injected through the constructor
        public SampleController(IAppSageLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Sample endpoint was called");
                
                // Your business logic here
                
                return Ok(new { message = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred in sample endpoint", ex);
                return StatusCode(500, "An error occurred");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Csp.Api.Services;

namespace Csp.Api.Controllers
{
    /// <summary>
    /// Maintenance endpoints for administrators (e.g., manual fine calculation trigger).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IFineCalculationService _fineCalculationService;

        public MaintenanceController(IFineCalculationService fineCalculationService)
        {
            _fineCalculationService = fineCalculationService;
        }

        /// <summary>
        /// Manually triggers the overdue fine calculation job. Admin only. Not for production use.
        /// </summary>
        /// <returns>Number of lending records updated.</returns>
        [HttpPost("trigger-fine-calculation")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> TriggerFineCalculation()
        {
            var affected = await _fineCalculationService.CalculateOverdueFinesAsync();
            return Ok(new { affected });
        }
    }
}

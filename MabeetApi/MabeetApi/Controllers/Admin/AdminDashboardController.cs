using MabeetApi.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MabeetApi.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
  

    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _dashboardService.GetDashboardDataAsync();
            return Ok(result);
        }
    }
}

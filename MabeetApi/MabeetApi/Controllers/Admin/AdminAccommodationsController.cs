using MabeetApi.DTOs.Admin.Accommodations;
using MabeetApi.Services.Admin.Accommodations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MabeetApi.Controllers.Admin
{
    [Route("api/admin/accommodations")]
    [ApiController]
    public class AdminAccommodationsController : ControllerBase
    {
        private readonly IAdminAccommodationService _adminAccommodationService;

        public AdminAccommodationsController(IAdminAccommodationService adminAccommodationService)
        {
            _adminAccommodationService = adminAccommodationService;
        }

        // GET: api/admin/accommodations
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var accommodations = await _adminAccommodationService.GetAllAsync();
            return Ok(accommodations);
        }

        // GET: api/admin/accommodations/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var accommodation = await _adminAccommodationService.GetByIdAsync(id);
            if (accommodation == null)
                return NotFound();

            return Ok(accommodation);
        }

        // PUT: api/admin/accommodations/{id}/status
        [Authorize]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAccommodationStatusDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _adminAccommodationService.UpdateStatusAsync(id, dto);
            if (!result)
                return NotFound();

            return Ok(new { message = "Accommodation status updated successfully" });
        }

        // DELETE: api/admin/accommodations/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _adminAccommodationService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return Ok(new { message = "Accommodation deleted successfully" });
        }
    }
}

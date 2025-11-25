using MabeetApi.DTOs.Admin.Accommodations;
using MabeetApi.Services.Admin.Accommodations;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accommodations = await _adminAccommodationService.GetAllAsync();
            return Ok(accommodations);
        }

        // GET: api/admin/accommodations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var accommodation = await _adminAccommodationService.GetByIdAsync(id);
            if (accommodation == null)
                return NotFound();

            return Ok(accommodation);
        }

        // PUT: api/admin/accommodations/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAccommodationStatusDto dto)
        {
            var result = await _adminAccommodationService.UpdateStatusAsync(id, dto);
            if (!result)
                return NotFound();

            return Ok(new { message = "Accommodation status updated successfully" });
        }

        // DELETE: api/admin/accommodations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _adminAccommodationService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return Ok(new { message = "Accommodation deleted successfully" });
        }
    }
}

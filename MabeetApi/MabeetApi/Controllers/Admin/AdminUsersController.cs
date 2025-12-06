using MabeetApi.DTOs.Admin;
using MabeetApi.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace MabeetApi.Controllers.Admin
{
    [Route("api/admin/users")]  
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminService;

        public AdminUsersController(IAdminUserService adminService)
        {
            _adminService = adminService;
        }

        // get : api/admin/users
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        // get : api/admin/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // change role : api/admin/users/changerole
        [Authorize]
        [HttpPut("changerole")]
        public async Task<IActionResult> ChangeRole(ChangeUserRoleDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _adminService.ChangeUserRoleAsync(dto);
            if (!result) return BadRequest("Cannot change role");

            return Ok("Role updated successfully");
        }

        // switch active/inactive : api/admin/users/toggle-status
        [Authorize]
        [HttpPut("toggle-status")]
        public async Task<IActionResult> ToggleStatus(ToggleUserStatusDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _adminService.ToggleUserStatusAsync(dto);
            if (!result) return BadRequest("Cannot update status");

            return Ok("User status updated successfully");
        }

        // delete : api/admin/users/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                return Forbid();
            var result = await _adminService.DeleteUserAsync(id);
            if (!result) return BadRequest("Cannot delete user");

            return Ok("User deleted successfully");
        }
    }
}

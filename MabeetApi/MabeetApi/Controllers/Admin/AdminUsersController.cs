using MabeetApi.DTOs.Admin;
using MabeetApi.Services.Admin;
using Microsoft.AspNetCore.Mvc;


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
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        // get : api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // change role : api/admin/users/changerole
        [HttpPut("changerole")]
        public async Task<IActionResult> ChangeRole(ChangeUserRoleDto dto)
        {
            var result = await _adminService.ChangeUserRoleAsync(dto);
            if (!result) return BadRequest("Cannot change role");

            return Ok("Role updated successfully");
        }

        // switch active/inactive : api/admin/users/toggle-status
        [HttpPut("toggle-status")]
        public async Task<IActionResult> ToggleStatus(ToggleUserStatusDto dto)
        {
            var result = await _adminService.ToggleUserStatusAsync(dto);
            if (!result) return BadRequest("Cannot update status");

            return Ok("User status updated successfully");
        }

        // delete : api/admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _adminService.DeleteUserAsync(id);
            if (!result) return BadRequest("Cannot delete user");

            return Ok("User deleted successfully");
        }
    }
}

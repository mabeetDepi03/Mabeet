using MabeetApi.Data;
using MabeetApi.DTOs.Admin;
using MabeetApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MabeetApi.Services.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public AdminUserService(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // get all users
        public async Task<List<AdminUserListDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .Select(u => new AdminUserListDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    NationalID = u.NationalID,
                    Type = u.Type,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        // get user by id
        public async Task<AdminUserListDto> GetUserByIdAsync(string userId)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return null;

            return new AdminUserListDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                NationalID = u.NationalID,
                Type = u.Type,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            };
        }

        // change user role
        public async Task<bool> ChangeUserRoleAsync(ChangeUserRoleDto dto)
        {
            var u = await _userManager.FindByIdAsync(dto.UserId);
            if (u == null) return false;

            u.Type = dto.NewRole;
            await _userManager.UpdateAsync(u);
            return true;
        }

        // change user active status
        public async Task<bool> ToggleUserStatusAsync(ToggleUserStatusDto dto)
        {
            var u = await _userManager.FindByIdAsync(dto.UserId);
            if (u == null) return false;

            u.IsActive = dto.IsActive;
            await _userManager.UpdateAsync(u);

            return true;
        }

        // delete user
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var u = await _userManager.FindByIdAsync(userId);
            if (u == null) return false;

            var result = await _userManager.DeleteAsync(u);
            return result.Succeeded;
        }
    }
}

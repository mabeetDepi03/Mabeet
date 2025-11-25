using MabeetApi.Entities;

namespace MabeetApi.DTOs.Admin
{
    public class ChangeUserRoleDto
    {
        public string UserId { get; set; }
        public UserRole NewRole { get; set; }
    }
}

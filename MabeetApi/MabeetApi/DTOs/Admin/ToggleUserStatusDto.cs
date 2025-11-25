namespace MabeetApi.DTOs.Admin
{
    public class ToggleUserStatusDto
    {
        public string UserId { get; set; }
        public bool IsActive { get; set; }
    }
}

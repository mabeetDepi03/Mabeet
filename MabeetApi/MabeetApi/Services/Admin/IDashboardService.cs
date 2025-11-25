using MabeetApi.DTOs.Admin.Dashboard;

namespace MabeetApi.Services.Admin
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
    }
}

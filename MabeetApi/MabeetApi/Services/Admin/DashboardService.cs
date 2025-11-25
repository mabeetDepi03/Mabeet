using MabeetApi.Data;
using MabeetApi.DTOs.Admin.Dashboard;
using MabeetApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace MabeetApi.Services.Admin
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            var dto = new AdminDashboardDto();

            // Users
            dto.TotalUsers = await _context.Users.CountAsync();
            dto.TotalAdmins = await _context.Users.CountAsync(u => u.type == UserRole.Admin);
            dto.TotalOwners = await _context.Users.CountAsync(u => u.type == UserRole.Owner);
            dto.TotalClients = await _context.Users.CountAsync(u => u.type == UserRole.Client);

            // Accommodations
            dto.TotalAccommodations = await _context.Accommodations.CountAsync();
            dto.ApprovedAccommodations = await _context.Accommodations.CountAsync(a => a.IsApproved);
            dto.PendingAccommodations = await _context.Accommodations.CountAsync(a => !a.IsApproved);

            // Bookings
            dto.TotalBookings = await _context.Bookings.CountAsync();
            dto.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
            dto.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.Status == "Confirmed");
            dto.CancelledBookings = await _context.Bookings.CountAsync(b => b.Status == "Cancelled");
            dto.CompletedBookings = await _context.Bookings.CountAsync(b => b.Status == "Completed");

            return dto;
        }
    }
}

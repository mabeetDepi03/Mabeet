namespace MabeetApi.DTOs.Admin.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalOwners { get; set; }
        public int TotalClients { get; set; }
        public int TotalAdmins { get; set; }

        public int TotalAccommodations { get; set; }
        public int ApprovedAccommodations { get; set; }
        public int PendingAccommodations { get; set; }

        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int CompletedBookings { get; set; }
    }
}

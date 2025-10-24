using MabeetEF.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MabeetEF.Context
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=DESKTOP-1F1A64E\\SQLEXPRESS;Initial Catalog=Mabeet;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=True"
            );
        }

        // ==============================
        // DbSets
        // ==============================
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<LocalLoding> LocalLodings { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<StudentHouse> StudentHouses { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<StudentRoom> StudentRooms { get; set; }
        public DbSet<HotelRoom> HotelRooms { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================================
            // TPH: Accommodation hierarchy
            // ========================================================
            modelBuilder.Entity<Accommodation>()
                .HasDiscriminator<string>("AccommodationType")
                .HasValue<Hotel>("Hotel")
                .HasValue<LocalLoding>("LocalLoding")
                .HasValue<StudentHouse>("StudentHouse");

            // ========================================================
            // AppUser Relations
            // ========================================================
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasMany(u => u.Accommodations)
                      .WithOne(a => a.AppUser)
                      .HasForeignKey(a => a.AppUserID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Bookings)
                      .WithOne(b => b.AppUser)
                      .HasForeignKey(b => b.AppUserID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Favorites)
                      .WithOne(f => f.AppUser)
                      .HasForeignKey(f => f.AppUserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================================
            // Accommodation Relations
            // ========================================================
            modelBuilder.Entity<Accommodation>(entity =>
            {
                // M:N ==> Favorite
                entity.HasMany(a => a.Favorites)
                      .WithMany(f => f.Accommodations)
                      .UsingEntity<Dictionary<string, object>>(
                          "AccommodationFavorite",
                          j => j
                                .HasOne<Favorite>()
                                .WithMany()
                                .HasForeignKey("FavoriteID")
                                .OnDelete(DeleteBehavior.Restrict),
                          j => j
                                .HasOne<Accommodation>()
                                .WithMany()
                                .HasForeignKey("AccommodationID")
                                .OnDelete(DeleteBehavior.Restrict));

                // M:N ==> Amenities
                entity.HasMany(a => a.Amenities)
                      .WithMany(am => am.Accommodations)
                      .UsingEntity<Dictionary<string, object>>(
                          "AccommodationAmenity",
                          j => j
                                .HasOne<Amenity>()
                                .WithMany()
                                .HasForeignKey("AmenityID")
                                .OnDelete(DeleteBehavior.Restrict),
                          j => j
                                .HasOne<Accommodation>()
                                .WithMany()
                                .HasForeignKey("AccommodationID")
                                .OnDelete(DeleteBehavior.Restrict));

                // 1:M ==> Images
                entity.HasMany(a => a.Images)
                      .WithOne(i => i.Accommodation)
                      .HasForeignKey(i => i.AccommodationID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // Governorate Relations
            // ========================================================
            modelBuilder.Entity<Governorate>(entity =>
            {
                entity.HasMany(g => g.Cities)
                      .WithOne(c => c.Governorate)
                      .HasForeignKey(c => c.GovernorateID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // City Relations
            // ========================================================
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasMany(c => c.Locations)
                      .WithOne(l => l.City)
                      .HasForeignKey(l => l.CityID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // Location Relations
            // ========================================================
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasMany(l => l.Accommodations)
                      .WithOne(a => a.Location)
                      .HasForeignKey(a => a.LocationID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // Booking Relations
            // ========================================================
            modelBuilder.Entity<Booking>(entity =>
            {
                // M:N ==> LocalLoding
                entity.HasMany(b => b.LocalLodings)
                      .WithMany(l => l.Bookings)
                      .UsingEntity<Dictionary<string, object>>(
                          "BookingLocalLoding",
                          j => j.HasOne<LocalLoding>().WithMany().HasForeignKey("LocalLodingID").OnDelete(DeleteBehavior.Restrict),
                          j => j.HasOne<Booking>().WithMany().HasForeignKey("BookingID").OnDelete(DeleteBehavior.Cascade));

                // M:N ==> HotelRoom
                entity.HasMany(b => b.HotelRooms)
                      .WithMany(r => r.Bookings)
                      .UsingEntity<Dictionary<string, object>>(
                          "BookingHotelRoom",
                          j => j.HasOne<HotelRoom>().WithMany().HasForeignKey("HotelRoomID").OnDelete(DeleteBehavior.Restrict),
                          j => j.HasOne<Booking>().WithMany().HasForeignKey("BookingID").OnDelete(DeleteBehavior.Cascade));

                // M:N ==> Bed
                entity.HasMany(b => b.Beds)
                      .WithMany(r => r.Bookings)
                      .UsingEntity<Dictionary<string, object>>(
                          "BookingBed",
                          j => j.HasOne<Bed>().WithMany().HasForeignKey("BedID").OnDelete(DeleteBehavior.Restrict),
                          j => j.HasOne<Booking>().WithMany().HasForeignKey("BookingID").OnDelete(DeleteBehavior.Cascade));

                // 1:1 ==> Payment
                entity.HasOne(b => b.Payment)
                      .WithOne(p => p.Booking)
                      .HasForeignKey<Payment>(p => p.BookingID)
                      .OnDelete(DeleteBehavior.Cascade);

                // 1:1 ==> Review
                entity.HasOne(b => b.Review)
                      .WithOne(r => r.Booking)
                      .HasForeignKey<Review>(r => r.BookingID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================================
            // Hotel Relations
            // ========================================================
            modelBuilder.Entity<Hotel>(entity =>
            {
                entity.HasMany(h => h.HotelRooms)
                      .WithOne(r => r.Hotel)
                      .HasForeignKey(r => r.AccommodationID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // HotelRoom Relations
            // ========================================================
            modelBuilder.Entity<HotelRoom>(entity =>
            {
                entity.HasMany(r => r.Images)
                      .WithMany(i => i.HotelRooms)
                      .UsingEntity<Dictionary<string, object>>(
                          "HotelRoomImage",
                          j => j.HasOne<Image>().WithMany().HasForeignKey("ImageID").OnDelete(DeleteBehavior.Restrict),
                          j => j.HasOne<HotelRoom>().WithMany().HasForeignKey("HotelRoomID").OnDelete(DeleteBehavior.Cascade));
            });

            // ========================================================
            // StudentHouse Relations
            // ========================================================
            modelBuilder.Entity<StudentHouse>(entity =>
            {
                entity.HasMany(h => h.StudentRooms)
                      .WithOne(r => r.StudentHouse)
                      .HasForeignKey(r => r.AccommodationID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================================
            // StudentRoom Relations
            // ========================================================
            modelBuilder.Entity<StudentRoom>(entity =>
            {
                entity.HasMany(r => r.Beds)
                      .WithOne(b => b.StudentRoom)
                      .HasForeignKey(b => b.StudentRoomID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

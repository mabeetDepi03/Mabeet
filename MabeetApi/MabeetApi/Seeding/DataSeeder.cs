using MabeetApi.Data;
using MabeetApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MabeetApi.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            await context.Database.MigrateAsync();

            // ============================
            // 1) Seed Users (All Active)
            // ============================
            if (!context.Users.Any())
            {
                var admin = new AppUser
                {
                    UserName = "admin@mabeet.com",
                    Email = "admin@mabeet.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    NationalID = "10000000000000",
                    PhoneNumber = "01000000000",
                    type = UserRole.Admin,
                    IsActive = true
                };
                await userManager.CreateAsync(admin, "Admin@123");

                var owner = new AppUser
                {
                    UserName = "owner@mabeet.com",
                    Email = "owner@mabeet.com",
                    FirstName = "Hotel",
                    LastName = "Owner",
                    NationalID = "20000000000000",
                    PhoneNumber = "01011111111",
                    type = UserRole.Owner,
                    IsActive = true
                };
                await userManager.CreateAsync(owner, "Owner@123");

                var client1 = new AppUser
                {
                    UserName = "client1@mabeet.com",
                    Email = "client1@mabeet.com",
                    FirstName = "Ali",
                    LastName = "Hassan",
                    NationalID = "30000000000000",
                    PhoneNumber = "01022222222",
                    type = UserRole.Client,
                    IsActive = true
                };
                await userManager.CreateAsync(client1, "Client@123");

                var client2 = new AppUser
                {
                    UserName = "client2@mabeet.com",
                    Email = "client2@mabeet.com",
                    FirstName = "Sara",
                    LastName = "Mohamed",
                    NationalID = "40000000000000",
                    PhoneNumber = "01033333333",
                    type = UserRole.Client,
                    IsActive = true
                };
                await userManager.CreateAsync(client2, "Client@123");
            }

            await context.SaveChangesAsync();

            // ============================
            // 2) Governorates + Cities
            // ============================
            if (!context.Governorates.Any())
            {
                var govs = new List<Governorate>
                {
                    new Governorate { GovernorateName = "Cairo" },
                    new Governorate { GovernorateName = "Giza" },
                    new Governorate { GovernorateName = "Alexandria" },
                    new Governorate { GovernorateName = "Mansoura" }
                };

                context.Governorates.AddRange(govs);
                await context.SaveChangesAsync();

                var cities = new List<City>
                {
                    new City { CityName = "Nasr City", GovernorateID = govs[0].GovernorateID },
                    new City { CityName = "Maadi", GovernorateID = govs[0].GovernorateID },
                    new City { CityName = "Dokki", GovernorateID = govs[1].GovernorateID },
                    new City { CityName = "Haram", GovernorateID = govs[1].GovernorateID },
                    new City { CityName = "Smouha", GovernorateID = govs[2].GovernorateID },
                    new City { CityName = "Miami", GovernorateID = govs[2].GovernorateID },
                    new City { CityName = "Talkha", GovernorateID = govs[3].GovernorateID },
                    new City { CityName = "Toriel", GovernorateID = govs[3].GovernorateID }
                };

                context.Cities.AddRange(cities);
                await context.SaveChangesAsync();

                // ============================
                // 3) Locations
                // ============================
                var locations = new List<Location>
                {
                    new Location { Region = "District 7", Street = "Main Street", CityID = cities[0].CityID },
                    new Location { Region = "Corniche", Street = "Nile Street", CityID = cities[1].CityID },
                    new Location { Region = "El Tahrir", Street = "Opera Street", CityID = cities[2].CityID },
                    new Location { Region = "Faisal", Street = "El Bata Street", CityID = cities[3].CityID },
                    new Location { Region = "Smouha Center", Street = "Victoria Road", CityID = cities[4].CityID },
                    new Location { Region = "Miami Beach", Street = "Sea Street", CityID = cities[5].CityID },
                    new Location { Region = "Talkha Square", Street = "Main Road", CityID = cities[6].CityID },
                    new Location { Region = "Toriel Park", Street = "Family Street", CityID = cities[7].CityID }
                };

                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();

                // ============================
                // 4) Hotels
                // ============================
                var owner = context.Users.First(x => x.type == UserRole.Owner);

                var hotels = new List<Hotel>
                {
                    new Hotel { AccommodationName = "Grand Nile Hotel", StarsRate = 5, AccommodationDescription = "Luxury 5-star hotel", LocationID = locations[0].LocationID, AppUserID = owner.Id, AccommodationType="Hotel" },
                    new Hotel { AccommodationName = "City View Hotel", StarsRate = 4, AccommodationDescription = "City center hotel", LocationID = locations[1].LocationID, AppUserID = owner.Id, AccommodationType="Hotel" },
                    new Hotel { AccommodationName = "Royal Plaza", StarsRate = 4, AccommodationDescription = "Modern rooms & services", LocationID = locations[2].LocationID, AppUserID = owner.Id, AccommodationType="Hotel" },
                    new Hotel { AccommodationName = "Sea Breeze", StarsRate = 3, AccommodationDescription = "Sea view rooms", LocationID = locations[5].LocationID, AppUserID = owner.Id, AccommodationType="Hotel" },
                    new Hotel { AccommodationName = "Delta Inn", StarsRate = 3, AccommodationDescription = "Budget friendly stay", LocationID = locations[7].LocationID, AppUserID = owner.Id, AccommodationType="Hotel" }
                };

                context.Hotels.AddRange(hotels);
                await context.SaveChangesAsync();

                // Rooms
                foreach (var hotel in hotels)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        context.HotelRooms.Add(new HotelRoom
                        {
                            RoomNumber = i,
                            Type = RoomType.Double,
                            PricePerNight = 500 + i * 100,
                            RoomDescription = $"Room {i} at {hotel.AccommodationName}",
                            AccommodationID = hotel.AccommodationID,
                            IsAvailable = true
                        });
                    }
                }

                await context.SaveChangesAsync();

                // ============================
                // 5) Local Lodgings (Apartments)
                // ============================
                var local = new List<LocalLoding>
                {
                    new LocalLoding { AccommodationName="Apartment 101", Area=120, Floor=5, TotalRooms=3, TotalGuests=4, PricePerNight=400, IsAvailable=true, LocationID=locations[3].LocationID, AppUserID=owner.Id, AccommodationDescription="Nice family apartment", AccommodationType="LocalLoding" },
                    new LocalLoding { AccommodationName="Apartment 203", Area=95, Floor=2, TotalRooms=2, TotalGuests=3, PricePerNight=300, IsAvailable=true, LocationID=locations[4].LocationID, AppUserID=owner.Id, AccommodationDescription="Cozy apartment", AccommodationType="LocalLoding" },
                    new LocalLoding { AccommodationName="Studio 11", Area=50, Floor=1, TotalRooms=1, TotalGuests=2, PricePerNight=200, IsAvailable=true, LocationID=locations[1].LocationID, AppUserID=owner.Id, AccommodationDescription="Small studio", AccommodationType="LocalLoding" },
                    new LocalLoding { AccommodationName="Studio 9B", Area=65, Floor=4, TotalRooms=1, TotalGuests=2, PricePerNight=250, IsAvailable=true, LocationID=locations[5].LocationID, AppUserID=owner.Id, AccommodationDescription="Sea view studio", AccommodationType="LocalLoding" },
                    new LocalLoding { AccommodationName="Apartment 88", Area=115, Floor=6, TotalRooms=3, TotalGuests=5, PricePerNight=450, IsAvailable=true, LocationID=locations[6].LocationID, AppUserID=owner.Id, AccommodationDescription="Spacious apartment", AccommodationType="LocalLoding" },
                    new LocalLoding { AccommodationName="Apartment 52", Area=105, Floor=3, TotalRooms=3, TotalGuests=4, PricePerNight=380, IsAvailable=true, LocationID=locations[7].LocationID, AppUserID=owner.Id, AccommodationDescription="Clean apartment", AccommodationType="LocalLoding" }
                };

                context.LocalLodings.AddRange(local);
                await context.SaveChangesAsync();

                // ============================
                // 6) Student Houses + Rooms + Beds
                // ============================
                var studentHouses = new List<StudentHouse>
                {
                    new StudentHouse { AccommodationName="Student House A", Area=300, Floor=5, TotalGuests=20, LocationID=locations[2].LocationID, AppUserID=owner.Id, AccommodationDescription="Male dormitory", AccommodationType="StudentHouse" },
                    new StudentHouse { AccommodationName="Student House B", Area=250, Floor=3, TotalGuests=15, LocationID=locations[4].LocationID, AppUserID=owner.Id, AccommodationDescription="Female dormitory", AccommodationType="StudentHouse" },
                    new StudentHouse { AccommodationName="Student House C", Area=280, Floor=4, TotalGuests=18, LocationID=locations[6].LocationID, AppUserID=owner.Id, AccommodationDescription="Mixed dormitory", AccommodationType="StudentHouse" },
                    new StudentHouse { AccommodationName="Student House D", Area=260, Floor=4, TotalGuests=16, LocationID=locations[0].LocationID, AppUserID=owner.Id, AccommodationDescription="Economy dormitory", AccommodationType="StudentHouse" }
                };

                context.StudentHouses.AddRange(studentHouses);
                await context.SaveChangesAsync();

                foreach (var sh in studentHouses)
                {
                    for (int r = 1; r <= 3; r++)
                    {
                        var room = new StudentRoom
                        {
                            TotalBeds = 2,
                            AccommodationID = sh.AccommodationID
                        };

                        context.StudentRooms.Add(room);
                        await context.SaveChangesAsync();

                        for (int b = 1; b <= 2; b++)
                        {
                            context.Beds.Add(new Bed
                            {
                                RoomDescription = $"Bed {b} in Room {r}",
                                PricePerNight = 80,
                                StudentRoomID = room.StudentRoomID,
                                IsAvailable = true
                            });
                        }
                    }
                }

                await context.SaveChangesAsync();
            }

            // ============================
            // 7) Amenities
            // ============================
            if (!context.Amenities.Any())
            {
                var am = new List<Amenity>
                {
                    new Amenity { AmenityName="Free WiFi" },
                    new Amenity { AmenityName="Parking" },
                    new Amenity { AmenityName="Air Conditioning" },
                    new Amenity { AmenityName="TV" },
                    new Amenity { AmenityName="Kitchen" },
                    new Amenity { AmenityName="Elevator" },
                    new Amenity { AmenityName="Heater" },
                    new Amenity { AmenityName="Washer" },
                    new Amenity { AmenityName="Dryer" },
                    new Amenity { AmenityName="Swimming Pool" }
                };

                context.Amenities.AddRange(am);
            }

            await context.SaveChangesAsync();
        }
    }
}

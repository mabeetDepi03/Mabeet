using MabeetApi.Data;
using MabeetApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MabeetApi.Seeding
{
    public static class DataSeeder
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            await SeedGeographicData(context);

            // 2. مستخدمين (بدون Identity)
            await SeedUsers(context);

            // 3. عقارات
            await SeedAccommodations(context);

            // 4. حجوزات
            await SeedBookings(context);

            Console.WriteLine("✅ Data seeding completed successfully! No authentication required.");
        }

        private static async Task SeedUsers(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new List<AppUser>();
                var random = new Random();

                // إنشاء 30 مستخدم بدون Identity
                for (int i = 1; i <= 30; i++)
                {
                    var userRole = i switch
                    {
                        1 => UserRole.Admin,
                        <= 10 => UserRole.Owner,
                        _ => UserRole.Client
                    };

                    var user = new AppUser
                    {
                        UserName = $"user{i}",
                        Email = $"user{i}@mabeet.com",
                        FirstName = GetRandomArabicFirstName(random),
                        LastName = GetRandomArabicLastName(random),
                        NationalID = GenerateNationalID(random),
                        RoleType = userRole,
                        CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                        PasswordHash = $"hashed_password_{i}" // كلمة مرور وهمية للاختبار
                    };

                    users.Add(user);
                }

                context.Users.AddRange(users);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedGeographicData(AppDbContext context)
        {
            if (!context.Governorates.Any())
            {
                var governorates = new[]
                {
                    "القاهرة", "الجيزة", "الإسكندرية", "الدقهلية", "البحر الأحمر", "البحيرة", "الفيوم",
                    "الغربية", "الإسماعيلية", "المنوفية", "المنيا", "القليوبية", "الوادي الجديد",
                    "السويس", "أسوان", "أسيوط", "بني سويف", "بورسعيد", "دمياط", "سوهاج",
                    "جنوب سيناء", "كفر الشيخ", "مطروح", "الأقصر", "قنا", "شمال سيناء", "الشرقية",
                    "الغردقة", "مرسى مطروح", "الزقازيق"
                }.Select((name, index) => new Governorate
                {
                    GovernorateName = name
                }).ToList();

                context.Governorates.AddRange(governorates);
                await context.SaveChangesAsync();

                // 30 مدينة
                var cities = new List<City>();
                var random = new Random();
                for (int i = 0; i < 30; i++)
                {
                    cities.Add(new City
                    {
                        CityName = $"مدينة {i + 1}",
                        GovernorateID = governorates[i].GovernorateID
                    });
                }

                context.Cities.AddRange(cities);
                await context.SaveChangesAsync();

                // 30 منطقة
                var locations = new List<Location>();
                var regions = new[] { "الشمال", "الجنوب", "الشرق", "الغرب", "الوسط" };
                var streets = new[] { "التحرير", "النصر", "الحرية", "الاستقلال", "الشهداء", "الجامعة", "النهضة" };

                for (int i = 0; i < 30; i++)
                {
                    locations.Add(new Location
                    {
                        Region = $"{regions[random.Next(regions.Length)]} - منطقة {i + 1}",
                        Street = $"شارع {streets[random.Next(streets.Length)]}",
                        CityID = cities[i].CityID
                    });
                }

                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAccommodations(AppDbContext context)
        {
            if (!context.Accommodations.Any())
            {
                var users = await context.Users.Where(u => u.RoleType == UserRole.Owner).ToListAsync();
                var locations = await context.Locations.Take(30).ToListAsync();
                var random = new Random();

                var accommodations = new List<Accommodation>();

                for (int i = 0; i < 30; i++)
                {
                    var owner = users[random.Next(users.Count)];
                    var location = locations[i];

                    Accommodation accommodation;

                    if (i < 15) // 15 شقة
                    {
                        accommodation = new LocalLoding
                        {
                            AccommodationName = $"شقة فاخرة {i + 1}",
                            AccommodationDescription = $"شقة جميلة في {location.Region}",
                            AppUserID = owner.Id,
                            LocationID = location.LocationID,
                            Area = random.Next(70, 200),
                            Floor = random.Next(1, 15),
                            TotalRooms = random.Next(1, 5),
                            TotalGuests = random.Next(2, 8),
                            PricePerNight = random.Next(200, 1000),
                            IsAvailable = random.Next(2) == 1,
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 180))
                        };
                    }
                    else if (i < 25) // 10 فنادق
                    {
                        accommodation = new Hotel
                        {
                            AccommodationName = $"فندق {GetRandomHotelName(random)}",
                            AccommodationDescription = $"فندق {random.Next(3, 6)} نجوم",
                            AppUserID = owner.Id,
                            LocationID = location.LocationID,
                            StarsRate = random.Next(3, 6),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 180))
                        };
                    }
                    else // 5 مساكن طلابية
                    {
                        accommodation = new StudentHouse
                        {
                            AccommodationName = $"سكن طلاب {i + 1}",
                            AccommodationDescription = "سكن طلابي آمن ونظيف",
                            AppUserID = owner.Id,
                            LocationID = location.LocationID,
                            Area = random.Next(150, 400),
                            Floor = random.Next(1, 5),
                            TotalGuests = random.Next(10, 50),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 180))
                        };
                    }

                    accommodations.Add(accommodation);
                }

                context.Accommodations.AddRange(accommodations);
                await context.SaveChangesAsync();

                await SeedHotelRoomsAndBeds(context);
            }
        }

        private static async Task SeedHotelRoomsAndBeds(AppDbContext context)
        {
            var hotels = await context.Hotels.ToListAsync();
            var studentHouses = await context.StudentHouses.ToListAsync();
            var random = new Random();

            // غرف الفنادق
            var hotelRooms = new List<HotelRoom>();
            foreach (var hotel in hotels)
            {
                for (int i = 1; i <= 10; i++) // 10 غرف لكل فندق
                {
                    hotelRooms.Add(new HotelRoom
                    {
                        RoomNumber = i * 100 + random.Next(1, 20),
                        Type = (RoomType)random.Next(0, 3),
                        RoomDescription = $"غرفة {GetRoomTypeArabic((RoomType)random.Next(0, 3))} فاخرة",
                        PricePerNight = random.Next(150, 800),
                        IsAvailable = random.Next(2) == 1,
                        AccommodationID = hotel.AccommodationID
                    });
                }
            }
            context.HotelRooms.AddRange(hotelRooms);

            // أسرة المساكن الطلابية
            var studentRooms = new List<StudentRoom>();
            var beds = new List<Bed>();

            foreach (var house in studentHouses)
            {
                for (int i = 1; i <= 5; i++) // 5 غرف لكل سكن
                {
                    var studentRoom = new StudentRoom
                    {
                        TotalBeds = random.Next(2, 6),
                        AccommodationID = house.AccommodationID
                    };
                    studentRooms.Add(studentRoom);
                }
            }

            context.StudentRooms.AddRange(studentRooms);
            await context.SaveChangesAsync();

            // إضافة الأسرة
            foreach (var room in studentRooms)
            {
                for (int i = 1; i <= room.TotalBeds; i++)
                {
                    beds.Add(new Bed
                    {
                        RoomDescription = $"سرير {i} - غرفة {room.StudentRoomID}",
                        PricePerNight = random.Next(50, 150),
                        IsAvailable = random.Next(2) == 1,
                        StudentRoomID = room.StudentRoomID
                    });
                }
            }
            context.Beds.AddRange(beds);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBookings(AppDbContext context)
        {
            if (!context.Bookings.Any())
            {
                var clients = await context.Users.Where(u => u.RoleType == UserRole.Client).ToListAsync();
                var localLodings = await context.LocalLodings.ToListAsync();
                var hotelRooms = await context.HotelRooms.ToListAsync();
                var beds = await context.Beds.Where(b => b.IsAvailable).ToListAsync();
                var random = new Random();

                var bookings = new List<Booking>();

                for (int i = 0; i < 30; i++)
                {
                    var client = clients[random.Next(clients.Count)];
                    var checkIn = DateTime.Now.AddDays(-random.Next(1, 365));
                    var checkOut = checkIn.AddDays(random.Next(1, 14));
                    var nights = (checkOut - checkIn).Days;

                    var booking = new Booking
                    {
                        CheckIN = checkIn,
                        CheckOUT = checkOut,
                        TotalPrice = random.Next(500, 5000),
                        AppUserID = client.Id,
                        Status = GetRandomStatus(random),
                        CreatedAt = checkIn.AddDays(-random.Next(1, 30))
                    };

                    bookings.Add(booking);
                }

                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();

                // ربط الحجوزات بالإقامات
                await LinkBookingsToAccommodations(context, bookings, localLodings, hotelRooms, beds);
            }
        }

        private static async Task LinkBookingsToAccommodations(AppDbContext context,
            List<Booking> bookings, List<LocalLoding> localLodings,
            List<HotelRoom> hotelRooms, List<Bed> beds)
        {
            var random = new Random();

            foreach (var booking in bookings)
            {
                var accommodationType = random.Next(0, 3);

                switch (accommodationType)
                {
                    case 0 when localLodings.Any():
                        booking.LocalLodings.Add(localLodings[random.Next(localLodings.Count)]);
                        break;
                    case 1 when hotelRooms.Any():
                        booking.HotelRooms.Add(hotelRooms[random.Next(hotelRooms.Count)]);
                        break;
                    case 2 when beds.Any():
                        booking.Beds.Add(beds[random.Next(beds.Count)]);
                        break;
                }
            }

            await context.SaveChangesAsync();
        }

        #region Helper Methods
        private static string GetRandomArabicFirstName(Random random)
        {
            var firstNames = new[] { "أحمد", "محمد", "محمود", "علي", "خالد", "ياسر", "مصطفى", "حسن",
                                   "سارة", "فاطمة", "آية", "مريم", "هناء", "نور", "ريم" };
            return firstNames[random.Next(firstNames.Length)];
        }

        private static string GetRandomArabicLastName(Random random)
        {
            var lastNames = new[] { "محمد", "علي", "حسن", "إبراهيم", "عمر", "سعيد", "كامل", "شعبان",
                                  "خالد", "موسى", "ناصر", "عادل", "رجب", "فاروق" };
            return lastNames[random.Next(lastNames.Length)];
        }

        private static string GenerateNationalID(Random random)
        {
            return new string(Enumerable.Range(0, 14)
                .Select(_ => (char)('0' + random.Next(10)))
                .ToArray());
        }

        private static string GetRandomHotelName(Random random)
        {
            var names = new[] { "النيل", "الأهرام", "الحرية", "السلام", "الفردوس", "الرياض", "الوحدة", "الكرامة" };
            return names[random.Next(names.Length)];
        }

        private static string GetRoomTypeArabic(RoomType type)
        {
            return type switch
            {
                RoomType.Single => "فردي",
                RoomType.Double => "دبل",

            };
        }

        private static string GetRandomStatus(Random random)
        {
            var statuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
            return statuses[random.Next(statuses.Length)];
        }
        #endregion
    }
}
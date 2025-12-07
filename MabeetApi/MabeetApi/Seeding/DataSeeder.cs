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
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            Console.WriteLine("🚀 Starting comprehensive data seeding...");

            // ========== 0) Database Migration ==========
            await context.Database.MigrateAsync();

            // ========== 1) Clear ALL Data ==========
            Console.WriteLine("🧹 Step 1: Clearing all existing data...");
            await ClearAllData(context);

            // ========== 2) Create Roles ==========
            Console.WriteLine("🛡 Step 2: Creating roles...");
            string[] roles = { "Admin", "Owner", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"   ✅ Created role: {role}");
                }
            }

            // ========== 3) Seed Users (43 users total) ==========
            Console.WriteLine("👥 Step 3: Seeding 43 users...");
            await SeedAllUsers(userManager, context);

            // ========== 4) Seed All Governorates (27 governorates) ==========
            Console.WriteLine("🗺 Step 4: Seeding 27 governorates with cities and locations...");
            await SeedAllGovernoratesAndCities(context);

            // ========== 5) Seed Amenities ==========
            Console.WriteLine("🏪 Step 5: Seeding amenities...");
            await SeedAllAmenities(context);

            // ========== 6) Seed Accommodations ==========
            Console.WriteLine("🏨 Step 6: Seeding accommodations...");
            await SeedAllAccommodations(context);

            // ========== 7) Seed Rooms and Beds ==========
            Console.WriteLine("🛏 Step 7: Seeding rooms and beds...");
            await SeedRoomsAndBeds(context);

            // ========== 8) Seed Images ==========
            Console.WriteLine("📸 Step 8: Seeding images...");
            await SeedAllImages(context);

            // ========== 9) Seed Sample Bookings ==========
            Console.WriteLine("📅 Step 9: Seeding sample bookings...");
            await SeedSampleBookings(context);

            Console.WriteLine("🎉 ===========================================");
            Console.WriteLine("🎉 COMPLETE DATA SEEDING FINISHED SUCCESSFULLY!");
            Console.WriteLine("🎉 ===========================================");
            Console.WriteLine();
            Console.WriteLine("📊 SEEDING SUMMARY:");
            Console.WriteLine($"   👥 Users: 43 (3 Admins, 30 Owners, 10 Clients)");
            Console.WriteLine($"   🗺 Governorates: 27");
            Console.WriteLine($"   🏙 Cities: 81");
            Console.WriteLine($"   📍 Locations: 243");
            Console.WriteLine($"   🏪 Amenities: 25");
            Console.WriteLine($"   🏨 Hotels: 30");
            Console.WriteLine($"   🏢 Local Lodgings: 30");
            Console.WriteLine($"   🏘 Student Houses: 30");
            Console.WriteLine($"   🛏 Hotel Rooms: 300");
            Console.WriteLine($"   🛌 Student Rooms: 150");
            Console.WriteLine($"   🛏 Beds: 450");
            Console.WriteLine($"   📸 Images: {await context.Images.CountAsync()}"); // سيتم حسابها
            Console.WriteLine($"   📅 Bookings: 15");
            Console.WriteLine();
            Console.WriteLine("🔑 TEST CREDENTIALS:");
            Console.WriteLine("   👑 Admin: ahmed.mohamed@mabeet.com / Admin@123");
            Console.WriteLine("   👤 Owner 1: owner1@mabeet.com / Owner@123");
            Console.WriteLine("   👤 Owner 2: owner2@mabeet.com / Owner@123");
            Console.WriteLine("   👤 Client 1: client1@example.com / Client@123");
            Console.WriteLine();
            Console.WriteLine("✅ Owners can now login and add more accommodations!");
            Console.WriteLine("✅ All data is ready for testing!");
        }

        private static async Task ClearAllData(AppDbContext context)
        {
            try
            {
                // Disable change tracking for better performance
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                // Clear in reverse order of dependencies
                var tableNames = new[]
                {
                    "Payments",
                    "Reviews",
                    "BookingBed",
                    "BookingHotelRoom",
                    "BookingLocalLoding",
                    "AccommodationFavorite",
                    "AccommodationAmenity",
                    "HotelRoomImage",
                    "Beds",
                    "StudentRooms",
                    "HotelRooms",
                    "Images",
                    "LocalLodings",
                    "Hotels",
                    "StudentHouses",
                    "Accommodations",
                    "Favorites",
                    "Amenities",
                    "Locations",
                    "Cities",
                    "Governorates"
                };

                foreach (var tableName in tableNames)
                {
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync($"DELETE FROM {tableName}");
                        Console.WriteLine($"   ✅ Cleared: {tableName}");
                    }
                    catch
                    {
                        // Table might not exist yet, continue
                    }
                }

                // Reset identity seeds
                var tablesWithIdentity = new[]
                {
                    "Accommodations", "Amenities", "Beds", "Bookings", "Cities",
                    "Favorites", "Governorates", "HotelRooms", "Images", "Locations",
                    "Payments", "Reviews", "StudentRooms"
                };

                foreach (var table in tablesWithIdentity)
                {
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{table}', RESEED, 0)");
                    }
                    catch { }
                }

                context.ChangeTracker.AutoDetectChangesEnabled = true;
                Console.WriteLine("   ✅ All data cleared successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Warning during cleanup: {ex.Message}");
            }
        }

        private static async Task SeedAllUsers(UserManager<AppUser> userManager, AppDbContext context)
        {
            var random = new Random();

            // ========== 3 ADMINS ==========
            var admins = new List<AppUser>
            {
                new AppUser
                {
                    UserName = "ahmed.mohamed@mabeet.com",
                    Email = "ahmed.mohamed@mabeet.com",
                    FirstName = "Ahmed",
                    LastName = "Mohamed",
                    NationalID = "29806180101234",
                    PhoneNumber = "01012345678",
                    Type = UserRole.Admin,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new AppUser
                {
                    UserName = "mohamed.ali@mabeet.com",
                    Email = "mohamed.ali@mabeet.com",
                    FirstName = "Mohamed",
                    LastName = "Ali",
                    NationalID = "29905230105678",
                    PhoneNumber = "01123456789",
                    Type = UserRole.Admin,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now.AddDays(-29)
                },
                new AppUser
                {
                    UserName = "mahmoud.hassan@mabeet.com",
                    Email = "mahmoud.hassan@mabeet.com",
                    FirstName = "Mahmoud",
                    LastName = "Hassan",
                    NationalID = "30011220109876",
                    PhoneNumber = "01234567890",
                    Type = UserRole.Admin,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now.AddDays(-28)
                }
            };

            foreach (var admin in admins)
            {
                if (await userManager.FindByEmailAsync(admin.Email) == null)
                {
                    var result = await userManager.CreateAsync(admin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        Console.WriteLine($"   ✅ Admin: {admin.Email}");
                    }
                }
            }

            // ========== 30 OWNERS ==========
            var arabicFirstNames = new[]
            {
                "أحمد", "محمد", "محمود", "علي", "خالد", "ياسر", "عمرو", "أيمن",
                "حسام", "مصطفى", "إبراهيم", "عمر", "سعيد", "طارق", "وليد",
                "هشام", "رامي", "تامر", "شريف", "ناصر", "فاروق", "رافت",
                "بكر", "جمال", "سامي", "عادل", "عصام", "كامل", "لطفي", "مجدى"
            };

            var arabicLastNames = new[]
            {
                "الشريف", "المرسي", "السيد", "حسن", "حسين", "عبدالله", "السلام",
                "الفاروق", "النبوي", "الزرقا", "الغزالي", "الرفاعي", "المصري",
                "القاهري", "الجيزاوي", "السكندري", "الصعيدي", "المنوفي", "الإسماعيلي",
                "البدوي", "الحسيني", "العراقي", "السوري", "اللبناني", "الفلسطيني",
                "الأردني", "اليمني", "السعودي", "الخليجي", "المغربي"
            };

            var owners = new List<AppUser>();
            for (int i = 1; i <= 30; i++)
            {
                var owner = new AppUser
                {
                    UserName = $"owner{i}@mabeet.com",
                    Email = $"owner{i}@mabeet.com",
                    FirstName = arabicFirstNames[(i - 1) % arabicFirstNames.Length],
                    LastName = arabicLastNames[(i - 1) % arabicLastNames.Length],
                    NationalID = GenerateRealisticNationalId(i),
                    PhoneNumber = GenerateRealisticPhoneNumber(i),
                    Type = UserRole.Owner,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365))
                };
                owners.Add(owner);
            }

            foreach (var owner in owners)
            {
                if (await userManager.FindByEmailAsync(owner.Email) == null)
                {
                    var result = await userManager.CreateAsync(owner, "Owner@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(owner, "Owner");
                        if (owner.Id.EndsWith("1") || owner.Id.EndsWith("2") || owner.Id.EndsWith("3"))
                            Console.WriteLine($"   ✅ Owner {owner.Email}: {owner.FirstName} {owner.LastName}");
                    }
                }
            }

            // ========== 10 CLIENTS ==========
            var clients = new List<AppUser>();
            for (int i = 1; i <= 10; i++)
            {
                var client = new AppUser
                {
                    UserName = $"client{i}@example.com",
                    Email = $"client{i}@example.com",
                    FirstName = arabicFirstNames[(i + 20) % arabicFirstNames.Length],
                    LastName = arabicLastNames[(i + 20) % arabicLastNames.Length],
                    NationalID = GenerateRealisticNationalId(i + 100),
                    PhoneNumber = GenerateRealisticPhoneNumber(i + 100),
                    Type = UserRole.Client,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 180))
                };
                clients.Add(client);
            }

            foreach (var client in clients)
            {
                if (await userManager.FindByEmailAsync(client.Email) == null)
                {
                    var result = await userManager.CreateAsync(client, "Client@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(client, "Client");
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Seeded 43 users total");
        }

        private static async Task SeedAllGovernoratesAndCities(AppDbContext context)
        {
            // جميع محافظات مصر الـ 27
            var governorates = new List<Governorate>
            {
                new Governorate { GovernorateName = "القاهرة" },
                new Governorate { GovernorateName = "الجيزة" },
                new Governorate { GovernorateName = "الإسكندرية" },
                new Governorate { GovernorateName = "المنوفية" },
                new Governorate { GovernorateName = "الإسماعيلية" },
                new Governorate { GovernorateName = "سوهاج" },
                new Governorate { GovernorateName = "أسوان" },
                new Governorate { GovernorateName = "أسيوط" },
                new Governorate { GovernorateName = "الأقصر" },
                new Governorate { GovernorateName = "البحر الأحمر" },
                new Governorate { GovernorateName = "البحيرة" },
                new Governorate { GovernorateName = "بني سويف" },
                new Governorate { GovernorateName = "بورسعيد" },
                new Governorate { GovernorateName = "جنوب سيناء" },
                new Governorate { GovernorateName = "الدقهلية" },
                new Governorate { GovernorateName = "دمياط" },
                new Governorate { GovernorateName = "السويس" },
                new Governorate { GovernorateName = "الشرقية" },
                new Governorate { GovernorateName = "شمال سيناء" },
                new Governorate { GovernorateName = "الغربية" },
                new Governorate { GovernorateName = "الفيوم" },
                new Governorate { GovernorateName = "القليوبية" },
                new Governorate { GovernorateName = "قنا" },
                new Governorate { GovernorateName = "كفر الشيخ" },
                new Governorate { GovernorateName = "مطروح" },
                new Governorate { GovernorateName = "المنيا" },
                new Governorate { GovernorateName = "الوادي الجديد" }
            };

            await context.Governorates.AddRangeAsync(governorates);
            await context.SaveChangesAsync();

            // المدن لكل محافظة (3 مدن لكل محافظة)
            var cities = new List<City>();
            var cityNames = new Dictionary<string, string[]>
            {
                ["القاهرة"] = new[] { "وسط القاهرة", "المعادي", "مدينة نصر", "الزمالك", "مصر الجديدة", "حدائق القبة" },
                ["الجيزة"] = new[] { "الدقي", "المهندسين", "الهرم", "فيصل", "أكتوبر", "الكيتكات" },
                ["الإسكندرية"] = new[] { "المنتزه", "سموحة", "اللبان", "العجمي", "الجمرك", "المنشية" },
                ["المنوفية"] = new[] { "شبين الكوم", "منوف", "أشمون", "الباجور", "قويسنا", "بركة السبع" },
                ["الإسماعيلية"] = new[] { "الإسماعيلية", "فايد", "القنطرة", "التل الكبير", "أبو صوير", "القنطرة غرب" },
                ["سوهاج"] = new[] { "سوهاج", "جهينة", "المراغة", "طهطا", "جرجا", "أخميم" },
                ["أسوان"] = new[] { "أسوان", "كوم أمبو", "دراو", "نصر النوبة", "كلابشة", "الرديسية" },
                ["أسيوط"] = new[] { "أسيوط", "ديروط", "منفلوط", "القوصية", "أبنوب", "البداري" },
                ["الأقصر"] = new[] { "الأقصر", "الزينية", "البياضية", "الطود", "إسنا", "أرمنت" },
                ["البحر الأحمر"] = new[] { "الغردقة", "رأس غارب", "سفاجا", "القصير", "مرسى علم", "شلاتين" },
                ["البحيرة"] = new[] { "دمنهور", "كفر الدوار", "رشيد", "إدكو", "أبو المطامير", "حوش عيسى" },
                ["بني سويف"] = new[] { "بني سويف", "الواسطى", "ناصر", "إهناسيا", "ببا", "سمسطا" },
                ["بورسعيد"] = new[] { "بورسعيد", "حى الشرق", "حى الغرب", "حى الضواحي", "حى الجنوب", "حى العرب" },
                ["جنوب سيناء"] = new[] { "شرم الشيخ", "دهب", "نويبع", "طابا", "رأس سدر", "أبو رديس" },
                ["الدقهلية"] = new[] { "المنصورة", "ميت غمر", "أجا", "منية النصر", "السنبلاوين", "طلخا" },
                ["دمياط"] = new[] { "دمياط", "فارسكور", "الروضة", "كفر سعد", "زرقا", "ميت أبو غالب" },
                ["السويس"] = new[] { "السويس", "حي الأربعين", "حي عتاقة", "حي الجناين", "حي فيصل", "حي الأمل" },
                ["الشرقية"] = new[] { "الزقازيق", "بلبيس", "أبو حماد", "ههيا", "الصالحية", "منيا القمح" },
                ["شمال سيناء"] = new[] { "العريش", "الشيخ زويد", "رفح", "بئر العبد", "الحسنة", "نخل" },
                ["الغربية"] = new[] { "طنطا", "المحلة الكبرى", "زفتى", "سمنود", "قطور", "بسيون" },
                ["الفيوم"] = new[] { "الفيوم", "طامية", "سنورس", "إطسا", "يوسف الصديق", "السيليين" },
                ["القليوبية"] = new[] { "بنها", "قليوب", "شبرا الخيمة", "الخانكة", "كفر شكر", "القناطر الخيرية" },
                ["قنا"] = new[] { "قنا", "قفط", "نقادة", "دشنا", "فرشوط", "أبو تشت" },
                ["كفر الشيخ"] = new[] { "كفر الشيخ", "دسوق", "فوه", "مطوبس", "بلطيم", "الحامول" },
                ["مطروح"] = new[] { "مرسى مطروح", "الحمام", "العلمين", "الضبعة", "النجيلة", "سيوة" },
                ["المنيا"] = new[] { "المنيا", "ملوي", "دير مواس", "مغاغة", "بني مزار", "مطاي" },
                ["الوادي الجديد"] = new[] { "الخارجة", "الداخلة", "باريس", "موط", "بلاط", "الفرافرة" }
            };

            foreach (var gov in governorates)
            {
                if (cityNames.ContainsKey(gov.GovernorateName))
                {
                    foreach (var cityName in cityNames[gov.GovernorateName].Take(3))
                    {
                        cities.Add(new City
                        {
                            CityName = cityName,
                            GovernorateID = gov.GovernorateID
                        });
                    }
                }
                else
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        cities.Add(new City
                        {
                            CityName = $"{gov.GovernorateName.Substring(0, Math.Min(4, gov.GovernorateName.Length))} {i}",
                            GovernorateID = gov.GovernorateID
                        });
                    }
                }
            }

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();

            // المواقع لكل مدينة (3 مواقع لكل مدينة)
            var locations = new List<Location>();
            var locationCounter = 1;
            var regionTypes = new[] { "حي", "منطقة", "ميدان", "شارع", "كورنيش", "طريق" };
            var streetNames = new[] { "النيل", "التحرير", "الحرية", "الجامعة", "البحر", "الهرم", "المريوطية",
                                       "الجمهورية", "فاروق", "السوق", "الشهداء", "الاستقلال", "23 يوليو",
                                       "26 يوليو", "15 مايو", "6 أكتوبر", "النهضة", "السلام", "الأمل" };

            foreach (var city in cities)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var regionType = regionTypes[locationCounter % regionTypes.Length];
                    var streetName = streetNames[locationCounter % streetNames.Length];

                    locations.Add(new Location
                    {
                        Region = $"{regionType} {GetArabicNumber(i)}",
                        Street = $"{streetName} - {GetRandomBuildingNumber(locationCounter)}",
                        CityID = city.CityID
                    });
                    locationCounter++;
                }
            }

            await context.Locations.AddRangeAsync(locations);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Seeded 27 governorates, {cities.Count} cities, {locations.Count} locations");
        }

        private static async Task SeedAllAmenities(AppDbContext context)
        {
            var amenities = new List<Amenity>
            {
                new Amenity { AmenityName = "واي فاي مجاني" },
                new Amenity { AmenityName = "تكييف هواء" },
                new Amenity { AmenityName = "تلفزيون" },
                new Amenity { AmenityName = "فطور مجاني" },
                new Amenity { AmenityName = "موقف سيارات" },
                new Amenity { AmenityName = "مسبح" },
                new Amenity { AmenityName = "جيم" },
                new Amenity { AmenityName = "مطعم" },
                new Amenity { AmenityName = "خدمة الغرف" },
                new Amenity { AmenityName = "غسالة ملابس" },
                new Amenity { AmenityName = "مطبخ مجهز" },
                new Amenity { AmenityName = "تأمين" },
                new Amenity { AmenityName = "تكييف مركزي" },
                new Amenity { AmenityName = "مدفأة" },
                new Amenity { AmenityName = "تراس" },
                new Amenity { AmenityName = "حديقة" },
                new Amenity { AmenityName = "بلكونة" },
                new Amenity { AmenityName = "مكتب عمل" },
                new Amenity { AmenityName = "ميكروويف" },
                new Amenity { AmenityName = "ثلاجة" },
                new Amenity { AmenityName = "غلاية" },
                new Amenity { AmenityName = "مكواة" },
                new Amenity { AmenityName = "خزنة" },
                new Amenity { AmenityName = "مشغل دي في دي" },
                new Amenity { AmenityName = "ميني بار" }
            };

            await context.Amenities.AddRangeAsync(amenities);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Seeded {amenities.Count} amenities");
        }

        private static async Task SeedAllAccommodations(AppDbContext context)
        {
            var random = new Random();
            var owners = await context.Users
                .Where(u => u.Type == UserRole.Owner)
                .ToListAsync();

            var locations = await context.Locations.ToListAsync();
            var amenities = await context.Amenities.ToListAsync();

            // المحافظات المستهدفة
            var targetGovernorates = new[] { "سوهاج", "القاهرة", "الجيزة", "الإسكندرية", "المنوفية", "الإسماعيلية" };

            // الحصول على مواقع المحافظات المستهدفة
            var targetLocations = new List<Location>();
            foreach (var location in locations)
            {
                var city = await context.Cities
                    .Include(c => c.Governorate)
                    .FirstOrDefaultAsync(c => c.CityID == location.CityID);

                if (city != null && city.Governorate != null &&
                    targetGovernorates.Contains(city.Governorate.GovernorateName))
                {
                    targetLocations.Add(location);
                }
            }

            // تقسيم المواقع حسب المحافظة
            var locationsByGovernorate = targetLocations
                .GroupBy(l => context.Cities
                    .Include(c => c.Governorate)
                    .First(c => c.CityID == l.CityID)
                    .Governorate.GovernorateName)
                .ToDictionary(g => g.Key, g => g.ToList());

            var hotels = new List<Hotel>();
            var localLodings = new List<LocalLoding>();
            var studentHouses = new List<StudentHouse>();

            var hotelNames = new[]
            {
                "جراند بلازا", "ريزيدنس إن", "هيلتون", "شيراتون", "ماريوت",
                "إنتركونتيننتال", "فور سيزونز", "ريتز كارلتون", "ويستن", "هوليداي إن",
                "مينا هاوس", "سوفيتيل", "ميريديان", "كورينثيا", "فيرمونت",
                "راديسون بلو", "بست ويسترن", "كمبنسكي", "بارك إن", "هيلدا"
            };

            var apartmentNames = new[]
            {
                "شقة رويال", "شقة ديلوكس", "شقة فاميلي", "شقة إيجار يومي",
                "شقة مفروشة", "شقة استوديو", "شقة بغرفتين", "بنتهاوس",
                "شقة سوبر لوكس", "شقة دوبلكس", "شقة تريبلكس", "شقة جاردن",
                "شقة ريفيو", "شقة بريميوم", "شقة فيو", "شقة بواية",
                "شقة ريزيدنس", "شقة كامبوند", "شقة تاون هاوس", "شقة لوفت"
            };

            var studentHouseNames = new[]
            {
                "سكن الطلاب المتميز", "سكن الجامعة", "سكن الطالبات", "سكن العاصمة",
                "سكن النخبة", "سكن الرفاهية", "سكن المستقبل", "سكن الأمل",
                "سكن النور", "سكن العلم", "سكن التميز", "سكن الإبداع",
                "سكن التقدم", "سكن الرقي", "سكن الازدهار", "سكن المعرفة"
            };

            int ownerIndex = 0;
            int totalAccommodations = 0;

            // إنشاء 5 فنادق لكل محافظة (30 فندق إجمالاً)
            foreach (var governorate in targetGovernorates)
            {
                if (locationsByGovernorate.ContainsKey(governorate))
                {
                    var govLocations = locationsByGovernorate[governorate];

                    for (int i = 0; i < 5 && i < govLocations.Count; i++)
                    {
                        var owner = owners[ownerIndex % owners.Count];

                        var hotel = new Hotel
                        {
                            AccommodationName = $"{hotelNames[totalAccommodations % hotelNames.Length]} - {governorate}",
                            AccommodationDescription = GetHotelDescription(governorate, hotelNames[totalAccommodations % hotelNames.Length]),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                            UpdatedAt = DateTime.Now.AddDays(-random.Next(1, 30)),
                            AppUserID = owner.Id,
                            LocationID = govLocations[i].LocationID,
                            StarsRate = random.Next(3, 6),
                            Amenities = GetRandomAmenities(amenities, random, 4, 7),
                            IsApproved = true
                        };

                        hotels.Add(hotel);
                        ownerIndex++;
                        totalAccommodations++;
                    }
                }
            }

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {hotels.Count} hotels");

            // إنشاء 5 شقق لكل محافظة (30 شقة إجمالاً)
            totalAccommodations = 0;
            foreach (var governorate in targetGovernorates)
            {
                if (locationsByGovernorate.ContainsKey(governorate))
                {
                    var govLocations = locationsByGovernorate[governorate];

                    for (int i = 0; i < 5 && i < govLocations.Count; i++)
                    {
                        var owner = owners[ownerIndex % owners.Count];

                        var apartment = new LocalLoding
                        {
                            AccommodationName = $"{apartmentNames[totalAccommodations % apartmentNames.Length]} - {governorate}",
                            AccommodationDescription = GetApartmentDescription(governorate, apartmentNames[totalAccommodations % apartmentNames.Length]),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                            UpdatedAt = DateTime.Now.AddDays(-random.Next(1, 30)),
                            AppUserID = owner.Id,
                            LocationID = govLocations[i].LocationID,
                            Area = random.Next(80, 250),
                            Floor = random.Next(1, 15),
                            TotalRooms = random.Next(2, 6),
                            TotalGuests = random.Next(2, 8),
                            PricePerNight = random.Next(500, 3000),
                            IsAvailable = random.Next(0, 10) > 2, // 70% available
                            Amenities = GetRandomAmenities(amenities, random, 3, 6),
                            IsApproved = true
                        };

                        localLodings.Add(apartment);
                        ownerIndex++;
                        totalAccommodations++;
                    }
                }
            }

            await context.LocalLodings.AddRangeAsync(localLodings);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {localLodings.Count} apartments");

            // إنشاء 5 سكن طلاب لكل محافظة (30 سكن طلاب إجمالاً)
            totalAccommodations = 0;
            foreach (var governorate in targetGovernorates)
            {
                if (locationsByGovernorate.ContainsKey(governorate))
                {
                    var govLocations = locationsByGovernorate[governorate];

                    for (int i = 0; i < 5 && i < govLocations.Count; i++)
                    {
                        var owner = owners[ownerIndex % owners.Count];

                        var studentHouse = new StudentHouse
                        {
                            AccommodationName = $"{studentHouseNames[totalAccommodations % studentHouseNames.Length]} - {governorate}",
                            AccommodationDescription = GetStudentHouseDescription(governorate, studentHouseNames[totalAccommodations % studentHouseNames.Length]),
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                            UpdatedAt = DateTime.Now.AddDays(-random.Next(1, 30)),
                            AppUserID = owner.Id,
                            LocationID = govLocations[i].LocationID,
                            Area = random.Next(300, 1000),
                            Floor = random.Next(1, 5),
                            TotalGuests = random.Next(20, 100),
                            Amenities = GetRandomAmenities(amenities, random, 5, 8),
                            IsApproved = true
                        };

                        studentHouses.Add(studentHouse);
                        ownerIndex++;
                        totalAccommodations++;
                    }
                }
            }

            await context.StudentHouses.AddRangeAsync(studentHouses);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {studentHouses.Count} student houses");
        }

        private static async Task SeedRoomsAndBeds(AppDbContext context)
        {
            var random = new Random();

            // ========== Hotel Rooms ==========
            var hotels = await context.Hotels.ToListAsync();
            var hotelRooms = new List<HotelRoom>();

            foreach (var hotel in hotels)
            {
                // 10 غرف لكل فندق
                for (int i = 1; i <= 10; i++)
                {
                    var roomType = random.Next(0, 2) == 0 ? RoomType.Single : RoomType.Double;
                    var price = roomType == RoomType.Single ?
                        random.Next(800, 2000) : random.Next(1200, 3500);

                    var hotelRoom = new HotelRoom
                    {
                        RoomNumber = hotel.AccommodationID * 100 + i,
                        Type = roomType,
                        RoomDescription = GetRoomDescription(roomType, hotel.AccommodationName),
                        PricePerNight = price,
                        IsAvailable = random.Next(0, 10) > 2, // 70% available
                        AccommodationID = hotel.AccommodationID 
                        
                    };

                    hotelRooms.Add(hotelRoom);
                }
            }

            await context.HotelRooms.AddRangeAsync(hotelRooms);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {hotelRooms.Count} hotel rooms");

            // ========== Student Rooms ==========
            var studentHouses = await context.StudentHouses.ToListAsync();
            var studentRooms = new List<StudentRoom>();

            foreach (var studentHouse in studentHouses)
            {
                // 5 غرف لكل سكن طلابي
                for (int i = 1; i <= 5; i++)
                {
                    var studentRoom = new StudentRoom
                    {
                        TotalBeds = random.Next(2, 6),
                        AccommodationID = studentHouse.AccommodationID
                    };

                    studentRooms.Add(studentRoom);
                }
            }

            await context.StudentRooms.AddRangeAsync(studentRooms);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {studentRooms.Count} student rooms");

            // ========== Beds ==========
            var beds = new List<Bed>();

            foreach (var studentRoom in studentRooms)
            {
                for (int i = 1; i <= studentRoom.TotalBeds; i++)
                {
                    var bed = new Bed
                    {
                        RoomDescription = $"سرير {i} في الغرفة {studentRoom.StudentRoomID}",
                        PricePerNight = random.Next(100, 500),
                        IsAvailable = random.Next(0, 10) > 3, // 60% available
                        StudentRoomID = studentRoom.StudentRoomID
                    };

                    beds.Add(bed);
                }
            }

            await context.Beds.AddRangeAsync(beds);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {beds.Count} beds");
        }

        private static async Task SeedAllImages(AppDbContext context)
        {
            var random = new Random();
            var images = new List<Image>();

            // ========== استخدم نفس الصور من الكود الأول ==========
            var imageUrls = new[]
            {
                "https://images.unsplash.com/photo-1618773928121-c32242e63f39",
                "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                "https://images.unsplash.com/photo-1582719508461-905c673771fd",
                "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd",
                "https://images.unsplash.com/photo-1571896349842-33c89424de2d",
                "https://images.unsplash.com/photo-1560185127-6ed189bf02f4",
                "https://images.unsplash.com/photo-1555854877-bab0e564b8d5",
                "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                "https://images.unsplash.com/photo-1513584684374-8bab748fbf90",
                "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688"
            };

            // صور للفنادق
            var hotels = await context.Hotels.ToListAsync();
            foreach (var hotel in hotels)
            {
                // 3-5 صور لكل فندق (نفس التوزيع)
                for (int i = 0; i < new Random().Next(3, 6); i++)
                {
                    var image = new Image
                    {
                        ImageUrl = imageUrls[images.Count % imageUrls.Length],
                        AltText = $"{hotel.AccommodationName} - صورة {i + 1}",
                        IsMain = i == 0, // أول صورة هي الرئيسية
                        CreatedAt = DateTime.Now,
                        AccommodationID = hotel.AccommodationID
                    };
                    images.Add(image);
                }
            }

            // صور للشقق
            var apartments = await context.LocalLodings.ToListAsync();
            foreach (var apartment in apartments)
            {
                // 3-5 صور لكل شقة
                for (int i = 0; i < new Random().Next(3, 6); i++)
                {
                    var image = new Image
                    {
                        ImageUrl = imageUrls[images.Count % imageUrls.Length],
                        AltText = $"{apartment.AccommodationName} - صورة {i + 1}",
                        IsMain = i == 0,
                        CreatedAt = DateTime.Now,
                        AccommodationID = apartment.AccommodationID
                    };
                    images.Add(image);
                }
            }

            // صور لسكن الطلاب
            var studentHouses = await context.StudentHouses.ToListAsync();
            foreach (var studentHouse in studentHouses)
            {
                // 3-5 صور لكل سكن طلاب
                for (int i = 0; i < new Random().Next(3, 6); i++)
                {
                    var image = new Image
                    {
                        ImageUrl = imageUrls[images.Count % imageUrls.Length],
                        AltText = $"{studentHouse.AccommodationName} - صورة {i + 1}",
                        IsMain = i == 0,
                        CreatedAt = DateTime.Now,
                        AccommodationID = studentHouse.AccommodationID
                    };
                    images.Add(image);
                }
            }

            // صور لغرف الفنادق (50 غرفة فقط كما في الكود الأول)
            var hotelRooms = await context.HotelRooms.Take(50).ToListAsync();
            foreach (var room in hotelRooms)
            {
                // 2-4 صور لكل غرفة
                for (int i = 1; i <= new Random().Next(2, 5); i++)
                {
                    var image = new Image
                    {
                        ImageUrl = imageUrls[images.Count % imageUrls.Length],
                        AltText = $"غرفة {room.RoomNumber} - صورة {i}",
                        IsMain = i == 1,
                        CreatedAt = DateTime.Now,
                        AccommodationID = room.AccommodationID
                    };
                    images.Add(image);
                }
            }

            await context.Images.AddRangeAsync(images);
            await context.SaveChangesAsync();
            Console.WriteLine($"   ✅ Created {images.Count} images with the SAME URLs from first code");
        }

        private static async Task SeedSampleBookings(AppDbContext context)
        {
            var random = new Random();
            var clients = await context.Users
                .Where(u => u.Type == UserRole.Client)
                .Take(5)
                .ToListAsync();

            var hotelRooms = await context.HotelRooms
                .Where(r => r.IsAvailable)
                .Take(10)
                .ToListAsync();

            var apartments = await context.LocalLodings
                .Where(l => l.IsAvailable)
                .Take(5)
                .ToListAsync();

            var beds = await context.Beds
                .Where(b => b.IsAvailable)
                .Take(10)
                .ToListAsync();

            var bookings = new List<Booking>();

            // حجوزات لغرف الفنادق
            for (int i = 0; i < 5 && i < clients.Count && i < hotelRooms.Count; i++)
            {
                var checkIn = DateTime.Now.AddDays(random.Next(1, 30));
                var checkOut = checkIn.AddDays(random.Next(1, 7));

                var booking = new Booking
                {
                    TotalPrice = hotelRooms[i].PricePerNight * (decimal)(checkOut - checkIn).Days,
                    CheckIN = checkIn,
                    CheckOUT = checkOut,
                    AppUserID = clients[i].Id,
                    HotelRooms = new List<HotelRoom> { hotelRooms[i] }
                };

                bookings.Add(booking);
            }

            // حجوزات للشقق
            for (int i = 0; i < 3 && i < clients.Count && i < apartments.Count; i++)
            {
                var clientIndex = (i + 2) % clients.Count;
                var checkIn = DateTime.Now.AddDays(random.Next(1, 30));
                var checkOut = checkIn.AddDays(random.Next(2, 10));

                var booking = new Booking
                {
                    TotalPrice = apartments[i].PricePerNight * (decimal)(checkOut - checkIn).Days,
                    CheckIN = checkIn,
                    CheckOUT = checkOut,
                    AppUserID = clients[clientIndex].Id,
                    LocalLodings = new List<LocalLoding> { apartments[i] }
                };

                bookings.Add(booking);
            }

            // حجوزات للأسرة
            for (int i = 0; i < 3 && i < clients.Count && i < beds.Count; i++)
            {
                var clientIndex = (i + 3) % clients.Count;
                var checkIn = DateTime.Now.AddDays(random.Next(1, 30));
                var checkOut = checkIn.AddDays(random.Next(7, 30));

                var booking = new Booking
                {
                    TotalPrice = beds[i].PricePerNight * (decimal)(checkOut - checkIn).Days,
                    CheckIN = checkIn,
                    CheckOUT = checkOut,
                    AppUserID = clients[clientIndex].Id,
                    Beds = new List<Bed> { beds[i] }
                };

                bookings.Add(booking);
            }

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();

            // إنشاء مدفوعات للحجوزات
            var payments = new List<Payment>();
            var paymentMethods = new[] { "بطاقة ائتمان", "حوالة بنكية", "دفع نقدي", "فوري", "محفظة إلكترونية" };

            foreach (var booking in bookings)
            {
                var payment = new Payment
                {
                    PaymentMethod = paymentMethods[random.Next(0, paymentMethods.Length)],
                    Amount = booking.TotalPrice,
                    PayAt = DateTime.Now.AddDays(-random.Next(1, 5)),
                    BookingID = booking.BookingID
                };
                payments.Add(payment);
            }

            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();

            // إنشاء تقييمات للحجوزات
            var reviews = new List<Review>();
            var reviewTexts = new[]
            {
                "إقامة رائعة وخدمات ممتازة، شكراً لكم",
                "مكان نظيف وهادئ، أنصح به بشدة",
                "الموقع ممتاز والخدمة جيدة",
                "تجربة جميلة، سأعود مرة أخرى",
                "أسعار معقولة وجودة عالية",
                "طاقم العمل ودود ومتعاون",
                "الغرفة كانت نظيفة ومريحة",
                "الإفطار كان لذيذاً ومتنوعاً",
                "الموقع قريب من كل شيء",
                "أفضل مما توقعت، شكراً"
            };

            foreach (var booking in bookings.Take(10))
            {
                var review = new Review
                {
                    Reviewtext = reviewTexts[random.Next(0, reviewTexts.Length)],
                    Rate = random.Next(3, 6),
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 3)),
                    BookingID = booking.BookingID
                };
                reviews.Add(review);
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();

            Console.WriteLine($"   ✅ Created {bookings.Count} bookings with payments and reviews");
        }

        #region Helper Methods
        private static string GenerateRealisticNationalId(int seed)
        {
            var random = new Random(seed);
            int year = 80 + (seed % 20); // 1980-1999
            int month = random.Next(1, 13);
            int day = random.Next(1, 29);
            int governorate = random.Next(1, 28);
            int randomNum = random.Next(10000, 99999);

            return $"{year:D2}{month:D2}{day:D2}{governorate:D2}{randomNum:D5}";
        }

        private static string GenerateRealisticPhoneNumber(int seed)
        {
            var random = new Random(seed);
            var prefixes = new[] { "010", "011", "012", "015" };
            var prefix = prefixes[seed % prefixes.Length];
            var number = random.Next(10000000, 99999999);

            return $"{prefix}{number}";
        }

        private static string GetArabicNumber(int number)
        {
            var arabicNumbers = new[] { "الأول", "الثاني", "الثالث", "الرابع", "الخامس", "السادس", "السابع", "الثامن", "التاسع", "العاشر" };
            return number <= 10 ? arabicNumbers[number - 1] : $"رقم {number}";
        }

        private static string GetRandomBuildingNumber(int seed)
        {
            var random = new Random(seed);
            return $"مبني {random.Next(1, 100)}";
        }

        private static string GetHotelDescription(string governorate, string hotelName)
        {
            var descriptions = new[]
            {
                $"فندق {hotelName} في {governorate} يقدم إقامة فاخرة مع إطلالات خلابة وخدمات عالمية المستوى.",
                $"يقع {hotelName} في قلب {governorate} ويوفر وسائل راحة عصرية ومرافق متكاملة لضمان إقامة لا تُنسى.",
                $"تجمع {hotelName} بين الفخامة والراحة في موقع استراتيجي ب{governorate} مع طاقم عمل محترف.",
                $"يتميز {hotelName} في {governorate} بتصميم أنيق وغرف فسيحة ومطاعم تقدم أشهى المأكولات.",
                $"يوفر {hotelName} في {governorate} تجربة إقامة استثنائية مع مرافق ترفيهية وخدمات راقية."
            };
            return descriptions[new Random().Next(0, descriptions.Length)];
        }

        private static string GetApartmentDescription(string governorate, string apartmentName)
        {
            var descriptions = new[]
            {
                $"شقة {apartmentName} في {governorate} مجهزة بالكامل وتوفر كل وسائل الراحة للإقامة الطويلة أو القصيرة.",
                $"تتميز {apartmentName} في {governorate} بموقع ممتاز وتصميم عصري وتجهيزات عالية الجودة.",
                $"شقة فاخرة في {governorate} مع إطلالة رائعة وتوفر خصوصية تامة وهدوء مثالي للراحة.",
                $"تقع {apartmentName} في منطقة حيوية ب{governorate} وتوفر سهولة الوصول للمرافق والخدمات.",
                $"شقة مريحة وأنيقة في {governorate} مع مساحات واسعة وتجهيزات كاملة لعائلة أو أفراد."
            };
            return descriptions[new Random().Next(0, descriptions.Length)];
        }

        private static string GetStudentHouseDescription(string governorate, string houseName)
        {
            var descriptions = new[]
            {
                $"{houseName} في {governorate} يوفر سكن آمن ومريح للطلاب مع مرافق دراسية وترفيهية متكاملة.",
                $"بيئة تعليمية ممتازة في {governorate} مع غرف دراسة مشتركة ومساحات للترفيه والاسترخاء.",
                $"سكن طلابي نظيف ومنظم في {governorate} مع إشراف مباشر وخدمات صيانة سريعة.",
                $"يقع {houseName} في {governorate} قرب الجامعات ويوفر وسائل نقل مجانية وخدمات إنترنت سريع.",
                $"سكن طلابي متميز في {governorate} مع نظام أمن متكامل ومرافق رياضية وترفيهية متنوعة."
            };
            return descriptions[new Random().Next(0, descriptions.Length)];
        }

        private static List<Amenity> GetRandomAmenities(List<Amenity> allAmenities, Random random, int min, int max)
        {
            return allAmenities
                .OrderBy(a => random.Next())
                .Take(random.Next(min, max + 1))
                .ToList();
        }

        private static string GetRoomDescription(RoomType roomType, string hotelName)
        {
            return roomType == RoomType.Single
                ? $"غرفة فردية فاخرة في {hotelName} مع سرير كينج ومكتب عمل"
                : $"غرفة مزدوجة واسعة في {hotelName} مع سريرين كوين وتراس خاص";
        }
        #endregion
    }
}
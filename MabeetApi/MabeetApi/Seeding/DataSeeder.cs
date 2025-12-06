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
			var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); // 🛑 ضروري جداً

			await context.Database.MigrateAsync();

			// ========== 0) تهيئة الأدوار (Roles) ==========
			string[] roles = { "Admin", "Owner", "Client" };
			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			// ========== 1) إنشاء المستخدمين وربطهم بالأدوار ==========
			if (!context.Users.Any())
			{
				// Admin
				var admin = new AppUser
				{
					UserName = "admin@mabeet.com",
					Email = "admin@mabeet.com",
					FirstName = "Admin",
					LastName = "Main",
					NationalID = "11111111111111",
					PhoneNumber = "01000000000",
					Type = UserRole.Admin,
					IsActive = true,
					EmailConfirmed = true
				};
				if (await userManager.FindByEmailAsync(admin.Email) == null)
				{
					await userManager.CreateAsync(admin, "Admin@123");
					await userManager.AddToRoleAsync(admin, "Admin"); // 🛑 الربط بالدور
				}

				// Owner
				var owner = new AppUser
				{
					UserName = "owner@mabeet.com",
					Email = "owner@mabeet.com",
					FirstName = "Owner",
					LastName = "One",
					NationalID = "22222222222222",
					PhoneNumber = "01011111111",
					Type = UserRole.Owner,
					IsActive = true,
					EmailConfirmed = true
				};
				if (await userManager.FindByEmailAsync(owner.Email) == null)
				{
					await userManager.CreateAsync(owner, "Owner@123");
					await userManager.AddToRoleAsync(owner, "Owner"); // 🛑 الربط بالدور
				}

				// Client 1
				var client1 = new AppUser
				{
					UserName = "client1@mabeet.com",
					Email = "client1@mabeet.com",
					FirstName = "Client",
					LastName = "One",
					NationalID = "33333333333333",
					PhoneNumber = "01022222222",
					Type = UserRole.Client,
					IsActive = true,
					EmailConfirmed = true
				};
				if (await userManager.FindByEmailAsync(client1.Email) == null)
				{
					await userManager.CreateAsync(client1, "Client@123");
					await userManager.AddToRoleAsync(client1, "Client"); // 🛑 الربط بالدور
				}

				// Client 2
				var client2 = new AppUser
				{
					UserName = "client2@mabeet.com",
					Email = "client2@mabeet.com",
					FirstName = "Client",
					LastName = "Two",
					NationalID = "44444444444444",
					PhoneNumber = "01033333333",
					Type = UserRole.Client,
					IsActive = true,
					EmailConfirmed = true
				};
				if (await userManager.FindByEmailAsync(client2.Email) == null)
				{
					await userManager.CreateAsync(client2, "Client@123");
					await userManager.AddToRoleAsync(client2, "Client");
				}

				await context.SaveChangesAsync();
			}
			else
			{
				// 🛑 إصلاح المستخدمين القدامى (إذا كانت الداتابيز موجودة ولكن الأدوار ناقصة)
				var allUsers = await userManager.Users.ToListAsync();
				foreach (var user in allUsers)
				{
					if (!await userManager.IsInRoleAsync(user, user.Type.ToString()))
					{
						await userManager.AddToRoleAsync(user, user.Type.ToString());
					}
				}
			}

			var ownerUser = await context.Users.FirstOrDefaultAsync(u => u.Type == UserRole.Owner);
			var clientUsers = await context.Users.Where(u => u.Type == UserRole.Client).ToListAsync();

			if (ownerUser == null) return; // أمان إضافي

			// ========== 2) Governorates + Cities + Locations ==========
			if (!context.Governorates.Any())
			{
                var cairo = new Governorate { GovernorateName = "Cairo" };
                var giza = new Governorate { GovernorateName = "Giza" };
                var alex = new Governorate { GovernorateName = "Alexandria" };
                var aswan = new Governorate { GovernorateName = "Aswan" };
                var asyut = new Governorate { GovernorateName = "Asyut" };
                var behaira = new Governorate { GovernorateName = "Beheira" };
                var bniSuef = new Governorate { GovernorateName = "Beni Suef" };
                var dakahlia = new Governorate { GovernorateName = "Dakahlia" };
                var damietta = new Governorate { GovernorateName = "Damietta" };
                var faiyum = new Governorate { GovernorateName = "Faiyum" };
                var ghharbia = new Governorate { GovernorateName = "Gharbia" };               
                var ismailia = new Governorate { GovernorateName = "Ismailia" };
                var kafrElSheikh = new Governorate { GovernorateName = "Kafr El Sheikh" };
                var luxor = new Governorate { GovernorateName = "Luxor" };
                var matruh = new Governorate { GovernorateName = "Matruh" };
                var minya = new Governorate { GovernorateName = "Minya" };
                var monufia = new Governorate { GovernorateName = "Monufia" };
                var newValley = new Governorate { GovernorateName = "New Valley" };
                var northSinai = new Governorate { GovernorateName = "North Sinai" };
                var portSaid = new Governorate { GovernorateName = "Port Said" };
                var qalyubia = new Governorate { GovernorateName = "Qalyubia" };
                var qena = new Governorate { GovernorateName = "Qena" };
                var redSea = new Governorate { GovernorateName = "Red Sea" };
                var sohag = new Governorate { GovernorateName = "Sohag" };
                var southSinai = new Governorate { GovernorateName = "South Sinai" };
                var suiiz = new Governorate { GovernorateName = "Suez" };
                var sharqia = new Governorate { GovernorateName = "Sharqia" };


                context.Governorates.AddRange(cairo, giza, alex);
				await context.SaveChangesAsync();

				var cities = new List<City>
				{
					new City { CityName = "Nasr City", GovernorateID = cairo.GovernorateID },
					new City { CityName = "Maadi", GovernorateID = cairo.GovernorateID },
					new City { CityName = "Dokki", GovernorateID = giza.GovernorateID },
					new City { CityName = "Haram", GovernorateID = giza.GovernorateID },
					new City { CityName = "Smouha", GovernorateID = alex.GovernorateID },
					new City { CityName = "Miami", GovernorateID = alex.GovernorateID }
				};

				context.Cities.AddRange(cities);
				await context.SaveChangesAsync();

				var locations = new List<Location>
				{
					new Location { Region = "District 7", Street = "Main St", CityID = cities[0].CityID },
					new Location { Region = "Corniche", Street = "Nile St", CityID = cities[1].CityID },
					new Location { Region = "El Tahrir", Street = "Square", CityID = cities[2].CityID },
					new Location { Region = "Faisal", Street = "Street 10", CityID = cities[3].CityID },
					new Location { Region = "Smouha Center", Street = "Victoria Rd", CityID = cities[4].CityID },
					new Location { Region = "Miami Beach", Street = "Sea St", CityID = cities[5].CityID }
				};

				context.Locations.AddRange(locations);
				await context.SaveChangesAsync();

				// ========== 3) Accommodations ==========
				var hotels = new List<Hotel>
				{
					new Hotel
					{
						AccommodationName = "Grand Nile Hotel",
						AccommodationDescription = "5-star hotel with Nile view",
						StarsRate = 5,
						LocationID = locations[0].LocationID,
						AppUserID = ownerUser.Id,
						AccommodationType = "Hotel",
						IsApproved = true
					},
					new Hotel
					{
						AccommodationName = "City View Hotel",
						AccommodationDescription = "4-star hotel in downtown",
						StarsRate = 4,
						LocationID = locations[1].LocationID,
						AppUserID = ownerUser.Id,
						AccommodationType = "Hotel",
						IsApproved = true
					}
				};

				context.Hotels.AddRange(hotels);
				await context.SaveChangesAsync();

				var localLodings = new List<LocalLoding>
				{
					new LocalLoding
					{
						AccommodationName = "Family Apartment Nasr City",
						AccommodationDescription = "Spacious apartment for families",
						Area = 120,
						Floor = 5,
						TotalRooms = 3,
						TotalGuests = 5,
						PricePerNight = 450,
						IsAvailable = true,
						LocationID = locations[2].LocationID,
						AppUserID = ownerUser.Id,
						AccommodationType = "LocalLoding",
						IsApproved = true
					},
					new LocalLoding
					{
						AccommodationName = "Sea View Apartment Miami",
						AccommodationDescription = "Nice sea view apartment",
						Area = 90,
						Floor = 7,
						TotalRooms = 2,
						TotalGuests = 4,
						PricePerNight = 500,
						IsAvailable = true,
						LocationID = locations[5].LocationID,
						AppUserID = ownerUser.Id,
						AccommodationType = "LocalLoding",
						IsApproved = true
					}
				};

				context.LocalLodings.AddRange(localLodings);
				await context.SaveChangesAsync();

				var studentHouse = new StudentHouse
				{
					AccommodationName = "Student House A",
					AccommodationDescription = "Shared rooms for students",
					Area = 300,
					Floor = 3,
					TotalGuests = 18,
					LocationID = locations[4].LocationID,
					AppUserID = ownerUser.Id,
					AccommodationType = "StudentHouse",
					IsApproved = true
				};

				context.StudentHouses.Add(studentHouse);
				await context.SaveChangesAsync();

				// ========== 4) HotelRooms ==========
				foreach (var hotel in hotels)
				{
					for (int i = 1; i <= 3; i++)
					{
						context.HotelRooms.Add(new HotelRoom
						{
							RoomNumber = i,
							Type = RoomType.Double,
							RoomDescription = $"Room {i} in {hotel.AccommodationName}",
							PricePerNight = 600 + (i * 50),
							IsAvailable = true,
							AccommodationID = hotel.AccommodationID
						});
					}
				}
				await context.SaveChangesAsync();

				// ========== 5) StudentRooms + Beds ==========
				var studentRooms = new List<StudentRoom>();
				for (int r = 1; r <= 3; r++)
				{
					var room = new StudentRoom { TotalBeds = 2, AccommodationID = studentHouse.AccommodationID };
					studentRooms.Add(room);
				}
				context.StudentRooms.AddRange(studentRooms);
				await context.SaveChangesAsync();

				foreach (var sr in studentRooms)
				{
					for (int b = 1; b <= 2; b++)
					{
						context.Beds.Add(new Bed
						{
							RoomDescription = $"Bed {b} in room {sr.StudentRoomID}",
							PricePerNight = 100,
							IsAvailable = true,
							StudentRoomID = sr.StudentRoomID
						});
					}
				}
				await context.SaveChangesAsync();

				// ========== 6) Images ==========
				var accommodations = await context.Accommodations.ToListAsync();
				foreach (var acc in accommodations)
				{
					for (int i = 1; i <= 3; i++)
					{
						context.Images.Add(new Image
						{
							ImageUrl = $"https://source.unsplash.com/800x600/?room,hotel&sig={acc.AccommodationID}_{i}",
							AltText = $"{acc.AccommodationName} image {i}",
							IsMain = (i == 1),
							AccommodationID = acc.AccommodationID
						});
					}
				}
				await context.SaveChangesAsync();
			}

			// ========== 7) Bookings ==========
			if (!context.Bookings.Any())
			{
				var anyClient = await context.Users.FirstOrDefaultAsync(u => u.Type == UserRole.Client);
				var anyRoom = await context.HotelRooms.FirstOrDefaultAsync();

				if (anyClient != null && anyRoom != null)
				{
					var booking = new Booking
					{
						AppUserID = anyClient.Id,
						CheckIN = DateTime.Today.AddDays(3),
						CheckOUT = DateTime.Today.AddDays(6),
						TotalPrice = anyRoom.PricePerNight * 3,
						Status = "Confirmed",
						CreatedAt = DateTime.Now
					};
					booking.HotelRooms.Add(anyRoom);
					context.Bookings.Add(booking);
					await context.SaveChangesAsync();
				}
			}
		}
	}
}
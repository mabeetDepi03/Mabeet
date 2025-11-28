using MabeetApi.Data;
using MabeetApi.DTOs.Property;
using MabeetApi.Entities;
using MabeetApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MabeetApi.Services.Property
{
    public class AccommodationService : IAccommodationService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccommodationService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ----------------------------------------------------------------------
        // 1. CREATE OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<AccommodationDetailDto> CreateAccommodationAsync(AccommodationCreateDto dto, string hostId)
        {
            // 1. Create Location
            var location = new Location
            {
                Region = dto.Location.Region,
                Street = dto.Location.Street,
                CityID = dto.Location.CityID
            };
            _context.Locations.Add(location);
            await _context.SaveChangesAsync(); // نحفظ عشان نجيب الـ LocationID

            // 2. Create Accommodation based on type
            Accommodation accommodation = dto.AccommodationType.ToLower() switch
            {
                "hotel" => new Hotel
                {
                    AccommodationName = dto.AccommodationName,
                    AccommodationDescription = dto.AccommodationDescription,
                    AppUserID = hostId,
                    LocationID = location.LocationID,
                    StarsRate = dto.StarsRate ?? 3
                },
                "localloding" => new LocalLoding
                {
                    AccommodationName = dto.AccommodationName,
                    AccommodationDescription = dto.AccommodationDescription,
                    AppUserID = hostId,
                    LocationID = location.LocationID,
                    Area = dto.Area ?? 0,
                    Floor = dto.Floor ?? 1,
                    TotalRooms = dto.TotalRooms ?? 1,
                    TotalGuests = dto.TotalGuests ?? 1,
                    PricePerNight = dto.PricePerNight ?? 0
                },
                "studenthouse" => new StudentHouse
                {
                    AccommodationName = dto.AccommodationName,
                    AccommodationDescription = dto.AccommodationDescription,
                    AppUserID = hostId,
                    LocationID = location.LocationID,
                    Area = dto.Area ?? 0,
                    Floor = dto.Floor ?? 1,
                    TotalGuests = dto.TotalGuests ?? 1
                },
                _ => throw new ArgumentException("Invalid accommodation type")
            };

            _context.Accommodations.Add(accommodation);
            await _context.SaveChangesAsync(); // نحفظ عشان نجيب الـ AccommodationID

            // 3. Add Amenities
            if (dto.AmenityIds != null && dto.AmenityIds.Any())
            {
                var amenities = await _context.Amenities
                    .Where(a => dto.AmenityIds.Contains(a.AmenityID))
                    .ToListAsync();

                foreach (var amenity in amenities)
                {
                    accommodation.Amenities.Add(amenity);
                }
            }

            // 4. Add Hotel Rooms if any
            if (dto.HotelRooms != null && dto.HotelRooms.Any() && accommodation is Hotel hotel)
            {
                foreach (var roomDto in dto.HotelRooms)
                {
                    var hotelRoom = new HotelRoom
                    {
                        RoomNumber = roomDto.RoomNumber,
                        Type = roomDto.Type,
                        RoomDescription = roomDto.RoomDescription,
                        PricePerNight = roomDto.PricePerNight,
                        IsAvailable = roomDto.IsAvailable,
                        AccommodationID = hotel.AccommodationID
                    };
                    _context.HotelRooms.Add(hotelRoom);
                }
            }

            // 5. Add Student Rooms if any
            if (dto.StudentRooms != null && dto.StudentRooms.Any() && accommodation is StudentHouse studentHouse)
            {
                foreach (var roomDto in dto.StudentRooms)
                {
                    var studentRoom = new StudentRoom
                    {
                        TotalBeds = roomDto.TotalBeds,
                        AccommodationID = studentHouse.AccommodationID
                    };
                    _context.StudentRooms.Add(studentRoom);
                    await _context.SaveChangesAsync(); // نحفظ هنا للحصول على الـ ID قبل إضافة الأسرة

                    // Add Beds
                    if (roomDto.Beds != null)
                    {
                        foreach (var bedDto in roomDto.Beds)
                        {
                            var bed = new Bed
                            {
                                RoomDescription = bedDto.RoomDescription,
                                PricePerNight = bedDto.PricePerNight,
                                IsAvailable = bedDto.IsAvailable,
                                StudentRoomID = studentRoom.StudentRoomID
                            };
                            _context.Beds.Add(bed);
                        }
                    }
                }
            }

            // 6. Save all changes
            await _context.SaveChangesAsync();

            // 7. Map and return
            return await MapToAccommodationDetailDto(accommodation.AccommodationID);
        }

        // ----------------------------------------------------------------------
        // 2. READ OPERATIONS (Host Specific)
        // ----------------------------------------------------------------------

        public async Task<IEnumerable<AccommodationListDto>> GetHostAccommodationsAsync(string hostId)
        {
            var accommodations = await _context.Accommodations
                .Include(a => a.Location)
                    .ThenInclude(l => l.City)
                    .ThenInclude(c => c.Governorate)
                .Include(a => a.Images)
                .Where(a => a.AppUserID == hostId)
                .ToListAsync();

            var accommodationDtos = new List<AccommodationListDto>();

            foreach (var accommodation in accommodations)
            {
                var mainImage = accommodation.Images?.FirstOrDefault(i => i.IsMain);

                var dto = new AccommodationListDto
                {
                    AccommodationID = accommodation.AccommodationID,
                    AccommodationName = accommodation.AccommodationName,
                    AccommodationType = accommodation.AccommodationType,
                    CreatedAt = accommodation.CreatedAt,
                    Region = accommodation.Location?.Region,
                    CityName = accommodation.Location?.City?.CityName,
                    GovernorateName = accommodation.Location?.City?.Governorate?.GovernorateName,
                    MainImageUrl = mainImage?.ImageUrl,
                    MainImageAltText = mainImage?.AltText
                };

                // Type-specific properties
                switch (accommodation)
                {
                    case Hotel hotel:
                        dto.StarsRate = hotel.StarsRate;
                        break;
                    case LocalLoding localLoding:
                        dto.PricePerNight = localLoding.PricePerNight;
                        dto.IsAvailable = localLoding.IsAvailable;
                        break;
                }

                accommodationDtos.Add(dto);
            }

            return accommodationDtos;
        }

        public async Task<AccommodationDetailDto?> GetAccommodationByIdAsync(int id, string hostId)
        {
            Accommodation? accommodation = null;

            // جرب Hotel أولاً
            accommodation = await _context.Accommodations
                .OfType<Hotel>()
                .Include(h => h.Location)
                    .ThenInclude(l => l.City)
                    .ThenInclude(c => c.Governorate)
                .Include(h => h.AppUser)
                .Include(h => h.Amenities)
                .Include(h => h.Images)
                .Include(h => h.HotelRooms)
                    .ThenInclude(hr => hr.Images)
                .FirstOrDefaultAsync(h => h.AccommodationID == id && h.AppUserID == hostId);

            if (accommodation == null)
            {
                // جرب StudentHouse
                accommodation = await _context.Accommodations
                    .OfType<StudentHouse>()
                    .Include(sh => sh.Location)
                        .ThenInclude(l => l.City)
                        .ThenInclude(c => c.Governorate)
                    .Include(sh => sh.AppUser)
                    .Include(sh => sh.Amenities)
                    .Include(sh => sh.Images)
                    .Include(sh => sh.StudentRooms)
                        .ThenInclude(sr => sr.Beds)
                    .FirstOrDefaultAsync(sh => sh.AccommodationID == id && sh.AppUserID == hostId);
            }

            if (accommodation == null)
            {
                // جرب LocalLoding
                accommodation = await _context.Accommodations
                    .OfType<LocalLoding>()
                    .Include(ll => ll.Location)
                        .ThenInclude(l => l.City)
                        .ThenInclude(c => c.Governorate)
                    .Include(ll => ll.AppUser)
                    .Include(ll => ll.Amenities)
                    .Include(ll => ll.Images)
                    .FirstOrDefaultAsync(ll => ll.AccommodationID == id && ll.AppUserID == hostId);
            }

            if (accommodation == null) return null;

            return MapToAccommodationDetailDto(accommodation);
        }

        // ----------------------------------------------------------------------
        // 3. UPDATE OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<AccommodationDetailDto?> UpdateAccommodationAsync(AccommodationUpdateDto dto, string hostId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Location)
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.AccommodationID == dto.AccommodationID && a.AppUserID == hostId);

            if (accommodation == null) return null;

            // Update basic info
            accommodation.AccommodationName = dto.AccommodationName;
            accommodation.AccommodationDescription = dto.AccommodationDescription;
            accommodation.UpdatedAt = DateTime.Now;

            // Update location
            if (dto.Location != null)
            {
                if (accommodation.Location == null)
                {
                    // لو مفيش Location موجود، نعمل جديد
                    accommodation.Location = new Location
                    {
                        Region = dto.Location.Region,
                        Street = dto.Location.Street,
                        CityID = dto.Location.CityID
                    };
                    _context.Locations.Add(accommodation.Location);
                }
                else
                {
                    // لو فيه Location موجود، نعدل عليه
                    accommodation.Location.Region = dto.Location.Region;
                    accommodation.Location.Street = dto.Location.Street;
                    accommodation.Location.CityID = dto.Location.CityID;
                }
            }

            // Update type-specific properties
            switch (accommodation)
            {
                case Hotel hotel when dto.StarsRate.HasValue:
                    hotel.StarsRate = dto.StarsRate.Value;
                    break;
                case LocalLoding localLoding:
                    if (dto.Area.HasValue) localLoding.Area = dto.Area.Value;
                    if (dto.Floor.HasValue) localLoding.Floor = dto.Floor.Value;
                    if (dto.TotalRooms.HasValue) localLoding.TotalRooms = dto.TotalRooms.Value;
                    if (dto.TotalGuests.HasValue) localLoding.TotalGuests = dto.TotalGuests.Value;
                    if (dto.PricePerNight.HasValue) localLoding.PricePerNight = dto.PricePerNight.Value;
                    if (dto.IsAvailable.HasValue) localLoding.IsAvailable = dto.IsAvailable.Value;
                    break;
                case StudentHouse studentHouse:
                    if (dto.Area.HasValue) studentHouse.Area = dto.Area.Value;
                    if (dto.Floor.HasValue) studentHouse.Floor = dto.Floor.Value;
                    if (dto.TotalGuests.HasValue) studentHouse.TotalGuests = dto.TotalGuests.Value;
                    break;
            }

            // Update amenities
            if (dto.AmenityIds != null)
            {
                accommodation.Amenities.Clear();
                var amenities = await _context.Amenities
                    .Where(a => dto.AmenityIds.Contains(a.AmenityID))
                    .ToListAsync();

                foreach (var amenity in amenities)
                {
                    accommodation.Amenities.Add(amenity);
                }
            }

            await _context.SaveChangesAsync();
            return MapToAccommodationDetailDto(accommodation);
        }

        // ----------------------------------------------------------------------
        // 4. DELETE OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<bool> DeleteAccommodationAsync(int id, string hostId)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == id && a.AppUserID == hostId);

            if (accommodation == null) return false;

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------------
        // 5. IMAGES OPERATIONS (ACCOMMODATION LEVEL)
        // ----------------------------------------------------------------------

        public async Task<bool> UploadImagesAsync(ImageUploadDto dto, string hostId)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == dto.AccommodationID && a.AppUserID == hostId);

            if (accommodation == null || dto.ImageFile == null) return false;

            var fileName = await SaveFileAsync(dto.ImageFile, "accommodations");

            // Create image record
            var image = new Image
            {
                ImageUrl = $"/uploads/accommodations/{fileName}",
                AltText = dto.AltText ?? accommodation.AccommodationName,
                IsMain = dto.IsMain,
                AccommodationID = dto.AccommodationID,
                CreatedAt = DateTime.Now
            };

            // If this is main image, unset previous main image
            if (dto.IsMain)
            {
                var previousMain = await _context.Images
                    .Where(i => i.AccommodationID == dto.AccommodationID && i.IsMain)
                    .FirstOrDefaultAsync();

                if (previousMain != null)
                    previousMain.IsMain = false;
            }

            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetMainImageAsync(int accommodationId, int imageId, string hostId)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId && a.AppUserID == hostId);

            if (accommodation == null) return false;

            // Unset current main image
            var currentMain = await _context.Images
                .Where(i => i.AccommodationID == accommodationId && i.IsMain)
                .FirstOrDefaultAsync();

            if (currentMain != null)
                currentMain.IsMain = false;

            // Set new main image
            var newMain = await _context.Images
                .FirstOrDefaultAsync(i => i.ImageID == imageId && i.AccommodationID == accommodationId);

            if (newMain == null) return false;

            newMain.IsMain = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImageAsync(int accommodationId, int imageId, string hostId)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId && a.AppUserID == hostId);

            if (accommodation == null) return false;

            var image = await _context.Images
                .FirstOrDefaultAsync(i => i.ImageID == imageId && i.AccommodationID == accommodationId);

            if (image == null) return false;

            // Delete physical file
            DeletePhysicalFile(image.ImageUrl);

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------------
        // 6. AMENITIES OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<bool> UpdateAmenitiesAsync(int accommodationId, List<int> amenityIds, string hostId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId && a.AppUserID == hostId);

            if (accommodation == null) return false;

            accommodation.Amenities.Clear();

            if (amenityIds != null && amenityIds.Any())
            {
                var amenities = await _context.Amenities
                    .Where(a => amenityIds.Contains(a.AmenityID))
                    .ToListAsync();

                foreach (var amenity in amenities)
                {
                    accommodation.Amenities.Add(amenity);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------------
        // 7. HOTEL ROOMS OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<HotelRoomDto?> AddHotelRoomAsync(int accommodationId, HotelRoomCreateDto dto, string hostId)
        {
            var accommodation = await _context.Accommodations
                .OfType<Hotel>()
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId && a.AppUserID == hostId);

            if (accommodation == null) return null;

            var hotelRoom = new HotelRoom
            {
                RoomNumber = dto.RoomNumber,
                Type = dto.Type,
                RoomDescription = dto.RoomDescription,
                PricePerNight = dto.PricePerNight,
                IsAvailable = dto.IsAvailable,
                AccommodationID = accommodationId
            };

            _context.HotelRooms.Add(hotelRoom);
            await _context.SaveChangesAsync();

            return MapToHotelRoomDto(hotelRoom);
        }

        public async Task<bool> UpdateHotelRoomAsync(int roomId, HotelRoomCreateDto dto, string hostId)
        {
            var hotelRoom = await _context.HotelRooms
                .Include(hr => hr.Hotel)
                .FirstOrDefaultAsync(hr => hr.HotelRoomID == roomId && hr.Hotel.AppUserID == hostId);

            if (hotelRoom == null) return false;

            hotelRoom.RoomNumber = dto.RoomNumber;
            hotelRoom.Type = dto.Type;
            hotelRoom.RoomDescription = dto.RoomDescription;
            hotelRoom.PricePerNight = dto.PricePerNight;
            hotelRoom.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHotelRoomAsync(int roomId, string hostId)
        {
            var hotelRoom = await _context.HotelRooms
                .Include(hr => hr.Hotel)
                .FirstOrDefaultAsync(hr => hr.HotelRoomID == roomId && hr.Hotel.AppUserID == hostId);

            if (hotelRoom == null) return false;

            _context.HotelRooms.Remove(hotelRoom);
            await _context.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------------
        // 8. STUDENT ROOMS & BEDS OPERATIONS
        // ----------------------------------------------------------------------

        public async Task<StudentRoomDto?> AddStudentRoomAsync(int accommodationId, StudentRoomCreateDto dto, string hostId)
        {
            var accommodation = await _context.Accommodations
                .OfType<StudentHouse>()
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId && a.AppUserID == hostId);

            if (accommodation == null) return null;

            var studentRoom = new StudentRoom
            {
                TotalBeds = dto.TotalBeds,
                AccommodationID = accommodationId
            };

            _context.StudentRooms.Add(studentRoom);
            await _context.SaveChangesAsync(); // نحفظ هنا للحصول على الـ ID قبل إضافة الأسرة

            // Add beds
            if (dto.Beds != null)
            {
                foreach (var bedDto in dto.Beds)
                {
                    var bed = new Bed
                    {
                        RoomDescription = bedDto.RoomDescription,
                        PricePerNight = bedDto.PricePerNight,
                        IsAvailable = bedDto.IsAvailable,
                        StudentRoomID = studentRoom.StudentRoomID
                    };
                    _context.Beds.Add(bed);
                }
            }

            await _context.SaveChangesAsync();

            // إعادة تحميل الغرفة مع الأسرة الجديدة للـ Mapping
            var createdRoom = await _context.StudentRooms
                .Include(sr => sr.Beds)
                .FirstOrDefaultAsync(sr => sr.StudentRoomID == studentRoom.StudentRoomID);

            return MapToStudentRoomDto(createdRoom!);
        }

        public async Task<bool> UpdateStudentRoomAsync(int roomId, StudentRoomUpdateDto dto, string hostId)
        {
            var studentRoom = await _context.StudentRooms
                .Include(sr => sr.StudentHouse)
                .FirstOrDefaultAsync(sr => sr.StudentRoomID == roomId && sr.StudentHouse!.AppUserID == hostId);

            if (studentRoom == null) return false;

            studentRoom.TotalBeds = dto.TotalBeds;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BedDto?> AddBedToRoomAsync(int roomId, BedCreateDto dto, string hostId)
        {
            var studentRoom = await _context.StudentRooms
                .Include(sr => sr.StudentHouse)
                .FirstOrDefaultAsync(sr => sr.StudentRoomID == roomId && sr.StudentHouse!.AppUserID == hostId);

            if (studentRoom == null) return null;

            var bed = new Bed
            {
                RoomDescription = dto.RoomDescription,
                PricePerNight = dto.PricePerNight,
                IsAvailable = dto.IsAvailable,
                StudentRoomID = roomId
            };

            _context.Beds.Add(bed);
            await _context.SaveChangesAsync();

            return MapToBedDto(bed);
        }

        public async Task<bool> UpdateBedAsync(int bedId, BedCreateDto dto, string hostId)
        {
            var bed = await _context.Beds
                .Include(b => b.StudentRoom)
                    .ThenInclude(sr => sr!.StudentHouse)
                .FirstOrDefaultAsync(b => b.BedID == bedId && b.StudentRoom!.StudentHouse!.AppUserID == hostId);

            if (bed == null) return false;

            bed.RoomDescription = dto.RoomDescription;
            bed.PricePerNight = dto.PricePerNight;
            bed.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBedAsync(int bedId, string hostId)
        {
            var bed = await _context.Beds
               .Include(b => b.StudentRoom)
                   .ThenInclude(sr => sr!.StudentHouse)
               .FirstOrDefaultAsync(b => b.BedID == bedId && b.StudentRoom!.StudentHouse!.AppUserID == hostId);

            if (bed == null) return false;

            _context.Beds.Remove(bed);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStudentRoomAsync(int roomId, string hostId)
        {
            var studentRoom = await _context.StudentRooms
                .Include(sr => sr.StudentHouse)
                .FirstOrDefaultAsync(sr => sr.StudentRoomID == roomId && sr.StudentHouse!.AppUserID == hostId);

            if (studentRoom == null) return false;

            _context.StudentRooms.Remove(studentRoom);
            await _context.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------------
        // 9. HELPER METHODS (Mappers & File Handlers)
        // ----------------------------------------------------------------------

        // Helper method to save file and return the generated filename
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }

        // Helper method to delete physical file
        private void DeletePhysicalFile(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        // Helper method to map Accommodation by ID
        private async Task<AccommodationDetailDto?> MapToAccommodationDetailDto(int accommodationId)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.Location)
                    .ThenInclude(l => l.City)
                    .ThenInclude(c => c.Governorate)
                .Include(a => a.AppUser)
                .Include(a => a.Amenities)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId);

            if (accommodation == null) return null;

            // Load related data based on type
            if (accommodation is Hotel hotel)
            {
                await _context.Entry(hotel)
                    .Collection(h => h.HotelRooms)
                    .Query()
                    .Include(hr => hr.Images)
                    .LoadAsync();
            }
            else if (accommodation is StudentHouse studentHouse)
            {
                await _context.Entry(studentHouse)
                    .Collection(sh => sh.StudentRooms)
                    .Query()
                    .Include(sr => sr.Beds)
                    .LoadAsync();
            }

            return MapToAccommodationDetailDto(accommodation);
        }

        // Helper method to map Accommodation to Detail DTO
        private AccommodationDetailDto? MapToAccommodationDetailDto(Accommodation accommodation)
        {
            if (accommodation == null) return null;

            var dto = new AccommodationDetailDto
            {
                AccommodationID = accommodation.AccommodationID,
                AccommodationName = accommodation.AccommodationName,
                AccommodationDescription = accommodation.AccommodationDescription,
                AccommodationType = accommodation.AccommodationType,
                CreatedAt = accommodation.CreatedAt,
                UpdatedAt = accommodation.UpdatedAt,
                Location = new LocationDto
                {
                    LocationID = accommodation.Location?.LocationID ?? 0,
                    Region = accommodation.Location?.Region ?? string.Empty,
                    Street = accommodation.Location?.Street ?? string.Empty,
                    CityID = accommodation.Location?.CityID ?? 0,
                    CityName = accommodation.Location?.City?.CityName ?? string.Empty,
                    GovernorateName = accommodation.Location?.City?.Governorate?.GovernorateName ?? string.Empty
                },
                HostId = accommodation.AppUserID,
                HostName = $"{accommodation.AppUser?.FirstName} {accommodation.AppUser?.LastName}",
                HostEmail = accommodation.AppUser?.Email ?? string.Empty,
                Amenities = accommodation.Amenities?.Select(MapToAmenityDto).ToList() ?? new List<AmenityDto>(),
                Images = accommodation.Images?.Select(MapToImageDto).ToList() ?? new List<ImageDto>()
            };

            // Type-specific properties
            switch (accommodation)
            {
                case Hotel hotel:
                    dto.StarsRate = hotel.StarsRate;
                    dto.HotelRooms = hotel.HotelRooms?.Select(MapToHotelRoomDto).ToList() ?? new List<HotelRoomDto>();
                    break;

                case LocalLoding localLoding:
                    dto.Area = localLoding.Area;
                    dto.Floor = localLoding.Floor;
                    dto.TotalRooms = localLoding.TotalRooms;
                    dto.TotalGuests = localLoding.TotalGuests;
                    dto.PricePerNight = localLoding.PricePerNight;
                    dto.IsAvailable = localLoding.IsAvailable;
                    break;

                case StudentHouse studentHouse:
                    dto.Area = studentHouse.Area;
                    dto.Floor = studentHouse.Floor;
                    dto.TotalGuests = studentHouse.TotalGuests;
                    dto.StudentRooms = studentHouse.StudentRooms?.Select(MapToStudentRoomDto).ToList() ?? new List<StudentRoomDto>();
                    break;
            }

            return dto;
        }

        // General Mappers
        private ImageDto MapToImageDto(Image i) => new ImageDto
        {
            ImageID = i.ImageID,
            ImageUrl = i.ImageUrl,
            AltText = i.AltText,
            IsMain = i.IsMain,
            CreatedAt = i.CreatedAt,
            AccommodationID = i.AccommodationID
        };

        private AmenityDto MapToAmenityDto(Amenity a) => new AmenityDto
        {
            AmenityID = a.AmenityID,
            AmenityName = a.AmenityName
        };

        private HotelRoomDto MapToHotelRoomDto(HotelRoom hr) => new HotelRoomDto
        {
            HotelRoomID = hr.HotelRoomID,
            RoomNumber = hr.RoomNumber,
            Type = hr.Type,
            RoomDescription = hr.RoomDescription,
            PricePerNight = hr.PricePerNight,
            IsAvailable = hr.IsAvailable,
            AccommodationID = hr.AccommodationID,
            Images = hr.Images?.Select(MapToImageDto).ToList() ?? new List<ImageDto>()
        };

        private BedDto MapToBedDto(Bed b) => new BedDto
        {
            BedID = b.BedID,
            RoomDescription = b.RoomDescription,
            PricePerNight = b.PricePerNight,
            IsAvailable = b.IsAvailable,
            StudentRoomID = b.StudentRoomID
        };

        private StudentRoomDto MapToStudentRoomDto(StudentRoom sr) => new StudentRoomDto
        {
            StudentRoomID = sr.StudentRoomID,
            TotalBeds = sr.TotalBeds,
            AccommodationID = sr.AccommodationID,
            Beds = sr.Beds?.Select(MapToBedDto).ToList() ?? new List<BedDto>()
        };
    }
}
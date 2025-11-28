using MabeetApi.DTOs.Property;
using MabeetApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MabeetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Host")]
    public class AccommodationController : ControllerBase
    {
        private readonly IAccommodationService _accommodationService;

        public AccommodationController(IAccommodationService accommodationService)
        {
            _accommodationService = accommodationService;
        }

        // ----------------------------------------------------------------------
        // 1. CREATE OPERATIONS
        // ----------------------------------------------------------------------

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AccommodationDetailDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAccommodation([FromBody] AccommodationCreateDto dto)
        {
            // استخراج AppUserID من التوكن (الـ HostId)
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            try
            {
                var newAccommodation = await _accommodationService.CreateAccommodationAsync(dto, hostId);
                // نستخدم CreatedAtAction لإرجاع 201 Created مع رابط لجلب الإقامة الجديدة
                return CreatedAtAction(
                    nameof(GetAccommodationById),
                    new { id = newAccommodation.AccommodationID },
                    newAccommodation
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                // يمكن أن يكون هناك معالجة أخطاء أكثر تحديداً هنا
                return StatusCode(500, new { Message = "An error occurred while creating the accommodation." });
            }
        }

        // ----------------------------------------------------------------------
        // 2. READ OPERATIONS (Host Specific)
        // ----------------------------------------------------------------------

        /// <summary>
        /// جلب جميع الإقامات التي يمتلكها المضيف الحالي
        /// </summary>
        /// <returns>قائمة مختصرة بالإقامات</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AccommodationListDto>))]
        public async Task<IActionResult> GetHostAccommodations()
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var accommodations = await _accommodationService.GetHostAccommodationsAsync(hostId);
            return Ok(accommodations);
        }

        /// <summary>
        /// جلب تفاصيل إقامة محددة للمضيف الحالي
        /// </summary>
        /// <param name="id">معرف الإقامة</param>
        /// <returns>تفاصيل الإقامة</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccommodationDetailDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccommodationById(int id)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var accommodation = await _accommodationService.GetAccommodationByIdAsync(id, hostId);

            if (accommodation == null)
            {
                return NotFound(new { Message = $"Accommodation with ID {id} not found or not owned by the host." });
            }

            return Ok(accommodation);
        }

        // ----------------------------------------------------------------------
        // 3. UPDATE OPERATIONS (Main Accommodation)
        // ----------------------------------------------------------------------

        /// <summary>
        /// تحديث المعلومات الأساسية للإقامة
        /// </summary>
        /// <param name="id">معرف الإقامة</param>
        /// <param name="dto">بيانات التحديث</param>
        /// <returns>تفاصيل الإقامة المحدثة</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccommodationDetailDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccommodation(int id, [FromBody] AccommodationUpdateDto dto)
        {
            if (id != dto.AccommodationID)
            {
                return BadRequest(new { Message = "Route ID and DTO ID mismatch." });
            }

            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var updatedAccommodation = await _accommodationService.UpdateAccommodationAsync(dto, hostId);

            if (updatedAccommodation == null)
            {
                return NotFound(new { Message = $"Accommodation with ID {id} not found or not owned by the host." });
            }

            return Ok(updatedAccommodation);
        }

        // ----------------------------------------------------------------------
        // 4. DELETE OPERATIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// حذف إقامة محددة
        /// </summary>
        /// <param name="id">معرف الإقامة</param>
        /// <returns>نتيجة عملية الحذف</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccommodation(int id)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.DeleteAccommodationAsync(id, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Accommodation with ID {id} not found or not owned by the host." });
            }

            return NoContent(); // 204 No Content for successful deletion
        }

        // ----------------------------------------------------------------------
        // 5. IMAGES OPERATIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// رفع صورة جديدة للإقامة
        /// </summary>
        [HttpPost("{accommodationId}/images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(int accommodationId, [FromForm] ImageUploadDto dto)
        {
            if (accommodationId != dto.AccommodationID)
            {
                return BadRequest(new { Message = "Route ID and DTO ID mismatch." });
            }

            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            if (dto.ImageFile == null)
            {
                return BadRequest(new { Message = "Image file is required." });
            }

            var result = await _accommodationService.UploadImagesAsync(dto, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Accommodation with ID {accommodationId} not found or not owned by the host." });
            }

            return Ok(new { Message = "Image uploaded successfully." });
        }

        /// <summary>
        /// تعيين صورة كصورة رئيسية للإقامة
        /// </summary>
        [HttpPut("{accommodationId}/images/{imageId}/main")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetMainImage(int accommodationId, int imageId)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.SetMainImageAsync(accommodationId, imageId, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Accommodation ID {accommodationId} or Image ID {imageId} not found or not owned by the host." });
            }

            return NoContent();
        }

        /// <summary>
        /// حذف صورة من الإقامة
        /// </summary>
        [HttpDelete("{accommodationId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteImage(int accommodationId, int imageId)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.DeleteImageAsync(accommodationId, imageId, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Accommodation ID {accommodationId} or Image ID {imageId} not found or not owned by the host." });
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------
        // 6. AMENITIES OPERATIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// تحديث قائمة المرافق (Amenities) الخاصة بالإقامة
        /// </summary>
        [HttpPut("{accommodationId}/amenities")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAmenities(int accommodationId, [FromBody] List<int> amenityIds)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.UpdateAmenitiesAsync(accommodationId, amenityIds, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Accommodation with ID {accommodationId} not found or not owned by the host." });
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------
        // 7. HOTEL ROOMS OPERATIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// إضافة غرفة فندقية جديدة إلى فندق
        /// </summary>
        [HttpPost("{accommodationId}/hotel-rooms")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(HotelRoomDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddHotelRoom(int accommodationId, [FromBody] HotelRoomCreateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var newRoom = await _accommodationService.AddHotelRoomAsync(accommodationId, dto, hostId);

            if (newRoom == null)
            {
                return NotFound(new { Message = $"Accommodation ID {accommodationId} not found, not a Hotel, or not owned by the host." });
            }

            return CreatedAtAction(
                nameof(GetAccommodationById), // افتراضًا أن تفاصيل الإقامة تعرض الغرف
                new { id = accommodationId },
                newRoom
            );
        }

        /// <summary>
        /// تحديث تفاصيل غرفة فندقية
        /// </summary>
        [HttpPut("hotel-rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHotelRoom(int roomId, [FromBody] HotelRoomCreateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.UpdateHotelRoomAsync(roomId, dto, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Hotel Room with ID {roomId} not found or not owned by the host." });
            }

            return NoContent();
        }

        /// <summary>
        /// حذف غرفة فندقية
        /// </summary>
        [HttpDelete("hotel-rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHotelRoom(int roomId)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.DeleteHotelRoomAsync(roomId, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Hotel Room with ID {roomId} not found or not owned by the host." });
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------
        // 8. STUDENT ROOMS & BEDS OPERATIONS
        // ----------------------------------------------------------------------

        /// <summary>
        /// إضافة غرفة طلابية جديدة إلى سكن طلابي
        /// </summary>
        [HttpPost("{accommodationId}/student-rooms")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StudentRoomDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStudentRoom(int accommodationId, [FromBody] StudentRoomCreateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var newRoom = await _accommodationService.AddStudentRoomAsync(accommodationId, dto, hostId);

            if (newRoom == null)
            {
                return NotFound(new { Message = $"Accommodation ID {accommodationId} not found, not a Student House, or not owned by the host." });
            }

            return CreatedAtAction(
                nameof(GetAccommodationById),
                new { id = accommodationId },
                newRoom
            );
        }

        /// <summary>
        /// تحديث عدد الأسرة في الغرفة الطلابية
        /// </summary>
        [HttpPut("student-rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStudentRoom(int roomId, [FromBody] StudentRoomUpdateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.UpdateStudentRoomAsync(roomId, dto, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Student Room with ID {roomId} not found or not owned by the host." });
            }

            return NoContent();
        }

        /// <summary>
        /// إضافة سرير جديد إلى غرفة طلابية
        /// </summary>
        [HttpPost("student-rooms/{roomId}/beds")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BedDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddBedToRoom(int roomId, [FromBody] BedCreateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var newBed = await _accommodationService.AddBedToRoomAsync(roomId, dto, hostId);

            if (newBed == null)
            {
                return NotFound(new { Message = $"Student Room with ID {roomId} not found or not owned by the host." });
            }

            return Created(string.Empty, newBed); // Created with empty string for location header
        }

        /// <summary>
        /// تحديث تفاصيل سرير محدد في غرفة طلابية
        /// </summary>
        [HttpPut("beds/{bedId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBed(int bedId, [FromBody] BedCreateDto dto)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.UpdateBedAsync(bedId, dto, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Bed with ID {bedId} not found or not owned by the host." });
            }

            return NoContent();
        }

        /// <summary>
        /// حذف سرير من غرفة طلابية
        /// </summary>
        [HttpDelete("beds/{bedId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBed(int bedId)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.DeleteBedAsync(bedId, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Bed with ID {bedId} not found or not owned by the host." });
            }

            return NoContent();
        }

        /// <summary>
        /// حذف غرفة طلابية بالكامل
        /// </summary>
        [HttpDelete("student-rooms/{roomId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteStudentRoom(int roomId)
        {
            var hostId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (hostId == null) return Unauthorized(new { Message = "Host ID not found in token." });

            var result = await _accommodationService.DeleteStudentRoomAsync(roomId, hostId);

            if (!result)
            {
                return NotFound(new { Message = $"Student Room with ID {roomId} not found or not owned by the host." });
            }

            return NoContent();
        }
    }
}
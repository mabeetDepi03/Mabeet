// js/bookings.js

document.addEventListener('DOMContentLoaded', () => {
    loadUserBookings();
});

async function loadUserBookings() {
    const container = document.getElementById('bookings-container');
    const spinner = document.getElementById('loading-spinner');
    const noBookingsMsg = document.getElementById('no-bookings-message');

    // التأكد من تسجيل الدخول
    const token = ApiService.getToken();
    if (!token) {
        window.location.href = 'login.html';
        return;
    }

    // استخراج معرف المستخدم (UserId) من التوكن
    const user = JSON.parse(localStorage.getItem('userData'));
    let userId = user.useId
    console.log(userId)
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(c => 
            '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
        ).join(''));
        const decodedToken = JSON.parse(jsonPayload);
        userId = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || decodedToken.nameid || decodedToken.sub;
    } catch (e) {
        console.error("Error parsing token:", e);
    }

    if (!userId) {
        Swal.fire('خطأ', 'تعذر التحقق من بيانات المستخدم', 'error');
        return;
    }

    try {
        // جلب الحجوزات من الـ API
        const bookings = await ApiService.get(`/Bookings/user/${userId}`);
        
        // 🟢 نأمل أن السيرفر الآن يرسل OwnerName و AccommodationName بفضل إصلاح الباك إند

        spinner.style.display = 'none';

        if (!bookings || bookings.length === 0) {
            noBookingsMsg.style.display = 'block';
            return;
        }

        container.style.display = 'block';
        container.innerHTML = '';

        // ترتيب الحجوزات (الأحدث أولاً)
        bookings.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

        const allAccommodations = await ApiService.get('/Availability/accommodations?t=' + Date.now());

        bookings.forEach(booking => {
            const card = createBookingCard(booking, allAccommodations);
            container.innerHTML += card;
        });

    } catch (error) {
        console.error("Error loading bookings:", error);
        spinner.style.display = 'none';
        Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'حدث خطأ أثناء تحميل الحجوزات: ' + error.message
        });
    }
}

// 🟢 دالة البحث عن الصورة والسعر واسم المالك (HostName)
function createBookingCard(booking, allAccommodations) {
    // 1. تحديد لون الحالة
    let statusBadge = '';
    let statusText = booking.status;
    let canCancel = false;

    switch (booking.status) {
        case 'Pending':
            statusBadge = 'bg-warning text-dark';
            statusText = 'قيد الانتظار';
            canCancel = true;
            break;
        case 'Confirmed':
            statusBadge = 'bg-success';
            statusText = 'مؤكد';
            canCancel = true; 
            break;
        case 'Cancelled':
            statusBadge = 'bg-danger';
            statusText = 'ملغي';
            break;
        case 'Completed':
            statusBadge = 'bg-primary';
            statusText = 'مكتمل';
            break;
        default:
            statusBadge = 'bg-secondary';
    }

    // تنسيق التواريخ
    const checkIn = new Date(booking.checkIN).toLocaleDateString('ar-EG');
    const checkOut = new Date(booking.checkOUT).toLocaleDateString('ar-EG');
    
    const accName = booking.accommodationName || '';
    const accMatch = allAccommodations.find(a => 
        (a.accommodationName && a.accommodationName.includes(accName)) || 
        (a.accommodationID === booking.localLodingID) // بحث سريع عن الشقق
    );
    
    let imageUrl = '';
    let totalPricePerNight = 0; // السعر لليلة الواحدة
    
    if (accMatch) {
        let rawImg = accMatch.mainImageUrl || accMatch.MainImageUrl;
        imageUrl = ApiService.getImageUrl(rawImg);
        
        // 🟢 محاولة استخراج سعر الليلة الواحدة من بيانات العقار الأم
        totalPricePerNight = accMatch.pricePerNight || accMatch.PricePerNight || 0;
    } else {
        // صورة احتياطية ذكية
        let typeKeyword = 'house';
        if(booking.accommodationType && booking.accommodationType.includes('Hotel')) typeKeyword = 'hotel';
        if(booking.accommodationType && booking.accommodationType.includes('Student')) typeKeyword = 'student room';
        imageUrl = `https://placehold.co/400x300?text=${typeKeyword}`;
    }

    // 🟢 حساب سعر الليلة الواحدة من الإجمالي
    const totalDays = (new Date(booking.checkOUT).getTime() - new Date(booking.checkIN).getTime()) / (1000 * 60 * 60 * 24);
    if (totalDays > 0 && booking.totalPrice) {
        totalPricePerNight = (booking.totalPrice / totalDays).toFixed(0);
    }
    
    return `
        <div class="booking-item shadow-sm p-3 mb-4 bg-white rounded border" id="booking-${booking.bookingID}">
            <div class="row align-items-center">
                <div class="col-md-3 mb-3 mb-md-0">
                    <img src="${imageUrl}" alt="Booking Image" class="img-fluid rounded w-100 object-fit-cover" 
                         style="height: 150px;" onerror="this.src='https://placehold.co/400x300?text=Booking'">
                </div>
                <div class="col-md-9">
                    <div class="booking-details">
                        <div class="d-flex justify-content-between align-items-start">
                            <h3 class="h5 fw-bold mb-3">${booking.accommodationName || 'حجز مكان إقامة'}</h3>
                            <span class="badge ${statusBadge} px-3 py-2 rounded-pill">${statusText}</span>
                        </div>
                        
                        <div class="row mb-3 text-muted small">
                            <div class="col-md-6 mb-2">
                                <i class="fas fa-calendar-alt text-primary me-2"></i> 
                                <strong>من:</strong> ${checkIn} 
                                <i class="fas fa-arrow-left mx-2"></i> 
                                <strong>إلى:</strong> ${checkOut}
                            </div>
                            <div class="col-md-6 mb-2">
                                <i class="fas fa-money-bill-wave text-success me-2"></i>
                                <strong>الإجمالي:</strong> ${booking.totalPrice} ج.م 
                                <small class="text-dark">(${totalPricePerNight} ج.م / ليلة)</small>
                            </div>
                            <div class="col-md-6">
                                <i class="fas fa-tag text-secondary me-2"></i>
                                <strong>النوع:</strong> ${booking.accommodationType || 'غير محدد'}
                            </div>
                            <div class="col-md-6">
                                <i class="fas fa-user-tie text-secondary me-2"></i>
                                <strong>المالك:</strong> ${booking.ownerName || '---'}
                            </div>
                        </div>

                        <div class="booking-actions mt-3 d-flex gap-2">
                            ${canCancel ? `
                                <button onclick="cancelBooking(${booking.bookingID})" class="btn btn-outline-danger btn-sm">
                                    <i class="fas fa-times-circle me-1"></i> إلغاء الحجز
                                </button>
                            ` : ''}
                            <button class="btn btn-outline-primary btn-sm" disabled>
                                <i class="fas fa-eye me-1"></i> التفاصيل
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// دالة إلغاء الحجز
window.cancelBooking = async function(id) {
    const result = await Swal.fire({
        title: 'هل أنت متأكد؟',
        text: "هل تريد إلغاء هذا الحجز؟ لا يمكن التراجع عن هذا الإجراء.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، قم بالإلغاء',
        cancelButtonText: 'تراجع'
    });

    if (result.isConfirmed) {
        try {
            // إظهار التحميل
            Swal.fire({title: 'جاري الإلغاء...', didOpen: () => Swal.showLoading()});

            // استدعاء الـ API
            const response = await fetch(`${API_BASE_URL}/Bookings/${id}/cancel`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${ApiService.getToken()}`
                }
            });

            if (response.ok) {
                await Swal.fire('تم الإلغاء!', 'تم إلغاء حجزك بنجاح.', 'success');
                loadUserBookings(); 
                throw new Error('فشل في إلغاء الحجز');
            }
        } catch (error) {
            Swal.fire('خطأ', 'حدث خطأ أثناء محاولة الإلغاء.', 'error');
        }
    }
};
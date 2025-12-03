// js/student-housing.js

document.addEventListener('DOMContentLoaded', () => {
    console.log("🚀 [StudentHousing] الصفحة بدأت التحميل...");
    loadStudentHousing();
});

async function loadStudentHousing(filters = {}) {
    const container = document.getElementById('student-container');
    const spinner = document.getElementById('loading-spinner');
    
    if(spinner) spinner.style.display = 'block';
    if(container) container.innerHTML = '';

    try {
        const today = new Date();
        const tomorrow = new Date(today);
        tomorrow.setDate(tomorrow.getDate() + 1);

        const params = {
            CheckIN: filters.CheckIN || today.toISOString(),
            CheckOUT: filters.CheckOUT || tomorrow.toISOString(),
            AccommodationType: 'StudentHouse', // فلتر من السيرفر
            ...filters
        };

        console.log("🔄 [API Request] جاري طلب السكن الطلابي...", params);
        
        // false: عشان ميطلبش تسجيل دخول
        const accommodations = await ApiService.get('/Availability/accommodations', params, false);
        
        console.log("📦 [API Response] الداتا الخام:", accommodations);

        if (!accommodations || accommodations.length === 0) {
            if(spinner) spinner.style.display = 'none';
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا يوجد سكن طلابي متاح حالياً.</div></div>';
            return;
        }

        // فلترة إضافية للتأكيد
        const housing = accommodations.filter(acc => {
            const type = (acc.accommodationType || acc.AccommodationType || "").toLowerCase();
            return type.includes('student');
        });

        if(spinner) spinner.style.display = 'none';

        if (housing.length === 0) {
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا يوجد سكن طلابي مطابق.</div></div>';
            return;
        }

        housing.forEach((house, index) => {
            console.group(`🎓 سكن طلابي ${index + 1}: ${house.accommodationName || house.AccommodationName}`);

            const id = house.accommodationID || house.AccommodationID;
            const name = house.accommodationName || house.AccommodationName;

            // 1. السعر (جاي جاهز من الباك إند بعد التعديل)
            let price = house.pricePerNight || house.PricePerNight || 0;
            console.log(`💰 السعر المستلم: ${price}`);

            // 2. حساب عدد الأسرة بدقة (للعرض فقط)
            let bedCount = 0;
            const rooms = house.studentRooms || house.StudentRooms || [];
            if (rooms.length > 0) {
                rooms.forEach(r => {
                    if(r.beds) bedCount += r.beds.length;
                    else if(r.Beds) bedCount += r.Beds.length;
                });
            } else {
                // لو مفيش غرف راجعة، نستخدم الرقم الإجمالي المخزن
                bedCount = house.totalGuests || house.TotalGuests || 0;
            }
            console.log(`🛏️ عدد الأسرة المحسوب: ${bedCount}`);

            // لو السعر لسه 0 (داتا قديمة)، نحاول نطلعه من الأسرة
            if (price === 0 && rooms.length > 0) {
                console.warn("⚠️ السعر 0، محاولة استخراجه من الأسرة...");
                rooms.forEach(r => {
                    const beds = r.beds || r.Beds || [];
                    beds.forEach(b => {
                        const p = b.pricePerNight || b.PricePerNight || 0;
                        if (p > 0 && (price === 0 || p < price)) price = p;
                    });
                });
            }

            console.groupEnd();

            // الصورة
            const imgObj = (house.images && house.images.length > 0) ? house.images[0] : null;
            const imgUrl = ApiService.getImageUrl(imgObj ? (imgObj.imageUrl || imgObj.ImageUrl) : null);

            // الموقع (المنطقة ثم المدينة)
            const loc = house.location || house.Location || {};
            const region = house.region || house.Region || loc.region || loc.Region || "مصر";
            const cityName = house.cityName || house.CityName || (loc.city ? (loc.city.cityName || loc.city.CityName) : "");
            
            const priceDisplay = price > 0 
                ? `<span class="fw-bold fs-5">${price}</span> <small>ج.م / سرير</small>` 
                : '<span class="text-muted small">تواصل للسعر</span>';
            
            const detailsLink = `property-details.html?id=${id}&checkIn=${params.CheckIN}&checkOut=${params.CheckOUT}`;

            container.innerHTML += `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 shadow-sm border-0 property-card">
                        <div class="position-relative">
                            <img src="${imgUrl}" class="card-img-top" style="height: 250px; object-fit: cover;" 
                                 alt="${name}" onerror="this.src='https://placehold.co/600x400?text=No+Image'">
                            <span class="badge position-absolute top-0 end-0 m-3" style="background-color: #6f42c1;">سكن طلابي</span>
                        </div>
                        <div class="card-body p-3">
                            <h5 class="card-title fw-bold text-dark mb-0 text-truncate">${name}</h5>
                            
                            <p class="text-muted small mb-2">
                                <i class="fas fa-map-marker-alt text-primary me-1"></i> ${region} - ${cityName}
                            </p>

                            <p class="text-muted small mb-3">
                                <i class="fas fa-users text-secondary me-1"></i> ${bedCount > 0 ? bedCount + ' سرير متاح' : 'متاح للحجز'}
                            </p>
                            
                            <div class="d-flex justify-content-between align-items-center pt-3 border-top">
                                <span style="color: #6f42c1;">${priceDisplay}</span>
                                <a href="${detailsLink}" class="btn btn-outline-primary btn-sm rounded-pill px-4" 
                                   style="border-color: #6f42c1; color: #6f42c1;">حجز سرير</a>
                            </div>
                        </div>
                    </div>
                </div>`;
        });

    } catch (e) { 
        console.error(e);
        if(spinner) spinner.style.display = 'none'; 
        container.innerHTML = '<div class="alert alert-danger">حدث خطأ في الاتصال.</div>';
    }
}
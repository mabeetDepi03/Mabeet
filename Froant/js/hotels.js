// js/hotels.js

document.addEventListener('DOMContentLoaded', () => {
    console.log("🚀 [Hotels] الصفحة بدأت التحميل...");
    loadHotels();
});

async function loadHotels(filters = {}) {
    const container = document.getElementById('hotels-container');
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
            AccommodationType: 'Hotel', 
            ...filters
        };

        console.log("🔄 [API Request] جاري طلب البيانات من السيرفر...", params);

        // 🟢 نطلب البيانات بدون إجبار تسجيل الدخول (false)
        const accommodations = await ApiService.get('/Availability/accommodations', params, false);
        
        console.log("📦 [API Response] الداتا الخام اللي وصلت:", accommodations);

        if (!accommodations || accommodations.length === 0) {
            console.warn("⚠️ السيرفر رجع مصفوفة فاضية!");
            if(spinner) spinner.style.display = 'none';
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا توجد فنادق متاحة حالياً.</div></div>';
            return;
        }

        const hotels = accommodations.filter(acc => {
            const type = (acc.accommodationType || acc.AccommodationType || "").toLowerCase();
            return type.includes('hotel');
        });

        console.log(`✅ [Filter] عدد الفنادق بعد الفلترة: ${hotels.length}`);

        if(spinner) spinner.style.display = 'none';

        hotels.forEach((hotel, index) => {
       
            console.group(`🏨 فندق ${index + 1}: ${hotel.accommodationName || hotel.AccommodationName}`);
            
            const id = hotel.accommodationID || hotel.AccommodationID;
            const name = hotel.accommodationName || hotel.AccommodationName;
      
            let finalPrice = hotel.pricePerNight || hotel.PricePerNight || 0;
            console.log(`💰 السعر المباشر (من الباك إند): ${finalPrice}`);

            const rooms = hotel.hotelRooms || hotel.HotelRooms || [];
            console.log(`🛏️ عدد الغرف المرفقة: ${rooms.length}`, rooms);

            if (finalPrice === 0 && rooms.length > 0) {
                console.log("⚠️ السعر المباشر 0، جاري البحث عن أرخص غرفة...");
                const prices = rooms.map(r => r.pricePerNight || r.PricePerNight).filter(p => p > 0);
                if (prices.length > 0) {
                    finalPrice = Math.min(...prices);
                    console.log(`✅ تم العثور على سعر بديل من الغرف: ${finalPrice}`);
                } else {
                    console.log("❌ الغرف موجودة لكن أسعارها كلها 0!");
                }
            } else if (finalPrice > 0) {
                console.log("✅ السعر وصل تمام من الباك إند!");
            } else {
                console.log("❌ السعر 0 ومفيش غرف (غالباً ده فندق قديم فاضي)");
            }

            console.groupEnd();
            // ========================================================

            // الصورة
            const imgObj = (hotel.images && hotel.images.length > 0) ? hotel.images[0] : null;
            const imgUrl = ApiService.getImageUrl(imgObj ? (imgObj.imageUrl || imgObj.ImageUrl) : null);

            // تجهيز العرض
            const priceDisplay = finalPrice > 0 
                ? `<span class="fw-bold fs-5">${finalPrice}</span> <small>ج.م / ليلة</small>` 
                : '<span class="text-muted small">السعر حسب الغرفة</span>';

            const loc = hotel.location || hotel.Location || {};
            const region = hotel.region || hotel.Region || loc.region || loc.Region || "مصر";
            const cityName = hotel.cityName || hotel.CityName || (loc.city ? (loc.city.cityName || loc.city.CityName) : "");
            
            const detailsLink = `property-details.html?id=${id}&checkIn=${params.CheckIN}&checkOut=${params.CheckOUT}`;

            container.innerHTML += `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 shadow-sm border-0 property-card">
                        <div class="position-relative">
                            <img src="${imgUrl}" class="card-img-top" style="height: 250px; object-fit: cover;" 
                                 alt="${name}" onerror="this.src='https://placehold.co/600x400?text=No+Image'">
                            <span class="badge bg-primary position-absolute top-0 end-0 m-3">فندق</span>
                        </div>
                        <div class="card-body p-3">
                            <h5 class="card-title fw-bold text-dark mb-0 text-truncate">${name}</h5>
                            <p class="text-muted small mb-2">
                                <i class="fas fa-map-marker-alt text-primary me-1"></i> ${region} - ${cityName}
                            </p>
                            
                            <div class="d-flex justify-content-between align-items-center pt-3 border-top">
                                <span class="text-primary">${priceDisplay}</span>
                                <a href="${detailsLink}" class="btn btn-outline-primary btn-sm rounded-pill px-4">التفاصيل</a>
                            </div>
                        </div>
                    </div>
                </div>`;
        });

    } catch (e) { 
        console.error("❌ Error loading hotels:", e);
        if(spinner) spinner.style.display = 'none';
        container.innerHTML = '<div class="alert alert-danger">حدث خطأ في الاتصال.</div>';
    }
}
// js/apartments.js

document.addEventListener('DOMContentLoaded', () => {
    console.log("🚀 [Apartments] الصفحة بدأت التحميل...");
    loadApartments();
});

async function loadApartments(filters = {}) {
    const container = document.getElementById('apartments-container');
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
            AccommodationType: 'LocalLoding', // اسم النوع في الباك إند

            // 🟢 التعديل: إضافة فلتر Status لضمان جلب الشقق المعتمدة فقط من الإدارة
            Status: 'Approved', 
            
            ...filters
        };

        console.log("🔄 [API Request] جاري طلب الشقق المعتمدة...", params);
        // false: عشان ميطلبش تسجيل دخول
        const accommodations = await ApiService.get('/Availability/accommodations', params, false);
        console.log("📦 [API Response] الداتا الخام للشقق:", accommodations);
        
        if (!accommodations || accommodations.length === 0) {
            if(spinner) spinner.style.display = 'none';
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا توجد شقق متاحة حالياً.</div></div>';
            return;
        }

        // فلترة للتأكيد (بندور على LocalLoding أو Apartment)
        const apartments = accommodations.filter(acc => {
            const type = (acc.accommodationType || acc.AccommodationType || "").toLowerCase();
            return type.includes('local') || type.includes('apartment') || type.includes('loding');
        });

        if(spinner) spinner.style.display = 'none';

        if (apartments.length === 0) {
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا توجد شقق مطابقة.</div></div>';
            return;
        }

        apartments.forEach((apt, index) => {
            console.group(`🏠 شقة ${index + 1}: ${apt.accommodationName || apt.AccommodationName}`);
            
            const id = apt.accommodationID || apt.AccommodationID;
            const name = apt.accommodationName || apt.AccommodationName;

            // السعر (يُفترض وجوده في ListDto لـ LocalLoding)
            const price = apt.pricePerNight || apt.PricePerNight || 0;
            console.log(`💰 السعر: ${price}`);

            // الموقع (المنطقة + المدينة)
            const loc = apt.location || apt.Location || {};
            const region = apt.region || apt.Region || loc.region || loc.Region || "مصر";
            const cityName = apt.cityName || apt.cityName || (loc.city ? (loc.city.cityName || loc.city.CityName) : "");
            
            console.log(`📍 الموقع: ${region} - ${cityName}`);
            console.groupEnd();

            // الصورة
            const imgObj = (apt.images && apt.images.length > 0) ? apt.images[0] : null;
            const imgUrl = ApiService.getImageUrl(apt.mainImageUrl || apt.MainImageUrl || (imgObj ? (imgObj.imageUrl || imgObj.ImageUrl) : null));

            const priceDisplay = price > 0 
                ? `<span class="fw-bold fs-5">${price}</span> <small>ج.م / ليلة</small>` 
                : '<span class="text-muted small">تواصل للسعر</span>';

            const detailsLink = `property-details.html?id=${id}&checkIn=${params.CheckIN}&checkOut=${params.CheckOUT}`;

            container.innerHTML += `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 shadow-sm border-0 property-card">
                        <div class="position-relative">
                            <img src="${imgUrl}" class="card-img-top" style="height: 250px; object-fit: cover;" 
                                 alt="${name}" onerror="this.src='https://placehold.co/600x400?text=No+Image'">
                            <span class="badge bg-success position-absolute top-0 end-0 m-3">شقة سكنية</span>
                        </div>
                        <div class="card-body p-3">
                            <h5 class="card-title fw-bold text-dark mb-2">${name}</h5>
                            
                            <p class="text-muted small mb-3">
                                <i class="fas fa-map-marker-alt text-success me-1"></i> ${region} - ${cityName}
                            </p>
                            
                            <div class="d-flex justify-content-between align-items-center pt-3 border-top">
                                <span class="text-success">${priceDisplay}</span>
                                <a href="${detailsLink}" class="btn btn-outline-success btn-sm rounded-pill px-4">التفاصيل</a>
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
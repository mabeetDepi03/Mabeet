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
        // ⭐️ قراءة قيم الفلاتر من واجهة المستخدم
        const checkInElement = document.getElementById('checkIn');
        const checkOutElement = document.getElementById('checkOut');
        const cityFilterElement = document.getElementById('cityFilter');

        const checkInValue = checkInElement ? checkInElement.value : new Date().toISOString().split('T')[0];
        const checkOutValue = checkOutElement ? checkOutElement.value : new Date(new Date().setDate(new Date().getDate() + 1)).toISOString().split('T')[0];
        const CheckIN = checkInValue ? new Date(checkInValue).toISOString() : new Date().toISOString();
        const CheckOUT = checkOutValue ? new Date(checkOutValue).toISOString() : new Date(new Date().setDate(new Date().getDate() + 1)).toISOString();

        const targetGovernorates = "سوهاج,القاهرة,الجيزة,الإسكندرية,المنوفية,الإسماعيلية";
        
        // القيمة التي تستخدم لفلترة الـ API (عادةً تكون المحافظة)
        const cityFilterValue = cityFilterElement ? cityFilterElement.value : ''; 
        const governorateFilter = cityFilterValue || targetGovernorates;

        // 🆕 جلب اسم المدينة الذي اختاره المستخدم من الـ Dropdown لفلترة النتائج محلياً
        let selectedCityNameForClientFilter = '';
        if (cityFilterElement && cityFilterValue) {
            const selectedOption = cityFilterElement.options[cityFilterElement.selectedIndex];
            selectedCityNameForClientFilter = selectedOption ? selectedOption.textContent : '';
        }
        
        // 1. تحديد بارامترات الطلب (للفلترة الأولية على الـ API)
        const params = {
            CheckIN: CheckIN,
            CheckOUT: CheckOUT,
            AccommodationType: 'LocalLoding',
            Governorate: governorateFilter, 
            Status: 'Approved', 
            ...filters
        };
        
        delete params.cityFilter;
        delete params.CityID; 
        delete params.CityName;
        if (filters.CheckIN) params.CheckIN = filters.CheckIN;
        if (filters.CheckOUT) params.CheckOUT = filters.CheckOUT;
        if (filters.Governorate) params.Governorate = filters.Governorate;
        
        console.log("🔄 [API Request] جاري طلب الشقق المعتمدة والمفلترة...", params);

        // جلب البيانات من API
        const accommodations = await ApiService.get('/Availability/accommodations', params, false);
        console.log("📦 [API Response] الداتا الخام للشقق:", accommodations);
        
        // =================================================================
        // 🌟 دالة مساعدة لاستخراج بيانات المدينة بدقة
        // =================================================================
        const mapCityData = (acc) => {
            const loc = acc.location || acc.Location || {};
            const governorateName = acc.region || acc.Region || loc.region || loc.Region;
            let cityName = acc.cityName || acc.CityName; 
            
            // التعامل مع حقل city الذي قد يكون مصفوفة
            if (!cityName && loc.city) {
                let cityData = loc.city;
                if (Array.isArray(cityData) && cityData.length > 0) {
                    cityData = cityData[0]; 
                }
                if (cityData) {
                    cityName = cityData.cityName || cityData.CityName;
                }
            }
            return { 
                governorateName, 
                // نستخدم اسم المدينة المستخرج، وإلا نرجع لاسم المحافظة كاسم عرض للمدينة
                cityName: cityName || governorateName 
            };
        };

        // =================================================================
        // 1. ملء فلتر المدن من بيانات العقارات المسترجعة
        // =================================================================
        if (cityFilterElement) {
            
            const uniqueGovernorates = new Set();
            const citiesMap = {}; // مفتاح: اسم المحافظة | قيمة: اسم المدينة للعرض

            accommodations.forEach(acc => {
                const data = mapCityData(acc);
                if (data.governorateName && !uniqueGovernorates.has(data.governorateName)) { 
                    uniqueGovernorates.add(data.governorateName);
                    citiesMap[data.governorateName] = data.cityName;
                }
            });

            // إعادة بناء قائمة الفلتر
            cityFilterElement.innerHTML = '<option value="">جميع المدن المتاحة</option>';
            
            // ترتيب وعرض المدن/المحافظات
            Object.keys(citiesMap).sort().forEach(governorate => {
                const option = document.createElement('option');
                option.value = governorate; // القيمة المرسلة للـ API
                option.textContent = citiesMap[governorate]; // اسم المدينة المعروض
                
                if (governorate === cityFilterValue) {
                    option.selected = true;
                }
                cityFilterElement.appendChild(option);
            });
            console.log(`✅ [City Filter] تم تحديث الفلتر ليعرض ${Object.keys(citiesMap).length} مدينة من بيانات الشقق المتاحة.`);
        }
        
        // =================================================================

        if (!accommodations || accommodations.length === 0) {
            if(spinner) spinner.style.display = 'none';
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا توجد شقق متاحة حالياً أو مطابقة للفلتر.</div></div>';
            return;
        }
        
        // 🚨 الخطوة الثانية: تطبيق فلترة إضافية على جانب العميل بناءً على اسم المدينة الظاهر
        let filteredAccommodations = accommodations;

        if (selectedCityNameForClientFilter && selectedCityNameForClientFilter !== 'جميع المدن المتاحة') {
            filteredAccommodations = accommodations.filter(acc => {
                const data = mapCityData(acc);
                // الفلترة تتم على اسم المدينة المستخرج بدقة
                return data.cityName === selectedCityNameForClientFilter;
            });
            console.log(`✅ [Client Filter] تم تصفية النتائج إلى ${filteredAccommodations.length} نتيجة بناءً على المدينة: ${selectedCityNameForClientFilter}`);
        }

        // تطبيق فلترة النوع على النتائج المفلترة النهائية
        const apartments = filteredAccommodations.filter(acc => {
            const type = (acc.accommodationType || acc.AccommodationType || "").toLowerCase();
            return type.includes('local') || type.includes('apartment') || type.includes('loding');
        });

        if(spinner) spinner.style.display = 'none';

        if (apartments.length === 0) {
            container.innerHTML = '<div class="col-12 text-center"><div class="alert alert-info">لا توجد شقق مطابقة للفلتر المحدد.</div></div>';
            return;
        }

        container.innerHTML = '';
        apartments.forEach((apt, index) => {
            const id = apt.accommodationID || apt.AccommodationID;
            const name = apt.accommodationName || apt.AccommodationName;
            const price = apt.pricePerNight || apt.PricePerNight || 0;
            const data = mapCityData(apt); // استخدام الدالة المستحدثة
            const finalCityName = data.cityName; // اسم المدينة النهائي للعرض
            
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
                                <i class="fas fa-map-marker-alt text-success me-1"></i> ${finalCityName}
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
window.applyUnifiedFilter = function() {
    loadApartments({});
}
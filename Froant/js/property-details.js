
let currentPrice = 0;
let swiperInstance = null;  
let choicesInstance = null; 
let datePicker = null;

let selectedUnitId = null;
let selectedUnitType = null;
let selectedUnitLabel = null;

document.addEventListener('DOMContentLoaded', () => {
    console.log("🚀 [Property Details] الصفحة بدأت التحميل...");

    datePicker = flatpickr("#dateRange", {
        mode: "range",
        minDate: "today",
        dateFormat: "Y-m-d",
        locale: "ar",
        onChange: function(selectedDates, dateStr, instance) {
            if (selectedDates.length === 2) {
                document.getElementById('checkIn').value = instance.formatDate(selectedDates[0], "Y-m-d");
                document.getElementById('checkOut').value = instance.formatDate(selectedDates[1], "Y-m-d");
                calculateTotal(); 
            }
        }
    });

    // 2. تهيئة مكتبة القائمة المنسدلة (Choices.js)
    const unitSelectElement = document.getElementById('unitSelect');
    if (unitSelectElement) {
        choicesInstance = new Choices(unitSelectElement, {
            searchEnabled: false,
            itemSelectText: '',
            placeholder: true,
            placeholderValue: 'جاري تحميل الوحدات...',
            noResultsText: 'لا توجد وحدات متاحة',
            shouldSort: false,
        });
    }

    // 3. بدء التحميل
    const urlParams = new URLSearchParams(window.location.search);
    const id = urlParams.get('id');
    const checkInParam = urlParams.get('checkIn');
    const checkOutParam = urlParams.get('checkOut');

    if (!id) {
        Swal.fire('خطأ', 'رابط غير صحيح', 'error').then(() => window.location.href = 'index.html');
        return;
    }

    if(checkInParam && checkOutParam) {
        document.getElementById('checkIn').value = checkInParam.split('T')[0];
        document.getElementById('checkOut').value = checkOutParam.split('T')[0];
        if(datePicker) datePicker.setDate([checkInParam, checkOutParam]);
    }

    loadDetails(id);

    const bookBtn = document.getElementById('bookBtn');
    if(bookBtn) bookBtn.addEventListener('click', proceedToBooking);
});

async function loadDetails(id) {
    const spinner = document.getElementById('loading-spinner');
    const content = document.getElementById('details-content');

    try {
        if(spinner) spinner.style.display = 'block';
        if(content) content.style.display = 'none';

        // طلب البيانات
        const property = await ApiService.get(`/Availability/accommodation/${id}`, {}, false);
        console.log("📦 [API Response]", property);

        if (!property) throw new Error("لم يتم العثور على العقار");

        // 1. النصوص والموقع
        setText('header-title', property.accommodationName || property.AccommodationName);
        setText('prop-desc', property.accommodationDescription || property.AccommodationDescription || "لا يوجد وصف.");
        
        const loc = property.location || property.Location || {};
        const region = property.region || property.Region || loc.region || loc.Region || "مصر";
        const city = property.cityName || property.CityName || (loc.city ? (loc.city.cityName || loc.city.CityName) : "");
        setText('prop-location', `${region}, ${city}`);

        // 2. السلايدر
        setupSwiperImages(property);

        // 3. إعداد القائمة والسعر
        setupUnitsDropdown(property);

        if(spinner) spinner.style.display = 'none';
        if(content) content.style.display = 'block';

        calculateTotal();

    } catch (error) {
        console.error("❌ Error:", error);
        if(spinner) spinner.innerHTML = `<div class="alert alert-danger">حدث خطأ: ${error.message}</div>`;
    }
}

function setText(id, text) {
    const el = document.getElementById(id);
    if(el) el.innerText = text;
}

function setupSwiperImages(property) {
    const wrapper = document.getElementById('swiper-wrapper');
    if(!wrapper) return;
    wrapper.innerHTML = '';
    let allImages = [];

    const mainImgRaw = property.mainImageUrl || property.MainImageUrl || (property.images?.[0]?.imageUrl);
    if(mainImgRaw) allImages.push(ApiService.getImageUrl(mainImgRaw));

    if (property.images && property.images.length > 0) {
        property.images.forEach(img => {
            const url = ApiService.getImageUrl(img.imageUrl || img.ImageUrl);
            if (!allImages.includes(url)) allImages.push(url);
        });
    }

    if(allImages.length === 0) {
        wrapper.innerHTML = `<div class="swiper-slide"><img src="https://placehold.co/800x400?text=No+Image" alt="Placeholder"></div>`;
    } else {
        allImages.forEach(url => {
            wrapper.innerHTML += `<div class="swiper-slide"><img src="${url}" alt="Property Image"></div>`;
        });
    }

    if (swiperInstance) swiperInstance.destroy();
    swiperInstance = new Swiper(".mySwiper", {
        loop: allImages.length > 1,
        autoplay: allImages.length > 1 ? { delay: 4000, disableOnInteraction: false } : false,
        pagination: { el: ".swiper-pagination", clickable: true },
        navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" },
        effect: 'fade', fadeEffect: { crossFade: true },
        allowTouchMove: allImages.length > 1,
    });
}

// 🟢 دالة إعداد الوحدات (مع حفظ الحالة في متغيرات عامة)
function setupUnitsDropdown(property) {
    const type = (property.accommodationType || property.AccommodationType || "").toLowerCase();
    let choicesArray = [];
    console.group("🛏️ إعداد الوحدات...");

    let defaultPrice = property.pricePerNight || property.PricePerNight || 0;
    let isFirstSelected = false; 

    // --- منطق الفنادق والسكن الطلابي ---
    if (type.includes('hotel') || type.includes('student')) {
        const rooms = type.includes('hotel') ? (property.hotelRooms || property.HotelRooms) : (property.studentRooms || property.StudentRooms) || [];
        
        if (rooms.length > 0) {
            rooms.forEach(room => {
                const units = type.includes('hotel') ? [room] : (room.beds || room.Beds || []);
                units.forEach((unit, idx) => {
                    if(unit.isAvailable !== false) {
                        const p = unit.pricePerNight || unit.PricePerNight || defaultPrice;
                        const uId = unit.hotelRoomID || unit.HotelRoomID || unit.bedID || unit.BedID;
                        const uType = type.includes('hotel') ? 'HotelRoomID' : 'BedID';
                        const uLabel = type.includes('hotel') ? `غرفة ${unit.roomNumber}` : `سرير ${idx + 1}`;

                        // اختيار تلقائي للأول وحفظه في المتغيرات العامة
                        const shouldSelect = !isFirstSelected;
                        if (shouldSelect) {
                            isFirstSelected = true;
                            currentPrice = p;
                            // 🟢 الحفظ هنا هو الحل
                            selectedUnitId = uId;
                            selectedUnitType = uType;
                            selectedUnitLabel = uLabel;
                        }

                        choicesArray.push({
                            value: uId,
                            label: uLabel,
                            customProperties: { price: p, type: uType },
                            selected: shouldSelect 
                        });
                    }
                });
            });
        }
    } 
    // --- منطق الشقق ---
    else {
        const uId = property.accommodationID || property.AccommodationID;
        const uType = 'LocalLodingID';
        const uLabel = 'شقة كاملة';
        
        choicesArray.push({
            value: uId,
            label: uLabel,
            customProperties: { price: defaultPrice, type: uType },
            selected: true 
        });
        
        currentPrice = defaultPrice;
        // 🟢 الحفظ
        selectedUnitId = uId;
        selectedUnitType = uType;
        selectedUnitLabel = uLabel;
        isFirstSelected = true;
    }

    if(choicesArray.length === 0) {
        choicesArray.push({ value: '0', label: 'السعر الأساسي للمبنى', disabled: true, selected: true });
        currentPrice = defaultPrice;
    }

    if(choicesInstance) {
        choicesInstance.setChoices(choicesArray, 'value', 'label', true);
    }

    console.log(`✅ تم تحديد الوحدة: ${selectedUnitId} (${selectedUnitType}) بسعر: ${currentPrice}`);

    // الاستماع للتغيير وتحديث المتغيرات العامة
    document.getElementById('unitSelect').addEventListener('change', function() {
        if(!choicesInstance) return;
        const selectedValue = choicesInstance.getValue(true);
        if (selectedValue && selectedValue.customProperties) {
            currentPrice = parseFloat(selectedValue.customProperties.price);
            
            // 🟢 تحديث المتغيرات عند تغيير المستخدم للاختيار
            selectedUnitId = selectedValue.value;
            selectedUnitType = selectedValue.customProperties.type;
            selectedUnitLabel = selectedValue.label;
            
            calculateTotal();
        }
    });
    
    console.groupEnd();
}

function calculateTotal() {
    const priceEl = document.getElementById('prop-price');
    const totalDisplay = document.getElementById('total-price-display');
    const checkInVal = document.getElementById('checkIn').value;
    const checkOutVal = document.getElementById('checkOut').value;

    if(!priceEl) return;
    priceEl.innerText = (currentPrice > 0) ? currentPrice : "0";

    if(!currentPrice || currentPrice === 0) {
        if(totalDisplay) totalDisplay.classList.add('d-none');
        return;
    }

    if (checkInVal && checkOutVal) {
        const d1 = new Date(checkInVal);
        const d2 = new Date(checkOutVal);
        if (d2 > d1) {
            const diffDays = Math.ceil(Math.abs(d2 - d1) / (1000 * 60 * 60 * 24)); 
            const total = diffDays * currentPrice;
            if(totalDisplay) {
                totalDisplay.classList.remove('d-none');
                totalDisplay.innerHTML = `
                    <div class="d-flex justify-content-between text-secondary mb-1">
                        <span>المدة:</span> <span>${diffDays} ليلة</span>
                    </div>
                    <div class="d-flex justify-content-between fw-bold" style="color: #1B3C53; font-size: 1.2rem;">
                        <span>الإجمالي:</span> <span>${total} ج.م</span>
                    </div>
                `;
            }
        } else { if(totalDisplay) totalDisplay.classList.add('d-none'); }
    }
}

function proceedToBooking(e) {
    if(e) e.preventDefault();
    console.log("🚀 جاري تنفيذ الحجز...");

    // 1. التحقق من التوكن
    const token = ApiService.getToken();
    if (!token) {
        Swal.fire({ icon: 'warning', title: 'تنبيه', text: 'يجب تسجيل الدخول أولاً', confirmButtonText: 'دخول' })
            .then(() => { localStorage.setItem('redirectAfterLogin', window.location.href); window.location.href = 'login.html'; });
        return;
    }

    // 2. التحقق من المستخدم
    const userDataStr = localStorage.getItem('userData');
    let userId = null;
    if(userDataStr) { try { userId = JSON.parse(userDataStr).id; } catch(e) {} }

    if (!userId) { Swal.fire('خطأ', 'بيانات المستخدم غير مكتملة (ID)', 'error'); return; }

    // 3. التحقق من الوحدة (نستخدم المتغيرات العامة المضمونة)
    console.log(`🔎 الوحدة المختارة: ID=${selectedUnitId}, Type=${selectedUnitType}`);

    if (!selectedUnitId || !selectedUnitType || selectedUnitId == "0") {
        Swal.fire('تنبيه', 'يرجى اختيار وحدة متاحة للحجز', 'warning');
        return;
    }
    
    // 4. التحقق من التواريخ
    const checkIn = document.getElementById('checkIn').value;
    const checkOut = document.getElementById('checkOut').value;

    if (!checkIn || !checkOut) { Swal.fire('تنبيه', 'يرجى اختيار التواريخ', 'warning'); return; }

    const bookingData = {
        userId: userId,
        checkIN: new Date(checkIn).toISOString(),
        checkOUT: new Date(checkOut).toISOString(),
        // نستخدم المتغيرات العامة هنا بدلاً من القراءة من المكتبة
        localLodingID: (selectedUnitType === 'LocalLodingID') ? parseInt(selectedUnitId) : null,
        hotelRoomID: (selectedUnitType === 'HotelRoomID') ? parseInt(selectedUnitId) : null,
        bedID: (selectedUnitType === 'BedID') ? parseInt(selectedUnitId) : null,
        _displayPrice: currentPrice,
        _displayName: document.getElementById('header-title').innerText,
        _displayUnit: selectedUnitLabel
    };
    
    console.log("📦 البيانات المرسلة:", bookingData);
    localStorage.setItem('pendingBooking', JSON.stringify(bookingData));
    window.location.href = 'pay.html';
}
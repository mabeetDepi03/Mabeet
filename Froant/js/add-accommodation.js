document.addEventListener('DOMContentLoaded', () => {
    // 💡 أولاً: جلب المدن وملء قائمة الاختيار (باستخدام البيانات الثابتة والمفلترة)
    loadCities(); 
    
    // 🔍 ثانيًا: جلب كل العقارات وطباعتها في الكونسول (لا يزال يعمل إذا كان التوكين صحيحاً)
    loadAllAccommodationsData(); 

    const urlParams = new URLSearchParams(window.location.search);
    const id = urlParams.get('id');

    if (id) {
        document.getElementById('pageTitle').innerHTML = '<i class="fas fa-edit"></i> تعديل العقار';
        document.getElementById('submitBtn').innerText = 'حفظ التعديلات';
        loadAccommodationForEdit(id);
    } 
});

let currentLocationId = 0; 
let hasExistingUnits = false; 

// 🆕 الدالة الجديدة لتحميل كل العقارات وطباعتها
async function loadAllAccommodationsData() {
    try {
        const token = ApiService.getToken(); 
        
        if (!token) {
            console.warn("⚠️ [Auth] لا يوجد توكين. لا يمكن جلب بيانات العقارات. يرجى تسجيل الدخول.");
            return; 
        }

        console.log("%c🌐 [GET] جاري جلب كل بيانات العقارات...", "color: purple; font-weight: bold;");
        
        // استخدام GET /api/Accommodation لجلب كل العقارات
        const response = await fetch(`${API_BASE_URL}/Accommodation`, {
            headers: { 'Authorization': `Bearer ${token}` } 
        }); 
        
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`فشل في جلب كل العقارات. (رمز الحالة: ${response.status}. الرد من الخادم: ${errorText.substring(0, 100)})`);
        }
        
        const allAccommodations = await response.json();
        
        console.log("📦 [API Response - All Accommodations] البيانات الخام الكاملة المستلمة:", allAccommodations);
        
        console.log(`✅ [Success] تم تحميل ${allAccommodations.length} عقار بنجاح.`);

    } catch (error) {
        console.error("❌ خطأ أثناء تحميل كل العقارات:", error);
    }
}


// دالة جلب المدن (باستخدام البيانات الثابتة المفلترة)
async function loadCities() {
    try {
        console.log("%c🏙️ [Local Data] جاري تحميل قائمة المحافظات المفلترة...", "color: orange; font-weight: bold;");
        
        // 🔴 البيانات الثابتة المفلترة التي طلبها المستخدم
        const governorates = [
            "سوهاج", "القاهرة", "الجيزة", "الإسكندرية", "المنوفية", "الإسماعيلية"
        ];
        
        const citySelect = document.getElementById('cityId');
        
        citySelect.innerHTML = '<option value="" disabled selected>-- اختر المحافظة --</option>'; 
        
        // يتم استخدام اسم المحافظة كقيمة (Value) وكعرض (Text Content) مؤقتاً
        governorates.forEach((city, index) => {
            const option = document.createElement('option');
            // 💡 استخدام (index + 1) كـ CityID مؤقت إذا كان يحتاجه السيرفر
            option.value = index + 1; 
            option.textContent = city;
            citySelect.appendChild(option);
        });
        
        console.log(`✅ [Success] تم تحميل ${governorates.length} محافظة بنجاح من بيانات ثابتة ومفلترة.`);

    } catch (error) {
        console.error("❌ خطأ أثناء تحميل المدن الثابتة:", error);
        Swal.fire('خطأ', `تعذر بناء قائمة المدن.`, 'error');
    }
}


async function loadAccommodationForEdit(id) {
    try {
        const token = ApiService.getToken();
        console.log(`%c📥 [GET] جاري جلب بيانات العقار رقم: ${id}`, "color: blue; font-weight: bold;");

        const response = await fetch(`${API_BASE_URL}/Accommodation/${id}`, {
            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' }
        });

        if (!response.ok) throw new Error("لم يتم العثور على العقار.");
        const prop = await response.json();

        console.log("📦 [API Response] البيانات المستلمة كاملة:", prop);

        // تعبئة البيانات الأساسية
        document.getElementById('name').value = prop.accommodationName || prop.AccommodationName || "";
        document.getElementById('description').value = prop.accommodationDescription || prop.AccommodationDescription || "";
        document.getElementById('price').value = prop.pricePerNight || prop.PricePerNight || 0;
        
        // تعبئة النوع
        const typeSelect = document.getElementById('type');
        const type = (prop.accommodationType || prop.AccommodationType || "").toLowerCase();
        for(let i=0; i<typeSelect.options.length; i++){
            if(typeSelect.options[i].value.toLowerCase() === type){
                typeSelect.selectedIndex = i;
                break;
            }
        }
        window.toggleUnitCount(); // استدعاء الدالة كـ window.toggleUnitCount لتجنب خطأ undefined

        // 🟢 (مهم) حساب وعرض العدد الموجود حالياً في الداتا بيز
        // ده بيعرفنا الـ API راجعة بـ كام غرفة
        if(prop.hotelRooms && prop.hotelRooms.length > 0) {
            console.log(`🔢 [Count] وجدنا ${prop.hotelRooms.length} غرفة فندقية مسجلة.`);
            document.getElementById('unitCount').value = prop.hotelRooms.length;
            hasExistingUnits = true;
        } else if (prop.studentRooms && prop.studentRooms.length > 0) {
            // حساب إجمالي الأسرة في كل الغرف الطلابية
            let bedCount = 0;
            prop.studentRooms.forEach(r => { if(r.beds) bedCount += r.beds.length; });
            console.log(`🔢 [Count] وجدنا ${bedCount} سرير طلابي مسجل.`);
            document.getElementById('unitCount').value = bedCount > 0 ? bedCount : 1;
            hasExistingUnits = bedCount > 0;
        } else {
            console.log("ℹ️ [Info] هذا العقار لا يحتوي على أي وحدات (فارغ).");
            hasExistingUnits = false;
        }

        // تعبئة الموقع
        if (prop.location) {
            document.getElementById('region').value = prop.location.region || "";
            document.getElementById('street').value = prop.location.street || "";
            document.getElementById('cityId').value = prop.location.cityID || 1;
            currentLocationId = prop.location.locationID || prop.LocationID || 0;
        }

        // عرض الصورة
        const imgSection = document.getElementById('existingImagesSection');
        const imgContainer = document.getElementById('existingImagesContainer');
        const mainImg = prop.mainImageUrl || prop.MainImageUrl || (prop.images?.[0]?.imageUrl);
        
        if(mainImg) {
            imgSection.classList.remove('hidden');
            imgContainer.innerHTML = `
                <div class="img-wrapper">
                    <img src="${ApiService.getImageUrl(mainImg)}" alt="Current Image">
                    <span class="badge bg-primary position-absolute top-0 start-0 m-1">الحالية</span>
                </div>`;
        }

        document.getElementById('addForm').dataset.editId = id;

    } catch (error) {
        console.error(error);
        Swal.fire('خطأ', 'تعذر تحميل بيانات العقار', 'error');
    }
}

document.getElementById('addForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const id = e.target.dataset.editId;
    const isEdit = !!id;
    const token = ApiService.getToken();

    if (!token) { window.location.href = 'login.html'; return; }

    const priceVal = parseFloat(document.getElementById('price').value);
    
    const unitCountVal = parseInt(document.getElementById('unitCount').value) || 1;
    const typeVal = document.getElementById('type').value;

    console.log(`📝 [Input] المالك أدخل عدد وحدات: ${unitCountVal}`);
    console.log(`💰 [Input] السعر المدخل: ${priceVal}`);

    const payload = {
        accommodationID: isEdit ? parseInt(id) : 0, 
        accommodationName: document.getElementById('name').value,
        accommodationDescription: document.getElementById('description').value,
        accommodationType: typeVal,
        pricePerNight: isNaN(priceVal) ? 0 : priceVal,
        location: {
            cityID: parseInt(document.getElementById('cityId').value),
            region: document.getElementById('region').value,
            street: document.getElementById('street').value,
            cityName: document.getElementById('cityId').options[document.getElementById('cityId').selectedIndex].text,
            governorateName: document.getElementById('cityId').options[document.getElementById('cityId').selectedIndex].text // استخدام اسم المحافظة كـ governorateName
        },
        starsRate: 1, 
        area: 50, floor: 1, 

        totalRooms: unitCountVal, 
        totalGuests: unitCountVal, 
        amenityIds: [],
        
        hotelRooms: [],
        studentRooms: []
    };


    if (!isEdit || !hasExistingUnits) { 
        console.log(`⚙️ [Logic] جاري إنشاء ${unitCountVal} وحدات جديدة من نوع ${typeVal}...`);
        
        if (typeVal === 'Hotel') {
            for(let i=1; i<=unitCountVal; i++) {
                payload.hotelRooms.push({
                    roomNumber: 100 + i, 
                    type: 1,
                    roomDescription: "غرفة قياسية",
                    pricePerNight: priceVal, 
                    isAvailable: true,
                    imageIDs: []
                });
            }
        } else if (typeVal === 'StudentHouse') {
            const beds = [];
            for(let i=1; i<=unitCountVal; i++) {
                beds.push({
                    roomDescription: "سرير طالب",
                    pricePerNight: priceVal,
                    isAvailable: true
                });
            }
            payload.studentRooms.push({
                totalBeds: unitCountVal,
                beds: beds
            });
        }
        console.log(`✅ [Logic] تم تجهيز ${payload.hotelRooms.length + payload.studentRooms.length} عنصر للإرسال.`);
    } else {
        console.log("⚠️ [Logic] تعديل عقار موجود بالفعل وبه وحدات، لن نقوم بإعادة إنشاء الوحدات لتجنب التكرار.");
    }

    if (isEdit && currentLocationId > 0) {
        payload.location.locationID = currentLocationId; 
    }

    console.log("📤 [Payload] البيانات النهائية التي سيتم إرسالها:", payload);

    Swal.fire({ title: 'جاري الحفظ...', didOpen: () => Swal.showLoading() });

    try {
        const url = isEdit 
            ? `${API_BASE_URL}/Accommodation/${id}` 
            : `${API_BASE_URL}/Accommodation`;
            
        const method = isEdit ? 'PUT' : 'POST';

        const response = await fetch(url, {
            method: method,
            headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const txt = await response.text();
            throw new Error(txt);
        }

        let savedId = id;
        if (!isEdit) {
            const data = await response.json();
            savedId = data.accommodationID || data.AccommodationID || data.id;
            console.log(`✅ [Success] تم إنشاء العقار بنجاح برقم: ${savedId}`);
        }

        // رفع الصورة
        const fileInput = document.getElementById('imageFile');
        if (fileInput.files.length > 0 && savedId) {
            console.log("📸 [Image] جاري رفع الصورة...");
            const formData = new FormData();
            formData.append('ImageFile', fileInput.files[0]);
            formData.append('AccommodationID', savedId);
            formData.append('IsMain', 'true'); 
            formData.append('AltText', payload.accommodationName);

            const imgResp = await fetch(`${API_BASE_URL}/Accommodation/${savedId}/images`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData
            });

            if(!imgResp.ok) console.warn("⚠️ [Image] فشل رفع الصورة");
            else console.log("✅ [Image] تم رفع الصورة بنجاح");
        }

        await Swal.fire({
            icon: 'success',
            title: 'تمت العملية بنجاح!',
            text: `تم حفظ العقار بـ ${unitCountVal} وحدات.`,
            showConfirmButton: true,
            confirmButtonText: 'موافق', 
            allowOutsideClick: false 
        });

 
        window.location.href = 'owner-dashboard.html';

    } catch (error) {
        console.error("❌ خطأ أثناء الحفظ:", error);
        let msg = error.message;
        try { const j = JSON.parse(msg); if(j.errors) msg = JSON.stringify(j.errors); } catch(e){}
        Swal.fire({ icon: 'error', title: 'فشل الحفظ', text: msg });
    }
});

window.toggleUnitCount = function() {
    const type = document.getElementById('type').value;
    const group = document.getElementById('unitCountGroup');
    const label = document.getElementById('unitCountLabel');
    
    if (type === 'Hotel') {
        group.style.display = 'block';
        label.innerText = 'عدد الغرف الفندقية';
    } else if (type === 'StudentHouse') {
        group.style.display = 'block';
        label.innerText = 'عدد الأسرة المتاحة';
    } else {
        group.style.display = 'none';
        document.getElementById('unitCount').value = 1;
    }
};


const token = localStorage.getItem('userToken');
const role = localStorage.getItem('userRole');

if (!token || role !== 'Owner') {
    window.location.href = 'login.html';
}

async function fetchMyAccommodations() {
    const listContainer = document.getElementById('accommodationsList');
    const token = localStorage.getItem('userToken'); // 🔹 هنا ناخد التوكن
    const headers = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}) // 🔹 لو فيه توكن، نضيفه
    };
    listContainer.innerHTML = '<div class="loading-spinner">جاري تحميل عقاراتك...</div>';

    try {
        console.log("🔄 [Owner] جاري طلب قائمة العقارات من السيرفر...");
        
        const response = await fetch(`${API_BASE_URL}/Accommodation?t=${new Date().getTime()}`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (response.status === 401) {
            Swal.fire('تنبيه', 'انتهت الجلسة', 'warning').then(() => logout());
            return;
        }

        const data = await response.json();
        listContainer.innerHTML = '';

        if (!data || data.length === 0) {
            listContainer.innerHTML = '<p class="text-center">لا توجد عقارات.</p>';
            return;
        }

       for (const acc of data) {
            const imageUrl = ApiService.getImageUrl(acc.mainImageUrl || acc.MainImageUrl);
            const name = acc.accommodationName || acc.AccommodationName;
          
            let locationDisplay = acc.region || acc.Region; 

            if (!locationDisplay || locationDisplay === "string") {
                locationDisplay = acc.governorateName || acc.cityName || "موقع غير محدد";
            }

            const type = acc.accommodationType || acc.AccommodationType || "غير محدد";
            const desc = acc.accommodationDescription || acc.AccommodationDescription || "";
            const id = acc.accommodationID || acc.AccommodationID;

            // السعر
            let priceDisplay = "0";
            const price = acc.pricePerNight || acc.PricePerNight;
            if (price) {
                priceDisplay = `<span style="font-weight:bold; color:#28a745;">${price}</span> <small style="color:#777">ج.م</small>`;
            } else {
                priceDisplay = `<span style="color:#999;">(السعر غير محدد)</span>`;
            }

            const isApproved = acc.isApproved || acc.IsApproved;
            const statusBadge = isApproved 
                ? '<span class="badge bg-success">معتمد</span>' 
                : '<span class="badge bg-warning text-dark">بانتظار الموافقة</span>';

            const card = `
                <div class="card h-100" id="card-${id}">
                    <div style="position:relative;">
                        <img src="${imageUrl}" class="card-img" alt="${name}" 
                             style="height: 200px; object-fit: cover; width: 100%;"
                             onerror="this.src='https://placehold.co/300x200?text=No+Image'">
                        <span style="position:absolute; top:10px; left:10px; background:rgba(0,0,0,0.7); color:white; padding:2px 8px; border-radius:4px; font-size:12px;">${type}</span>
                    </div>
                    
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <h3 class="card-title" style="font-size:1.1rem; font-weight:bold;">${name}</h3>
                            ${statusBadge}
                        </div>
                        
                        <p class="text-muted small mb-1"><i class="fas fa-map-marker-alt"></i> ${locationDisplay}</p>
                        
                        <!-- 🟢 مكان عدد الوحدات -->
                        <p class="text-muted small mb-2"><i class="fas fa-layer-group"></i> الوحدات: <strong id="units-count-${id}">...</strong></p>
                        
                        <div class="card-price mb-2">${priceDisplay}</div>
                        <p class="text-muted small text-truncate" style="max-width: 100%;">${desc}</p>
                    </div>
                    
                    <div class="card-actions p-3 border-top d-flex justify-content-between">
                        <button class="btn-action btn-edit" onclick="location.href='add-accommodation.html?id=${id}'" style="color: #f39c12;">
                            <i class="fas fa-edit"></i> تعديل
                        </button>
                        <button class="btn-action btn-delete" onclick="deleteAccommodation(${id})" style="color: #e74c3c;">
                            <i class="fas fa-trash"></i> حذف
                        </button>
                    </div>
                </div>
            `;
            listContainer.innerHTML += card;

            fetchDetailsAndUpdateCount(id, type);
        }

    } catch (error) {
        console.error(error);
        listContainer.innerHTML = '<p style="color:red">خطأ في الاتصال بالسيرفر</p>';
    }
}

async function fetchDetailsAndUpdateCount(id, type) {
   const token = localStorage.getItem('userToken'); // 🔹 هنا ناخد التوكن
    const headers = {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {}) // 🔹 لو فيه توكن، نضيفه
    };
    try {
        const response = await fetch(`${API_BASE_URL}/Accommodation/${id}`, {
            method: 'GET',
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        if (response.ok) {
            const detail = await response.json();
            let countText = "0";
            
            // حساب العدد الدقيق من التفاصيل
            if (type.toLowerCase().includes('hotel')) {
                const count = detail.hotelRooms ? detail.hotelRooms.length : 0;
                countText = `${count} غرفة`;
            } else if (type.toLowerCase().includes('student')) {
                let beds = 0;
                if (detail.studentRooms) {
                    detail.studentRooms.forEach(r => { if(r.beds) beds += r.beds.length; });
                }
                countText = `${beds} سرير`;
            } else {

                countText = "1 وحدة";
            }
            

            const el = document.getElementById(`units-count-${id}`);
            if (el) el.innerText = countText;
        }
    } catch (e) {
        console.error(`فشل جلب تفاصيل العقار ${id}`, e);
    }
}

function logout() {
    localStorage.clear();
    window.location.href = 'login.html';
}

window.deleteAccommodation = async (id) => {
    if(await Swal.fire({title: 'تأكيد الحذف؟', text: "سيتم حذف العقار وجميع الغرف المرتبطة به.", icon: 'warning', showCancelButton: true, confirmButtonText: 'نعم، احذف', cancelButtonText: 'إلغاء'}).then(r => r.isConfirmed)) {
        try {
            const response = await fetch(`${API_BASE_URL}/Accommodation/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            
            if (response.ok) {
                Swal.fire('تم', 'تم الحذف بنجاح', 'success');
                fetchMyAccommodations();
            } else {
                Swal.fire('خطأ', 'لا يمكن حذف العقار (قد يكون مرتبطاً بحجوزات)', 'error');
            }
        } catch (e) {
            Swal.fire('خطأ', 'حدث خطأ في الاتصال', 'error');
        }
    }
};

document.addEventListener('DOMContentLoaded', fetchMyAccommodations);
// js/add-accommodation.js

const PORT = "5216"; 
const API_URL = `http://localhost:${PORT}/api`;
const SERVER_URL = `http://localhost:${PORT}`; 

const token = localStorage.getItem('userToken');
const role = localStorage.getItem('userRole');

if (!token || role !== 'Owner') {
    window.location.href = 'login.html';
}

const urlParams = new URLSearchParams(window.location.search);
const editId = urlParams.get('id');

// تهيئة الصفحة
document.addEventListener('DOMContentLoaded', async () => {
    if (editId) {
        document.getElementById('pageTitle').innerHTML = '<i class="fas fa-edit"></i> تعديل بيانات العقار';
        document.getElementById('submitBtn').textContent = 'حفظ التعديلات';
        document.getElementById('imageLabel').textContent = 'إضافة صورة جديدة';
        await loadAccommodationData(editId);
    }
});

async function loadAccommodationData(id) {
    try {
        const response = await fetch(`${API_URL}/Accommodation/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) throw new Error('فشل جلب البيانات');
        
        const data = await response.json();

        // تعبئة البيانات
        document.getElementById('name').value = data.AccommodationName || data.accommodationName || data.name || '';
        document.getElementById('description').value = data.AccommodationDescription || data.accommodationDescription || data.description || '';
        document.getElementById('price').value = data.PricePerNight || data.pricePerNight || data.price || 0;
        document.getElementById('type').value = data.AccommodationType || data.accommodationType || '';
        
        const cityName = data.CityName || data.cityName;
        if (cityName) {
            const citySelect = document.getElementById('cityId');
            for(let i=0; i<citySelect.options.length; i++) {
                if(citySelect.options[i].text.includes(cityName)) {
                    citySelect.selectedIndex = i;
                    break;
                }
            }
        }
        
        const loc = data.Location || data.location || {};
        document.getElementById('region').value = data.Region || data.region || loc.Region || loc.region || '';
        document.getElementById('street').value = data.Street || data.street || loc.Street || loc.street || '';

        // استخراج ID العقار
        const accId = data.AccommodationID || data.accommodationID || data.id || id;

        // عرض الصور
        const images = data.Images || data.images;
        if (images && images.length > 0) {
            const container = document.getElementById('existingImagesContainer');
            const section = document.getElementById('existingImagesSection');
            section.classList.remove('hidden');
            container.innerHTML = '';

            images.forEach(img => {
                const imgId = img.ImageID || img.imageID || img.id;
                const imgUrlVal = img.ImageUrl || img.imageUrl || img.url;
                
                if (!imgId) return;

                let imgSrc = imgUrlVal.startsWith('http') ? imgUrlVal : `${SERVER_URL}${imgUrlVal}`;
                
                const div = document.createElement('div');
                div.className = 'img-wrapper';
                div.innerHTML = `
                    <img src="${imgSrc}" alt="image">
                    <button type="button" class="img-delete-btn" onclick="deleteImage('${accId}', '${imgId}', this)">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                container.appendChild(div);
            });
        }

    } catch (error) {
        console.error(error);
        Swal.fire('خطأ', 'تعذر تحميل بيانات العقار', 'error');
    }
}

// دالة الحذف
window.deleteImage = async (accId, imgId, btnElement) => {
    if (!accId || !imgId || accId === 'undefined' || imgId === 'undefined') {
        Swal.fire('خطأ', 'بيانات الصورة غير مكتملة', 'error');
        return;
    }

    const result = await Swal.fire({
        title: 'حذف الصورة؟',
        text: "لا يمكن التراجع عن هذا الإجراء",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'نعم، حذف',
        cancelButtonText: 'إلغاء'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`${API_URL}/Accommodation/${accId}/images/${imgId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });

            if (response.ok) {
                btnElement.parentElement.remove();
                const container = document.getElementById('existingImagesContainer');
                if (container.children.length === 0) {
                    document.getElementById('existingImagesSection').classList.add('hidden');
                }
                Swal.fire('تم', 'تم حذف الصورة بنجاح', 'success');
            } else {
                Swal.fire('خطأ', 'فشل حذف الصورة من السيرفر', 'error');
            }
        } catch (e) {
            Swal.fire('خطأ', 'خطأ في الاتصال', 'error');
        }
    }
};

document.getElementById('addForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = true;
    submitBtn.textContent = 'جاري المعالجة...';

    // جمع البيانات
    const nameVal = document.getElementById('name').value;
    const descVal = document.getElementById('description').value;
    const priceVal = parseFloat(document.getElementById('price').value);
    const typeVal = document.getElementById('type').value;
    
    const citySelect = document.getElementById('cityId');
    const cityIdVal = parseInt(citySelect.value);
    const cityNameText = citySelect.options[citySelect.selectedIndex].text;
    
    let governorateNameText = "Cairo";
    if (cityIdVal === 2) governorateNameText = "Giza";
    else if (cityIdVal === 3) governorateNameText = "Alexandria";

    const regionVal = document.getElementById('region').value;
    const streetVal = document.getElementById('street').value;

    const accommodationData = {
        AccommodationName: nameVal,
        AccommodationDescription: descVal,
        AccommodationType: typeVal,
        PricePerNight: priceVal,
        
        Location: {
            CityID: cityIdVal,
            CityName: cityNameText,
            GovernorateName: governorateNameText,
            Region: regionVal,
            Street: streetVal
        },

        StarsRate: typeVal === 'Hotel' ? 3 : 1, 
        Area: 100, Floor: 1, TotalRooms: 3, TotalGuests: 4, IsAvailable: true 
    };

    if (editId) {
        accommodationData.AccommodationID = parseInt(editId);
    }

    try {
        let response;
        let targetId = editId;

        if (editId) {
            // PUT
            response = await fetch(`${API_URL}/Accommodation/${editId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(accommodationData)
            });
        } else {
            // POST
            response = await fetch(`${API_URL}/Accommodation`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(accommodationData)
            });
        }

        if (!response.ok && response.status !== 204) {
            const errorData = await response.json();
            let msg = 'حدث خطأ في البيانات';
            if(errorData.errors) msg = Object.values(errorData.errors).flat().join('\n');
            else if (errorData.message) msg = errorData.message;
            throw new Error(msg);
        }

        if (!editId) {
            const newAcc = await response.json();
            targetId = newAcc.AccommodationID || newAcc.accommodationID || newAcc.id;
        }

        // رفع الصورة
        let imageUploadFailed = false;
        let imgErrorMsg = '';
        const imageInput = document.getElementById('imageFile');
        
        if (imageInput.files.length > 0 && targetId) {
            const formData = new FormData();
            formData.append('AccommodationID', targetId);
            formData.append('ImageFile', imageInput.files[0]);
            formData.append('AltText', nameVal || 'صورة العقار'); 

            try {
                const imgResp = await fetch(`${API_URL}/Accommodation/${targetId}/images`, {
                    method: 'POST',
                    headers: { 'Authorization': `Bearer ${token}` },
                    body: formData
                });
                
                if(!imgResp.ok) {
                     imageUploadFailed = true;
                     const textError = await imgResp.text();
                     if (textError.includes("ArgumentNullException")) {
                        imgErrorMsg = "مجلد wwwroot/images غير موجود.";
                     } else {
                        try {
                            const jsonErr = JSON.parse(textError);
                            imgErrorMsg = jsonErr.title || "خطأ غير معروف";
                        } catch { imgErrorMsg = "خطأ في السيرفر (500)"; }
                     }
                }
            } catch (err) {
                imageUploadFailed = true;
                imgErrorMsg = "فشل الاتصال";
            }
        }

        let title = 'تم بنجاح!';
        let text = editId ? 'تم تعديل البيانات.' : 'تم إضافة العقار.';
        let icon = 'success';

        if (imageUploadFailed) {
             title = 'تم الحفظ جزئياً';
             text = `تم حفظ العقار ولكن فشل رفع الصورة: ${imgErrorMsg}`;
             icon = 'warning';
        }

        await Swal.fire({
            icon: icon,
            title: title,
            text: text,
            confirmButtonText: 'حسناً'
        });

        // 🛑 التعديل هنا: التوجيه لصفحة المالك دائماً في الحالتين (إضافة أو تعديل)
        window.location.href = 'owner-dashboard.html';

    } catch (error) {
        console.error(error);
        Swal.fire('خطأ', error.message, 'error');
        submitBtn.disabled = false;
        submitBtn.textContent = editId ? 'حفظ التعديلات' : 'حفظ العقار';
    }
});
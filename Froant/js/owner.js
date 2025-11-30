// js/owner.js

const PORT = "5216"; 
const API_URL = `http://localhost:${PORT}/api`;
const SERVER_URL = `http://localhost:${PORT}`; 

const token = localStorage.getItem('userToken');
const role = localStorage.getItem('userRole');

if (!token || role !== 'Owner') {
    window.location.href = 'login.html';
}

async function fetchMyAccommodations() {
    const listContainer = document.getElementById('accommodationsList');
    listContainer.innerHTML = '<div class="loading-spinner">جاري تحميل عقاراتك...</div>';

    try {
        const response = await fetch(`${API_URL}/Accommodation`, {
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
            listContainer.innerHTML = `
                <div style="grid-column: 1/-1; text-align: center; padding: 50px;">
                    <i class="fas fa-home" style="font-size: 50px; color: #ddd; margin-bottom: 20px;"></i>
                    <p>لا توجد عقارات مضافة حتى الآن.</p>
                    <a href="add-accommodation.html" class="add-btn" style="display:inline-block; margin-top:10px;">أضف عقارك الأول</a>
                </div>
            `;
            return;
        }

        data.forEach(acc => {
            // 🛑 تصحيح: قراءة الخصائص سواء كانت PascalCase أو camelCase
            const imgPath = acc.MainImageUrl || acc.mainImageUrl;
            const name = acc.AccommodationName || acc.accommodationName;
            const city = acc.CityName || acc.cityName || 'غير محدد';
            const price = acc.PricePerNight || acc.pricePerNight;
            const id = acc.AccommodationID || acc.accommodationID;

            let imageUrl = 'https://placehold.co/300x200?text=No+Image'; 
            
            if (imgPath) {
                if (imgPath.startsWith('http')) {
                    imageUrl = imgPath;
                } else {
                    imageUrl = `${SERVER_URL}${imgPath}`;
                }
            }

            const card = `
                <div class="card">
                    <img src="${imageUrl}" class="card-img" alt="${name}" 
                         onerror="this.onerror=null;this.src='https://placehold.co/300x200?text=Error+Loading'">
                    
                    <div class="card-body">
                        <h3 class="card-title">${name}</h3>
                        <span class="card-location"><i class="fas fa-map-marker-alt"></i> ${city}</span>
                        <div class="card-price">${price} ج.م <small style="color:#777">/ ليلة</small></div>
                    </div>
                    
                    <div class="card-actions">
                        <button class="btn-action btn-edit" onclick="editAccommodation(${id})">
                            <i class="fas fa-edit"></i> تعديل
                        </button>
                        <button class="btn-action btn-delete" onclick="deleteAccommodation(${id})">
                            <i class="fas fa-trash"></i> حذف
                        </button>
                    </div>
                </div>
            `;
            listContainer.innerHTML += card;
        });

    } catch (error) {
        console.error('Error fetching data:', error);
        listContainer.innerHTML = '<p style="color:red; text-align:center;">حدث خطأ أثناء الاتصال بالسيرفر.</p>';
    }
}

function logout() {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userRole');
    localStorage.removeItem('isLoggedIn');
    window.location.href = 'login.html';
}

async function deleteAccommodation(id) {
    const result = await Swal.fire({
        title: 'هل أنت متأكد؟',
        text: "لن تتمكن من استرجاع هذا العقار!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذفه!',
        cancelButtonText: 'إلغاء'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`${API_URL}/Accommodation/${id}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                await Swal.fire('تم الحذف!', 'تم حذف العقار بنجاح.', 'success');
                fetchMyAccommodations();
            } else {
                Swal.fire('خطأ!', 'فشل حذف العقار.', 'error');
            }
        } catch (error) {
            Swal.fire('خطأ!', 'حدث خطأ في الاتصال بالسيرفر.', 'error');
        }
    }
}

function editAccommodation(id) {
    window.location.href = `add-accommodation.html?id=${id}`;
}

document.addEventListener('DOMContentLoaded', fetchMyAccommodations);
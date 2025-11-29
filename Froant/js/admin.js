
window.IS_REAL_API = true; 
const API_BASE_URL = 'https://localhost:7066/api/admin'; 

const contentPlaceholder = document.getElementById('content-placeholder');
const sidebarLinksDesktop = document.querySelectorAll('#admin-sidebar .nav-link');
const sidebarLinksMobile = document.querySelectorAll('#mobile-nav-links .nav-link');
const adminLogoutBtnDesktop = document.getElementById('adminLogoutBtn');
const adminLogoutBtnMobile = document.getElementById('adminLogoutBtnMobile');

// **********************************************
// وظائف المساعدة المشتركة
// **********************************************

async function fetchAdminData(route, method = 'GET', body = null) {
    if (!window.IS_REAL_API && typeof mockFetch === 'function') { 
        return mockFetch(route, method, body);
    }

    const url = `${API_BASE_URL}${route}`;

    const headers = {
        'Content-Type': 'application/json',
    };

    const config = {
        method: method,
        headers: headers,
    };

    if (body && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
        config.body = JSON.stringify(body);
    }
    
    try {
        const response = await fetch(url, config);
        
        if (response.status === 204) {
            return { success: true }; 
        }

        // ✅ التعديل هنا: فحص نوع المحتوى قبل محاولة قراءته كـ JSON
        const contentType = response.headers.get("content-type");
        let data;

        if (contentType && contentType.indexOf("application/json") !== -1) {
            // إذا كان الرد JSON، اقرأه كـ JSON
            data = await response.json();
        } else {
            // إذا كان نصاً عادياً (مثل رسائل النجاح من السيرفر)، اقرأه كنص
            const text = await response.text();
            // غلّف النص في كائن ليتوافق مع باقي الكود
            data = { message: text, success: response.ok };
        }

        if (!response.ok) {
            // التعامل مع رسائل الخطأ من الـ API بشكل أفضل
            const errorMessage = data.message || data.title || `Error: ${response.status}`;
            Swal.fire('خطأ', errorMessage, 'error');
            return null;
        }

        return data;

    } catch (error) {
        console.error("Fetch Error:", error);
        Swal.fire('خطأ في الاتصال', 'فشل الاتصال بالخادم.', 'error');
        return null;
    }
}

// **********************************************
// Dashboard
// **********************************************
async function loadDashboard() {
    contentPlaceholder.innerHTML = '<div class="text-center p-5"><i class="fas fa-spinner fa-spin fa-2x"></i> جارٍ تحميل لوحة التحكم...</div>';
    const stats = await fetchAdminData('/dashboard'); 
    
    if (stats) {
        // نستخدم أسماء الحقول كما تأتي من الـ DTO (camelCase)
        const html = `
            <h2 class="section-title mb-5">لوحة التحكم الرئيسية</h2>
            <div class="row g-4" data-aos="fade-up">
                <div class="col-lg-3 col-md-6"><div class="dashboard-card bg-primary-light text-primary"><i class="fas fa-users"></i><h5>إجمالي المستخدمين</h5><p class="h1">${stats.totalUsers || 0}</p></div></div>
                <div class="col-lg-3 col-md-6"><div class="dashboard-card bg-success-light text-success"><i class="fas fa-user-tie"></i><h5>إجمالي الملاك</h5><p class="h1">${stats.totalOwners || 0}</p></div></div>
                <div class="col-lg-3 col-md-6"><div class="dashboard-card bg-info-light text-info"><i class="fas fa-user-circle"></i><h5>إجمالي العملاء</h5><p class="h1">${stats.totalClients || 0}</p></div></div>
                <div class="col-lg-3 col-md-6"><div class="dashboard-card bg-danger-light text-danger"><i class="fas fa-user-secret"></i><h5>إجمالي المشرفين</h5><p class="h1">${stats.totalAdmins || 0}</p></div></div>
                
                <div class="col-lg-4 col-md-6">
                    <div class="dashboard-card bg-warning-light text-warning">
                        <i class="fas fa-building"></i>
                        <h5>إجمالي العقارات</h5>
                        <p class="h1">${stats.totalAccommodations || 0}</p>
                        <small class="text-muted">المعتمد: ${stats.approvedAccommodations || 0} | بانتظار: ${stats.pendingAccommodations || 0}</small>
                    </div>
                </div>

                <div class="col-lg-4 col-md-6">
                    <div class="dashboard-card bg-secondary-light text-secondary">
                        <i class="fas fa-calendar-alt"></i>
                        <h5>إجمالي الحجوزات</h5>
                        <p class="h1">${stats.totalBookings || 0}</p>
                        <small class="text-muted">قيد الانتظار: ${stats.pendingBookings || 0}</small>
                    </div>
                </div>
            </div>
        `;
        contentPlaceholder.innerHTML = html;
        AOS.init();
    }
}

// **********************************************
// Users
// **********************************************
async function loadUsers() {
    contentPlaceholder.innerHTML = '<div class="text-center p-5"><i class="fas fa-spinner fa-spin fa-2x"></i> جارٍ تحميل المستخدمين...</div>';
    const users = await fetchAdminData('/users');
    
    if (users) {
        const html = generateUsersHtml(users);
        contentPlaceholder.innerHTML = html;
        addUsersEventListeners();
    }
}

function generateUsersHtml(users) {
    const roleMap = { 1: 'Admin', 2: 'Owner', 3: 'Client' };
    
    // تأكد من أن أسماء الحقول هنا تطابق AdminUserListDto بصيغة camelCase
    let tableRows = users.map(user => `
        <tr>
            <td>${user.firstName} ${user.lastName}</td>
            <td>${user.email}</td>
            <td>${roleMap[user.type] || 'User'}</td>
            <td>
                <span class="badge ${user.isActive ? 'bg-success' : 'bg-danger'}">
                    ${user.isActive ? 'مفعل' : 'معطل'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-info change-role-btn" data-id="${user.id}" data-role="${user.type}">تغيير الدور</button>
                <button class="btn btn-sm ${user.isActive ? 'btn-warning' : 'btn-success'} toggle-status-btn" data-id="${user.id}" data-active="${user.isActive}">
                    ${user.isActive ? 'تعطيل' : 'تفعيل'}
                </button>
                <button class="btn btn-sm btn-danger delete-user-btn" data-id="${user.id}">حذف</button>
            </td>
        </tr>
    `).join('');

    return `
        <h2 class="section-title mb-4">إدارة المستخدمين</h2>
        <div class="admin-table-container">
            <table class="table table-hover table-responsive">
                <thead><tr><th>الاسم</th><th>البريد</th><th>الدور</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
                <tbody>${tableRows}</tbody>
            </table>
        </div>
    `;
}

function addUsersEventListeners() {
    document.querySelectorAll('.change-role-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const userId = e.target.dataset.id;
            const currentRole = parseInt(e.target.dataset.role);
            
            const { value: newRole } = await Swal.fire({
                title: 'تغيير دور المستخدم',
                input: 'select',
                inputOptions: { 1: 'Admin', 2: 'Owner', 3: 'Client' },
                inputValue: currentRole,
                showCancelButton: true
            });

            if (newRole) {
                const result = await fetchAdminData('/users/changerole', 'PUT', { userId: userId, newRole: parseInt(newRole) });
                if (result) loadUsers(); 
            }
        });
    });

    document.querySelectorAll('.toggle-status-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const userId = e.target.dataset.id;
            const isActive = e.target.dataset.active === 'true';
            
            const { isConfirmed } = await Swal.fire({
                title: isActive ? 'تعطيل المستخدم؟' : 'تفعيل المستخدم؟',
                icon: 'warning',
                showCancelButton: true
            });

            if (isConfirmed) {
                const result = await fetchAdminData('/users/toggle-status', 'PUT', { userId: userId, isActive: !isActive });
                if (result) loadUsers(); 
            }
        });
    });
    
    document.querySelectorAll('.delete-user-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const userId = e.target.dataset.id;
            const { isConfirmed } = await Swal.fire({ title: 'حذف نهائي؟', icon: 'error', showCancelButton: true });
            if (isConfirmed) {
                const result = await fetchAdminData(`/users/${userId}`, 'DELETE');
                if (result) loadUsers(); 
            }
        });
    });
}

// **********************************************
// 🟩 Admin Accommodations (تم التحديث هنا ✅)
// **********************************************

async function loadAccommodations() {
    contentPlaceholder.innerHTML = '<div class="text-center p-5"><i class="fas fa-spinner fa-spin fa-2x"></i> جارٍ تحميل العقارات...</div>';
    const accommodations = await fetchAdminData('/accommodations');

    if (accommodations) {
        const html = generateAccommodationsHtml(accommodations);
        contentPlaceholder.innerHTML = html;
        addAccommodationEventListeners();
    }
}

function generateAccommodationsHtml(accommodations) {
    // ✅ تحديث: استخدام الأسماء الصحيحة من AdminAccommodationListDto
    // accommodationID, accommodationName, accommodationType, ownerName, isApproved
    
    let tableRows = accommodations.map(acc => `
        <tr>
            <td>${acc.accommodationID}</td> <!-- كان acc.id -->
            <td>${acc.accommodationName}</td> <!-- كان acc.name -->
            <td>${acc.accommodationType}</td> <!-- كان acc.type -->
            <td>${acc.ownerName}</td> <!-- كان acc.owner -->
            <td>
                <span class="badge ${acc.isApproved ? 'bg-success' : 'bg-warning'}">
                    ${acc.isApproved ? 'موافق عليه' : 'بانتظار الموافقة'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-info details-acc-btn" data-id="${acc.accommodationID}">التفاصيل</button>
                <button class="btn btn-sm ${acc.isApproved ? 'btn-warning' : 'btn-success'} toggle-status-acc-btn" data-id="${acc.accommodationID}" data-approved="${acc.isApproved}">
                    ${acc.isApproved ? 'رفض' : 'موافقة'}
                </button>
                <button class="btn btn-sm btn-danger delete-acc-btn" data-id="${acc.accommodationID}">حذف</button>
            </td>
        </tr>
    `).join('');

    return `
        <h2 class="section-title mb-4">إدارة العقارات</h2>
        <div class="admin-table-container">
            <table class="table table-hover table-responsive">
                <thead><tr><th>ID</th><th>الاسم</th><th>النوع</th><th>المالك</th><th>الحالة</th><th>الإجراءات</th></tr></thead>
                <tbody>${tableRows}</tbody>
            </table>
        </div>
    `;
}

function addAccommodationEventListeners() {
    document.querySelectorAll('.toggle-status-acc-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const id = parseInt(e.target.dataset.id);
            const isApproved = e.target.dataset.approved === 'true';
            
            const { isConfirmed } = await Swal.fire({
                title: !isApproved ? 'الموافقة على العقار؟' : 'رفض العقار؟',
                icon: 'warning',
                showCancelButton: true
            });

            // تأكد من أن الـ DTO في الباك إند يستقبل IsApproved (UpdateAccommodationStatusDto)
            if (isConfirmed) {
                const result = await fetchAdminData(`/accommodations/${id}/status`, 'PUT', { isApproved: !isApproved });
                if (result) loadAccommodations(); 
            }
        });
    });
    
    document.querySelectorAll('.delete-acc-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const id = parseInt(e.target.dataset.id);
            const { isConfirmed } = await Swal.fire({ title: 'حذف نهائي؟', icon: 'error', showCancelButton: true });
            
            if (isConfirmed) {
                const result = await fetchAdminData(`/accommodations/${id}`, 'DELETE');
                if (result) loadAccommodations(); 
            }
        });
    });
    
    document.querySelectorAll('.details-acc-btn').forEach(button => {
        button.addEventListener('click', async (e) => {
            const id = parseInt(e.target.dataset.id);
            const details = await fetchAdminData(`/accommodations/${id}`);
            
            if (details) {
                // ✅ تحديث: استخدام الأسماء الصحيحة من AdminAccommodationDetailsDto
                Swal.fire({
                    title: `تفاصيل: ${details.accommodationName}`,
                    html: `<p><strong>المالك:</strong> ${details.ownerName}</p>
                           <p><strong>المدينة:</strong> ${details.city}</p>
                           <p><strong>النوع:</strong> ${details.accommodationType}</p>
                           <p><strong>الحالة:</strong> ${details.isApproved ? 'موافق عليه' : 'بانتظار الموافقة'}</p>
                           <p><strong>الوصف:</strong> ${details.description || 'لا يوجد'}</p>`,
                    icon: 'info'
                });
            }
        });
    });
}

// **********************************************
// Bookings
// **********************************************
async function loadBookings() {
    contentPlaceholder.innerHTML = '<div class="text-center p-5"><i class="fas fa-spinner fa-spin fa-2x"></i> جارٍ تحميل الحجوزات...</div>';
    const bookings = await fetchAdminData('/bookings');
    if (bookings) {
        const html = generateBookingsHtml(bookings);
        contentPlaceholder.innerHTML = html;
        addBookingEventListeners();
    }
}

function generateBookingsHtml(bookings) {
    const statusMap = { 'Pending': 'warning', 'Confirmed': 'success', 'Cancelled': 'danger', 'Completed': 'primary' };
    
    let tableRows = bookings.map(booking => `
        <tr>
            <td>${booking.bookingID}</td>
            <td>${booking.accommodationName || 'غير محدد'}</td>
            <td>${new Date(booking.checkIN).toLocaleDateString()}</td>
            <td>${new Date(booking.checkOUT).toLocaleDateString()}</td>
            <td>${booking.totalPrice}</td>
            <td><span class="badge bg-${statusMap[booking.status] || 'secondary'}">${booking.status}</span></td>
            <td>
                <button class="btn btn-sm btn-info change-status-book-btn" data-id="${booking.bookingID}" data-status="${booking.status}">حالة</button>
                <button class="btn btn-sm btn-warning cancel-book-btn" data-id="${booking.bookingID}">إلغاء</button>
                <button class="btn btn-sm btn-danger delete-book-btn" data-id="${booking.bookingID}">حذف</button>
            </td>
        </tr>
    `).join('');

    return `
        <h2 class="section-title mb-4">إدارة الحجوزات</h2>
        <div class="admin-table-container">
            <table class="table table-hover table-responsive">
                <thead><tr><th>ID</th><th>العقار</th><th>دخول</th><th>خروج</th><th>السعر</th><th>الحالة</th><th>إجراءات</th></tr></thead>
                <tbody>${tableRows}</tbody>
            </table>
        </div>
    `;
}

function addBookingEventListeners() {
    // ... (نفس المنطق السابق، تأكد من استخدام booking.bookingID في الزر)
    document.querySelectorAll('.change-status-book-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const id = e.target.dataset.id;
            const current = e.target.dataset.status;
            const { value } = await Swal.fire({ 
                title: 'تغيير الحالة', input: 'select', 
                inputOptions: { Pending: 'Pending', Confirmed: 'Confirmed', Cancelled: 'Cancelled', Completed: 'Completed' },
                inputValue: current, showCancelButton: true 
            });
            if (value) {
                const res = await fetchAdminData(`/bookings/${id}/status`, 'PUT', { status: value });
                if (res) loadBookings();
            }
        });
    });
    
    document.querySelectorAll('.cancel-book-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const id = e.target.dataset.id;
            const { isConfirmed } = await Swal.fire({ title: 'إلغاء الحجز؟', icon: 'warning', showCancelButton: true });
            if (isConfirmed) {
                const res = await fetchAdminData(`/bookings/${id}/cancel`, 'PUT');
                if (res) loadBookings();
            }
        });
    });

    document.querySelectorAll('.delete-book-btn').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const id = e.target.dataset.id;
            const { isConfirmed } = await Swal.fire({ title: 'حذف نهائي؟', icon: 'error', showCancelButton: true });
            if (isConfirmed) {
                const res = await fetchAdminData(`/bookings/${id}`, 'DELETE');
                if (res) loadBookings();
            }
        });
    });
}

function initAdmin() {
    if (adminLogoutBtnDesktop) adminLogoutBtnDesktop.addEventListener('click', (e) => e.preventDefault()); 
    if (adminLogoutBtnMobile) adminLogoutBtnMobile.addEventListener('click', (e) => e.preventDefault());

    const allSidebarLinks = [...sidebarLinksDesktop, ...sidebarLinksMobile];
    allSidebarLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const section = e.target.closest('.nav-link').dataset.section;
            allSidebarLinks.forEach(l => l.classList.remove('active'));
            e.target.closest('.nav-link').classList.add('active');

            switch (section) {
                case 'dashboard': loadDashboard(); break;
                case 'users': loadUsers(); break;
                case 'accommodations': loadAccommodations(); break;
                case 'bookings': loadBookings(); break;
            }
        });
    });

    const defaultLink = document.querySelector('.nav-link[data-section="dashboard"]');
    if (defaultLink) {
        defaultLink.classList.add('active');
        loadDashboard();
    }
}

document.addEventListener('DOMContentLoaded', initAdmin);
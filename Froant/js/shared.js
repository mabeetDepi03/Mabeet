

// js/shared.js

function renderComponents() {
    const navbarPlaceholder = document.getElementById('navbar-placeholder');
    if (navbarPlaceholder && typeof MabeetComponents !== 'undefined') {
        navbarPlaceholder.innerHTML = MabeetComponents.createNavbar();
    }
    
    const footerPlaceholder = document.getElementById('footer-placeholder');
    if (footerPlaceholder && typeof MabeetComponents !== 'undefined') {
        footerPlaceholder.innerHTML = MabeetComponents.createFooter();
    }
}

function updateAuthUI() {

    const isLoggedIn = typeof MabeetAuth !== 'undefined' 
        ? MabeetAuth.isLoggedIn() 
        : (localStorage.getItem('isLoggedIn') === 'true');

    const userData = typeof MabeetAuth !== 'undefined' 
        ? MabeetAuth.getCurrentUser() 
        : JSON.parse(localStorage.getItem('userData') || '{}');

    const authButtons = document.getElementById('authButtons');
    const userMenu = document.getElementById('userMenu');
    const userAvatar = document.getElementById('userAvatar');

    if (authButtons && userMenu) {
        if (isLoggedIn) {
            // حالة: مسجل دخول
            authButtons.classList.add('d-none');
            userMenu.classList.remove('d-none');
            userMenu.classList.add('d-flex');
            
            // تحديث الصورة والاسم
            if (userAvatar && userData) {
                const displayName = userData.firstName 
                    ? `${userData.firstName} ${userData.lastName || ''}` 
                    : (userData.email || 'مستخدم');
                
                // استخدام خدمة ui-avatars لتوليد صورة بالأحرف الأولى
                userAvatar.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=1B3C53&color=fff&size=64`;
            }
        } else {
            // حالة: غير مسجل
            authButtons.classList.remove('d-none');
            userMenu.classList.add('d-none');
            userMenu.classList.remove('d-flex');
        }
    }
}

// 3. دالة معالجة تسجيل الخروج
function handleLogout() {
    Swal.fire({
        title: 'تأكيد تسجيل الخروج',
        text: 'هل أنت متأكد؟',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'نعم، خروج',
        cancelButtonText: 'إلغاء',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
    }).then((result) => {
        if (result.isConfirmed) {
            if (typeof MabeetAuth !== 'undefined') {
                MabeetAuth.logout();
            } else {
                // احتياطي لو الكلاس مش موجود
                localStorage.clear();
                window.location.href = 'index.html';
            }
        }
    });
}

// 4. دالة البحث الموحدة (تعمل في صفحات الفنادق، الشقق، والسكن)
function applyUnifiedFilter() {
    const citySelect = document.getElementById('cityFilter');
    if (!citySelect) return;

    const cityId = citySelect.value;
    const filter = cityId ? { CityID: cityId } : {};

    // استدعاء دالة التحميل المناسبة حسب الصفحة الحالية
    if (typeof loadHotels === 'function') {
        loadHotels(filter);
    } else if (typeof loadApartments === 'function') {
        loadApartments(filter);
    } else if (typeof loadStudentHousing === 'function') {
        loadStudentHousing(filter);
    } else {
        console.warn("لم يتم العثور على دالة تحميل مناسبة في هذه الصفحة.");
    }
}

document.addEventListener('DOMContentLoaded', () => {
    // 1. رسم الناف بار والفوتر
    renderComponents();

    // 2. تحديث الحالة (دخول/خروج)
    updateAuthUI();
    
    // 3. إدارة الأحداث (Event Delegation) للأزرار التي تم إنشاؤها ديناميكياً
    document.body.addEventListener('click', function(e) {
  
        if (e.target.closest('#logoutBtn')) {
            e.preventDefault();
            handleLogout();
        }
        
   
    });
});

// تأثير تغيير لون الناف بار عند التمرير
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('mainNav');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('shadow-sm'); // إضافة ظل خفيف
            navbar.style.background = 'rgba(255, 255, 255, 0.98)';
        } else {
            navbar.classList.remove('shadow-sm');
            navbar.style.background = 'rgba(255, 255, 255, 0.95)';
        }
    }
});
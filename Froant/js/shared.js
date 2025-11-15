// shared.js
// يحتوي على الأكواد المشتركة لجميع الصفحات
function renderComponents() {
    const navbarPlaceholder = document.getElementById('navbar-placeholder');
    if (navbarPlaceholder) {
        navbarPlaceholder.innerHTML = MabeetComponents.createNavbar();
    }
    const footerPlaceholder = document.getElementById('footer-placeholder');
    if (footerPlaceholder) {
        footerPlaceholder.innerHTML = MabeetComponents.createFooter();
    }
}

// دالة لتحديث واجهة المستخدم بعد تسجيل الدخول/الخروج
function updateAuthUI() {
    const isLoggedIn = localStorage.getItem('userLoggedIn') === 'true';
    const authButtons = document.getElementById('authButtons');
    const userMenu = document.getElementById('userMenu');
    const userEmail = localStorage.getItem('userEmail');
    const userAvatar = document.getElementById('userAvatar');

    if (authButtons && userMenu) {
        if (isLoggedIn) {
            authButtons.classList.add('d-none');
            userMenu.classList.remove('d-none');
            userMenu.classList.add('d-flex');
            if (userAvatar && userEmail) {
                userAvatar.src = `https://ui-avatars.com/api/?name=${userEmail}&background=4a6cf7&color=fff`;
            }
        } else {
            authButtons.classList.remove('d-none');
            userMenu.classList.add('d-none');
            userMenu.classList.remove('d-flex');
        }
    }
}

// دالة لإظهار المودال (نافذة منبثقة) الخاصة بتسجيل الدخول
function showLoginModal() {
    Swal.fire({
        title: 'تسجيل الدخول',
        html: `
            <input type="email" id="swal-email" class="swal2-input" placeholder="البريد الإلكتروني">
            <input type="password" id="swal-password" class="swal2-input" placeholder="كلمة المرور">
        `,
        focusConfirm: false,
        preConfirm: () => {
            const email = Swal.getPopup().querySelector('#swal-email').value;
            const password = Swal.getPopup().querySelector('#swal-password').value;
            if (!email || !password) {
                Swal.showValidationMessage('الرجاء إدخال البريد الإلكتروني وكلمة المرور');
                return false;
            }
            if (MabeetAuth.login(email, password)) {
                updateAuthUI();
                Swal.fire({
                    icon: 'success',
                    title: 'تم تسجيل الدخول بنجاح!',
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                Swal.showValidationMessage('البريد الإلكتروني أو كلمة المرور غير صحيحة');
                return false;
            }
        }
    });
}

// دالة لمعالجة تسجيل الخروج
function handleLogout() {
    Swal.fire({
        title: 'تأكيد تسجيل الخروج',
        text: 'هل أنت متأكد من رغبتك في تسجيل الخروج؟',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'نعم، تسجيل خروج',
        cancelButtonText: 'إلغاء',
    }).then((result) => {
        if (result.isConfirmed) {
            MabeetAuth.logout();
            updateAuthUI();
            Swal.fire({
                icon: 'success',
                title: 'تم تسجيل الخروج',
                text: 'تم تسجيل خروجك بنجاح',
                timer: 2000,
                showConfirmButton: false
            });
        }
    });
}

// استدعاء الدوال عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', () => {
    // خطوة 1: عرض المكونات (النافبار والفوتر)
    renderComponents();

    // خطوة 2: تحديث واجهة المستخدم بناءً على حالة تسجيل الدخول
    updateAuthUI();
    
    // خطوة 3: إضافة المستمعين للأزرار بشكل ديناميكي بعد إنشاء النافبار
    document.addEventListener('click', function(e) {
        if (e.target && e.target.id === 'loginBtn') {
            e.preventDefault();
            showLoginModal();
        } else if (e.target && e.target.id === 'registerBtn') {
            e.preventDefault();
            window.location.href = 'regester.html';
        } else if (e.target && e.target.id === 'logoutBtn') {
            e.preventDefault();
            handleLogout();
        }
    });
});

// تأثير الناف بار عند التمرير
window.addEventListener('scroll', () => {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
});
// shared.js
// يحتوي على الأكواد المشتركة لجميع الصفحات
function renderComponents() {
    const navbarPlaceholder = document.getElementById('navbar-placeholder');
    if (navbarPlaceholder) {
        navbarPlaceholder.innerHTML = MabeetComponents.createNavbar();
    }
    const footerPlaceholder = document.getElementById('footer-placeholder');
    if (footerPlaceholder) {
        footerPlaceholder.innerHTML = MabeetComponents.createFooter();
    }
}

// دالة لتحديث واجهة المستخدم بعد تسجيل الدخول/الخروج
function updateAuthUI() {
    const isLoggedIn = localStorage.getItem('userLoggedIn') === 'true';
    const authButtons = document.getElementById('authButtons');
    const userMenu = document.getElementById('userMenu');
    const userEmail = localStorage.getItem('userEmail');
    const userAvatar = document.getElementById('userAvatar');

    if (authButtons && userMenu) {
        if (isLoggedIn) {
            authButtons.classList.add('d-none');
            userMenu.classList.remove('d-none');
            userMenu.classList.add('d-flex');
            if (userAvatar && userEmail) {
                userAvatar.src = `https://ui-avatars.com/api/?name=${userEmail}&background=4a6cf7&color=fff`;
            }
        } else {
            authButtons.classList.remove('d-none');
            userMenu.classList.add('d-none');
            userMenu.classList.remove('d-flex');
        }
    }
}

// دالة لعرض نافذة تسجيل الدخول المنبثقة
function showLoginModal() {
    // ... (code for login modal remains the same) ...
}

// دالة لمعالجة تسجيل الخروج
function handleLogout() {
    Swal.fire({
        title: 'تأكيد تسجيل الخروج',
        text: 'هل أنت متأكد من رغبتك في تسجيل الخروج؟',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'نعم، تسجيل خروج',
        cancelButtonText: 'إلغاء',
        customClass: {
            confirmButton: 'btn btn-auth me-2',
            cancelButton: 'btn btn-outline-auth'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            MabeetAuth.logout();
            updateAuthUI();
            Swal.fire({
                icon: 'success',
                title: 'تم تسجيل الخروج',
                text: 'تم تسجيل خروجك بنجاح',
                timer: 2000,
                showConfirmButton: false
            });
        }
    });
}

// دالة جديدة لتحديث اسم المستخدم في صفحة الملف الشخصي
function updateProfileName() {
    const userData = MabeetAuth.getCurrentUser();
    if (userData) {
        const fullName = `${userData.firstName} ${userData.lastName}`;
        const profileNameElement = document.getElementById('profile-user-name');
        const profileFullNameElement = document.getElementById('profile-full-name');
        const profileEmailElement = document.getElementById('profile-email');
        const profilePhoneElement = document.getElementById('profile-phone');
        const profileUserTypeElement = document.getElementById('profile-user-type');
        const profilePictureElement = document.querySelector('.profile-picture');

        if (profileNameElement) profileNameElement.textContent = fullName;
        if (profileFullNameElement) profileFullNameElement.textContent = fullName;
        if (profileEmailElement) profileEmailElement.textContent = userData.email;
        if (profilePhoneElement) profilePhoneElement.textContent = userData.phone || 'غير متاح';
        if (profileUserTypeElement) profileUserTypeElement.textContent = userData.userType || 'غير محدد';
        if (profilePictureElement) profilePictureElement.src = `https://ui-avatars.com/api/?name=${userData.firstName}+${userData.lastName}&background=4a6cf7&color=fff`;
    }
}

// استدعاء الدوال عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', () => {
    // خطوة 1: عرض المكونات (النافبار والفوتر)
    renderComponents();

    // خطوة 2: تحديث واجهة المستخدم بناءً على حالة تسجيل الدخول
    updateAuthUI();
    
    // خطوة 3: إضافة المستمعين للأزرار بشكل ديناميكي بعد إنشاء النافبار
    document.addEventListener('click', function(e) {
        if (e.target && e.target.id === 'loginBtn') {
            e.preventDefault();
            showLoginModal();
        } else if (e.target && e.target.id === 'registerBtn') {
            e.preventDefault();
            window.location.href = 'regester.html';
        } else if (e.target && e.target.id === 'logoutBtn') {
            e.preventDefault();
            handleLogout();
        }
    });

    // خطوة 4: تحديث اسم المستخدم على صفحة الملف الشخصي
    if (window.location.pathname.includes('profile.html')) {
        updateProfileName();
    }
});

// تأثير الناف بار عند التمرير
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('mainNav');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
});
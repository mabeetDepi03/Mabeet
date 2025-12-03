// js/components.js

class MabeetComponents {
    static createNavbar() {

        const isLoggedIn = localStorage.getItem('isLoggedIn') === 'true'; 
  
        let userData = {};
        try {
            userData = JSON.parse(localStorage.getItem('userData') || '{}');
        } catch (e) {
            console.error("Error parsing userData", e);
        }

        // 1. تحديد الاسم
        const firstName = userData.firstName || 'مستخدم';
        
        // 2. تحديد نوع المستخدم (Role)
        const role = userData.role || 'Client'; // الافتراضي عميل

        // 3. 🟢 اللوجيك السحري: تحديد الرابط والاسم حسب النوع
        let dashboardLink = "profile.html"; // الافتراضي
        let dashboardText = "الملف الشخصي";
        let dashboardIcon = "fa-user";

        if (role === 'Admin') {
            dashboardLink = "Admin.html";
            dashboardText = "لوحة التحكم (Admin)";
            dashboardIcon = "fa-cogs";
        } else if (role === 'Owner') {
            dashboardLink = "owner-dashboard.html";
            dashboardText = "إدارة عقاراتي";
            dashboardIcon = "fa-building";
        }

        // رابط الصورة الرمزية
        const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(firstName)}&background=1B3C53&color=fff&size=40`;

        return `
            <nav class="navbar navbar-expand-lg fixed-top" id="mainNav">
                <div class="container">
                    <a class="navbar-brand" href="index.html">
                        <i class="fa-solid fa-bed me-2"></i>
                        <span>Mabeet</span>
                    </a>
                    
                    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                    
                    <div class="collapse navbar-collapse" id="navbarNav">
                        <ul class="navbar-nav mx-auto">
                            <li class="nav-item"><a class="nav-link" href="index.html">الرئيسية</a></li>
                            <li class="nav-item"><a class="nav-link" href="hotels.html">الفنادق</a></li>
                            <li class="nav-item"><a class="nav-link" href="apartments.html">الشقق</a></li>
                            <li class="nav-item"><a class="nav-link" href="student-housing.html">السكن الطلابي</a></li>
                        </ul>
                        
                        <div class="d-flex align-items-center ${isLoggedIn ? 'd-none' : ''}" id="authButtons">
                            <a href="login.html" class="btn btn-auth me-2" id="loginBtn">دخول</a>
                            <a href="regester.html" class="btn btn-outline-auth" id="registerBtn">تسجيل</a>
                        </div>
                        
                        <div class="d-flex align-items-center ${isLoggedIn ? '' : 'd-none'}" id="userMenu">
                            
                            ${role === 'Client' ? `
                            <a href="profile.html#bookings" class="text-secondary me-3 position-relative" title="حجوزاتي">
                                <i class="fas fa-calendar-check fa-lg"></i>
                            </a>` : ''}
                            
                            <div class="dropdown user-dropdown">
                                <a href="#" class="d-flex align-items-center text-decoration-none dropdown-toggle" id="userDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                                    <img src="${avatarUrl}" class="user-avatar rounded-circle border border-2 border-white shadow-sm" alt="Avatar" width="40" height="40">
                                </a>
                                <ul class="dropdown-menu dropdown-menu-end shadow-lg border-0 mt-2" aria-labelledby="userDropdown">
                                    <li class="px-3 py-2 text-center border-bottom bg-light">
                                        <span class="fw-bold d-block text-primary">${firstName}</span>
                                        <span class="badge bg-secondary" style="font-size: 0.7rem;">${role}</span>
                                    </li>
                                    
                                    <li>
                                        <a class="dropdown-item py-2" href="${dashboardLink}">
                                            <i class="fas ${dashboardIcon} me-2 text-secondary"></i> ${dashboardText}
                                        </a>
                                    </li>

                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item py-2 text-danger" href="#" id="logoutBtn"><i class="fas fa-sign-out-alt me-2"></i> خروج</a></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </nav>
        `;
    }

  
    static createFooter() {
        return `
            <footer id="footer">
                <div class="container text-center text-md-start">
                    <div class="row">
                        <div class="col-md-3 mx-auto mt-3">
                            <h5 class="footer-title">Mabeet</h5>
                            <p>مكانك الأول لحجز الفنادق، الشقق، والسكن الطلابي بأفضل الأسعار.</p>
                        </div>
                        <div class="col-md-2 mx-auto mt-3">
                            <h5 class="footer-title">الخدمات</h5>
                            <p><a href="hotels.html">الفنادق</a></p>
                            <p><a href="apartments.html">الشقق</a></p>
                            <p><a href="student-housing.html">السكن الطلابي</a></p>
                        </div>
                        <div class="col-md-3 mx-auto mt-3">
                            <h5 class="footer-title">روابط</h5>
                            <p><a href="profile.html">حسابي</a></p>
                            <p><a href="#">المساعدة</a></p>
                        </div>
                        <div class="col-md-4 mx-auto mt-3">
                            <h5 class="footer-title">تواصل معنا</h5>
                            <p><i class="fas fa-envelope me-2"></i> info@mabeet.com</p>
                        </div>
                    </div>
                    <hr class="my-4">
                    <div class="text-center">
                        <p>© 2025 جميع الحقوق محفوظة لـ <strong>Mabeet</strong></p>
                    </div>
                </div>
            </footer>
        `;
    }


    static init() {

        const navContainer = document.getElementById('navbar-placeholder'); 
        if (navContainer) {
            navContainer.innerHTML = MabeetComponents.createNavbar();

            const logoutButton = document.getElementById('logoutBtn');
            if (logoutButton) { 
                logoutButton.addEventListener('click', function(e) {
                    e.preventDefault();
                    if (typeof MabeetAuth !== 'undefined') MabeetAuth.logout();
                    else { localStorage.clear(); window.location.href = 'index.html'; }
                });
            }
        }

        const footerContainer = document.getElementById('footer-placeholder');
        if (footerContainer) footerContainer.innerHTML = MabeetComponents.createFooter();
        else {
            const oldFooter = document.getElementById('footer');
            if (oldFooter) oldFooter.innerHTML = MabeetComponents.createFooter();
        }
    }
}

document.addEventListener('DOMContentLoaded', MabeetComponents.init);
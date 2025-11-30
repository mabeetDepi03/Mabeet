// auth.js

// 🛑 تأكدي أن هذا الرقم يطابق البورت المفتوح في المتصفح عند تشغيل الـ Swagger
const PORT = "5216"; 
const API_BASE_URL = `http://localhost:${PORT}/api`; 

class MabeetAuth {

    // ================== Login ==================
    static async login(email, password) {
        try {
            const response = await fetch(`${API_BASE_URL}/Users/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email: email, password: password })
            });

            const data = await response.json();

            if (response.ok) {
                // تخزين التوكن
                localStorage.setItem('userToken', data.token);
                // تخزين الصلاحية القادمة من السيرفر
                localStorage.setItem('userRole', data.userRole || 'Client'); 
                localStorage.setItem('isLoggedIn', 'true');
                
                return { 
                    success: true, 
                    message: "تم تسجيل الدخول بنجاح!",
                    userRole: data.userRole // نرجع الدور لصفحة الـ login عشان التوجيه
                };
            } else {
                return { success: false, message: data.message || 'بيانات الدخول غير صحيحة' };
            }
        } catch (error) {
            console.error('Login Error:', error);
            return { success: false, message: "فشل الاتصال بالخادم. تأكد من تشغيل الـ API" };
        }
    }

    // ================== Register ==================
    static async register(userData) {
        try {
            // تجهيز البيانات لتطابق الـ Backend
            const payload = {
                FirstName: userData.firstName,
                LastName: userData.lastName,
                Email: userData.email,
                NationalID: userData.nationalID,
                PhoneNumber: userData.phoneNumber,
                Password: userData.password,
                ConfirmPassword: userData.confirmPassword,
                UserType: userData.userType // "Client" Or "Owner"
            };

            const response = await fetch(`${API_BASE_URL}/Users/register`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            const data = await response.json();

            if (response.ok) {
                return { success: true, message: "تم إنشاء الحساب بنجاح!" };
            } else {
                let errorMsg = data.message || 'حدث خطأ أثناء التسجيل';
                if(data.errors) {
                     // دمج الأخطاء في رسالة واحدة
                     errorMsg += ": " + JSON.stringify(data.errors);
                }
                return { success: false, message: errorMsg };
            }
        } catch (error) {
            console.error('Register Error:', error);
            return { success: false, message: "فشل الاتصال بالخادم" };
        }
    }
    
    // ================== Logout ==================
    static logout() {
        localStorage.removeItem('userToken');
        localStorage.removeItem('userRole');
        localStorage.removeItem('isLoggedIn');
        window.location.href = 'login.html';
    }
}
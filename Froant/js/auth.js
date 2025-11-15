// auth.js
class MabeetAuth {
    static userTypes = {
        STUDENT: 'student',
        REGULAR: 'regular',
        HOTEL_OWNER: 'hotel_owner',
        BROKER: 'broker'
    };

    static register(userData) {
        // التحقق من صحة البيانات
        if (!this.validateUserData(userData)) {
            return false;
        }

        // تخزين بيانات المستخدم
        localStorage.setItem('userData', JSON.stringify(userData));
        localStorage.setItem('userLoggedIn', 'true');
        localStorage.setItem('userEmail', userData.email);
        
        return true;
    }

    static login(email, password) {
        // في التطبيق الحقيقي، سيتم الاتصال بالخادم
        // هنا محاكاة للعملية
        const userData = JSON.parse(localStorage.getItem('userData') || '{}');
        
        if (userData.email === email && userData.password === password) {
            localStorage.setItem('userLoggedIn', 'true');
            localStorage.setItem('userEmail', email);
            return true;
        }
        
        return false;
    }

    static logout() {
        localStorage.removeItem('userLoggedIn');
        localStorage.removeItem('userEmail');
        // لا نزيل userData للحفاظ على الملف الشخصي
    }

    static isLoggedIn() {
        return localStorage.getItem('userLoggedIn') === 'true';
    }

    static getCurrentUser() {
        if (this.isLoggedIn()) {
            return JSON.parse(localStorage.getItem('userData') || '{}');
        }
        return null;
    }

    static getUserType() {
        const user = this.getCurrentUser();
        return user ? user.userType : null;
    }

    static validateUserData(userData) {
        // التحقق من صحة البيانات
        const requiredFields = ['firstName', 'lastName', 'email', 'password', 'userType'];
        
        for (let field of requiredFields) {
            if (!userData[field]) {
                return false;
            }
        }

        // التحقق من صحة البريد الإلكتروني
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(userData.email)) {
            return false;
        }

        // التحقق من قوة كلمة المرور
        if (userData.password.length < 6) {
            return false;
        }

        return true;
    }

    static isHotelOwner() {
        return this.getUserType() === this.userTypes.HOTEL_OWNER;
    }

    static isBroker() {
        return this.getUserType() === this.userTypes.BROKER;
    }

    static isStudent() {
        return this.getUserType() === this.userTypes.STUDENT;
    }
}
// Authentication functions
const auth = {
    // Check if user is logged in
    isLoggedIn() {
        const token = localStorage.getItem('token');
        const user = localStorage.getItem('user');
        return token && user;
    },

    // Get current user
    getCurrentUser() {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    },

    // Login function
    async login(username, password) {
        try {
            utils.showLoading();
            
            const response = await api.auth.login(username, password);
            
            // Store token and user data
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify({
                employeeId: response.employeeId,
                username: response.username,
                fullName: response.fullName,
                role: response.role
            }));

            utils.hideLoading();
            utils.showToast('เข้าสู่ระบบสำเร็จ', 'success');
            
            // Redirect based on role
            if (response.role === 'Admin') {
                window.location.href = '/admin/dashboard.html';
            } else {
                window.location.href = '/staff/dashboard.html';
            }

            return response;
        } catch (error) {
            utils.hideLoading();
            utils.showToast(error.message || 'เข้าสู่ระบบไม่สำเร็จ', 'error');
            throw error;
        }
    },

    // Logout function
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        utils.showToast('ออกจากระบบแล้ว', 'info');
        window.location.href = '/index.html';
    },

    // Check role permission
    hasRole(requiredRole) {
        const user = this.getCurrentUser();
        if (!user) return false;

        if (requiredRole === 'Admin') {
            return user.role === 'Admin';
        } else if (requiredRole === 'Staff') {
            return user.role === 'Staff' || user.role === 'Admin';
        }

        return false;
    },

    // Initialize auth check for protected pages
    init() {
        // Check if on login page
        if (window.location.pathname.includes('index.html') || window.location.pathname === '/') {
            if (this.isLoggedIn()) {
                const user = this.getCurrentUser();
                if (user.role === 'Admin') {
                    window.location.href = '/admin/dashboard.html';
                } else {
                    window.location.href = '/staff/dashboard.html';
                }
            }
            return;
        }

        // Check authentication for protected pages
        if (!this.isLoggedIn()) {
            window.location.href = '/index.html';
            return;
        }

        // Check role-based access
        const path = window.location.pathname;
        if (path.includes('/admin/')) {
            if (!this.hasRole('Admin')) {
                utils.showToast('คุณไม่มีสิทธิ์เข้าถึงหน้านี้', 'error');
                window.location.href = '/staff/dashboard.html';
                return;
            }
        }

        // Update UI with user info
        this.updateUserInfo();
    },

    // Update user info in UI
    updateUserInfo() {
        const user = this.getCurrentUser();
        if (!user) return;

        // Update user name displays
        const userNameElements = document.querySelectorAll('.user-name');
        userNameElements.forEach(el => el.textContent = user.fullName);

        // Update user role displays
        const userRoleElements = document.querySelectorAll('.user-role');
        userRoleElements.forEach(el => el.textContent = user.role === 'Admin' ? 'ผู้ดูแลระบบ' : 'พนักงาน');

        // Show/hide admin-only elements
        const adminElements = document.querySelectorAll('.admin-only');
        adminElements.forEach(el => {
            if (this.hasRole('Admin')) {
                el.classList.remove('hidden');
            } else {
                el.classList.add('hidden');
            }
        });
    }
};

// Initialize authentication on page load
document.addEventListener('DOMContentLoaded', () => {
    auth.init();
});
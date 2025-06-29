// API Base URL
const API_BASE_URL = 'http://localhost:5081/api';

// API Helper Functions
const api = {
    // Get auth token from localStorage
    getToken() {
        return localStorage.getItem('token');
    },

    // Get auth headers
    getHeaders(includeAuth = true) {
        const headers = {
            'Content-Type': 'application/json'
        };

        if (includeAuth) {
            const token = this.getToken();
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }
        }

        return headers;
    },

    // Generic API call function
    async call(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        const config = {
            ...options,
            headers: this.getHeaders(options.includeAuth !== false)
        };

        try {
            const response = await fetch(url, config);
            
            if (response.status === 401) {
                // Unauthorized - redirect to login
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                window.location.href = '/index.html';
                return;
            }

            // Check if response has content
            const contentType = response.headers.get('content-type');
            let data = null;
            
            if (contentType && contentType.includes('application/json')) {
                const text = await response.text();
                if (text.trim()) {
                    data = JSON.parse(text);
                }
            } else {
                // For non-JSON responses or empty responses
                data = await response.text();
            }

            if (!response.ok) {
                throw new Error((data && data.message) || `HTTP ${response.status}: ${response.statusText}`);
            }

            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    // Shorthand methods
    get(endpoint, options = {}) {
        return this.call(endpoint, { method: 'GET', ...options });
    },

    post(endpoint, data = null, options = {}) {
        const config = { method: 'POST', ...options };
        if (data) {
            config.body = JSON.stringify(data);
        }
        return this.call(endpoint, config);
    },

    put(endpoint, data = null, options = {}) {
        const config = { method: 'PUT', ...options };
        if (data) {
            config.body = JSON.stringify(data);
        }
        return this.call(endpoint, config);
    },

    delete(endpoint, options = {}) {
        return this.call(endpoint, { method: 'DELETE', ...options });
    },

    // Auth endpoints
    auth: {
        login(username, password) {
            return api.call('/auth/login', {
                method: 'POST',
                body: JSON.stringify({ username, password }),
                includeAuth: false
            });
        }
    },

    // Health check
    health: {
        check() {
            return api.call('/health', { includeAuth: false });
        }
    },

    // Books endpoints
    books: {
        getAll(page = 1, pageSize = 20, status = null) {
            let endpoint = `/books?page=${page}&pageSize=${pageSize}`;
            if (status) endpoint += `&status=${status}`;
            return api.call(endpoint);
        },

        search(query) {
            return api.call(`/books/search?query=${encodeURIComponent(query)}`);
        },

        create(book) {
            return api.call('/books', {
                method: 'POST',
                body: JSON.stringify(book)
            });
        },

        update(id, book) {
            return api.call(`/books/${id}`, {
                method: 'PUT',
                body: JSON.stringify(book)
            });
        },

        delete(id) {
            return api.call(`/books/${id}`, {
                method: 'DELETE'
            });
        },

        uploadImage(bookId, imageFile) {
            const formData = new FormData();
            formData.append('image', imageFile);
            
            return fetch(`${API_BASE_URL}/books/${bookId}/upload-image`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${api.getToken()}`
                },
                body: formData
            }).then(async response => {
                console.log('Upload response status:', response.status);
                console.log('Upload response headers:', response.headers.get('content-type'));
                
                // Check if response has content
                const contentType = response.headers.get('content-type');
                let data = null;
                
                if (contentType && contentType.includes('application/json')) {
                    const text = await response.text();
                    console.log('Upload response text:', text);
                    if (text.trim()) {
                        data = JSON.parse(text);
                    }
                } else {
                    // For non-JSON responses or empty responses
                    data = await response.text();
                    console.log('Upload response (non-JSON):', data);
                }
                
                if (!response.ok) {
                    throw new Error((data && data.message) || `HTTP ${response.status}: ${response.statusText}`);
                }
                
                return data;
            }).catch(error => {
                console.error('Upload image error:', error);
                throw error;
            });
        },

        deleteImage(bookId) {
            return api.call(`/books/${bookId}/image`, {
                method: 'DELETE'
            });
        }
    },

    // Customers endpoints
    customers: {
        getAll(page = 1, pageSize = 20) {
            return api.call(`/customers?page=${page}&pageSize=${pageSize}`);
        },

        search(query) {
            return api.call(`/customers/search?query=${encodeURIComponent(query)}`);
        },

        getById(id) {
            return api.call(`/customers/${id}`);
        },

        create(customer) {
            return api.call('/customers', {
                method: 'POST',
                body: JSON.stringify(customer)
            });
        },

        update(id, customer) {
            return api.call(`/customers/${id}`, {
                method: 'PUT',
                body: JSON.stringify(customer)
            });
        },

        updateStatus(id, status) {
            return api.call(`/customers/${id}/status`, {
                method: 'PUT',
                body: JSON.stringify({ status })
            });
        },

        exportCSV() {
            return api.call('/customers/export/csv');
        },

        uploadImage(customerId, imageFile) {
            const formData = new FormData();
            formData.append('image', imageFile);
            
            return fetch(`${API_BASE_URL}/customers/${customerId}/upload-image`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${api.getToken()}`
                },
                body: formData
            }).then(async response => {
                const data = await response.json();
                if (!response.ok) {
                    throw new Error(data.message || 'เกิดข้อผิดพลาด');
                }
                return data;
            });
        },

        deleteImage(customerId) {
            return api.call(`/customers/${customerId}/image`, {
                method: 'DELETE'
            });
        }
    },

    // Rentals endpoints
    rentals: {
        getAll(status = null, date = null) {
            let endpoint = '/rentals';
            const params = [];
            if (status) params.push(`status=${status}`);
            if (date) params.push(`date=${date}`);
            if (params.length > 0) {
                endpoint += '?' + params.join('&');
            }
            return api.call(endpoint);
        },

        borrow(rentalData) {
            return api.call('/rentals/borrow', {
                method: 'POST',
                body: JSON.stringify(rentalData)
            });
        },

        return(rentalId, notes) {
            return api.call('/rentals/return', {
                method: 'POST',
                body: JSON.stringify({
                    rentalId,
                    notes
                })
            });
        },

        returnWithFine(rentalId, fineAmount) {
            return api.call('/rentals/return-with-fine', {
                method: 'POST',
                body: JSON.stringify({
                    rentalId,
                    fineAmount
                })
            });
        },

        forceReturn(rentalId) {
            return api.call('/rentals/force-return', {
                method: 'POST',
                body: JSON.stringify({ rentalId })
            });
        },

        getByCustomer(customerId) {
            return api.call(`/rentals/customer/${customerId}`);
        },

        getRecentActivities() {
            return api.call('/rentals/recent-activities');
        }
    },

    // Categories endpoints
    categories: {
        getAll() {
            return api.call('/categories');
        },

        create(category) {
            return api.call('/categories', {
                method: 'POST',
                body: JSON.stringify(category)
            });
        },

        update(id, category) {
            return api.call(`/categories/${id}`, {
                method: 'PUT',
                body: JSON.stringify(category)
            });
        },

        delete(id) {
            return api.call(`/categories/${id}`, {
                method: 'DELETE'
            });
        }
    },

    // Settings endpoints
    settings: {
        getAll() {
            return api.call('/settings');
        }
    },

    // Dashboard endpoints
    dashboard: {
        getStats() {
            return api.call('/dashboard/stats');
        }
    }
};

// Utility functions
const utils = {
    // Format date to Thai locale
    formatDate(date) {
        return new Date(date).toLocaleDateString('th-TH', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    // Format date time to Thai locale
    formatDateTime(date) {
        return new Date(date).toLocaleString('th-TH', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('th-TH', {
            style: 'currency',
            currency: 'THB'
        }).format(amount);
    },

    // Show toast notification
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `fixed top-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 transform transition-all duration-300 ${
            type === 'success' ? 'bg-green-500' :
            type === 'error' ? 'bg-red-500' :
            type === 'warning' ? 'bg-yellow-500' :
            'bg-blue-500'
        } text-white`;
        toast.textContent = message;
        document.body.appendChild(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 10);

        // Remove after 3 seconds
        setTimeout(() => {
            toast.style.transform = 'translateX(400px)';
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, 3000);
    },

    // Show loading spinner
    showLoading() {
        const loading = document.getElementById('loading');
        if (loading) loading.classList.remove('hidden');
    },

    // Hide loading spinner
    hideLoading() {
        const loading = document.getElementById('loading');
        if (loading) loading.classList.add('hidden');
    },

    // Confirm dialog
    async confirm(message) {
        return window.confirm(message);
    }
};
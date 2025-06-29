/**
 * Pagination Component
 * รองรับการแสดงหน้าข้อมูลแบบแบ่งหน้า
 */

class Pagination {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        this.currentPage = options.currentPage || 1;
        this.totalPages = options.totalPages || 1;
        this.totalItems = options.totalItems || 0;
        this.pageSize = options.pageSize || 10;
        this.onPageChange = options.onPageChange || (() => {});
        this.maxVisiblePages = options.maxVisiblePages || 5;
        
        this.render();
    }

    updatePagination(currentPage, totalPages, totalItems, pageSize) {
        this.currentPage = currentPage;
        this.totalPages = totalPages;
        this.totalItems = totalItems;
        this.pageSize = pageSize;
        this.render();
    }

    render() {
        if (!this.container) {
            console.error('Pagination container not found');
            return;
        }

        // ถ้ามีแค่ 1 หน้า ไม่ต้องแสดง pagination
        if (this.totalPages <= 1) {
            this.container.innerHTML = '';
            return;
        }

        const startItem = (this.currentPage - 1) * this.pageSize + 1;
        const endItem = Math.min(this.currentPage * this.pageSize, this.totalItems);

        this.container.innerHTML = `
            <div class="flex flex-col sm:flex-row justify-between items-center space-y-3 sm:space-y-0">
                <!-- ข้อมูลแสดงช่วงรายการ -->
                <div class="text-sm text-gray-700">
                    แสดง <span class="font-medium text-gray-900">${startItem}</span> ถึง 
                    <span class="font-medium text-gray-900">${endItem}</span> จาก 
                    <span class="font-medium text-gray-900">${this.totalItems}</span> รายการ
                </div>

                <!-- ปุ่ม Pagination -->
                <div class="flex items-center space-x-2">
                    ${this.renderPreviousButton()}
                    ${this.renderPageNumbers()}
                    ${this.renderNextButton()}
                </div>
            </div>
        `;

        this.attachEventListeners();
    }

    renderPreviousButton() {
        const isDisabled = this.currentPage <= 1;
        const disabledClass = isDisabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-100';
        
        return `
            <button 
                class="pagination-btn pagination-prev relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-l-md ${disabledClass}"
                ${isDisabled ? 'disabled' : ''}
                data-page="${this.currentPage - 1}"
            >
                <i class="fas fa-chevron-left"></i>
                <span class="ml-1 hidden sm:inline">ก่อนหน้า</span>
            </button>
        `;
    }

    renderNextButton() {
        const isDisabled = this.currentPage >= this.totalPages;
        const disabledClass = isDisabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-100';
        
        return `
            <button 
                class="pagination-btn pagination-next relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-r-md ${disabledClass}"
                ${isDisabled ? 'disabled' : ''}
                data-page="${this.currentPage + 1}"
            >
                <span class="mr-1 hidden sm:inline">ถัดไป</span>
                <i class="fas fa-chevron-right"></i>
            </button>
        `;
    }

    renderPageNumbers() {
        const pages = this.calculateVisiblePages();
        let html = '';

        pages.forEach(page => {
            if (page === '...') {
                html += `
                    <span class="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300">
                        ...
                    </span>
                `;
            } else {
                const isActive = page === this.currentPage;
                const activeClass = isActive 
                    ? 'z-10 bg-blue-50 border-blue-500 text-blue-600' 
                    : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50';

                html += `
                    <button 
                        class="pagination-btn relative inline-flex items-center px-4 py-2 text-sm font-medium border ${activeClass}"
                        data-page="${page}"
                        ${isActive ? 'aria-current="page"' : ''}
                    >
                        ${page}
                    </button>
                `;
            }
        });

        return html;
    }

    calculateVisiblePages() {
        const pages = [];
        const half = Math.floor(this.maxVisiblePages / 2);

        let start = Math.max(1, this.currentPage - half);
        let end = Math.min(this.totalPages, start + this.maxVisiblePages - 1);

        // ปรับ start ใหม่หากจำนวนหน้าไม่เพียงพอ
        if (end - start + 1 < this.maxVisiblePages) {
            start = Math.max(1, end - this.maxVisiblePages + 1);
        }

        // เพิ่มหน้าแรกและ ... หากจำเป็น
        if (start > 1) {
            pages.push(1);
            if (start > 2) {
                pages.push('...');
            }
        }

        // เพิ่มหน้าที่อยู่ในช่วง
        for (let i = start; i <= end; i++) {
            pages.push(i);
        }

        // เพิ่ม ... และหน้าสุดท้ายหากจำเป็น
        if (end < this.totalPages) {
            if (end < this.totalPages - 1) {
                pages.push('...');
            }
            pages.push(this.totalPages);
        }

        return pages;
    }

    attachEventListeners() {
        const buttons = this.container.querySelectorAll('.pagination-btn');
        buttons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const page = parseInt(e.currentTarget.dataset.page);
                if (page && page !== this.currentPage && page >= 1 && page <= this.totalPages) {
                    this.goToPage(page);
                }
            });
        });
    }

    goToPage(page) {
        if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
            this.currentPage = page;
            this.onPageChange(page);
            this.render();
        }
    }

    // Utility methods
    getPageInfo() {
        return {
            currentPage: this.currentPage,
            totalPages: this.totalPages,
            totalItems: this.totalItems,
            pageSize: this.pageSize,
            startItem: (this.currentPage - 1) * this.pageSize + 1,
            endItem: Math.min(this.currentPage * this.pageSize, this.totalItems)
        };
    }

    refresh() {
        this.render();
    }
}

// Export for use in other files
window.Pagination = Pagination;

// Helper function for creating pagination HTML directly
window.createPaginationHTML = function(options) {
    const {
        currentPage = 1,
        totalPages = 1,
        totalItems = 0,
        startItem = 1,
        endItem = 10,
        onPageChange = () => {},
        showExportButton = true
    } = options;

    // ถ้าไม่มีข้อมูลหรือมีแค่หน้าเดียว
    if (totalPages <= 1 && totalItems <= 10) {
        return showExportButton ? `
            <div class="flex justify-end">
                <button id="exportToExcel" class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg font-medium">
                    <i class="fas fa-file-excel mr-2"></i>ส่งออก Excel
                </button>
            </div>
        ` : '';
    }

    // สร้าง pagination buttons
    const prevDisabled = currentPage <= 1;
    const nextDisabled = currentPage >= totalPages;
    
    // คำนวณหน้าที่จะแสดง
    const maxVisible = 5;
    const pages = [];
    const half = Math.floor(maxVisible / 2);
    let start = Math.max(1, currentPage - half);
    let end = Math.min(totalPages, start + maxVisible - 1);
    
    if (end - start + 1 < maxVisible) {
        start = Math.max(1, end - maxVisible + 1);
    }
    
    if (start > 1) {
        pages.push(1);
        if (start > 2) pages.push('...');
    }
    
    for (let i = start; i <= end; i++) {
        pages.push(i);
    }
    
    if (end < totalPages) {
        if (end < totalPages - 1) pages.push('...');
        pages.push(totalPages);
    }

    return `
        <div class="flex flex-col sm:flex-row justify-between items-center space-y-3 sm:space-y-0">
            <!-- ข้อมูลแสดงช่วงรายการ -->
            <div class="text-sm text-gray-700">
                แสดง <span class="font-medium text-gray-900">${startItem}</span> ถึง 
                <span class="font-medium text-gray-900">${endItem}</span> จาก 
                <span class="font-medium text-gray-900">${totalItems}</span> รายการ
            </div>

            <!-- ปุ่ม Pagination และ Export -->
            <div class="flex items-center space-x-4">
                ${showExportButton ? `
                    <button id="exportToExcel" class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg font-medium">
                        <i class="fas fa-file-excel mr-2"></i>ส่งออก Excel
                    </button>
                ` : ''}
                
                <div class="flex items-center space-x-2">
                    <button 
                        onclick="${prevDisabled ? '' : `(${onPageChange})(${currentPage - 1})`}"
                        class="relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-l-md ${prevDisabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-100'}"
                        ${prevDisabled ? 'disabled' : ''}
                    >
                        <i class="fas fa-chevron-left"></i>
                        <span class="ml-1 hidden sm:inline">ก่อนหน้า</span>
                    </button>
                    
                    ${pages.map(page => {
                        if (page === '...') {
                            return `
                                <span class="relative inline-flex items-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300">
                                    ...
                                </span>
                            `;
                        } else {
                            const isActive = page === currentPage;
                            const activeClass = isActive 
                                ? 'z-10 bg-blue-50 border-blue-500 text-blue-600' 
                                : 'bg-white border-gray-300 text-gray-500 hover:bg-gray-50';
                            
                            return `
                                <button 
                                    onclick="${isActive ? '' : `(${onPageChange})(${page})`}"
                                    class="relative inline-flex items-center px-4 py-2 text-sm font-medium border ${activeClass}"
                                    ${isActive ? 'aria-current="page"' : ''}
                                >
                                    ${page}
                                </button>
                            `;
                        }
                    }).join('')}
                    
                    <button 
                        onclick="${nextDisabled ? '' : `(${onPageChange})(${currentPage + 1})`}"
                        class="relative inline-flex items-center px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-r-md ${nextDisabled ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-100'}"
                        ${nextDisabled ? 'disabled' : ''}
                    >
                        <span class="mr-1 hidden sm:inline">ถัดไป</span>
                        <i class="fas fa-chevron-right"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
};
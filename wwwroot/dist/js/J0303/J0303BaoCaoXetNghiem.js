let fullData = [];
let listDv = [];
let listNhomDv = [];
let currentPage = 1;
let pageSize = 10;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;

function validateDateRange(tuNgay, denNgay) {
    if (!tuNgay || !denNgay) return false;

    const tuNgayDate = new Date(tuNgay);
    const denNgayDate = new Date(denNgay);

    if (tuNgayDate > denNgayDate) {
        toastr.error("Lỗi: Từ ngày phải nhỏ hơn hoặc bằng Đến ngày");
        return false;
    }
    return true;
}

function initDatePicker() {
    $('.date-input').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        language: 'vi',
        todayHighlight: true,
        orientation: 'bottom auto'
    });

    $('.datepicker-trigger').click(function () {
        $(this).closest('.input-group').find('.date-input').datepicker('show');
    });

    function validateDateInput($input) {
        const val = $input.val().trim();
        const isValidFormat = /^\d{2}-\d{2}-\d{4}$/.test(val);

        if (!isValidFormat) {
            const today = new Date();
            const dd = String(today.getDate()).padStart(2, '0');
            const mm = String(today.getMonth() + 1).padStart(2, '0');
            const yyyy = today.getFullYear();
            const todayStr = `${dd}-${mm}-${yyyy}`;

            $input.val(todayStr);
            $input.datepicker('update', todayStr);
        }
    }

    $('.date-input').each(function () {
        const input = this;

        input.addEventListener("input", function () {
            let value = input.value.replace(/\D/g, "");
            let formatted = "";
            let selectionStart = input.selectionStart;

            if (value.length > 0) formatted += value.substring(0, 2);
            if (value.length >= 3) formatted += "-" + value.substring(2, 4);
            if (value.length >= 5) formatted += "-" + value.substring(4, 8);

            if (formatted !== input.value) {
                const prevLength = input.value.length;
                input.value = formatted;
                const newLength = formatted.length;
                const diff = newLength - prevLength;
                input.setSelectionRange(selectionStart + diff, selectionStart + diff);
            }
        });


        input.addEventListener("click", function () {
            const pos = input.selectionStart;
            if (pos <= 2) input.setSelectionRange(0, 2);
            else if (pos <= 5) input.setSelectionRange(3, 5);
            else input.setSelectionRange(6, 10);
        });


        input.addEventListener("keydown", function (e) {
            const pos = input.selectionStart;
            let val = input.value;

            if (e.key === "Backspace" && (pos === 3 || pos === 6)) {
                e.preventDefault();
                input.value = val.slice(0, pos - 1) + val.slice(pos);
                input.setSelectionRange(pos - 1, pos - 1);
            }
            if (e.key === "Delete" && (pos === 2 || pos === 5)) {
                e.preventDefault();
                input.value = val.slice(0, pos) + val.slice(pos + 1);
                input.setSelectionRange(pos, pos);
            }


            if (e.key === "Enter") {
                e.preventDefault();
                validateDateInput($(input));
            }
        });


        input.addEventListener("blur", function () {
            validateDateInput($(input));
        });
    });
}


function formatDateForServer(dateStr) {
    if (!dateStr || typeof dateStr !== 'string') return null;
    const parts = dateStr.split('-');
    if (parts.length !== 3) return null;
    const [day, month, year] = parts;
    return `${year}-${month}-${day}`;
}


function formatDateDisplay(dateString) {
    const date = new Date(dateString);
    if (isNaN(date)) return '';

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    return `${day}-${month}-${year} ${hours}:${minutes}:${seconds}`;
}

function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}

function handleFilter() {
    $('.btnFilterXetNghiem').off('click').on('click', function (e) {
        e.preventDefault();
        showLoading();

        setTimeout(function () {
            try {
                const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
                const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();

                if (!tuNgayRaw || !denNgayRaw) {
                    toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
                    hideLoading();
                    return;
                }

                if (!validateDateRange(tuNgayRaw, denNgayRaw)) {
                    hideLoading();
                    return;
                }

                const tuNgay = formatDateForServer(tuNgayRaw);
                const denNgay = formatDateForServer(denNgayRaw);

                console.log("🔍 Filter với:", { tuNgay, denNgay });

                $.ajax({
                    url: '/bao_cao_xet_nghiem/tk/FilterByDay',
                    type: 'POST',
                    data: { tuNgay, denNgay },
                    beforeSend: function () {
                        showLoading();
                    },
                    success: function (response) {
                        if (response.success) {
                            fullData = response.data || [];

                            currentPage = 1;
                            pageSize = parseInt($('#pageSizeSelect').val()) || 10;

                            lastFilteredTuNgay = tuNgayRaw;
                            lastFilteredDenNgay = denNgayRaw;
                            renderHeader();
                            renderTable();
                            renderPagination();

                            toastr.success("Lọc dữ liệu thành công!");
                        } else {
                            toastr.error("Lỗi khi lọc dữ liệu");
                        }
                    },
                    error: function (xhr) {
                        console.error("❌ Lỗi kết nối:", xhr);
                        toastr.error("❌ Lỗi kết nối: " + xhr.responseText);
                    },
                    complete: function () {
                        hideLoading();
                    }
                });
            } catch (err) {
                console.error("❌ Lỗi trong setTimeout:", err);
                hideLoading();
            }
        }, 100);
    });
}

function renderHeader(tenBacSiDauTien = "Tên người chỉ định") {
    const thead = document.querySelector('.table-wrapper-scroll thead');
    thead.innerHTML = '';

    thead.innerHTML = `
        <tr>
            <th class="text-start" colspan="6">${tenBacSiDauTien}</th>
        </tr>
        <tr>
            <th>STT</th>
            <th>Mã DV</th>
            <th>Tên dịch vụ</th>
            <th>BH</th>
            <th>DV</th>
            <th>Tổng</th>
        </tr>
    `;
}

function renderTable() {
    const tableWrapper = $('.table-wrapper-scroll');
    const tbody = $('#tableBody');
    const tfoot = tableWrapper.find('tfoot')[0];
    tbody.empty();
    tfoot.innerHTML = '';

    showLoading();

    setTimeout(() => {
        try {
            if (!fullData || fullData.length === 0) {
                renderHeader("Tên người chỉ định");
                tbody.append(`<tr><td colspan="6" class="text-center text-muted">Không có dữ liệu</td></tr>`);
                tfoot.innerHTML = '';
                hideLoading();
                return;
            }


            const totalRecords = fullData.length;
            const pages = Math.max(1, Math.ceil(totalRecords / pageSize));
            if (currentPage > pages) currentPage = pages;


            const startIndex = (currentPage - 1) * pageSize;
            const endIndex = Math.min(startIndex + pageSize, totalRecords);
            const currentPageData = fullData.slice(startIndex, endIndex);


            let groupedByDoctor = {};
            currentPageData.forEach(item => {
                let bacsi = item.nguoiChiDinh || "Không rõ bác sĩ";
                if (!groupedByDoctor[bacsi]) groupedByDoctor[bacsi] = [];
                groupedByDoctor[bacsi].push(item);
            });

            let tongCongSLBH = 0, tongCongSLDV = 0, tongCongAll = 0;
            let sttGlobal = startIndex + 1;
            let isFirstDoctor = true;


            const doctors = Object.keys(groupedByDoctor);


            let headerText = "Tên người chỉ định";
            if (doctors.length > 0) {
                headerText = doctors[0];
            }

            renderHeader(headerText);

            if (doctors.length === 0) {
                tbody.append(`<tr><td colspan="6" class="text-center text-muted">Không có dữ liệu cho trang này</td></tr>`);
                hideLoading();
                return;
            }

            doctors.forEach(tenBacSi => {
                const dichVuList = groupedByDoctor[tenBacSi];


                if (!isFirstDoctor) {

                    tbody.append(`
                        <tr>
                            <th class="text-start" colspan="6">${tenBacSi}</th>
                        </tr>
                    `);
                }

                let subSLBH = 0, subSLDV = 0, subTotal = 0;


                dichVuList.forEach(dv => {
                    let slbh = dv.slbh != null ? parseFloat(dv.slbh) : 0;
                    let sldv = dv.sldv != null ? parseFloat(dv.sldv) : 0;
                    let total = slbh + sldv;

                    subSLBH += slbh;
                    subSLDV += sldv;
                    subTotal += total;

                    tbody.append(`
                        <tr>
                            <td>${sttGlobal++}</td>
                            <td>${dv.maDichVu}</td>
                            <td class="text-start">${dv.tenDichVu}</td>
                            <td class="text-end">${formatSoTien(slbh, true)}</td>
                            <td class="text-end">${formatSoTien(sldv, true)}</td>
                            <td class="text-end">${formatSoTien(total, true)}</td>
                        </tr>
                    `);
                });


                tbody.append(`
                    <tr class="subtotal-row">
                        <td class="text-center" colspan="3"><strong>Cộng</strong></td>
                        <td class="text-end"><strong>${formatSoTien(subSLBH, true)}</strong></td>
                        <td class="text-end"><strong>${formatSoTien(subSLDV, true)}</strong></td>
                        <td class="text-end"><strong>${formatSoTien(subTotal, true)}</strong></td>
                    </tr>
                `);

                tongCongSLBH += subSLBH;
                tongCongSLDV += subSLDV;
                tongCongAll += subTotal;

                isFirstDoctor = false;
            });


            let tongCongAllFullData = 0, tongCongSLBHFullData = 0, tongCongSLDVFullData = 0;
            fullData.forEach(item => {
                let slbh = item.slbh != null ? parseFloat(item.slbh) : 0;
                let sldv = item.sldv != null ? parseFloat(item.sldv) : 0;
                tongCongSLBHFullData += slbh;
                tongCongSLDVFullData += sldv;
                tongCongAllFullData += (slbh + sldv);
            });

            tfoot.innerHTML = `
                <tr class="total-row">
                    <td class="text-center" colspan="3"><strong>Tổng cộng</strong></td>
                    <td class="text-end"><strong>${formatSoTien(tongCongSLBHFullData, true)}</strong></td>
                    <td class="text-end"><strong>${formatSoTien(tongCongSLDVFullData, true)}</strong></td>
                    <td class="text-end"><strong>${formatSoTien(tongCongAllFullData, true)}</strong></td>
                </tr>
            `;

        } catch (error) {
            console.error('Lỗi khi render table:', error);
            renderHeader("Tên người chỉ định");
            tbody.append(`<tr><td colspan="6" class="text-center text-danger">Đã xảy ra lỗi khi tải dữ liệu</td></tr>`);
        } finally {
            hideLoading();
        }
    }, 100);
}




function formatSoTien(soTien) {
    if (soTien === null || soTien === '' || soTien === undefined) return 'Không rõ';


    if (typeof soTien === 'string') {
        soTien = parseHocPhiToNumber(soTien);
        if (soTien === null || isNaN(soTien)) {
            return '<span class="text-danger">Sai định dạng</span>';
        }
    }

    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(soTien);
}

$(document).on('change', '#pageSizeSelect', function () {
    pageSize = parseInt($(this).val()) || 10;
    currentPage = 1;

    if (fullData && fullData.length > 0) {
        renderTable();
        renderPagination();
    } else {

    }
});

function renderPagination() {
    const pagination = $('#pagination');
    pagination.empty();

    const totalRecords = fullData.length;
    const pages = Math.max(1, Math.ceil(totalRecords / pageSize));

    if (currentPage > pages) currentPage = pages;

    $('#paginationContainer').text(`Trang ${currentPage}/${pages} – Tổng ${totalRecords} bản ghi`);

    pagination.append(`
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
        </li>
    `);

    const visibleCount = 3;
    let startPage = Math.max(1, currentPage - 1);
    let endPage = Math.min(pages, startPage + visibleCount - 1);

    if (endPage - startPage + 1 < visibleCount) {
        startPage = Math.max(1, endPage - visibleCount + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        pagination.append(`
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `);
    }

    pagination.append(`
        <li class="page-item ${currentPage === pages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.min(pages, currentPage + 1)}">Sau</a>
        </li>
    `);

    pagination.find('a.page-link').on('click', function (e) {
        e.preventDefault();
        const page = parseInt($(this).data('page'));
        if (!isNaN(page) && page !== currentPage) {
            currentPage = page;
            renderTable();
            renderPagination();
        }
    });
}

async function handleExportExcel() {
    const btn = document.getElementById("btnExportExcelXetNghiem");

    btn.addEventListener("click", async function () {
        if (!btn.dataset.originalHTML) {
            btn.dataset.originalHTML = btn.innerHTML.trim();
        }

        const tuNgayRaw = document.getElementById("tuNgayDesktop").value;
        const denNgayRaw = document.getElementById("denNgayDesktop").value;
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);

        if (!tuNgayRaw || !denNgayRaw) {
            toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất Excel.");
            return;
        }

        if (!fullData || fullData.length === 0) {
            toastr.error("Vui lòng lọc dữ liệu trước khi xuất Excel.");
            return;
        }

        if (tuNgayRaw !== lastFilteredTuNgay || denNgayRaw !== lastFilteredDenNgay) {
            toastr.error("Bạn đã thay đổi khoảng thời gian nhưng chưa bấm Lọc lại.");
            return;
        }

        if (!validateDateRange(tuNgayRaw, denNgayRaw)) {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
            return;
        }

        btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
        btn.disabled = true;

        try {
            const formData = new FormData();
            formData.append('tuNgay', tuNgay);
            formData.append('denNgay', denNgay);

            const response = await fetch('/bao_cao_xet_nghiem/export-excel', {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Lỗi khi xuất Excel');
            }

            const blob = await response.blob();


            if (blob.size < 1000) {
                toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
                return;
            }


            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            const filename = `BaoCaoXetNghiem.xlsx`;

            a.download = filename;
            document.body.appendChild(a);
            a.click();


            setTimeout(() => {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 100);

            toastr.success("Xuất Excel thành công!");
        } catch (error) {
            console.error("❌ Lỗi khi xuất Excel:", error);
            toastr.error("Lỗi khi xuất Excel: " + error.message);
        } finally {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        }
    });
}


document.addEventListener('DOMContentLoaded', async () => {
    initDatePicker();
    handleFilter();
    renderHeader();
    renderTable();
    handleExportExcel();
});
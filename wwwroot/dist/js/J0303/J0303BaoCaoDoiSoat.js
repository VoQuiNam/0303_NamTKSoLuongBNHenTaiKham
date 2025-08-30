let fullData = [];
let currentPage = 1;
let pageSize = 20;
let doanhNghiepInfo = null;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;

toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "3000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
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

function updateTable(data) {
    fullData = data || [];
    currentPage = 1;
    pageSize = parseInt($('#pageSizeSelect').val()) || 20;

    renderTable();
    renderPagination();
}

function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    if (!fullData || fullData.length === 0) {
        tbody.append(`<tr><td colspan="16" class="text-center text-muted">Không có dữ liệu phù hợp.</td></tr>`);
        return;
    }

    fullData.sort((a, b) => new Date(a.ngayGioGiaoDich) - new Date(b.ngayGioGiaoDich));

    const startIndex = (currentPage - 1) * pageSize;
    const pageData = fullData.slice(startIndex, startIndex + pageSize);

    let tongBVUBAll = 0;
    let tongBIDVAll = 0;
    fullData.forEach(item => {
        tongBVUBAll += item.bvuB_SoTien || 0;
        tongBIDVAll += item.bidV_SoTien || 0;
    });

   
    pageData.forEach((item, index) => {
        const row = `
            <tr>
                <td class="text-center">${startIndex + index + 1}</td>
                <td class="text-center">${item.maYTe || ''}</td>
                <td class="text-center">${item.maDot || ''}</td>
                <td class="text-start">${item.hoTenBenhNhan || ''}</td>
                <td class="text-center">${item.soDienThoai || ''}</td>
                <td class="text-end">${formatSoTien(item.soTienTrenBL)}</td>
                <td class="text-center">${item.soBL || ''}</td>
                <td class="text-end">${formatSoTien(item.soTienTrenHD)}</td>
                <td class="text-center">${item.soHD || ''}</td>
                <td class="text-end">${formatSoTien(item.tongSoTien)}</td>
                <td class="text-center">${formatDateDisplay(item.ngayGioGiaoDich) || ''}</td>
                <td class="text-start">${item.userThanhToan || ''}</td>
                <td class="text-end">${formatSoTien(item.bvuB_SoTien)}</td>
                <td class="text-center">${item.bvuB_TrangThai || 'Chưa có'}</td>
                <td class="text-end">${formatSoTien(item.bidV_SoTien)}</td>
                <td class="text-center">${item.bidV_TrangThai || 'Chưa có'}</td>
            </tr>
        `;
        tbody.append(row);
    });

    const totalRow = `
        <tr class="fw-bold">
            <td colspan="12" class="text-end fw-bold">Tổng cộng (tất cả trang):</td>
            <td class="text-end fw-bold">${formatSoTien(tongBVUBAll)}</td>
            <td></td>
            <td class="text-end fw-bold">${formatSoTien(tongBIDVAll)}</td>
            <td></td>
        </tr>
    `;
    tbody.append(totalRow);
}


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


$(document).on('change', '#pageSizeSelect', function () {
    pageSize = parseInt($(this).val()) || 10;
    currentPage = 1;

    if (fullData && fullData.length > 0) {
        renderTable();
        renderPagination();
    } else {
        toastr.error("Vui lòng lọc dữ liệu trước khi thay đổi số dòng hiển thị.");
    }
});


function handleFilter() {
    $('.btnFilterBidv').off('click').on('click', function (e) {
        e.preventDefault();

        setTimeout(function () {
            const idChiNhanh = window._idcn;


            const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
            const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();

            if (!tuNgayRaw || !denNgayRaw) {
                toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
                return;
            }




            const tuNgayDate = new Date(tuNgayRaw.split('-').reverse().join('-'));
            const denNgayDate = new Date(denNgayRaw.split('-').reverse().join('-'));

            if (tuNgayDate > denNgayDate) {
                $('#tuNgayDesktop').val(denNgayRaw);
                $('#tuNgayDesktop').datepicker('update', denNgayRaw);

                $('#tuNgayMobile').val(denNgayRaw);
                $('#tuNgayMobile').datepicker('update', denNgayRaw);
            }


            const tuNgay = formatDateForServer($('#tuNgayDesktop').val() || $('#tuNgayMobile').val());
            const denNgay = formatDateForServer($('#denNgayDesktop').val() || $('#denNgayMobile').val());

            if (!validateDateRange(tuNgay, denNgay)) {
                return;
            }


            $.ajax({

                url: '/bao_cao_doi_soat_bidv/tk/FilterByDay',
                type: 'POST',
                data: { tuNgay, denNgay, idChiNhanh },
                success: function (response) {
                    if (response.success) {
                        updateTable(response.data);

                        doanhNghiepInfo = response.thongTinDoanhNghiep || null;
                        if (doanhNghiepInfo) {
                            $('#tenCSKCB').text("🏥 " + doanhNghiepInfo.TenCSKCB);
                            $('#diaChiCSKCB').text("📍 " + doanhNghiepInfo.DiaChi);
                            $('#dienThoaiCSKCB').text("📞 " + doanhNghiepInfo.DienThoai);
                        }
                        lastFilteredTuNgay = tuNgayRaw;
                        lastFilteredDenNgay = denNgayRaw;
                        toastr.success("Lọc dữ liệu thành công!");
                    } else {
                        toastr.error("Lỗi: " + (response.error || "Lỗi khi lọc dữ liệu"));
                    }
                },
                error: function (xhr) {
                    toastr.error("❌ Lỗi kết nối: " + xhr.responseText);
                }
            });
        }, 100);
    });

}


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

async function handleExportExcel() {
    const btn = document.getElementById("btnExportExcelGoiKham");

    btn.addEventListener("click", async function () {
        if (!btn.dataset.originalHTML) {
            btn.dataset.originalHTML = btn.innerHTML.trim();
        }

        const tuNgayRaw = document.getElementById("tuNgayDesktop").value || document.getElementById("tuNgayMobile").value;
        const denNgayRaw = document.getElementById("denNgayDesktop").value || document.getElementById("denNgayMobile").value;
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);
        const idChiNhanh = window._idcn;

       

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

        if (!validateDateRange(tuNgay, denNgay)) {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
            return;
        }

        btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
        btn.disabled = true;

        try {
            const response = await fetch(`/bao_cao_doi_soat_bidv/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}`);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            const blob = await response.blob();

            if (blob.size < 1000) {
                toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
                return;
            }

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "BaoCaoDoiSoatBidv.xlsx";
            document.body.appendChild(a);
            a.click();

            setTimeout(() => {
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            }, 100);

            toastr.success("Xuất Excel thành công!");
        } catch (error) {
            toastr.error("Lỗi khi xuất Excel: " + error.message);
        } finally {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        }
    });
}



function handleExportPDF() {
    $(".btnExportPDFMobile").off("click").on("click", function () {
        exportPDFHandler(this, "Mobile");
    });

    $(".btnExportPDFDesktop").off("click").on("click", function () {
        exportPDFHandler(this, "Desktop");
    });
}

function exportPDFHandler(btn, viewType) {
    if (!btn.dataset.originalHTML) {
        btn.dataset.originalHTML = btn.innerHTML.trim();
    }

    const tuNgay = document.getElementById(viewType === "Mobile" ? "tuNgayMobile" : "tuNgayDesktop").value;
    const denNgay = document.getElementById(viewType === "Mobile" ? "denNgayMobile" : "denNgayDesktop").value;

    if (!fullData || fullData.length === 0) {
        toastr.error("Vui lòng lọc dữ liệu trước khi xuất PDF.");
        return;
    }

    if (!tuNgay || !denNgay) {
        toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất PDF.");
        btn.innerHTML = btn.dataset.originalHTML;
        btn.disabled = false;
        return;
    }

    if (tuNgay !== lastFilteredTuNgay || denNgay !== lastFilteredDenNgay) {
        toastr.error("Bạn đã thay đổi khoảng thời gian nhưng chưa bấm Lọc lại.");
        btn.innerHTML = btn.dataset.originalHTML;
        btn.disabled = false;
        return;
    }

    if (!validateDateRange(tuNgay, denNgay)) {
        btn.innerHTML = btn.dataset.originalHTML;
        btn.disabled = false;
        return;
    }

    btn.innerHTML = `
        <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
    `;
    btn.disabled = true;

    const idChiNhanh = window._idcn;
    const formattedTuNgay = formatDateForServer(tuNgay);
    const formattedDenNgay = formatDateForServer(denNgay);

    let url = "/bao_cao_doi_soat_bidv/export/pdf?";
    if (formattedTuNgay) url += `tuNgay=${formattedTuNgay}&`;
    if (formattedDenNgay) url += `denNgay=${formattedDenNgay}&`;
    if (idChiNhanh) url += `idChiNhanh=${idChiNhanh}`;

    fetch(url, {
        method: "GET",
        headers: { 'Accept': 'application/pdf' }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || "Không thể tải file PDF");
                });
            }
            return response.blob();
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "BaoCaoDoiSoatBidv.pdf";
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);

            toastr.success("Xuất PDF thành công!");

            if (blob.size < 1000) {
                toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
            }
        })

        .catch(error => {
            toastr.error("Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        });
}

function formatSoTien(soTien) {
    if (soTien == null || soTien === '') return 'Không rõ';

    if (typeof soTien === 'string') {
        soTien = parseHocPhiToNumber(soTien);
        if (soTien === null) {
            return '<span class="text-danger">Sai định dạng</span>';
        }
    }
    const formatter = new Intl.NumberFormat('vi-VN', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 20
    });

    return formatter.format(soTien);
}



document.addEventListener('DOMContentLoaded', function () {
    initDatePicker();
    renderTable();
    handleFilter();
    handleExportExcel();
    handleExportPDF();
});
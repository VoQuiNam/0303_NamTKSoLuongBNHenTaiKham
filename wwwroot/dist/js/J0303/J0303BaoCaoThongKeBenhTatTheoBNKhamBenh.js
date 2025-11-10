let fullData = [];
let listPhongBuong = [];
let listDvuKyThuat = [];
let currentPage = 1;
let pageSize = 10;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;


function validateDateRange(tuNgay, denNgay) {
    if (!tuNgay || !denNgay) return false;

    const tuNgayDate = new Date(tuNgay);
    const denNgayDate = new Date(denNgay);
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

function parseHocPhiToNumber(str) {
    if (!str) return null;
    return parseFloat(str.replace(/,/g, ''));
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

function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}


let listDichVuKyThuat = [];
let listNhomDichVuKyThuat = [];

// sửa
async function loadData() {
    try {
        const idChiNhanh = window._idcn || 0;

        const nhomPhongRes = await fetch(`/danh_sach_kham_benh_theo_bac_si/phong-buong/all`).then(r => r.json());
        listPhongBuong = nhomPhongRes;

        const dichVuRes = await fetch('/danh_sach_kham_benh_theo_bac_si/dich-vu-ky-thuat/all').then(r => r.json());
        listDichVuKyThuat = dichVuRes;
    } catch (err) {
        console.error("❌ Lỗi tải JSON:", err);
    }
}

function handleFilter() {
    $('.btnFilter').off('click').on('click', function (e) {
        e.preventDefault();
        showLoading();

        console.log('gọi chưa');
        console.log('gọi chi nhanh: ', window._idcn);
        setTimeout(function () {
            try {

                const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
                const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();



                if (!tuNgayRaw || !denNgayRaw) {
                    toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
                    hideLoading();

                    return;
                }

                const tuNgayDate = new Date(tuNgayRaw.split('-').reverse().join('-'));
                const denNgayDate = new Date(denNgayRaw.split('-').reverse().join('-'));



                if (tuNgayDate > denNgayDate) {
                    console.warn("Cảnh báo: Từ ngày > Đến ngày, đang tự động hoán đổi");
                    $('#tuNgayDesktop').val(denNgayRaw);
                    $('#tuNgayDesktop').datepicker('update', denNgayRaw);
                    $('#tuNgayMobile').val(denNgayRaw);
                    $('#tuNgayMobile').datepicker('update', denNgayRaw);

                    tuNgayRaw = denNgayRaw;
                }

                const tuNgay = formatDateForServer(tuNgayRaw);
                const denNgay = formatDateForServer(denNgayRaw);




                $.ajax({
                    url: '/bao_cao_thong_ke_benh_tat_theo_benh_nhan_kham_benh/tk/FilterByDay',
                    type: 'POST',
                    data: {
                        tuNgay,
                        denNgay,
                        idChiNhanh: window._idcn || 0

                    },
                    beforeSend: function () {

                        showLoading();
                    },
                    success: function (response) {
                        if (response.success) {
                            fullData = response.data || [];

                            console.log('fulldata: ', fullData);
                            currentPage = 1;
                            pageSize = parseInt($('#pageSizeSelect').val()) || 10;


                            renderTable();
                            renderPagination();
                            lastFilteredTuNgay = tuNgayRaw;
                            lastFilteredDenNgay = denNgayRaw;
                            toastr.success("Lọc dữ liệu thành công!");

                            console.log('Filter dates - TuNgay:', tuNgay, 'DenNgay:', denNgay);
                            console.log('All data from API:', response.data);
                            console.log('All data length:', response.data.length);
                        } else {

                            toastr.error("Lỗi: " + (response.error || "Không lấy được dữ liệu"));
                        }
                    },
                    error: function (xhr) {

                        toastr.error("❌ Lỗi kết nối: " + xhr.responseText);
                    }, complete: function () {
                        hideLoading();
                    }
                });

            } catch (err) {
                console.error("❌ Lỗi trong setTimeout:", err);
                console.error("Stack trace:", err.stack);
            }
        }, 100);
    });
}


function renderTable() {
    const tbody = $('#tableBody');
    const tfoot = document.querySelector(".table-wrapper-scroll tfoot");
    tbody.html('');
    tfoot.innerHTML = '';

    const totalCols = 12; // Tổng số cột: Mã ICD + Tên bệnh + Tổng số + 3 cột "Trong đó" + 6 cột "Cách giải quyết"
    showLoading();

    setTimeout(() => {
        try {
            if (!fullData || fullData.length === 0) {
                tbody.html(`
                    <tr>
                        <td colspan="${totalCols}" style="text-align:center; vertical-align:middle; border: 1px solid #ccc; padding: 6px 8px; background-color: #fff;">
                            Không có dữ liệu phù hợp
                        </td>
                    </tr>
                `);
                $('#paginationContainer').text(`Trang 0/0 – Tổng 0 bản ghi`);
                return;
            }

            // 🆕 NHÓM DỮ LIỆU THEO TEN_BENH_GOC
            let groupedByTenBenhGoc = {};
            fullData.forEach(item => {
                let tenBenhGoc = item.tenBenhGoc || "Không xác định";
                if (!groupedByTenBenhGoc[tenBenhGoc]) {
                    groupedByTenBenhGoc[tenBenhGoc] = [];
                }
                groupedByTenBenhGoc[tenBenhGoc].push(item);
            });

            // Thêm debug trong phần nhóm dữ liệu
            Object.keys(groupedByTenBenhGoc).forEach(tenBenh => {
                console.log(`Group ${tenBenh}:`, groupedByTenBenhGoc[tenBenh].length, 'records');
            });

            // 🆕 TẠO MẢNG DỮ LIỆU ĐÃ ĐƯỢC NHÓM THEO TEN_BENH_GOC
            let groupedData = [];
            Object.keys(groupedByTenBenhGoc).forEach(tenBenhGoc => {
                groupedData = groupedData.concat(groupedByTenBenhGoc[tenBenhGoc]);
            });

            const totalRecords = groupedData.length;
            const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));
            if (currentPage > totalPages) currentPage = totalPages;

            const startIndex = (currentPage - 1) * pageSize;
            const pageData = groupedData.slice(startIndex, startIndex + pageSize);

            $('#paginationContainer').text(`Trang ${currentPage}/${totalPages} – Tổng ${totalRecords} bản ghi`);

            let html = '';
            let currentTenBenhGoc = null;
            let displayedTenBenhGoc = new Set();

            pageData.forEach((item, index) => {
                const tenBenhGoc = item.tenBenhGoc || "Không xác định";

                // 🆕 NẾU GẶP TEN_BENH_GOC MỚI, THÊM HEADER
                if (tenBenhGoc !== currentTenBenhGoc && !displayedTenBenhGoc.has(tenBenhGoc)) {
                    // Dòng header cho nhóm bệnh - MERGE TẤT CẢ CÁC CỘT
                    html += `
                        <tr class="benh-goc-header-row" style="font-weight: bold; background-color: #f8f9fa;">
                            <td colspan="${totalCols}" class="text-start fw-bold" style="padding-left: 10px;">${tenBenhGoc}</td>
                        </tr>
                    `;

                    currentTenBenhGoc = tenBenhGoc;
                    displayedTenBenhGoc.add(tenBenhGoc);
                }

                // HIỂN THỊ CHI TIẾT TỪNG BỆNH (có mã ICD)
                let row = `<tr>`;
                row += `<td class="text-center">${item.maBenhEdit || ''}</td>`;
                row += `<td class="text-start">${item.tenBenhEdit || ''}</td>`;
                row += `<td class="text-center">${item.tongSo || 0}</td>`;
                row += `<td class="text-center">${item.trongDoNu || 0}</td>`;
                row += `<td class="text-center">${item.trongDoDuoi15Tuoi || 0}</td>`;
                row += `<td class="text-center">${item.trongDoDuoi6Tuoi || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetRaToa || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetVaoVien || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetNgoaiTru || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetChuyenVien || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetHenTaiKham || 0}</td>`;
                row += `<td class="text-center">${item.cachGiaiQuyetKhac || 0}</td>`;
                row += `</tr>`;
                html += row;
            });

            tbody.html(html);

        } catch (error) {
            console.error('Lỗi khi render table:', error);
            tbody.html(`
                <tr>
                    <td colspan="${totalCols}" style="text-align:center; border:1px solid #ccc; padding:6px; background:#fff; color:red;">
                        Đã xảy ra lỗi khi tải dữ liệu
                    </td>
                </tr>
            `);
        } finally {
            hideLoading();
        }
    }, 100);
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


let tenNVDN = "";

$.getJSON("dist/data/json/Dm_NhanVien.json", data => {
    const nv = data.find(n => n.id === _idNVDN || n.ID === _idNVDN || n.Id === _idNVDN);
    if (nv) {
        tenNVDN = nv.ten || nv.Ten || nv.TenNhanVien || "";
        console.log("Tên nhân viên:", tenNVDN);
    } else {
        console.warn("Không tìm thấy nhân viên có ID =", idNVDN);
    }
});

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
    const idnv = tenNVDN || 0;

    let url = "/bao_cao_thong_ke_benh_tat_theo_benh_nhan_kham_benh/export/pdf?";
    if (formattedTuNgay) url += `tuNgay=${formattedTuNgay}&`;
    if (formattedDenNgay) url += `denNgay=${formattedDenNgay}&`;
    if (idChiNhanh) url += `idChiNhanh=${idChiNhanh}&`;
    if (idnv) url += `idnv=${encodeURIComponent(idnv)}`;

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
            if (blob.size === 0) {
                throw new Error("File PDF trống");
            }

            const blobUrl = window.URL.createObjectURL(blob);

            // 👉 Tạo iframe ẩn và in trực tiếp (giống hàm xuatFilePDF_SinhHocPhanTu)
            const iframe = document.createElement('iframe');
            iframe.style.display = 'none';
            iframe.src = blobUrl;
            document.body.appendChild(iframe);

            iframe.onload = function () {
                const printWindow = iframe.contentWindow;
                printWindow.focus();
                printWindow.print();



            };

            toastr.success("Xuất PDF thành công!");
            if (blob.size < 5000) {
                toastr.warning("File PDF có kích thước nhỏ, có thể không có dữ liệu.");
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

async function handleExportExcel() {
    const btn = document.getElementById("btnExportExcel");

    btn.addEventListener("click", async function () {
        if (!btn.dataset.originalHTML) {
            btn.dataset.originalHTML = btn.innerHTML.trim();
        }

        const tuNgayRaw = document.getElementById("tuNgayDesktop").value || document.getElementById("tuNgayMobile").value;
        const denNgayRaw = document.getElementById("denNgayDesktop").value || document.getElementById("denNgayMobile").value;
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);
        const idChiNhanh = window._idcn;
        const idnv = tenNVDN || 0;

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

            const response = await fetch(`/bao_cao_thong_ke_benh_tat_theo_benh_nhan_kham_benh/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}&idnv=${idnv}`);

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
            a.download = "ThongKeBenhTatTheoBenhNhanKhamBenh.xlsx";
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



//sửa
document.addEventListener('DOMContentLoaded', async () => {
    //await loadHeaderData();
    await loadData();
    renderTable();
    initDatePicker();
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});
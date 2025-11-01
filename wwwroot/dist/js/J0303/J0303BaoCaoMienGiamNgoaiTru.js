let fullData = [];
let listDv = [];
let listNhomDv = [];
let currentPage = 1;
let pageSize = 10;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;
const fixedColumns = [
    { title: 'STT' },
    { title: 'Mã bệnh nhân' },
    { title: 'Số bệnh án' },
    { title: 'Số lưu trữ' },
    { title: 'Họ tên' },
    { title: 'Năm sinh' },
    { title: 'Khoa điều trị' },
    { title: 'Sổ chứng từ' },
    { title: 'Ngày duyệt' },
    { title: 'Người đề nghị duyệt cấp 1' },
    { title: 'Người duyệt cấp 2' },
    { title: 'Tỷ lệ miễn giảm(%)' },
    { title: 'Số tiền miễn giảm' }
    //{ title: 'Tổng cộng' }
];

const fixedEndColumns = [
    { title: 'Tổng cộng' }
];




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


//async function loadJsonData() {
//    try {
//        const [dvRes, nhomDvRes] = await Promise.all([
//            fetch('/bao_cao_mien_giam_ngoai_tru/dich-vu-ky-thuat/all').then(r => r.json()),
//            fetch('/bao_cao_mien_giam_ngoai_tru/nhom-dich-vu/all').then(r => r.json()),
//        ]);

//        listDv = dvRes;
//        listNhomDv = nhomDvRes;

//        console.log('nhóm dv', nhomDvRes);
//        console.log('nhóm ddv', dvRes);

//        renderHeader();



//    } catch (err) {
//        console.error("❌ Lỗi tải JSON:", err);
//    }
//}

function renderHeader() {
    const thead = document.querySelector('table thead');
    thead.innerHTML = '';

    // 🟢 Tổng số cột: cột cố định đầu + 9 cột chi tiết + cột tổng cộng cuối
    const chiTietColumns = [
        { title: 'Thuốc' },
        { title: 'Khám bệnh' },
        { title: 'Xét nghiệm' },
        { title: 'Siêu âm' },
        { title: 'DVKT Cao (Scanner, MRI,..)' },
        { title: 'GPB' },
        { title: 'CĐHA' },
        { title: 'Nội soi' },
        { title: 'Khác' },
    ];

    const totalCols = fixedColumns.length + chiTietColumns.length + fixedEndColumns.length;

    // ===== Hàng 1 =====
    let row1 = `<tr>`;
    row1 += fixedColumns.map((col, idx) => {
        let z = 30;
        if (idx === 2) z = 101;
        else if (idx <= 4) z = 100;

        return `<th rowspan="2" style="z-index:${z}; position:sticky; top:0; 
            background-color:#f8f8f8; border:1px solid #ddd;">
            ${col.title}
        </th>`;
    }).join('');

    // Nhóm cột Chi tiết
    row1 += `<th colspan="${chiTietColumns.length}" 
        style="text-align:center; position:sticky; top:0; z-index:30; background-color:#fff; 
        border:1px solid #ddd; box-shadow:0 2px 3px -1px rgba(0,0,0,0.1);">
        Chi tiết
    </th>`;

    // Cột Tổng cộng (cuối bảng)
    row1 += fixedEndColumns.map(col =>
        `<th rowspan="2" style="position:sticky; top:0; z-index:30; 
        background-color:#f8f8f8; border:1px solid #ddd; 
        box-shadow:0 2px 3px -1px rgba(0,0,0,0.1);">
        ${col.title}</th>`
    ).join('');

    row1 += `</tr>`;

    // ===== Hàng 2 =====
    let row2 = `<tr>`;
    row2 += chiTietColumns.map(c =>
        `<th style="width:150px; position:sticky; z-index:20; background-color:#f8f8f8; 
        border:1px solid #ddd; box-shadow:0 2px 3px -1px rgba(0,0,0,0.1);">
        ${c.title}</th>`
    ).join('');
    row2 += `</tr>`;

    // Gán header vào bảng
    thead.innerHTML = row1 + row2;

    // ===== Điều chỉnh sticky height =====
    setTimeout(() => {
        const row1Height = thead.querySelector('tr:nth-child(1)').offsetHeight;
        const row2Height = thead.querySelector('tr:nth-child(2)').offsetHeight;

        thead.style.setProperty('--row1-height', row1Height + 'px');
        thead.style.setProperty('--row2-height', row2Height + 'px');

        thead.querySelectorAll('tr:nth-child(2) th').forEach(th => {
            th.style.top = `${row1Height}px`;
        });
    }, 0);
}


function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}

function handleFilter() {
    $('.btnFilterNgoaiTru').off('click').on('click', function (e) {
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
                    url: '/bao_cao_mien_giam_ngoai_tru/tk/FilterByDay',
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

/* sửa */
function calculateTotals(data) {
    let totalMienGiam = 0;
    let totalSoTien = 0;
    let totalThuoc = 0;
    let totalChiTietNhom = {};

    listNhomDv.forEach(nhom => totalChiTietNhom[nhom.id] = 0);

    data.forEach(item => {
        totalMienGiam += Number(item.mienGiam) || 0;
      
        totalSoTien += Number(item.soTien) || 0;
        totalThuoc += Number(item.thuoc) || 0;

        listNhomDv.forEach(nhom => {
            if (item.idNhomDichVu === nhom.id) {
                totalChiTietNhom[nhom.id] += Number(item.soTienChiTiet) || 0;
            }
        });
    });

    return {
        totalMienGiam, totalSoTien, totalThuoc, totalChiTietNhom
    };
}

/*sửa */
    function renderTable() {
        const tbody = $('#tableBody');
        const tfoot = document.querySelector(".table-wrapper-scroll tfoot");
        tbody.html('');
        tfoot.innerHTML = '';
        const totalCols = fixedColumns.length + 9 + fixedEndColumns.length;
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

                const totalRecords = fullData.length;
                const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));
                if (currentPage > totalPages) currentPage = totalPages;

                const startIndex = (currentPage - 1) * pageSize;
                const pageData = fullData.slice(startIndex, startIndex + pageSize);

                $('#paginationContainer').text(`Trang ${currentPage}/${totalPages} – Tổng ${totalRecords} bản ghi`);

                let html = '';
                pageData.forEach((item, index) => {
                    let row = `<tr>`;
                    row += `<td class="text-center">${startIndex + index + 1}</td>`;
                    row += `<td class="text-center">${item.maBenhNhan || ''}</td>`;
                    row += `<td class="text-start">${item.soBenhAn || ''}</td>`;
                    row += `<td class="text-start">${item.soLuuTru || ''}</td>`;
                    row += `<td class="text-start">${item.hoTen || ''}</td>`;
                    row += `<td class="text-center">${item.namSinh || ''}</td>`;
                    row += `<td class="text-start">${item.khoaDieuTri || ''}</td>`;
                    row += `<td class="text-start">${item.soChungTu || ''}</td>`;
                    row += `<td class="text-center">${item.ngayDuyet ? formatDateDisplay(item.ngayDuyet) : ''}</td>`;
                    row += `<td class="text-start">${item.nguoiDeNghiDuyetCap1 || ''}</td>`;
                    row += `<td class="text-start">${item.nguoiDuyetCap2 || ''}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.tiLeMienGiam)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.soTienMienGiam)}</td>`;

                    // 🟢 Cột chi tiết cố định
                    row += `<td class="text-end">${formatSoTien(item.thuoc)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.khamBenh)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.xetNghiem)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.sieuAm)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.dvktCao)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.gpb)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.cdha)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.noiSoi)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.khac)}</td>`;

                    // Tổng cộng
                    row += `<td class="text-end fw-bold">${formatSoTien(item.tongCong ?? 0)}</td>`;
                    row += `</tr>`;
                    html += row;
                });

                tbody.html(html);

                // 🧮 Tính tổng
                const totals = {
                    mienGiam: 0,
                    thuoc: 0,
                    khamBenh: 0,
                    xetNghiem: 0,
                    sieuAm: 0,
                    dvktCao: 0,
                    gpb: 0,
                    cdha: 0,
                    noiSoi: 0,
                    khac: 0,
                    tongCong: 0
                };

                fullData.forEach(item => {
                    totals.mienGiam += Number(item.soTienMienGiam) || 0;
                    totals.thuoc += Number(item.thuoc) || 0;
                    totals.khamBenh += Number(item.khamBenh) || 0;
                    totals.xetNghiem += Number(item.xetNghiem) || 0;
                    totals.sieuAm += Number(item.sieuAm) || 0;
                    totals.dvktCao += Number(item.dvktCao) || 0;
                    totals.gpb += Number(item.gpb) || 0;
                    totals.cdha += Number(item.cdha) || 0;
                    totals.noiSoi += Number(item.noiSoi) || 0;
                    totals.khac += Number(item.khac) || 0;
                    totals.tongCong += Number(item.tongCong) || 0;
                });

        

                // 🧾 Dòng tổng
                let totalRow = `<tr style="font-weight:bold; background:#f2f2f2;" class="total-row" >`;
                totalRow += `<td colspan="12" style="text-align:center;">Tổng cộng</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.mienGiam)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.thuoc)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.khamBenh)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.xetNghiem)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.sieuAm)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.dvktCao)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.gpb)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.cdha)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.noiSoi)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.khac)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.tongCong)}</td>`;
                totalRow += `</tr>`;

                tfoot.innerHTML = totalRow;

         


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
    if (soTien === null || soTien === '' || soTien === undefined) return '';

    if (typeof soTien === 'string') {
        soTien = parseHocPhiToNumber(soTien);
        if (soTien === null || isNaN(soTien)) {
            return '<span class="text-danger">Sai định dạng</span>';
        }
    }

    // ✅ Giữ đúng 2 chữ số sau dấu thập phân
    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    }).format(soTien);
}



function renderPagination() {
    const pagination = $('#pagination');
    pagination.empty();


    const grouped = {};
    fullData.forEach(item => {
        const key = `${item.soBienLai}_${item.soChungTu}`;
        if (!grouped[key]) grouped[key] = item;
    });
    const allGroups = Object.values(grouped);

    const totalRecords = allGroups.length;
    const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));
    if (currentPage > totalPages) currentPage = totalPages;

    $('#paginationContainer').text(`Trang ${currentPage}/${totalPages} – Tổng ${totalRecords} bản ghi`);

    pagination.append(`
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
        </li>
    `);

    const visibleCount = 3;
    let startPage = Math.max(1, currentPage - 1);
    let endPage = Math.min(totalPages, startPage + visibleCount - 1);

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
        <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.min(totalPages, currentPage + 1)}">Sau</a>
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


function handleExportPDF() {
    $(".btnExportPDFMobile").off("click").on("click", function () {
        exportPDFHandler(this, "Mobile");
    });

    $(".btnExportPDFDesktop").off("click").on("click", function () {
        exportPDFHandler(this, "Desktop");
    });
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


function handleExportExcel() {
    const btnDesktop = document.getElementById("btnExportExcelMienGiamNgoaiTru");
    const btnMobile = document.getElementById("btnExportExcelMienGiamNgoaiTruMobile");

    [btnDesktop, btnMobile].forEach(btn => {
        if (!btn) return;

        btn.addEventListener("click", async function () {
            if (!btn.dataset.originalHTML) btn.dataset.originalHTML = btn.innerHTML.trim();

            const isMobile = btn === btnMobile;
            const tuNgayRaw = isMobile ? document.getElementById("tuNgayMobile").value : document.getElementById("tuNgayDesktop").value;
            const denNgayRaw = isMobile ? document.getElementById("denNgayMobile").value : document.getElementById("denNgayDesktop").value;
            const tuNgay = formatDateForServer(tuNgayRaw);
            const denNgay = formatDateForServer(denNgayRaw);
            const idcn = window._idcn || 0;
            const idnv = tenNVDN || 0;

            console.log('idcn: ', idcn);



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



            if (!validateDateRange(tuNgayRaw, denNgayRaw)) return;

            btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
            btn.disabled = true;

            try {
                let url = `/bao_cao_mien_giam_ngoai_tru/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idcn}&idnv=${idnv}`;

                const response = await fetch(url);
                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(errorText || "Lỗi khi xuất Excel");
                }

                const blob = await response.blob();
                if (blob.size < 1000) {
                    toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
                    return;
                }

                const blobUrl = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = blobUrl;
                a.download = `BaoCaoMienGiamNgoaiTru.xlsx`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(blobUrl);

                toastr.success("Xuất Excel thành công!");
            } catch (error) {
                console.error(error);
                toastr.error("Lỗi khi xuất Excel: " + error.message);
            } finally {
                btn.innerHTML = btn.dataset.originalHTML;
                btn.disabled = false;
            }
        });
    });
}



document.addEventListener('DOMContentLoaded', async () => {
    initDatePicker();
    renderHeader();
    renderTable();

    handleFilter();
    handleExportExcel();
});
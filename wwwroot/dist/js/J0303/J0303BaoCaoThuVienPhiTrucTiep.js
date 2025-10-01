let fullData = [];
let listDv = [];
let listNhomDv = [];
let currentPage = 1;
let pageSize = 10;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;
const fixedColumns = [
    { title: 'STT' },
    { title: 'Mã BN/Mã đợt' },
    { title: 'Họ và tên' },
    { title: 'Năm sinh' },
    { title: 'Mã thẻ BHYT' },
    { title: 'Đối tượng' },
    { title: 'Ngày thu' },
    { title: 'Quyển sổ' },
    { title: 'Số biên lai' },
    { title: 'Số chứng từ' },
    { title: 'Miễn giảm' },
    { title: 'Lý do miễn' },
    { title: 'Nhập viện nhập miễn' },
    { title: 'Ghi chú miễn' },
    { title: 'Nợ' },
    { title: 'Số tiền' }
];

const fixedEndColumns = [
    { title: 'Hủy' },
    { title: 'Hoàn' },
    { title: 'Ngày Hủy/Hoàn' }
];



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


async function loadJsonData() {
    try {
        const [dvRes, nhomDvRes] = await Promise.all([
            fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/dich-vu-ky-thuat/all').then(r => r.json()),
            fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/nhom-dich-vu/all').then(r => r.json()),
        ]);

        listDv = dvRes;
        listNhomDv = nhomDvRes;

        renderHeader(); 



    } catch (err) {
        console.error("❌ Lỗi tải JSON:", err);
    }
}

    function renderHeader() {
        const thead = document.querySelector('table thead');
        thead.innerHTML = '';


        const totalCols = fixedColumns.length + listNhomDv.length + fixedEndColumns.length + 2;



        let row1 = `<tr>`;
      
        row1 += fixedColumns.map((col, idx) => {
            let z = 30;
            if (idx === 2) {
                z = 101; 
            } else if (idx <= 3) {
                z = 100;
            }
            return `<th rowspan="2" style="z-index:${z}; position:sticky; top:0; background-color:#f8f8f8; border:1px solid #ddd;">${col.title}</th>`;
        }).join('');



        row1 += `<th colspan="${listNhomDv.length + 2}" style="text-align:center; position:sticky; top:0; z-index:30; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">Thông tin chi tiết</th>`;
        row1 += fixedEndColumns.map(col =>
            `<th rowspan="2" style="position:sticky; top:0; z-index:30; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${col.title}</th>`
        ).join('');

        row1 += `</tr>`;

        let row2 = `<tr>`;
        row2 += `<th style="position:sticky; z-index:20; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">Thuốc</th>`;
        row2 += listNhomDv.map(nhom =>
            `<th style="width:150px; position:sticky; z-index:20; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${nhom.ten}</th>`
        ).join('');
        row2 += `<th style="position:sticky; z-index:20; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">Tổng cộng</th>`;
        row2 += `</tr>`;

        let row3 = `<tr>`;


        let colIndex = 0;


        for (let i = 0; i < fixedColumns.length; i++) {
            row3 += `<th style="position:sticky; z-index:${i <= 3 ? 100 : 10}; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex === 0 ? 'A' : colIndex}</th>`;
            colIndex++;
        }


        row3 += `<th style="position:sticky; z-index:10; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex}</th>`;
        colIndex++;

        for (let i = 0; i < listNhomDv.length; i++) {
            row3 += `<th style="position:sticky; z-index:10; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex}</th>`;
            colIndex++;
        }

        row3 += `<th style="position:sticky; z-index:10; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex}</th>`;
        colIndex++;

        for (let i = 0; i < fixedEndColumns.length; i++) {
            row3 += `<th style="position:sticky; z-index:10; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex}</th>`;
            colIndex++;
        }

        row3 += `</tr>`;

        thead.innerHTML = row1 + row2 + row3;


        setTimeout(() => {
            const row1Height = thead.querySelector('tr:nth-child(1)').offsetHeight;
            const row2Height = thead.querySelector('tr:nth-child(2)').offsetHeight;

            thead.style.setProperty('--row1-height', row1Height + 'px');
            thead.style.setProperty('--row2-height', row2Height + 'px');

          
            thead.querySelectorAll('tr:nth-child(2) th').forEach(th => {
                th.style.top = `${row1Height}px`;
            });

           
            thead.querySelectorAll('tr:nth-child(3) th').forEach((th, idx) => {
                const isSpecial =
                    idx === 0 ||                 
                    (idx >= 1 && idx <= 15) ||    
                    (idx >= 29 && idx <= 31);    

                th.style.top = isSpecial
                    ? `${row1Height + row2Height - 1}px`
                    : `${row1Height + row2Height}px`;

               
                if (idx === 2) {
                    th.style.zIndex = 101;
                } else {
                    th.style.zIndex = idx <= 3 ? 100 : 10;
                }
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
    $('.btnFilterBidv').off('click').on('click', function (e) {
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
                    url: '/bao_cao_thu_vien_phi_truc_tiep/tk/FilterByDay',
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

                        console.log('data: ', response);
                        if (response.success) {
                            fullData = response.data || [];
                            console.log("📊 Dữ liệu trả về:", fullData);

                           

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

function calculateTotals(data) {
    let totalMienGiam = 0;
    let totalNo = 0;
    let totalSoTien = 0;
    let totalThuoc = 0;
    let totalChiTietNhom = {};

    listNhomDv.forEach(nhom => totalChiTietNhom[nhom.id] = 0);

    data.forEach(item => {
        totalMienGiam += Number(item.mienGiam) || 0;
        totalNo += Number(item.no) || 0;
        totalSoTien += Number(item.soTien) || 0;
        totalThuoc += Number(item.thuoc) || 0;

        listNhomDv.forEach(nhom => {
            if (item.idNhomDichVu === nhom.id) {
                totalChiTietNhom[nhom.id] += Number(item.soTienChiTiet) || 0;
            }
        });
    });

    return { totalMienGiam, totalNo, totalSoTien, totalThuoc, totalChiTietNhom };
}

function renderTable() {
        const tbody = $('#tableBody');
        const tfoot = document.querySelector(".table-wrapper-scroll tfoot");
        tbody.html('');
        tfoot.innerHTML = '';
        const totalCols = fixedColumns.length + listNhomDv.length + fixedEndColumns.length + 2;
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

             
                const grouped = {};
                fullData.forEach(item => {
                    const key = `${item.soBienLai}_${item.soChungTu}`;
                    if (!grouped[key]) {
                        grouped[key] = {
                            ...item,
                            dichvu: {}
                        };
                    }
                    const nhomId = item.idNhomDVKT;
                    const val = Number(item.soTienChiTiet) || 0;
                    if (nhomId) {
                        grouped[key].dichvu[nhomId] = (grouped[key].dichvu[nhomId] || 0) + val;
                    }
                });

                const allGroups = Object.values(grouped);

              
                const totalRecords = allGroups.length;
                const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));
                if (currentPage > totalPages) currentPage = totalPages;

                const startIndex = (currentPage - 1) * pageSize;
                const pageData = allGroups.slice(startIndex, startIndex + pageSize);

                $('#paginationContainer').text(`Trang ${currentPage}/${totalPages} – Tổng ${totalRecords} bản ghi`);

               
                let html = '';
                pageData.forEach((item, index) => {
                    let row = `<tr>`;
                    row += `<td class="text-center">${startIndex + index + 1}</td>`;
                    row += `<td class="text-center">${item.maBN || ''}</td>`;
                    row += `<td class="text-start">${item.hoTen || ''}</td>`;
                    row += `<td class="text-center">${item.namSinh || ''}</td>`;
                    row += `<td class="text-center">${item.maTheBHYT || ''}</td>`;
                    row += `<td class="text-start">${item.doiTuong || ''}</td>`;
                    row += `<td class="text-center">${item.ngayThu ? formatDateDisplay(item.ngayThu) : ''}</td>`;
                    row += `<td class="text-center">${item.quyenSo || ''}</td>`;
                    row += `<td class="text-center">${item.soBienLai || ''}</td>`;
                    row += `<td class="text-center">${item.soChungTu || ''}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.mienGiam)}</td>`;
                    row += `<td class="text-end">${item.lyDoMien || ''}</td>`;
                    row += `<td class="text-end">${item.nhapVienNhapMien || ''}</td>`;
                    row += `<td class="text-end">${item.ghiChuMien || ''}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.no)}</td>`;
                    row += `<td class="text-end">${formatSoTien(item.soTien)}</td>`; 
                    row += `<td class="text-end">${formatSoTien(item.thuoc)}</td>`;

                   
                    listNhomDv.forEach(nhom => {
                        const val = item.dichvu[nhom.id] || 0;
                        row += `<td class="text-end">${formatSoTien(val)}</td>`;
                    });

                   
                    row += `<td class="text-end fw-bold">${formatSoTien(item.soTien)}</td>`;

                    row += `<td class="text-end">${item.huy || '-'}</td>`;
                    row += `<td class="text-end">${item.hoan || '-'}</td>`;
                    row += `<td class="text-end">${item.ngayHuyHoan || '-'}</td>`;
                    row += `</tr>`;

                    html += row;
                });

                tbody.html(html);

               
                const totals = {
                    totalMienGiam: 0,
                    totalNo: 0,
                    totalSoTien: 0,
                    totalThuoc: 0,
                    totalChiTietNhom: {},
                    totalTong: 0
                };

                allGroups.forEach(item => {
                    totals.totalMienGiam += Number(item.mienGiam) || 0;
                    totals.totalNo += Number(item.no) || 0;
                    totals.totalSoTien += Number(item.soTien) || 0;
                    totals.totalThuoc += Number(item.thuoc) || 0;
                    listNhomDv.forEach(nhom => {
                        totals.totalChiTietNhom[nhom.id] = (totals.totalChiTietNhom[nhom.id] || 0) + (item.dichvu[nhom.id] || 0);
                    });
                    totals.totalTong += Number(item.soTien) || 0;
                });

                let totalRow = `<tr style="font-weight:bold; background:#f2f2f2;">`;
                totalRow += `<td colspan="10" style="text-align:center;">Tổng cộng</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.totalMienGiam)}</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.totalNo)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.totalSoTien)}</td>`;
                totalRow += `<td class="text-end">${formatSoTien(totals.totalThuoc)}</td>`;

                listNhomDv.forEach(nhom => {
                    totalRow += `<td class="text-end">${formatSoTien(totals.totalChiTietNhom[nhom.id] || 0)}</td>`;
                });

                totalRow += `<td class="text-end">${formatSoTien(totals.totalTong)}</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `<td class="text-end">-</td>`;
                totalRow += `</tr>`;

                tfoot.innerHTML = totalRow;

            } catch (error) {
                console.error('Lỗi khi render table:', error);
                tbody.html(`
                    <tr>
                        <td colspan="50" style="text-align:center; vertical-align:middle; border: 1px solid #ccc; padding: 6px 8px; background-color: #fff; color: red;">
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
        return;
    }

    if (tuNgay !== lastFilteredTuNgay || denNgay !== lastFilteredDenNgay) {
        toastr.error("Bạn đã thay đổi khoảng thời gian nhưng chưa bấm Lọc lại.");
        btn.innerHTML = btn.dataset.originalHTML;
        btn.disabled = false;
        return;
    }



    if (!validateDateRange(tuNgay, denNgay)) {
        return;
    }

    btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
    btn.disabled = true;

    const idChiNhanh = window._idcn || 0;
    const formattedTuNgay = formatDateForServer(tuNgay);
    const formattedDenNgay = formatDateForServer(denNgay);


    const idNhomDichVu = 0;
    const idDichVuKyThuat = 0;

    let url = `/bao_cao_thu_vien_phi_truc_tiep/export/pdf?`;
    url += `tuNgay=${formattedTuNgay}&`;
    url += `denNgay=${formattedDenNgay}&`;
    url += `idChiNhanh=${idChiNhanh}&`;
    url = url.replace(/&$/, "");

    fetch(url, {
        method: "GET",
        headers: { 'Accept': 'application/pdf' }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || "Không thể tải file PDF"); });
            }
            return response.blob();
        })
        .then(blob => {
            if (blob.size < 1000) {
                toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
                return;
            }
            const blobUrl = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = blobUrl;
            a.download = `BaoCaoThuVienPhiTrucTiep.pdf`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(blobUrl);
            toastr.success("Xuất PDF thành công!");
        })
        .catch(error => {
            toastr.error("Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        });
}


function handleExportExcel() {
    const btnDesktop = document.getElementById("btnExportExcelGoiKham");
    const btnMobile = document.getElementById("btnExportExcelGoiKhamMobile");

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
                let url = `/bao_cao_thu_vien_phi_truc_tiep/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idcn}`;

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
                a.download = `BaoCaoThuVienPhiTrucTiep.xlsx`;
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
    await loadJsonData();
    renderTable();
    
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});
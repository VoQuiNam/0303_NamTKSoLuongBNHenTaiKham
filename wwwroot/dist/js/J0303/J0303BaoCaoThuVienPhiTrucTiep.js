let fullData = [];
let listNhomDv = [];
let currentPage = 1;
let pageSize = 10;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;
const fixedColumns = [
    { title: 'STT', width: '50px' },
    { title: 'Mã BN/Mã đợt', width: '100px' },
    { title: 'Họ và tên', width: '120px' },
    { title: 'Năm sinh', width: '80px' },
    { title: 'Mã thẻ BHYT', width: '120px' },
    { title: 'Đối tượng', width: '100px' },
    { title: 'Ngày thu', width: '100px' },
    { title: 'Quyển sổ', width: '100px' },
    { title: 'Số biên lai', width: '100px' },
    { title: 'Số chứng từ', width: '100px' },
    { title: 'Miễn giảm', width: '90px' },
    { title: 'Lý do miễn', width: '120px' },
    { title: 'Nhập viện nhập miễn', width: '150px' },
    { title: 'Ghi chú miễn', width: '150px' },
    { title: 'Nợ', width: '80px' },
    { title: 'Số tiền', width: '100px' }
];

const fixedEndColumns = [
    { title: 'Hủy', width: '90px' },
    { title: 'Hoàn', width: '90px' },
    { title: 'Ngày Hủy/Hoàn', width: '120px' }
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


async function loadJsonData() {
    try {
        const nhomDvRes = await fetch('/dist/data/json/DM_NhomDichVuKyThuat.json').then(r => r.json());
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
    row1 += fixedColumns.map(col =>
        `<th rowspan="2" style="width:${col.width}; position:sticky; top:0; z-index:30; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${col.title}</th>`
    ).join('');
    row1 += `<th colspan="${listNhomDv.length + 2}" style="text-align:center; position:sticky; top:0; z-index:30; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">Thông tin chi tiết</th>`;
    row1 += fixedEndColumns.map(col =>
        `<th rowspan="2" style="width:${col.width}; position:sticky; top:0; z-index:30; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${col.title}</th>`
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
        row3 += `<th style="position:sticky; z-index:10; background-color:#f8f8f8; border:1px solid #ddd; box-shadow: 0 2px 3px -1px rgba(0,0,0,0.1);">${colIndex === 0 ? 'A' : colIndex}</th>`;
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

        
        thead.querySelectorAll('tr:nth-child(2) th').forEach(th => {
            th.style.top = `${row1Height}px`;
        });

        thead.querySelectorAll('tr:nth-child(3) th').forEach(th => {
            th.style.top = `${row1Height + row2Height}px`;
        });
    }, 0);
}

function handleFilter() {
    $('.btnFilterBidv').off('click').on('click', function (e) {
        e.preventDefault();

        setTimeout(function () {
            try {

                const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
                const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();



                if (!tuNgayRaw || !denNgayRaw) {
                    toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
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
                        idChiNhanh: window._idcn || 0,
                        idNhomDichVu: 0,
                        idDichVuKyThuat: 0
                    },
                    success: function (response) {


                        if (response.success) {
                            fullData = response.data || [];

                           
                            fullData.forEach(item => {
                                const nhomdichvu = listNhomDv.find(p => p.id === item.idNhomDichVu);
                                item.tenNhomDV = nhomdichvu?.ten || "Không rõ nhóm dịch vụ";
                            });

                            console.log("✅ Dữ liệu sau khi xử lý thêm tên nhóm dịch vụ:", fullData); 

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
                    }
                });

            } catch (err) {
                console.error("❌ Lỗi trong setTimeout:", err);
                console.error("Stack trace:", err.stack);
            }
        }, 100);
    });
}

function randomDecimal(min, max) {
    return (Math.random() * (max - min) + min).toFixed(0);
}


function formatSoTienOrDash(value) {
    if (!value || Number(value) === 0) return '-';
    return formatSoTien(value);
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
    tbody.html('');

    if (!fullData || fullData.length === 0) {
        const totalCols = fixedColumns.length + listNhomDv.length + fixedEndColumns.length + 2; 
        tbody.html(`<tr><td colspan="${totalCols}" style="text-align:center;">Không có dữ liệu</td></tr>`);
        return;
    }

    const startIndex = (currentPage - 1) * pageSize;
    const pageData = fullData.slice(startIndex, startIndex + pageSize);

    let html = '';

    pageData.forEach((item, index) => {
        if (item.thuoc === undefined || item.thuoc === null) {
            item.thuoc = 0;
        }

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
        row += `<td class="text-end">${formatSoTienOrDash(item.mienGiam)}</td>`;
        row += `<td class="text-end">${item.lyDoMien || ''}</td>`;
        row += `<td class="text-end">${item.nhapVienNhapMien || ''}</td>`;
        row += `<td class="text-end">${item.ghiChuMien || ''}</td>`;
        row += `<td class="text-end">${formatSoTienOrDash(item.no)}</td>`;
        row += `<td class="text-end">${formatSoTienOrDash(item.soTien)}</td>`;
        row += `<td class="text-end">${formatSoTienOrDash(item.thuoc)}</td>`;

        let sumChiTiet = Number(item.thuoc) || 0;

        listNhomDv.forEach(nhom => {
            if (item.idNhomDichVu === nhom.id) {
                const val = Number(item.soTienChiTiet) || 0;
                row += `<td class="text-end">${formatSoTienOrDash(val)}</td>`;
                sumChiTiet += val;
            } else {
                row += `<td class="text-end">-</td>`;
            }
        });

        row += `<td class="text-end">${formatSoTienOrDash(sumChiTiet)}</td>`;

        row += `<td class="text-end">${item.huy || '-'}</td>`;
        row += `<td class="text-end">${item.hoan || '-'}</td>`;
        row += `<td class="text-end">${item.ngayHuyHoan || '-'}</td>`;
        row += `</tr>`;

        html += row;
    });


    const totals = calculateTotals(fullData);
    let totalChiTietSum = totals.totalThuoc;
    listNhomDv.forEach(nhom => {
        totalChiTietSum += totals.totalChiTietNhom[nhom.id];
    });

   
    let totalRow = `<tr style="font-weight:bold; background:#f2f2f2;">`;
    totalRow += `<td colspan="10" style="text-align:center;">Tổng cộng</td>`; 
    totalRow += `<td class="text-end">${formatSoTienOrDash(totals.totalMienGiam)}</td>`;
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `<td class="text-end">${formatSoTienOrDash(totals.totalNo)}</td>`;
    totalRow += `<td class="text-end">${formatSoTienOrDash(totals.totalSoTien)}</td>`;
    totalRow += `<td class="text-end">${formatSoTienOrDash(totals.totalThuoc)}</td>`;


    listNhomDv.forEach(nhom => {
        totalRow += `<td class="text-end">${formatSoTienOrDash(totals.totalChiTietNhom[nhom.id])}</td>`;
    });

  
    totalRow += `<td class="text-end">${formatSoTienOrDash(totalChiTietSum)}</td>`;

   
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `<td class="text-end">-</td>`;
    totalRow += `</tr>`;

    html += totalRow;

    tbody.html(html);
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

    let url = `/bao_cao_thu_vien_phi_truc_tiep/export/pdf?`;
    url += `tuNgay=${formattedTuNgay}&`;
    url += `denNgay=${formattedDenNgay}&`;
    url += `idChiNhanh=${idChiNhanh}&`;
    url += `idNhomDichVu=${idNhomDichVu}&`;
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
            a.download = `DanhSachThuVienPhiTrucTiep_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.pdf`;
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
                let url = `/bao_cao_thu_vien_phi_truc_tiep/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idcn}&idNhomDichVu=0`;

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
                a.download = `DanhSachThuVienPhiTrucTiep_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.xlsx`;
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

document.addEventListener('DOMContentLoaded', async () => {
    initDatePicker();
    await loadJsonData();
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});
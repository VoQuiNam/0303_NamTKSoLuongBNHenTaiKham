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


async function loadJsonData() {
    try {
        const [dvRes, nhomDvRes, thietBiRes] = await Promise.all([
            fetch('/dist/data/json/DM_DichVuKyThuat.json').then(r => r.json()),
            fetch('/dist/data/json/DM_NhomDichVuKyThuat.json').then(r => r.json()),
           
        ]);

        listDv = dvRes;
        listNhomDv = nhomDvRes;
       

       
    } catch (err) {
        console.error("❌ Lỗi tải JSON:", err);
    }
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
                    url: '/danh_sach_bn_thuc_hien_theo_thiet_bi/tk/FilterByDay',
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
                                const dichvu = listDv.find(k => k.id === item.idDichVuKyThuat);

                                item.tenNhomDV = nhomdichvu?.ten || "Không rõ nhóm dịch vụ";
                                item.tenDV = dichvu?.ten || "Không rõ dịch vụ";

                         
                            });

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

function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    if (!fullData || fullData.length === 0) {
        tbody.append('<tr><td colspan="29" class="text-center">Không có dữ liệu</td></tr>');
        return;
    }

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const pageData = fullData.slice(start, end);

    let totalSoLuong = 0;
    let totalDoanhThu = 0;
    let totalBaoHiem = 0;
    let totalDaThanhToan = 0;
    let totalChuaThanhToan = 0;

    fullData.forEach(item => {
        totalSoLuong += item.soLuong ? parseFloat(item.soLuong) : 0;
        totalDoanhThu += item.doanhThu ? parseFloat(item.doanhThu) : 0;
        totalBaoHiem += item.baoHiem ? parseFloat(item.baoHiem) : 0;
        totalDaThanhToan += item.daThanhToan ? parseFloat(item.daThanhToan) : 0;
        totalChuaThanhToan += item.chuaThanhToan ? parseFloat(item.chuaThanhToan) : 0;
    });


    pageData.forEach((item, index) => {
        const ngayYC = item.ngayYC ? formatDateDisplay(item.ngayYC) : '';
        const ngayTH = item.ngayTH ? formatDateDisplay(item.ngayTH) : '';

        const doanhThu = item.doanhThu ? parseFloat(item.doanhThu) : 0;
        const baoHiem = item.baoHiem ? parseFloat(item.baoHiem) : 0;
        const daThanhToan = item.daThanhToan ? parseFloat(item.daThanhToan) : 0;
        const chuaThanhToan = item.chuaThanhToan ? parseFloat(item.chuaThanhToan) : 0;

        const row = `
          <tr>
            <td class="text-center">${start + index + 1}</td>
            <td class="text-center">${item.maYT || ''}</td>
            <td class="text-center">${item.soHS || ''}</td>
            <td class="text-center">${item.soBA || ''}</td>
            <td class="text-center">${item.icd || ''}</td>
            <td class="text-start">${item.hoTen || ''}</td>
            <td class="text-start">${item.gioiTinh || ''}</td>
            <td class="text-center">${item.soBHYT || ''}</td>
            <td class="text-start">${item.kcbbd || ''}</td>
            <td class="text-center">${item.dt === true ? 'X' : ''}</td>
            <td class="text-start">${item.doiTuong || ''}</td>
            <td class="text-start">${item.tinhTrang || ''}</td>
            <td class="text-start">${item.noiChiDinh || ''}</td>
            <td class="text-start">${item.bacSi || ''}</td>
            <td class="text-start">${item.tenNhomDV || ''}</td>
            <td class="text-start">${item.tenDV || ''}</td>
            <td class="text-center">${item.soLuong || '0'}</td>
            <td class="text-center">${ngayYC}</td>
            <td class="text-center">${ngayTH}</td>
            <td class="text-center">${item.quyenSo || ''}</td>
            <td class="text-center">${item.soBL || ''}</td>
            <td class="text-center">${item.chungTu || ''}</td>
            <td class="text-start">${item.tenThietBi || ''}</td>
            <td class="text-end">${doanhThu > 0 ? formatSoTien(doanhThu) : '-'}</td>
<td class="text-end">${baoHiem > 0 ? formatSoTien(baoHiem) : '-'}</td>
<td class="text-end">${daThanhToan > 0 ? formatSoTien(daThanhToan) : '-'}</td>
<td class="text-end">${chuaThanhToan > 0 ? formatSoTien(chuaThanhToan) : '-'}</td>

            <td class="text-center">${item.huyHoan === true ? 'X' : ''}</td>
            <td class="text-center">${item.trangThaiThanhToan === true ? 'X' : ''}</td>
          </tr>
        `;
        tbody.append(row);
    });

    const totalRow = `
      <tr style="font-weight:bold; background:#f2f2f2;">
        <td colspan="16" class="text-center">Tổng cộng</td>
        <td class="text-end">${totalSoLuong}</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td class="text-end">${formatSoTien(totalDoanhThu)}</td>
        <td class="text-end">${formatSoTien(totalBaoHiem)}</td>
        <td class="text-end">${formatSoTien(totalDaThanhToan)}</td>
        <td class="text-end">${formatSoTien(totalChuaThanhToan)}</td>
        <td></td>
        <td></td
      </tr>
    `;
    tbody.append(totalRow);
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
        toastr.error("Vui lòng lọc dữ liệu trước khi thay đổi số dòng hiển thị.");
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

    let url = `/danh_sach_bn_thuc_hien_theo_thiet_bi/export/pdf?`;
    url += `tuNgay=${formattedTuNgay}&`;
    url += `denNgay=${formattedDenNgay}&`;
    url += `idChiNhanh=${idChiNhanh}&`;
    url += `idNhomDichVu=${idNhomDichVu}&`;
    url += `idDichVuKyThuat=${idDichVuKyThuat}`;
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
            a.download = `DanhSachBNThucHienTheoThietBi_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.pdf`;
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
                let url = `/danh_sach_bn_thuc_hien_theo_thiet_bi/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idcn}&idNhomDichVu=0&idDichVuKyThuat=0`;

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
                a.download = `DanhSachBNThucHienTheoThietBi_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.xlsx`;
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
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});

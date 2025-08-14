let fullData = []; // lưu toàn bộ dữ liệu sau khi lọc
let currentPage = 1;
let pageSize = 20;
let doanhNghiepInfo = null;


// === Khởi tạo Datepicker ===
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
}

// === Định dạng ngày cho server ===
function formatDateForServer(dateStr) {
    if (!dateStr || typeof dateStr !== 'string') return null;
    const parts = dateStr.split('-');
    if (parts.length !== 3) return null;
    const [day, month, year] = parts;
    return `${year}-${month}-${day}`;
}

// === Định dạng ngày hiển thị ===
function formatDateDisplay(dateString) {
    const date = new Date(dateString);
    if (isNaN(date)) return ''; // tránh lỗi nếu date không hợp lệ

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();

    return `${day}-${month}-${year}`;
}


// === Cập nhật bảng dữ liệu ===
function updateTable(data) {
    fullData = data || [];
    currentPage = 1;
    pageSize = parseInt($('#pageSizeSelector').val()) || 20;

    renderTable();
    renderPagination();
}

function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    if (!fullData || fullData.length === 0) {
        tbody.append(`<tr><td colspan="13" class="text-center text-muted">Không có dữ liệu phù hợp.</td></tr>`);
        return;
    }

    // Sắp xếp theo ngày hẹn khám tăng dần
    fullData.sort((a, b) => new Date(a.ngayHenKham) - new Date(b.ngayHenKham));

    const startIndex = (currentPage - 1) * pageSize;
    const pageData = fullData.slice(startIndex, startIndex + pageSize);

    pageData.forEach((item, index) => {
        const row = `
            <tr>
                <td class="text-center">${startIndex + index + 1}</td>
                <td class="text-center">${item.maYTe}</td>
                <td style="max-width: 150px;">${item.hoVaTen}</td>
                <td class="text-center">${item.namSinh}</td>
                <td class="text-center">${item.gioiTinh}</td>
                <td class="text-center">${item.quocTich}</td>
                <td class="text-center" style="max-width: 140px;">${item.cccD_PASSPORT}</td>
                <td class="text-center" style="max-width: 120px;">${item.sdt}</td>
                <td class="text-center">${formatDateDisplay(item.ngayHenKham)}</td>
                <td class="text-center" style="max-width: 150px;">${item.bacSiHenKham}</td>
                <td class="text-center">${item.nhacHen}</td>
                <td style="max-width: 150px;">${item.ghiChu}</td>
                <td class="text-center">${item.idcn}</td>
            </tr>
        `;
        tbody.append(row);
    });
}


function renderPagination() {
    const container = $('#paginationContainer');
    container.empty();

    const totalPages = Math.ceil(fullData.length / pageSize);
    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const li = $(`
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <button class="page-link">${i}</button>
            </li>
        `);
        li.on('click', function () {
            currentPage = i;
            renderTable();
            renderPagination();
        });
        container.append(li);
    }
}

$(document).on('change', '#pageSizeSelector', function () {
    pageSize = parseInt($(this).val());
    currentPage = 1;

    if (fullData && fullData.length > 0) {
        const totalPages = Math.ceil(fullData.length / pageSize);

        renderTable();
        renderPagination();
    } else {
        console.log("⚠️ Chưa có dữ liệu để phân trang.");
        alert("Vui lòng lọc dữ liệu trước khi thay đổi số dòng hiển thị.");
    }
});



// === Xử lý nút lọc ===
function handleFilter() {
    $('.btnFilter').off('click').on('click', function (e) {
        e.preventDefault();

        const idChiNhanh = window._idcn;
        const tuNgay = formatDateForServer($('#tuNgayDesktop').val() || $('#tuNgayMobile').val());
        const denNgay = formatDateForServer($('#denNgayDesktop').val() || $('#denNgayMobile').val());

        // 1. Kiểm tra đã chọn đủ ngày chưa - PHẢI KIỂM TRA TRƯỚC
        if (!tuNgay || !denNgay) {
            alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
            return;
        }

        // 2. Kiểm tra ngày hợp lệ - SAU KHI ĐÃ CÓ ĐỦ 2 NGÀY
        if (!validateDateRange(tuNgay, denNgay)) return;

        // Xử lý AJAX
        $.ajax({
            url: '/tk/FilterByDay',
            type: 'POST',
            data: { tuNgay, denNgay, idChiNhanh },
            success: function (response) {
                console.log("Response từ server:", response);
                if (response.success) {
                    updateTable(response.data);

                    doanhNghiepInfo = response.thongTinDoanhNghiep || null;
                    if (doanhNghiepInfo) {
                        $('#tenCSKCB').text("🏥 " + doanhNghiepInfo.TenCSKCB);
                        $('#diaChiCSKCB').text("📍 " + doanhNghiepInfo.DiaChi);
                        $('#dienThoaiCSKCB').text("📞 " + doanhNghiepInfo.DienThoai);
                    }

                    alert("✅ Lọc dữ liệu thành công!");
                } else {
                    alert("❌ " + (response.error || "Lỗi khi lọc dữ liệu"));
                }
            },
            error: function (xhr) {
                alert("❌ Lỗi kết nối: " + xhr.responseText);
            }
        });
    });
}



// === Xử lý nút xuất Excel ===
function handleExportExcel() {
    $('.btnExportExcel').off('click').on('click', function () {
        const btn = $(this);

        const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
        const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);
        const idChiNhanh = window._idcn;

        const svgExcelIcon = `
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 24 24">
                <path d="M19 2H8a1 1 0 0 0-1 1v4H3v10h4v4a1 1 0 0 0 1 1h11a1 1 0 0 0 1-1V3a1 1 0 0 0-1-1zM8 15l2.5-3L8 9h2l1.5 2L13 9h2l-2.5 3L15 15h-2l-1.5-2L10 15H8z" />
            </svg> Excel`;

        if (!tuNgayRaw || !denNgayRaw) {
            alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất Excel.");
            return;
        }

        if (!validateDateRange(tuNgay, denNgay)) {
            btn.html(svgExcelIcon);
            btn.prop('disabled', false);
            return;
        }

        // Hiển thị spinner và disable nút
        btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
        btn.prop('disabled', true);

        // Tạo URL và chuyển hướng để tải file
        const url = `/export/excel?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}`;
        window.location.href = url;

        alert("✅ Xuất Excel thành công!");

        // Khôi phục nút sau 1.5 giây
        setTimeout(() => {
            btn.html(svgExcelIcon);
            btn.prop('disabled', false);
        }, 1500);
    });
}




// === Xử lý nút xuất PDF ===
function handleExportPDF() {
    $(".btnExportPDFMobile").off("click").on("click", function () {
        exportPDFHandler(this, "Mobile");
    });

    $(".btnExportPDFDesktop").off("click").on("click", function () {
        exportPDFHandler(this, "Desktop");
    });
}

function exportPDFHandler(btn, viewType) {
    const tuNgay = document.getElementById(
        viewType === "Mobile" ? "tuNgayMobile" : "tuNgayDesktop"
    ).value;

    const denNgay = document.getElementById(
        viewType === "Mobile" ? "denNgayMobile" : "denNgayDesktop"
    ).value;

    const svgPDFIcon = `
        <svg class="icon-pdf" xmlns="http://www.w3.org/2000/svg"
             viewBox="0 0 384 512" width="16" height="16" fill="currentColor">
            <path d="M181.9 256.1c-5-16-4.9-46.9-2-46.9 8.4 0 7.6 36.9 2 46.9zm-1.7 47.2c-7.7 20.2-17.3 43.3-28.4 62.7
                     18.3-7 39-17.2 62.9-21.9-12.7-9.6-24.9-23.4-34.5-40.8zM86.1 428.1c0 .8 13.2-5.4 34.9-40.2
                     -6.7 6.3-29.1 24.5-34.9 40.2zM248 160h136v328c0 13.3-10.7 24-24 24H24c-13.3 0-24-10.7-24-24V24
                     C0 10.7 10.7 0 24 0h200v136c0 13.3 10.7 24 24 24z"/>
        </svg> PDF`;

    if (!tuNgay || !denNgay) {
        alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất PDF.");
        btn.innerHTML = svgPDFIcon;
        btn.disabled = false;
        return;
    }

    if (!validateDateRange(tuNgay, denNgay)) {
        btn.innerHTML = svgPDFIcon;
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

    let url = "/export/pdf?";
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
            a.download = "DanhSachHenKham.pdf";
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);

            alert("✅ Xuất PDF thành công!");
        })
        .catch(error => {
            console.error("Error:", error);
            alert("❌ Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = svgPDFIcon;
            btn.disabled = false;
        });
}


function validateDateRange(tuNgay, denNgay) {
    if (!tuNgay || !denNgay) return false;
    
    const tuNgayDate = new Date(tuNgay);
    const denNgayDate = new Date(denNgay);

    if (tuNgayDate > denNgayDate) {
        alert("❌ Lỗi: Từ ngày phải nhỏ hơn hoặc bằng Đến ngày");
        return false;
    }
    return true;
}



// === Khởi chạy tất cả khi DOM sẵn sàng ===
$(document).ready(function () {
    initDatePicker();
    handleFilter();
    handleExportExcel();
    handleExportPDF();
});

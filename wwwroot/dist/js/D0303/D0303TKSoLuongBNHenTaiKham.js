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

        // Tự động định dạng khi nhập
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

        // Chọn khối khi click
        input.addEventListener("click", function () {
            const pos = input.selectionStart;
            if (pos <= 2) input.setSelectionRange(0, 2);
            else if (pos <= 5) input.setSelectionRange(3, 5);
            else input.setSelectionRange(6, 10);
        });

        // Xử lý xóa dấu gạch
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

            // Kiểm tra khi nhấn Enter
            if (e.key === "Enter") {
                e.preventDefault();
                validateDateInput($(input));
            }
        });

        // Kiểm tra khi blur
        input.addEventListener("blur", function () {
            validateDateInput($(input));
        });
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
                <td class="text-center" style="width: 50px;">${startIndex + index + 1}</td>
                <td class="text-center">${item.maYTe}</td>
                <td class="text-start" style="max-width: 150px;">${item.hoVaTen}</td>
                <td class="text-center">${item.namSinh}</td>
                <td class="text-start">${item.gioiTinh}</td>
                <td class="text-start">${item.quocTich}</td>
                <td class="text-center" style="max-width: 140px;">${item.cccD_PASSPORT}</td>
                <td class="text-center" style="max-width: 120px;">${item.sdt}</td>
                <td class="text-center">${formatDateDisplay(item.ngayHenKham)}</td>
                <td class="text-start" style="max-width: 150px;">${item.bacSiHenKham}</td>
                <td class="text-start">${item.nhacHen}</td>
                <td style="max-width: 150px;">${item.ghiChu}</td>
                <td class="text-center" style="width: 50px;">${item.idcn}</td>
            </tr>
        `;
        tbody.append(row);
    });
}


function renderPagination() {
    const container = $('#pagination');
    container.empty();

    const totalPages = Math.ceil(fullData.length / pageSize);
    if (totalPages <= 1) return;

    // Nút Previous
    const prevDisabled = currentPage === 1 ? 'disabled' : '';
    const prevLi = $(`
        <li class="page-item ${prevDisabled}">
            <a class="page-link" href="#" data-page="${currentPage - 1}">&laquo;</a>
        </li>
    `);
    prevLi.on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            renderTable();
            renderPagination();
        }
    });
    container.append(prevLi);

    // Các nút số trang
    for (let i = 1; i <= totalPages; i++) {
        const li = $(`
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `);
        li.on('click', function () {
            currentPage = i;
            renderTable();

            // 👉 Auto scroll ngang nếu là trang cuối
            if (currentPage === totalPages) {
                setTimeout(() => {
                    const wrapper = document.querySelector('.table-wrapper');
                    if (wrapper) {
                        wrapper.scrollLeft = wrapper.scrollWidth;
                    }
                }, 0);
            }

            renderPagination();
        });
        container.append(li);
    }

    // Nút Next
    const nextDisabled = currentPage === totalPages ? 'disabled' : '';
    const nextLi = $(`
        <li class="page-item ${nextDisabled}">
            <a class="page-link" href="#" data-page="${currentPage + 1}">&raquo;</a>
        </li>
    `);
    nextLi.on('click', function () {
        if (currentPage < totalPages) {
            currentPage++;
            renderTable();
            renderPagination();
        }
    });
    container.append(nextLi);
}


$(document).on('change', '#pageSizeSelect', function () {
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

        setTimeout(function () {
            const idChiNhanh = window._idcn;

            const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
            const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();

            if (!tuNgayRaw || !denNgayRaw) {
                alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
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

            if (!validateDateRange(tuNgay, denNgay)) return;

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
        }, 100); // Delay 100ms để input cập nhật xong
    });

}



// === Xử lý nút xuất Excel ===
function handleExportExcel() {
    $('.btnExportExcel').off('click').on('click', function () {
        const btn = $(this);

        // Lưu nội dung gốc nếu chưa có
        if (!btn.data('originalText')) {
            btn.data('originalText', btn.html().trim());
        }

        const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
        const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);
        const idChiNhanh = window._idcn;

        if (!tuNgayRaw || !denNgayRaw) {
            alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất Excel.");
            return;
        }

        if (!validateDateRange(tuNgay, denNgay)) {
            btn.html(btn.data('originalText'));
            btn.prop('disabled', false);
            return;
        }

        // Hiển thị spinner và disable nút
        btn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Đang xuất...');
        btn.prop('disabled', true);

        // Tạo URL và chuyển hướng để tải file
        const url = `/export/excel?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}`;
        window.location.href = url;

        alert("✅ Xuất Excel thành công!");

        // Khôi phục nút sau 1.5 giây
        setTimeout(() => {
            btn.html(btn.data('originalText'));
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
    // Lưu nội dung gốc của nút nếu chưa có
    if (!btn.dataset.originalText) {
        btn.dataset.originalText = btn.innerHTML.trim();
    }

    // Lấy giá trị ngày từ input
    const tuNgay = document.getElementById(
        viewType === "Mobile" ? "tuNgayMobile" : "tuNgayDesktop"
    ).value;

    const denNgay = document.getElementById(
        viewType === "Mobile" ? "denNgayMobile" : "denNgayDesktop"
    ).value;

    // Kiểm tra ngày
    if (!tuNgay || !denNgay) {
        alert("⚠️ Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất PDF.");
        btn.innerHTML = btn.dataset.originalText;
        btn.disabled = false;
        return;
    }

    if (!validateDateRange(tuNgay, denNgay)) {
        btn.innerHTML = btn.dataset.originalText;
        btn.disabled = false;
        return;
    }

    // Hiển thị spinner khi đang xử lý
    btn.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Đang xuất...
    `;
    btn.disabled = true;

    // Chuẩn bị dữ liệu gửi lên server
    const idChiNhanh = window._idcn;
    const formattedTuNgay = formatDateForServer(tuNgay);
    const formattedDenNgay = formatDateForServer(denNgay);

    let url = "/export/pdf?";
    if (formattedTuNgay) url += `tuNgay=${formattedTuNgay}&`;
    if (formattedDenNgay) url += `denNgay=${formattedDenNgay}&`;
    if (idChiNhanh) url += `idChiNhanh=${idChiNhanh}`;

    // Gọi API xuất PDF
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
            // Khôi phục lại nút
            btn.innerHTML = btn.dataset.originalText;
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

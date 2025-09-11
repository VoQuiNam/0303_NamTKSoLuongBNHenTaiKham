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


//async function loadJsonData() {
//    try {
//        const nhomPhongRes = await fetch('/dist/data/json/DM_PhongBuong.json').then(r => r.json());
//        listPhongBuong = nhomPhongRes;
//        renderHeader();
//    } catch (err) {
//        console.error("❌ Lỗi tải JSON:", err);
//    }
//}

function renderHeader() {
    const thead = document.querySelector('table thead');
    thead.innerHTML = '';

    // Tạo hàng tiêu đề với chỉ các phòng ĐÃ LỌC
    let row = `<tr>`;
    row += `<th class="text-center" style="width:10%;">STT</th>`;
    row += `<th class="text-center" style="width:15%;">Dịch vụ</th>`;

    // Thêm cột cho các phòng ĐÃ LỌC
    if (window.filteredPhongList && window.filteredPhongList.length > 0) {
        window.filteredPhongList.forEach(phong => {
            row += `<th class="text-center" style="width:15%;">${phong.ten}</th>`;
        });
    } else {
        // Nếu không có phòng nào được lọc, hiển thị tất cả (fallback)
        listPhongBuong.forEach(phong => {
            row += `<th class="text-center" style="width:15%;">${phong.ten}</th>`;
        });
    }

    row += `<th class="text-center" style="width:11%;">Tổng</th>`;
    row += `</tr>`;

    thead.innerHTML = row;
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
    await loadJsonData();
    //loadDichVu();
    renderTable();
    initDatePicker();
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});

document.addEventListener("DOMContentLoaded", loadDichVu);
/*==================== SỰ KIỆN CHỌN NHÓM DỊCH VỤ ====================*/
let dichVuData = []; // load từ JSON
let currentIndex = -1; // item đang được highlight
let selectedId = 0;    // Ghi nhớ item đã chọn (0 = Tất cả)

const input = document.getElementById("nhomDichVuInput");
const dropdown = document.getElementById("nhomDichVuDropdown");

// Hidden inputs
const hiddenId = document.getElementById("nhomDichVuIdHidden");
const hiddenMa = document.getElementById("nhomDichVuMaHidden");
const hiddenTen = document.getElementById("nhomDichVuTenHidden");
const hiddenVietTat = document.getElementById("nhomDichVuVietTatHidden");

// Mặc định: Tất cả
function setAllDefault() {
    selectedId = 0;
    hiddenId.value = 0;
    hiddenMa.value = "";
    hiddenTen.value = "Tất cả";
    hiddenVietTat.value = "";
    input.value = "Tất cả";
}
setAllDefault();

function renderDropdown(list, selId = null, keyword = "") {
    dropdown.innerHTML = "";
    currentIndex = -1;

    // Luôn chèn "Tất cả" ở đầu
    const allItem = { id: 0, ma: "", ten: "Tất cả", viettat: "" };
    const renderList = [allItem, ...list];

    // Regex không phân biệt hoa/thường
    const regex = keyword ? new RegExp(`(${keyword})`, "gi") : null;

    renderList.forEach((item, index) => {
        const div = document.createElement("div");
        div.className = "dropdown-item d-flex justify-content-between";
        div.dataset.id = item.id;
        div.style.cursor = "pointer";
        div.style.userSelect = "none";

        // Tô màu phần match trong tên
        let tenHTML = item.ten;
        if (regex) tenHTML = tenHTML.replace(regex, `<span style="background:yellow;color:black;">$1</span>`);

        // span tên (bên trái)
        const spanTen = document.createElement("span");
        spanTen.innerHTML = tenHTML;

        // span viết tắt (bên phải)
        const spanVT = document.createElement("span");
        spanVT.style.color = "inherit";
        if (item.viettat) {
            let vtHTML = item.viettat;
            if (regex) vtHTML = vtHTML.replace(regex, `<span style="background:yellow;color:black;">$1</span>`);
            spanVT.innerHTML = `[${vtHTML}]`;
        }

        div.appendChild(spanTen);
        div.appendChild(spanVT);

        div.addEventListener("click", function () {
            selectItem(item);
        });
        dropdown.appendChild(div);
    });

    dropdown.style.display = renderList.length > 0 ? "block" : "none";

    // Nếu có selId (mục đã chọn trước đó) thì highlight nó
    if (selId !== null) {
        const idx = renderList.findIndex(x => x.id === selId);
        if (idx >= 0) {
            highlightItem(idx);
        }
    }
}


function selectItem(item) {
    // 👉 Input chỉ hiện tên
    input.value = item.ten;

    hiddenId.value = item.id;
    hiddenMa.value = item.ma || "";
    hiddenTen.value = item.ten || "";
    hiddenVietTat.value = item.viettat || "";

    // Cập nhật selectedId để lần sau focus sẽ highlight đúng
    selectedId = item.id;

    dropdown.style.display = "none";
}

function highlightItem(index) {
    const items = dropdown.querySelectorAll(".dropdown-item");
    if (!items.length) return;

    items.forEach(el => {
        el.style.background = "";
        el.style.color = "";
    });

    if (index >= 0 && index < items.length) {
        items[index].style.background = "#0d6efd"; // đậm hơn
        items[index].style.color = "#fff";
        currentIndex = index;

        items[index].scrollIntoView({
            block: "nearest",
            behavior: "smooth"
        });
    }
}

async function loadDichVu() {
    try {
        const response = await fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/nhom-dich-vu/all');
        console.log(response);
        // Kiểm tra nếu response không ok thì throw error
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Chỉ parse JSON một lần
        dichVuData = await response.json();

        // Sắp xếp theo tên
        dichVuData.sort((a, b) => a.ten.localeCompare(b.ten, "vi", { sensitivity: "base" }));
        renderDropdown(dichVuData, selectedId);

    } catch (err) {
        console.error("Lỗi load JSON:", err);
    }
}

// ==================== THÊM SỰ KIỆN CHO INPUT ====================
document.addEventListener("DOMContentLoaded", function () {
    // Sự kiện khi click vào input
    input.addEventListener("click", function () {
        renderDropdown(dichVuData, selectedId);
    });

    // Sự kiện khi focus vào input
    input.addEventListener("focus", function () {
        renderDropdown(dichVuData, selectedId);
    });

    // Sự kiện khi nhập từ khóa tìm kiếm
    input.addEventListener("input", function () {
        const keyword = input.value.trim();
        if (keyword === "") {
            renderDropdown(dichVuData, selectedId);
        } else {
            const filtered = dichVuData.filter(item =>
                item.ten.toLowerCase().includes(keyword.toLowerCase()) ||
                (item.viettat && item.viettat.toLowerCase().includes(keyword.toLowerCase()))
            );
            renderDropdown(filtered, null, keyword);
        }
    });

    // Sự kiện khi click ra ngoài để ẩn dropdown
    document.addEventListener("click", function (e) {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.style.display = "none";
        }
    });

    // Sự kiện bàn phím (để di chuyển bằng mũi tên)
    input.addEventListener("keydown", function (e) {
        const items = dropdown.querySelectorAll(".dropdown-item");
        if (!items.length) return;

        if (e.key === "ArrowDown") {
            e.preventDefault();
            highlightItem((currentIndex + 1) % items.length);
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            highlightItem((currentIndex - 1 + items.length) % items.length);
        } else if (e.key === "Enter" && currentIndex >= 0) {
            e.preventDefault();
            const selectedItem = items[currentIndex];
            const itemId = parseInt(selectedItem.dataset.id);
            const allItem = { id: 0, ma: "", ten: "Tất cả", viettat: "" };
            const item = [allItem, ...dichVuData].find(x => x.id === itemId);
            if (item) selectItem(item);
        }
    });
});

// Tải dữ liệu JSON (giả sử được tải trong loadJsonData)
let listDichVuKyThuat = [];
let listNhomDichVuKyThuat = [];

async function loadJsonData() {
    try {
        const nhomPhongRes = await fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/phong-buong/all').then(r => r.json());
        listPhongBuong = nhomPhongRes;

        const dichVuRes = await fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/dich-vu-ky-thuat/all').then(r => r.json());
        listDichVuKyThuat = dichVuRes;

        const nhomDichVuRes = await fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/nhom-dich-vu/all').then(r => r.json());
        listNhomDichVuKyThuat = nhomDichVuRes;

        renderHeader();
    } catch (err) {
        console.error("❌ Lỗi tải JSON:", err);
    }
}

function handleFilter() {
    $('.btnFilterBacSi').off('click').on('click', function (e) {
        e.preventDefault();

        setTimeout(function () {
            try {
                const idChiNhanh = window._idcn || 0;

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

                let idNhomdichvu = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;
                let idPhong = parseInt($("#selectedPhongId").val()) || 0;

                $.ajax({
                    url: '/bao_cao_thu_tong_hop_dv_theo_khoa_phong/tk/FilterByDay',
                    type: 'POST',
                    data: { tuNgay, denNgay, idChiNhanh, idNhomdichvu, idPhong },
                    success: function (response) {
                        if (response.success) {
                            console.log("✅ Dữ liệu trả về:", response);
                            fullData = response.data || [];
                            let filteredPhongIds = [];

                            fullData.forEach(item => {
                                // Chuẩn hóa lại tên field từ server
                                item.idDichVuKyThuat = item.idDichVuKyThuat || item.iddvkt;
                                item.idPhong = item.idPhong || item.idPhongBuong;

                                // Tìm phòng
                                const phong = listPhongBuong.find(p => p.id === item.idPhong);
                                // Tìm dịch vụ kỹ thuật
                                const dichVu = listDichVuKyThuat.find(d => d.id === item.idDichVuKyThuat);
                                // Tìm nhóm DVKT từ dịch vụ
                                const nhomDichVu = dichVu ? listNhomDichVuKyThuat.find(n => n.id === dichVu.idNhomDichVu) : null;

                                // Gán lại tên hiển thị
                                item.tenPhong = phong?.ten || "Không rõ phòng";
                                item.tenDichVuKyThuat = dichVu?.ten || "Không rõ dịch vụ";
                                item.tenNhomDvKyThuat = nhomDichVu?.ten || "Không rõ nhóm dịch vụ";
                                item.idNhomDichVu = dichVu?.idNhomDichVu || 0;

                                if (item.idPhong && !filteredPhongIds.includes(item.idPhong)) {
                                    filteredPhongIds.push(item.idPhong);
                                }
                            });

                            // Lưu danh sách phòng đã lọc
                            window.filteredPhongList = listPhongBuong.filter(phong =>
                                filteredPhongIds.includes(phong.id)
                            );

                            // Reset phân trang + render lại
                            currentPage = 1;
                            pageSize = parseInt($('#pageSizeSelect').val()) || 10;
                            khoaStt = 1;
                            renderHeader();
                            renderTable();
                            renderPagination();

                            lastFilteredTuNgay = tuNgayRaw;
                            lastFilteredDenNgay = denNgayRaw;

                            toastr.success("Lọc dữ liệu thành công!");
                        }
                    },
                    error: function (xhr) {
                        console.error("❌ Lỗi kết nối:", xhr);
                        toastr.error("❌ Lỗi kết nối: " + xhr.responseText);
                    }
                });
            } catch (err) {
                console.error("❌ Lỗi trong setTimeout:", err);
            }
        }, 100);
    });
}



//function renderTable() {
//    const tbody = $('#tableBody');
//    tbody.empty();

//    let selectedNhomDichVuId = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;
//    let filteredNhomDichVuList = listNhomDichVuKyThuat;
//    if (selectedNhomDichVuId !== 0) {
//        filteredNhomDichVuList = listNhomDichVuKyThuat.filter(nhom => nhom.id === selectedNhomDichVuId);
//    }

//    const phongList = window.filteredPhongList || listPhongBuong;

//    // Tính tổng toàn bảng
//    let tongTatCaGia = 0;

//    filteredNhomDichVuList.forEach(nhomDichVu => {
//        if (!nhomDichVu.active) return;
//        // LƯU Ý: không lọc theo dv.active để không bỏ sót DV như "xông họng"
//        const dichVuInNhom = listDichVuKyThuat.filter(dv => dv.idNhomDichVu === nhomDichVu.id);

//        dichVuInNhom.forEach(dv => {
//            phongList.forEach(phong => {
//                const gia = fullData
//                    .filter(item => item.idDichVuKyThuat === dv.id && item.idPhong === phong.id)
//                    .reduce((sum, item) => sum + (item.gia || 0), 0);
//                tongTatCaGia += gia;
//            });
//        });
//    });

//    // Nếu không có dữ liệu
//    if (tongTatCaGia === 0) {
//        const colCount = 2 + phongList.length + 1;
//        tbody.append(`<tr><td colspan="${colCount}" class="text-center text-muted">Không có dữ liệu</td></tr>`);
//        return;
//    }

//    // Duyệt qua từng nhóm
//    filteredNhomDichVuList.forEach((nhomDichVu, index) => {
//        if (!nhomDichVu.active) return;

//        // KHÔNG lọc theo dv.active => hiển thị tất cả dịch vụ trong nhóm
//        const dichVuInNhom = listDichVuKyThuat.filter(dv => dv.idNhomDichVu === nhomDichVu.id);

//        // Tính tổng nhóm và tổng theo phòng trong nhóm
//        let tongNhom = 0;
//        let tongPhongTrongNhom = {};
//        phongList.forEach(phong => tongPhongTrongNhom[phong.id] = 0);

//        dichVuInNhom.forEach(dv => {
//            phongList.forEach(phong => {
//                const gia = fullData
//                    .filter(item => item.idDichVuKyThuat === dv.id && item.idPhong === phong.id)
//                    .reduce((sum, item) => sum + (item.gia || 0), 0);
//                tongNhom += gia;
//                tongPhongTrongNhom[phong.id] += gia;
//            });
//        });

//        // --- DEBUG LOG: in ra tên các dịch vụ trong nhóm + tổng theo phòng để bạn kiểm tra "xông họng" ---
//        console.log(`== NHÓM: ${nhomDichVu.ten} (số DV=${dichVuInNhom.length}) - Tổng nhóm: ${tongNhom.toLocaleString()}`);
//        console.log('  Danh sách dịch vụ (tên):', dichVuInNhom.map(d => d.ten));
//        for (const [phongId, gia] of Object.entries(tongPhongTrongNhom)) {
//            console.log(`    Phòng ${phongId}: ${gia.toLocaleString()}`);
//        }
//        // --- end debug ---

//        // Render header nhóm + tổng theo phòng
//        let headerRow = `<tr class="bg-light font-weight-bold">
//            <td colspan="2" class="text-start ps-2">
//                ${String(index + 1).padStart(2, '0')}. ${nhomDichVu.ten} (${dichVuInNhom.length})
//            </td>`;
//        phongList.forEach(phong => {
//            headerRow += `<td class="text-end pe-2">${tongPhongTrongNhom[phong.id] > 0 ? formatSoTien(tongPhongTrongNhom[phong.id]) : ''}</td>`;
//        });
//        headerRow += `<td class="text-end pe-2">${tongNhom > 0 ? formatSoTien(tongNhom) : ''}</td></tr>`;
//        tbody.append(headerRow);

//        // Hiển thị danh sách dịch vụ trong nhóm (phân trang)
//        const start = (currentPage - 1) * pageSize;
//        const end = start + pageSize;
//        const pageData = dichVuInNhom.slice(start, end);

//        pageData.forEach((dichVu, subIndex) => {
//            let rowHtml = `<tr>
//                <td class="text-center">${String(subIndex + 1).padStart(2, '0')}</td>
//                <td class="text-start ps-2">${dichVu.ten}</td>`;
//            let tongGia = 0;
//            phongList.forEach(phong => {
//                const gia = fullData
//                    .filter(item => item.idDichVuKyThuat === dichVu.id && item.idPhong === phong.id)
//                    .reduce((sum, item) => sum + (item.gia || 0), 0);
//                tongGia += gia;
//                rowHtml += `<td class="text-end pe-2">${gia > 0 ? formatSoTien(gia) : ''}</td>`;
//            });
//            rowHtml += `<td class="text-end pe-2 font-weight-bold">${tongGia > 0 ? formatSoTien(tongGia) : ''}</td></tr>`;
//            tbody.append(rowHtml);
//        });
//    });

//    // Hàng tổng cuối cùng chỉ hiển thị tổng cộng (không cộng từng phòng)
//    let totalRowHtml = `<tr class="bg-secondary text-white font-weight-bold">
//        <td colspan="${2 + phongList.length}" class="text-start ps-2">TỔNG CỘNG</td>
//        <td class="text-end pe-2">${tongTatCaGia > 0 ? formatSoTien(tongTatCaGia) : ''}</td>
//    </tr>`;
//    tbody.append(totalRowHtml);
//}





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
    const formattedTuNgay = formatDateForServer(tuNgay);  // yyyy-MM-dd
    const formattedDenNgay = formatDateForServer(denNgay); // yyyy-MM-dd
    const idNhomDichVu = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;
    const idDichVuKyThuat = 0;

    console.log('id nhom dich vu: ', idNhomDichVu);

    const url = `/bao_cao_thu_tong_hop_dv_theo_khoa_phong/export/pdf?` +
        `tuNgay=${formattedTuNgay}&denNgay=${formattedDenNgay}&idChiNhanh=${idChiNhanh}` +
        `&idNhomDichVu=${idNhomDichVu}&idDichVuKyThuat=${idDichVuKyThuat}`;

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
            a.download = `BaoCao_${formattedTuNgay}_${formattedDenNgay}.pdf`;
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

async function handleExportExcel() {
    const btnDesktop = document.getElementById("btnExportExcelGoiKham");
    const btnMobile = document.getElementById("btnExportExcelGoiKhamMobile");

    [btnDesktop, btnMobile].forEach(btn => {
        if (!btn) return;

        btn.addEventListener("click", async function () {
            try {
                if (!btn.dataset.originalHTML) btn.dataset.originalHTML = btn.innerHTML.trim();

                const isMobile = btn === btnMobile;
                const tuNgayRaw = isMobile ? document.getElementById("tuNgayMobile").value : document.getElementById("tuNgayDesktop").value;
                const denNgayRaw = isMobile ? document.getElementById("denNgayMobile").value : document.getElementById("denNgayDesktop").value;

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

                // Disable button và hiển thị spinner
                btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
                btn.disabled = true;

                // Chuyển ngày sang định dạng server
                const tuNgay = formatDateForServer(tuNgayRaw);
                const denNgay = formatDateForServer(denNgayRaw);
                const idcn = window._idcn || 0;

                // Lấy danh sách phòng có dữ liệu thực tế
                let phongCoDuLieu = [];
                fullData.forEach(item => {
                    if (item.idPhong && !phongCoDuLieu.includes(item.idPhong)) {
                        phongCoDuLieu.push(item.idPhong);
                    }
                });

                // Lấy phòng được chọn trên UI
                let idPhong = parseInt($("#selectedPhongId").val()) || 0;
                if (idPhong && !phongCoDuLieu.includes(idPhong)) {
                    idPhong = 0; // nếu chọn phòng không có dữ liệu => export tất cả
                }

                // Lấy danh sách nhóm dịch vụ & dịch vụ dựa trên JSON
                let idNhomDichVu = parseInt(document.getElementById("nhomDichVuIdHidden")?.value) || 0;
                let idDichVuKyThuat = parseInt(document.getElementById("dichVuKyThuatIdHidden")?.value) || 0;

                // Build URL export
                const url = `/bao_cao_thu_tong_hop_dv_theo_khoa_phong/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idcn}&idPhong=${idPhong}&idDichVuKyThuat=${idDichVuKyThuat}&idNhomDichVu=${idNhomDichVu}`;

                // Gọi API export
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

                // Tải file
                const blobUrl = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = blobUrl;
                a.download = `BaoCaoTongHopDichVuKhoaPhong_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.xlsx`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(blobUrl);

                toastr.success("Xuất Excel thành công!");
            } catch (error) {
                console.error(error);
                toastr.error("Lỗi khi xuất Excel: " + error.message);
            } finally {
                // Reset button
                btn.innerHTML = btn.dataset.originalHTML;
                btn.disabled = false;
            }
        });
    });
}


//function renderPagination() {
//    const pagination = $('#pagination');
//    pagination.empty();

//    const selectedNhomDichVuId = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;
//    let dichVuHienThi = [];

//    let filteredNhomDichVuList = listNhomDichVuKyThuat;
//    if (selectedNhomDichVuId !== 0) {
//        filteredNhomDichVuList = listNhomDichVuKyThuat.filter(nhom => nhom.id === selectedNhomDichVuId);
//    }

//    filteredNhomDichVuList.forEach(nhomDichVu => {
//        if (!nhomDichVu.active) return;
//        const dichVuInNhom = listDichVuKyThuat.filter(dv => dv.idNhomDichVu === nhomDichVu.id && dv.active);
//        dichVuHienThi = dichVuHienThi.concat(dichVuInNhom);
//    });

//    // Kiểm tra tổng giá
//    let tongTatCaGia = 0;
//    dichVuHienThi.forEach(dv => {
//        (window.filteredPhongList || []).forEach(phong => {
//            const data = fullData.find(item => item.idDichVuKyThuat === dv.id && item.idPhong === phong.id);
//            tongTatCaGia += data ? data.gia : 0;
//        });
//    });

//    const totalRecords = dichVuHienThi.length;
//    const pages = tongTatCaGia === 0 ? 1 : Math.max(1, Math.ceil(totalRecords / pageSize));

//    if (currentPage > pages) currentPage = pages;

//    const totalText = tongTatCaGia === 0 ? 'Tổng 0 dịch vụ' : `Tổng ${totalRecords} dịch vụ`;
//    $('#paginationContainer').text(`Trang ${currentPage}/${pages} – ${totalText}`);

//    // Nút trước
//    pagination.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
//        <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
//    </li>`);

//    const visibleCount = 3;
//    let startPage = Math.max(1, currentPage - 1);
//    let endPage = Math.min(pages, startPage + visibleCount - 1);

//    if (endPage - startPage + 1 < visibleCount) {
//        startPage = Math.max(1, endPage - visibleCount + 1);
//    }

//    for (let i = startPage; i <= endPage; i++) {
//        pagination.append(`<li class="page-item ${i === currentPage ? 'active' : ''}">
//            <a class="page-link" href="#" data-page="${i}">${i}</a>
//        </li>`);
//    }

//    // Nút sau
//    pagination.append(`<li class="page-item ${currentPage === pages ? 'disabled' : ''}">
//        <a class="page-link" href="#" data-page="${Math.min(pages, currentPage + 1)}">Sau</a>
//    </li>`);

//    pagination.find('a.page-link').on('click', function (e) {
//        e.preventDefault();
//        const page = parseInt($(this).data('page'));
//        if (!isNaN(page) && page !== currentPage) {
//            currentPage = page;
//            renderTable();
//            renderPagination();
//        }
//    });
//}

function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    const phongList = window.filteredPhongList || listPhongBuong;

    let selectedNhomDichVuId = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;

    // Lấy danh sách nhóm DVKT từ dữ liệu đã filter (fullData)
    let nhomIds = [...new Set(fullData.map(item => item.idNhomDichVu))];
    let filteredNhomDichVuList = listNhomDichVuKyThuat.filter(nhom => nhomIds.includes(nhom.id));

    if (selectedNhomDichVuId !== 0) {
        filteredNhomDichVuList = filteredNhomDichVuList.filter(nhom => nhom.id === selectedNhomDichVuId);
    }

    if (!fullData || fullData.length === 0) {
        const colCount = 2 + phongList.length + 1;
        tbody.append(`<tr><td colspan="${colCount}" class="text-center text-muted">Không có dữ liệu</td></tr>`);
        return;
    }

    // Gom tất cả dịch vụ theo nhóm
    let allDichVuHienThi = [];
    filteredNhomDichVuList.forEach(nhom => {
        const dichVuInNhom = fullData.filter(item => item.idNhomDichVu === nhom.id);
        allDichVuHienThi = allDichVuHienThi.concat(dichVuInNhom);
    });

    // Phân trang
    const totalRecords = allDichVuHienThi.length;
    const pages = Math.max(1, Math.ceil(totalRecords / pageSize));
    if (currentPage > pages) currentPage = pages;

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const pageData = allDichVuHienThi.slice(start, end);

    // Tổng toàn bảng
    let tongTatCaGia = 0;
    phongList.forEach(phong => {
        tongTatCaGia += fullData
            .filter(item => item.idPhong === phong.id)
            .reduce((sum, item) => sum + (item.giaTien || 0), 0);
    });

    // STT toàn cục
    let sttGlobal = start + 1;

    // Render theo nhóm
    filteredNhomDichVuList.forEach((nhomDichVu, index) => {
        const dichVuInNhom = pageData.filter(dv => dv.idNhomDichVu === nhomDichVu.id);
        if (dichVuInNhom.length === 0) return;

        // Tính tổng nhóm
        let tongNhom = 0;
        let tongPhongTrongNhom = {};
        phongList.forEach(phong => {
            const tongPhong = fullData
                .filter(item => item.idPhong === phong.id && item.idNhomDichVu === nhomDichVu.id)
                .reduce((sum, item) => sum + (item.giaTien || 0), 0);
            tongPhongTrongNhom[phong.id] = tongPhong;
            tongNhom += tongPhong;
        });

        // Hàng nhóm
        let headerRow = `<tr class="bg-light font-weight-bold">
            <td colspan="2" class="text-start ps-2">
                ${String(index + 1).padStart(2, '0')}. ${nhomDichVu.ten}
            </td>`;
        phongList.forEach(phong => {
            headerRow += `<td class="text-end pe-2">${tongPhongTrongNhom[phong.id] > 0 ? formatSoTien(tongPhongTrongNhom[phong.id]) : ''}</td>`;
        });
        headerRow += `<td class="text-end pe-2">${tongNhom > 0 ? formatSoTien(tongNhom) : ''}</td></tr>`;
        tbody.append(headerRow);

        // Hàng dịch vụ trong nhóm
        dichVuInNhom.forEach(dichVu => {
            let rowHtml = `<tr>
                <td class="text-center">${String(sttGlobal).padStart(2, '0')}</td>
                <td class="text-start ps-2">${dichVu.tenDichVuKyThuat}</td>`;
            let tongGia = 0;
            phongList.forEach(phong => {
                const gia = fullData
                    .filter(item => item.idDichVuKyThuat === dichVu.idDichVuKyThuat && item.idPhong === phong.id)
                    .reduce((sum, item) => sum + (item.giaTien || 0), 0);
                tongGia += gia;
                rowHtml += `<td class="text-end pe-2">${gia > 0 ? formatSoTien(gia) : ''}</td>`;
            });
            rowHtml += `<td class="text-end pe-2 font-weight-bold">${tongGia > 0 ? formatSoTien(tongGia) : ''}</td></tr>`;
            tbody.append(rowHtml);
            sttGlobal++;
        });
    });

    // Tổng cuối bảng
    let totalRowHtml = `<tr style="background-color:#f2f2f2; color:black; font-weight:bold;">
        <td colspan="${2 + phongList.length}" class="text-start ps-2">TỔNG CỘNG</td>
        <td class="text-end pe-2">${tongTatCaGia > 0 ? formatSoTien(tongTatCaGia) : ''}</td>
    </tr>`;
    tbody.append(totalRowHtml);
}


function renderPagination() {
    const pagination = $('#pagination');
    pagination.empty();

    const phongList = window.filteredPhongList || listPhongBuong;
    let selectedNhomDichVuId = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;

    // Lấy danh sách nhóm DVKT từ fullData
    let nhomIds = [...new Set(fullData.map(item => item.idNhomDichVu))];
    let filteredNhomDichVuList = listNhomDichVuKyThuat.filter(nhom => nhomIds.includes(nhom.id));

    if (selectedNhomDichVuId !== 0) {
        filteredNhomDichVuList = filteredNhomDichVuList.filter(nhom => nhom.id === selectedNhomDichVuId);
    }

    // Gom tất cả dịch vụ theo nhóm
    let dichVuHienThi = [];
    filteredNhomDichVuList.forEach(nhom => {
        const dichVuInNhom = fullData.filter(item => item.idNhomDichVu === nhom.id);
        dichVuHienThi = dichVuHienThi.concat(dichVuInNhom);
    });

    const totalRecords = dichVuHienThi.length;
    const pages = Math.max(1, Math.ceil(totalRecords / pageSize));
    if (currentPage > pages) currentPage = pages;

    $('#paginationContainer').text(`Trang ${currentPage}/${pages} – Tổng ${totalRecords} dịch vụ`);

    // Nút trước
    pagination.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
    </li>`);

    const visibleCount = 3;
    let startPage = Math.max(1, currentPage - 1);
    let endPage = Math.min(pages, startPage + visibleCount - 1);
    if (endPage - startPage + 1 < visibleCount) {
        startPage = Math.max(1, endPage - visibleCount + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        pagination.append(`<li class="page-item ${i === currentPage ? 'active' : ''}">
            <a class="page-link" href="#" data-page="${i}">${i}</a>
        </li>`);
    }

    // Nút sau
    pagination.append(`<li class="page-item ${currentPage === pages ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${Math.min(pages, currentPage + 1)}">Sau</a>
    </li>`);

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
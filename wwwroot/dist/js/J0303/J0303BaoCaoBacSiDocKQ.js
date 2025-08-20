let listPhong = [];  // Phòng buồng
let listKhoa = [];   // Khoa
let fullData = [];
let currentPage = 1;
let pageSize = 20;
let flattenedData = [];
let phongIndex = 1; // 🔢 Đánh số thứ tự cho nhóm Phòng

function initSearchDropdown({ inputId, dropdownId, hiddenFieldId, data, onSelect }) {
    const $input = $(`#${inputId}`);
    const $dropdown = $(`#${dropdownId}`);
    let currentIndex = -1;
    let currentData = data;
    function renderDropdown(filter = "", overrideData = null) {
        if (overrideData) currentData = overrideData;
        const lower = filter.toLowerCase();
        const list = currentData.filter(item =>
            item.ten.toLowerCase().includes(lower) ||
            (item.alias && item.alias.toLowerCase().includes(lower))
        );

        $dropdown.empty();
        currentIndex = -1;

        if (list.length === 0) {
            $dropdown.append(`<div class="list-group-item text-muted">Không tìm thấy</div>`);
        } else {
            list.forEach(item => {
                let tenHienThi = item.ten;
                if (filter.trim() !== "") {
                    const regex = new RegExp(`(${filter})`, "gi");
                    tenHienThi = tenHienThi.replace(regex, "<mark>$1</mark>");
                }
                $dropdown.append(
                    `<div class="list-group-item list-group-item-action d-flex justify-content-between align-items-center p-3" 
                    data-id="${item.id}" 
                    data-ten="${item.ten}">
                    <div class="tenHienThi truncate-text" title="${item.ten}">
                        ${tenHienThi}
                    </div>
                    ${item.alias ? `<div class="px-1 text-muted">[${item.alias}]</div>` : ""}
                </div>`
                );
            });

            // ✅ Tự động chọn dòng đầu tiên
            currentIndex = 0;
            const firstItem = $dropdown.find(".list-group-item").eq(0);
            firstItem.addClass("active");
        }

        $dropdown.show();
    }


    $input.on("input focus", function (e) {
        const val = $(this).val();
        if (e.type === "focus") $(this).select();
        renderDropdown(val);
    });

    $dropdown.on("click", ".list-group-item", function () {
        const ten = $(this).data("ten");
        const id = $(this).data("id");
        $input.val(ten);
        $(`#${hiddenFieldId}`).val(id);
        $dropdown.hide();

        if (onSelect) onSelect({ id, ten });
    });

    $input.on("keydown", function (e) {
        const items = $dropdown.find(".list-group-item");
        if (!items.length) return;

        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
            e.preventDefault();
            currentIndex = (e.key === "ArrowDown")
                ? (currentIndex + 1) % items.length
                : (currentIndex - 1 + items.length) % items.length;

            items.removeClass("active").eq(currentIndex).addClass("active");
            items.eq(currentIndex)[0].scrollIntoView({ behavior: "smooth", block: "nearest" });

        } else if (e.key === "Enter") {
            e.preventDefault();
            if (currentIndex >= 0) {
                const selected = items.eq(currentIndex);
                const ten = selected.data("ten");
                const id = selected.data("id");

                selectedId = id;
                $input.val(ten);
                $(`#${hiddenFieldId}`).val(id);
                $dropdown.hide();

                if (onSelect) onSelect({ id, ten });
            }
        }
    });

    $(document).on("click", function (e) {
        if (!$(e.target).closest(`#${inputId}, #${dropdownId}`).length) {
            $dropdown.hide();
        }
    });

    return { renderDropdown };
}

function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    if (!fullData || fullData.length === 0) {
        tbody.append(`<tr><td colspan="7" class="text-center text-muted">Không có dữ liệu phù hợp.</td></tr>`);
        return;
    }

    // Gom nhóm theo Khoa
    const khoaGroups = {};
    fullData.forEach(item => {
        if (!khoaGroups[item.idKhoa]) {
            khoaGroups[item.idKhoa] = {
                tenKhoa: item.tenKhoa,
                tong: { thuPhi: 0, bhyt: 0, no: 0, mienGiam: 0 },
                phongGroups: {}
            };
        }
        const khoa = khoaGroups[item.idKhoa];

        if (!khoa.phongGroups[item.idPhong]) {
            khoa.phongGroups[item.idPhong] = {
                tenPhong: item.tenPhong,
                tong: { thuPhi: 0, bhyt: 0, no: 0, mienGiam: 0 },
                list: []
            };
        }
        const phong = khoa.phongGroups[item.idPhong];

        // Push bác sĩ vào list
        phong.list.push(item);

        // Cộng dồn cho phòng
        phong.tong.thuPhi += item.thuPhi || 0;
        phong.tong.bhyt += item.bhyt || 0;
        phong.tong.no += item.no || 0;
        phong.tong.mienGiam += item.mienGiam || 0;

        // Cộng dồn cho khoa
        khoa.tong.thuPhi += item.thuPhi || 0;
        khoa.tong.bhyt += item.bhyt || 0;
        khoa.tong.no += item.no || 0;
        khoa.tong.mienGiam += item.mienGiam || 0;
    });

    let stt = 1;
    let khoaIndex = 1;

    // Render bảng
    Object.values(khoaGroups).forEach(khoa => {
        const tongKhoa = khoa.tong.thuPhi + khoa.tong.bhyt + khoa.tong.no + khoa.tong.mienGiam;

        // 👉 Level 1: Khoa (vàng)
        tbody.append(`
            <tr class="fw-bold"">
                <td colspan="6" class="text-start">${khoaIndex}. ${khoa.tenKhoa}</td>
                <td>${tongKhoa}</td>
            </tr>
        `);
        khoaIndex++;

        // Level 2: Phòng trong khoa (cam)
        Object.values(khoa.phongGroups).forEach(phong => {
            const tongPhong = phong.tong.thuPhi + phong.tong.bhyt + phong.tong.no + phong.tong.mienGiam;

            tbody.append(`
                <tr class="fw-bold">
                    <td colspan="6" class="ps-4 text-start">${phong.tenPhong}</td>
                    <td>${tongPhong}</td>
                </tr>
            `);

            // Level 3: Bác sĩ chi tiết (trắng)
            phong.list.forEach(item => {
                const tongBacSi = (item.thuPhi || 0) + (item.bhyt || 0) + (item.no || 0) + (item.mienGiam || 0);

                const row = `
                    <tr>
                        <td>${stt++}</td>
                        <td class="text-start">${item.bacSiChiDinh || ''}</td>
                        <td>${item.thuPhi || 0}</td>
                        <td>${item.bhyt || 0}</td>
                        <td>${item.no || 0}</td>
                        <td>${item.mienGiam || 0}</td>
                        <td>${tongBacSi}</td>
                    </tr>
                `;
                tbody.append(row);
            });
        });
    });
}

function handleFilter() {
    $('.btnFilterBacSi').off('click').on('click', function (e) {
        e.preventDefault();
        console.log("👉 Nút Lọc đã click!");

        setTimeout(function () {
            try {
                console.log("⏱️ Bắt đầu xử lý filter...");

                const idChiNhanh = window._idcn || 0;

                // Lấy ngày từ input Desktop hoặc Mobile
                const tuNgayRaw = $('#tuNgayDesktop').val() || $('#tuNgayMobile').val();
                const denNgayRaw = $('#denNgayDesktop').val() || $('#denNgayMobile').val();

                if (!tuNgayRaw || !denNgayRaw) {
                    toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày");
                    return;
                }

                // Parse và kiểm tra thứ tự ngày
                const tuNgayDate = new Date(tuNgayRaw.split('-').reverse().join('-'));
                const denNgayDate = new Date(denNgayRaw.split('-').reverse().join('-'));

                if (tuNgayDate > denNgayDate) {
                    $('#tuNgayDesktop').val(denNgayRaw);
                    $('#tuNgayDesktop').datepicker('update', denNgayRaw);
                    $('#tuNgayMobile').val(denNgayRaw);
                    $('#tuNgayMobile').datepicker('update', denNgayRaw);
                }

                // Format date cho server yyyy-MM-dd
                const tuNgay = formatDateForServer($('#tuNgayDesktop').val() || $('#tuNgayMobile').val());
                const denNgay = formatDateForServer($('#denNgayDesktop').val() || $('#denNgayMobile').val());

                // Lấy idKhoa & idPhong
                let idKhoa = parseInt($("#selectedKhoaId").val());
                let idPhong = parseInt($("#selectedPhongId").val());

                // Nếu không chọn → gửi 0 để backend trả tất cả
                if (isNaN(idKhoa)) idKhoa = 0;
                if (isNaN(idPhong)) idPhong = 0;

                console.log("🔎 Params gửi lên API:", { tuNgay, denNgay, idChiNhanh, idKhoa, idPhong });

                $.ajax({
                    url: '/bao_cao_bac_si_doc_kq/tk/FilterByDay',
                    type: 'POST',
                    data: { tuNgay, denNgay, idChiNhanh, idKhoa, idPhong },
                    success: function (response) {
                        if (response.success) {
                            fullData = response.data || [];

                            // Gán lại tên Phòng và Khoa từ danh sách đã load
                            fullData.forEach(item => {
                                const phong = listPhong.find(p => p.id === item.idPhong);
                                const khoa = listKhoa.find(k => k.id === item.idKhoa);

                                item.tenPhong = phong?.ten || "Không rõ phòng";
                                item.tenKhoa = khoa?.ten || "Không rõ khoa";
                            });

                            console.log("📊 Dữ liệu fullData:", fullData);

                            renderTable(); // render lại bảng theo dữ liệu lọc
                            toastr.success("Lọc dữ liệu thành công!");
                        } else {
                            toastr.error("Lỗi: " + (response.error || "Lỗi khi lọc dữ liệu"));
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

    let parts = dateStr.includes('/') ? dateStr.split('/') : dateStr.split('-');
    if (parts.length !== 3) return null;

    let day, month, year;

    if (dateStr.includes('/')) {
        // dd/mm/yyyy
        [day, month, year] = parts;
    } else {
        // dd-mm-yyyy
        [day, month, year] = parts;
    }

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

function handleExportPDF() {
    $(".btnExportPDFMobile").off("click").on("click", function () {
        exportPDFHandler(this, "Mobile");
    });

    $(".btnExportPDFDesktop").off("click").on("click", function () {
        exportPDFHandler(this, "Desktop");
    });
}

async function handleExportExcel() {
    const btn = document.getElementById("btnExportExcelGoiKham");

    if (!btn) {
        console.error("❌ Không tìm thấy nút #btnExportExcelGoiKham");
        return;
    }

    btn.addEventListener("click", async function () {
        if (!btn.dataset.originalHTML) {
            btn.dataset.originalHTML = btn.innerHTML.trim();
        }

        // Log check input DOM
        const tuNgayDesktopEl = document.getElementById("tuNgayDesktop");
        const tuNgayMobileEl = document.getElementById("tuNgayMobile");
        const denNgayDesktopEl = document.getElementById("denNgayDesktop");
        const denNgayMobileEl = document.getElementById("denNgayMobile");
        const selectKhoaEl = document.getElementById("selectedKhoaId");
        const selectPhongEl = document.getElementById("selectedPhongId");


        const tuNgayRaw = (tuNgayDesktopEl && tuNgayDesktopEl.value) || (tuNgayMobileEl && tuNgayMobileEl.value);
        const denNgayRaw = (denNgayDesktopEl && denNgayDesktopEl.value) || (denNgayMobileEl && denNgayMobileEl.value);
        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);
        const idChiNhanh = window._idcn;

        if (!tuNgayRaw || !denNgayRaw) {
            toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất Excel.");
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
            const checkUrl = `/bao_cao_bac_si_doc_kq/check-data?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}`;
            console.log("Fetch checkUrl:", checkUrl);

            const checkResponse = await fetch(checkUrl);
            if (!checkResponse.ok) {
                const errorText = await checkResponse.text();
                throw new Error(errorText);
            }

            const checkData = await checkResponse.json();
            console.log("Response check-data:", checkData);

            const { hasData } = checkData;
        

            if (!hasData) {
                toastr.error("Lỗi khi xuất Excel: Không có dữ liệu để xuất Excel");
                btn.innerHTML = btn.dataset.originalHTML;
                btn.disabled = false;
                return;
            }

            const idKhoa = (selectKhoaEl && selectKhoaEl.value) || 0;
            const idPhong = (selectPhongEl && selectPhongEl.value) || 0;

         

            const exportUrl = `/bao_cao_bac_si_doc_kq/export/excel?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}&idKhoa=${idKhoa}&idPhong=${idPhong}`;
            console.log("Fetch exportUrl:", exportUrl);

            const exportResponse = await fetch(exportUrl);
            if (!exportResponse.ok) {
                const errorText = await exportResponse.text();
                throw new Error("Không thể tải file Excel: " + errorText);
            }

            const blob = await exportResponse.blob();
            if (blob.size < 1000) {
                toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
                return;
            }

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "BaoCaoBacSiDocKQ.xlsx";
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


function exportPDFHandler(btn, viewType) {
    if (!btn.dataset.originalHTML) {
        btn.dataset.originalHTML = btn.innerHTML.trim();
    }

    const tuNgay = document.getElementById(viewType === "Mobile" ? "tuNgayMobile" : "tuNgayDesktop").value;
    const denNgay = document.getElementById(viewType === "Mobile" ? "denNgayMobile" : "denNgayDesktop").value;

    // Lấy giá trị Khoa/Phòng từ hidden input
    const idKhoa = document.getElementById("selectedKhoaId").value || 0;
    const idPhong = document.getElementById("selectedPhongId").value || 0;

    if (!tuNgay || !denNgay) {
        toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất PDF.");
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

    let url = "/bao_cao_bac_si_doc_kq/export/pdf?";
    if (formattedTuNgay) url += `tuNgay=${formattedTuNgay}&`;
    if (formattedDenNgay) url += `denNgay=${formattedDenNgay}&`;
    if (idChiNhanh) url += `idChiNhanh=${idChiNhanh}&`;
    if (idKhoa) url += `idKhoa=${idKhoa}&`;
    if (idPhong) url += `idPhong=${idPhong}&`;
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
            const blobUrl = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = blobUrl;
            a.download = `BaoCaoBacSiDocKQ_${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '')}.pdf`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(blobUrl);
            toastr.success("Xuất PDF thành công!");
            if (blob.size < 1000) toastr.warning("Không có dữ liệu trong khoảng thời gian đã chọn.");
        })
        .catch(error => {
            toastr.error("Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        });
}


$(document).ready(function () {
    // Load Khoa
    $.getJSON("/dist/data/json/DM_Khoa.json", function (data) {
        listKhoa = data.map(n => {
            let alias = n.viettat && n.viettat.trim() !== ""
                ? n.viettat.toUpperCase()
                : n.ten.trim().split(/\s+/).map(word => word.charAt(0).toUpperCase()).join("");
            return { ...n, alias };
        });

        // ✅ Log danh sách Khoa
        console.log("📌 Danh sách Khoa:", listKhoa);
        $.getJSON("/dist/data/json/DM_PhongBuong.json", function (dataPB) {
            listPhong = dataPB.map(n => {
                let alias = n.viettat && n.viettat.trim() !== ""
                    ? n.viettat.toUpperCase()
                    : n.ten.trim().split(/\s+/).map(word => word.charAt(0).toUpperCase()).join("");
                return { ...n, alias };
            });

            // ✅ Log danh sách Phòng/Buồng
            console.log("📌 Danh sách Phòng/Buồng:", listPhong);

            // Dropdown
            const khoaDropdown = initSearchDropdown({
                inputId: "searchKhoa",
                dropdownId: "dropdownKhoa",
                hiddenFieldId: "selectedKhoaId",
                data: listKhoa,
                onSelect: ({ id }) => {
                    const currentPhongId = $("#selectedPhongId").val();
                    const currentPhong = listPhong.find(p => p.id === currentPhongId);

                    if (!currentPhong || currentPhong.idKhoa !== id) {
                        $("#searchPhong").val("");
                        $("#selectedPhongId").val("");
                    }
                    phongDropdown.renderDropdown("", listPhong.filter(p => p.idKhoa === id));
                }
            });

            const phongDropdown = initSearchDropdown({
                inputId: "searchPhong",
                dropdownId: "dropdownPhong",
                hiddenFieldId: "selectedPhongId",
                data: listPhong,
                onSelect: ({ id }) => {
                    const phong = listPhong.find(p => p.id === id);
                    if (phong) {
                        const khoa = listKhoa.find(k => k.id === phong.idKhoa);
                        if (khoa) {
                            $("#searchKhoa").val(khoa.ten);
                            $("#selectedKhoaId").val(khoa.id);
                        }
                    }
                }
            });

            renderTable();
            handleFilter();
            handleExportPDF();
            handleExportExcel();
        });
    });
});

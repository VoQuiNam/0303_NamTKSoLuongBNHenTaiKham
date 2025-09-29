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




function renderHeader() {
    const thead = document.querySelector('table thead');
    thead.innerHTML = '';

    let row = `<tr>`;
    row += `<th class="text-center col-stt">STT</th>`;
    row += `<th class="text-center col-dichvu">Dịch vụ</th>`;

    listPhongBuong.forEach(phong => {
        row += `<th class="text-center col-phong">${phong.ten}</th>`;
    });

    row += `<th class="text-center col-tong">Tổng</th>`;
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
    loadDichVu();
    renderTable();
    initDatePicker();
    handleFilter();
    handleExportPDF();
    handleExportExcel();
});

document.addEventListener("DOMContentLoaded", loadDichVu);

let tomSelectNhomDichVu = null;

function initTomSelectNhomDichVu() {
    const selectElement = document.getElementById('nhomDichVuSelect');

    if (tomSelectNhomDichVu) {
        tomSelectNhomDichVu.destroy();
    }

    function generateAbbreviation(text) {
        if (!text || text === 'Tất cả') return '';

      
        const words = text.split(' ');
        let abbreviation = '';

        for (const word of words) {
            if (word.length > 0) {
                abbreviation += word[0].toUpperCase();
            }
        }

        return abbreviation;
    }

    tomSelectNhomDichVu = new TomSelect(selectElement, {
        valueField: 'id',
        labelField: 'ten',
        searchField: ['ten', 'viettat'],
        placeholder: 'Chọn nhóm dịch vụ',
        allowEmptyOption: true,
        create: false,
        maxOptions: null,
        render: {
            option: function (data, escape) {
  
                const vietTat = data.ten === 'Tất cả' ? '' : generateAbbreviation(data.ten);

                return `
                    <div class="d-flex justify-content-between">
                        <span>${escape(data.ten)}</span>
                        <span class="text-muted">${vietTat ? '[' + escape(vietTat) + ']' : ''}</span>
                    </div>
                `;
            },
            item: function (data, escape) {
                
                const vietTat = data.ten === 'Tất cả' ? '' : generateAbbreviation(data.ten);

                return `<div>${escape(data.ten)} ${vietTat ? '[' + escape(vietTat) + ']' : ''}</div>`;
            }
        },
        onInitialize: function () {
           
            this.setValue('0');
        },
        onChange: function (value) {
            const selectedItem = this.options[value];

            if (selectedItem) {
                const vietTat = selectedItem.ten === 'Tất cả' ? '' : generateAbbreviation(selectedItem.ten);

                document.getElementById('nhomDichVuIdHidden').value = selectedItem.id || 0;
                document.getElementById('nhomDichVuMaHidden').value = selectedItem.ma || '';
                document.getElementById('nhomDichVuTenHidden').value = selectedItem.ten || '';
                document.getElementById('nhomDichVuVietTatHidden').value = vietTat || '';
            } else {
             
                document.getElementById('nhomDichVuIdHidden').value = 0;
                document.getElementById('nhomDichVuMaHidden').value = '';
                document.getElementById('nhomDichVuTenHidden').value = 'Tất cả';
                document.getElementById('nhomDichVuVietTatHidden').value = '';
            }
        }
    });

  
    tomSelectNhomDichVu.on('type', function (str) {
    
    });
}


function updateTomSelectData(data) {
    if (tomSelectNhomDichVu) {
       
        const currentOptions = tomSelectNhomDichVu.options;
        Object.keys(currentOptions).forEach(key => {
            if (key !== '0') {
                tomSelectNhomDichVu.removeOption(key);
            }
        });

       
        tomSelectNhomDichVu.addOptions(data);

        tomSelectNhomDichVu.refreshOptions();
    }
}


async function loadDichVu() {
    try {
        const response = await fetch('/bao_cao_thu_tong_hop_dv_theo_khoa_phong/nhom-dich-vu/all');

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        dichVuData = await response.json();

        dichVuData.sort((a, b) => a.ten.localeCompare(b.ten, "vi", { sensitivity: "base" }));

        initTomSelectNhomDichVu();

        updateTomSelectData(dichVuData);

    } catch (err) {
        console.error("Lỗi load JSON:", err);
    }
}


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


function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}


function handleFilter() {
    $('.btnFilterBacSi').off('click').on('click', function (e) {
        e.preventDefault();

        showLoading();

        setTimeout(function () {
            try {
                const idChiNhanh = window._idcn || 0;

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
                    data: { tuNgay, denNgay, idChiNhanh, idDichVuKyThuat: idNhomdichvu, idPhong },
                    beforeSend: function () {
                       
                        showLoading();
                    },
                    success: function (response) {

                        if (response.success) {
                            console.log('id nhom dich vu: ', idNhomdichvu, idPhong);

                            fullData = response.data || [];
                            let filteredPhongIds = [];

                            fullData.forEach(item => {
                             
                                item.idDichVuKyThuat = item.idDichVuKyThuat || item.iddvkt;
                                item.idPhong = item.idPhong || item.idPhongBuong;

                               
                                const phong = listPhongBuong.find(p => p.id === item.idPhong);
                                
                                const dichVu = listDichVuKyThuat.find(d => d.id === item.idDichVuKyThuat);
                               
                                const nhomDichVu = dichVu ? listNhomDichVuKyThuat.find(n => n.id === dichVu.idNhomDichVu) : null;

                               
                                item.tenPhong = phong?.ten || "Không rõ phòng";
                                item.tenDichVuKyThuat = dichVu?.ten || "Không rõ dịch vụ";
                                item.tenNhomDvKyThuat = nhomDichVu?.ten || "Không rõ nhóm dịch vụ";
                                item.idNhomDichVu = dichVu?.idNhomDichVu || 0;

                                if (item.idPhong && !filteredPhongIds.includes(item.idPhong)) {
                                    filteredPhongIds.push(item.idPhong);
                                }
                            });

                          
                            window.filteredPhongList = listPhongBuong.filter(phong =>
                                filteredPhongIds.includes(phong.id)
                            );
                            
                            currentPage = 1;
                            pageSize = parseInt($('#pageSizeSelect').val()) || 10;
                            khoaStt = 1;
                            renderHeader();
                            renderTable();
                            renderPagination();

                            lastFilteredTuNgay = tuNgayRaw;
                            lastFilteredDenNgay = denNgayRaw;

                            toastr.success("Lọc dữ liệu thành công!");
                        } else {
                            toastr.error("Lỗi khi lọc dữ liệu");
                        }
                    },
                    error: function (xhr) {
                        console.error("❌ Lỗi kết nối:", xhr);
                        toastr.error("❌ Lỗi kết nối: " + xhr.responseText);
                    },
                    complete: function () {
                       
                        hideLoading();
                    }
                });
            } catch (err) {
                console.error("❌ Lỗi trong setTimeout:", err);
                hideLoading();
            }
        }, 100);
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



    const url = `/bao_cao_thu_tong_hop_dv_theo_khoa_phong/export/pdf?` +
        `tuNgay=${formattedTuNgay}&denNgay=${formattedDenNgay}&idChiNhanh=${idChiNhanh}` +
        `&idNhomDichVu=${idNhomDichVu}&idDichVuKyThuat=${idDichVuKyThuat}`;

    console.log(url);



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
            a.download = `BaoCaoTongHopDichVuKhoaPhong.pdf`;
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
                a.download = `BaoCaoTongHopDichVuKhoaPhong.xlsx`;
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




function renderTable() {
    const tbody = $('#tableBody');
    const tfoot = document.querySelector(".table-wrapper-scroll tfoot");
    tbody.empty();
    tfoot.innerHTML = '';

    showLoading();

    setTimeout(() => {
        try {
            const phongList = listPhongBuong;
            let selectedNhomDichVuId = parseInt(document.getElementById("nhomDichVuIdHidden").value) || 0;

            if (!fullData || fullData.length === 0) {
                // Tính tổng cột dựa trên header: STT + Dịch vụ + phòng + Tổng cộng
                const colCount = 2 + (phongList ? phongList.length : 0) + 1;
                tbody.append(`<tr><td colspan="15" class="text-center text-muted">Không có dữ liệu</td></tr>`);
                hideLoading();
                return;
            }

            // Lọc nhóm theo dữ liệu
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

            // Phân trang dịch vụ theo pageSize
            const totalRecords = dichVuHienThi.length;
            const pages = Math.max(1, Math.ceil(totalRecords / pageSize));
            if (currentPage > pages) currentPage = pages;

            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            const pageData = dichVuHienThi.slice(start, end);

            let sttGlobal = start + 1;

            // Lấy các nhóm hiện tại trong pageData
            let nhomBlocks = filteredNhomDichVuList.map((nhom, idx) => {
                const items = pageData.filter(dv => dv.idNhomDichVu === nhom.id);
                return { nhom, items, sttNhom: idx + 1 }; // STT nhóm liên tục theo filteredNhomDichVuList
            }).filter(block => block.items.length > 0);

            // render từng nhóm
            nhomBlocks.forEach((block) => {
                const nhomDichVu = block.nhom;
                const dichVuInNhom = block.items;

                // Tính tổng nhóm theo tất cả dịch vụ của nhóm, không phụ thuộc trang
                let tongNhomTheoPhong = {};
                phongList.forEach(phong => {
                    tongNhomTheoPhong[phong.id] = fullData
                        .filter(item => item.idNhomDichVu === nhomDichVu.id && item.idPhong === phong.id)
                        .reduce((sum, item) => sum + (parseFloat(item.giaTien) || 0), 0);
                });

                let tongNhom = Object.values(tongNhomTheoPhong).reduce((a, b) => a + b, 0);

                // header nhóm sử dụng sttNhom
                let headerRow = `<tr style="background-color:#f8f9fa; font-weight:bold;">
  <td colspan="2" class="text-start ps-2">${block.sttNhom}. ${nhomDichVu.ten}</td>`;
                phongList.forEach(phong => {
                    headerRow += `<td class="text-end pe-2" style="font-weight:bold;">${tongNhomTheoPhong[phong.id] > 0 ? formatSoTien(tongNhomTheoPhong[phong.id]) : ''}</td>`;
                });
                headerRow += `<td class="text-end pe-2" style="font-weight:bold;">${formatSoTien(tongNhom)}</td></tr>`;
                tbody.append(headerRow);



                // render chi tiết dịch vụ
                dichVuInNhom.forEach(item => {
                    let rowHtml = `<tr>
                        <td class="text-center">${sttGlobal}</td>
                        <td class="text-start ps-2">${item.tenDichVuKyThuat}</td>`;
                    phongList.forEach(phong => {
                        if (item.idPhong === phong.id) {
                            rowHtml += `<td class="text-end pe-2">${formatSoTien(item.giaTien)}</td>`;
                        } else {
                            rowHtml += `<td></td>`;
                        }
                    });
                    rowHtml += `<td class="text-end pe-2 font-weight-bold">${formatSoTien(item.giaTien)}</td></tr>`;
                    tbody.append(rowHtml);
                    sttGlobal++;
                });
            });

            // tổng cuối bảng
            let tongTatCaGia = fullData.reduce((sum, item) => sum + (parseFloat(item.giaTien) || 0), 0);
            let totalRowHtml = `<tr style="background-color:#f2f2f2; color:black; font-weight:bold;">
                <td colspan="${2 + phongList.length}" class="text-start ps-2">TỔNG CỘNG</td>
                <td class="text-end pe-2">${formatSoTien(tongTatCaGia)}</td>
            </tr>`;
            tfoot.innerHTML = totalRowHtml;

        } catch (error) {
            console.error('Lỗi khi render table:', error);
            const phongList = window.filteredPhongList || listPhongBuong;
            const colCount = 2 + phongList.length + 1;
            tbody.append(`<tr><td colspan="${colCount}" class="text-center text-danger">Đã xảy ra lỗi khi tải dữ liệu</td></tr>`);
        } finally {
            hideLoading();
        }
    }, 100);
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
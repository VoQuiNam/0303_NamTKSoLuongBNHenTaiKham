let listPhong = [];
let listKhoa = [];
let fullData = [];
let currentPage = 1;
let pageSize = 20;
let phongIndex = 1;
let khoaSttGlobal = 1;
let lastFilteredTuNgay = null;
let lastFilteredDenNgay = null;
let tomSelectKhoa = null;
let tomSelectPhong = null;

function initTomSelect({ selectId, placeholder, data, onSelect, onClear }) {
    const selectElement = document.getElementById(selectId);

    if (!selectElement) {
        console.error(`Không tìm thấy element với id: ${selectId}`);
        return null;
    }

    const firstOption = selectElement.querySelector('option');
    selectElement.innerHTML = firstOption ? firstOption.outerHTML : '';

    data.forEach(item => {
        const option = document.createElement('option');
        option.value = item.id;
        option.textContent = item.ten;
        option.setAttribute('data-alias', item.alias || '');
        selectElement.appendChild(option);
    });

    const tomSelect = new TomSelect(`#${selectId}`, {
        plugins: [],
        valueField: 'value',
        labelField: 'text',
        searchField: ['text', 'alias'],
        create: false,
        maxItems: 1,
        placeholder: placeholder,
        allowEmptyOption: false,
        closeAfterSelect: true,
        loadThrottle: null,
        loadingClass: null,
        preload: true,
        load: function (query, callback) {
            const filteredData = data.filter(item => {
                const searchText = query.toLowerCase();
                return item.ten.toLowerCase().includes(searchText) ||
                    (item.alias && item.alias.toLowerCase().includes(searchText));
            }).map(item => ({
                value: item.id,
                text: item.ten,
                alias: item.alias || ''
            }));

            callback(filteredData);
        },
        render: {
            option: function (item, escape) {
                let html = `<div class="d-flex justify-content-between align-items-center">`;
                let displayText = escape(item.text);
                let aliasText = item.alias ? escape(item.alias) : '';

                const searchQuery = this.inputState.query || '';
                if (searchQuery.trim() !== '') {
                    const regex = new RegExp(`(${escape(searchQuery)})`, 'gi');
                    displayText = displayText.replace(regex, '<mark class="highlight">$1</mark>');
                    if (aliasText) {
                        aliasText = aliasText.replace(regex, '<mark class="highlight">$1</mark>');
                    }
                }

                html += `<div class="tenHienThi">${displayText}</div>`;
                if (item.alias) {
                    html += `<div class="px-1 text-muted">[${aliasText}]</div>`;
                }
                html += `</div>`;
                return html;
            },
            item: function (item, escape) {
                return `<div>${escape(item.text)}</div>`;
            },
            loading: function () {
                return '';
            },
            no_results: function () {
                return '<div class="no-results">Không tìm thấy kết quả</div>';
            }
        },
        onInitialize: function () {

            this.setValue('0', true);


            if (selectId === 'khoaSelect') {
                $('#khoaIdHidden').val('0');
                $('#khoaMaHidden').val('');
                $('#khoaTenHidden').val('Tất cả');
                $('#khoaVietTatHidden').val('');
            } else if (selectId === 'phongSelect') {
                $('#phongIdHidden').val('0');
                $('#phongMaHidden').val('');
                $('#phongTenHidden').val('Tất cả');
                $('#phongVietTatHidden').val('');
            }

            if (onClear) onClear();
        },
        onBlur: function () {
            if (!this.getValue() || this.getValue() === '') {
                this.setValue('0', true);
            }
        }
    });

    tomSelect.on('change', function (value) {
        if (value && value !== '') {
            const selectedItem = data.find(item => item.id.toString() === value.toString());
            if (selectedItem && onSelect) {
                onSelect({ id: selectedItem.id, ten: selectedItem.ten });
            }
        } else {
            if (selectId === 'khoaSelect') {
                $('#khoaIdHidden').val('0');
                $('#khoaMaHidden').val('');
                $('#khoaTenHidden').val('Tất cả');
                $('#khoaVietTatHidden').val('');
            } else if (selectId === 'phongSelect') {
                $('#phongIdHidden').val('0');
                $('#phongMaHidden').val('');
                $('#phongTenHidden').val('Tất cả');
                $('#phongVietTatHidden').val('');
            }

            if (onClear) onClear();
        }
    });

    return tomSelect;
}


function flattenData(khoaGroups) {
    const flatList = [];
    Object.values(khoaGroups).forEach(khoa => {
        Object.values(khoa.phongGroups).forEach(phong => {
            phong.list.forEach(item => {
                flatList.push({ khoa, phong, item });
            });
        });
    });
    return flatList;
}

function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'block';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}


function renderTable() {
    const tbody = $('#tableBody');
    tbody.empty();

    if (!fullData || fullData.length === 0) {
        tbody.append(`<tr><td colspan="7" class="text-center text-muted">Không có dữ liệu phù hợp.</td></tr>`);
        return;
    }

    showLoading();

    setTimeout(() => {
        try {
            const khoaGroups = {};
            fullData.forEach(item => {
                if (!khoaGroups[item.idKhoa]) {
                    khoaGroups[item.idKhoa] = {
                        tenKhoa: item.tenKhoa,
                        tongSoCa: 0,
                        phongGroups: {}
                    };
                }
                const khoa = khoaGroups[item.idKhoa];

                if (!khoa.phongGroups[item.idPhong]) {
                    khoa.phongGroups[item.idPhong] = {
                        tenPhong: item.tenPhong,
                        tongSoCa: 0,
                        list: []
                    };
                }
                const phong = khoa.phongGroups[item.idPhong];
                phong.list.push(item);
            });

            Object.values(khoaGroups).forEach(khoa => {
                Object.values(khoa.phongGroups).forEach(phong => {
                    phong.tongSoCa = phong.list.reduce((sum, item) => {
                        return sum + ((item.thuPhi || 0) + (item.bhyt || 0) + (item.no || 0) + (item.mienGiam || 0));
                    }, 0);
                });

                khoa.tongSoCa = Object.values(khoa.phongGroups)
                    .reduce((sum, phong) => sum + phong.tongSoCa, 0);
            });

            const flatList = [];
            Object.values(khoaGroups).forEach(khoa => {
                Object.values(khoa.phongGroups).forEach(phong => {
                    phong.list.forEach(item => {
                        flatList.push({ khoa, phong, item });
                    });
                });
            });

            const startIndex = (currentPage - 1) * pageSize;
            const endIndex = Math.min(startIndex + pageSize, flatList.length);
            const pagedData = flatList.slice(startIndex, endIndex);

            let khoaSttStart = 1;
            if (currentPage > 1) {
                const allKhoaIds = Object.keys(khoaGroups);
                let itemsCount = 0;

                for (let i = 0; i < allKhoaIds.length; i++) {
                    const khoaId = allKhoaIds[i];
                    const khoa = khoaGroups[khoaId];
                    const khoaItemCount = Object.values(khoa.phongGroups).reduce((total, phong) => total + phong.list.length, 0);

                    itemsCount += khoaItemCount;
                    if (itemsCount >= startIndex) {
                        khoaSttStart = i + 1;
                        break;
                    }
                }
            }

            let currentKhoaStt = khoaSttStart;
            let lastKhoa = null;
            let lastPhong = null;
            let stt = startIndex + 1;

            pagedData.forEach(({ khoa, phong, item }) => {
                if (lastKhoa !== khoa) {
                    tbody.append(`
                        <tr class="fw-bold" style="background-color: #f8f9fa;">
                            <td colspan="6" style="text-align: left; padding-left: 8px; font-weight: bold;">
                                ${String(currentKhoaStt++).padStart(2)}. ${khoa.tenKhoa}
                            </td>
                            <td>${khoa.tongSoCa}</td>
                        </tr>
                    `);
                    lastKhoa = khoa;
                    lastPhong = null;
                }

                if (lastPhong !== phong) {
                    tbody.append(`
                        <tr class="fw-bold">
                            <td colspan="6" style="text-align: left; padding-left: 32px; font-weight: bold;">
                                ${phong.tenPhong}
                            </td>
                            <td>${phong.tongSoCa}</td>
                        </tr>
                    `);
                    lastPhong = phong;
                }

                tbody.append(`
                    <tr>
                        <td style="border-right: 1px solid #dee2e6; text-align: center; width: 40px;">
                            ${stt++}
                        </td>
                        <td style="text-align: left; padding-left: 16px;">
                            ${item.bacSiChiDinh || ''}
                        </td>
                        <td>${item.thuPhi || 0}</td>
                        <td>${item.bhyt || 0}</td>
                        <td>${item.no || 0}</td>
                        <td>${item.mienGiam || 0}</td>
                        <td>${(item.thuPhi || 0) + (item.bhyt || 0) + (item.no || 0) + (item.mienGiam || 0)}</td>
                    </tr>
                `);
            });
        } catch (error) {
            console.error('Lỗi khi render table:', error);
            tbody.append(`<tr><td colspan="7" class="text-center text-danger">Đã xảy ra lỗi khi tải dữ liệu</td></tr>`);
        } finally {
            hideLoading();
        }
    }, 100);
}



function updateTable(data) {
    fullData = data || [];
    currentPage = 1;
    pageSize = parseInt($('#pageSizeSelect').val()) || 20;
    khoaSttGlobal = 1;

    renderTable();
    renderPagination();
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
    khoaSttGlobal = 1;
    if (fullData && fullData.length > 0) {
        renderTable();
        renderPagination();
    } else {
        toastr.error("Vui lòng lọc dữ liệu trước khi thay đổi số dòng hiển thị.");
    }
});

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

                const idKhoa = $('#khoaIdHidden').val() || 0;
                const idPhong = $('#phongIdHidden').val() || 0;

             

                $.ajax({
                    url: '/bao_cao_bac_si_doc_kq/tk/FilterByDay',
                    type: 'POST',
                    data: {
                        tuNgay,
                        denNgay,
                        idChiNhanh,
                        idKhoa: idKhoa || 0,
                        idPhong: idPhong || 0
                    },
                    beforeSend: function () {
                       
                        showLoading();
                    },
                    success: function (response) {
                        if (response.success) {
                            fullData = response.data || [];

                           

                            fullData.forEach(item => {
                                const itemKhoaId = parseInt(item.idKhoa);
                                const itemPhongId = parseInt(item.idPhong);

                                const phong = listPhong.find(p => {
                                    const phongId = parseInt(p.id);
                                    return phongId === itemPhongId;
                                });

                                const khoa = listKhoa.find(k => {
                                    const khoaId = parseInt(k.id);
                                    return khoaId === itemKhoaId;
                                });

                                item.tenPhong = phong?.ten || "Không rõ phòng";
                                item.tenKhoa = khoa?.ten || "Không rõ khoa";

                              
                            });

                          

                            currentPage = 1;
                            pageSize = parseInt($('#pageSizeSelect').val()) || 10;
                            khoaSttGlobal = 1;
                            renderTable();
                            renderPagination();
                            lastFilteredTuNgay = tuNgayRaw;
                            lastFilteredDenNgay = denNgayRaw;
                            toastr.success("Lọc dữ liệu thành công!");
                        } else {
                            toastr.error("Lỗi: " + (response.error || "Lỗi khi lọc dữ liệu"));
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

        const tuNgayRaw = document.getElementById("tuNgayDesktop")?.value || document.getElementById("tuNgayMobile")?.value;
        const denNgayRaw = document.getElementById("denNgayDesktop")?.value || document.getElementById("denNgayMobile")?.value;
        const selectKhoaEl = document.getElementById("khoaIdHidden");
        const selectPhongEl = document.getElementById("phongIdHidden");
        const idChiNhanh = window._idcn;

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

        const tuNgay = formatDateForServer(tuNgayRaw);
        const denNgay = formatDateForServer(denNgayRaw);

        if (!validateDateRange(tuNgay, denNgay)) {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
            return;
        }

        const idKhoa = selectKhoaEl?.value || 0;
        const idPhong = selectPhongEl?.value || 0;

        btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
        btn.disabled = true;
        showLoading();

        try {
            const exportUrl = `/bao_cao_bac_si_doc_kq/check-and-export?tuNgay=${tuNgay}&denNgay=${denNgay}&idcn=${idChiNhanh}&idKhoa=${idKhoa}&idPhong=${idPhong}`;
           

            const exportResponse = await fetch(exportUrl);

            if (!exportResponse.ok) {
                const errorText = await exportResponse.text();
                try {
                    const errorData = JSON.parse(errorText);
                    if (errorData.message) {
                        throw new Error(errorData.message);
                    }
                } catch (e) {
                    throw new Error(errorText || "Lỗi không xác định");
                }
            }

            const contentType = exportResponse.headers.get('content-type');

            if (contentType && contentType.includes('application/json')) {
                const responseData = await exportResponse.json();
                if (!responseData.hasData) {
                    toastr.error(responseData.message || "Không có dữ liệu trong khoảng ngày đã chọn.");
                    return;
                }
            } else if (contentType && (contentType.includes('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') ||
                contentType.includes('application/octet-stream'))) {
                const blob = await exportResponse.blob();

                if (blob.size === 0) {
                    toastr.warning("File Excel trống, không có dữ liệu để xuất.");
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
            } else {
                throw new Error("Định dạng phản hồi không xác định từ server");
            }
        } catch (error) {
            toastr.error("Lỗi khi xuất Excel: " + error.message);
        } finally {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
            hideLoading();
        }
    });
}




function exportPDFHandler(btn, viewType) {
    if (!btn.dataset.originalHTML) {
        btn.dataset.originalHTML = btn.innerHTML.trim();
    }

    const tuNgay = document.getElementById(viewType === "Mobile" ? "tuNgayMobile" : "tuNgayDesktop")?.value;
    const denNgay = document.getElementById(viewType === "Mobile" ? "denNgayMobile" : "denNgayDesktop")?.value;

    const idKhoa = document.getElementById("khoaIdHidden")?.value || 0;
    const idPhong = document.getElementById("phongIdHidden")?.value || 0;

    if (!tuNgay || !denNgay) {
        toastr.error("Vui lòng chọn đầy đủ Từ ngày và Đến ngày trước khi xuất PDF.");
        btn.innerHTML = btn.dataset.originalHTML;
        btn.disabled = false;
        return;
    }

    if (!fullData || fullData.length === 0) {
        toastr.error("Vui lòng lọc dữ liệu trước khi xuất PDF.");
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

    let url = "/bao_cao_bac_si_doc_kq/export/pdf?";
    if (formattedTuNgay) url += `tuNgay=${formattedTuNgay}&`;
    if (formattedDenNgay) url += `denNgay=${formattedDenNgay}&`;
    if (idChiNhanh) url += `idChiNhanh=${idChiNhanh}&`;
    if (idKhoa && idKhoa !== '0') url += `idKhoa=${idKhoa}&`;
    if (idPhong && idPhong !== '0') url += `idPhong=${idPhong}&`;
    url = url.replace(/&$/, "");

   

    fetch(url, {
        method: "GET",
        headers: {
            'Accept': 'application/pdf',
            'Cache-Control': 'no-cache'
        }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || "Không thể tải file PDF");
                });
            }

            const contentType = response.headers.get('content-type');
            if (!contentType || !contentType.includes('application/pdf')) {
                throw new Error("Phản hồi không phải là file PDF");
            }

            return response.blob();
        })
        .then(blob => {
            if (blob.size === 0) {
                throw new Error("File PDF trống");
            }

            const blobUrl = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = blobUrl;
            a.download = `BaoCaoBacSiDocKQ.pdf`;
            document.body.appendChild(a);
            a.click();
            a.remove();

            setTimeout(() => {
                window.URL.revokeObjectURL(blobUrl);
            }, 100);

            toastr.success("Xuất PDF thành công!");

            if (blob.size < 5000) {
                toastr.warning("File PDF có kích thước nhỏ, có thể không có dữ liệu.");
            }
        })
        .catch(error => {
            console.error('PDF Export Error:', error);
            toastr.error("Lỗi khi xuất PDF: " + error.message);
        })
        .finally(() => {
            btn.innerHTML = btn.dataset.originalHTML;
            btn.disabled = false;
        });
}



document.addEventListener('DOMContentLoaded', function () {
    Promise.all([
        fetch('/dist/data/json/DM_Khoa.json').then(response => {
            if (!response.ok) throw new Error('Lỗi khi tải danh sách khoa');
            return response.json();
        }),
        fetch('/dist/data/json/DM_PhongBuong.json').then(response => {
            if (!response.ok) throw new Error('Lỗi khi tải danh sách phòng');
            return response.json();
        })
    ]).then(([khoaData, phongData]) => {
        listKhoa = khoaData.map(n => {
            let alias = n.viettat && n.viettat.trim() !== ""
                ? n.viettat.toUpperCase()
                : n.ten.trim().split(/\s+/).map(word => word.charAt(0).toUpperCase()).join("");
            return { ...n, alias };
        });

        listPhong = phongData.map(n => {
            let alias = n.viettat && n.viettat.trim() !== ""
                ? n.viettat.toUpperCase()
                : n.ten.trim().split(/\s+/).map(word => word.charAt(0).toUpperCase()).join("");
            return { ...n, alias };
        });

        tomSelectKhoa = initTomSelect({
            selectId: 'khoaSelect',
            placeholder: 'Chọn khoa',
            data: listKhoa,
            onSelect: ({ id, ten }) => {
               

                if (id === '0') {
                    resetKhoaAndPhongToAll();
                    return;
                }

                const selectedKhoa = listKhoa.find(k => k.id === id);
                if (selectedKhoa) {
                    $('#khoaIdHidden').val(selectedKhoa.id);
                    $('#khoaMaHidden').val(selectedKhoa.ma || '');
                    $('#khoaTenHidden').val(selectedKhoa.ten);
                    $('#khoaVietTatHidden').val(selectedKhoa.viettat || '');

                    if (tomSelectPhong) {
                        const phongTheoKhoa = listPhong.filter(p => p.idKhoa === selectedKhoa.id);

                        tomSelectPhong.clear();
                        tomSelectPhong.clearOptions();
                        tomSelectPhong.addOption({ value: '0', text: 'Tất cả', alias: '' });

                        phongTheoKhoa.forEach(p => {
                            tomSelectPhong.addOption({
                                value: String(p.id),
                                text: p.ten,
                                alias: p.alias || ''
                            });
                        });

                        tomSelectPhong.refreshOptions(false);
                        tomSelectPhong.setValue('0', true);
                    }
                }
            },
            onClear: () => {
                resetPhongToAll();
            },
            
        });

        tomSelectPhong = initTomSelect({
            selectId: 'phongSelect',
            placeholder: 'Chọn phòng',
            data: listPhong,
            onSelect: ({ id, ten }) => {
                const selectedPhongLocal = listPhong.find(p => p.id == id);
                if (selectedPhongLocal) {
                    $('#phongIdHidden').val(selectedPhongLocal.id);
                    $('#phongMaHidden').val(selectedPhongLocal.ma || '');
                    $('#phongTenHidden').val(selectedPhongLocal.ten);
                    $('#phongVietTatHidden').val(selectedPhongLocal.viettat || '');

                    
                    if (selectedPhongLocal.idKhoa && tomSelectKhoa) {
                        tomSelectKhoa.setValue(String(selectedPhongLocal.idKhoa), true);
                    }
                }
            },
            onClear: () => {
                $('#phongIdHidden').val('0');
                $('#phongMaHidden').val('');
                $('#phongTenHidden').val('Tất cả');
                $('#phongVietTatHidden').val('');
            }
        });

      
        function setupInputHandlers() {
           
            const khoaInput = document.querySelector('#khoaSelect ~ .ts-control input');
            const phongInput = document.querySelector('#phongSelect ~ .ts-control input');

            if (khoaInput) {
              
                khoaInput.addEventListener('input', function (e) {
                   

                 
                    if (!this.value || this.value.trim() === '') {
                       
                        setTimeout(() => {
                           
                            this.setValue('0', true);
                            resetKhoaAndPhongToAll();
                        }, 100);
                    }
                });

             
                khoaInput.addEventListener('blur', function (e) {
                    setTimeout(() => {
                        const currentValue = tomSelectKhoa.getValue();
                      

                        if (!currentValue || currentValue === '' || currentValue.length === 0) {
                           
                            resetKhoaAndPhongToAll();
                        }
                    }, 150);
                });
            }

            if (phongInput) {
              
                phongInput.addEventListener('input', function (e) {
                  

                   
                    if (!this.value || this.value.trim() === '') {
                      
                        setTimeout(() => {
                            resetPhongToAllOnly();
                        }, 100);
                    }
                });

               
                phongInput.addEventListener('blur', function (e) {
                    setTimeout(() => {
                        const currentValue = tomSelectPhong.getValue();
                       

                        if (!currentValue || currentValue === '' || currentValue.length === 0) {
                            resetPhongToAllOnly();
                        }
                    }, 150);
                });
            }
        }

      
        const khoaSelectElement = document.getElementById('khoaSelect');
        if (khoaSelectElement) {
            khoaSelectElement.addEventListener('click', function (e) {
               
                setTimeout(() => {
                    const dropdownItems = document.querySelectorAll('.ts-dropdown .option[data-value="0"]');
                    dropdownItems.forEach(item => {
                        item.addEventListener('click', function () {
                            resetKhoaAndPhongToAll();
                        });
                    });
                }, 100);
            });
        }

      
        function resetKhoaAndPhongToAll() {
           
            if (tomSelectKhoa) {
                
                tomSelectKhoa.clear();
                tomSelectKhoa.clearOptions();
                tomSelectKhoa.addOption({ value: '0', text: 'Tất cả', alias: '' });

              
                listKhoa.forEach(k => {
                    tomSelectKhoa.addOption({
                        value: String(k.id),
                        text: k.ten,
                        alias: k.alias || ''
                    });
                });

                tomSelectKhoa.refreshOptions(false);
                tomSelectKhoa.setValue('0', true);
            }


            $('#khoaIdHidden').val('0');
            $('#khoaMaHidden').val('');
            $('#khoaTenHidden').val('Tất cả');
            $('#khoaVietTatHidden').val('');

           
            resetPhongToAll();

         
        }

       
        function resetPhongToAll() {
           

            if (tomSelectPhong) {
                tomSelectPhong.clear();
                tomSelectPhong.clearOptions();
                tomSelectPhong.addOption({ value: '0', text: 'Tất cả', alias: '' });

                
                listPhong.forEach(p => {
                    tomSelectPhong.addOption({
                        value: String(p.id),
                        text: p.ten,
                        alias: p.alias || ''
                    });
                });

                tomSelectPhong.refreshOptions(false);
                tomSelectPhong.setValue('0', true);

                
                $('#phongIdHidden').val('0');
                $('#phongMaHidden').val('');
                $('#phongTenHidden').val('Tất cả');
                $('#phongVietTatHidden').val('');
            }
        }

       
        function resetPhongToAllOnly() {
          

            if (tomSelectPhong) {
                tomSelectPhong.clear();
                tomSelectPhong.clearOptions();
                tomSelectPhong.addOption({ value: '0', text: 'Tất cả', alias: '' });


                listPhong.forEach(p => {
                    tomSelectPhong.addOption({
                        value: String(p.id),
                        text: p.ten,
                        alias: p.alias || ''
                    });
                });

                tomSelectPhong.refreshOptions(false);
                tomSelectPhong.setValue('0', true);

                
                $('#phongIdHidden').val('0');
                $('#phongMaHidden').val('');
                $('#phongTenHidden').val('Tất cả');
                $('#phongVietTatHidden').val('');

               
            }
        }

        $('#khoaSelect').on('change', function () {
            const selectedValue = $(this).val();
           

            if (selectedValue == '0') {
                resetKhoaAndPhongToAll();
            }
        });

      
        setTimeout(() => {
            setupInputHandlers();
        }, 500);

       
        document.addEventListener('keydown', function (e) {
           
            const activeElement = document.activeElement;
            if (activeElement && activeElement.closest('.ts-control')) {
                const input = activeElement;

               
                if (e.key === 'Delete' || e.key === 'Backspace') {
                   
                    setTimeout(() => {
                        if (!input.value || input.value.trim() === '') {
                            const tsControl = input.closest('.ts-control');

                            if (tsControl.previousElementSibling &&
                                tsControl.previousElementSibling.id === 'khoaSelect') {
                               
                            } else if (tsControl.previousElementSibling &&
                                tsControl.previousElementSibling.id === 'phongSelect') {
                              
                              
                                resetPhongToAllOnly();
                            }
                        }
                    }, 50);
                }
            }
        });

    }).catch(error => {
        console.error('❌ Lỗi khi tải dữ liệu:', error);
        toastr.error('Không thể tải danh sách khoa/phòng: ' + error.message);
    });

    initDatePicker();
    renderTable();
    handleFilter();
    handleExportExcel();
    handleExportPDF();
});

(function ($) {
    $(document).ready(function () {
        $('.date-input').datepicker({ dateFormat: 'dd-mm-yy' });
        const parseDate = (dateStr) => {
            const [day, month, year] = dateStr.split('-').map(Number);
            return new Date(year, month - 1, day);
        };

        const formatDate = (date) => {
            const day = String(date.getDate()).padStart(2, '0');
            const month = String(date.getMonth() + 1).padStart(2, '0');
            return `${day}-${month}-${date.getFullYear()}`;
        };

        const getMonthDateRange = (year, month) => ({
            start: new Date(year, month - 1, 1),
            end: new Date(year, month, 0)
        });

        const autoAdjustDates = () => {
            const tuNgayStr = $('#tuNgayDesktop').val();
            const denNgayStr = $('#denNgayDesktop').val();
            if (tuNgayStr && denNgayStr) {
                try {
                    const tuNgay = parseDate(tuNgayStr);
                    const denNgay = parseDate(denNgayStr);
                    if (tuNgay > denNgay) {
                        $('#tuNgayDesktop').val(denNgayStr).addClass('highlight-adjust');
                        setTimeout(() => $('#tuNgayDesktop').removeClass('highlight-adjust'), 1000);
                    }
                } catch (e) {
                    console.error("Lỗi định dạng ngày", e);
                }
            }
        };

        $('#tuNgayDesktop, #denNgayDesktop').on('input change paste', () => {
            if ($('#tuNgayDesktop').val().length === 10 && $('#denNgayDesktop').val().length === 10) {
                setTimeout(autoAdjustDates, 10);
            }
        });

        $('.datepicker-trigger').click(() => setTimeout(autoAdjustDates, 100));

        $('#selectGiaiDoan').change(function () {
            const selectedValue = $(this).val();
            const container = $('#selectContainer').empty();

            container.css('justify-content', (selectedValue === 'Nam' || selectedValue === 'Ngay') ? 'flex-start' : 'space-around');

            const currentYear = new Date().getFullYear();
            const currentMonth = new Date().getMonth() + 1;
            const currentQuy = Math.ceil(currentMonth / 3);

            const createDropdownInput = (id, label, values, defaultValue, onSelect, maxLength = 10) => {
                container.append(`
                    <div data-dropdown-wrapper style="width: 45%; position: relative;">
                        <label class="form-label">${label}</label>
                        <input type="number" class="form-control" id="${id}" value="${defaultValue}"
                            oninput="if(this.value.length > ${maxLength}) this.value = this.value.slice(0, ${maxLength});"
                            autocomplete="off">
                        <div id="${id}Dropdown" class="dropdown-menu-custom"></div>
                    </div>
                `);

                const $input = $('#' + id);
                const $dropdown = $('#' + id + 'Dropdown');
                let currentHighlightIndex = -1;

                const renderList = (filter = '') => {
                    $dropdown.empty().show();
                    currentHighlightIndex = -1;
                    let filteredValues = values.filter(v => !filter || v.toString().includes(filter));
                    if (!filteredValues.length) filteredValues = values;

                    filteredValues.forEach((val, index) => {
                        const isSelected = parseInt($input.val(), 10) === val;
                        const $item = $(`<a href="#" class="dropdown-item ${isSelected ? 'active bg-primary text-white' : ''}" data-val="${val}">${val}</a>`);
                        $item.on('click', (e) => { e.preventDefault(); selectItem(val); });
                        $item.on('mouseenter', () => { currentHighlightIndex = index; highlightCurrentItem(); });
                        $dropdown.append($item);
                        if (isSelected) currentHighlightIndex = index;
                    });

                    highlightCurrentItem();
                };

                const highlightCurrentItem = () => {
                    const items = $dropdown.find('.dropdown-item');
                    items.removeClass('active bg-primary text-white');
                    if (currentHighlightIndex >= 0 && currentHighlightIndex < items.length) {
                        items.eq(currentHighlightIndex).addClass('active bg-primary text-white')[0].scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                    }
                };

                const selectItem = (val) => {
                    $input.val(val);
                    $dropdown.hide();
                    onSelect && onSelect(val);
                };

                $input.on('focus click input', () => renderList($input.val()));

                $input.on('keydown', (e) => {
                    const items = $dropdown.find('.dropdown-item');
                    if (!items.length) return;
                    if (['ArrowUp', 'ArrowDown', 'Enter', 'Escape', 'Tab'].includes(e.key)) e.preventDefault();

                    if (e.key === 'ArrowUp') currentHighlightIndex = (currentHighlightIndex <= 0) ? items.length - 1 : currentHighlightIndex - 1;
                    if (e.key === 'ArrowDown') currentHighlightIndex = (currentHighlightIndex >= items.length - 1) ? 0 : currentHighlightIndex + 1;
                    if (e.key === 'Enter') selectItem(parseInt(items.eq(currentHighlightIndex).data('val')));
                    if (e.key === 'Escape') $dropdown.hide();

                    highlightCurrentItem();
                });

                $(document).off('click.dropdown-' + id).on('click.dropdown-' + id, (e) => {
                    if (!$(e.target).closest('[data-dropdown-wrapper]').length) $dropdown.hide();
                });
            };

            const updateDates = () => {
                let year = parseInt($('#yearInput').val()) || currentYear;
                if (year < 0 || year > currentYear) year = currentYear;

                if (selectedValue === 'Nam') {
                    $('#tuNgayDesktop').val(`01-01-${year}`);
                    $('#denNgayDesktop').val(`31-12-${year}`);
                } else if (selectedValue === 'Quy') {
                    let quy = parseInt($('#quyInput').val()) || currentQuy;
                    quy = Math.min(Math.max(quy, 1), 4);
                    const startMonth = (quy - 1) * 3 + 1;
                    const endMonth = startMonth + 2;
                    $('#tuNgayDesktop').val(formatDate(new Date(year, startMonth - 1, 1)));
                    $('#denNgayDesktop').val(formatDate(new Date(year, endMonth, 0)));
                } else if (selectedValue === 'Thang') {
                    let month = parseInt($('#thangInput').val()) || currentMonth;
                    month = Math.min(Math.max(month, 1), 12);
                    const { start, end } = getMonthDateRange(year, month);
                    $('#tuNgayDesktop').val(formatDate(start));
                    $('#denNgayDesktop').val(formatDate(end));
                } else if (selectedValue === 'Ngay') {
                    const todayStr = formatDate(new Date());
                    $('#tuNgayDesktop, #denNgayDesktop').val(todayStr);
                }

                $('#tuNgayDesktop, #denNgayDesktop').prop('disabled', selectedValue !== 'Ngay');
                $('#tuNgayDesktop').datepicker('setDate', $('#tuNgayDesktop').val());
                $('#denNgayDesktop').datepicker('setDate', $('#denNgayDesktop').val());
            };

            const startYear = 2000;
            const yearOptions = Array.from({ length: currentYear - startYear + 1 }, (_, i) => startYear + i);
            createDropdownInput('yearInput', 'Năm', yearOptions, currentYear, updateDates, 4);

            if (selectedValue === 'Quy') createDropdownInput('quyInput', 'Quý', [1, 2, 3, 4], currentQuy, updateDates, 1);
            if (selectedValue === 'Thang') createDropdownInput('thangInput', 'Tháng', Array.from({ length: 12 }, (_, i) => i + 1), currentMonth, updateDates, 2);

            updateDates();
        });
    });
})(jQuery);

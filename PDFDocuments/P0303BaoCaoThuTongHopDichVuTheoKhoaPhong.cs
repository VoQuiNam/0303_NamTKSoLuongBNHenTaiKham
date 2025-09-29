using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoThuTongHopDichVuTheoKhoaPhong : IDocument
    {
        private readonly List<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly long? _idPhong;
        private readonly long? _idDichVuKyThuat;
        private readonly long? _idNhomDichVu;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;

        public List<M0303Phong> _nhomphongList;
        public List<M0303DichVuKyThuat> _dichvukythuatList;
        public List<M0303NhomDichVuKyThuat> _nhomdichvukythuatList;   // ✅ thêm field đúng kiểu
        public P0303BaoCaoThuTongHopDichVuTheoKhoaPhong(
        List<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO> data,
        DateTime? tuNgay,
        DateTime? denNgay,
        int idPhong,
        int idDichVuKyThuat,
        string logoPath,
        M0303ThongTinDoanhNghiep thongTinDoanhNghiep,
        int idNhomDichVu = 0) // Thêm parameter mới
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _idPhong = idPhong;
            _idDichVuKyThuat = idDichVuKyThuat;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;
            _idNhomDichVu = idNhomDichVu; // Gán giá trị
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;





        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            // Lọc dữ liệu theo ngày nếu có
            var filteredData = _data;
            if (_tuNgay != null && _denNgay != null)
            {
                filteredData = _data.Where(x =>
                    x.NgayTongHop.HasValue &&
                    x.NgayTongHop.Value.Date >= _tuNgay.Value.Date &&
                    x.NgayTongHop.Value.Date <= _denNgay.Value.Date
                ).ToList();
            }

            // Lọc theo idNhomDichVu / idDichVuKyThuat nếu có
            filteredData = filteredData
                .Where(d => (_idNhomDichVu == 0 || d.IDNhomDVKT == _idNhomDichVu)
                         && (_idDichVuKyThuat == 0 || (d.IDDVKT.HasValue && d.IDDVKT.Value == _idDichVuKyThuat)))
                .ToList();

            var phongList = _nhomphongList ?? new List<M0303Phong>();
            var nhomDichVuList = _nhomdichvukythuatList ?? new List<M0303NhomDichVuKyThuat>();

            // Lấy tất cả phòng (không lọc theo dữ liệu)
            var phongListHienThi = phongList
                .OrderBy(p => p.ten)
                .ToList();

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(7));

                // Header
                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(38).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                                col.Item().Height(28).Image(_logoPath, ImageScaling.FitHeight);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep?.TenCSKCB ?? "").Bold().FontSize(8);
                            col.Item().Text(_thongTinDoanhNghiep?.DiaChi ?? "").FontSize(6);
                            col.Item().Text("Điện thoại: " + (_thongTinDoanhNghiep?.DienThoai ?? "")).FontSize(6);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("BÁO CÁO TỔNG HỢP DỊCH VỤ THEO KHOA/PHÒNG").Bold().FontSize(9);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}").FontSize(6);
                        });
                    });
                    headerCol.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                });

                // Nội dung bảng
                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(20); // STT
                            columns.RelativeColumn(3);  // Dịch vụ
                            foreach (var _ in phongListHienThi)
                                columns.RelativeColumn();
                            columns.RelativeColumn();   // Tổng cộng
                        });

                        // Header - SỬ DỤNG .Header() ĐỂ HIỂN THỊ TRÊN MỖI TRANG
                        table.Header(header =>
                        {
                            header.Cell().Element(c => c.Border(0.5f).AlignCenter().AlignMiddle().Text("STT").Bold().FontSize(7));
                            header.Cell().Element(c => c.Border(0.5f).AlignCenter().AlignMiddle().Text("Dịch vụ").Bold().FontSize(7));
                            foreach (var phong in phongListHienThi)
                                header.Cell().Element(c => c.Border(0.5f).AlignCenter().AlignMiddle().Text(phong.ten).Bold().FontSize(6));
                            header.Cell().Element(c => c.Border(0.5f).AlignCenter().AlignMiddle().Text("Tổng cộng").Bold().FontSize(7));
                        });

                        decimal tongTatCa = 0;
                        int sttNhom = 1;
                        int sttDV = 1;

                        // Nhóm dữ liệu theo nhóm dịch vụ
                        var nhomDichVuIds = filteredData.Select(d => d.IDNhomDVKT).Distinct().ToList();
                        var nhomDichVuCoDuLieu = nhomDichVuList.Where(n => nhomDichVuIds.Contains(n.id)).OrderBy(n => n.ten).ToList();

                        foreach (var nhom in nhomDichVuCoDuLieu)
                        {
                            var dichVuTrongNhom = filteredData.Where(d => d.IDNhomDVKT == nhom.id).ToList();

                            // Tính tổng theo phòng cho nhóm - HIỂN THỊ TẤT CẢ PHÒNG
                            var tongTheoPhong = phongListHienThi
                                .Select(p => (decimal)dichVuTrongNhom
                                                .Where(d => d.IDPhongBuong == p.id)
                                                .Sum(d => d.GiaTien ?? 0))
                                .ToList();
                            decimal tongNhom = tongTheoPhong.Sum();
                            tongTatCa += tongNhom;

                            // Dòng tên nhóm dịch vụ kỹ thuật (STT nhóm)
                            table.Cell().ColumnSpan(2).Element(c =>
                                c.Border(0.5f)
                                 .AlignLeft()
                                 .AlignMiddle()
                                 .Padding(2)
                                 .Text($"{sttNhom}. {nhom.ten}")
                                 .Bold()
                                 .FontSize(6));

                            // Tổng theo từng phòng ngay dòng tên nhóm - HIỂN THỊ TẤT CẢ PHÒNG
                            for (int i = 0; i < phongListHienThi.Count; i++)
                            {
                                var tongPhong = tongTheoPhong[i];
                                table.Cell().Element(c => c.Border(0.5f).AlignRight().AlignMiddle().Padding(2)
                                    .Text(tongPhong != 0 ? tongPhong.ToString("N0") : " ").Bold().FontSize(6));
                            }

                            // Cột tổng cộng nhóm
                            table.Cell().Element(c => c.Border(0.5f).AlignRight().AlignMiddle().Padding(2)
                                .Text(tongNhom != 0 ? tongNhom.ToString("N0") : " ").Bold().FontSize(6));

                            sttNhom++; // Tăng STT nhóm

                            // HIỂN THỊ TẤT CẢ DỊCH VỤ (KHÔNG GỘP TRÙNG) - GIỮ NGUYÊN TẤT CẢ BẢN GHI
                            foreach (var dv in dichVuTrongNhom)
                            {
                                var tongDV = dv.GiaTien.HasValue ? (decimal)dv.GiaTien.Value : 0m;

                                table.Cell().Element(c => c.Border(0.5f).AlignCenter().AlignMiddle().Padding(2).Text((sttDV++).ToString()).FontSize(6));
                                table.Cell().Element(c => c.Border(0.5f).AlignMiddle().Padding(2).Text(dv.TenDichVu ?? "").FontSize(6));

                                // Hiển thị giá trị cho từng phòng (kể cả phòng không có dữ liệu)
                                foreach (var phong in phongListHienThi)
                                {
                                    var gia = (dv.IDPhongBuong == phong.id) ? (decimal)(dv.GiaTien ?? 0) : 0m;

                                    table.Cell().Element(c => c.Border(0.5f).AlignRight().AlignMiddle().Padding(2)
                                        .Text(gia != 0 ? gia.ToString("N0") : " ").FontSize(6));
                                }

                                // Cột tổng cộng dịch vụ
                                table.Cell().Element(c => c.Border(0.5f).AlignRight().AlignMiddle().Padding(2)
                                    .Text(tongDV != 0 ? tongDV.ToString("N0") : " ").FontSize(6));
                            }
                        }

                        // Tổng cuối bảng
                        table.Cell().ColumnSpan((uint)(2 + phongListHienThi.Count)).Element(c =>
                            c.Border(0.5f).Padding(5).AlignLeft().Text("TỔNG CỘNG").Bold().FontSize(8));
                        table.Cell().Element(c =>
                            c.Border(0.5f).Padding(5).AlignRight().Text(tongTatCa != 0 ? tongTatCa.ToString("N0") : " ").Bold().FontSize(7));
                    });
                });

                // Footer
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(6);
                    x.CurrentPageNumber().FontSize(6);
                    x.Span(" / ").FontSize(6);
                    x.TotalPages().FontSize(6);
                });
            });
        }

















    }
}

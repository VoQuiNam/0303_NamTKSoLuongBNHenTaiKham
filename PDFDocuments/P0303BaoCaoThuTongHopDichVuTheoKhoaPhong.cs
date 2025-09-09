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
            Console.WriteLine($"== DEBUG: _idNhomDichVu trong Compose = {_idNhomDichVu}");

            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            // Copy dữ liệu readonly vào danh sách làm việc
            var filteredData = _data;

            // Lọc theo ngày nếu có
            if (_tuNgay != null && _denNgay != null)
            {
                filteredData = _data.Where(x =>
                    x.NgayTongHop.HasValue &&
                    x.NgayTongHop.Value.Date >= _tuNgay.Value.Date &&
                    x.NgayTongHop.Value.Date <= _denNgay.Value.Date
                ).ToList();
            }

            // Chuẩn bị danh sách phòng, dịch vụ và nhóm dịch vụ
            var phongList = _nhomphongList ?? new List<M0303Phong>();
            var dichVuList = _dichvukythuatList ?? new List<M0303DichVuKyThuat>();
            var nhomDichVuList = _nhomdichvukythuatList ?? new List<M0303NhomDichVuKyThuat>();

            // Lọc phòng chỉ lấy những phòng có dữ liệu
            var phongListHienThi = phongList
                .Where(p => filteredData.Any(d => d.IdPhong.HasValue && d.IdPhong.Value == p.id))
                .OrderBy(p => p.ten)
                .ToList();

            // Lọc nhóm dịch vụ theo _idNhomDichVu
            var nhomDVList = nhomDichVuList
                .Where(n => _idNhomDichVu == 0 || n.id == _idNhomDichVu)
                .Select(nhom => new
                {
                    Id = nhom.id,
                    Ten = nhom.ten,
                    DichVuList = dichVuList
                        .Where(dv => dv.idNhomDichVu == nhom.id)
                        .Select(dv => new
                        {
                            Id = dv.id,
                            Ten = dv.ten,
                            GiaTheoPhong = phongListHienThi.Select(phong => new
                            {
                                PhongId = phong.id,
                                Gia = filteredData
                                    .Where(x => x.IdDichVuKyThuat.HasValue &&
                                                x.IdDichVuKyThuat.Value == dv.id &&
                                                x.IdPhong.HasValue &&
                                                x.IdPhong.Value == phong.id)
                                    .Select(x => x.Gia ?? 0m)
                                    .Sum()
                            }).ToList(),
                            TongGia = filteredData
                                .Where(x => x.IdDichVuKyThuat.HasValue &&
                                            x.IdDichVuKyThuat.Value == dv.id)
                                .Select(x => x.Gia ?? 0m)
                                .Sum()
                        })
                        .ToList()
                })
                .ToList();

            int tongSoBanGhi = nhomDVList.Sum(n => n.DichVuList.Count);

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(8);
                page.DefaultTextStyle(x => x
                    .FontFamily("Times New Roman")
                    .FontSize(7)
                );

                // Header
                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(50).Column(col =>
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
                            col.Item().AlignRight().Text($"Tổng số dịch vụ: {tongSoBanGhi}").FontSize(6);
                        });
                    });
                    headerCol.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                });

                // Nội dung
                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        // Columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(20); // STT
                            columns.RelativeColumn(3);  // Dịch vụ
                            foreach (var _ in phongListHienThi)
                                columns.RelativeColumn();
                            columns.RelativeColumn();   // Tổng cộng
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)
                                                       .Border(0.5f)
                                                       .AlignCenter()       // căn ngang
                                                       .AlignMiddle()       // căn dọc
                                                       .Text("STT").Bold().FontSize(7).LineHeight(1.6f));

                            header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)
                                                       .Border(0.5f)
                                                       .AlignCenter()
                                                       .AlignMiddle()
                                                       .Text("Dịch vụ").Bold().FontSize(7).LineHeight(1.6f));

                            foreach (var phong in phongListHienThi)
                                header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)
                                                           .Border(0.5f)
                                                           .AlignCenter()
                                                           .AlignMiddle()
                                                           .Text(phong.ten).Bold().FontSize(6).LineHeight(1.6f));

                            header.Cell().Element(c => c.Background(Colors.Grey.Lighten3)
                                                       .Border(0.5f)
                                                       .AlignCenter()
                                                       .AlignMiddle()
                                                       .Text("Tổng cộng").Bold().FontSize(7).LineHeight(1.6f));
                        });



                        //int nhomIndex = 1;
                        decimal tongTatCaDichVu = 0;

                        // Dữ liệu
                        foreach (var nhom in nhomDVList)
                        {
                            var tongTheoPhong = phongListHienThi.Select(p =>
                                nhom.DichVuList.Sum(dv => dv.GiaTheoPhong.FirstOrDefault(x => x.PhongId == p.id)?.Gia ?? 0m)
                            ).ToList();

                            var tongTatCa = tongTheoPhong.Sum();
                            tongTatCaDichVu += tongTatCa;

                            // Hàng nhóm
                            table.Cell().ColumnSpan(2).Element(c =>
                                c.Border(0.5f)
                                 .Padding(5)
                                 //.Text($"{nhomIndex++}. {nhom.Ten}")
                                 .Text($"{nhom.Ten}")
                                 .Bold()
                                 .FontSize(7)
                                 .LineHeight(1.8f)
                            );
                            foreach (var tong in tongTheoPhong)
                                table.Cell().Element(c => c
                                    .Border(0.5f)
                                    .AlignRight()   // căn ngang phải
                                    .AlignMiddle()  // căn dọc giữa
                                    .Padding(3)
                                    .Text(tong != 0 ? tong.ToString("N0") : " ")
                                    .Bold()
                                    .FontSize(6)
                                    .LineHeight(1.6f)
                                );
                            table.Cell().Element(c => c
                                .Border(0.5f)
                                .AlignRight()
                                .AlignMiddle()
                                .Padding(3)
                                .Text(tongTatCa != 0 ? tongTatCa.ToString("N0") : " ")
                                .Bold()
                                .FontSize(6)
                                .LineHeight(1.6f)
                            );

                            // Hàng dịch vụ
                            int dvIndex = 1;
                            foreach (var dv in nhom.DichVuList)
                            {
                                table.Cell().Element(c => c
                                    .Border(0.5f)
                                    .AlignCenter()
                                    .AlignMiddle() // căn dọc giữa
                                    .Padding(2)
                                    .Text(dvIndex++.ToString())
                                    .FontSize(6)
                                    .LineHeight(1.6f)
                                );
                                table.Cell().Element(c => c
                                    .Border(0.5f)
                                    .AlignMiddle() // căn dọc giữa
                                    .Padding(2)
                                    .Text(dv.Ten)
                                    .FontSize(6)
                                    .LineHeight(1.6f)
                                );
                                foreach (var phong in phongListHienThi)
                                {
                                    var gia = dv.GiaTheoPhong.FirstOrDefault(x => x.PhongId == phong.id)?.Gia ?? 0m;
                                    table.Cell().Element(c => c
                                        .Border(0.5f)
                                        .AlignRight()   // căn ngang phải
                                        .AlignMiddle()  // căn dọc giữa
                                        .Padding(2)
                                        .Text(gia != 0 ? gia.ToString("N0") : " ")
                                        .FontSize(6)
                                        .LineHeight(1.6f)
                                    );
                                }
                                table.Cell().Element(c => c
                                    .Border(0.5f)
                                    .AlignRight()
                                    .AlignMiddle()  // căn dọc giữa
                                    .Padding(2)
                                    .Text(dv.TongGia != 0 ? dv.TongGia.ToString("N0") : " ")
                                    .Bold()
                                    .FontSize(6)
                                    .LineHeight(1.6f)
                                );
                            }
                        }

                        // Tổng cuối bảng
                        table.Cell().ColumnSpan((uint)(2 + phongListHienThi.Count)).Element(c =>
                            c
                             .Border(0.5f)
                             .Padding(5)
                             .AlignMiddle()
                             .AlignRight()
                        );

                        table.Cell().Element(c =>
                            c
                             .Border(0.5f)
                             .Padding(5)
                             .AlignMiddle()
                             .AlignRight()
                             .Column(col =>
                             {
                                 col.Item().AlignMiddle().AlignRight()
                                    .Text(tongTatCaDichVu != 0 ? tongTatCaDichVu.ToString("N0") : " ")
                                    .Bold()
                                    .FontSize(7)
                                    .LineHeight(1.8f);
                             })
                        );
                    });
                });

                // Footer
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(6).LineHeight(1.4f);
                    x.CurrentPageNumber().FontSize(6).LineHeight(1.4f);
                    x.Span(" / ").FontSize(6).LineHeight(1.4f);
                    x.TotalPages().FontSize(6).LineHeight(1.4f);
                });
            });
        }












    }
}

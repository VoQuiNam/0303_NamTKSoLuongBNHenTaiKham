using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303DanhSachBNThucHienTheoThietBi : IDocument
    {
        private readonly List<M0303DanhSachBNThucHienTheoThietBiSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private const int TotalColumns = 29;

      
        public P0303DanhSachBNThucHienTheoThietBi(List<M0303DanhSachBNThucHienTheoThietBiSTO> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, dynamic thongTinDoanhNghiep)
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
      
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            
            var reportData = (_data ?? new List<M0303DanhSachBNThucHienTheoThietBiSTO>())
                .Select(x => new
                {
                    x.MaYT,
                    x.SoHS,
                    x.SoBA,
                    x.ICD,
                    x.HoTen,
                    x.TenGioiTinh,
                    x.SoBHYT,
                    x.KCBBD,
                    x.DT,
                    x.DoiTuong,
                    x.TinhTrang,
                    x.NoiChiDinh,
                    x.BacSi,
                    TenNhomDV = x.TenNhomDichVu,
                    TenDVKT = x.TenDichVuKyThuat,
                    x.SoLuong,
                    x.NgayYC,
                    x.NgayTH,
                    x.QuyenSo,
                    x.SoBL,
                    x.ChungTu,
                    x.TenThietBi,
                    x.DoanhThu,
                    x.BaoHiem,
                    x.DaThanhToan,
                    x.ChuaThanhToan,
                    x.HuyHoan,
                    x.TrangThaiThanhToan
                })
                .ToList();

            const int ColumnCount = 29;

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(8));

                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(60).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                                col.Item().Height(36).Image(_logoPath, ImageScaling.FitHeight);
                            else
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(8);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep?.TenCSKCB ?? "").Bold().FontSize(11);
                            col.Item().Text(_thongTinDoanhNghiep?.DiaChi ?? "").FontSize(8);
                            col.Item().Text("Điện thoại: " + (_thongTinDoanhNghiep?.DienThoai ?? "")).FontSize(8);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("DANH SÁCH BN THỰC HIỆN THEO THIẾT BỊ").Bold().FontSize(12);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}").FontSize(8);
                        });
                    });

                    headerCol.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < ColumnCount; i++)
                                columns.RelativeColumn();
                        });

                        string[] headers = {
                    "STT", "Mã YT", "Số HS", "Số BA", "ICD", "Họ tên", "Giới tính", "Số BHYT",
                    "KCBBD", "ĐT", "Đối tượng", "TT", "Nơi chỉ định", "Bác sỹ",
                    "Nhóm DV", "Dịch vụ", "SL", "Ngày YC", "Ngày TH", "Quyển sổ", "Số BL",
                    "Chứng từ", "Thiết bị", "Doanh thu", "Bảo hiểm", "Đã thanh toán", "Chưa thanh toán",
                    "Hủy Hoàn", "Đã thanh toán"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Element(c =>
                                {
                                    c.Background(Colors.Grey.Lighten3)
                                     .Border(1).BorderColor(Colors.Grey.Darken1)
                                     .Padding(3)
                                     .AlignCenter().AlignMiddle()   // căn giữa dọc
                                     .Text(h).Bold();
                                });
                            }
                        });

                        int stt = 1;

                        int totalSoLuong = reportData.Sum(x => x.SoLuong ?? 0);
                        decimal totalDoanhThu = reportData.Sum(x => x.DoanhThu ?? 0);
                        decimal totalDaThanhToan = reportData.Sum(x => x.DaThanhToan ?? 0);
                        decimal totalChuaThanhToan = reportData.Sum(x => x.ChuaThanhToan ?? 0);
                        decimal totalBaoHiem = reportData.Sum(x => x.BaoHiem ?? 0);

                        foreach (var item in reportData)
                        {
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().AlignMiddle().Text((stt++).ToString()));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.MaYT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.SoHS ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.SoBA ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.ICD ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.HoTen ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenGioiTinh ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.SoBHYT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.KCBBD ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.DT ?? "")); 
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.DoiTuong ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TinhTrang ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.NoiChiDinh ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.BacSi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenNhomDV ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenDVKT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(item.SoLuong?.ToString() ?? "0"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(FormatDateAMPM(item.NgayYC)));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
                                .Text(FormatDateAMPM(item.NgayTH)));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.QuyenSo ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.SoBL ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.ChungTu ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenThietBi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(item.DoanhThu.HasValue && item.DoanhThu.Value != 0
         ? item.DoanhThu.Value.ToString("N0")
         : "-"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(item.BaoHiem.HasValue && item.BaoHiem.Value != 0
         ? item.BaoHiem.Value.ToString("N0")
         : "-"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().
     AlignMiddle().Text(item.DaThanhToan.HasValue && item.DaThanhToan.Value != 0
         ? item.DaThanhToan.Value.ToString("N0")
         : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
                                .Text(item.ChuaThanhToan.HasValue && item.ChuaThanhToan.Value != 0
                                    ? item.ChuaThanhToan.Value.ToString("N0")
                                    : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().AlignMiddle().Text(item.HuyHoan ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().AlignMiddle().Text(item.TrangThaiThanhToan ?? ""));
                        }

                        table.Cell().ColumnSpan(16).Element(c =>
                            c.Border(1).Padding(3).AlignCenter().AlignMiddle().Text("Tổng cộng").Bold()
                        );
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().AlignMiddle().Text(totalSoLuong.ToString()).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(totalDoanhThu != 0 ? totalDoanhThu.ToString("N0") : "0").Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(totalBaoHiem != 0 ? totalBaoHiem.ToString("N0") : "0").Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(totalDaThanhToan != 0 ? totalDaThanhToan.ToString("N0") : "0").Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
     .Text(totalChuaThanhToan != 0 ? totalChuaThanhToan.ToString("N0") : "0").Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                    });

                   
                });


                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(9);
                    x.CurrentPageNumber().FontSize(9);
                    x.Span(" / ").FontSize(9);
                    x.TotalPages().FontSize(9);
                });
            });
        }

        private string FormatDateAMPM(DateTime? date)
        {
            return date?.ToString("dd-MM-yyyy hh:mm:ss tt") ?? "";
        }



    }
}

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
        //private readonly long? _idNhomDichVu;
        //private readonly long? _idDichVuKyThuat;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private const int TotalColumns = 29;

        //private List<M0303NhomDichVuKyThuat> _nhomdichvukythuatList;
        //private List<M0303DichVuKyThuat> _dichvukythuatList;

        public P0303DanhSachBNThucHienTheoThietBi(List<M0303DanhSachBNThucHienTheoThietBiSTO> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, dynamic thongTinDoanhNghiep)
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            //_idNhomDichVu = idNhomDichVu;
            //_idDichVuKyThuat = idDichVuKyThuat;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;


            //string nhomdichvukythuatJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_NhomDichVuKyThuat.json"));
            //_nhomdichvukythuatList = JsonConvert.DeserializeObject<List<M0303NhomDichVuKyThuat>>(nhomdichvukythuatJson);

            //string dichvukythuatJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_DichVuKyThuat.json"));
            //_dichvukythuatList = JsonConvert.DeserializeObject<List<M0303DichVuKyThuat>>(dichvukythuatJson);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            // Dùng luôn dữ liệu từ STO (không cần map ID nữa)
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

                // ========== HEADER ==========
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

                // ========== TABLE ==========
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
                    "KCBBD", "ĐT", "Đối tượng", "TT", "Nơi chỉ định", "Bác sĩ",
                    "Nhóm DV", "Dịch vụ", "SL", "Ngày YC", "Ngày TH", "Quyển sổ", "Số BL",
                    "Chứng từ", "Thiết bị", "Doanh thu", "BHYT", "Đã thanh toán", "Chưa thanh toán",
                    "Hủy/Hoàn", "Thanh toán"
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

                        // Tính tổng
                        int totalSoLuong = reportData.Sum(x => x.SoLuong ?? 0);
                        decimal totalDoanhThu = reportData.Sum(x => x.DoanhThu ?? 0);
                        decimal totalDaThanhToan = reportData.Sum(x => x.DaThanhToan ?? 0);
                        decimal totalChuaThanhToan = reportData.Sum(x => x.ChuaThanhToan ?? 0);

                        // Dữ liệu chi tiết
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
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.DT ?? "")); // Số điện thoại
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.DoiTuong ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TinhTrang ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.NoiChiDinh ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.BacSi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenNhomDV ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenDVKT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(item.SoLuong?.ToString() ?? "0"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(item.NgayYC?.ToString("dd-MM-yyyy HH:mm:ss") ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(item.NgayTH?.ToString("dd-MM-yyyy HH:mm:ss") ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.QuyenSo ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.SoBL ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.ChungTu ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.TenThietBi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle()
                                .Text(item.DoanhThu.HasValue ? item.DoanhThu.Value.ToString("N0") : "-"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignMiddle().Text(item.BaoHiem ?? ""));
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

                        // Tổng cuối
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
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(totalDoanhThu.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().Text(totalDaThanhToan.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().AlignMiddle().AlignMiddle().Text(totalChuaThanhToan.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                    });

                    contentCol.Item().PaddingTop(10).ShowEntire().Column(summaryCol =>
                    {
                        summaryCol.Item().Row(row =>
                        {
                            row.RelativeColumn(6);
                            row.RelativeColumn().AlignCenter().Column(c =>
                            {
                                c.Item().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
                                   .FontSize(9).Italic();
                                c.Item().PaddingBottom(5);
                            });
                        });


                        summaryCol.Item().PaddingHorizontal(20).Row(row =>
                        {

                            row.RelativeColumn().AlignLeft().PaddingRight(10).Column(c =>
                            {
                                c.Item().Text("THỦ TRƯỞNG ĐƠN VỊ").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên, đóng dấu)").Italic().FontSize(8);
                            });


                            row.RelativeColumn().AlignCenter().PaddingHorizontal(5).Column(c =>
                            {
                                c.Item().Text("THỦ QUỸ").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });


                            row.RelativeColumn().AlignCenter().PaddingHorizontal(5).Column(c =>
                            {
                                c.Item().Text("KẾ TOÁN").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });


                            row.RelativeColumn().AlignRight().PaddingLeft(10).Column(c =>
                            {
                                c.Item().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });
                        });
                    });
                });

                // ========== FOOTER ==========
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(9);
                    x.CurrentPageNumber().FontSize(9);
                    x.Span(" / ").FontSize(9);
                    x.TotalPages().FontSize(9);
                });
            });
        }




    }
}

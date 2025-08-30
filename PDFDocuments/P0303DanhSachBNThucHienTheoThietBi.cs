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
        private readonly long? _idNhomDichVu;
        private readonly long? _idDichVuKyThuat;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private const int TotalColumns = 29;

        private List<M0303NhomDichVuKyThuat> _nhomdichvukythuatList;
        private List<M0303DichVuKyThuat> _dichvukythuatList;

        public P0303DanhSachBNThucHienTheoThietBi(List<M0303DanhSachBNThucHienTheoThietBiSTO> data, DateTime? tuNgay, DateTime? denNgay, int idNhomDichVu, int idDichVuKyThuat, string logoPath, dynamic thongTinDoanhNghiep)
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _idNhomDichVu = idNhomDichVu;
            _idDichVuKyThuat = idDichVuKyThuat;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;


            string nhomdichvukythuatJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_NhomDichVuKyThuat.json"));
            _nhomdichvukythuatList = JsonConvert.DeserializeObject<List<M0303NhomDichVuKyThuat>>(nhomdichvukythuatJson);

            string dichvukythuatJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_DichVuKyThuat.json"));
            _dichvukythuatList = JsonConvert.DeserializeObject<List<M0303DichVuKyThuat>>(dichvukythuatJson);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

           
            var nhomDVDict = _nhomdichvukythuatList?.ToDictionary(n => Convert.ToInt64(n.id), n => n.ten)
                             ?? new Dictionary<long, string>();
            var dvDict = _dichvukythuatList?.ToDictionary(d => Convert.ToInt64(d.id), d => d.ten)
                         ?? new Dictionary<long, string>();

           
            var reportData = (_data ?? new List<M0303DanhSachBNThucHienTheoThietBiSTO>()).Select(x =>
            {
                string tenNhom = x.IdNhomDichVu.HasValue && nhomDVDict.TryGetValue(x.IdNhomDichVu.Value, out var _tenNhom)
                                 ? _tenNhom : "";
                string tenDv = x.IdDichVuKyThuat.HasValue && dvDict.TryGetValue(x.IdDichVuKyThuat.Value, out var _tenDv)
                               ? _tenDv : "";

                return new
                {
                    x.MaYT,
                    x.SoHS,
                    x.SoBA,
                    x.ICD,
                    x.HoTen,
                    x.GioiTinh,
                    x.SoBHYT,
                    x.KCBBD,
                    x.DT,
                    x.DoiTuong,
                    x.TinhTrang,
                    x.NoiChiDinh,
                    x.BacSi,
                    TenNhomDV = tenNhom,
                    TenDVKT = tenDv,
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
                };
            }).ToList();

            const int ColumnCount = 29;

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);
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
                    "KCBBD", "ĐT", "Đối tượng", "TT", "Nơi chỉ định", "Bác sĩ",
                    "Nhóm DV", "Dịch vụ", "SL", "Ngày YC", "Ngày TH", "Quyển sổ", "Số BL",
                    "Chứng từ", "Thiết bị", "Doanh thu", "BHYT", "Đã thanh toán", "Chưa thanh toán",
                    "Hủy/Hoàn", "Đã thanh toán"
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
                                     .AlignCenter()
                                     .Text(h).Bold();
                                });
                            }
                        });

                        int stt = 1;

                       
                        int totalSoLuong = reportData.Sum(x => x.SoLuong ?? 0);
                        decimal totalDoanhThu = reportData.Sum(x => x.DoanhThu ?? 0);
                        decimal totalBaoHiem = reportData.Sum(x => x.BaoHiem ?? 0);
                        decimal totalDaThanhToan = reportData.Sum(x => x.DaThanhToan ?? 0);
                        decimal totalChuaThanhToan = reportData.Sum(x => x.ChuaThanhToan ?? 0);

                        
                        foreach (var item in reportData)
                        {
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().Text((stt++).ToString()));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.MaYT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.SoHS ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.SoBA ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.ICD ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.HoTen ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.GioiTinh ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.SoBHYT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.KCBBD ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().Text(item.DT == true ? "X" : ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.DoiTuong ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.TinhTrang ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.NoiChiDinh ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.BacSi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.TenNhomDV ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.TenDVKT ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(item.SoLuong?.ToString() ?? "0"));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(item.NgayYC?.ToString("dd-MM-yyyy HH:mm:ss") ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(item.NgayTH?.ToString("dd-MM-yyyy HH:mm:ss") ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.QuyenSo ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.SoBL ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.ChungTu ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).Text(item.TenThietBi ?? ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight()
                            .Text(item.DoanhThu.HasValue && item.DoanhThu.Value != 0 ? item.DoanhThu.Value.ToString("N0") : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight()
                                .Text(item.BaoHiem.HasValue && item.BaoHiem.Value != 0 ? item.BaoHiem.Value.ToString("N0") : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight()
                                .Text(item.DaThanhToan.HasValue && item.DaThanhToan.Value != 0 ? item.DaThanhToan.Value.ToString("N0") : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignRight()
                                .Text(item.ChuaThanhToan.HasValue && item.ChuaThanhToan.Value != 0 ? item.ChuaThanhToan.Value.ToString("N0") : "-"));

                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().Text(item.HuyHoan == true ? "X" : ""));
                            table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().Text(item.TrangThaiThanhToan == true ? "X" : ""));
                        }

            
                        table.Cell().ColumnSpan(16).Element(c =>
                            c.Border(1).Padding(3).AlignCenter().Text("Tổng cộng").Bold()
                        );
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignCenter().Text(totalSoLuong.ToString()).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).Text(""));
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(totalDoanhThu.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(totalBaoHiem.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(totalDaThanhToan.ToString("N0")).Bold());
                        table.Cell().Element(c => c.Border(1).Padding(3).AlignRight().Text(totalChuaThanhToan.ToString("N0")).Bold());
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



    }
}

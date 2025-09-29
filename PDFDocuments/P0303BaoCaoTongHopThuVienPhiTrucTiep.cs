using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;



namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoTongHopThuVienPhiTrucTiep : IDocument
    {
        private readonly List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        // ✨ thêm property này để nhận danh sách nhóm dịch vụ từ ngoài
        public List<string> NhomDichVuList { get; set; } = new();
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private const int TotalColumns = 29;


        public P0303BaoCaoTongHopThuVienPhiTrucTiep(List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, dynamic thongTinDoanhNghiep)
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

            var reportData = _data ?? new List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>();

            // Danh sách nhóm dịch vụ
            var nhomDichVuList = NhomDichVuList != null && NhomDichVuList.Any()
                ? NhomDichVuList
                : reportData.Select(x => x.TenNhomDichVu)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

            string[] fixedCols = { "STT", "Mã BN/Mã đợt", "Họ và tên", "Năm sinh", "Mã thẻ BHYT", "Đối tượng", "Ngày thu", "Quyển sổ", "Số biên lai", "Số chứng từ", "Miễn giảm", "Lý do miễn", "Nhập viện nhập miễn", "Ghi chú miễn", "Nợ", "Số tiền" };
            string[] fixedEndCols = { "Hủy", "Hoàn", "Ngày Hủy/Hoàn" };

            int totalCols = fixedCols.Length + 1 + nhomDichVuList.Count + 1 + fixedEndCols.Length;

            var groupedByBienLai = reportData.GroupBy(x => new { x.MaBN, x.SoBienLai }).ToList();

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily("Arial Narrow").FontSize(6f));

                // HEADER
                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(35f).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                                col.Item().Height(25f).Image(_logoPath, ImageScaling.FitHeight);
                            else
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(5f);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep?.TenCSKCB ?? "").Bold().FontSize(8f);
                            col.Item().Text(_thongTinDoanhNghiep?.DiaChi ?? "").FontSize(6f);
                            col.Item().Text("Điện thoại: " + (_thongTinDoanhNghiep?.DienThoai ?? "")).FontSize(6f);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("BÁO CÁO TỔNG HỢP THU VIỆN PHÍ TRỰC TIẾP").Bold().FontSize(9f);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}").FontSize(6f);
                        });
                    });

                    headerCol.Item().PaddingVertical(2f).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                });

                // CONTENT
                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < totalCols; i++)
                                columns.RelativeColumn();
                        });

                        // HEADER BẢNG
                        table.Header(header =>
                        {
                            // Hàng 1
                            foreach (var col in fixedCols)
                                header.Cell().RowSpan(2).Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                        .AlignCenter().AlignMiddle().Text(col).Bold().FontSize(5f));

                            header.Cell().ColumnSpan((uint)(1 + nhomDichVuList.Count + 1)).Element(c =>
                                c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                    .AlignCenter().AlignMiddle().Text("THÔNG TIN CHI TIẾT").Bold().FontSize(5f));

                            foreach (var col in fixedEndCols)
                                header.Cell().RowSpan(2).Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                        .AlignCenter().AlignMiddle().Text(col).Bold().FontSize(5f));

                            // Hàng 2
                            header.Cell().Element(c =>
                                c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                    .AlignCenter().AlignMiddle().Text("Thuốc").Bold().FontSize(5f));

                            foreach (var nhom in nhomDichVuList)
                                header.Cell().Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                        .AlignCenter().AlignMiddle().Text(nhom).WrapAnywhere().Bold().FontSize(4f));

                            header.Cell().Element(c =>
                                c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f)
                                    .AlignCenter().AlignMiddle().Text("Tổng cộng").Bold().FontSize(5f));

                            // Hàng 3: đánh số cột
                            for (int i = 1; i <= totalCols; i++)
                            {
                                string colLabel = i == 1 ? "A" : (i - 1).ToString();

                                header.Cell().Element(c =>
                                    c.Background(Colors.Grey.Lighten4)
                                     .Border(0.5f)
                                     .Padding(1.5f)
                                     .AlignCenter()
                                     .AlignMiddle()
                                     .Text(colLabel)
                                     .FontSize(5f));
                            }

                        });


                        // BODY
                        int stt = 1;
                        foreach (var blGroup in groupedByBienLai)
                        {
                            var item = blGroup.First();

                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text((stt++).ToString()).FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.MaBN ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.HoTen ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.NamSinh?.ToString() ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.MaTheBHYT ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.DoiTuong ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.NgayThu?.ToString("dd-MM-yyyy") ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.QuyenSo ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.SoBienLai ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.SoChungTu ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(item.MienGiam)).FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.LyDoMien ?? "").FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.NhapVienNhapMien ?? "").FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.GhiChuMien ?? "").FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(item.No)).FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(item.SoTien)).FontSize(5f));

                            // Thuốc
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(item.Thuoc)).FontSize(5f));

                            // Nhóm dịch vụ
                            foreach (var nhom in nhomDichVuList)
                            {
                                var tongChiTiet = blGroup.Where(x => x.TenNhomDichVu == nhom).Sum(x => x.SoTienChiTiet ?? 0);
                                table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle()
                                    .Text(tongChiTiet > 0 ? FormatNumber(tongChiTiet) : "-").FontSize(5f));
                            }

                            // Tổng cộng chi tiết
                            var totalChiTietBL = (item.Thuoc ?? 0) + nhomDichVuList.Sum(n => blGroup.Where(x => x.TenNhomDichVu == n).Sum(x => x.SoTienChiTiet ?? 0));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalChiTietBL)).FontSize(5f));

                            // Hủy/Hoàn/Ngày Hủy
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.Huy == true ? "1" : "0").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.Hoan == true ? "1" : "0").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(item.NgayHuyHoan?.ToString("dd-MM-yy") ?? "-").FontSize(5f));
                        }

                        // Dòng tổng cuối bảng
                        var totalMienGiam = groupedByBienLai.Sum(g => g.First().MienGiam ?? 0);
                        var totalNo = groupedByBienLai.Sum(g => g.First().No ?? 0);
                        var totalSoTien = groupedByBienLai.Sum(g => g.First().SoTien ?? 0);
                        var totalThuoc = groupedByBienLai.Sum(g => g.First().Thuoc ?? 0);
                        var totalChiTietNhom = nhomDichVuList.Distinct().ToDictionary(
                            n => n,
                            n => groupedByBienLai.Sum(g => g.Where(x => x.TenNhomDichVu == n).Sum(x => x.SoTienChiTiet ?? 0))
                        );
                        var totalTongChiTiet = totalThuoc + totalChiTietNhom.Sum(kv => kv.Value);

                        table.Cell().ColumnSpan(10).Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text("TỔNG CỘNG").Bold().FontSize(5f));
                        table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalMienGiam)).Bold().FontSize(5f));
                        for (int i = 0; i < 3; i++)
                            table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text(" ").FontSize(5f));
                        table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalNo)).Bold().FontSize(5f));
                        table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalSoTien)).Bold().FontSize(5f));
                        table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalThuoc)).Bold().FontSize(5f));
                        foreach (var nhom in nhomDichVuList)
                            table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalChiTietNhom[nhom])).Bold().FontSize(5f));
                        table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignRight().AlignMiddle().Text(FormatNumber(totalTongChiTiet)).Bold().FontSize(5f));
                        for (int i = 0; i < 3; i++)
                            table.Cell().Element(x => x.Background(Colors.Grey.Lighten2).Border(0.5f).Padding(1.5f).AlignCenter().AlignMiddle().Text("-").FontSize(5f));
                    });
                });

                // Footer chỉ hiện số trang
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(6f);
                    x.CurrentPageNumber().FontSize(6f);
                    x.Span(" / ").FontSize(6f);
                    x.TotalPages().FontSize(6f);
                });
            });
        }








        private string AbbreviateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return " ";
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength) + "...";
        }


        private string FormatNumber(decimal? value, bool isSimpleFormat = false)
        {
            if (value == null || value == 0) return "-";
            return isSimpleFormat ? value.Value.ToString("N0") : value.Value.ToString("N0");
        }
    }
}

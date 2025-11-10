using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303DanhSachKhamBenhTheoHuongGiaiQuyet : IDocument
    {
        private readonly List<M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep? _thongTinDoanhNghiep;
        private readonly string? _idnv;
        private readonly int? _idGiaiQuyet;
        private readonly string _tenNhanVien; // THÊM FIELD NÀY
        public P0303DanhSachKhamBenhTheoHuongGiaiQuyet(
            List<M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTO> data,
            DateTime? tuNgay,
            DateTime? denNgay,
            string logoPath,
            M0303ThongTinDoanhNghiep? thongTinDoanhNghiep,
            string tenNhanVien = null,
            int? idGiaiQuyet = null
            )
        {
            _data = data ?? new List<M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTO>();
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;
            _idGiaiQuyet = idGiaiQuyet;
            _tenNhanVien = tenNhanVien;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        /*sửa*/
        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            // 🆕 NHÓM DỮ LIỆU THEO BÁC SĨ
            var groupedByBacSi = _data
                .GroupBy(x => x.TenGiaiQuyet ?? "Không rõ tên giải quyết")
                .ToDictionary(g => g.Key, g => g.ToList());

            // 🆕 TÍNH TỔNG TOÀN BỘ
            int totalSoLuong = _data.Sum(x => x.SoLuong ?? 0);
            int totalBHYT = _data.Count(x => x.BHYT == true);

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(9).FontColor(Colors.Black));

                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(60).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                            {
                                col.Item().Height(35).Image(_logoPath, ImageScaling.FitHeight);
                            }
                            else
                            {
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(9);
                            }
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep.TenCSKCB).Bold().FontSize(11);
                            col.Item().Text(_thongTinDoanhNghiep.DiaChi).FontSize(8);
                            col.Item().Text("Điện thoại: " + _thongTinDoanhNghiep.DienThoai).FontSize(8);
                            col.Item().Text("PHÒNG CÔNG NGHỆ THÔNG TIN").Bold().FontSize(11);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("DANH SÁCH KHÁM BỆNH THEO HƯỚNG GIẢI QUYẾT")
                                .Bold().FontSize(12).FontColor(Colors.Black);
                            col.Item().AlignRight().Text("Đơn vị điều trị dịch vụ")
                                .Italic().FontSize(9);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}")
                                .FontSize(8).FontColor(Colors.Black);
                        });
                    });

                    headerCol.Item().PaddingVertical(4).LineHorizontal(1)
                        .LineColor(Colors.Grey.Darken2);
                });

                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        // 🆕 ĐỊNH NGHĨA CỘT THEO BẢNG CỦA BẠN
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25); // STT
                            columns.RelativeColumn(0.8f); // Ngày khám
                            columns.RelativeColumn(1f); // Mã y tế
                            columns.RelativeColumn(1.5f); // Họ tên bệnh nhân
                            columns.RelativeColumn(0.7f); // Ngày sinh
                            columns.ConstantColumn(30); // BHYT
                            columns.RelativeColumn(1.8f); // Tên dịch vụ
                            columns.RelativeColumn(1.2f); // Nơi thực hiện
                            columns.ConstantColumn(35); // Số lượng
                        });

                        table.Header(header =>
                        {
                            void AddHeaderCell(string text)
                            {
                                header.Cell()
                                    .Border(1).BorderColor(Colors.Black)
                                    .Background(Colors.Grey.Lighten4)
                                    .PaddingVertical(3).PaddingHorizontal(2)
                                    .AlignCenter().AlignMiddle()
                                    .Text(text).Bold().FontSize(8);
                            }

                            AddHeaderCell("STT");
                            AddHeaderCell("Ngày khám");
                            AddHeaderCell("Mã y tế");
                            AddHeaderCell("Họ tên bệnh nhân");
                            AddHeaderCell("Ngày sinh");
                            AddHeaderCell("BHYT");
                            AddHeaderCell("Tên dịch vụ");
                            AddHeaderCell("Nơi thực hiện");
                            AddHeaderCell("Số lượng");
                        });

                        int stt = 1;

                        // 🆕 DUYỆT QUA TỪNG NHÓM BÁC SĨ
                        foreach (var bacSiGroup in groupedByBacSi)
                        {
                            string bacSi = bacSiGroup.Key;
                            var patients = bacSiGroup.Value;

                            // 🆕 TÍNH TỔNG CHO BÁC SĨ NÀY
                            int subTotalSoLuong = patients.Sum(x => x.SoLuong ?? 0);
                            int subTotalBHYT = patients.Count(x => x.BHYT == true);

                            // 🆕 THÊM DÒNG HEADER CHO BÁC SĨ VỚI THÔNG TIN TỔNG
                            table.Cell().ColumnSpan(5)
                                .Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignMiddle().Text($"- {bacSi}").Bold().FontSize(8);

                            table.Cell().ColumnSpan(1)
                                .Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignMiddle().AlignCenter()
                                .Text(subTotalBHYT.ToString()).Bold().FontSize(8);

                            table.Cell().ColumnSpan(2)
                                .Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignMiddle().AlignCenter()
                                .Text("").Bold().FontSize(8);

                            table.Cell().ColumnSpan(1)
                                .Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignMiddle().AlignCenter()
                                .Text(subTotalSoLuong.ToString()).Bold().FontSize(8);

                            // 🆕 THÊM CÁC DÒNG DỮ LIỆU BỆNH NHÂN
                            foreach (var item in patients)
                            {
                                void AddDataCell(string text, bool center = false, bool left = false)
                                {
                                    var cell = table.Cell().Border(1).BorderColor(Colors.Black)
                                        .PaddingVertical(2).PaddingHorizontal(2)
                                        .AlignMiddle().Text(text ?? "").FontSize(7).WrapAnywhere();

                                    if (center) cell.AlignCenter();
                                    if (left) cell.AlignLeft();
                                }

                                AddDataCell(stt++.ToString(), true); // STT
                                AddDataCell(item.NgayKham?.ToString("dd-MM-yyyy") ?? "", true); // Ngày khám
                                AddDataCell(item.MaYTe ?? "", true); // Mã y tế
                                AddDataCell(item.TenBN ?? ""); // Họ tên bệnh nhân
                                AddDataCell(item.NgaySinh?.ToString("dd-MM-yyyy") ?? "", true); // Ngày sinh
                                AddDataCell(item.BHYT == true ? "X" : "", true); // BHYT
                                AddDataCell(item.TenDichVu ?? ""); // Tên dịch vụ
                                AddDataCell(item.NoiThucHien ?? ""); // Nơi thực hiện
                                AddDataCell((item.SoLuong ?? 0).ToString(), true); // Số lượng
                            }
                        }

                        // 🆕 THÊM DÒNG TỔNG CỘNG CUỐI BẢNG
                        table.Cell().ColumnSpan(5)
                            .Border(1).BorderColor(Colors.Black)
                            .PaddingVertical(3).PaddingHorizontal(2)
                            .AlignMiddle().AlignCenter()
                            .Text("").Bold().FontSize(8);

                        table.Cell().ColumnSpan(1)
                            .Border(1).BorderColor(Colors.Black)
                            .PaddingVertical(3).PaddingHorizontal(2)
                            .AlignMiddle().AlignCenter()
                            .Text(totalBHYT.ToString()).Bold().FontSize(8);

                        table.Cell().ColumnSpan(2)
                            .Border(1).BorderColor(Colors.Black)
                            .PaddingVertical(3).PaddingHorizontal(2)
                            .AlignMiddle().AlignCenter()
                            .Text("Tổng cộng:").Bold().FontSize(8);

                        table.Cell().ColumnSpan(1)
                            .Border(1).BorderColor(Colors.Black)
                            .PaddingVertical(3).PaddingHorizontal(2)
                            .AlignMiddle().AlignCenter()
                            .Text(totalSoLuong.ToString()).Bold().FontSize(8);
                    });

                    contentCol.Item().PaddingTop(20).AlignRight().Width(200).Column(nguoiLapCol =>
                    {
                        nguoiLapCol.Item().AlignCenter().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}").Italic().FontSize(10);
                        nguoiLapCol.Item().PaddingTop(5).AlignCenter().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(10);
                        // THÊM HIỂN THỊ TÊN NHÂN VIÊN Ở ĐÂY
                        if (!string.IsNullOrEmpty(_tenNhanVien))
                        {
                            nguoiLapCol.Item().PaddingTop(40).AlignCenter().Text(_tenNhanVien).Bold().FontSize(10);
                        }
                        else
                        {
                            nguoiLapCol.Item().PaddingTop(40).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(9);
                        }
                    });
                });

                page.Footer()
                    .AlignRight()
                    .Text(x =>
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

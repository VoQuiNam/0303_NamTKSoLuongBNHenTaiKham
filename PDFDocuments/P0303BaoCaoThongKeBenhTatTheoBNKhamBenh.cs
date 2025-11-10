using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;


namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoThongKeBenhTatTheoBNKhamBenh : IDocument
    {
        private readonly List<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private readonly string _tenNhanVien; // THÊM FIELD NÀY

        public P0303BaoCaoThongKeBenhTatTheoBNKhamBenh(List<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, M0303ThongTinDoanhNghiep thongTinDoanhNghiep, string tenNhanVien = null)
        {
            _data = data ?? new List<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO>();
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep ?? new M0303ThongTinDoanhNghiep
            {
                TenCSKCB = "Tên đơn vị",
                DiaChi = "",
                DienThoai = ""
            };
            _tenNhanVien = tenNhanVien; // Gán trực tiếp
        }


        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        /*sửa*/
        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            // 🆕 NHÓM DỮ LIỆU THEO TEN_BENH_GOC
            var groupedByTenBenhGoc = _data
                .GroupBy(x => x.TenBenhGoc ?? "Không xác định")
                .ToDictionary(g => g.Key, g => g.ToList());

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(8).FontColor(Colors.Black));

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
                         
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("THỐNG KÊ BỆNH TẬT THEO BỆNH NHÂN KHÁM BỆNH")
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
                        // 🆕 ĐỊNH NGHĨA CỘT THEO BẢNG THỐNG KÊ BỆNH TẬT
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60); // Mã ICD
                            columns.RelativeColumn(2f); // Tên bệnh
                            columns.ConstantColumn(50); // Tổng số
                            columns.ConstantColumn(40); // Nữ
                            columns.ConstantColumn(50); // < 15 tuổi
                            columns.ConstantColumn(50); // < 6 tuổi
                            columns.ConstantColumn(50); // Ra toa
                            columns.ConstantColumn(50); // Vào viện
                            columns.ConstantColumn(50); // Ngoại trú
                            columns.ConstantColumn(50); // Chuyển viện
                            columns.ConstantColumn(50); // Hẹn tái khám
                            columns.ConstantColumn(40); // Khác
                        });

                        // 🆕 HEADER BẢNG VỚI 2 DÒNG
                        table.Header(header =>
                        {
                            void AddHeaderCell(string text, int rowspan = 1, int colspan = 1)
                            {
                                header.Cell().RowSpan((uint)rowspan).ColumnSpan((uint)colspan)
                                    .Border(1).BorderColor(Colors.Black)
                                    .PaddingVertical(3).PaddingHorizontal(2)
                                    .AlignCenter().AlignMiddle()
                                    .Text(text).Bold().FontSize(7);
                            }

                            AddHeaderCell("Mã ICD", 2, 1);
                            AddHeaderCell("Tên bệnh", 2, 1);
                            AddHeaderCell("Tổng số", 2, 1);
                            AddHeaderCell("Trong đó", 1, 3);
                            AddHeaderCell("Cách giải quyết", 1, 6);

                            // Dòng 2 - các header này không cần rowspan/colspan
                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Nữ").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("< 15 tuổi").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("< 6 tuổi").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Ra toa").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Vào viện").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Ngoại trú").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Chuyển viện").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Hẹn tái khám").Bold().FontSize(7);

                            header.Cell().Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(2)
                                .AlignCenter().AlignMiddle()
                                .Text("Khác").Bold().FontSize(7);
                        });

                        // 🆕 DUYỆT QUA TỪNG NHÓM BỆNH GỐC
                        foreach (var benhGocGroup in groupedByTenBenhGoc)
                        {
                            string tenBenhGoc = benhGocGroup.Key;
                            var items = benhGocGroup.Value;

                            // 🆕 THÊM DÒNG HEADER CHO NHÓM BỆNH GỐC
                            table.Cell().ColumnSpan(12)
                                .Border(1).BorderColor(Colors.Black)
                                .PaddingVertical(3).PaddingHorizontal(5)
                                .AlignMiddle().AlignLeft()
                                .Text(tenBenhGoc).Bold().FontSize(8);

                            // 🆕 THÊM CÁC DÒNG DỮ LIỆU CHI TIẾT
                            foreach (var item in items)
                            {
                                void AddDataCell(string text, bool center = true)
                                {
                                    var cell = table.Cell().Border(1).BorderColor(Colors.Black)
                                        .PaddingVertical(2).PaddingHorizontal(2)
                                        .AlignMiddle().Text(text ?? "").FontSize(7);

                                    if (center) cell.AlignCenter();
                                    else cell.AlignLeft();
                                }

                                void AddDataCellNumber(double? value)
                                {
                                    AddDataCell((value ?? 0).ToString());
                                }

                                table.Cell().Border(1).BorderColor(Colors.Black)
                               .PaddingVertical(2).PaddingHorizontal(2)
                               .AlignCenter().AlignMiddle()
                               .Text(item.MaBenhEdit ?? "").FontSize(7);
                                AddDataCell(item.TenBenhEdit ?? "", false); // Tên bệnh
                                AddDataCellNumber(item.TongSo); // Tổng số
                                AddDataCellNumber(item.TrongDoNu); // Nữ
                                AddDataCellNumber(item.TrongDoDuoi15Tuoi); // < 15 tuổi
                                AddDataCellNumber(item.TrongDoDuoi6Tuoi); // < 6 tuổi
                                AddDataCellNumber(item.CachGiaiQuyetRaToa); // Ra toa
                                AddDataCellNumber(item.CachGiaiQuyetVaoVien); // Vào viện
                                AddDataCellNumber(item.CachGiaiQuyetNgoaiTru); // Ngoại trú
                                AddDataCellNumber(item.CachGiaiQuyetChuyenVien); // Chuyển viện
                                AddDataCellNumber(item.CachGiaiQuyetHenTaiKham); // Hẹn tái khám
                                AddDataCellNumber(item.CachGiaiQuyetKhac); // Khác
                            }
                        }

                     
                    });

                    contentCol.Item().PaddingTop(20).AlignRight().Width(200).Column(nguoiLapCol =>
                    {
                        nguoiLapCol.Item().AlignCenter().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}").Italic().FontSize(8);
                        nguoiLapCol.Item().PaddingTop(5).AlignCenter().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(8);

                        if (!string.IsNullOrEmpty(_tenNhanVien))
                        {
                            nguoiLapCol.Item().PaddingTop(30).AlignCenter().Text(_tenNhanVien).Bold().FontSize(8);
                        }
                        else
                        {
                            nguoiLapCol.Item().PaddingTop(30).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(7);
                        }
                    });
                });

                page.Footer()
                    .AlignRight()
                    .Text(x =>
                    {
                        x.Span("Trang ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span(" / ").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
            });
        }
    }
}

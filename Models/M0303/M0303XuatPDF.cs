//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using System.Collections.Generic;

//namespace Nam_ThongKeSoLuongBNHenTaiKham.Models
//{
//    public class NamMau2ListPDF : IDocument
//    {
//        private readonly List<Nam_Mau3> _data;
//        private readonly DateTime? _tuNgay;
//        private readonly DateTime? _denNgay;
//        private readonly string _logoPath;
//        public NamMau2ListPDF(List<Nam_Mau3> data, DateTime? tuNgay, DateTime? denNgay, string logoPath)
//        {
//            _data = data;
//            _tuNgay = tuNgay;
//            _denNgay = denNgay;
//            _logoPath = logoPath;
//        }

//        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;


//        public void Compose(IDocumentContainer container)
//        {
//            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
//            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";
//            int tongSoHoaDon = _data.Count;

//            // === TRANG 1: BẢNG DỮ LIỆU ===
//            container.Page(page =>
//            {
//                page.Size(PageSizes.A4.Landscape());
//                page.Margin(15);
//                page.PageColor(Colors.White);
//                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10).FontColor(Colors.Black));

//                // === HEADER: chỉ trang đầu ===
//                page.Header().ShowOnce().Column(headerCol =>
//                {
//                    headerCol.Item().Row(row =>
//                    {
//                        row.RelativeColumn().Column(col =>
//                        {
//                            if (File.Exists(_logoPath))
//                            {
//                                col.Item().Height(40).Image(_logoPath, ImageScaling.FitHeight);
//                            }
//                            else
//                            {
//                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(9);
//                            }

//                            col.Item().Text("ĐẠI HỌC Y DƯỢC THÀNH PHỐ HỒ CHÍ MINH").Bold().FontSize(12);
//                            col.Item().Text("PHÒNG KHÁM CHUYÊN KHOA RĂNG – HÀM – MẶT").Bold().FontSize(11);
//                            col.Item().Text("652 Nguyễn Trãi, P.11, Q.5, TP HCM").FontSize(9);
//                            col.Item().Text("Số 2 Phù Đổng Thiên Vương, P.11, Q.5, TP HCM").FontSize(9);
//                            col.Item().Text("Điện thoại: 028 3855 9225").FontSize(9);
//                        });

//                        row.RelativeColumn().Column(col =>
//                        {
//                            col.Item().AlignRight().Text("BẢNG THỐNG KÊ SỐ LƯỢNG BN TÁI KHÁM")
//                                .Bold().FontSize(14).FontColor(Colors.Black);
//                            col.Item().AlignRight().Text("Đơn vị điều trị dịch vụ")
//                                .Italic().FontSize(10);
//                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}")
//                                .FontSize(9).FontColor(Colors.Black);
//                        });
//                    });

//                    headerCol.Item().PaddingVertical(6).LineHorizontal(1)
//                        .LineColor(Colors.Grey.Darken2);
//                });

//                // === NỘI DUNG: BẢNG DỮ LIỆU ===
//                page.Content().Column(contentCol =>
//                {
//                    contentCol.Item().Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.ConstantColumn(25);
//                            columns.RelativeColumn();
//                            columns.RelativeColumn(1.5f);
//                            columns.ConstantColumn(50);
//                            columns.ConstantColumn(50);
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn();
//                            columns.RelativeColumn(1.5f);
//                            columns.ConstantColumn(50);
//                            columns.RelativeColumn(1.5f);
//                            columns.ConstantColumn(40);
//                        });

//                        table.Header(header =>
//                        {
//                            void AddHeaderCell(string text)
//                            {
//                                header.Cell()
//                                    .Border(1).BorderColor(Colors.Grey.Medium)
//                                    .Background(Colors.Grey.Lighten4)
//                                    .PaddingVertical(4).PaddingHorizontal(3)
//                                    .AlignCenter().AlignMiddle()
//                                    .Text(text).Bold().FontSize(9);
//                            }

//                            AddHeaderCell("STT");
//                            AddHeaderCell("Mã y tế");
//                            AddHeaderCell("Họ và tên");
//                            AddHeaderCell("Năm sinh");
//                            AddHeaderCell("Giới tính");
//                            AddHeaderCell("Quốc tịch");
//                            AddHeaderCell("CCCD/Passport");
//                            AddHeaderCell("SĐT");
//                            AddHeaderCell("Ngày hẹn");
//                            AddHeaderCell("Bác sĩ");
//                            AddHeaderCell("Nhắc hẹn");
//                            AddHeaderCell("Ghi chú");
//                            AddHeaderCell("IDCN");
//                        });

//                        int stt = 1;
//                        foreach (var item in _data)
//                        {
//                            void AddDataCell(string text, bool center = false)
//                            {
//                                var cell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1)
//                                    .PaddingVertical(2).PaddingHorizontal(3)
//                                    .AlignMiddle().Text(text ?? "").FontSize(8).WrapAnywhere();

//                                if (center) cell.AlignCenter();
//                            }

//                            AddDataCell(stt++.ToString(), true);
//                            AddDataCell(item.MaYTe ?? "", true);
//                            AddDataCell(item.HoVaTen ?? "");
//                            AddDataCell(item.NamSinh?.ToString() ?? "", true);
//                            AddDataCell(item.GioiTinh ?? "", true);
//                            AddDataCell(item.QuocTich ?? "");
//                            AddDataCell(item.CCCD_PASSPORT ?? "");
//                            AddDataCell(item.SDT ?? "");
//                            AddDataCell(item.NgayHenKham?.ToString("dd/MM/yyyy") ?? "", true);
//                            AddDataCell(item.BacSiHenKham ?? "");
//                            AddDataCell(item.NhacHen ?? "", true);
//                            AddDataCell(item.GhiChu ?? "");
//                            AddDataCell(item.IDCN?.ToString() ?? "", true);
//                        }
//                    });
//                });

//                // === FOOTER ===
//                page.Footer()
//                    .AlignRight()
//                    .Text(x =>
//                    {
//                        x.Span("Trang ").FontSize(9);
//                        x.CurrentPageNumber().FontSize(9);
//                        x.Span(" / ").FontSize(9);
//                        x.TotalPages().FontSize(9);
//                    });
//            });

//            // === TRANG 2: TỔNG KẾT & CHỮ KÝ ===
//            container.Page(page =>
//            {
//                page.Size(PageSizes.A4.Landscape());
//                page.Margin(15);
//                page.PageColor(Colors.White);
//                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10).FontColor(Colors.Black));

//                page.Content().MinimalBox().ShowEntire().Column(col =>
//                {
//                    col.Item().Text($"Tổng số hoá đơn: {tongSoHoaDon}").FontSize(9);

//                    col.Item().Row(row =>
//                    {
//                        row.RelativeColumn().Text("");
//                        row.RelativeColumn().Text("");
//                        row.RelativeColumn().Text("");
//                        row.RelativeColumn().AlignCenter().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
//                            .FontSize(9).Italic();
//                    });

//                    col.Item().PaddingTop(5).Row(row =>
//                    {
//                        row.RelativeColumn().Column(c =>
//                        {
//                            c.Item().Text("THỦ TRƯỞNG ĐƠN VỊ").Bold().FontSize(9);
//                            c.Item().Text("(Ký, họ tên, đóng dấu)").Italic().FontSize(8);
//                        });

//                        row.RelativeColumn().Column(c =>
//                        {
//                            c.Item().Text("THỦ QUỸ").Bold().FontSize(9);
//                            c.Item().Text("(Ký, họ tên)").Italic().FontSize(8);
//                        });

//                        row.RelativeColumn().Column(c =>
//                        {
//                            c.Item().Text("KẾ TOÁN").Bold().FontSize(9);
//                            c.Item().Text("(Ký, họ tên)").Italic().FontSize(8);
//                        });

//                        row.RelativeColumn().Column(c =>
//                        {
//                            c.Item().AlignCenter().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(9);
//                            c.Item().AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
//                        });
//                    });
//                });

//                page.Footer()
//                    .AlignRight()
//                    .Text(x =>
//                    {
//                        x.Span("Trang ").FontSize(9);
//                        x.CurrentPageNumber().FontSize(9);
//                        x.Span(" / ").FontSize(9);
//                        x.TotalPages().FontSize(9);
//                    });
//            });
//        }





//    }
//}


using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303XuatPDF : IDocument
    {
        private readonly List<M0303TKSoLuongBNHenKham> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;


        public M0303XuatPDF(List<M0303TKSoLuongBNHenKham> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, dynamic thongTinDoanhNghiep)
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
            int tongSoHoaDon = _data.Count;

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10).FontColor(Colors.Black));

                // === HEADER: chỉ trang đầu ===
                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(60).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                            {
                                col.Item().Height(40).Image(_logoPath, ImageScaling.FitHeight);
                            }
                            else
                            {
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(9);
                            }
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep.TenCSKCB).Bold().FontSize(12);
                            col.Item().Text(_thongTinDoanhNghiep.DiaChi).FontSize(9);
                            col.Item().Text("Điện thoại: " + _thongTinDoanhNghiep.DienThoai).FontSize(9);
                        });


                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("BẢNG THỐNG KÊ SỐ LƯỢNG BN TÁI KHÁM")
                                .Bold().FontSize(14).FontColor(Colors.Black);
                            col.Item().AlignRight().Text("Đơn vị điều trị dịch vụ")
                                .Italic().FontSize(10);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}")
                                .FontSize(9).FontColor(Colors.Black);
                        });
                    });

                    headerCol.Item().PaddingVertical(6).LineHorizontal(1)
                        .LineColor(Colors.Grey.Darken2);
                });

                // === NỘI DUNG: BẢNG DỮ LIỆU ===
                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn();
                            columns.RelativeColumn(1.5f);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(1.5f);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            void AddHeaderCell(string text)
                            {
                                header.Cell()
                                    .Border(1).BorderColor(Colors.Grey.Medium)
                                    .Background(Colors.Grey.Lighten4)
                                    .PaddingVertical(4).PaddingHorizontal(3)
                                    .AlignCenter().AlignMiddle()
                                    .Text(text).Bold().FontSize(9);
                            }

                            AddHeaderCell("STT");
                            AddHeaderCell("Mã y tế");
                            AddHeaderCell("Họ và tên");
                            AddHeaderCell("Năm sinh");
                            AddHeaderCell("Giới tính");
                            AddHeaderCell("Quốc tịch");
                            AddHeaderCell("CCCD/Passport");
                            AddHeaderCell("SĐT");
                            AddHeaderCell("Ngày hẹn");
                            AddHeaderCell("Bác sĩ");
                            AddHeaderCell("Nhắc hẹn");
                            AddHeaderCell("Ghi chú");
                        });

                        int stt = 1;
                        foreach (var item in _data)
                        {
                            void AddDataCell(string text, bool center = false)
                            {
                                var cell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(2).PaddingHorizontal(3)
                                    .AlignMiddle().Text(text ?? "").FontSize(8).WrapAnywhere();

                                if (center) cell.AlignCenter();
                            }

                            AddDataCell(stt++.ToString(), true);
                            AddDataCell(item.MaYTe ?? "", true);
                            AddDataCell(item.HoVaTen ?? "");
                            AddDataCell(item.NamSinh?.ToString() ?? "", true);
                            AddDataCell(item.GioiTinh ?? "", true);
                            AddDataCell(item.QuocTich ?? "");
                            AddDataCell(item.CCCD_PASSPORT ?? "");
                            AddDataCell(item.SDT ?? "");
                            AddDataCell(item.NgayHenKham?.ToString("dd-MM-yyyy") ?? "", true);
                            AddDataCell(item.BacSiHenKham ?? "");
                            AddDataCell(item.NhacHen ?? "", true);
                            AddDataCell(item.GhiChu ?? "");
                            
                        }
                    });

                    // === PHẦN TỔNG KẾT VÀ CHỮ KÝ - ĐẢM BẢO LUÔN DÍNH LIỀN ===
                    // === PHẦN TỔNG KẾT VÀ CHỮ KÝ - ĐẢM BẢO CÂN ĐỐI 2 BÊN ===
                    contentCol.Item().PaddingTop(10).ShowEntire().Column(summaryCol =>
                    {
                        // Phần ngày tháng - căn giữa
                        summaryCol.Item().Row(row =>
                        {
                            row.RelativeColumn(6); // Giảm từ 3 xuống 2 cột trống
                            row.RelativeColumn().AlignCenter().Column(c =>
                            {
                                c.Item().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
                                   .FontSize(9).Italic();
                                c.Item().PaddingBottom(5);
                            });
                        });

                        // Bảng chữ ký - co vào 2 bên
                        summaryCol.Item().PaddingHorizontal(20).Row(row => // Thêm padding 2 bên
                        {
                            // Cột Thủ trưởng - thêm padding phải
                            row.RelativeColumn().AlignLeft().PaddingRight(10).Column(c =>
                            {
                                c.Item().Text("THỦ TRƯỞNG ĐƠN VỊ").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên, đóng dấu)").Italic().FontSize(8);
                            });

                            // Cột Thủ quỹ
                            row.RelativeColumn().AlignCenter().PaddingHorizontal(5).Column(c =>
                            {
                                c.Item().Text("THỦ QUỸ").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });
                            
                            // Cột Kế toán
                            row.RelativeColumn().AlignCenter().PaddingHorizontal(5).Column(c =>
                            {
                                c.Item().Text("KẾ TOÁN").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });

                            // Cột Người lập bảng - thêm padding trái
                            row.RelativeColumn().AlignRight().PaddingLeft(10).Column(c =>
                            {
                                c.Item().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(9);
                                c.Item().PaddingTop(6).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(8);
                            });
                        });
                    });
                });

                // === FOOTER ===
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

using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303TKSoLuongBNHenKham : IDocument
    {
        private readonly List<M0303TKSoLuongBNHenKhamSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;


        public P0303TKSoLuongBNHenKham(List<M0303TKSoLuongBNHenKhamSTO> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, M0303ThongTinDoanhNghiep thongTinDoanhNghiep)
        {
            _data = data ?? new List<M0303TKSoLuongBNHenKhamSTO>();
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep ?? new M0303ThongTinDoanhNghiep
            {
                TenCSKCB = "Tên đơn vị",
                DiaChi = "",
                DienThoai = ""
            };
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

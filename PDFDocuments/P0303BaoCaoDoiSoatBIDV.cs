using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoDoiSoatBIDV : IDocument
    {
        private readonly List<M0303BaoCaoDoiSoatBIDV> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;


        public P0303BaoCaoDoiSoatBIDV(List<M0303BaoCaoDoiSoatBIDV> data, DateTime? tuNgay, DateTime? denNgay, string logoPath, dynamic thongTinDoanhNghiep)
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
                            col.Item().AlignRight().Text("BẢNG BÁO CÁO ĐỐI SOÁT BIDV")
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
                            columns.RelativeColumn(1.9f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(3.8f);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(1.8f);
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(1.9f);
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(4.3f);
                            columns.RelativeColumn(2.5f);

                            
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(1.8f);
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(3.0f);
                        });


                        table.Header(header =>
                        {
                            void H(string text, uint rowSpan = 1, uint colSpan = 1, bool isGroup = false)
                            {
                                header.Cell().RowSpan(rowSpan).ColumnSpan(colSpan).Element(c =>
                                {
                                    c.Border(1).BorderColor(Colors.Grey.Medium)
                                     .Background(isGroup ? Colors.Grey.Lighten2 : Colors.Grey.Lighten2)
                                     .PaddingVertical(4).PaddingHorizontal(3)
                                     .AlignCenter().AlignMiddle()
                                     .Text(text).Bold().FontSize(isGroup ? 9 : 8);
                                });
                            }

                            H("STT", 2);
                            H("Mã y tế", 2);
                            H("Mã đợt", 2);
                            H("Họ tên BN", 2);
                            H("SĐT", 2);
                            H("Tiền BL", 2);
                            H("Số BL", 2);
                            H("Tiền HĐ", 2);
                            H("Số HĐ", 2);
                            H("Tổng tiền", 2);
                            H("Ngày GD", 2);
                            H("User TT", 2);
                            H("BVUB", 1, 2, true);
                            H("BIDV", 1, 2, true);

                           
                            H("Số tiền");       
                            H("Trạng thái");    
                            H("Số tiền");       
                            H("Trạng thái");    
                        });




                        int stt = 1;
                        decimal tongBVUB = 0;
                        decimal tongBIDV = 0;
                        foreach (var item in _data)
                        {
                            void AddDataCell(object value, bool center = false, bool isNumber = false, bool isPhone = false, bool alignRight = false)
                            {
                                var text = value?.ToString() ?? "";
                                if (isNumber && decimal.TryParse(text, out var number))
                                {
                                    text = number.ToString("#,##0");
                                }

                                var cell = table.Cell().Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(2).PaddingHorizontal(3)
                                    .AlignMiddle().Text(text).FontSize(8);

                                if (isPhone)
                                {
                                    cell.WrapAnywhere(false);
                                }
                                else
                                {
                                    cell.WrapAnywhere();
                                }

                                if (alignRight)
                                {
                                    cell.AlignRight();
                                }
                                else if (center)
                                {
                                    cell.AlignCenter();
                                }
                            }

                            AddDataCell(stt++, true);
                            AddDataCell(item.MaYTe, true);
                            AddDataCell(item.MaDot, true);
                            AddDataCell(item.HoTenBenhNhan);
                            AddDataCell(item.SoDienThoai, true, false, true);
                            AddDataCell(item.SoTienTrenBL, false, true, false, true);
                            AddDataCell(item.SoBL, true);
                            AddDataCell(item.SoTienTrenHD, false, true, false, true);
                            AddDataCell(item.SoHD, true);
                            AddDataCell(item.TongSoTien, false, true, false, true);
                            AddDataCell(item.NgayGioGiaoDich?.ToString("dd/MM/yyyy HH:mm:ss"), true);
                            AddDataCell(item.UserThanhToan, false, false);
                            AddDataCell(item.BVUB_SoTien, false, true, false, true);
                            AddDataCell(item.BVUB_TrangThai, true);
                            AddDataCell(item.BIDV_SoTien, false, true, false, true);
                            AddDataCell(item.BIDV_TrangThai, true);


                            if (decimal.TryParse(item.BVUB_SoTien?.ToString(), out var bvub))
                                tongBVUB += bvub;
                            if (decimal.TryParse(item.BIDV_SoTien?.ToString(), out var bidv))
                                tongBIDV += bidv;
                        }

                        table.Footer(footer =>
                        {
   
                            void FooterCell(string text, bool isBold = true, bool alignRight = false, uint colSpan = 1)
                            {
                                var cell = footer.Cell().ColumnSpan(colSpan).Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(2).PaddingHorizontal(3)
                                    .AlignMiddle().Text(text).FontSize(8);

                                if (isBold) cell.Bold();
                                if (alignRight) cell.AlignRight();
                            }

                        
                            FooterCell("Tổng cộng:", true, true, 12);
                            FooterCell(tongBVUB.ToString("#,##0"), true, true);
                            FooterCell("", false); 
                            FooterCell(tongBIDV.ToString("#,##0"), true, true);
                            FooterCell("", false);
                        });


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

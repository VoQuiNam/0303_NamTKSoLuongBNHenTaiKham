using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{

    public class P0303PhieuNhapKhoInGop : IDocument
    {
        private readonly List<M0303PhieuNhapKhoSTO> _data;
        //private readonly DateTime? _ngayGioNhap;
        //private readonly long? _IdKhoHang;
        //private readonly long? _IdNhaCungCap;
        //private readonly long? _IDHangHoa;
        //private readonly long? _IDDonViTinhNhap;
        private readonly long? _IdPhieuNhapKho;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;

        public P0303PhieuNhapKhoInGop(
    List<M0303PhieuNhapKhoSTO> data,
    long IdPhieuNhapKho,
    string logoPath,
    M0303ThongTinDoanhNghiep thongTinDoanhNghiep)
        {
            _data = data;
            _IdPhieuNhapKho = IdPhieuNhapKho;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var ngayChungTuStr = _data.FirstOrDefault()?.NgayGioNhap?.ToString("dd/MM/yyyy")
                      ?? DateTime.Now.ToString("dd/MM/yyyy");

            var tongSoKhoan = _data.Count;
            var tongTien = (double)(_data.Sum(x => x.ThanhTien ?? 0));

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(15);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10).FontColor(Colors.Black));

                page.Header().ShowOnce().Column(headerCol =>
                {
                    // Logo và thông tin bệnh viện
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
                            col.Item().AlignLeft().PaddingLeft(10).Text(_thongTinDoanhNghiep.TenCSKCB).Bold().FontSize(12);
                            col.Item().AlignLeft().PaddingLeft(10).Text(_thongTinDoanhNghiep.DiaChi).FontSize(9);
                            col.Item().AlignLeft().PaddingLeft(10).Text("Điện thoại: " + (_thongTinDoanhNghiep?.DienThoai ?? "")).FontSize(9);
                        });

                        // Cột 3: Nội dung "Mẫu số C30-HD" (bên phải)
                        row.RelativeColumn().AlignRight().Column(col =>
                        {
                            col.Item().Text("Mẫu số C30- HD").AlignCenter().Bold().FontSize(10);
                            col.Item().Text("(Ban hành kèm thông tư 107/2017/TT-BTC\n24/11/2017)").Italic().FontSize(7);

                          
                        });
                    });

                    // Tiêu đề phiếu nhập kho
                    headerCol.Item()
                        .PaddingVertical(5)
                        .AlignCenter()
                        .Text("PHIẾU NHẬP KHO")
                        .Bold()
                        .FontSize(14);

                    // Ngày chứng từ
                    // Lấy tất cả các ngày chứng từ phân cách bằng dấu phẩy
                    var allDates = string.Join(", ", _data.Select(x => x.NgayGioNhap?.ToString("dd 'tháng' MM 'năm' yyyy")).Distinct());

                    headerCol.Item()
                        .AlignCenter()
                        .Text($"Ngày chứng từ: {allDates}")
                        .FontSize(10)
                        .Italic();

                    // Ngày chứng từ và Nợ/Có
                    headerCol.Item().Row(row =>
                    {
                        // Đưa Nợ/Có sang phải
                        row.RelativeColumn().AlignRight().Column(col =>
                        {
                            col.Item().PaddingTop(5).Text("Nợ:........................").FontSize(8);
                            col.Item().Text("Có:........................").FontSize(8);
                        });
                    });

                    // Thông tin chung - THÊM PADDING TOP ĐỂ TẠO KHOẢNG CÁCH
                    headerCol.Item()
     .PaddingTop(10)
     .PaddingBottom(5)
     .DefaultTextStyle(TextStyle.Default.LineHeight(1.3f)) // Đặt ở container, không phải Table
     .Table(table =>
     {
         table.ColumnsDefinition(columns =>
         {
             columns.ConstantColumn(100);   // Cột nhãn
             columns.RelativeColumn();      // Cột dữ liệu
         });

         // Thêm mỗi cell với khoảng cách dòng
         table.Cell().Border(0).PaddingBottom(4).Text("- Nhập tại kho:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.TenKhoHang).Distinct()))
             .FontSize(10).Bold().AlignLeft();

         table.Cell().Border(0).PaddingBottom(4).Text("- Đơn vị giao hàng:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.TenNhaCungCap).Distinct()))
             .FontSize(10).Bold().AlignLeft();

         table.Cell().Border(0).PaddingBottom(4).Text("- Số chứng từ:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.SoPhieuNhap).Distinct()))
             .FontSize(10).Bold().AlignLeft();

         table.Cell().Border(0).PaddingBottom(4).Text("- Theo số HD:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.SoHoaDon).Distinct()))
             .FontSize(10).Bold().AlignLeft();

         table.Cell().Border(0).PaddingBottom(4).Text("- Ngày hoá đơn:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.NgayHoaDon?.ToString("dd/MM/yyyy")).Distinct()))
             .FontSize(10).Bold().AlignLeft();

         table.Cell().Border(0).PaddingBottom(4).Text("- Nội dung:").FontSize(10);
         table.Cell().Border(0).PaddingBottom(4).Text(string.Join(", ", _data.Select(x => x.NoiDung).Distinct()))
             .FontSize(10).Bold().AlignLeft();
     });


                    
                });

                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25); // STT
                            columns.RelativeColumn(3f);  // Tên, nhãn hiệu, quy cách
                            columns.RelativeColumn(1.5f); // Mã số
                            columns.ConstantColumn(40); // ĐVT
                            columns.RelativeColumn(1.2f); // Số lô
                            columns.RelativeColumn(1.2f); // Hạn dùng
                            columns.ConstantColumn(60); // Số lượng theo chứng từ
                            columns.ConstantColumn(60); // Số lượng thực nhập
                            columns.RelativeColumn(1.5f); // Đơn giá
                            columns.RelativeColumn(1.5f); // Thành tiền
                        });

                        table.Header(header =>
                        {
                            void HeaderCell(string text, bool center = true, uint colSpan = 1, uint rowSpan = 1, bool bold = true)
                            {
                                var cell = header.Cell()
                                    .ColumnSpan(colSpan)
                                    .RowSpan(rowSpan)
                                    .Border(1)
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(2)
                                    .AlignMiddle();

                                if (center)
                                    cell = cell.AlignCenter();
                                else
                                    cell = cell.AlignLeft();

                                var textStyle = cell.Text(text).FontSize(9);
                                if (bold) textStyle = textStyle.Bold(); // Chỉ in đậm nếu bold = true
                            }


                            // Dòng 1 - Các header chính (rowSpan = 2 cho các cột chỉ có 1 dòng)
                            HeaderCell("STT", true, 1, 2); // rowSpan = 2
                            HeaderCell("Tên, nhãn hiệu,\nquy cách, phẩm chất\nvật tư (SP, hàng hóa)", true, 1, 2); // rowSpan = 2
                            HeaderCell("Mã số", true, 1, 2); // rowSpan = 2
                            HeaderCell("ĐVT", true, 1, 2); // rowSpan = 2
                            HeaderCell("Số lô", true, 1, 2); // rowSpan = 2
                            HeaderCell("Hạn dùng", true, 1, 2); // rowSpan = 2

                            // Nhóm SỐ LƯỢNG - colspan = 2, rowSpan = 1 (chỉ chiếm 1 dòng)
                            HeaderCell("Số lượng", true, 2, 1); // colspan = 2, rowSpan = 1

                            HeaderCell("Đơn giá\n(VAT)", true, 1, 2); // rowSpan = 2
                            HeaderCell("Thành tiền\n(VAT)", true, 1, 2); // rowSpan = 2

                            // Dòng 2 - Chỉ có 2 cột con của nhóm Số lượng
                            HeaderCell("Theo\nchứng từ", true, 1, 1, false); // Không in đậm
                            HeaderCell("Thực\nnhập", true, 1, 1, false);     // Không in đậm
                        });

                        // Dữ liệu
                        int stt = 1;
                        foreach (var item in _data)
                        {
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(stt++.ToString()).FontSize(9);
                            table.Cell().Border(1).Padding(2).AlignMiddle().Text(item.TenHangHoa ?? "").FontSize(9);
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(item.MaHangHoa ?? "").FontSize(9);
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(item.TenDVT ?? "").FontSize(9);
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(item.SoLo ?? "").FontSize(9);
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle().Text(item.HanDung?.ToString("dd/MM/yyyy") ?? "").FontSize(9);

                            // Cột "Theo chứng từ" (Số lượng theo chứng từ)
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle()
                                .Text(Convert.ToDecimal(item.SoLuongNhap ?? 0).ToString()).FontSize(9);

                            // Cột "Thực nhập" (Số lượng thực nhập)
                            table.Cell().Border(1).Padding(2).AlignCenter().AlignMiddle()
                                .Text(Convert.ToDouble(item.SoLuongNhap ?? 0).ToString()).FontSize(9);

                            table.Cell().Border(1).Padding(2).AlignRight().AlignMiddle()
                                .Text(Convert.ToDouble(item.DonGiaNhap ?? 0).ToString("#,##0.00")).FontSize(9);

                            table.Cell().Border(1).Padding(2).AlignRight().AlignMiddle()
                                .Text(Convert.ToDouble(item.ThanhTien ?? 0).ToString("#,##0")).FontSize(9);
                        }
                    });

                    // ĐƯA PHẦN "CỘNG KHOẢN" VÀ "TỔNG CỘNG" RA NGOÀI BẢNG (căn trái)
                    contentCol.Item().PaddingTop(10).Column(col =>
                    {
                        // ĐƯA PHẦN "CỘNG KHOẢN" VÀ "TỔNG CỘNG" RA NGOÀI BẢNG
                        contentCol.Item().PaddingTop(10).Row(row =>
                        {
                            // Phần bên trái: "Cộng khoản: X khoản."
                            row.RelativeColumn().AlignLeft().Text(text =>
                            {
                                text.Span("Cộng khoản: ").FontSize(9);         // Bình thường
                                text.Span($"{tongSoKhoan}").Bold().FontSize(9); // Chỉ số đậm
                                text.Span(" khoản.").FontSize(9);              // Bình thường
                            });


                            // Phần bên phải: "TỔNG CỘNG: X,XXX,XXX"
                            row.RelativeColumn().AlignRight().Text($"TỔNG CỘNG: {tongTien.ToString("#,##0")}").Bold().FontSize(9);
                        });

                        // Tổng số tiền bằng chữ (nằm ngay bên dưới, căn trái)
                        contentCol.Item().PaddingTop(5).Text("Tổng số tiền (Bằng chữ): " + ConvertToWords(tongTien))
                            .Italic().FontSize(10);
                    });

                    // Chữ ký
                    contentCol.Item().PaddingTop(20).Table(signatureTable =>
                    {
                        signatureTable.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(); // Cột trái: Kế toán
                            columns.RelativeColumn(); // Cột giữa: Thủ kho
                            columns.RelativeColumn(); // Cột phải: Trưởng khoa
                        });

                        // Dòng 1: Ngày tháng năm (chỉ ở cột phải)
                        signatureTable.Cell().AlignCenter().Text(""); // Cột trái: trống
                        signatureTable.Cell().AlignCenter().Text(""); // Cột giữa: trống
                                                                      // Dòng 1: Ngày tháng năm (chỉ ở cột phải)
                        signatureTable.Cell()
                         .AlignCenter()
                         .PaddingBottom(7) // Đặt khoảng cách trước khi thêm Text
                         .Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
                         .FontSize(10);



                        // Dòng 2: Chức danh
                        signatureTable.Cell().AlignCenter().Text("Kế toán").Bold().FontSize(10);
                        signatureTable.Cell().AlignCenter().Text("Thủ kho").Bold().FontSize(10);
                        signatureTable.Cell().AlignCenter().Text("Trưởng khoa dược/VTYT").Bold().FontSize(10);

                        // Dòng 3: Chữ ký
                        signatureTable.Cell().AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(9);
                        signatureTable.Cell().AlignCenter().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                        signatureTable.Cell().AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(9);
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

        public static string ConvertToWords(double number)
        {
            if (number == 0) return "Không đồng";
            return ConvertNumberToWordsVietnamese(number) + " đồng";
        }

        private static string ConvertNumberToWordsVietnamese(double number)
        {
            string[] so = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] hang = { "", "nghìn", "triệu", "tỷ" };

            var sb = new System.Text.StringBuilder();
            string s = ((long)number).ToString();
            int i = 0;
            bool am = false;

            if (s.StartsWith("-"))
            {
                am = true;
                s = s.Substring(1);
            }

            int j = 0;
            while (s.Length > 0)
            {
                int len = s.Length;
                int so3 = int.Parse(s.Substring(Math.Max(0, len - 3), len >= 3 ? 3 : len));
                s = s.Substring(0, Math.Max(0, len - 3));

                if (so3 > 0 || (i == 3 && sb.Length > 0))
                {
                    string chu = DocSo3ChuSo(so3, so);
                    if (!string.IsNullOrEmpty(chu))
                    {
                        if (sb.Length > 0) sb.Insert(0, " ");
                        sb.Insert(0, chu + " " + hang[i]);
                    }
                }
                i++;
                j++;
            }

            string ketQua = sb.ToString().Trim();
            if (am) ketQua = "Âm " + ketQua;
            ketQua = char.ToUpper(ketQua[0]) + ketQua.Substring(1);
            return ketQua;
        }

        private static string DocSo3ChuSo(int so3, string[] so)
        {
            int tram = so3 / 100;
            int chuc = (so3 % 100) / 10;
            int donvi = so3 % 10;

            var sb = new System.Text.StringBuilder();

            if (tram > 0)
            {
                sb.Append(so[tram] + " trăm");
                if (chuc == 0 && donvi > 0) sb.Append(" linh");
            }

            if (chuc > 0)
            {
                if (sb.Length > 0) sb.Append(" ");
                if (chuc == 1)
                    sb.Append("mười");
                else
                    sb.Append(so[chuc] + " mươi");
            }

            if (donvi > 0)
            {
                if (sb.Length > 0) sb.Append(" ");
                if (donvi == 1 && chuc > 1)
                    sb.Append("mốt");
                else if (donvi == 5 && chuc >= 1)
                    sb.Append("lăm");
                else
                    sb.Append(so[donvi]);
            }

            return sb.ToString();
        }

    }
}

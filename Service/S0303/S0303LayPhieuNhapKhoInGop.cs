using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Fluent;
namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303LayPhieuNhapKhoInGop : I0303LayPhieuNhapKhoInGop
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303LayPhieuNhapKhoInGop(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }


        public async Task<IActionResult> ExportToPDF(long idPhieuNhapKho, int? idChiNhanh)
        {
            try
            {
                // 1. Lấy dữ liệu phiếu nhập kho từ stored procedure
                var data = await _localDb.M0303PhieuNhapKhoSTOs
                    .FromSqlInterpolated($@"
                EXEC [dbo].[S0303_PhieuNhapKho]
                    @IDPhieuNhapKho = {idPhieuNhapKho},
                    @IdChiNhanh = {idChiNhanh}
            ")
                    .ToListAsync();

                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất PDF");

                // 2. Lấy thông tin doanh nghiệp theo chi nhánh
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
     .AsNoTracking()
     .Where(x => idChiNhanh.HasValue && x.IDChiNhanh == idChiNhanh.Value)
     .Select(x => new M0303ThongTinDoanhNghiep
     {
         TenCSKCB = x.TenCSKCB ?? "",
         DiaChi = x.DiaChi ?? "",
         DienThoai = x.DienThoai ?? "",
         MaCSKCB = x.MaCSKCB ?? ""
     }).FirstOrDefaultAsync() ?? new M0303ThongTinDoanhNghiep();


                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                // 3. Tạo PDF bằng QuestPDF
                var document = new P0303PhieuNhapKhoInGop(
                    data,
                    idPhieuNhapKho, // truyền thêm nếu cần cho tiêu đề hoặc footer
                    logoPath,
                    thongTinDoanhNghiep
                );

                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"PhieuNhapKho_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi tạo PDF: {ex.Message}") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> ExportExcel(long idPhieuNhapKho, int? idChiNhanh)
        {
            try
            {
                // 1️⃣ Lấy dữ liệu phiếu nhập kho
                var data = await _localDb.M0303PhieuNhapKhoSTOs
                    .FromSqlInterpolated($@"
                EXEC [dbo].[S0303_PhieuNhapKho]
                    @IDPhieuNhapKho = {idPhieuNhapKho},
                    @IdChiNhanh = {idChiNhanh}
            ").ToListAsync();

                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất Excel");

                // 2️⃣ Lấy thông tin doanh nghiệp (bệnh viện)
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => idChiNhanh.HasValue && x.IDChiNhanh == idChiNhanh.Value)
                    .Select(x => new M0303ThongTinDoanhNghiep
                    {
                        TenCSKCB = x.TenCSKCB ?? "",
                        DiaChi = x.DiaChi ?? "",
                        DienThoai = x.DienThoai ?? "",
                        MaCSKCB = x.MaCSKCB ?? ""
                    }).FirstOrDefaultAsync();

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("PHIẾU NHẬP KHO");

                int row = 1;

                // 3️⃣ Logo + thông tin bệnh viện
                if (System.IO.File.Exists(logoPath))
                {
                    ws.AddPicture(logoPath)
                        .MoveTo(ws.Cell(row, 1))
                        .Scale(0.08);
                }

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 45;
                ws.Column(8).Width = 20;

                ws.Cell(row, 2).Value = thongTinDoanhNghiep?.TenCSKCB ?? "";
                ws.Cell(row, 2).Style.Font.Bold = true;
                ws.Cell(row, 2).Style.Font.FontSize = 11;
                ws.Cell(row, 2).Style.Font.FontName = "Times New Roman";

                // Merge cột G, H, I cho "Mẫu số C30 - HD"
                ws.Range(row, 8, row, 10).Merge();
                ws.Cell(row, 8).Value = "Mẫu số C30 - HD";
                ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, 8).Style.Font.FontSize = 10;
                ws.Cell(row, 8).Style.Font.Italic = true;
                ws.Cell(row, 8).Style.Font.Bold = true;


                row++;
                ws.Cell(row, 2).Value = thongTinDoanhNghiep?.DiaChi ?? "";
                ws.Cell(row, 2).Style.Font.FontSize = 10;

                // Thông tư nằm ngay bên phải hàng địa chỉ
                ws.Cell(row, 10).Value = "(Ban hành kèm theo Thông tư 107/2017/TT-BTC ngày 24/11/2017)";
                ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 10).Style.Font.FontSize = 10;
                ws.Cell(row, 10).Style.Font.Italic = true;

                row++;
                ws.Cell(row, 2).Value = $"ĐT: {thongTinDoanhNghiep?.DienThoai ?? ""}";
                ws.Cell(row, 2).Style.Font.FontSize = 10;

                // 4️⃣ Tiêu đề phiếu
                row += 2;
                ws.Range(row, 1, row, 9).Merge();
                ws.Cell(row, 1).Value = "PHIẾU NHẬP KHO";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 16;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                ws.Range(row, 1, row, 9).Merge();

                var ngay = data.First().NgayGioNhap;
                if (ngay != null)
                {
                    ws.Cell(row, 1).Value = $"Ngày chứng từ: {ngay.Value:dd} tháng {ngay.Value:MM} năm {ngay.Value:yyyy}";
                }
                else
                {
                    ws.Cell(row, 1).Value = "Ngày ..... tháng ..... năm ......";
                }

                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 1).Style.Font.FontSize = 11;
                ws.Cell(row, 1).Style.Font.FontName = "Times New Roman";
                row++;
                ws.Cell(row, 10).Value = "Nợ: .........................";
                ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 10).Style.Font.FontSize = 11;

                row++;
                ws.Cell(row, 10).Value = "Có: .........................";
                ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 10).Style.Font.FontSize = 11;

                // Dòng trống trước phần thông tin kho
                row++;

                // 5️⃣ Thông tin chung
                row++;
                ws.Cell(row, 1).Value = "- Nhập tại kho:";
                ws.Cell(row, 2).Value = data.First().TenKhoHang;
                ws.Cell(row, 2).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "- Đơn vị giao hàng:";
                ws.Cell(row, 2).Value = data.First().TenNhaCungCap;
                ws.Cell(row, 2).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "- Số chứng từ:";
                ws.Cell(row, 2).Value = data.First().SoPhieuNhap;
                ws.Cell(row, 2).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "- Theo số HĐ:";
                ws.Cell(row, 2).Value = data.First().SoHoaDon;
                ws.Cell(row, 2).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "- Ngày hóa đơn:";
                ws.Cell(row, 2).Value = data.First().NgayHoaDon?.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "- Nội dung:";
                ws.Cell(row, 2).Value = data.First().NoiDung;
                ws.Cell(row, 2).Style.Font.Bold = true;
                // 6️⃣ Dòng trống
                row += 2;

                // 7️⃣ Header bảng hàng hóa
                // 7️⃣ Header bảng hàng hóa
                double[] columnWidths = { 5, 35, 10, 8, 10, 12, 12, 12, 15, 50 };
                for (int i = 0; i < columnWidths.Length; i++)
                    ws.Column(i + 1).Width = columnWidths[i];

                // Header row 1
                var headerRow1 = ws.Row(row);
                headerRow1.Height = 25;

                // Đặt các header đơn giản không merge trước
                ws.Range(row, 1, row + 1, 1).Merge().Value = "STT";
                ws.Range(row, 2, row + 1, 2).Merge().Value = "Tên, nhãn hiệu, quy cách, phẩm chất vật tư";
                ws.Range(row, 3, row + 1, 3).Merge().Value = "Mã số";
                ws.Range(row, 4, row + 1, 4).Merge().Value = "ĐVT";
                ws.Range(row, 5, row + 1, 5).Merge().Value = "Số lô";
                ws.Range(row, 6, row + 1, 6).Merge().Value = "Hạn dùng";

                // Merge cột Số lượng nhập (7 và 8)
                ws.Range(row, 7, row, 8).Merge().Value = "Số lượng nhập";

                // Các header còn lại
                ws.Range(row, 9, row + 1, 9).Merge().Value = "Đơn giá (VAT)";
                ws.Range(row, 10, row + 1, 10).Merge().Value = "Thành tiền (VAT)";

                // Áp dụng style cho tất cả header
                for (int col = 1; col <= 10; col++)
                {
                    var range = col == 7 ? ws.Range(row, 7, row, 8) :
                                col >= 7 && col <= 8 ? null : // Bỏ qua cột 8 đã merge
                                ws.Range(row, col, row + 1, col);

                    if (range != null)
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.FontColor = XLColor.Black;
                        range.Style.Fill.BackgroundColor = XLColor.LightGray;
                        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                // Header row 2 - chỉ cho cột Số lượng
                row++;
                var headerRow2 = ws.Row(row);
                headerRow2.Height = 25;

                string[] subHeaders = { "Theo chứng từ", "Thực nhập" };
                for (int i = 0; i < subHeaders.Length; i++)
                {
                    var col = 7 + i;
                    var cell = ws.Cell(row, col);
                    cell.Value = subHeaders[i];
                    cell.Style.Font.FontColor = XLColor.Black;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // 8️⃣ Dòng dữ liệu
                int stt = 1;
                double tongCong = 0;
                foreach (var item in data)
                {
                    row++;
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.TenHangHoa;
                    ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, 3).Value = item.MaHangHoa;
                    ws.Cell(row, 4).Value = item.TenDVT;
                    ws.Cell(row, 5).Value = item.SoLo;
                    ws.Cell(row, 6).Value = item.HanDung?.ToString("dd/MM/yyyy");

                    // Cột số lượng với 2 cột con
                    ws.Cell(row, 7).Value = item.SoLuongNhap; // Theo chứng từ
                    ws.Cell(row, 8).Value = item.SoLuongNhap; // Thực nhập (có thể điều chỉnh theo logic của bạn)

                    ws.Cell(row, 9).Value = item.DonGiaNhap;
                    ws.Cell(row, 10).Value = item.ThanhTien;

                    tongCong += item.ThanhTien ?? 0;

                    for (int c = 1; c <= 10; c++)
                    {
                        ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(row, c).Style.Font.FontName = "Times New Roman";
                        ws.Cell(row, c).Style.Font.FontSize = 10;

                        // Căn chỉnh theo loại dữ liệu
                        if (c == 2)
                            ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // Tên vật tư căn trái
                        else if (c == 9 || c == 10)
                            ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; // Số lượng, đơn giá, thành tiền căn phải
                        else
                            ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    ws.Cell(row, 9).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0";
                }

                // 9️⃣ Tổng cộng
                row++;
                ws.Cell(row, 1).Value = "Cộng khoản:";
                ws.Cell(row, 1).Style.Font.Bold = false;

                ws.Cell(row, 2).Value = $"{data.Count} khoản";
                ws.Cell(row, 2).Style.Font.Bold = true;

                // Gộp phần còn lại để đẹp bố cục
                ws.Range(row, 3, row, 8).Merge();




                ws.Cell(row, 10).Value = $"TỔNG CỘNG: {tongCong:#,##0}";
                ws.Cell(row, 10).Style.Font.Bold = true;
                ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                // 🔟 Ghi chú + chữ ký
                row++;
                ws.Cell(row, 1).Value = "Tổng số tiền (Bằng chữ):";
                ws.Cell(row, 1).Style.Font.Italic = false;

                ws.Cell(row, 2).Value = ConvertToWords(tongCong);
                ws.Cell(row, 2).Style.Font.Italic = true;

                // Gộp lại cho đẹp (tùy layout)
                ws.Range(row, 2, row, 10).Merge();

                // Sau phần tổng số tiền (bằng chữ)
                row += 2;

                // 🧾 Dòng ngày tháng năm (ở cột phải)
                ws.Range(row, 8, row, 9).Merge();
                ws.Cell(row, 8).Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 8).Style.Font.FontSize = 10;
                ws.Cell(row, 8).Style.Font.FontName = "Times New Roman";

                row++;

                // 🧍‍♂️ Dòng chức danh
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 1).Value = "Kế toán";
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 1).Style.Font.Bold = true;

                ws.Range(row, 4, row, 6).Merge();
                ws.Cell(row, 4).Value = "Thủ kho";
                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 4).Style.Font.Bold = true;

                ws.Range(row, 7, row, 10).Merge();
                ws.Cell(row, 7).Value = "Trưởng khoa dược/VTYT";
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 7).Style.Font.Bold = true;

                row++;

                // 🖊️ Dòng ký tên
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 1).Value = "(Ký, họ tên)";
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Cell(row, 1).Style.Font.FontSize = 9;

                ws.Range(row, 4, row, 6).Merge();
                ws.Cell(row, 4).Value = "(Ký, ghi rõ họ tên)";
                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 4).Style.Font.Italic = true;
                ws.Cell(row, 4).Style.Font.FontSize = 9;

                ws.Range(row, 7, row, 10).Merge();
                ws.Cell(row, 7).Value = "(Ký, họ tên)";
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 7).Style.Font.Italic = true;
                ws.Cell(row, 7).Style.Font.FontSize = 9;


                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 15;
                ws.Column(5).Width = 10; // Cột "Theo chứng từ" - giữ nguyên hoặc điều chỉnh
                ws.Column(7).Width = 15; // Cột "Theo chứng từ" - giữ nguyên hoặc điều chỉnh
                ws.Column(8).Width = 10; // Cột "Thực nhập" - nhỏ hơn
                ws.Column(9).Width = 15;
                ws.Column(10).Width = 15;


                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"PhieuNhapKho_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi xuất Excel: {ex.Message}") { StatusCode = 500 };
            }
        }

        private string ConvertToWords(double number)
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
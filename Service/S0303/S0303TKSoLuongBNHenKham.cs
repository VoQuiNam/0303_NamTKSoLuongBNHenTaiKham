using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Context;
using Nam_ThongKeSoLuongBNHenTaiKham.Models;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;
using QuestPDF.Fluent;
using static Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303.C0303TKSoLuongBNHenTaiKhamController;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service
{
    public class S0303TKSoLuongBNHenKham : ControllerBase,I0303TKSoLuongBNHenKham
    {
        private readonly M0303AppDbContext _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303TKSoLuongBNHenKham(M0303AppDbContext localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }


        public async Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn)
        {
            var danhSach = _localDb.M0303Thongtinbnhenkhams.AsQueryable();

            if (tuNgay.HasValue && denNgay.HasValue)
            {
                danhSach = danhSach.Where(x => x.NgayHenKham.HasValue &&
                                               x.NgayHenKham.Value.Date >= tuNgay.Value.Date &&
                                               x.NgayHenKham.Value.Date <= denNgay.Value.Date);
            }

            if (idcn.HasValue && idcn.Value > 0)
            {
                danhSach = danhSach.Where(x => x.IDCN == idcn.Value);
            }

            var list = danhSach.ToList();

            var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                .AsNoTracking()
                .Where(x => x.IDChiNhanh == idcn)
                .Select(x => new M0303ThongTinDoanhNghiep
                {
                    TenCoQuanChuyenMon = "SỞ Y TẾ TP. HỒ CHÍ MINH",
                    TenCSKCB = x.TenCSKCB ?? "",
                    DiaChi = x.DiaChi ?? "",
                    DienThoai = x.DienThoai ?? "",
                    Email = x.Email ?? "",
                    Website = x.Website ?? "",
                    MaCSKCB = x.MaCSKCB ?? ""
                })
                .FirstOrDefaultAsync();

            thongTinDoanhNghiep ??= new M0303ThongTinDoanhNghiep
            {
                TenCoQuanChuyenMon = "SỞ Y TẾ TP. HỒ CHÍ MINH",
                TenCSKCB = "CƠ SỞ KHÁM CHỮA BỆNH CHƯA XÁC ĐỊNH"
            };

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Thống kê BN tái khám");

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:B4").Merge();
                    ws.Column(1).Width = 14;
                    ws.Column(2).Width = 5;

                    var img = ws.AddPicture(logoPath)
             .MoveTo(ws.Cell("A1"), 0, 10)
             .WithPlacement(XLPicturePlacement.FreeFloating)
             .Scale(0.2);

                }

                string tenCoQuan = thongTinDoanhNghiep.TenCoQuanChuyenMon;
                string tenCSKCB = thongTinDoanhNghiep.TenCSKCB;
                bool hienTenCSKCB = !string.Equals(tenCoQuan.Trim(), tenCSKCB.Trim(), StringComparison.OrdinalIgnoreCase);
                string diaChi = thongTinDoanhNghiep.DiaChi;
                string dienThoai = thongTinDoanhNghiep.DienThoai;

                ws.Range("C1:O1").Merge().Value = tenCoQuan;
                ws.Range("C1:O1").Style.Font.FontName = "Times New Roman";
                ws.Range("C1:O1").Style.Font.FontSize = 10;
                ws.Range("C1:O1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Row(1).Height = 25; 

                if (hienTenCSKCB)
                {
                    ws.Range("C2:O2").Merge().Value = tenCSKCB;
                    ws.Range("C2:O2").Style.Font.FontName = "Times New Roman";
                    ws.Range("C2:O2").Style.Font.FontSize = 10;
                    ws.Range("C2:O2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Row(2).Height = 20;
                }

                ws.Range("C3:O3").Merge().Value = diaChi;
                ws.Range("C3:O3").Style.Font.FontName = "Times New Roman";
                ws.Range("C3:O3").Style.Font.FontSize = 10;
                ws.Range("C3:O3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Row(3).Height = 20;


                ws.Range("C4:O4").Merge().Value = $"Điện thoại: {dienThoai}";
                ws.Range("C4:O4").Style.Font.FontName = "Times New Roman";
                ws.Range("C4:O4").Style.Font.FontSize = 10;
                ws.Range("C4:O4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Row(4).Height = 20;

                ws.Range("A6:M6").Merge().Value = "BẢNG THỐNG KÊ SỐ LƯỢNG BN TÁI KHÁM";
                ws.Range("A6:M6").Style.Font.Bold = true;
                ws.Range("A6:M6").Style.Font.FontSize = 24;
                ws.Range("A6:M6").Style.Font.FontName = "Times New Roman";
                ws.Range("A6:M6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A6:M6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(6).Height = 40;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd/MM/yyyy} đến ngày {denNgay.Value:dd/MM/yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:M7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:M7").Style.Font.Bold = true;
                ws.Range("A7:M7").Style.Font.FontSize = 12;
                ws.Range("A7:M7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 20;

                string[] headers = {
                    "STT", "Mã y tế", "Họ và tên", "Năm sinh", "Giới tính",
                    "Quốc tịch", "CCCD/Passport", "SĐT", "Ngày hẹn", "Bác sĩ",
                    "Nhắc hẹn", "Ghi chú"
                };


                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(9, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                int row = 10;
                int stt = 1;
                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.MaYTe;
                    ws.Cell(row, 3).Value = item.HoVaTen;
                    ws.Cell(row, 4).Value = item.NamSinh;
                    ws.Cell(row, 5).Value = item.GioiTinh;
                    ws.Cell(row, 6).Value = item.QuocTich;
                    ws.Cell(row, 7).Value = item.CCCD_PASSPORT;
                    ws.Cell(row, 8).Value = item.SDT;
                    ws.Cell(row, 9).Value = item.NgayHenKham?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 10).Value = item.BacSiHenKham;
                    ws.Cell(row, 11).Value = item.NhacHen;
                    ws.Cell(row, 12).Value = item.GhiChu;


                    int[] centerCols = { 1, 2, 4, 7, 8, 9 };
                    foreach (int col in centerCols)
                    {
                        ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    for (int col = 1; col <= 12; col++)
                    {
                        var cell = ws.Cell(row, col);
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    row++;
                }

                ws.Columns().AdjustToContents();
                ws.Rows().Height = 20;
                ws.Style.Font.FontSize = 11;
                ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int footerRow = row + 2;

                ws.Range($"K{footerRow}:L{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Range($"K{footerRow}:L{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"K{footerRow}:L{footerRow}").Style.Font.Italic = true;
                ws.Range($"K{footerRow}:L{footerRow}").Style.Font.FontSize = 10;


                string[] nguoiKy = { "THỦ TRƯỞNG ĐƠN VỊ", "THỦ QUỸ", "KẾ TOÁN", "NGƯỜI LẬP BẢNG" };
                string[] cotKyStart = { "B", "E", "H", "K" };

                for (int i = 0; i < nguoiKy.Length; i++)
                {
                    string colStart = cotKyStart[i];
                    string colEnd = ((char)(colStart[0] + (i == 3 ? 1 : 2))).ToString();


                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Merge().Value = nguoiKy[i];
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.Bold = true;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.FontSize = 10;

                    string ghiChu = i == 0 ? "(Ký, họ tên, đóng dấu)" : "(Ký, họ tên)";
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Merge().Value = ghiChu;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.FontSize = 10;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.Italic = true;
                }


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ThongKeBN_TaiKham.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            try
            {
                var query = _localDb.M0303Thongtinbnhenkhams.AsQueryable();
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                Console.WriteLine("Logo path: " + logoPath);


                if (tuNgay.HasValue)
                {
                    query = query.Where(x => x.NgayHenKham >= tuNgay.Value);
                }

                if (denNgay.HasValue)
                {
                    query = query.Where(x => x.NgayHenKham <= denNgay.Value);
                }

                if (idChiNhanh.HasValue && idChiNhanh > 0)
                {
                    query = query.Where(x => x.IDCN == idChiNhanh.Value);
                }

                var data = await query.AsNoTracking().ToListAsync();

                if (data == null || !data.Any())
                {
                    return BadRequest("Không có dữ liệu để xuất PDF");
                }

                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => x.IDChiNhanh == idChiNhanh)
                    .Select(x => new M0303ThongTinDoanhNghiep
                    {
                        TenCSKCB = x.TenCSKCB ?? "",
                        DiaChi = x.DiaChi ?? "",
                        DienThoai = x.DienThoai ?? "",
                        Email = x.Email ?? "",
                        Website = x.Website ?? "",
                        MaCSKCB = x.MaCSKCB ?? ""
                    })

                    .FirstOrDefaultAsync();



                var document = new P0303TKSoLuongBNHenKham(data, tuNgay, denNgay, logoPath, thongTinDoanhNghiep);



                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", $"DanhSachHenKham_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        public async Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh)
        {
            Console.WriteLine("===== FilterByDay CALLED =====");
            Console.WriteLine($"TuNgay: {tuNgay}, DenNgay: {denNgay}, IDCN: {idChiNhanh}");

            try
            {
                DateTime? parsedTuNgay = !string.IsNullOrEmpty(tuNgay)
                    ? DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null)
                    : null;

                DateTime? parsedDenNgay = !string.IsNullOrEmpty(denNgay)
                    ? DateTime.ParseExact(denNgay, "yyyy-MM-dd", null)
                    : null;

                var data = await _localDb.Set<M0303TKSoLuongBNHenKhamSTO>()
    .FromSqlRaw("EXEC S0303_TKSoLuongBNHenTaiKham @TuNgay, @DenNgay, @IDCN",
        new SqlParameter("@TuNgay", parsedTuNgay ?? (object)DBNull.Value),
        new SqlParameter("@DenNgay", parsedDenNgay ?? (object)DBNull.Value),
        new SqlParameter("@IDCN", idChiNhanh))
    .AsNoTracking()
    .ToListAsync();


                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => x.IDChiNhanh == idChiNhanh)
                    .Select(x => new
                    {
                        TenCSKCB = x.TenCSKCB ?? "",
                        DiaChi = x.DiaChi ?? "",
                        DienThoai = x.DienThoai ?? "",
                        Email = x.Email ?? "",
                        Website = x.Website ?? "",
                        MaCSKCB = x.MaCSKCB ?? ""
                    })
                    .FirstOrDefaultAsync();

                return new
                {
                    success = true,
                    data,
                    thongTinDoanhNghiep
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI: {ex.Message}");
                return new { success = false, error = ex.Message };
            }
        }
    }
}

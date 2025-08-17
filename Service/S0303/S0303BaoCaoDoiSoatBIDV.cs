using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Context;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Fluent;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303BaoCaoDoiSoatBIDV : ControllerBase,I0303BaoCaoDoiSoatBIDV
    {

        private readonly M0303AppDbContext _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303BaoCaoDoiSoatBIDV(M0303AppDbContext localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }

        public async Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh)
        {
           
            try
            {
                DateTime? parsedTuNgay = !string.IsNullOrEmpty(tuNgay)
                    ? DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null)
                    : null;

                DateTime? parsedDenNgay = !string.IsNullOrEmpty(denNgay)
                    ? DateTime.ParseExact(denNgay, "yyyy-MM-dd", null)
                    : null;

                var data = await _localDb.Set<M0303BaoCaoDoiSoatBIDVSTO>()
    .FromSqlRaw("EXEC S0303_BaoCaoDoiSoatBIDV @TuNgay, @DenNgay, @IDCN",
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

        public async Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn)
        {
            var danhSach = _localDb.M0303BaoCaoDoiSoatBIDVs.AsQueryable();

            if (tuNgay.HasValue && denNgay.HasValue)
            {
                danhSach = danhSach.Where(x => x.NgayGioGiaoDich.HasValue &&
                                               x.NgayGioGiaoDich.Value.Date >= tuNgay.Value.Date &&
                                               x.NgayGioGiaoDich.Value.Date <= denNgay.Value.Date);
            }

            if (idcn.HasValue && idcn.Value > 0)
            {
                danhSach = danhSach.Where(x => x.IDCN == idcn.Value);
            }

            var list = await danhSach.ToListAsync();

            if (!list.Any())
            {
                return BadRequest("Không có dữ liệu trong khoảng ngày đã chọn");
            }

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
                var ws = workbook.Worksheets.Add("Báo cáo đối soát BIDV");

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    
                    ws.Range("A1:B4").Merge();

                    
                    ws.Column(1).Width = 25; 
                    ws.Column(2).Width = 25;

                    
                    var img = ws.AddPicture(logoPath)
                        .MoveTo(ws.Cell("A1"), -10, 10)
                        .WithPlacement(XLPicturePlacement.FreeFloating)
                        .Scale(0.3);
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

                
                ws.Range("A6:P6").Merge().Value = "BẢNG BÁO CÁO ĐỐI SOÁT BIDV";
                ws.Range("A6:P6").Style.Font.Bold = true;
                ws.Range("A6:P6").Style.Font.FontSize = 24;
                ws.Range("A6:P6").Style.Font.FontName = "Times New Roman";
                ws.Range("A6:P6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A6:P6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(6).Height = 40;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd/MM/yyyy} đến ngày {denNgay.Value:dd/MM/yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:P7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:P7").Style.Font.Bold = true;
                ws.Range("A7:P7").Style.Font.FontSize = 12;
                ws.Range("A7:P7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 20;

                
                string[] mainHeaders = {
              "STT", "Mã y tế", "Mã đợt", "Họ tên bệnh nhân", "Số điện thoại",
    "Số tiền trên BL", "Số BL", "Số tiền trên HD", "Số HD", "Tổng số tiền",
    "Ngày giờ giao dịch", "User thanh toán"
        };

               
                double[] columnWidths = {
    7, 9, 11, 26, 15, 16, 9, 16, 9, 16, 23, 18,
    16, 13, 16, 13
};

                for (int i = 0; i < columnWidths.Length; i++)
                {
                    ws.Column(i + 1).Width = columnWidths[i];
                }

                
                var headerRow1 = ws.Row(9);
                headerRow1.Height = 25;

                
                for (int col = 1; col <= 12; col++)
                {
                    var range = ws.Range(9, col, 10, col).Merge();
                    var cell = ws.Cell(9, col);
                    cell.Value = mainHeaders[col - 1];

                    
                    range.Style.Font.Bold = true;
                    range.Style.Font.FontColor = XLColor.White;
                    range.Style.Fill.BackgroundColor = XLColor.Gray;
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                 
                    range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                }

                

                for (int i = 0; i < mainHeaders.Length; i++)
                {
                    ws.Cell(9, i + 1).Value = mainHeaders[i];
                }

                
                ws.Range(9, 13, 9, 14).Merge().Value = "BVUB";
                ws.Range(9, 15, 9, 16).Merge().Value = "BIDV";

               
                for (int col = 13; col <= 16; col += 2)
                {
                    var range = ws.Range(9, col, 9, col + 1);
                    range.Style.Font.Bold = true;
                    range.Style.Font.FontColor = XLColor.White;
                    range.Style.Fill.BackgroundColor = XLColor.Gray;
                    range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                
                var headerRow2 = ws.Row(10);
                headerRow2.Height = 25;

                string[] subHeaders = { "Số tiền", "Trạng thái", "Số tiền", "Trạng thái" };

                for (int i = 0; i < subHeaders.Length; i++)
                {
                    var col = 13 + i;
                    var cell = ws.Cell(10, col);
                    cell.Value = subHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Fill.BackgroundColor = XLColor.Gray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    
                    cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                }

                
                int row = 11;
                int stt = 1;
                decimal tongBVUB = 0;
                decimal tongBIDV = 0;

                foreach (var item in list)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.MaYTe;
                    ws.Cell(row, 3).Value = item.MaDot;
                    ws.Cell(row, 4).Value = item.HoTenBenhNhan;
                    ws.Cell(row, 5).Value = item.SoDienThoai;
                    ws.Cell(row, 6).Value = item.SoTienTrenBL;
                    ws.Cell(row, 7).Value = item.SoBL;
                    ws.Cell(row, 8).Value = item.SoTienTrenHD;
                    ws.Cell(row, 9).Value = item.SoHD;
                    ws.Cell(row, 10).Value = item.TongSoTien;
                    ws.Cell(row, 11).Value = item.NgayGioGiaoDich?.ToString("dd/MM/yyyy HH:mm:ss");
                    ws.Cell(row, 12).Value = item.UserThanhToan;
                    ws.Cell(row, 13).Value = item.BVUB_SoTien;
                    ws.Cell(row, 14).Value = item.BVUB_TrangThai;
                    ws.Cell(row, 15).Value = item.BIDV_SoTien;
                    ws.Cell(row, 16).Value = item.BIDV_TrangThai;

                    
                    if (item.BVUB_SoTien.HasValue) tongBVUB += item.BVUB_SoTien.Value;
                    if (item.BIDV_SoTien.HasValue) tongBIDV += item.BIDV_SoTien.Value;

                    
                    int[] moneyColumns = { 6, 8, 10, 13, 15 };
                    foreach (var col in moneyColumns)
                    {
                        ws.Cell(row, col).Style.NumberFormat.Format = "#,##0";
                        ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    
                    int[] centerCols = { 1, 2, 3, 5, 7, 9, 11, 12, 14, 16 };
                    foreach (int col in centerCols)
                    {
                        ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    
                    for (int col = 1; col <= 16; col++)
                    {
                        ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Cell(row, col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

               
                ws.Cell(row, 1).Value = "Tổng cộng:";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Range(row, 1, row, 12).Merge();

                ws.Cell(row, 13).Value = tongBVUB;
                ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 13).Style.Font.Bold = true;
                ws.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, 14).Value = "";

                ws.Cell(row, 15).Value = tongBIDV;
                ws.Cell(row, 15).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 15).Style.Font.Bold = true;
                ws.Cell(row, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, 16).Value = "";

               
                for (int col = 1; col <= 16; col++)
                {
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

               
                int footerRow = row + 2;

                string[] nguoiKy = { "THỦ TRƯỞNG ĐƠN VỊ", "THỦ QUỸ", "KẾ TOÁN", "NGƯỜI LẬP BẢNG" };
                string[] cotKyStart = { "B", "E", "H", "K" };

                for (int i = 0; i < nguoiKy.Length; i++)
                {
                    string colStart = cotKyStart[i];
                    string colEnd = ((char)(colStart[0] + (i == 3 ? 2 : 2))).ToString();

                    if (i == 3)
                    {
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.Italic = true;
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.FontSize = 10;
                    }

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
                              $"BaoCaoDoiSoatBIDV_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            try
            {
                var query = _localDb.M0303BaoCaoDoiSoatBIDVs.AsQueryable();
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");


                if (tuNgay.HasValue)
                {
                    query = query.Where(x => x.NgayGioGiaoDich >= tuNgay.Value);
                }

                if (denNgay.HasValue)
                {
                    query = query.Where(x => x.NgayGioGiaoDich <= denNgay.Value);
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



                var document = new P0303BaoCaoDoiSoatBIDV(data, tuNgay, denNgay, logoPath, thongTinDoanhNghiep);



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
    }
}

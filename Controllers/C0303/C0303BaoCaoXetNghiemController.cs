using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
   [Route("bao_cao_xet_nghiem")]
    public class C0303BaoCaoXetNghiemController : Controller
    {
        //private string _maChucNang = "/"bao_cao_xet_nghiem";
        //private IMemoryCachingServices _memoryCache;

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public C0303BaoCaoXetNghiemController(Context0303 localDb, IWebHostEnvironment env
            /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
         

            //_memoryCache = memoryCache;
        }
        public IActionResult V0303BaoCaoXetNghiemPage()
        {
            // var quyenVaiTro = await _memoryCache.getQuyenVaiTro(_maChucNang);
            //if (quyenVaiTro == null)
            //{
            //    return RedirectToAction("NotFound", "Home");
            //}
            //ViewBag.quyenVaiTro = quyenVaiTro;
            //ViewData["Title"] = CommonServices.toEmptyData(quyenVaiTro);

            ViewBag.quyenVaiTro = new
            {
                Them = true,
                Sua = true,
                Xoa = true,
                Xuat = true,
                CaNhan = true,
                Xem = true,
            };

            return View("~/Views/V0303/V0303BaoCaoXetNghiem/V0303BaoCaoXetNghiemPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay)
        {
            try
            {
                DateTime? parsedTuNgay = !string.IsNullOrEmpty(tuNgay)
                    ? DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null)
                    : null;

                DateTime? parsedDenNgay = !string.IsNullOrEmpty(denNgay)
                    ? DateTime.ParseExact(denNgay, "yyyy-MM-dd", null)
                    : null;

                var data = await _localDb.Set<M0303BaoCaoXetNghiem>()
                    .FromSqlRaw(@"EXEC S00_BCXetNghiem @TuNgay, @DenNgay",
                        new SqlParameter("@TuNgay", parsedTuNgay?.ToString("dd/MM/yyyy") ?? (object)DBNull.Value),
                        new SqlParameter("@DenNgay", parsedDenNgay?.ToString("dd/MM/yyyy") ?? (object)DBNull.Value))
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("nhan-vien/all")]
        public async Task<List<M0303NhanVien>> GetNhomNhanVien()
        {
            var nhomNhanVien = await _localDb.Set<M0303NhanVien>()
                .FromSqlRaw(@"select ID, TenNhanVien as NguoiChiDinh
                            from DM_NhanVien 
                            where Active = 1")
                .Select(nv => new M0303NhanVien
                {
                    ID = nv.ID,
                    NguoiChiDinh = nv.NguoiChiDinh ?? ""
                })
                .ToListAsync();

            return nhomNhanVien;
        }

        [HttpPost("export-excel")]
        public async Task<IActionResult> ExportExcel(
  string tuNgay,
  string denNgay,
  int idNhanVien = 0)
        {
            try
            {
                // 1. Parse ngày
                DateTime? parsedTuNgay = !string.IsNullOrEmpty(tuNgay)
                    ? DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null)
                    : null;

                DateTime? parsedDenNgay = !string.IsNullOrEmpty(denNgay)
                    ? DateTime.ParseExact(denNgay, "yyyy-MM-dd", null)
                    : null;

                // 2. Lấy dữ liệu từ stored procedure
                var data = await _localDb.Set<M0303BaoCaoXetNghiem>()
                    .FromSqlRaw(@"EXEC S00_BCXetNghiem @TuNgay, @DenNgay",
                        new SqlParameter("@TuNgay", parsedTuNgay?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value),
                        new SqlParameter("@DenNgay", parsedDenNgay?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value))
                    .AsNoTracking()
                    .ToListAsync();

                // 3. Filter theo nhân viên nếu có
                if (idNhanVien > 0)
                {
                    // Lấy tên nhân viên từ danh sách
                    var nhanVienList = await GetNhomNhanVien();
                    var selectedNhanVien = nhanVienList.FirstOrDefault(nv => nv.ID == idNhanVien);

                    if (selectedNhanVien != null)
                    {
                        data = data.Where(x =>
                            x.NguoiChiDinh != null &&
                            x.NguoiChiDinh.ToLower().Contains(selectedNhanVien.NguoiChiDinh.ToLower()))
                            .ToList();
                    }
                }

                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất Excel");

                // 4. Thông tin doanh nghiệp
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Select(x => new M0303ThongTinDoanhNghiep
                    {
                        TenCoQuanChuyenMon = "SỞ Y TẾ TP. HỒ CHÍ MINH",
                        TenCSKCB = x.TenCSKCB ?? "",
                        DiaChi = x.DiaChi ?? "",
                        DienThoai = x.DienThoai ?? "",
                        Email = x.Email ?? "",
                        Website = x.Website ?? "",
                        MaCSKCB = x.MaCSKCB ?? ""
                    }).FirstOrDefaultAsync();

                thongTinDoanhNghiep ??= new M0303ThongTinDoanhNghiep
                {
                    TenCoQuanChuyenMon = "SỞ Y TẾ TP. HỒ CHÍ MINH",
                    TenCSKCB = "BỆNH VIỆN UNG BƯỚU",
                    DiaChi = "Số 3 Nơ Trang Long, Phường 12, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    DienThoai = "(028) 38433022"
                };

                // 5. Tạo workbook Excel
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("BÁO CÁO XÉT NGHIỆM");

                // 5a. Logo
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    // Merge ô nếu cần
                    ws.Range("A1:B4").Merge();
                    ws.Column(1).Width = 0; // Giảm độ rộng cột
                    ws.Column(2).Width = 0;

                    // Chèn logo và scale nhỏ hơn
                    var picture = ws.AddPicture(logoPath)
                        .MoveTo(ws.Cell("A1"), 10, 2)  // Giảm offset (thử nghiệm giá trị)
                        .WithPlacement(XLPicturePlacement.FreeFloating)
                        .Scale(0.08); // Scale nhỏ hơn
                }

                // 4. Thông tin header
                string tenCoQuan = thongTinDoanhNghiep.TenCoQuanChuyenMon;
                string tenCSKCB = thongTinDoanhNghiep.TenCSKCB;
                bool hienTenCSKCB = !string.Equals(tenCoQuan.Trim(), tenCSKCB.Trim(), StringComparison.OrdinalIgnoreCase);
                string diaChi = thongTinDoanhNghiep.DiaChi;
                string dienThoai = thongTinDoanhNghiep.DienThoai;

                if (hienTenCSKCB)
                {
                    ws.Range("C1:O1").Merge().Value = tenCSKCB;
                    ws.Range("C1:O1").Style.Font.FontName = "Times New Roman";
                    ws.Range("C1:O1").Style.Font.FontSize = 10;
                    ws.Range("C1:O1").Style.Font.Bold = true;
                    ws.Range("C1:O1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                }

                ws.Range("C2:O2").Merge().Value = diaChi;
                ws.Range("C3:O3").Merge().Value = $"Điện thoại: {dienThoai}";

                var rangeTitle = ws.Range("A6:F6");
                rangeTitle.Merge();
                rangeTitle.Value = "BẢNG BÁO CÁO XÉT NGHIỆM";
                rangeTitle.Style.Font.FontSize = 24;
                rangeTitle.Style.Font.Bold = true;
                rangeTitle.Style.Font.FontName = "Times New Roman";
                rangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                ws.Row(6).Height = 40; // tăng chút cho font to

                string thoiGianThongKe = parsedTuNgay.HasValue && parsedDenNgay.HasValue
                    ? $"Từ ngày {parsedTuNgay.Value:dd-MM-yyyy} đến ngày {parsedDenNgay.Value:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:F7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:F7").Style.Font.Bold = true;
                ws.Range("A7:F7").Style.Font.FontSize = 12;
                ws.Range("A7:F7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 20;

                // 7. Header
                int startRow = 9;
                ws.Cell(startRow, 1).Value = "STT";
                ws.Cell(startRow, 2).Value = "Mã DV";
                ws.Cell(startRow, 3).Value = "Tên dịch vụ";
                ws.Cell(startRow, 4).Value = "Số lượng BHYT";
                ws.Cell(startRow, 5).Value = "Số lượng dịch vụ";
                ws.Cell(startRow, 6).Value = "Tổng số lượng";

                var headerRange = ws.Range(startRow, 1, startRow, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Fill.BackgroundColor = XLColor.Gray;   // Nền xám
                headerRange.Style.Font.FontColor = XLColor.White;        // Chữ trắng


                // 8. Dữ liệu chi tiết - Gom theo người chỉ định
                int row = startRow + 1;
                int stt = 1;
                decimal tongSLBH = 0;
                decimal tongSLDV = 0;
                decimal tongTatCa = 0;

                var groupedByDoctor = data.GroupBy(x => x.NguoiChiDinh ?? "Không rõ")
                                          .OrderBy(g => g.Key);

                foreach (var doctorGroup in groupedByDoctor)
                {
                    // Dòng tên bác sĩ - CĂN TRÁI
                    var doctorRange = ws.Range(row, 1, row, 6);
                    doctorRange.Merge();
                    doctorRange.Value = doctorGroup.Key;
                    doctorRange.Style.Font.Bold = true;
                    doctorRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    doctorRange.Style.Alignment.Indent = 1;
                    row++;

                    decimal subSLBH = 0;
                    decimal subSLDV = 0;
                    decimal subTotal = 0;

                    // Chi tiết dịch vụ của bác sĩ
                    foreach (var item in doctorGroup)
                    {
                        decimal slbh = item.SLBH != null ? decimal.Parse(item.SLBH.ToString()) : 0;
                        decimal sldv = item.SLDV != null ? decimal.Parse(item.SLDV.ToString()) : 0;
                        decimal total = slbh + sldv;

                        ws.Cell(row, 1).Value = stt++;
                        ws.Cell(row, 2).Value = item.MaDichVu;
                        ws.Cell(row, 3).Value = item.TenDichVu;
                        ws.Cell(row, 4).Value = slbh;
                        ws.Cell(row, 5).Value = sldv;
                        ws.Cell(row, 6).Value = total;

                        // Căn giữa cho STT và Mã DV
                        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // CĂN TRÁI cho Tên dịch vụ
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // CĂN PHẢI cho 3 cột số lượng
                        ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Định dạng số
                        if (slbh > 0) ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        if (sldv > 0) ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                        if (total > 0) ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                        subSLBH += slbh;
                        subSLDV += sldv;
                        subTotal += total;

                        row++;
                    }

                    // Dòng cộng cho bác sĩ
                    var subtotalRange = ws.Range(row, 1, row, 3);
                    subtotalRange.Merge();
                    subtotalRange.Value = "Cộng";
                    subtotalRange.Style.Font.Bold = true;
                    subtotalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(row, 4).Value = subSLBH;
                    ws.Cell(row, 5).Value = subSLDV;
                    ws.Cell(row, 6).Value = subTotal;

                    // CĂN PHẢI cho dòng cộng
                    ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Định dạng số cho subtotal
                    if (subSLBH > 0) ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                    if (subSLDV > 0) ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                    if (subTotal > 0) ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                    ws.Range(row, 4, row, 6).Style.Font.Bold = true;

                    tongSLBH += subSLBH;
                    tongSLDV += subSLDV;
                    tongTatCa += subTotal;

                    row++;
                }

                // 9. Tổng cuối bảng
                var totalLabelRange = ws.Range(row, 1, row, 3);
                totalLabelRange.Merge();
                totalLabelRange.Value = "Tổng cộng";
                totalLabelRange.Style.Font.Bold = true;
                totalLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, 4).Value = tongSLBH;
                ws.Cell(row, 5).Value = tongSLDV;
                ws.Cell(row, 6).Value = tongTatCa;

                // CĂN PHẢI cho dòng tổng cộng
                ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Định dạng số cho tổng cộng
                if (tongSLBH > 0) ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                if (tongSLDV > 0) ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                if (tongTatCa > 0) ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                ws.Range(row, 4, row, 6).Style.Font.Bold = true;

                // 10. Set width cột
                ws.Column(1).Width = 4;   // STT - căn giữa
                ws.Column(2).Width = 8;   // Mã DV - căn giữa  
                ws.Column(3).Width = 100; // Tên dịch vụ - căn trái
                ws.Column(4).Width = 18;  // SL BHYT - căn phải
                ws.Column(5).Width = 18;  // SL DV - căn phải
                ws.Column(6).Width = 18;  // Tổng - căn phải

                // 11. Border
                var dataRange = ws.Range(startRow, 1, row, 6);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // 12. Phần ký tên
                int footerRow = row + 2;
                string[] nguoiKy = { "NGƯỜI LẬP BẢNG" };
                string[] cotKyStart = { "E" };

                for (int i = 0; i < nguoiKy.Length; i++)
                {
                    string colStart = cotKyStart[i];
                    string colEnd = ((char)(colStart[0] + 2)).ToString();

                    // Dòng ngày tháng năm
                    ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                    ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.Italic = true;
                    ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.FontSize = 10;

                    // Dòng chức danh
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Merge().Value = nguoiKy[i];
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.Bold = true;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.FontSize = 10;

                    // Dòng ghi chú
                    string ghiChu = "(Ký, họ tên)";
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Merge().Value = ghiChu;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.FontSize = 10;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.Italic = true;
                }

                // 13. Xuất file
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoXetNghiem_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

    }
}

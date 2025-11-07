using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Fluent;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("danh_sach_kham_benh_theo_bac_si")]
    public class C0303DanhSachKhamBenhTheoBacSiController : Controller
    {
        //private string _maChucNang = "/bao_cao_thu_tong_hop_dv_theo_khoa_phong";
        //private IMemoryCachingServices _memoryCache;

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
    
       

        public C0303DanhSachKhamBenhTheoBacSiController(
            Context0303 localDb,
            IWebHostEnvironment env /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
           
        }

        public IActionResult V0303DanhSachKhamBenhTheoBacSiPage()
        {
            //var quyenVaiTro = await _memoryCache.getQuyenVaiTro(_maChucNang);
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

            return View("~/Views/V0303/V0303DanhSachKhamBenhTheoBacSi/V0303DanhSachKhamBenhTheoBacSiPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay,
int idChiNhanh)
        {
            DateTime? parsedTuNgay = !string.IsNullOrEmpty(tuNgay)
                   ? DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null)
                   : null;

            DateTime? parsedDenNgay = !string.IsNullOrEmpty(denNgay)
                ? DateTime.ParseExact(denNgay, "yyyy-MM-dd", null)
                : null;

            var data = await _localDb.Set<M0303DanhSachKhamBenhTheoBacSiSTO>()
                .FromSqlRaw(@"EXEC S0303_DanhSachKhamBenhTheoBacSi @TuNgay, @DenNgay, @IDCN",
                    new SqlParameter("@TuNgay", parsedTuNgay?.ToString("dd/MM/yyyy") ?? (object)DBNull.Value),
                    new SqlParameter("@DenNgay", parsedDenNgay?.ToString("dd/MM/yyyy") ?? (object)DBNull.Value),
                      new SqlParameter("@IDCN", idChiNhanh))
                .AsNoTracking()
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data
            });
        }


      

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPDF([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idChiNhanh, [FromQuery] string? idnv = null)
        {
            try
            {

                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idnv);


                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất PDF");

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => idChiNhanh.HasValue && x.IDChiNhanh == idChiNhanh.Value)
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


                var document = new P0303DanhSachKhamBenhTheoBacSi(data, tuNgay, denNgay, logoPath, thongTinDoanhNghiep, idnv);
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;


                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"DanhSachHenKham_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex);
                return new ObjectResult($"Lỗi khi tạo PDF: {ex.Message}") { StatusCode = 500 };
            }
        }

        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport(
    [FromQuery] DateTime? tuNgay,
    [FromQuery] DateTime? denNgay,
    [FromQuery] int? idcn,
    [FromQuery] string? idnv = null
    )
        {
            try
            {
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idcn);
                if (!data.Any())
                    return BadRequest("Không có dữ liệu để xuất Excel");

                // ---- Lấy thông tin cơ sở y tế ----
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => idcn.HasValue && x.IDChiNhanh == idcn.Value)
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
                    TenCSKCB = "BỆNH VIỆN UNG BƯỚU",
                    DiaChi = "Số 3 Nơ Trang Long, Phường 12, Quận Bình Thạnh, TP. HCM",
                    DienThoai = "(028) 38433022"
                };

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("DANH SÁCH KHÁM BỆNH THEO BÁC SĨ");

                // ====== LOGO + THÔNG TIN ======
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:A4").Merge();
                    ws.Column(1).Width = 20;
                    ws.AddPicture(logoPath)
                      .MoveTo(ws.Cell("A1"), 20, 5)
                      .WithPlacement(XLPicturePlacement.FreeFloating)
                      .Scale(0.08);
                }

                ws.Cell("B1").Value = thongTinDoanhNghiep.TenCSKCB;
                ws.Cell("B1").Style.Font.FontName = "Times New Roman";
                ws.Cell("B1").Style.Font.FontSize = 10;
                ws.Cell("B1").Style.Font.Bold = true;
                ws.Cell("B2").Value = thongTinDoanhNghiep.DiaChi;
                ws.Cell("B3").Value = $"Điện thoại: {thongTinDoanhNghiep.DienThoai}";
                ws.Cell("B4").Value = $"PHÒNG CÔNG NGHỆ THÔNG TIN";
                ws.Cell("B4").Style.Font.FontSize = 10;
                ws.Cell("B4").Style.Font.Bold = true;
                ws.Range("B1:B3").Style.Font.FontName = "Times New Roman";
                ws.Range("B1:B3").Style.Font.FontSize = 10;

                // ====== TIÊU ĐỀ ======
                ws.Range("D6:H6").Merge().Value = "DANH SÁCH KHÁM BỆNH THEO BÁC SĨ";
                ws.Range("D6:H6").Style.Font.FontSize = 20;
                ws.Range("D6:H6").Style.Font.Bold = true;
                ws.Range("D6:H6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;



                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay:dd-MM-yyyy} đến ngày {denNgay:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";
                ws.Range("D7:H7").Merge().Value = thoiGianThongKe;
                ws.Range("D7:H7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("D7:H7").Style.Font.Bold = true;

                // ====== HEADER BẢNG ======
                int row = 9;
                string[] headers = { "STT", "Ngày khám", "Mã y tế", "Họ tên bệnh nhân", "Ngày sinh", "BHYT", "Tên dịch vụ", "Nơi thực hiện", "Số lượng" };

                for (int col = 1; col <= headers.Length; col++)
                {
                    ws.Cell(row, col).Value = headers[col - 1];
                    ws.Cell(row, col).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, col).Style.Font.FontSize = 10;
                    ws.Cell(row, col).Style.Font.Bold = true;
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // ====== NHÓM DỮ LIỆU THEO BÁC SĨ ======
                var groupedByBacSi = data
                    .GroupBy(x => x.BacSiChiDinh ?? "Không rõ bác sĩ")
                    .ToDictionary(g => g.Key, g => g.ToList());

                row = 10;
                int stt = 1;

                // ====== DUYỆT QUA TỪNG NHÓM BÁC SĨ ======
                foreach (var bacSiGroup in groupedByBacSi)
                {
                    string bacSi = bacSiGroup.Key;
                    var patients = bacSiGroup.Value;

                    // Tính tổng cho bác sĩ này
                    int subTotalSoLuong = patients.Sum(x => x.SoLuong ?? 0);
                    int subTotalBHYT = patients.Count(x => x.BHYT == true);

                    // Dòng header cho bác sĩ
                    ws.Cell(row, 1).Value = $"- {bacSi}";
                    ws.Range(row, 1, row, 5).Merge();
                    ws.Cell(row, 1).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, 1).Style.Font.FontSize = 10;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    ws.Cell(row, 6).Value = subTotalBHYT;
                    ws.Cell(row, 6).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, 6).Style.Font.FontSize = 10;
                    ws.Cell(row, 6).Style.Font.Bold = true;
                    ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    ws.Cell(row, 7).Value = "";
                    ws.Range(row, 7, row, 8).Merge();
                    ws.Cell(row, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    ws.Cell(row, 9).Value = subTotalSoLuong;
                    ws.Cell(row, 9).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, 9).Style.Font.FontSize = 10;
                    ws.Cell(row, 9).Style.Font.Bold = true;
                    ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    row++;

                    // ====== DỮ LIỆU BỆNH NHÂN ======
                    foreach (var item in patients)
                    {
                        ws.Cell(row, 1).Value = stt++;
                        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 2).Value = item.NgayKham?.ToString("dd-MM-yyyy") ?? "";
                        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 3).Value = item.MaYTe ?? "";
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 4).Value = item.HoTenBenhNhan ?? "";

                        ws.Cell(row, 5).Value = item.NgaySinh?.ToString("dd-MM-yyyy") ?? "";
                        ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 6).Value = item.BHYT == true ? "X" : "";
                        ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 7).Value = item.TenDichVu ?? "";

                        ws.Cell(row, 8).Value = item.NoiThucHien ?? "";

                        ws.Cell(row, 9).Value = item.SoLuong ?? 0;
                        ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Áp dụng style cho toàn bộ dòng
                        for (int col = 1; col <= 9; col++)
                        {
                            ws.Cell(row, col).Style.Font.FontName = "Times New Roman";
                            ws.Cell(row, col).Style.Font.FontSize = 10;
                            ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        row++;
                    }
                }

                // ====== DÒNG TỔNG CỘNG CUỐI BẢNG ======
                int totalSoLuong = data.Sum(x => x.SoLuong ?? 0);
                int totalBHYT = data.Count(x => x.BHYT == true);

                // Tạo range cho toàn bộ dòng tổng cộng và áp dụng border
                var totalRowRange = ws.Range(row, 1, row, 9);
                totalRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                totalRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Ô 1-5: merge và để trống
                ws.Cell(row, 1).Value = "";
                ws.Range(row, 1, row, 5).Merge();
                ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

                // Ô 6: Tổng BHYT
                ws.Cell(row, 6).Value = totalBHYT;
                ws.Cell(row, 6).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 6).Style.Font.FontSize = 10;
                ws.Cell(row, 6).Style.Font.Bold = true;
                ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.LightGray;

                // Ô 7-8: Merge và ghi "Tổng cộng:"
                ws.Cell(row, 7).Value = "Tổng cộng:";
                ws.Range(row, 7, row, 8).Merge();
                ws.Cell(row, 7).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 7).Style.Font.FontSize = 10;
                ws.Cell(row, 7).Style.Font.Bold = true;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 7).Style.Fill.BackgroundColor = XLColor.LightGray;

                // Ô 9: Tổng số lượng
                ws.Cell(row, 9).Value = totalSoLuong;
                ws.Cell(row, 9).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 9).Style.Font.FontSize = 10;
                ws.Cell(row, 9).Style.Font.Bold = true;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.LightGray;
                // ====== FOOTER ======
                row += 2;
                ws.Cell(row, 7).Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Range(row, 7, row, 9).Merge();
                ws.Cell(row, 7).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 7).Style.Font.FontSize = 10;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                ws.Cell(row, 7).Value = "Người lập bảng";
                ws.Range(row, 7, row, 9).Merge();
                ws.Cell(row, 7).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 7).Style.Font.FontSize = 10;
                ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row + 5, 7, row + 5, 9).Merge().Value = idnv;
                ws.Cell(row + 5, 7).Style.Font.FontName = "Times New Roman";
                ws.Cell(row + 5, 7).Style.Font.FontSize = 10;
                ws.Cell(row + 5, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // ====== ĐIỀU CHỈNH ĐỘ RỘNG CỘT ======
                ws.Column(1).Width = 15;   // STT
                ws.Column(2).Width = 12;  // Ngày khám
                ws.Column(3).Width = 15;  // Mã y tế
                ws.Column(4).Width = 25;  // Họ tên bệnh nhân
                ws.Column(5).Width = 12;  // Ngày sinh
                ws.Column(6).Width = 8;   // BHYT
                ws.Column(7).Width = 35;  // Tên dịch vụ
                ws.Column(8).Width = 25;  // Nơi thực hiện
                ws.Column(9).Width = 12;  // Số lượng

                // ====== TRẢ FILE ======
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachKhamBenhTheoBacSi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xuất Excel: {ex.Message}");
            }
        }

        [NonAction]
        public async Task<List<M0303DanhSachKhamBenhTheoBacSiSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, string? idnv = null)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303DanhSachKhamBenhTheoBacSiSTOs
                .FromSqlInterpolated($"EXEC S0303_DanhSachKhamBenhTheoBacSi @TuNgay = {tuNgayStr}, @DenNgay = {denNgayStr}, @IDCN = {idCN}")
                .ToListAsync();
        }

    }
}

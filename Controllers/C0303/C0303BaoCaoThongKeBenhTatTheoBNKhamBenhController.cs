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
    [Route("bao_cao_thong_ke_benh_tat_theo_benh_nhan_kham_benh")]
    public class C0303BaoCaoThongKeBenhTatTheoBNKhamBenhController : Controller
    {
        //private string _maChucNang = "/bao_cao_thong_ke_benh_tat_theo_benh_nhan_kham_benh";
        //private IMemoryCachingServices _memoryCache;

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public C0303BaoCaoThongKeBenhTatTheoBNKhamBenhController(Context0303 localDb, IWebHostEnvironment env
            /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;

            //_memoryCache = memoryCache;
        }

        public IActionResult V0303BaoCaoThongKeBenhTatTheoBNKhamBenhPage()
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

            return View("~/Views/V0303/V0303BaoCaoThongKeBenhTatTheoBNKhamBenh/V0303BaoCaoThongKeBenhTatTheoBNKhamBenhPage.cshtml");
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

            var data = await _localDb.Set<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO>()
                .FromSqlRaw(@"EXEC S0303_BaoCaoThongKeBenhTatTheoBNKhamBenh @TuNgay, @DenNgay, @IDCN",
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

        [NonAction]
        public async Task<List<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, string? idnv = null)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTOs
                .FromSqlInterpolated($"EXEC S0303_BaoCaoThongKeBenhTatTheoBNKhamBenh @TuNgay = {tuNgayStr}, @DenNgay = {denNgayStr}, @IDCN = {idCN}")
                .ToListAsync();
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


                var document = new P0303BaoCaoThongKeBenhTatTheoBNKhamBenh(data, tuNgay, denNgay, logoPath, thongTinDoanhNghiep, idnv);
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
      [FromQuery] string? idnv = null)
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
                var ws = workbook.Worksheets.Add("THỐNG KÊ BỆNH TẬT");

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
                ws.Range("B1:B3").Style.Font.FontName = "Times New Roman";
                ws.Range("B1:B3").Style.Font.FontSize = 10;

                // ====== TIÊU ĐỀ ======
                ws.Range("B6:I6").Merge().Value = "BÁO CÁO THỐNG KÊ BỆNH TẬT THEO BỆNH NHÂN KHÁM BỆNH";
                ws.Range("B6:I6").Style.Font.FontSize = 14;
                ws.Range("B6:I6").Style.Font.Bold = true;
                ws.Range("B6:I6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay:dd-MM-yyyy} đến ngày {denNgay:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";
                ws.Range("B7:I7").Merge().Value = thoiGianThongKe;
                ws.Range("B7:I7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("B7:I7").Style.Font.Bold = true;

                // ====== HEADER BẢNG VỚI 2 DÒNG ======
                int row = 9;

                // Dòng 1 của header
                ws.Cell(row, 1).Value = "Mã ICD";
                ws.Range(row, 1, row + 1, 1).Merge();
                ws.Cell(row, 2).Value = "Tên bệnh";
                ws.Range(row, 2, row + 1, 2).Merge();
                ws.Cell(row, 3).Value = "Tổng số";
                ws.Range(row, 3, row + 1, 3).Merge();
                ws.Cell(row, 4).Value = "Trong đó";
                ws.Range(row, 4, row, 6).Merge();
                ws.Cell(row, 7).Value = "Cách giải quyết";
                ws.Range(row, 7, row, 12).Merge();

                // Dòng 2 của header
                ws.Cell(row + 1, 4).Value = "Nữ";
                ws.Cell(row + 1, 5).Value = "< 15 tuổi";
                ws.Cell(row + 1, 6).Value = "< 6 tuổi";
                ws.Cell(row + 1, 7).Value = "Ra toa";
                ws.Cell(row + 1, 8).Value = "Vào viện";
                ws.Cell(row + 1, 9).Value = "Ngoại trú";
                ws.Cell(row + 1, 10).Value = "Chuyển viện";
                ws.Cell(row + 1, 11).Value = "Hẹn tái khám";
                ws.Cell(row + 1, 12).Value = "Khác";

                // Áp dụng style cho header
                for (int col = 1; col <= 12; col++)
                {
                    ws.Cell(row, col).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, col).Style.Font.FontSize = 10;
                    ws.Cell(row, col).Style.Font.Bold = true;
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    if (col <= 12) // Dòng 2
                    {
                        ws.Cell(row + 1, col).Style.Font.FontName = "Times New Roman";
                        ws.Cell(row + 1, col).Style.Font.FontSize = 9;
                        ws.Cell(row + 1, col).Style.Font.Bold = true;
                        ws.Cell(row + 1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row + 1, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(row + 1, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                }

                // ====== NHÓM DỮ LIỆU THEO TEN_BENH_GOC ======
                var groupedByTenBenhGoc = data
                    .GroupBy(x => x.TenBenhGoc ?? "Không xác định")
                    .ToDictionary(g => g.Key, g => g.ToList());

                row = 11; // Bắt đầu từ dòng 11 sau header

                // ====== DUYỆT QUA TỪNG NHÓM BỆNH GỐC ======
                foreach (var benhGocGroup in groupedByTenBenhGoc)
                {
                    string tenBenhGoc = benhGocGroup.Key;
                    var items = benhGocGroup.Value;

                    // Dòng header cho nhóm bệnh gốc
                    ws.Cell(row, 1).Value = tenBenhGoc;
                    ws.Range(row, 1, row, 12).Merge();
                    ws.Cell(row, 1).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row, 1).Style.Font.FontSize = 10;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    row++;

                    // ====== DỮ LIỆU CHI TIẾT ======
                    foreach (var item in items)
                    {
                        ws.Cell(row, 1).Value = item.MaBenhEdit ?? "";
                        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 2).Value = item.TenBenhEdit ?? "";
                        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        ws.Cell(row, 3).Value = item.TongSo ?? 0;
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 4).Value = item.TrongDoNu ?? 0;
                        ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 5).Value = item.TrongDoDuoi15Tuoi ?? 0;
                        ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 6).Value = item.TrongDoDuoi6Tuoi ?? 0;
                        ws.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 7).Value = item.CachGiaiQuyetRaToa ?? 0;
                        ws.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 8).Value = item.CachGiaiQuyetVaoVien ?? 0;
                        ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 9).Value = item.CachGiaiQuyetNgoaiTru ?? 0;
                        ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 10).Value = item.CachGiaiQuyetChuyenVien ?? 0;
                        ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 11).Value = item.CachGiaiQuyetHenTaiKham ?? 0;
                        ws.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.Cell(row, 12).Value = item.CachGiaiQuyetKhac ?? 0;
                        ws.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Áp dụng style cho toàn bộ dòng
                        for (int col = 1; col <= 12; col++)
                        {
                            ws.Cell(row, col).Style.Font.FontName = "Times New Roman";
                            ws.Cell(row, col).Style.Font.FontSize = 9;
                            ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        row++;
                    }
                }

              
                // ====== FOOTER ======
                row += 2;
                ws.Cell(row, 9).Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Range(row, 9, row, 12).Merge();
                ws.Cell(row, 9).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 9).Style.Font.FontSize = 10;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
                ws.Cell(row, 9).Value = "NGƯỜI LẬP BẢNG";
                ws.Range(row, 9, row, 12).Merge();
                ws.Cell(row, 9).Style.Font.FontName = "Times New Roman";
                ws.Cell(row, 9).Style.Font.FontSize = 10;
                ws.Cell(row, 9).Style.Font.Bold = true;
                ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (!string.IsNullOrEmpty(idnv))
                {
                    ws.Range(row + 3, 9, row + 3, 12).Merge().Value = idnv;
                    ws.Cell(row + 3, 9).Style.Font.FontName = "Times New Roman";
                    ws.Cell(row + 3, 9).Style.Font.FontSize = 10;
                    ws.Cell(row + 3, 9).Style.Font.Bold = true;
                    ws.Cell(row + 3, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // ====== ĐIỀU CHỈNH ĐỘ RỘNG CỘT ======
                ws.Column(1).Width = 15;   // Mã ICD
                ws.Column(2).Width = 30;   // Tên bệnh
                ws.Column(3).Width = 8;    // Tổng số
                ws.Column(4).Width = 6;    // Nữ
                ws.Column(5).Width = 10;   // < 15 tuổi
                ws.Column(6).Width = 10;   // < 6 tuổi
                ws.Column(7).Width = 8;    // Ra toa
                ws.Column(8).Width = 8;    // Vào viện
                ws.Column(9).Width = 8;    // Ngoại trú
                ws.Column(10).Width = 10;  // Chuyển viện
                ws.Column(11).Width = 10;  // Hẹn tái khám
                ws.Column(12).Width = 6;   // Khác

                // ====== TRẢ FILE ======
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoThongKeBenhTat_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xuất Excel: {ex.Message}");
            }
        }
    }
}

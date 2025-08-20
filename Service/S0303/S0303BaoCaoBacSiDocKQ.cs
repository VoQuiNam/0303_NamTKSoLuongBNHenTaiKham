using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using Newtonsoft.Json;
using QuestPDF.Fluent;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303BaoCaoBacSiDocKQ : ControllerBase, IC0303BaoCaoBacSiDocKQ
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303BaoCaoBacSiDocKQ(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }

        public async Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh, int idKhoa, int idPhong)
        {
            try
            {
                // Nếu đầu vào null hoặc rỗng thì để DBNull
                object paramTuNgay = string.IsNullOrEmpty(tuNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                object paramDenNgay = string.IsNullOrEmpty(denNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                var data = await _localDb.Set<M0303BaoCaoBacSiDocKQSTO>()
                    .FromSqlRaw(@"EXEC S0303_BaoCaoBacSiDocKQ @TuNgay, @DenNgay, @IDCN, @IdKhoa, @IdPhong",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
                        new SqlParameter("@IDCN", idChiNhanh),
                        new SqlParameter("@IdKhoa", idKhoa),
                        new SqlParameter("@IdPhong", idPhong))
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


        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idKhoa, int idPhong)
        {
            try
            {
                // 1. Lấy dữ liệu từ DB
                var query = _localDb.M0303BaoCaoBacSiDocKQs.AsQueryable();

                if (tuNgay.HasValue)
                    query = query.Where(x => x.Ngay >= tuNgay.Value);

                if (denNgay.HasValue)
                    query = query.Where(x => x.Ngay <= denNgay.Value);

                if (idChiNhanh.HasValue && idChiNhanh > 0)
                    query = query.Where(x => x.IDCN == idChiNhanh.Value);

                if (idKhoa > 0)
                    query = query.Where(x => x.IdKhoa == idKhoa);

                if (idPhong > 0)
                    query = query.Where(x => x.IdPhong == idPhong);

                var data = await query.AsNoTracking().ToListAsync();

                if (data == null || !data.Any())
                    return BadRequest("Không có dữ liệu để xuất PDF");

                // 2. Lấy thông tin doanh nghiệp
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

                // 3. Đường dẫn logo
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                // 4. Tạo document PDF
                var document = new P0303BaoCaoBacSiDocKQ(data, tuNgay, denNgay, idPhong, idKhoa, logoPath, thongTinDoanhNghiep);

                // 5. Xuất PDF ra MemoryStream
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return File(stream, "application/pdf", $"BaoCaoBacSi_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        public async Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn, int? idKhoa = 0, int? idPhong = 0)
        {
            try
            {
                // 1️⃣ Đọc dữ liệu từ JSON
                string khoaJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist", "data", "json", "DM_Khoa.json"));
                var khoaList = JsonConvert.DeserializeObject<List<M0303Khoa>>(khoaJson);

                string phongJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist", "data", "json", "DM_PhongBuong.json"));
                var phongList = JsonConvert.DeserializeObject<List<M0303Phong>>(phongJson);

                // 2️⃣ Chuẩn bị tham số SP
                object paramTuNgay = tuNgay.HasValue ? tuNgay.Value.ToString("dd-MM-yyyy") : (object)DBNull.Value;
                object paramDenNgay = denNgay.HasValue ? denNgay.Value.ToString("dd-MM-yyyy") : (object)DBNull.Value;

                // 3️⃣ Gọi stored procedure
                var reportData = await _localDb.Set<M0303BaoCaoBacSiDocKQSTO>()
                    .FromSqlRaw(@"EXEC S0303_BaoCaoBacSiDocKQ 
                @TuNgay, @DenNgay, @IDCN, @IdKhoa, @IdPhong",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
                        new SqlParameter("@IDCN", idcn ?? (object)DBNull.Value),
                        new SqlParameter("@IdKhoa", idKhoa.HasValue && idKhoa.Value > 0 ? idKhoa.Value : 0),
                        new SqlParameter("@IdPhong", idPhong.HasValue && idPhong.Value > 0 ? idPhong.Value : 0))
                    .AsNoTracking()
                    .ToListAsync();

                if (!reportData.Any())
                    throw new Exception("Không có dữ liệu trong khoảng ngày đã chọn");

                // 4️⃣ Map tên khoa và phòng từ JSON
                var enrichedData = reportData.Select(item =>
                {
                    var khoa = khoaList?.FirstOrDefault(k => k.id == item.IdKhoa);
                    var phong = phongList?.FirstOrDefault(p => p.id == item.IdPhong);

                    return new
                    {
                        item.IdKhoa,
                        TenKhoa = khoa?.ten ?? $"Khoa {item.IdKhoa}",
                        item.IdPhong,
                        TenPhong = phong?.ten ?? $"Phòng {item.IdPhong}",
                        item.BacSiChiDinh,
                        item.ThuPhi,
                        item.BHYT,
                        item.No,
                        item.MienGiam
                    };
                }).ToList();

                // 5️⃣ Lấy thông tin doanh nghiệp
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
                    TenCSKCB = "BỆNH VIỆN UNG BƯỚU",
                    DiaChi = "Số 3 Nơ Trang Long, Phường 12, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    DienThoai = "(028) 38433022",
                    Email = "",
                    Website = "",
                    MaCSKCB = ""
                };

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Báo cáo bác sĩ chỉ định");

                    var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                    if (System.IO.File.Exists(logoPath))
                    {
                        // Chỉ merge A1:A4 cho logo (cột A)
                        ws.Range("A1:A4").Merge();

                        // Điều chỉnh width cho cột A (logo) và cột B (thông tin)
                        ws.Column(1).Width = 20; // Cột A cho logo
                        ws.Column(2).Width = 40; // Cột B cho thông tin doanh nghiệp

                        var img = ws.AddPicture(logoPath)
                            .MoveTo(ws.Cell("A1"), 5, 5) // Căn chỉnh logo trong ô A1
                            .WithPlacement(XLPicturePlacement.FreeFloating)
                            .Scale(0.25);
                    }

                    string tenCoQuan = thongTinDoanhNghiep.TenCoQuanChuyenMon;
                    string tenCSKCB = thongTinDoanhNghiep.TenCSKCB;
                    bool hienTenCSKCB = !string.Equals(tenCoQuan.Trim(), tenCSKCB.Trim(), StringComparison.OrdinalIgnoreCase);
                    string diaChi = thongTinDoanhNghiep.DiaChi;
                    string dienThoai = thongTinDoanhNghiep.DienThoai;

                    // Thông tin doanh nghiệp nằm trong cột B, sát bên phải logo
                    // Dòng 1: Tên cơ quan chuyên môn
                    ws.Cell("B1").Value = tenCoQuan;
                    ws.Cell("B1").Style.Font.FontName = "Times New Roman";
                    ws.Cell("B1").Style.Font.FontSize = 11;
                    ws.Cell("B1").Style.Font.Bold = true;
                    ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B1").Style.Alignment.Indent = 6; // Thụt lề 1 level
                    ws.Row(1).Height = 20;

                    // Dòng 2: Tên CSKCB
                    if (hienTenCSKCB)
                    {
                        ws.Cell("B2").Value = tenCSKCB;
                        ws.Cell("B2").Style.Font.FontName = "Times New Roman";
                        ws.Cell("B2").Style.Font.FontSize = 11;
                        ws.Cell("B2").Style.Font.Bold = true;
                        ws.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell("B2").Style.Alignment.Indent = 6; // Thụt lề 1 level
                        ws.Row(2).Height = 20;
                    }

                    // Dòng 3: Địa chỉ
                    ws.Cell("B3").Value = diaChi;
                    ws.Cell("B3").Style.Font.FontName = "Times New Roman";
                    ws.Cell("B3").Style.Font.FontSize = 10;
                    ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B3").Style.Alignment.Indent = 6; // Thụt lề 1 level
                    ws.Row(3).Height = 18;

                    // Dòng 4: Điện thoại
                    ws.Cell("B4").Value = $"Điện thoại: {dienThoai}";
                    ws.Cell("B4").Style.Font.FontName = "Times New Roman";
                    ws.Cell("B4").Style.Font.FontSize = 10;
                    ws.Cell("B4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B4").Style.Alignment.Indent = 6; // Thụt lề 1 level
                    ws.Row(4).Height = 18;

                    // Tiêu đề báo cáo - nằm giữa bảng (từ cột A đến G)
                    ws.Range("A6:G6").Merge().Value = "BÁO CÁO BÁC SĨ CHỈ ĐỊNH";
                    ws.Range("A6:G6").Style.Font.Bold = true;
                    ws.Range("A6:G6").Style.Font.FontSize = 16;
                    ws.Range("A6:G6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range("A6:G6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Row(6).Height = 30;

                    // Thời gian thống kê - nằm giữa bảng
                    string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                        ? $"Từ ngày {tuNgay.Value:dd/MM/yyyy} đến ngày {denNgay.Value:dd/MM/yyyy}"
                        : "Toàn bộ thời gian";

                    ws.Range("A7:G7").Merge().Value = thoiGianThongKe;
                    ws.Range("A7:G7").Style.Font.Bold = true;
                    ws.Range("A7:G7").Style.Font.FontSize = 12;
                    ws.Range("A7:G7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(7).Height = 22;

                    // Định dạng cột
                    ws.Column(1).Width = 8;    // STT
                    ws.Column(2).Width = 50;   // Bác sĩ (tăng width cho tên khoa dài)
                    ws.Column(3).Width = 15;   // Thu phí
                    ws.Column(4).Width = 15;   // BHYT
                    ws.Column(5).Width = 15;   // Nợ
                    ws.Column(6).Width = 15;   // Miễn giảm
                    ws.Column(7).Width = 15;   // Tổng số ca

                    // Header bảng
                    var headerRow = ws.Row(9);
                    string[] headers = { "STT", "Bác sĩ chỉ định", "Thu phí", "BHYT", "Nợ", "Miễn giảm", "Tổng số ca" };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = ws.Cell(9, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    // 7️⃣ Điền dữ liệu
                    int currentRow = 10;
                    int stt = 1;
                    int sttBacSi = 1; // STT bác sĩ tăng liên tục
                    var khoaGroups = enrichedData.GroupBy(x => x.IdKhoa).OrderBy(x => x.Key);

                    foreach (var khoa in khoaGroups)
                    {
                        int tongCaKhoa = khoa.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));

                        // Dòng tổng khoa - Merge từ cột 1 đến 6, chỉ cột 7 riêng
                        ws.Range(currentRow, 1, currentRow, 6).Merge().Value = $"{stt++}. {khoa.First().TenKhoa.ToUpper()}";
                        ws.Cell(currentRow, 7).Value = tongCaKhoa;
                        ws.Range(currentRow, 1, currentRow, 7).Style.Font.Bold = true;
                        ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // Căn trái cho khoa
                        currentRow++;

                        var phongGroups = khoa.GroupBy(x => x.IdPhong).OrderBy(x => x.Key);
                        foreach (var phong in phongGroups)
                        {
                            int tongCaPhong = phong.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));

                            // Dòng tổng phòng - Merge từ cột 1 đến 6, chỉ cột 7 riêng
                            ws.Range(currentRow, 1, currentRow, 6).Merge().Value = $"KHOA {phong.First().TenPhong.ToUpper()}";
                            ws.Cell(currentRow, 7).Value = tongCaPhong;
                            ws.Range(currentRow, 1, currentRow, 7).Style.Font.Bold = true;
                            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left; // Căn trái cho phòng
                            currentRow++;

                            // Chi tiết bác sĩ
                            foreach (var bacSi in phong.GroupBy(x => x.BacSiChiDinh))
                            {
                                var bacSiData = bacSi.First();
                                int tongThuPhi = bacSi.Sum(x => x.ThuPhi ?? 0);
                                int tongBHYT = bacSi.Sum(x => x.BHYT ?? 0);
                                int tongNo = bacSi.Sum(x => x.No ?? 0);
                                int tongMienGiam = bacSi.Sum(x => x.MienGiam ?? 0);
                                int tongBacSi = tongThuPhi + tongBHYT + tongNo + tongMienGiam;

                                ws.Cell(currentRow, 1).Value = sttBacSi++; // STT tăng liên tục
                                ws.Cell(currentRow, 2).Value = bacSiData.BacSiChiDinh ?? "";
                                ws.Cell(currentRow, 3).Value = tongThuPhi;
                                ws.Cell(currentRow, 4).Value = tongBHYT;
                                ws.Cell(currentRow, 5).Value = tongNo;
                                ws.Cell(currentRow, 6).Value = tongMienGiam;
                                ws.Cell(currentRow, 7).Value = tongBacSi;

                                // Format số
                                for (int col = 3; col <= 7; col++)
                                    ws.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0";

                                currentRow++;
                            }
                        }
                    }

                    // 8️⃣ Border và căn chỉnh
                    var dataRange = ws.Range(9, 1, currentRow - 1, 7);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Căn chỉnh theo ảnh mẫu - BỎ căn giữa cho cột 1
                    ws.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;   // Tên căn trái
                    for (int i = 3; i <= 7; i++)
                        ws.Column(i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Số liệu căn giữa

                    // Đặt căn giữa chỉ cho các ô STT của bác sĩ (không phải khoa/phòng)
                    for (int row = 10; row < currentRow; row++)
                    {
                        // Nếu là dòng bác sĩ (có giá trị số trong cột 1 và không phải merged cell)
                        if (ws.Cell(row, 1).Value.IsNumber && !ws.Cell(row, 1).IsMerged())
                        {
                            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                    }

                    // 9️⃣ Phần ký tên - KHÔNG dùng AdjustToContents()
                    int footerRow = currentRow + 2; // Cách 2 dòng sau dữ liệu

                    // Đặt chiều cao dòng và cỡ chữ mặc định
                    ws.Style.Font.FontSize = 11;
                    ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // CHỈ giữ lại người lập bảng và đặt ở cột F
                    string nguoiKy = "NGƯỜI LẬP BẢNG";
                    string cotKy = "F"; // Cột F

                    // Dòng ngày tháng - Căn giữa từ F đến H (merge 3 cột)
                    ws.Range($"{cotKy}{footerRow}:H{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                    ws.Range($"{cotKy}{footerRow}:H{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{cotKy}{footerRow}:H{footerRow}").Style.Font.Italic = true;
                    ws.Range($"{cotKy}{footerRow}:H{footerRow}").Style.Font.FontSize = 10;
                    ws.Row(footerRow).Height = 20;

                    // Dòng chức danh - Căn giữa từ F đến H (merge 3 cột)
                    ws.Range($"{cotKy}{footerRow + 1}:H{footerRow + 1}").Merge().Value = nguoiKy;
                    ws.Range($"{cotKy}{footerRow + 1}:H{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{cotKy}{footerRow + 1}:H{footerRow + 1}").Style.Font.Bold = true;
                    ws.Range($"{cotKy}{footerRow + 1}:H{footerRow + 1}").Style.Font.FontSize = 10;
                    ws.Row(footerRow + 1).Height = 20;

                    // Dòng ghi chú - Căn giữa từ F đến H (merge 3 cột)
                    string ghiChu = "(Ký, họ tên)";
                    ws.Range($"{cotKy}{footerRow + 2}:H{footerRow + 2}").Merge().Value = ghiChu;
                    ws.Range($"{cotKy}{footerRow + 2}:H{footerRow + 2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{cotKy}{footerRow + 2}:H{footerRow + 2}").Style.Font.FontSize = 10;
                    ws.Range($"{cotKy}{footerRow + 2}:H{footerRow + 2}").Style.Font.Italic = true;
                    ws.Row(footerRow + 2).Height = 20;

                    // 🔟 Xuất file
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"BaoCaoBacSiChiDinh_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo Excel: " + ex.Message);
            }
        }
    }
}

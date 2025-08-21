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


        public async Task<List<M0303BaoCaoBacSiDocKQSTO>> GetBNHenKhamAsync(
            DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idKhoa = 0, int idPhong = 0)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303BaoCaoBacSiDocKQSTOs
                .FromSqlInterpolated($@"
            EXEC S0303_BaoCaoBacSiDocKQ 
                @TuNgay = {tuNgayStr}, 
                @DenNgay = {denNgayStr}, 
                @IDCN = {idCN}, 
                @IdKhoa = {idKhoa}, 
                @IdPhong = {idPhong}")
                .ToListAsync();
        }


        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idKhoa = 0, int idPhong = 0)
        {
            try
            {
            
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idKhoa, idPhong);


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

             
                var document = new P0303BaoCaoBacSiDocKQ(
                    data,
                    tuNgay,
                    denNgay,
                    idPhong,
                    idKhoa,
                    logoPath,
                    thongTinDoanhNghiep
                );

               
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"BaoCaoBacSi_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi tạo PDF: {ex.Message}") { StatusCode = 500 };
            }
        }


        public async Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int? idKhoa = 0, int? idPhong = 0)
        {
            try
            {
          
                var khoaList = JsonConvert.DeserializeObject<List<M0303Khoa>>(System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist", "data", "json", "DM_Khoa.json")));
                var phongList = JsonConvert.DeserializeObject<List<M0303Phong>>(System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist", "data", "json", "DM_PhongBuong.json")));

              
                var reportData = await _localDb.M0303BaoCaoBacSiDocKQSTOs
                    .FromSqlInterpolated($@"
                EXEC S0303_BaoCaoBacSiDocKQ 
                    @TuNgay = {tuNgay?.ToString("dd/MM/yyyy") ?? ""},
                    @DenNgay = {denNgay?.ToString("dd/MM/yyyy") ?? ""},
                    @IDCN = {idChiNhanh ?? 0},
                    @IdKhoa = {idKhoa ?? 0},
                    @IdPhong = {idPhong ?? 0}")
                    .AsNoTracking()
                    .ToListAsync();

                if (!reportData.Any())
                    throw new Exception("Không có dữ liệu trong khoảng ngày đã chọn");

               
                var enrichedData = reportData.Select(item =>
                {
                    var khoa = khoaList.FirstOrDefault(k => k.id == item.IdKhoa);
                    var phong = phongList.FirstOrDefault(p => p.id == item.IdPhong);

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

            
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => x.IDChiNhanh == idChiNhanh)
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

               
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Báo cáo bác sĩ chỉ định");

               
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:A4").Merge();
                    ws.Column(1).Width = 20;
                    ws.Column(2).Width = 40;
                    var img = ws.AddPicture(logoPath)
                                .MoveTo(ws.Cell("A1"), 5, 5)
                                .WithPlacement(XLPicturePlacement.FreeFloating)
                                .Scale(0.25);
                }

               
                string tenCoQuan = thongTinDoanhNghiep.TenCoQuanChuyenMon;
                string tenCSKCB = thongTinDoanhNghiep.TenCSKCB;
                bool hienTenCSKCB = !string.Equals(tenCoQuan.Trim(), tenCSKCB.Trim(), StringComparison.OrdinalIgnoreCase);
                string diaChi = thongTinDoanhNghiep.DiaChi;
                string dienThoai = thongTinDoanhNghiep.DienThoai;

                ws.Cell("B1").Value = tenCoQuan;
                ws.Cell("B1").Style.Font.SetBold().Font.FontName = "Times New Roman".ToString();
                ws.Cell("B1").Style.Font.FontSize = 11;
                ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B1").Style.Alignment.Indent = 6;
                ws.Row(1).Height = 20;

                if (hienTenCSKCB)
                {
                    ws.Cell("B2").Value = tenCSKCB;
                    ws.Cell("B2").Style.Font.SetBold().Font.FontName = "Times New Roman".ToString();
                    ws.Cell("B2").Style.Font.FontSize = 11;
                    ws.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B2").Style.Alignment.Indent = 6;
                    ws.Row(2).Height = 20;
                }

                ws.Cell("B3").Value = diaChi;
                ws.Cell("B3").Style.Font.FontName = "Times New Roman";
                ws.Cell("B3").Style.Font.FontSize = 10;
                ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B3").Style.Alignment.Indent = 6;
                ws.Row(3).Height = 18;

                ws.Cell("B4").Value = $"Điện thoại: {dienThoai}";
                ws.Cell("B4").Style.Font.FontName = "Times New Roman";
                ws.Cell("B4").Style.Font.FontSize = 10;
                ws.Cell("B4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B4").Style.Alignment.Indent = 6;
                ws.Row(4).Height = 18;

                
                ws.Range("A6:G6").Merge().Value = "BÁO CÁO BÁC SĨ CHỈ ĐỊNH";
                ws.Range("A6:G6").Style.Font.SetBold().Font.FontSize = 16;
                ws.Range("A6:G6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A6:G6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(6).Height = 30;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd/MM/yyyy} đến ngày {denNgay.Value:dd/MM/yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:G7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:G7").Style.Font.SetBold().Font.FontSize = 12;
                ws.Range("A7:G7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 22;

        
                ws.Column(1).Width = 8;   
                ws.Column(2).Width = 50;   
                for (int i = 3; i <= 7; i++) ws.Column(i).Width = 15;

                
                string[] headers = { "STT", "Bác sĩ chỉ định", "Thu phí", "BHYT", "Nợ", "Miễn giảm", "Tổng số ca" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(9, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.SetBold();
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                
                int currentRow = 10;
                int sttKhoa = 1;
                int sttBacSi = 1;

                var khoaGroups = enrichedData.GroupBy(x => x.IdKhoa).OrderBy(x => x.Key);
                foreach (var khoa in khoaGroups)
                {
                    int tongCaKhoa = khoa.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));
                    ws.Range(currentRow, 1, currentRow, 6).Merge().Value = $"{sttKhoa++}. {khoa.First().TenKhoa.ToUpper()}";
                    ws.Cell(currentRow, 7).Value = tongCaKhoa;
                    ws.Range(currentRow, 1, currentRow, 7).Style.Font.SetBold();
                    ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    currentRow++;

                    var phongGroups = khoa.GroupBy(x => x.IdPhong).OrderBy(x => x.Key);
                    foreach (var phong in phongGroups)
                    {
                        int tongCaPhong = phong.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));
                        ws.Range(currentRow, 1, currentRow, 6).Merge().Value = $"PHÒNG {phong.First().TenPhong.ToUpper()}";
                        ws.Cell(currentRow, 7).Value = tongCaPhong;
                        ws.Range(currentRow, 1, currentRow, 7).Style.Font.SetBold();
                        ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        currentRow++;

                        foreach (var bacSi in phong.GroupBy(x => x.BacSiChiDinh))
                        {
                            var data = bacSi.First();
                            ws.Cell(currentRow, 1).Value = sttBacSi++;
                            ws.Cell(currentRow, 2).Value = data.BacSiChiDinh ?? "";
                            ws.Cell(currentRow, 3).Value = bacSi.Sum(x => x.ThuPhi ?? 0);
                            ws.Cell(currentRow, 4).Value = bacSi.Sum(x => x.BHYT ?? 0);
                            ws.Cell(currentRow, 5).Value = bacSi.Sum(x => x.No ?? 0);
                            ws.Cell(currentRow, 6).Value = bacSi.Sum(x => x.MienGiam ?? 0);
                            ws.Cell(currentRow, 7).Value = bacSi.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));

                            for (int col = 3; col <= 7; col++)
                                ws.Cell(currentRow, col).Style.NumberFormat.Format = "#,##0";

                            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            currentRow++;
                        }
                    }
                }

                
                var dataRange = ws.Range(9, 1, currentRow - 1, 7);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

               
                int footerRow = currentRow + 2;
                ws.Range($"F{footerRow}:H{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Range($"F{footerRow}:H{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"F{footerRow}:H{footerRow}").Style.Font.Italic = true;
                ws.Range($"F{footerRow}:H{footerRow}").Style.Font.FontSize = 10;
                ws.Row(footerRow).Height = 20;

                ws.Range($"F{footerRow + 1}:H{footerRow + 1}").Merge().Value = "NGƯỜI LẬP BẢNG";
                ws.Range($"F{footerRow + 1}:H{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"F{footerRow + 1}:H{footerRow + 1}").Style.Font.SetBold();
                ws.Range($"F{footerRow + 1}:H{footerRow + 1}").Style.Font.FontSize = 10;
                ws.Row(footerRow + 1).Height = 20;

                ws.Range($"F{footerRow + 2}:H{footerRow + 2}").Merge().Value = "(Ký, họ tên)";
                ws.Range($"F{footerRow + 2}:H{footerRow + 2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"F{footerRow + 2}:H{footerRow + 2}").Style.Font.Italic = true;
                ws.Range($"F{footerRow + 2}:H{footerRow + 2}").Style.Font.FontSize = 10;
                ws.Row(footerRow + 2).Height = 20;

               
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new FileContentResult(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"BaoCaoBacSiChiDinh_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo Excel: " + ex.Message);
            }
        }

    }
}

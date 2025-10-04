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
    public class S0303DanhSachBNThucHienTheoThietBi : ControllerBase, I0303DanhSachBNThucHienTheoThietBi
    {

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303DanhSachBNThucHienTheoThietBi(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }


        public async Task<object> FilterDanhSachBNTheoThietBiAsync(
 string tuNgay,
 string denNgay,
 int idChiNhanh)
        {
            try
            {
                object paramTuNgay = string.IsNullOrEmpty(tuNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                object paramDenNgay = string.IsNullOrEmpty(denNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                var data = await _localDb.Set<M0303DanhSachBNThucHienTheoThietBiSTO>()
                    .FromSqlRaw(@"EXEC S0302_DanhSachBenhNhanThietBi 
                            @TuNgay, @DenNgay, @IdChiNhanh",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
                        new SqlParameter("@IdChiNhanh", idChiNhanh))
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

        public async Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat()
        {
            var dsDichVuKyThuat = await _localDb.Set<M0303DanhSachBNThucHienTheoThietBiSTO>()
                .FromSqlRaw(@"
                        SELECT dvkt.TenDichVu as TenDichVuKyThuat
	                    FROM [dbo].[DM_DichVuKyThuat] dvkt , [dbo].[DM_NhomDichVuKyThuat] ndvkt  
	                    Where dvkt.IDNhomDichVu  = ndvkt.ID")
                .Select(dsdvkt => new M0303DichVuKyThuat
                {
                    ten = dsdvkt.TenDichVuKyThuat ?? ""
                })
                .ToListAsync();

            return dsDichVuKyThuat;
        }



        public async Task<List<M0303NhomDichVuKyThuat>> GetNhomDVKT()
        {
            var nhomDVKT = await _localDb.Set<M0303DanhSachBNThucHienTheoThietBiSTO>()
                .FromSqlRaw(@"SELECT TenDichVu AS TenNhomDichVu FROM [dbo].[DM_NhomDichVuKyThuat]")
                .Select(ndvkt => new M0303NhomDichVuKyThuat
                {
                    ten = ndvkt.TenNhomDichVu ?? ""
                })
                .ToListAsync();

            return nhomDVKT;
        }






        public async Task<List<M0303DanhSachBNThucHienTheoThietBiSTO>> GetBNHenKhamAsync(
        DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303DanhSachBNThucHienTheoThietBiSTOs
                .FromSqlInterpolated($@"
            EXEC S0302_DanhSachBenhNhanThietBi 
                @TuNgay = {tuNgayStr}, 
                @DenNgay = {denNgayStr}, 
                @IdChiNhanh = {idCN}")
                .ToListAsync();
        }

     


        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            try
            {

                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh);


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



                var document = new P0303DanhSachBNThucHienTheoThietBi(
                    data,
                    tuNgay,
                    denNgay,
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

        public async Task<IActionResult> ExportExcel(
   DateTime? tuNgay,
   DateTime? denNgay,
   int? idChiNhanh)
        {
            try
            {
               
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh);
                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất Excel");

                
                var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
                    .AsNoTracking()
                    .Where(x => idChiNhanh.HasValue && x.IDChiNhanh == idChiNhanh.Value)
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
                var ws = workbook.Worksheets.Add("Báo cáo thực hiện theo thiết bị");

               
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:B4").Merge();
                    ws.Column(1).Width = 20;
                    ws.Column(2).Width = 20;
                    ws.AddPicture(logoPath)
                        .MoveTo(ws.Cell("A1"), 30, 5)
                        .WithPlacement(XLPicturePlacement.FreeFloating)
                        .Scale(0.08);
                }

               
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

                var rangeTitle = ws.Range("K6:P6");
                rangeTitle.Merge();
                rangeTitle.Value = "BÁO CÁO BỆNH NHÂN THỰC HIỆN THEO THIẾT BỊ";
                rangeTitle.Style.Font.FontSize = 24;
                rangeTitle.Style.Font.Bold = true;
                rangeTitle.Style.Font.FontName = "Times New Roman";
                rangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Columns("K:P").AdjustToContents();

              
                ws.Row(6).Height = 40;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd-MM-yyyy} đến ngày {denNgay.Value:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";
                ws.Range("K7:Q7").Merge().Value = thoiGianThongKe;
                ws.Range("K7:Q7").Style.Font.Bold = true;
                ws.Range("K7:Q7").Style.Font.FontSize = 12;
                ws.Range("K7:Q7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

               
                string[] headers = {
            "STT","Mã YT","Số HS","Số BA","ICD","Họ và tên","Giới tính","Số BHYT","KCBBD",
            "ĐT","Đối tượng","TT","Nơi chỉ định","Bác sĩ","Nhóm DV","Dịch vụ","SL",
            "Ngày YC","Ngày TH","Quyển ","Số BL","Chứng từ","Thiết bị","Doanh thu","Bảo hiểm",
            "Đã thanh toán","Chưa thanh toán","Hủy Hoàn","Đã thanh toán"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cell(8, i + 1).Value = headers[i];
                    ws.Cell(8, i + 1).Style.Font.Bold = true;
                    ws.Cell(8, i + 1).Style.Fill.BackgroundColor = XLColor.Gray;
                    ws.Cell(8, i + 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(8, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                
                int row = 9;
                int stt = 1;
                int totalSoLuong = 0;
                decimal totalDoanhThu = 0;
                decimal totalDaThanhToan = 0;
                decimal totalChuaThanhToan = 0;
                decimal totalBaoHiem = 0;


                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = item.MaYT;
                    ws.Cell(row, 3).Value = item.SoHS;
                    ws.Cell(row, 4).Value = item.SoBA;
                    ws.Cell(row, 5).Value = item.ICD;
                    ws.Cell(row, 6).Value = item.HoTen;
                    ws.Cell(row, 7).Value = item.TenGioiTinh;
                    ws.Cell(row, 8).Value = item.SoBHYT;
                    ws.Cell(row, 9).Value = item.KCBBD;
                    ws.Cell(row, 10).Value = item.DT;
                    ws.Cell(row, 11).Value = item.DoiTuong;
                    ws.Cell(row, 12).Value = item.TinhTrang;
                    ws.Cell(row, 13).Value = item.NoiChiDinh;
                    ws.Cell(row, 14).Value = item.BacSi;
                    ws.Cell(row, 15).Value = item.TenNhomDichVu;
                    ws.Cell(row, 16).Value = item.TenDichVuKyThuat;
                    ws.Cell(row, 17).Value = item.SoLuong ?? 0;
                    ws.Cell(row, 18).Value = item.NgayYC?.ToString("dd-MM-yyyy hh:mm:ss tt");
                    ws.Cell(row, 19).Value = item.NgayTH?.ToString("dd-MM-yyyy hh:mm:ss tt");
                    ws.Cell(row, 20).Value = item.QuyenSo;
                    ws.Cell(row, 21).Value = item.SoBL;
                    ws.Cell(row, 22).Value = item.ChungTu;
                    ws.Cell(row, 23).Value = item.TenThietBi;

                    ws.Cell(row, 24).Value = (item.DoanhThu.HasValue && item.DoanhThu.Value != 0)
     ? item.DoanhThu.Value.ToString("#,##0")
     : "-";

                    ws.Cell(row, 25).Value = (item.BaoHiem.HasValue && item.BaoHiem.Value != 0)
                        ? item.BaoHiem.Value.ToString("#,##0")
                        : "-";

                    ws.Cell(row, 26).Value = (item.DaThanhToan.HasValue && item.DaThanhToan.Value != 0)
                        ? item.DaThanhToan.Value.ToString("#,##0")
                        : "-";

                    ws.Cell(row, 27).Value = (item.ChuaThanhToan.HasValue && item.ChuaThanhToan.Value != 0)
                        ? item.ChuaThanhToan.Value.ToString("#,##0")
                        : "-";


                    ws.Cell(row, 28).Value = item.HuyHoan;
                    ws.Cell(row, 29).Value = item.TrangThaiThanhToan;

                    totalSoLuong += item.SoLuong ?? 0;
                    totalDoanhThu += item.DoanhThu ?? 0;
                    totalDaThanhToan += item.DaThanhToan ?? 0;
                    totalChuaThanhToan += item.ChuaThanhToan ?? 0;

                    row++;
                }

               
                ws.Cell(row, 1).Value = "Tổng cộng";
                ws.Range(row, 1, row, 16).Merge();
                ws.Cell(row, 24).Value = totalDoanhThu == 0 ? 0 : totalDoanhThu;
                ws.Cell(row, 25).Value = totalBaoHiem == 0 ? 0 : totalBaoHiem;
                ws.Cell(row, 26).Value = totalDaThanhToan == 0 ? 0 : totalDaThanhToan;
                ws.Cell(row, 27).Value = totalChuaThanhToan == 0 ? 0 : totalChuaThanhToan;

                foreach (var c in new int[] { 17, 24, 25, 26, 27 })
                {
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, c).Style.Font.Bold = true;
                }

               
                var dataRange = ws.Range(8, 1, row, headers.Length);
                dataRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                
                ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(29).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

               
                ws.Column(24).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Column(25).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Column(26).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Column(27).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Column(28).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                
                ws.Column(13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Column(13).Style.Alignment.WrapText = true; 


                ws.Columns().AdjustToContents();
                ws.Column(2).Width = 15;
                foreach (var col in new[] { 24, 25, 26, 27 })
                {
                    ws.Cell(8, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

            
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"DanhSachBNThucHienTheoThietBi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi xuất Excel: {ex.Message}") { StatusCode = 500 };
            }
        }

      


    }
}
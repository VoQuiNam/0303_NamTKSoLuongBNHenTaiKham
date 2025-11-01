using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_mien_giam_ngoai_tru")]
    public class C0303BaoCaoMienGiamNgoaiTruController : Controller
    {
        //private string _maChucNang = "/bao_cao_mien_giam_ngoai_tru";
        //private IMemoryCachingServices _memoryCache;
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        //private readonly I0303BaoCaoMienGiamNgoaiTru _service;
        public C0303BaoCaoMienGiamNgoaiTruController(Context0303 localDb, IWebHostEnvironment env
          /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            //_service = service;


            //_memoryCache = memoryCache;
        }
        public IActionResult V0303BaoCaoMienGiamNgoaiTruPage()
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
            return View("~/Views/V0303/V0303BaoCaoMienGiamNgoaiTru/V0303BaoCaoMienGiamNgoaiTruPage.cshtml");
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

            var data = await _localDb.Set<M0303_BaoCaoMienGiamNgoaiTruSTO>()
                .FromSqlRaw(@"EXEC S0303_BaoCaoMienGiamNgoaiTru @TuNgay, @DenNgay, @IDCN",
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

        //[HttpGet("nhom-dich-vu/all")]
        //public async Task<List<M0303NhomDichVuKyThuat>> GetNhomDVKT()
        //{
        //    var nhomDVKT = await _localDb.Set<M0303_BaoCaoMienGiamNgoaiTruSTO>()
        //        .FromSqlRaw(@"SELECT ID AS IDNhomDVKT, TenDichVu AS TenNhomDichVu FROM [dbo].[DM_NhomDichVuKyThuat]")
        //        .Select(ndvkt => new M0303NhomDichVuKyThuat
        //        {
        //            id = ndvkt.IDNhomDVKT,
        //            ten = ndvkt.TenNhomDichVu ?? ""
        //        })
        //        .ToListAsync();

        //    return nhomDVKT;
        //}


       

        //[HttpGet("dich-vu-ky-thuat/all")]
        //public async Task<ActionResult<List<M0303DichVuKyThuat>>> GetDSDichVuKyThuat()
        //{
        //    try
        //    {
        //        var dsDichVuKyThuat = await _localDb
        //            .Set<M0303_BaoCaoMienGiamNgoaiTruSTO>()
        //            .FromSqlRaw(@"
        //            SELECT dvkt.ID AS IDDVKT, dvkt.TenDichVu, ndvkt.ID as IDNhomDVKT, ndvkt.TenDichVu as TenNhomDichVu
	       //             FROM [dbo].[DM_DichVuKyThuat] dvkt , [dbo].[DM_NhomDichVuKyThuat] ndvkt  
	       //             Where dvkt.IDNhomDichVu  = ndvkt.ID")
        //            .Select(dsdvkt => new M0303DichVuKyThuat
        //            {
        //                id = dsdvkt.IDDVKT,
        //                idNhomDichVu = dsdvkt.IDNhomDVKT,
        //                ten = dsdvkt.TenDichVu ?? ""
        //            })
        //            .ToListAsync();

        //        return Ok(dsDichVuKyThuat);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Lỗi khi lấy danh sách dịch vụ kỹ thuật: {ex.Message}");
        //    }
        //}

        private async Task<List<M0303_BaoCaoMienGiamNgoaiTruSTO>> GetBNHenKhamAsync(
    DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, string? idnv)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303_BaoCaoMienGiamNgoaiTruSTOs
                .FromSqlInterpolated($@"
            EXEC S0303_BaoCaoMienGiamNgoaiTru
                @TuNgay = {tuNgayStr}, 
                @DenNgay = {denNgayStr}, 
                @IdChiNhanh = {idCN}")
                .ToListAsync();
        }


       
        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport(
    [FromQuery] DateTime? tuNgay,
    [FromQuery] DateTime? denNgay,
    [FromQuery] int? idcn,
    [FromQuery] string? idnv)
        {
            try
            {
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idcn, idnv);
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
                var ws = workbook.Worksheets.Add("Báo cáo miễn giảm ngoại trú");

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
                ws.Range("K6:P6").Merge().Value = "BÁO CÁO MIỄN GIẢM NGOẠI TRÚ";
                ws.Range("K6:P6").Style.Font.FontSize = 20;
                ws.Range("K6:P6").Style.Font.Bold = true;
                ws.Range("K6:P6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay:dd-MM-yyyy} đến ngày {denNgay:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";
                ws.Range("K7:P7").Merge().Value = thoiGianThongKe;
                ws.Range("K7:P7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("K7:P7").Style.Font.Bold = true;

                // ====== HEADER ======
                int row = 9;
                string[] fixedCols =
                {
            "STT","Mã bệnh nhân","Số bệnh án","Số lưu trữ","Họ tên",
            "Năm sinh","Khoa điều trị","Số chứng từ","Ngày duyệt",
            "Người đề nghị duyệt cấp 1","Người duyệt cấp 2","Tỷ lệ miễn giảm(%)","Số tiền miễn giảm"
        };

                int col = 1;
                foreach (var header in fixedCols)
                {
                    ws.Cell(row, col).Value = header;
                    ws.Range(row, col, row + 1, col).Merge();
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(row, col).Style.Font.Bold = true;
                    col++;
                }

                // ====== CÁC CỘT CHI TIẾT CỐ ĐỊNH ======
                string[] chiTietCols = {
            "Thuốc","Khám bệnh","Xét nghiệm","Siêu âm",
            "DVKT cao(Scanner, MRI,..)","GPB","CDHA","Nội soi","Khác"
        };

                int chiTietStart = col;
                int chiTietEnd = chiTietStart + chiTietCols.Length - 1;

                ws.Range(row, chiTietStart, row, chiTietEnd).Merge().Value = "CHI TIẾT";
                ws.Range(row, chiTietStart, row, chiTietEnd).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(row, chiTietStart, row, chiTietEnd).Style.Font.Bold = true;

                int tongCongCol = chiTietEnd + 1;
                ws.Cell(row, tongCongCol).Value = "Tổng cộng";
                ws.Range(row, tongCongCol, row + 1, tongCongCol).Merge();
                ws.Cell(row, tongCongCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, tongCongCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, tongCongCol).Style.Font.Bold = true;

                // ====== DÒNG HEADER 2 ======
                row++;
                col = chiTietStart;
                foreach (var subCol in chiTietCols)
                    ws.Cell(row, col++).Value = subCol;

                var headerRange = ws.Range(row, chiTietStart, row, chiTietEnd);
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Font.Bold = true;

                // ====== DỮ LIỆU ======
                row++;
                int stt = 1;
                decimal tongMienGiam = 0, tongThuoc = 0, tongKhamBenh = 0, tongXetNghiem = 0,
                        tongSieuAm = 0, tongDVKTCao = 0, tongGPB = 0, tongCDHA = 0,
                        tongNoiSoi = 0, tongKhac = 0, tongTongCong = 0;

                ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                foreach (var item in data)
                {
                    int c = 1;
                    ws.Cell(row, c++).Value = stt++;
                    ws.Cell(row, c).Value = item.MaBenhNhan;
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c++;
                    ws.Cell(row, c++).Value = item.SoBenhAn;
                    ws.Cell(row, c++).Value = item.SoLuuTru;
                    ws.Cell(row, c++).Value = item.HoTen;
                    ws.Cell(row, c).Value = item.NamSinh;
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c++;
                    ws.Cell(row, c++).Value = item.KhoaDieuTri;
                    ws.Cell(row, c++).Value = item.SoChungTu;
                    ws.Cell(row, c).Value = item.NgayDuyet?.ToString("dd-MM-yyyy hh:mm:ss");
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c++;
                    ws.Cell(row, c++).Value = item.NguoiDeNghiDuyetCap1;
                    ws.Cell(row, c++).Value = item.NguoiDuyetCap2;
                    ws.Cell(row, c++).Value = item.TiLeMienGiam ?? 0;

                    var soTienMienGiam = item.SoTienMienGiam ?? 0;
                    ws.Cell(row, c).Value = soTienMienGiam;
                    ws.Cell(row, c).Style.NumberFormat.Format = (soTienMienGiam % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;

                    var thuoc = item.Thuoc ?? 0;
                    ws.Cell(row, c).Value = thuoc;
                    ws.Cell(row, c).Style.NumberFormat.Format = (thuoc % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongThuoc += thuoc;

                    var khambenh = item.KhamBenh ?? 0;
                    ws.Cell(row, c).Value = khambenh;
                    ws.Cell(row, c).Style.NumberFormat.Format = (khambenh % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongKhamBenh += khambenh;

                    var xetnghiem = item.XetNghiem ?? 0;
                    ws.Cell(row, c).Value = xetnghiem;
                    ws.Cell(row, c).Style.NumberFormat.Format = (xetnghiem % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongXetNghiem += xetnghiem;

                    var sieuam = item.SieuAm ?? 0;
                    ws.Cell(row, c).Value = sieuam;
                    ws.Cell(row, c).Style.NumberFormat.Format = (sieuam % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongSieuAm += sieuam;

                    var dvktcao = item.DVKTCao ?? 0;
                    ws.Cell(row, c).Value = dvktcao;
                    ws.Cell(row, c).Style.NumberFormat.Format = (dvktcao % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongDVKTCao += dvktcao;

                    var gpb = item.GPB ?? 0;
                    ws.Cell(row, c).Value = gpb;
                    ws.Cell(row, c).Style.NumberFormat.Format = (gpb % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongGPB += gpb;

                    var cdha = item.CDHA ?? 0;
                    ws.Cell(row, c).Value = cdha;
                    ws.Cell(row, c).Style.NumberFormat.Format = (cdha % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongCDHA += cdha;

                    var noisoi = item.NoiSoi ?? 0;
                    ws.Cell(row, c).Value = noisoi;
                    ws.Cell(row, c).Style.NumberFormat.Format = (noisoi % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongNoiSoi += noisoi;

                    var khac = item.Khac ?? 0;
                    ws.Cell(row, c).Value = khac;
                    ws.Cell(row, c).Style.NumberFormat.Format = (khac % 1 == 0) ? "#,##0" : "#,##0.00";
                    c++;
                    tongKhac += khac;

                    var tongCong = item.TongCong ?? (
                        (item.Thuoc ?? 0) + (item.KhamBenh ?? 0) + (item.XetNghiem ?? 0) +
                        (item.SieuAm ?? 0) + (item.DVKTCao ?? 0) + (item.GPB ?? 0) +
                        (item.CDHA ?? 0) + (item.NoiSoi ?? 0) + (item.Khac ?? 0)
                    );
                    ws.Cell(row, c).Value = tongCong;
                    ws.Cell(row, c).Style.NumberFormat.Format = (tongCong % 1 == 0) ? "#,##0" : "#,##0.00";
                    tongTongCong += tongCong;
                    row++;

                }

                // ====== DÒNG TỔNG ======
                // Dòng "TỔNG CỘNG"
                ws.Cell(row, 1).Value = "TỔNG CỘNG";
                ws.Range(row, 1, row, 12).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Các giá trị tổng
                decimal[] tongValues = new decimal[]
                {
    tongMienGiam, tongThuoc, tongKhamBenh, tongXetNghiem, tongSieuAm,
    tongDVKTCao, tongGPB, tongCDHA, tongNoiSoi, tongKhac, tongTongCong
                };

                // Gán giá trị và định dạng
                col = 13; // bắt đầu từ cột 13
                foreach (var val in tongValues)
                {
                    ws.Cell(row, col).Value = val;
                    ws.Cell(row, col).Style.NumberFormat.Format = (val % 1 == 0) ? "#,##0" : "#,##0.00";
                    col++;
                }


                // ====== STYLE ======
                ws.Range(9, 1, row, tongCongCol).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(9, 1, row, tongCongCol).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 15;   // STT
                ws.Column(2).Width = 15;  // Mã BN/Mã đợt
                ws.Column(3).Width = 20;  // Họ và tên
                ws.Column(4).Width = 10;  // Năm sinh
                ws.Column(5).Width = 20;  // Mã thẻ BHYT
                ws.Column(6).Width = 15;  // Đối tượng
                ws.Column(7).Width = 15;  // Ngày thu
                ws.Column(8).Width = 15;  // Quyển sổ
                ws.Column(9).Width = 20;  // Số biên lai
                ws.Column(10).Width = 25; // Số chứng từ
                ws.Column(11).Width = 20; // Miễn giảm
                ws.Column(12).Width = 20; // Lý do miễn
                ws.Column(13).Width = 20; // Nhập viện nhập miễn
                ws.Column(14).Width = 20; // Ghi chú miễn
                ws.Column(23).Width = 20; // Nợ doạn này của tui đâu

                // ====== FOOTER ======
                int footerRow = row + 2;
                ws.Range($"V{footerRow}:W{footerRow}").Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                ws.Range($"V{footerRow}:W{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"V{footerRow + 1}:W{footerRow + 1}").Merge().Value = "Người lập";
                ws.Range($"V{footerRow + 1}:W{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range($"V{footerRow + 5}:W{footerRow + 5}").Merge().Value = idnv;
                ws.Range($"V{footerRow + 5}:W{footerRow + 5}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // ====== TRẢ FILE ======
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoMienGiamNgoaiTru_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi xuất Excel: {ex.Message}");
            }
        }

    }
}

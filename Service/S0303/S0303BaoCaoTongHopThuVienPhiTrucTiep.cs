using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Fluent;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303BaoCaoTongHopThuVienPhiTrucTiep : ControllerBase, I0303BaoCaoTongHopThuVienPhiTrucTiep
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303BaoCaoTongHopThuVienPhiTrucTiep(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }

        public async Task<object> FilterBaoCaoTongHopThuVienPhiTrucTiepAsync(
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

                var data = await _localDb.Set<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>()
                    .FromSqlRaw(@"EXEC S0303_BaoCaoTongHopThuVienPhiTrucTiep
                            @TuNgay, @DenNgay, @IDCN",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
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


        public async Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat()
        {
            var dsDichVuKyThuat = await _localDb.Set<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>()
                .FromSqlRaw(@"
                        SELECT dvkt.ID AS IDDVKT, dvkt.TenDichVu, ndvkt.ID as IDNhomDVKT, ndvkt.TenDichVu as TenNhomDichVu
	                    FROM [dbo].[DM_DichVuKyThuat] dvkt , [dbo].[DM_NhomDichVuKyThuat] ndvkt  
	                    Where dvkt.IDNhomDichVu  = ndvkt.ID")
                .Select(dsdvkt => new M0303DichVuKyThuat
                {
                    id = dsdvkt.IDDVKT,
                    idNhomDichVu = dsdvkt.IDNhomDVKT,
                    ten = dsdvkt.TenDichVu ?? ""
                })
                .ToListAsync();

            return dsDichVuKyThuat;
        }



        public async Task<List<M0303NhomDichVuKyThuat>> GetNhomDVKT()
        {
            var nhomDVKT = await _localDb.Set<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>()
                .FromSqlRaw(@"SELECT ID AS IDNhomDVKT, TenDichVu AS TenNhomDichVu FROM [dbo].[DM_NhomDichVuKyThuat]")
                .Select(ndvkt => new M0303NhomDichVuKyThuat
                {
                    id = ndvkt.IDNhomDVKT,
                    ten = ndvkt.TenNhomDichVu ?? ""
                })
                .ToListAsync();

            return nhomDVKT;
        }

        public async Task<List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>> GetBNHenKhamAsync(
            DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;
            Console.WriteLine("IDCN: ", idCN);

            return await _localDb.M0303BaoCaoTongHopThuVienPhiTrucTiepSTOs
                .FromSqlInterpolated($@"
            EXEC S0303_BaoCaoTongHopThuVienPhiTrucTiep
                @TuNgay = {tuNgayStr}, 
                @DenNgay = {denNgayStr}, 
                @IdChiNhanh = {idCN}")
                .ToListAsync();
        }

        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            try
            {
                // Lấy dữ liệu từ DB
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh);
                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất PDF");

                // Lấy logo
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                // Lấy thông tin doanh nghiệp
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

                // Lấy danh sách nhóm dịch vụ
                var nhomDichVuList = (await this.GetNhomDVKT())
                    .Select(x => x.ten)
                    .ToList();

                // Tạo document PDF
                var document = new P0303BaoCaoTongHopThuVienPhiTrucTiep(
                    data,           // dữ liệu raw, Compose sẽ tự nhóm theo biên lai + nhóm dịch vụ
                    tuNgay,
                    denNgay,
                    logoPath,
                    thongTinDoanhNghiep
                );

                document.NhomDichVuList = nhomDichVuList;

                // Sinh PDF vào MemoryStream
                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"BaoCaoThuVienPhi_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi tạo PDF: {ex.Message}") { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh)
        {
            try
            {
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh);
                if (!data.Any())
                    return BadRequest("Không có dữ liệu để xuất Excel");

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
                    DiaChi = "Số 3 Nơ Trang Long, Phường 12, Quận Bình Thạnh, TP. HCM",
                    DienThoai = "(028) 38433022",
                    Email = "",
                    Website = "",
                    MaCSKCB = ""
                };

                var nhomDichVuList = (await GetNhomDVKT()).Select(x => x.ten).ToList();

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Báo cáo thu viện phí");

                // 5a. Logo
                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:A4").Merge();
                    ws.Column(1).Width = 20;
                    ws.Column(2).Width = 40;
                    ws.AddPicture(logoPath)
                      .MoveTo(ws.Cell("A1"), 20, 5)
                      .WithPlacement(XLPicturePlacement.FreeFloating)
                      .Scale(0.08);
                }

                // 5b. Thông tin cơ sở
                string tenCoQuan = thongTinDoanhNghiep.TenCoQuanChuyenMon;
                string tenCSKCB = thongTinDoanhNghiep.TenCSKCB;
                bool hienTenCSKCB = !string.Equals(tenCoQuan.Trim(), tenCSKCB.Trim(), StringComparison.OrdinalIgnoreCase);
                string diaChi = thongTinDoanhNghiep.DiaChi;
                string dienThoai = thongTinDoanhNghiep.DienThoai;

                if (hienTenCSKCB)
                {
                    ws.Cell("B1").Value = tenCSKCB;
                    ws.Cell("B1").Style.Font.FontName = "Times New Roman";
                    ws.Cell("B1").Style.Font.FontSize = 10;
                    ws.Cell("B1").Style.Font.Bold = true;
                    ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B1").Style.Alignment.Indent = 1;
                    ws.Row(2).Height = 20;
                }

                ws.Cell("B2").Value = diaChi;
                ws.Cell("B2").Style.Font.FontName = "Times New Roman";
                ws.Cell("B2").Style.Font.FontSize = 10;
                ws.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B2").Style.Alignment.Indent = 1;
                ws.Row(3).Height = 20;

                ws.Cell("B3").Value = $"Điện thoại: {dienThoai}";
                ws.Cell("B3").Style.Font.FontName = "Times New Roman";
                ws.Cell("B3").Style.Font.FontSize = 10;
                ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B3").Style.Alignment.Indent = 1;
                ws.Row(4).Height = 20;

                // --- Tiêu đề báo cáo
                var rangeTitle = ws.Range("K6:P6");
                rangeTitle.Merge();
                rangeTitle.Value = "BÁO CÁO TỔNG HỢP THU VIỆN PHÍ TRỰC TIẾP";
                rangeTitle.Style.Font.FontSize = 24;
                rangeTitle.Style.Font.Bold = true;
                rangeTitle.Style.Font.FontName = "Times New Roman";
                rangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                ws.Row(6).Height = 40; // tăng chiều cao cho tiêu đề

                // --- Thời gian thống kê
                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd-MM-yyyy} đến ngày {denNgay.Value:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";
                ws.Range("K7:Q7").Merge().Value = thoiGianThongKe;
                ws.Range("K7:Q7").Style.Font.Bold = true;
                ws.Range("K7:Q7").Style.Font.FontSize = 12;
                ws.Range("K7:Q7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 9; // bắt đầu bảng dữ liệu từ dòng 9 trở đi


                // --- Bảng dữ liệu
                string[] fixedCols = { "STT", "Mã BN/Mã đợt", "Họ và tên", "Năm sinh", "Mã thẻ BHYT", "Đối tượng", "Ngày thu", "Quyển sổ", "Số biên lai", "Số chứng từ", "Miễn giảm", "Lý do miễn", "Nhập viện nhập miễn", "Ghi chú miễn", "Nợ", "Số tiền" };
                string[] fixedEndCols = { "Hủy", "Hoàn", "Ngày Hủy/Hoàn" };
                int totalCols = fixedCols.Length + 1 + nhomDichVuList.Count + 1 + fixedEndCols.Length;

                int headerStart = row;

                // --- Hàng 1 header
                int colIndex = 1;
                foreach (var fc in fixedCols)
                {
                    ws.Cell(headerStart, colIndex).Value = fc;
                    ws.Range(headerStart, colIndex, headerStart + 1, colIndex).Merge();
                    ws.Cell(headerStart, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(headerStart, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(headerStart, colIndex).Style.Font.Bold = true;
                    ws.Cell(headerStart, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    colIndex++;
                }

                int chiTietColStart = colIndex;
                int chiTietColCount = nhomDichVuList.Count + 2; // Thuốc + nhóm DV + Tổng cộng
                ws.Range(headerStart, chiTietColStart, headerStart, chiTietColStart + chiTietColCount - 1).Merge().Value = "THÔNG TIN CHI TIẾT";
                ws.Range(headerStart, chiTietColStart, headerStart, chiTietColStart + chiTietColCount - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(headerStart, chiTietColStart, headerStart, chiTietColStart + chiTietColCount - 1).Style.Font.Bold = true;
                ws.Cell(headerStart, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;

                colIndex += chiTietColCount;

                foreach (var fc in fixedEndCols)
                {
                    ws.Cell(headerStart, colIndex).Value = fc;
                    ws.Range(headerStart, colIndex, headerStart + 1, colIndex).Merge();
                    ws.Cell(headerStart, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(headerStart, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(headerStart, colIndex).Style.Font.Bold = true;
                    ws.Cell(headerStart, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    colIndex++;
                }

                // --- Hàng 2 header (Thuốc + Nhóm DV + Tổng cộng)
                int row2 = headerStart + 1;
                colIndex = chiTietColStart;
                ws.Cell(row2, colIndex).Value = "Thuốc";
                ws.Cell(row2, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row2, colIndex).Style.Font.Bold = true;
                colIndex++;
                foreach (var nhom in nhomDichVuList)
                {
                    ws.Cell(row2, colIndex).Value = nhom;
                    ws.Cell(row2, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row2, colIndex).Style.Font.Bold = true;
                    colIndex++;
                }
                ws.Cell(row2, colIndex).Value = "Tổng cộng";
                ws.Cell(row2, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row2, colIndex).Style.Font.Bold = true;

                // --- Hàng 3 header (đánh số cột)
                int row3 = headerStart + 2;
                for (int i = 0; i < totalCols; i++)
                {
                    ws.Cell(row3, i + 1).Value = i == 0 ? "A" : i.ToString();
                    ws.Cell(row3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row3, i + 1).Style.Font.Bold = true;
                }

                // --- Fill dữ liệu
                int dataRow = row3 + 1;
                int stt = 1;
                decimal totalMienGiam = 0, totalNo = 0, totalSoTien = 0, totalThuoc = 0;
                Dictionary<string, decimal> tongTheoNhom = nhomDichVuList.Distinct().ToDictionary(n => n, n => 0m);

                ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                foreach (var blGroup in data.GroupBy(x => new { x.MaBN, x.SoBienLai }))
                {
                    var item = blGroup.First();
                    int c = 1;

                    ws.Cell(dataRow, c++).Value = stt++;
                    ws.Cell(dataRow, c++).Value = item.MaBN;
                    ws.Cell(dataRow, c++).Value = item.HoTen;
                    ws.Cell(dataRow, c++).Value = item.NamSinh;
                    ws.Cell(dataRow, c++).Value = item.MaTheBHYT;
                    ws.Cell(dataRow, c++).Value = item.DoiTuong;
                    ws.Cell(dataRow, c++).Value = item.NgayThu?.ToString("dd-MM-yyyy") ?? "-";
                    ws.Cell(dataRow, c++).Value = item.QuyenSo;
                    ws.Cell(dataRow, c++).Value = item.SoBienLai;
                    ws.Cell(dataRow, c++).Value = item.SoChungTu;

                    ws.Cell(dataRow, c).Value = item.MienGiam ?? 0; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(dataRow, c++).Value = item.LyDoMien;
                    ws.Cell(dataRow, c++).Value = item.NhapVienNhapMien;
                    ws.Cell(dataRow, c++).Value = item.GhiChuMien;
                    ws.Cell(dataRow, c).Value = item.No ?? 0; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(dataRow, c).Value = item.SoTien ?? 0; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";

                    // Thuốc
                    decimal tongChiTietBL = item.Thuoc ?? 0;
                    ws.Cell(dataRow, c).Value = tongChiTietBL; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";
                    totalThuoc += tongChiTietBL;

                    // Nhóm DV
                    foreach (var nhom in nhomDichVuList)
                    {
                        var tong = blGroup.Where(x => x.TenNhomDichVu == nhom).Sum(x => x.SoTienChiTiet ?? 0);
                        ws.Cell(dataRow, c).Value = tong; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";
                        tongTheoNhom[nhom] += tong;
                        tongChiTietBL += tong;
                    }

                    ws.Cell(dataRow, c).Value = tongChiTietBL; ws.Cell(dataRow, c++).Style.NumberFormat.Format = "#,##0.00";

                    ws.Cell(dataRow, c++).Value = item.Huy == true ? "1" : "-";
                    ws.Cell(dataRow, c++).Value = item.Hoan == true ? "1" : "-";
                    ws.Cell(dataRow, c++).Value = item.NgayHuyHoan?.ToString("dd-MM-yyyy") ?? "-";

                    totalMienGiam += item.MienGiam ?? 0;
                    totalNo += item.No ?? 0;
                    totalSoTien += item.SoTien ?? 0;

                    dataRow++;
                }

                // --- Dòng tổng cộng
                ws.Cell(dataRow, 1).Value = "TỔNG CỘNG";
                ws.Range(dataRow, 1, dataRow, 10).Merge();

                ws.Cell(dataRow, 11).Value = totalMienGiam; ws.Cell(dataRow, 11).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(dataRow, 15).Value = totalNo; ws.Cell(dataRow, 15).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(dataRow, 16).Value = totalSoTien; ws.Cell(dataRow, 16).Style.NumberFormat.Format = "#,##0.00";

                ws.Cell(dataRow, chiTietColStart).Value = totalThuoc; ws.Cell(dataRow, chiTietColStart).Style.NumberFormat.Format = "#,##0.00";

                colIndex = chiTietColStart + 1;
                foreach (var nhom in nhomDichVuList)
                {
                    ws.Cell(dataRow, colIndex).Value = tongTheoNhom[nhom]; ws.Cell(dataRow, colIndex++).Style.NumberFormat.Format = "#,##0.00";
                }

                ws.Cell(dataRow, chiTietColStart + nhomDichVuList.Count + 1).Value = (totalThuoc + tongTheoNhom.Values.Sum());
                ws.Cell(dataRow, chiTietColStart + nhomDichVuList.Count + 1).Style.NumberFormat.Format = "#,##0.00";

                ws.Cell(dataRow, chiTietColStart + nhomDichVuList.Count + 2).Value = "-";
                ws.Cell(dataRow, chiTietColStart + nhomDichVuList.Count + 3).Value = "-";
                ws.Cell(dataRow, chiTietColStart + nhomDichVuList.Count + 4).Value = "-";

                // --- Border toàn bộ bảng
                ws.Range(headerStart, 1, dataRow, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(headerStart, 1, dataRow, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // --- Căn giữa header
                ws.Range(headerStart, 1, row3, totalCols).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(headerStart, 1, row3, totalCols).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // --- Điều chỉnh độ rộng
                ws.Columns().AdjustToContents();
                ws.Column(1).Width = 15;   // STT
                ws.Column(2).Width = 15;  // Mã BN/Mã đợt
                ws.Column(3).Width = 20;  // Họ và tên
                ws.Column(4).Width = 10;  // Năm sinh
                ws.Column(5).Width = 20;  // Mã thẻ BHYT
                ws.Column(6).Width = 15;  // Đối tượng
                ws.Column(7).Width = 15;  // Ngày thu
                ws.Column(8).Width = 15;  // Quyển sổ
                ws.Column(9).Width = 15;  // Số biên lai
                ws.Column(10).Width = 15; // Số chứng từ
                ws.Column(11).Width = 15; // Miễn giảm
                ws.Column(12).Width = 20; // Lý do miễn
                ws.Column(13).Width = 20; // Nhập viện nhập miễn
                ws.Column(14).Width = 20; // Ghi chú miễn
                ws.Column(15).Width = 15; // Nợ
                ws.Column(16).Width = 15; // Số tiền
                int chiTietStart = 17;
                for (int i = chiTietStart; i <= chiTietStart + nhomDichVuList.Count + 1; i++) ws.Column(i).Width = 20;
                for (int i = chiTietStart + nhomDichVuList.Count + 2; i <= totalCols; i++) ws.Column(i).Width = 15;


                // ================== THÊM CHỮ KÝ ==================
                int footerRow = dataRow + 2;
                string[] nguoiKy = { "THỦ TRƯỞNG ĐƠN VỊ", "THỦ QUỸ", "KẾ TOÁN", "NGƯỜI LẬP BẢNG" };
                string[] cotKyStart = { "K", "M", "O", "Q" };

                for (int i = 0; i < nguoiKy.Length; i++)
                {
                    string colStart = cotKyStart[i];
                    string colEnd = ((char)(colStart[0] + 2)).ToString();

                    if (i == 3) // Người lập bảng thì thêm ngày tháng
                    {
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}")
                            .Merge().Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.Italic = true;
                        ws.Range($"{colStart}{footerRow}:{colEnd}{footerRow}").Style.Font.FontSize = 10;
                    }

                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}")
                        .Merge().Value = nguoiKy[i];
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.Bold = true;
                    ws.Range($"{colStart}{footerRow + 1}:{colEnd}{footerRow + 1}").Style.Font.FontSize = 10;

                    string ghiChu = i == 0 ? "(Ký, họ tên, đóng dấu)" : "(Ký, họ tên)";
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}")
                        .Merge().Value = ghiChu;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.FontSize = 10;
                    ws.Range($"{colStart}{footerRow + 2}:{colEnd}{footerRow + 2}").Style.Font.Italic = true;
                }


                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoThuVienPhi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}

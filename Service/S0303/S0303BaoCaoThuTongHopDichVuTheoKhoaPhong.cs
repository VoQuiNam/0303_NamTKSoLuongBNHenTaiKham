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
    public class S0303BaoCaoThuTongHopDichVuTheoKhoaPhong : ControllerBase, I0303BaoCaoThuTongHopDichVuTheoKhoaPhong
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<S0303BaoCaoThuTongHopDichVuTheoKhoaPhong> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public S0303BaoCaoThuTongHopDichVuTheoKhoaPhong(
            Context0303 localDb,
            IWebHostEnvironment env,
            ILogger<S0303BaoCaoThuTongHopDichVuTheoKhoaPhong> logger,
            IHttpClientFactory httpClientFactory)
        {
            _localDb = localDb;
            _env = env;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        //public async Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh, int idNhomKyThuat, int idPhong)
        //{
        //    try
        //    {
        //        object paramTuNgay = string.IsNullOrEmpty(tuNgay)
        //            ? (object)DBNull.Value
        //            : DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

        //        object paramDenNgay = string.IsNullOrEmpty(denNgay)
        //            ? (object)DBNull.Value
        //            : DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

        //        var data = await _localDb.Set<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>()
        //            .FromSqlRaw(@"EXEC S0303_BaoCaoTongHopDichVuTheoKhoaPhong @TuNgay, @DenNgay, @IDCN, @IdNhomKyThuat, @IdPhong",
        //                new SqlParameter("@TuNgay", paramTuNgay),
        //                new SqlParameter("@DenNgay", paramDenNgay),
        //                new SqlParameter("@IDCN", idChiNhanh),
        //                new SqlParameter("@idNhomKyThuat", idNhomKyThuat),
        //                new SqlParameter("@IdPhong", idPhong))
        //            .AsNoTracking()
        //            .ToListAsync();

        //        var thongTinDoanhNghiep = await _localDb.ThongTinDoanhNghieps
        //            .AsNoTracking()
        //            .Where(x => x.IDChiNhanh == idChiNhanh)
        //            .Select(x => new
        //            {
        //                TenCSKCB = x.TenCSKCB ?? "",
        //                DiaChi = x.DiaChi ?? "",
        //                DienThoai = x.DienThoai ?? "",
        //                Email = x.Email ?? "",
        //                Website = x.Website ?? "",
        //                MaCSKCB = x.MaCSKCB ?? ""
        //            })
        //            .FirstOrDefaultAsync();

        //        return new
        //        {
        //            success = true,
        //            data,
        //            thongTinDoanhNghiep
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ LỖI: {ex.Message}");
        //        return new { success = false, error = ex.Message };
        //    }
        //}

        public async Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh, int idDichVuKyThuat, int idPhong)
        {
            try
            {
                object paramTuNgay = string.IsNullOrEmpty(tuNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(tuNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                object paramDenNgay = string.IsNullOrEmpty(denNgay)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(denNgay, "yyyy-MM-dd", null).ToString("dd-MM-yyyy");

                var data = await _localDb.Set<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>()
                    .FromSqlRaw(@"EXEC S0305_BaoCaoTongHopDichVuTheoKhoaPhong 
                          @TuNgay, @DenNgay, @IDCN, @IDNDVKT, @IDPhongBuong",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
                        new SqlParameter("@IDCN", idChiNhanh),
                        new SqlParameter("@IDNDVKT", idDichVuKyThuat),   // ✅ sửa lại đúng tên
                        new SqlParameter("@IDPhongBuong", idPhong))
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

        public async Task<List<M0303Phong>> GetDSPhongBuong()
        {
            var dsPhongBuong = await _localDb.Set<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>()
                .FromSqlRaw(@"SELECT ID AS IDPhongBuong, TenPhong FROM [dbo].[DM_PhongBuong]")
                .Select(dspb => new M0303Phong
                {
                    id = dspb.IDPhongBuong,
                    ten = dspb.TenPhong ?? ""
                })
                .ToListAsync();

            return dsPhongBuong;
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

        public async Task<List<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>> GetBNHenKhamAsync(
            DateTime? tuNgay,
            DateTime? denNgay,
            int? idChiNhanh,
            int idPhong = 0,
            int idDichVuKyThuat = 0,
            int idNhomDichVu = 0)
        {
        

            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            var data = await _localDb.M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTOs
                .FromSqlInterpolated($@"
                    EXEC S0305_BaoCaoTongHopDichVuTheoKhoaPhong 
                    @TuNgay = {tuNgayStr}, 
                    @DenNgay = {denNgayStr}, 
                    @IDCN = {idCN}, 
                    @IDPhongBuong = {idPhong}, 
                    @IDNDVKT = {idDichVuKyThuat}")
                .ToListAsync();

            //string jsonFile = Path.Combine("wwwroot", "dist/data/json/DM_DichVuKyThuat.json");
            //var jsonData = await System.IO.File.ReadAllTextAsync(jsonFile);
            //var dsDichVu = JsonConvert.DeserializeObject<List<M0303DichVuKyThuat>>(jsonData);

            //if (dsDichVu != null && idNhomDichVu != 0)
            //{
            //    var dsIds = dsDichVu
            //        .Where(x => x.idNhomDichVu == idNhomDichVu)
            //        .Select(x => x.id)
            //        .ToList();

            //    _logger.LogInformation("DEBUG: dsIds.Count = {count}", dsIds.Count);

            //    data = data.Where(x => x.IdDichVuKyThuat.HasValue && dsIds.Contains((int)x.IdDichVuKyThuat.Value)).ToList();

            //    _logger.LogInformation("DEBUG: data.Count sau khi lọc = {count}", data.Count);
            //}
            data.ForEach(item => _logger.LogWarning(item.TenDichVu));

            return data;
        }


        public async Task<IActionResult> ExportToPDF(
      DateTime? tuNgay,
      DateTime? denNgay,
      int? idChiNhanh,
      int idPhong = 0,
      int idDichVuKyThuat = 0,
      int idNhomDichVu = 0)
        {
            // 1. Lấy dữ liệu báo cáo đã lọc
            var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idPhong, idDichVuKyThuat, idNhomDichVu);


            if (!data.Any())
                return new BadRequestObjectResult("Không có dữ liệu để xuất PDF");



            // 2. Lấy logo
            var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

            // 3. Lấy thông tin doanh nghiệp
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

            // 4. Khởi tạo báo cáo
            var document = new P0303BaoCaoThuTongHopDichVuTheoKhoaPhong(
                data,
                tuNgay,
                denNgay,
                idPhong,
                idDichVuKyThuat,
                logoPath,
                thongTinDoanhNghiep,
                idNhomDichVu
            );

            // 5. Đọc danh sách nhóm dịch vụ từ JSON
            //var nhomDichVuJson = await System.IO.File.ReadAllTextAsync(
            //    Path.Combine(_env.WebRootPath, "dist/data/json/DM_NhomDichVuKyThuat.json")
            //);
            //document._nhomdichvukythuatList = JsonConvert.DeserializeObject<List<M0303NhomDichVuKyThuat>>(nhomDichVuJson);

            document._nhomdichvukythuatList = await this.GetNhomDVKT();

            // 6. Đọc danh sách dịch vụ kỹ thuật từ JSON
            //var dichVuJson = await System.IO.File.ReadAllTextAsync(
            //    Path.Combine(_env.WebRootPath, "dist/data/json/DM_DichVuKyThuat.json")
            //);
            //document._dichvukythuatList = JsonConvert.DeserializeObject<List<M0303DichVuKyThuat>>(dichVuJson);

            document._dichvukythuatList = await this.GetDSDichVuKyThuat();

            // 7. Đọc danh sách phòng từ JSON (nếu cần)
            //var phongJson = await System.IO.File.ReadAllTextAsync(
            //    Path.Combine(_env.WebRootPath, "dist/data/json/DM_PhongBuong.json")
            //);
            //document._nhomphongList = JsonConvert.DeserializeObject<List<M0303Phong>>(phongJson);
            document._nhomphongList = await this.GetDSPhongBuong();


            // 8. Generate PDF
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = $"BaoCaoBacSi_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };
        }

        public async Task<IActionResult> ExportExcel(
        DateTime? tuNgay,
        DateTime? denNgay,
        int? idChiNhanh,
        int idPhong = 0,
        int idDichVuKyThuat = 0,
        int idNhomDichVu = 0)
        {
            try
            {
                // 1. Lấy dữ liệu đã lọc
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idPhong, idDichVuKyThuat, idNhomDichVu);
                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất Excel");

                // 2. Thông tin doanh nghiệp
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
                    }).FirstOrDefaultAsync();

                thongTinDoanhNghiep ??= new M0303ThongTinDoanhNghiep
                {
                    TenCoQuanChuyenMon = "SỞ Y TẾ TP. HỒ CHÍ MINH",
                    TenCSKCB = "BỆNH VIỆN UNG BƯỚU",
                    DiaChi = "Số 3 Nơ Trang Long, Phường 12, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    DienThoai = "(028) 38433022"
                };

                // 3. Load danh mục dịch vụ kỹ thuật và nhóm dịch vụ
                var dichVuList = await this.GetDSDichVuKyThuat();
                var nhomDichVuList = await this.GetNhomDVKT();
                if (idNhomDichVu > 0)
                    nhomDichVuList = nhomDichVuList.Where(n => n.id == idNhomDichVu).ToList();

                // 4. Load danh sách phòng
                var phongListJson = await this.GetDSPhongBuong();
                var phongList = phongListJson
                    .Where(p => data.Any(d => d.IDPhongBuong == p.id))
                    .OrderBy(x => x.id)
                    .Select(p => new { p.id, p.ten }).ToList();

                // 5. Tạo workbook Excel
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("BÁO CÁO TỔNG HỢP DỊCH VỤ");

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
                      .Scale(0.2);
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
                    ws.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell("B1").Style.Alignment.Indent = 10;
                    ws.Row(2).Height = 20;
                }

                ws.Cell("B2").Value = diaChi;
                ws.Cell("B2").Style.Font.FontName = "Times New Roman";
                ws.Cell("B2").Style.Font.FontSize = 10;
                ws.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B2").Style.Alignment.Indent = 10;
                ws.Row(3).Height = 20;

                ws.Cell("B3").Value = $"Điện thoại: {dienThoai}";
                ws.Cell("B3").Style.Font.FontName = "Times New Roman";
                ws.Cell("B3").Style.Font.FontSize = 10;
                ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B3").Style.Alignment.Indent = 10;
                ws.Row(4).Height = 20;

                // 6. Tiêu đề báo cáo
                ws.Range("A6:P6").Merge().Value = "BÁO CÁO TỔNG HỢP DỊCH VỤ THEO KHOA/PHÒNG";
                ws.Range("A6:P6").Style.Font.Bold = true;
                ws.Range("A6:P6").Style.Font.FontSize = 20;
                ws.Range("A6:P6").Style.Font.FontName = "Times New Roman";
                ws.Range("A6:P6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range("A6:P6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(6).Height = 40;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd-MM-yyyy} đến ngày {denNgay.Value:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:P7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:P7").Style.Font.Bold = true;
                ws.Range("A7:P7").Style.Font.FontSize = 12;
                ws.Range("A7:P7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 20;

                // 7. Header STT, Dịch vụ, các phòng, Tổng cộng
                int startRow = 9;
                int col = 1;
                ws.Cell(startRow, col++).Value = "STT";
                ws.Cell(startRow, col++).Value = "Dịch vụ";
                foreach (var phong in phongList)
                    ws.Cell(startRow, col++).Value = phong.ten;
                ws.Cell(startRow, col++).Value = "Tổng cộng";

                ws.Range(startRow, 1, startRow, col - 1).Style.Font.Bold = true;
                ws.Range(startRow, 1, startRow, col - 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Range(startRow, 1, startRow, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // 8. Dữ liệu chi tiết
                int row = startRow + 1;
                decimal tongTatCa = 0;
                int stt = 1;
                int sttNhomDichVu = 1;

                foreach (var nhom in nhomDichVuList)
                {
                    var dsDichVu = dichVuList
                        .Where(dv => dv.idNhomDichVu == nhom.id)
                        .ToList();

                    // Chỉ nhóm có dữ liệu
                    var hasData = dsDichVu.Any(dv => data.Any(x => x.IDDVKT == dv.id));
                    if (!hasData)
                        continue;

                    // Tính tổng từng phòng và tổng nhóm
                    var tongTheoPhong = phongList.Select(p =>
                        dsDichVu.Sum(dv => data.Where(x => x.IDDVKT == dv.id && x.IDPhongBuong == p.id)
                                                .Sum(x => (decimal?)x.GiaTien ?? 0))
                    ).ToList();
                    var tongNhom = tongTheoPhong.Sum();
                    tongTatCa += tongNhom;

                    // Dòng tên nhóm dịch vụ
                  
                    var nhomRange = ws.Range(row, 1, row, 2); // merge cột 1 + 2
                    nhomRange.Merge();
                    nhomRange.Value = $"{sttNhomDichVu++}. {nhom.ten}"; // hiển thị STT kế bên tên nhóm
                    nhomRange.Style.Font.Bold = true;
                    nhomRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    nhomRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    nhomRange.Style.Alignment.Indent = 1;
                    

                    // Điền tổng theo phòng
                    col = 3;
                    foreach (var tong in tongTheoPhong)
                    {
                        var cell = ws.Cell(row, col++);
                        cell.Value = tong == 0 ? "" : tong;
                        cell.Style.Font.Bold = true;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        if (tong != 0) cell.Style.NumberFormat.Format = "#,##0";
                    }

                    // Cột Tổng cộng nhóm
                    var cellTongNhom = ws.Cell(row, col++);
                    cellTongNhom.Value = tongNhom == 0 ? "" : tongNhom;
                    cellTongNhom.Style.Font.Bold = true;
                    cellTongNhom.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    if (tongNhom != 0) cellTongNhom.Style.NumberFormat.Format = "#,##0";

                    ws.Row(row).Height = 18;
                    row++;

                    // Dữ liệu chi tiết dịch vụ
                    foreach (var dv in dsDichVu)
                    {
                        var dataDV = data.Where(d => d.IDDVKT == dv.id).ToList();
                        if (!dataDV.Any()) continue;

                        foreach (var item in dataDV)
                        {
                            col = 1;
                            ws.Cell(row, col++).Value = stt++;
                            ws.Cell(row, col - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(row, col - 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            ws.Cell(row, col++).Value = dv.ten;

                            decimal tongDV = 0;
                            foreach (var phong in phongList)
                            {
                                var gia = (item.IDPhongBuong == phong.id) ? item.GiaTien : 0;
                                var cellGia = ws.Cell(row, col++);
                                cellGia.Value = gia == 0 ? "" : gia;
                                cellGia.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                if (gia != 0) cellGia.Style.NumberFormat.Format = "#,##0";

                                tongDV += (decimal)(gia ?? 0);
                            }

                            var cellTongDV = ws.Cell(row, col++);
                            cellTongDV.Value = tongDV == 0 ? "" : tongDV;
                            cellTongDV.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            if (tongDV != 0) cellTongDV.Style.NumberFormat.Format = "#,##0";

                            ws.Row(row).Height = 18;
                            row++;
                        }
                    }
                }

                // 9. Tổng cuối
                int totalCol = 3 + phongList.Count;
                var cellLabel = ws.Range(row, 1, row, totalCol - 1);
                cellLabel.Merge();
                cellLabel.Value = "TỔNG CỘNG";
                cellLabel.Style.Font.Bold = true;
                cellLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                cellLabel.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cellLabel.Style.Alignment.Indent = 1;

                var cellTongTatCa = ws.Cell(row, totalCol);
                cellTongTatCa.Value = tongTatCa == 0 ? "" : tongTatCa;
                cellTongTatCa.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cellTongTatCa.Style.Font.Bold = true;
                if (tongTatCa != 0) cellTongTatCa.Style.NumberFormat.Format = "#,##0";

                ws.Row(row).Height = 18;

                // 10. Set width cột
                ws.Column(1).Width = 6;
                ws.Column(2).Width = 80;

                ws.Column(2).Style.Alignment.WrapText = true;
                ws.Column(2).AdjustToContents();
                for (int i = 0; i < phongList.Count; i++)
                    ws.Column(3 + i).Width = 25;
                ws.Column(3 + phongList.Count).Width = 15;

                // 11. Border
                var dataRange = ws.Range(startRow, 1, row, 3 + phongList.Count);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // 12. Xuất file
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoTongHopDichVu_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }






    }
}

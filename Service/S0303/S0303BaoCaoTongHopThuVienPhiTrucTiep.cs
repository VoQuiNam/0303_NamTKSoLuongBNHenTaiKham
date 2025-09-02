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
 int idChiNhanh,
 int idNhomDichVu)
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
                            @TuNgay, @DenNgay, @IDCN, @IdNhomDichVu",
                        new SqlParameter("@TuNgay", paramTuNgay),
                        new SqlParameter("@DenNgay", paramDenNgay),
                        new SqlParameter("@IDCN", idChiNhanh),
                        new SqlParameter("@IdNhomDichVu", idNhomDichVu))
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


        public async Task<List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>> GetBNHenKhamAsync(
            DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idNhomDichVu = 0)
        {
            string tuNgayStr = tuNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            string denNgayStr = denNgay?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");
            int idCN = idChiNhanh ?? 0;

            return await _localDb.M0303BaoCaoTongHopThuVienPhiTrucTiepSTOs
                .FromSqlInterpolated($@"
            EXEC S0303_BaoCaoTongHopThuVienPhiTrucTiep
                @TuNgay = {tuNgayStr}, 
                @DenNgay = {denNgayStr}, 
                @IDCN = {idCN}, 
                @IdNhomDichVu = {idNhomDichVu}")
                .ToListAsync();
        }


        public async Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idNhomDichVu = 0)
        {
            try
            {

                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idNhomDichVu);


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



                var document = new P0303BaoCaoTongHopThuVienPhiTrucTiep(
                    data,
                    tuNgay,
                    denNgay,
                    idNhomDichVu,
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
       int? idChiNhanh,
       int idNhomDichVu = 0)
        {
            try
            {
                var data = await GetBNHenKhamAsync(tuNgay, denNgay, idChiNhanh, idNhomDichVu);
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

                var pathNhomDV = Path.Combine(_env.WebRootPath, "dist", "data", "json", "DM_NhomDichVuKyThuat.json");
                var nhomDichVuList = System.Text.Json.JsonSerializer.Deserialize<List<M0303NhomDichVuKyThuat>>(System.IO.File.ReadAllText(pathNhomDV));

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Báo cáo thu viện phí");

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ws.Range("A1:B4").Merge();
                    ws.Column(1).Width = 20;
                    ws.Column(2).Width = 20;
                    ws.AddPicture(logoPath)
                      .MoveTo(ws.Cell("A1"), 20, 20)
                      .WithPlacement(XLPicturePlacement.FreeFloating)
                      .Scale(0.2);
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
                ws.Row(1).Height = 20;

                if (hienTenCSKCB)
                {
                    ws.Range("C2:O2").Merge().Value = tenCSKCB;
                    ws.Range("C2:O2").Style.Font.FontName = "Times New Roman";
                    ws.Range("C2:O2").Style.Font.FontSize = 10;
                    ws.Range("C2:O2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Row(2).Height = 18;
                }

                ws.Range("C3:O3").Merge().Value = diaChi;
                ws.Range("C3:O3").Style.Font.FontName = "Times New Roman";
                ws.Range("C3:O3").Style.Font.FontSize = 10;
                ws.Range("C3:O3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Row(3).Height = 18;

                ws.Range("C4:O4").Merge().Value = $"Điện thoại: {dienThoai}";
                ws.Range("C4:O4").Style.Font.FontName = "Times New Roman";
                ws.Range("C4:O4").Style.Font.FontSize = 10;
                ws.Range("C4:O4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Row(4).Height = 18;

                ws.Range("A6:P6").Merge().Value = "BÁO CÁO TỔNG HỢP THU VIỆN PHÍ TRỰC TIẾP";
                ws.Range("A6:P6").Style.Font.Bold = true;
                ws.Range("A6:P6").Style.Font.FontSize = 18;
                ws.Range("A6:P6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(6).Height = 30;

                string thoiGianThongKe = tuNgay.HasValue && denNgay.HasValue
                    ? $"Từ ngày {tuNgay.Value:dd-MM-yyyy} đến ngày {denNgay.Value:dd-MM-yyyy}"
                    : "Toàn bộ thời gian";

                ws.Range("A7:P7").Merge().Value = thoiGianThongKe;
                ws.Range("A7:P7").Style.Font.Bold = true;
                ws.Range("A7:P7").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Row(7).Height = 20;

                int startRow = ws.LastRowUsed().RowNumber() + 2;
                string[] fixedCols = {
            "STT", "Mã BN/Mã đợt", "Họ và tên", "Năm sinh", "Mã thẻ BHYT",
            "Đối tượng", "Ngày thu", "Quyển sổ", "Số biên lai", "Số chứng từ",
            "Miễn giảm", "Lý do miễn", "Nhập viện nhập miễn",
            "Ghi chú miễn", "Nợ", "Số tiền"
        };
                string[] fixedEndCols = { "Hủy", "Hoàn", "Ngày Hủy/Hoàn" };

                int colIndex = 1;
                int totalCols = fixedCols.Length + 1 + nhomDichVuList.Count + 1 + fixedEndCols.Length;

                foreach (var fc in fixedCols)
                {
                    ws.Cell(startRow, colIndex).Value = fc;
                    ws.Range(startRow, colIndex, startRow + 1, colIndex).Merge();
                    ws.Cell(startRow, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(startRow, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(startRow, colIndex).Style.Font.Bold = true;
                    ws.Cell(startRow, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    colIndex++;
                }

                int chiTietStart = colIndex;
                int chiTietEnd = colIndex + nhomDichVuList.Count + 1;
                ws.Range(startRow, chiTietStart, startRow, chiTietEnd).Merge().Value = "THÔNG TIN CHI TIẾT";
                ws.Range(startRow, chiTietStart, startRow, chiTietEnd).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(startRow, chiTietStart, startRow, chiTietEnd).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Range(startRow, chiTietStart, startRow, chiTietEnd).Style.Font.Bold = true;
                ws.Range(startRow, chiTietStart, startRow, chiTietEnd).Style.Fill.BackgroundColor = XLColor.LightGray;

                int endColIndex = chiTietEnd + 1;
                foreach (var fe in fixedEndCols)
                {
                    ws.Cell(startRow, endColIndex).Value = fe;
                    ws.Range(startRow, endColIndex, startRow + 1, endColIndex).Merge();
                    ws.Cell(startRow, endColIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(startRow, endColIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(startRow, endColIndex).Style.Font.Bold = true;
                    ws.Cell(startRow, endColIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    endColIndex++;
                }

            
                colIndex = fixedCols.Length + 1;
                ws.Cell(startRow + 1, colIndex).Value = "Thuốc";
                ws.Cell(startRow + 1, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow + 1, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(startRow + 1, colIndex).Style.Font.Bold = true;
                ws.Cell(startRow + 1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                colIndex++;

                foreach (var nhom in nhomDichVuList)
                {
                    ws.Cell(startRow + 1, colIndex).Value = nhom.ten;
                    ws.Cell(startRow + 1, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(startRow + 1, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    ws.Cell(startRow + 1, colIndex).Style.Font.Bold = true;
                    ws.Cell(startRow + 1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    colIndex++;
                }

                ws.Cell(startRow + 1, colIndex).Value = "Tổng cộng";
                ws.Cell(startRow + 1, colIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(startRow + 1, colIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(startRow + 1, colIndex).Style.Font.Bold = true;
                ws.Cell(startRow + 1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;

                ws.Row(startRow).Height = 25;
                ws.Row(startRow + 1).Height = 22;

                ws.Range(startRow, 1, startRow + 1, totalCols).Style.Alignment.WrapText = true;
                ws.Range(startRow, 1, startRow + 1, totalCols).Style.Font.FontSize = 11;

                
                int colThuoc = 17;
                int colNhomDichVuStart = colThuoc + 1;
                int colTongCong = colNhomDichVuStart + nhomDichVuList.Count;
                int colHuy = colTongCong + 1;
                int colHoan = colHuy + 1;
                int colNgayHuyHoan = colHoan + 1;

                
                ws.Column(1).Width = 8;
                ws.Column(2).Width = 15;
                ws.Column(3).Width = 25;
                ws.Column(4).Width = 12;
                ws.Column(5).Width = 20;
                ws.Column(6).Width = 20;
                ws.Column(7).Width = 20;
                ws.Column(8).Width = 15;
                ws.Column(9).Width = 15;
                ws.Column(10).Width = 15;
                ws.Column(11).Width = 15;
                ws.Column(12).Width = 25;
                ws.Column(13).Width = 25;
                ws.Column(14).Width = 25;
                ws.Column(15).Width = 15;
                ws.Column(16).Width = 15;

                
                for (int i = 0; i < nhomDichVuList.Count; i++)
                    ws.Column(colNhomDichVuStart + i).Width = 20;

                ws.Column(colTongCong).Width = 15;   
                ws.Column(colHuy).Width = 10;         
                ws.Column(colHoan).Width = 10;        
                ws.Column(colNgayHuyHoan).Width = 25; 

               
                int row = startRow + 2;
                int stt = 1;
                decimal totalMienGiam = 0, totalNo = 0, totalSoTien = 0;
                Dictionary<int, decimal> tongTheoNhom = nhomDichVuList.ToDictionary(n => n.id, n => 0m);
                decimal totalTongCongChiTiet = 0;

                foreach (var item in data)
                {
                    int c = 1;
                    ws.Cell(row, c).Value = stt++;
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c++;
                    ws.Cell(row, c++).Value = item.MaBN;
                    ws.Cell(row, c - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, c++).Value = item.HoTen;
                    ws.Cell(row, c++).Value = item.NamSinh;
                    ws.Cell(row, c - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, c++).Value = item.MaTheBHYT;
                    ws.Cell(row, c - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, c++).Value = item.DoiTuong;

                 
                    if (item.NgayThu.HasValue)
                    {
                        ws.Cell(row, c).Value = item.NgayThu.Value;
                        ws.Cell(row, c).Style.DateFormat.Format = "dd-MM-yyyy HH:mm:ss";
                    }
                    else
                    {
                        ws.Cell(row, c).Value = "-";
                    }
                    c++;

                   
                    ws.Cell(row, c).Value = string.IsNullOrEmpty(item.QuyenSo) ? "-" : item.QuyenSo;
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                   
                    ws.Cell(row, c).Value = string.IsNullOrEmpty(item.SoBienLai) ? "-" : item.SoBienLai;
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                   
                    ws.Cell(row, c).Value = string.IsNullOrEmpty(item.SoChungTu) ? "-" : item.SoChungTu;
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                   
                    ws.Cell(row, c).Value = (item.MienGiam ?? 0) == 0 ? "-" : item.MienGiam;
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                   
                    ws.Cell(row, c++).Value = item.LyDoMien;

                   
                    ws.Cell(row, c++).Value = item.NhapVienNhapMien;

                   
                    ws.Cell(row, c++).Value = item.GhiChuMien;

                   
                    ws.Cell(row, c).Value = (item.No ?? 0) == 0 ? "-" : item.No;
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                   
                    ws.Cell(row, c).Value = (item.SoTien ?? 0) == 0 ? "-" : item.SoTien;
                    ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    totalMienGiam += item.MienGiam ?? 0;
                    totalNo += item.No ?? 0;
                    totalSoTien += item.SoTien ?? 0;

                    decimal tongChiTiet = 0;

                    
                    ws.Cell(row, c).Value = "-";
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    
                    foreach (var nhom in nhomDichVuList)
                    {
                        if (item.IdNhomDichVu == nhom.id && (item.SoTienChiTiet ?? 0) != 0)
                        {
                            ws.Cell(row, c).Value = item.SoTienChiTiet;
                            ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                            ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            tongChiTiet += item.SoTienChiTiet ?? 0;
                            tongTheoNhom[nhom.id] += item.SoTienChiTiet ?? 0;
                            totalTongCongChiTiet += item.SoTienChiTiet ?? 0;
                        }
                        else
                        {
                            ws.Cell(row, c).Value = "-";
                            ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        c++;
                    }
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                  
                    ws.Cell(row, c).Value = tongChiTiet == 0 ? "-" : tongChiTiet;
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    if (tongChiTiet != 0)
                        ws.Cell(row, c).Style.NumberFormat.Format = "#,##0";
                    c++;


                 
                    ws.Cell(row, c).Value = (item.Huy ?? 0) == 0 ? "-" : item.Huy;
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    
                    ws.Cell(row, c).Value = (item.Hoan ?? 0) == 0 ? "-" : item.Hoan;
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                   
                    if (item.NgayHuyHoan.HasValue)
                    {
                        ws.Cell(row, c).Value = item.NgayHuyHoan.Value;
                        ws.Cell(row, c).Style.DateFormat.Format = "dd-MM-yyyy HH:mm:ss";
                    }
                    else
                    {
                        ws.Cell(row, c).Value = "-";
                    }
                    ws.Cell(row, c++).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    row++;
                }

                
                int colIndex_MienGiam = 11;
                int colIndex_No = 15;
                int colIndex_SoTien = 16;

                ws.Cell(row, 1).Value = "TỔNG CỘNG";
                ws.Range(row, 1, row, colIndex_MienGiam - 1).Merge();
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(row, colIndex_MienGiam).Value = totalMienGiam == 0 ? "-" : totalMienGiam;
                ws.Cell(row, colIndex_MienGiam).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                if (totalMienGiam != 0)
                    ws.Cell(row, colIndex_MienGiam).Style.NumberFormat.Format = "#,##0";

                
                ws.Cell(row, colIndex_No).Value = totalNo == 0 ? "-" : totalNo;
                ws.Cell(row, colIndex_No).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, colIndex_No).Style.Font.Bold = true;
                if (totalNo != 0)
                    ws.Cell(row, colIndex_No).Style.NumberFormat.Format = "#,##0";

               
                ws.Cell(row, colIndex_SoTien).Value = totalSoTien == 0 ? "-" : totalSoTien;
                ws.Cell(row, colIndex_SoTien).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, colIndex_SoTien).Style.Font.Bold = true;
                if (totalSoTien != 0)
                    ws.Cell(row, colIndex_SoTien).Style.NumberFormat.Format = "#,##0";

               
                int colIndex_Thuoc = colThuoc;
                ws.Cell(row, colIndex_Thuoc).Value = "-";
                ws.Cell(row, colIndex_Thuoc).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, colIndex_Thuoc).Style.Font.Bold = true;

                
                int colChiTietStart_total = fixedCols.Length + 2;
                for (int i = 0; i < nhomDichVuList.Count; i++)
                {
                    var nhom = nhomDichVuList[i];
                    var giaTriNhom = tongTheoNhom[nhom.id];
                    ws.Cell(row, colChiTietStart_total + i).Value = giaTriNhom == 0 ? "-" : giaTriNhom;
                    ws.Cell(row, colChiTietStart_total + i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(row, colChiTietStart_total + i).Style.Font.Bold = true;
                    if (giaTriNhom != 0)
                        ws.Cell(row, colChiTietStart_total + i).Style.NumberFormat.Format = "#,##0";

                }

                
                var tongCongChiTiet = totalTongCongChiTiet;
                ws.Cell(row, colChiTietStart_total + nhomDichVuList.Count).Value = tongCongChiTiet == 0 ? "-" : tongCongChiTiet;
                ws.Cell(row, colChiTietStart_total + nhomDichVuList.Count).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, colChiTietStart_total + nhomDichVuList.Count).Style.Font.Bold = true;
                if (tongCongChiTiet != 0)
                    ws.Cell(row, colChiTietStart_total + nhomDichVuList.Count).Style.NumberFormat.Format = "#,##0";


                
                ws.Column(colNgayHuyHoan).AdjustToContents();

                
                if (ws.Column(colNgayHuyHoan).Width < 25)
                {
                    ws.Column(colNgayHuyHoan).Width = 25;
                }

                
                ws.Range(startRow, 1, row, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(startRow, 1, row, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"BaoCaoThuVienPhi_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }




    }
}

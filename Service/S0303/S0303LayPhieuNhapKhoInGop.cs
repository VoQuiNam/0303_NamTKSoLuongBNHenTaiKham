using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Fluent;
namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303LayPhieuNhapKhoInGop : I0303LayPhieuNhapKhoInGop
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;

        public S0303LayPhieuNhapKhoInGop(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }


        public async Task<IActionResult> ExportToPDF(long idPhieuNhapKho, int? idChiNhanh)
        {
            try
            {
                // 1. Lấy dữ liệu phiếu nhập kho từ stored procedure
                var data = await _localDb.M0303PhieuNhapKhoSTOs
                    .FromSqlInterpolated($@"
                EXEC [dbo].[S0303_PhieuNhapKho]
                    @IDPhieuNhapKho = {idPhieuNhapKho},
                    @IdChiNhanh = {idChiNhanh}
            ")
                    .ToListAsync();

                if (!data.Any())
                    return new BadRequestObjectResult("Không có dữ liệu để xuất PDF");

                // 2. Lấy thông tin doanh nghiệp theo chi nhánh
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

                var logoPath = Path.Combine(_env.WebRootPath, "dist", "img", "logo.png");

                // 3. Tạo PDF bằng QuestPDF
                var document = new P0303PhieuNhapKhoInGop(
                    data,
                    idPhieuNhapKho, // truyền thêm nếu cần cho tiêu đề hoặc footer
                    logoPath,
                    thongTinDoanhNghiep
                );

                var stream = new MemoryStream();
                document.GeneratePdf(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = $"PhieuNhapKho_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Lỗi khi tạo PDF: {ex.Message}") { StatusCode = 500 };
            }
        }

    }
}
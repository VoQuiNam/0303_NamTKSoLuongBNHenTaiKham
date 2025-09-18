using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_thu_tong_hop_dv_theo_khoa_phong")]
    public class C0303BaoCaoThuTongHopDichVuTheoKhoaPhongController : Controller
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly I0303BaoCaoThuTongHopDichVuTheoKhoaPhong _service;
        private readonly ILogger<C0303BaoCaoThuTongHopDichVuTheoKhoaPhongController> _logger;

        public C0303BaoCaoThuTongHopDichVuTheoKhoaPhongController(
            Context0303 localDb, 
            IWebHostEnvironment env, 
            I0303BaoCaoThuTongHopDichVuTheoKhoaPhong service,
            ILogger<C0303BaoCaoThuTongHopDichVuTheoKhoaPhongController> logger)
        {
            _localDb = localDb;
            _env = env;
            _service = service;
            _logger = logger;
        }

        public IActionResult V0303BaoCaoThuTongHopDichVuTheoKhoaPhongPage()
        {
            return View("~/Views/V0303/V0303BaoCaoThuTongHopDichVuTheoKhoaPhong/V0303BaoCaoThuTongHopDichVuTheoKhoaPhongPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay, int idChiNhanh, int idDichVuKyThuat, int idPhong)
        {
            try
            {
                Console.WriteLine($"TuNgay={tuNgay}, DenNgay={denNgay}, IDCN={idChiNhanh}, IDDVKT={idDichVuKyThuat}, IDPhong={idPhong}");

                var result = await _service.FilterByDayAsync(tuNgay, denNgay, idChiNhanh, idDichVuKyThuat, idPhong);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPDF(
    [FromQuery] DateTime? tuNgay,
    [FromQuery] DateTime? denNgay,
    [FromQuery] int? idChiNhanh,
    [FromQuery] int idPhong = 0,
    [FromQuery] int idDichVuKyThuat = 0,
    [FromQuery] int idNhomDichVu = 0)
        {
            try
            {
                // Truyền idNhomDichVu xuống service
                return await _service.ExportToPDF(tuNgay, denNgay, idChiNhanh, idPhong, idDichVuKyThuat, idNhomDichVu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idcn, [FromQuery] int idPhong, [FromQuery] int idDichVuKyThuat = 0,
        [FromQuery] int idNhomDichVu = 0)
        {
            try
            {
                var list = await _service.GetBNHenKhamAsync(tuNgay, denNgay, idcn, idDichVuKyThuat, idNhomDichVu);

                if (!list.Any())
                {
                    return Ok(new { hasData = false, message = "Không có dữ liệu trong khoảng ngày đã chọn" });
                }

                var result = await _service.ExportExcel(tuNgay, denNgay, idcn, idPhong, idDichVuKyThuat, idNhomDichVu);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi tạo Excel: {ex.Message}" });
            }
        }

        [HttpGet("nhom-dich-vu/all")]
        public async Task<List<M0303NhomDichVuKyThuat>> LayNhomDichVu()
        {
            try
            {
                var nhomDVKT = await _service.GetNhomDVKT();
                return nhomDVKT;
            }
            catch (Exception ex)
            {
                throw;
              
            }

        }

        [HttpGet("phong-buong/all")]
        public async Task<List<M0303Phong>> GetDSPhongBuong()
        {
            try
            {
                var dsPhongBuong = await _service.GetDSPhongBuong();
                return dsPhongBuong;
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet("dich-vu-ky-thuat/all")]
        public async Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat()
        {
            try
            {
                var dsDVuKT = await _service.GetDSDichVuKyThuat();
                return dsDVuKT;

            }
            catch (Exception ex)
            {
                throw;
            }

        }


    }
}

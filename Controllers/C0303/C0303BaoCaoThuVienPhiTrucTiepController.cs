using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_thu_vien_phi_truc_tiep")]
    public class C0303BaoCaoThuVienPhiTrucTiepController : Controller
    {
        //private string _maChucNang = "/bao_cao_thu_vien_phi_truc_tiep";
        //private IMemoryCachingServices _memoryCache;
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly I0303BaoCaoTongHopThuVienPhiTrucTiep _service;


        public C0303BaoCaoThuVienPhiTrucTiepController(Context0303 localDb, IWebHostEnvironment env, I0303BaoCaoTongHopThuVienPhiTrucTiep service
            /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            _service = service;


            //_memoryCache = memoryCache;
        }

        public IActionResult V0303BaoCaoThuVienPhiTrucTiepPage()
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
            return View("~/Views/V0303/V0303BaoCaoThuVienPhiTrucTiep/V0303BaoCaoThuVienPhiTrucTiepPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay,
    int idChiNhanh,
    int idNhomDichVu)
        {
            try
            {
                var result = await _service.FilterBaoCaoTongHopThuVienPhiTrucTiepAsync(tuNgay, denNgay, idChiNhanh, idNhomDichVu);
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
  [FromQuery] int idNhomDichVu = 0)
        {
            try
            {
                return await _service.ExportToPDF(tuNgay, denNgay, idChiNhanh, idNhomDichVu);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idcn, [FromQuery] int idNhomDichVu = 0)
        {
            try
            {
                var list = await _service.GetBNHenKhamAsync(tuNgay, denNgay, idcn, idNhomDichVu);

                if (!list.Any())
                {
                    return Ok(new { hasData = false, message = "Không có dữ liệu trong khoảng ngày đã chọn" });
                }

                var result = await _service.ExportExcel(tuNgay, denNgay, idcn, idNhomDichVu);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi tạo Excel: {ex.Message}" });
            }
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_bac_si_doc_kq")]
    public class C0303BaoCaoBacSiDocKQController : Controller
    {
        //private string _maChucNang = "/bao_cao_bac_si_doc_kq";
        //private IMemoryCachingServices _memoryCache;
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly IC0303BaoCaoBacSiDocKQ _service;


        public C0303BaoCaoBacSiDocKQController(Context0303 localDb, IWebHostEnvironment env, IC0303BaoCaoBacSiDocKQ service
            /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            _service = service;


            //_memoryCache = memoryCache;
        }
        public IActionResult V0303BaoCaoBacSiDocKQPage()
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
            return View("~/Views/V0303/V0303BaoCaoBacSiDocKQ/V0303BaoCaoBacSiDocKQPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay, int idChiNhanh, int idKhoa, int idPhong)
        {
            try
            {
                var result = await _service.FilterByDayAsync(tuNgay, denNgay, idChiNhanh, idKhoa,idPhong);
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
       [FromQuery] int idKhoa = 0,
       [FromQuery] int idPhong = 0)
        {
            try
            {
                return await _service.ExportToPDF(tuNgay, denNgay, idChiNhanh, idKhoa, idPhong);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }



        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idcn, [FromQuery] int idKhoa = 0,
 [FromQuery] int idPhong = 0)
        {
            try
            {
                var list = await _service.GetBNHenKhamAsync(tuNgay, denNgay, idcn, idKhoa, idPhong);

                if (!list.Any())
                {
                    return Ok(new { hasData = false, message = "Không có dữ liệu trong khoảng ngày đã chọn" });
                }

                var result = await _service.ExportExcel(tuNgay, denNgay, idcn, idKhoa, idPhong);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi tạo Excel: {ex.Message}" });
            }
        }


    }
}

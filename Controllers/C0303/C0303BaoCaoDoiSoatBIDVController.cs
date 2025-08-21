using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_doi_soat_bidv")]
    public class C0303BaoCaoDoiSoatBIDVController : Controller
    {
        //private string _maChucNang = "/bao_cao_doi_soat_bidv";
        //private IMemoryCachingServices _memoryCache;

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly I0303BaoCaoDoiSoatBIDV _service;

        public C0303BaoCaoDoiSoatBIDVController(Context0303 localDb, IWebHostEnvironment env
            , I0303BaoCaoDoiSoatBIDV service /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            _service = service;

            //_memoryCache = memoryCache;
        }

        public IActionResult V0303BaoCaoDoiSoatBIDVPage()
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

            return View("~/Views/V0303/V0303BaoCaoDoiSoatBIDV/V0303BaoCaoDoiSoatBIDVPage.cshtml");
        }

        [HttpPost("tk/FilterByDay")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay, int idChiNhanh)
        {
            try
            {
                var result = await _service.FilterByDayAsync(tuNgay, denNgay, idChiNhanh);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportToPDF([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idChiNhanh)
        {
            try
            {
                return await _service.ExportToPDF(tuNgay, denNgay, idChiNhanh);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet("check-and-export")]
        public async Task<IActionResult> CheckAndExport([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idcn)
        {
            try
            {
              
                var list = await _service.GetBNHenKhamAsync(tuNgay, denNgay, idcn);

                if (!list.Any())
                    return BadRequest(new { hasData = false, message = "Không có dữ liệu trong khoảng ngày đã chọn" });

            
                var result = await _service.ExportExcel(tuNgay, denNgay, idcn);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo Excel: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_bac_si_doc_kq")]
    public class C0303BaoCaoBacSiDocKQController : Controller
    {

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
            var danhSach = _localDb.M0303BaoCaoBacSiDocKQs.ToList();
            ViewBag.DanhSach = danhSach;
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
                // Gọi service với đủ parameter
                return await _service.ExportToPDF(tuNgay, denNgay, idChiNhanh, idKhoa, idPhong);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo PDF: {ex.Message}");
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] DateTime? tuNgay,
                                              [FromQuery] DateTime? denNgay,
                                              [FromQuery] int? idcn,
                                              [FromQuery] int idKhoa,
                                              [FromQuery] int idPhong)
        {
            try
            {
                var result = await _service.ExportExcel(tuNgay, denNgay, idcn, idKhoa, idPhong);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("check-data")]
        public IActionResult CheckData([FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay, [FromQuery] int? idcn)
        {
            var query = _localDb.M0303BaoCaoBacSiDocKQs.AsQueryable()
                .Where(x => x.Ngay >= tuNgay && x.Ngay <= denNgay);

            if (idcn.HasValue && idcn.Value > 0)
            {
                query = query.Where(x => x.IDCN == idcn.Value);
            }

            bool hasData = query.Any();
            return Ok(new { hasData });
        }
    }
}

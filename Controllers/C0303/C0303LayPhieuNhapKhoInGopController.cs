using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("lay_phieu_nhap_kho_in_gop")]
    public class C0303LayPhieuNhapKhoInGopController : Controller
    {

        //private string _maChucNang = "/lay_phieu_nhap_kho_in_gop";
        //private IMemoryCachingServices _memoryCache;

        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly I0303LayPhieuNhapKhoInGop _service;

        public C0303LayPhieuNhapKhoInGopController(Context0303 localDb, IWebHostEnvironment env, I0303LayPhieuNhapKhoInGop service
            /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            _service = service;


            //_memoryCache = memoryCache;
        }

        public IActionResult V0303LayPhieuNhapKhoInGopPage()
        {
            //var quyenVaiTro = await _memoryCache.getQuyenVaiTro(_maChucNang);
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

            return View("~/Views/V0303/V0303LayPhieuNhapKhoInGop/V0303LayPhieuNhapKhoInGopPage.cshtml");
        }


        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportToPDF(
      [FromQuery] long idPhieuNhapkho,
      [FromQuery] int? idChiNhanh = null)
        {
            return await _service.ExportToPDF(idPhieuNhapkho, idChiNhanh);
        }

    }
}

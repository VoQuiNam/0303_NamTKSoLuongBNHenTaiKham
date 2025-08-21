using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;
using QuestPDF.Fluent;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("bao_cao_thong_ke_so_luong_benh_nhan_hen_tai_kham")]
    public class C0303TKSoLuongBNHenTaiKhamController : Controller
    {
        //private string _maChucNang = "/bao_cao_thong_ke_so_luong_benh_nhan_hen_tai_kham";
        //private IMemoryCachingServices _memoryCache;


        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;
        private readonly I0303TKSoLuongBNHenKham _service;

        public C0303TKSoLuongBNHenTaiKhamController(I0303TKSoLuongBNHenKham service /*, IMemoryCachingServices memoryCache*/)
        {
            _service = service;

            //_memoryCache = memoryCache;
        }

        public async Task<IActionResult> V0303TKSoLuongBNHenTaiKhamPage()
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

            return View("~/Views/V0303/V0303TKSoLuongBNHenTaiKham/V0303TKSoLuongBNHenTaiKhamPage.cshtml");
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

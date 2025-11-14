using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{

    [Route("xuat_hang_hoa")]
    public class C0303importDMHangHoaController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly Context0303 _localDb;
        private readonly I0303importDMHangHoa _service;
        private readonly IConfiguration _config;
        //private string _maChucNang = "/xuat_hang_hoa";
        //private IMemoryCachingServices _memoryCache;


        public C0303importDMHangHoaController(Context0303 localDb, IWebHostEnvironment env
            , I0303importDMHangHoa service, IConfiguration config /*, IMemoryCachingServices memoryCache*/)
        {
            _localDb = localDb;
            _env = env;
            _service = service;
            _config = config;

            //_memoryCache = memoryCache;
        }


        [HttpGet("downloadTemplate")]
        public IActionResult DownloadTemplate()
        {
            string filePath = Path.Combine(_env.WebRootPath,
                "dist", "excel", "FileDanhMucThuoc_BH duyệt.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FileDanhMucThuoc_BH duyệt.xlsx");
        }

        [HttpPost("importExcel")]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            try
            {
                if (excelFile == null || excelFile.Length == 0)
                    return BadRequest(new { success = false, message = "Vui lòng chọn file Excel" });

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var extension = Path.GetExtension(excelFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { success = false, message = "Chỉ chấp nhận file Excel (.xlsx, .xls)" });

                
                var errors = await _service.ReadExcelAndImport(excelFile, _config.GetConnectionString("DefaultConnection"));

                if (errors.Any())
                {
                   
                    return BadRequest(new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi import dữ liệu",
                        errors = errors
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Import thành công!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi import Excel: {ex}");
                return BadRequest(new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }



        public IActionResult V0303importDMHangHoaPage()
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
            return View("~/Views/V0303/V0303importDMHangHoa/V0303importDMHangHoaPage.cshtml");
        }
    }
}

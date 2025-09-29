using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Controllers.C0303
{
    [Route("xuat_dich_vu_ky_thuat")]
    public class C0303importDMDichVuKyThuatController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly Context0303 _localDb;
        private readonly I0303importDichVuKyThuat _service;
        private readonly IConfiguration _config;

        public C0303importDMDichVuKyThuatController(Context0303 localDb, IWebHostEnvironment env
            , I0303importDichVuKyThuat service /*, IMemoryCachingServices memoryCache*/, IConfiguration config)
        {
            _localDb = localDb;
            _env = env;
            _service = service;
            _config = config;

            //_memoryCache = memoryCache;
        }

        public IActionResult V0303importDMDichVuKyThuatPage()
        {
            return View("~/Views/V0303/V0303importDMDichVuKyThuat/V0303importDMDichVuKyThuatPage.cshtml");
        }


        [HttpGet("downloadTemplate")]
        public IActionResult DownloadTemplate()
        {
            string filePath = Path.Combine(_env.WebRootPath,
                "dist", "excel", "DM_DichVuKyThuat.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DM_DichVuKyThuat.xlsx");
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

                // Không lỗi → trả success
                return Ok(new
                {
                    success = true,
                    message = "Import dịch vụ kỹ thuật thành công!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }


    }
}

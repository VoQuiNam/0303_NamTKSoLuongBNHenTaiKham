using Microsoft.AspNetCore.Mvc;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303
{
    public interface I0303TKSoLuongBNHenKham
    {
        // Filter bệnh nhân hẹn khám theo ngày và chi nhánh
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh);

        // Xuất dữ liệu ra PDF
        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        // Xuất dữ liệu ra Excel
        Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn);
    }
}

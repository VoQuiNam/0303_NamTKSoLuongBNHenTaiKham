using Microsoft.AspNetCore.Mvc;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303
{
    public interface I0303TKSoLuongBNHenKham
    {
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn);
    }
}

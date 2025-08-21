using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303
{
    public interface I0303TKSoLuongBNHenKham
    {
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);
        Task<List<M0303TKSoLuongBNHenKhamSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn);
    }
}

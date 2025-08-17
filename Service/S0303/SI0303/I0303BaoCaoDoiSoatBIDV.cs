using Microsoft.AspNetCore.Mvc;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303BaoCaoDoiSoatBIDV
    {
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh);
        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idcn);
    }
}

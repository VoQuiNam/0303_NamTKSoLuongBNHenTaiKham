using Microsoft.AspNetCore.Mvc;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface IC0303BaoCaoBacSiDocKQ
    {
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh, int idKhoa, int idPhong);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idKhoa, int idPhong);

        Task<ActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int? idKhoa = 0, int? idPhong = 0);
    }
}

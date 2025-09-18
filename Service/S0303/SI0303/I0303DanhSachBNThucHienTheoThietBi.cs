using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303DanhSachBNThucHienTheoThietBi
    {
        //    Task<object> FilterDanhSachBNTheoThietBiAsync(string tuNgay,
        //string denNgay,
        //int idChiNhanh,
        //int idNhomDichVu,
        //int idDichVuKyThuat);

        Task<object> FilterDanhSachBNTheoThietBiAsync(string tuNgay,
   string denNgay,
   int idChiNhanh);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<IActionResult> ExportExcel(
        DateTime? tuNgay,
        DateTime? denNgay,
        int? idChiNhanh
    );

        Task<List<M0303DanhSachBNThucHienTheoThietBiSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        //Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat();
    }
}

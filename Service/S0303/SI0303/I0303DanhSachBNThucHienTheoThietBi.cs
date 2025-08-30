using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303DanhSachBNThucHienTheoThietBi
    {
        Task<object> FilterDanhSachBNTheoThietBiAsync(string tuNgay,
    string denNgay,
    int idChiNhanh,
    int idNhomDichVu,
    int idDichVuKyThuat);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idNhomDichVu, int idDichVuKyThuat);

        Task<IActionResult> ExportExcel(
        DateTime? tuNgay,
        DateTime? denNgay,
        int? idChiNhanh,
        int idNhomDichVu = 0,
        int idDichVuKyThuat = 0
    );

        Task<List<M0303DanhSachBNThucHienTheoThietBiSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idNhomDichVu = 0, int idDichVuKyThuat = 0);
    }
}

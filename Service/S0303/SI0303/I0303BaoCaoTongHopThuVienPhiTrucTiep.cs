using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303BaoCaoTongHopThuVienPhiTrucTiep
    {
        Task<object> FilterBaoCaoTongHopThuVienPhiTrucTiepAsync(string tuNgay, string denNgay, int idChiNhanh);

        Task<List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>> GetBNHenKhamAsync(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<List<M0303NhomDichVuKyThuat>> GetNhomDVKT();

        Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat();

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh);

        Task<IActionResult> ExportExcel(
      DateTime? tuNgay,
      DateTime? denNgay,
      int? idChiNhanh);
    }
}

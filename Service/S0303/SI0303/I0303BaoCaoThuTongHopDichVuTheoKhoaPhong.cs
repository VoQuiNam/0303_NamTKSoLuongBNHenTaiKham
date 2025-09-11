using Microsoft.AspNetCore.Mvc;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303BaoCaoThuTongHopDichVuTheoKhoaPhong
    {
        Task<object> FilterByDayAsync(string tuNgay, string denNgay, int idChiNhanh, int idDichVuKyThuat, int idPhong);

        Task<IActionResult> ExportToPDF(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idPhong, int idDichVuKyThuat , int idNhomDichVu);
        Task<List<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>> GetBNHenKhamAsync(
                DateTime? tuNgay,
                DateTime? denNgay,
                int? idChiNhanh,
                int idPhong = 0,
                int idDichVuKyThuat = 0,
                int idNhomDichVu = 0
            );

        Task<List<M0303NhomDichVuKyThuat>> GetNhomDVKT();
        Task<List<M0303Phong>> GetDSPhongBuong();

        Task<List<M0303DichVuKyThuat>> GetDSDichVuKyThuat();

        Task<IActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay, int? idChiNhanh, int idPhong, int idDichVuKyThuat, int idNhomDichVu);
    }
}

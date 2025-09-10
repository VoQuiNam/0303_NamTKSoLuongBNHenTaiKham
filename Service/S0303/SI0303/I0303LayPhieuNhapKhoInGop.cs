using Microsoft.AspNetCore.Mvc;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303LayPhieuNhapKhoInGop
    {
        //  Task<IActionResult> ExportToPDF(DateTime ngayGioNhap, int? idChiNhanh,
        //long? IDKhoHang, long? IDNhaCungCap, long? IDHangHoa, long? IDDonViTinhNhap);

        Task<IActionResult> ExportToPDF(long IdPhieuNhapKhom, int? idChiNhanh);
    }
}

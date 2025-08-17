using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303BaoCaoDoiSoatBIDVSTO
    {
        public string MaYTe { get; set; }

        public string MaDot { get; set; }

        public string HoTenBenhNhan { get; set; }

        public string SoDienThoai { get; set; }

        public decimal? SoTienTrenBL { get; set; }

        public string SoBL { get; set; }

        public decimal? SoTienTrenHD { get; set; }

        public string SoHD { get; set; }

        public decimal? TongSoTien { get; set; }

        public DateTime? NgayGioGiaoDich { get; set; }

        public string UserThanhToan { get; set; }

        public decimal? BVUB_SoTien { get; set; }

        public string BVUB_TrangThai { get; set; }

        public decimal? BIDV_SoTien { get; set; }

        public string BIDV_TrangThai { get; set; }
    }
}

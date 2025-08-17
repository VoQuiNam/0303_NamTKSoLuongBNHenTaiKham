using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    [Table("Nam_BaoCaoDoiSoatBIDV")]
    public class M0303BaoCaoDoiSoatBIDV
    {
        [StringLength(10)]
        public string MaYTe { get; set; }

        [StringLength(50)]
        public string MaDot { get; set; }

        public int IDCN { get; set; }

        [StringLength(100)]
        public string HoTenBenhNhan { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienTrenBL { get; set; }

        [StringLength(50)]
        public string SoBL { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienTrenHD { get; set; }

        [StringLength(50)]
        public string SoHD { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongSoTien { get; set; }

        public DateTime? NgayGioGiaoDich { get; set; }

        [StringLength(100)]
        public string UserThanhToan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BVUB_SoTien { get; set; }

        [StringLength(50)]
        public string BVUB_TrangThai { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? BIDV_SoTien { get; set; }

        [StringLength(50)]
        public string BIDV_TrangThai { get; set; }
    }
}

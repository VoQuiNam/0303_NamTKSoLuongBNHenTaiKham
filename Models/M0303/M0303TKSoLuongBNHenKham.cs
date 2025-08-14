using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    [Table("Nam_Mau3")]
    public class M0303TKSoLuongBNHenKham
    {
        [Key]
        [StringLength(50)]
        public string MaYTe { get; set; }

        public long? IDCN { get; set; }

        [StringLength(100)]
        public string HoVaTen { get; set; }

        public int? NamSinh { get; set; }

        [StringLength(10)]
        public string GioiTinh { get; set; }

        [StringLength(50)]
        public string QuocTich { get; set; }

        [StringLength(50)]
        public string CCCD_PASSPORT { get; set; }

        [StringLength(20)]
        public string SDT { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? NgayHenKham { get; set; }

        [StringLength(100)]
        public string BacSiHenKham { get; set; }

        [StringLength(100)]
        public string NhacHen { get; set; }

        public string GhiChu { get; set; }
    }
}

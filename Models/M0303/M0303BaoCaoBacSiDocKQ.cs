using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    [Table("T0303_BaoCaoBacSiDocKQ")]
    public class M0303BaoCaoBacSiDocKQ
    {
        [Key]
        public long ID { get; set; }

        [StringLength(100)]
        public string BacSiChiDinh { get; set; }

        public int? ThuPhi { get; set; }

        public int? BHYT { get; set; }

        public int? No { get; set; }

        public int? MienGiam { get; set; }

        public long? IDCN { get; set; }

        public DateTime? Ngay { get; set; }

        public long? IdKhoa { get; set; }

        public long? IdPhong { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303TKSoLuongBNHenKham
    {
        public string MaYTe { get; set; }

        public long IDCN { get; set; }

        public string HoVaTen { get; set; }

        public int? NamSinh { get; set; }

        public string GioiTinh { get; set; }

        public string QuocTich { get; set; }

        public string CCCD_PASSPORT { get; set; }

        public string SDT { get; set; }
        public DateTime? NgayHenKham { get; set; }

        public string BacSiHenKham { get; set; }

        public string NhacHen { get; set; }

        public string GhiChu { get; set; }
    }
}

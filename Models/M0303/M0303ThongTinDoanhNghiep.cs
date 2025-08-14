using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    [Table("ThongTinDoanhNghiep")]
    public class M0303ThongTinDoanhNghiep
    {
        [Key]
        public long ID { get; set; }
        public string MaCSKCB { get; set; }
        public string TenCSKCB { get; set; }
        public string TenCoQuanChuyenMon { get; set; }
        public string DiaChi { get; set; }
        public long? IDTinh { get; set; }
        public long? IDQuan { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string HangBenhVien { get; set; }
        public string TuyenCMKT { get; set; }
        public string LoaiBenhVien { get; set; }
        public string UserNameBHXH { get; set; }
        public string PasswordBHXH { get; set; }
        public bool? Active { get; set; }
        public int? MaHang { get; set; }
        public string SoTk { get; set; }
        public string ChuTK { get; set; }
        public string NganHang { get; set; }
        public string MaSoThue { get; set; }
        public string Hotline { get; set; }
        public string Website { get; set; }
        public string Fanpage { get; set; }
        public long? IDThuTruongDonVi { get; set; }
        public long? IDKeToanTruong { get; set; }
        public string MaLienThongTTCSKCB { get; set; }
        public string MatKhauLienThongTTCSKCB { get; set; }
        public string APICheckIn { get; set; }
        public string APIGiamDinh { get; set; }
        public string APINhanBenh { get; set; }
        public string APIToken { get; set; }
        public string APITokenDaoTao { get; set; }
        public string PasswordDaoTao { get; set; }
        public bool? CongBHXH { get; set; }
        public string CapBenhVien { get; set; }
        public string MaLTDuocQuocGia { get; set; }
        public string TaiKhoanLTDuocQuocGia { get; set; }
        public string MatKhauLTDuocQuocGia { get; set; }
        public string TenPhieuInVaoVien { get; set; }
        public string TenPhieuInHangHoa { get; set; }
        public string APICongVan { get; set; }
        public string MauIn { get; set; }
        public string TenDangNhap { get; set; }
        public long IDChiNhanh { get; set; }
    }
}

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303HH_DM_HangSanXuat
    {
        public int ID { get; set; } // Primary Key, nếu DB có identity
        public string? MaHSX;
        public string? TenHSX;
        public int? ThongTu;
        public bool? Active;
    }
}

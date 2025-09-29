namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303HH_DM_DuongDung
    {
        public int ID { get; set; } // Primary Key, nếu DB có identity
        public string? MaDuongDung;
        public string? TenDuongDung;
        public int? ThongTu;
        public bool? Active;
    }
}

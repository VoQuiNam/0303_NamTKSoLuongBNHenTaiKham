namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303DM_QuocGia
    {
        public int ID { get; set; } // Primary Key, nếu DB có identity
        public string? MaQuocGia;
        public string? TenQuocGia;
        public int? ThongTu;
        public bool? Active;
    }
}

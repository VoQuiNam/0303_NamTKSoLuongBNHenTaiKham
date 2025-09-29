namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303HH_DM_DonViTinh
    {
        public int ID { get; set; } // Primary Key, nếu DB có identity
        public string? MaDVT;
        public string? TenDVT;
        public bool? Active;
    }
}

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303Phong
    {
        public long? id { get; set; }
        public int idKhoa { get; set; }
        public string ten { get; set; }
        public string viettat { get; set; }

        public long? idChiNhanh { get; set; }

        public bool active { get; set; }
    }
}

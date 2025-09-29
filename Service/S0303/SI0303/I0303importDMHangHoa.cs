using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303importDMHangHoa
    {
        Task<List<string>> ReadExcelAndImport(IFormFile file, string connectionString);
    }
}

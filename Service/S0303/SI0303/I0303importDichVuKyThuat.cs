namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303
{
    public interface I0303importDichVuKyThuat
    {
        Task<List<string>> ReadExcelAndImport(IFormFile file, string connectionString);
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class M0303AppDbContext : DbContext
    {
        public M0303AppDbContext(DbContextOptions<M0303AppDbContext> options) : base(options) { }

        public DbSet<M0303TKSoLuongBNHenKham> M0303Thongtinbnhenkhams { get; set; } // bảng SinhVien

        public DbSet<M0303ThongTinDoanhNghiep> ThongTinDoanhNghieps { get; set; } // bảng SinhVien
    }
}

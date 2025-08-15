using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Context
{
    public class M0303AppDbContext : DbContext
    {
        public M0303AppDbContext(DbContextOptions<M0303AppDbContext> options) : base(options) { }

        public DbSet<M0303TKSoLuongBNHenKham> M0303Thongtinbnhenkhams { get; set; }

        public DbSet<M0303TKSoLuongBNHenKhamSTO> M0303TKSoLuongBNHenKhamSTOs { get; set; }

        public DbSet<M0303ThongTinDoanhNghiep> ThongTinDoanhNghieps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M0303TKSoLuongBNHenKham>().HasNoKey();
            modelBuilder.Entity<M0303TKSoLuongBNHenKhamSTO>().HasNoKey();
        }

        public bool TestConnection()
        {
            try
            {
                return Database.CanConnect();
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}

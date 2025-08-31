using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303
{
    public class Context0303 : DbContext
    {
        public Context0303(DbContextOptions<Context0303> options) : base(options) { }

     

        public DbSet<M0303TKSoLuongBNHenKhamSTO> M0303TKSoLuongBNHenKhamSTOs { get; set; }

        public DbSet<M0303ThongTinDoanhNghiep> ThongTinDoanhNghieps { get; set; }

     
        public DbSet<M0303BaoCaoDoiSoatBIDVSTO> M0303BaoCaoDoiSoatBIDVSTOs { get; set; }

      
        public DbSet<M0303BaoCaoBacSiDocKQSTO> M0303BaoCaoBacSiDocKQSTOs { get; set; }

        public DbSet<M0303DanhSachBNThucHienTheoThietBiSTO> M0303DanhSachBNThucHienTheoThietBiSTOs { get; set; }

        public DbSet<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO> M0303BaoCaoTongHopThuVienPhiTrucTiepSTOs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<M0303TKSoLuongBNHenKhamSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoDoiSoatBIDVSTO>().HasNoKey(); 
            modelBuilder.Entity<M0303BaoCaoBacSiDocKQSTO>().HasNoKey();
            modelBuilder.Entity<M0303DanhSachBNThucHienTheoThietBiSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>().HasNoKey();
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

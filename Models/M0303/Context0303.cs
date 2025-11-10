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

        public DbSet<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO> M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTOs { get; set; }

        public DbSet<M0303LayPhieuNhapKhoInGopSTO> M0303LayPhieuNhapKhoInGopSTOs { get; set; }

        public DbSet<M0303importDMHangHoaSTO> M0303importDMHangHoaSTOs { get; set; }

        public DbSet<M0303DM_QuocGia> M0303DM_QuocGias { get; set; }
        public DbSet<M0303HH_DM_DonViTinh> M0303HH_DM_DonViTinhs { get; set; }
        public DbSet<M0303HH_DM_DuongDung> M0303HH_DM_DuongDungs { get; set; }
        public DbSet<M0303HH_DM_HangSanXuat> M0303HH_DM_HangSanXuats { get; set; }
        public DbSet<M0303HH_DM_NhaThau> M0303HH_DM_NhaThaus { get; set; }
        public DbSet<M0303HH_DM_HangHoa> M0303HH_DM_HangHoas { get; set; }
        public DbSet<M0303BaoCaoXetNghiem> M0303BaoCaoXetNghiems { get; set; }

        public DbSet<M0303_BaoCaoMienGiamNgoaiTruSTO> M0303_BaoCaoMienGiamNgoaiTruSTOs { get; set; }
        public DbSet<M0303DanhSachKhamBenhTheoBacSiSTO> M0303DanhSachKhamBenhTheoBacSiSTOs { get; set; }
        public DbSet<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO> M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTOs { get; set; }
        public DbSet<M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTO> M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTOs { get; set; }
        public DbSet<M0303DM_GiaiQuyet> M0303DM_GiaiQuyets { get; set; }

        // SỬA LẠI TÊN DBSET CHO ĐÚNG
        public DbSet<M0303PhieuNhapKhoSTO> M0303PhieuNhapKhoSTOs { get; set; }

        public DbSet<M0303NhomDichVuKyThuat> NhomDichVuKyThuat { get; set; }
        public DbSet<M0303Phong> PhongBuong { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<M0303TKSoLuongBNHenKhamSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoDoiSoatBIDVSTO>().HasNoKey(); 
            modelBuilder.Entity<M0303BaoCaoBacSiDocKQSTO>().HasNoKey();
            modelBuilder.Entity<M0303DanhSachBNThucHienTheoThietBiSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoThuTongHopDichVuTheoKhoaPhongSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>().HasNoKey();
            modelBuilder.Entity<M0303PhieuNhapKhoSTO>().HasNoKey();
            modelBuilder.Entity<M0303importDMHangHoaSTO>().HasNoKey();

            modelBuilder.Entity<M0303NhomDichVuKyThuat>().HasNoKey();
            modelBuilder.Entity<M0303Phong>().HasNoKey();
            modelBuilder.Entity<M0303DM_QuocGia>().HasNoKey();
            modelBuilder.Entity<M0303HH_DM_DonViTinh>().HasNoKey();
            modelBuilder.Entity<M0303HH_DM_DuongDung>().HasNoKey();
            modelBuilder.Entity<M0303HH_DM_HangSanXuat>().HasNoKey();
            modelBuilder.Entity<M0303HH_DM_NhaThau>().HasNoKey();
            modelBuilder.Entity<M0303HH_DM_HangHoa>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoXetNghiem>().HasNoKey();
            modelBuilder.Entity<M0303NhanVien>().HasNoKey();
            modelBuilder.Entity<M0303_BaoCaoMienGiamNgoaiTruSTO>().HasNoKey();
            modelBuilder.Entity<M0303DanhSachKhamBenhTheoBacSiSTO>().HasNoKey();
            modelBuilder.Entity<M0303BaoCaoThongKeBenhTatTheoBNKhamBenhSTO>().HasNoKey();
            modelBuilder.Entity<M0303DM_GiaiQuyet>().HasNoKey();
            modelBuilder.Entity<M0303DanhSachKhamBenhTheoHuongGiaiQuyetSTO>().HasNoKey();
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

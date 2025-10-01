using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;
using QuestPDF.Infrastructure;
using System;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<I0303TKSoLuongBNHenKham, S0303TKSoLuongBNHenKham>();
builder.Services.AddScoped<I0303BaoCaoDoiSoatBIDV, S0303BaoCaoDoiSoatBIDV>();
builder.Services.AddScoped<IC0303BaoCaoBacSiDocKQ, S0303BaoCaoBacSiDocKQ>();
builder.Services.AddScoped<I0303DanhSachBNThucHienTheoThietBi, S0303DanhSachBNThucHienTheoThietBi>();
builder.Services.AddScoped<I0303BaoCaoTongHopThuVienPhiTrucTiep, S0303BaoCaoTongHopThuVienPhiTrucTiep>();
builder.Services.AddScoped<I0303BaoCaoThuTongHopDichVuTheoKhoaPhong, S0303BaoCaoThuTongHopDichVuTheoKhoaPhong>();
builder.Services.AddScoped<I0303LayPhieuNhapKhoInGop, S0303LayPhieuNhapKhoInGop>();
builder.Services.AddScoped<I0303importDMHangHoa, S0303importDMHangHoa>();
builder.Services.AddScoped<I0303importDichVuKyThuat, S0303importDMDichVuKyThuat>();



builder.Services.AddDbContext<Context0303>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();


builder.Services.AddControllersWithViews();
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseWebSockets();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/bao_cao_thong_ke_so_luong_benh_nhan_hen_tai_kham");
});
 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=C0303TKSoLuongBNHenTaiKham}/{action=V0303TKSoLuongBNHenTaiKhamPage}/{id?}");

app.Run();

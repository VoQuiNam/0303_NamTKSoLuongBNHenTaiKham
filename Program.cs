using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Models;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;
using QuestPDF.Infrastructure;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<I0303TKSoLuongBNHenKham, S0303TKSoLuongBNHenKham>();
builder.Services.AddScoped<I0303BaoCaoDoiSoatBIDV, S0303BaoCaoDoiSoatBIDV>();
builder.Services.AddScoped<IC0303BaoCaoBacSiDocKQ, S0303BaoCaoBacSiDocKQ>();
builder.Services.AddScoped<I0303DanhSachBNThucHienTheoThietBi, S0303DanhSachBNThucHienTheoThietBi>();
builder.Services.AddScoped<I0303BaoCaoTongHopThuVienPhiTrucTiep, S0303BaoCaoTongHopThuVienPhiTrucTiep>();



builder.Services.AddDbContext<Context0303>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

builder.Services.AddHttpContextAccessor();


builder.Services.AddControllersWithViews();
QuestPDF.Settings.License = LicenseType.Community;


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

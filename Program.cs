using Microsoft.EntityFrameworkCore;
using Nam_ThongKeSoLuongBNHenTaiKham.Context;
using Nam_ThongKeSoLuongBNHenTaiKham.Models;
using Nam_ThongKeSoLuongBNHenTaiKham.Service;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.SI0303;
using QuestPDF.Infrastructure;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<I0303TKSoLuongBNHenKham, S0303TKSoLuongBNHenKham>();
builder.Services.AddScoped<I0303BaoCaoDoiSoatBIDV, S0303BaoCaoDoiSoatBIDV>();


builder.Services.AddDbContext<M0303AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddControllersWithViews();
QuestPDF.Settings.License = LicenseType.Community;


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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

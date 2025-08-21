using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Newtonsoft.Json;

namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoBacSiDocKQ:IDocument
    {

        private readonly List<M0303BaoCaoBacSiDocKQSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly long? _idKhoa;
        private readonly long? _idPhong;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;

        private List<M0303Khoa> _khoaList;
        private List<M0303Phong> _phongList;

        public P0303BaoCaoBacSiDocKQ(List<M0303BaoCaoBacSiDocKQSTO> data, DateTime? tuNgay, DateTime? denNgay, long IdPhong, long IdKhoa, string logoPath, dynamic thongTinDoanhNghiep)
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _idKhoa = IdKhoa;
            _idPhong = IdPhong;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;

  
            string khoaJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_Khoa.json"));
            _khoaList = JsonConvert.DeserializeObject<List<M0303Khoa>>(khoaJson);

            string phongJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_PhongBuong.json"));
            _phongList = JsonConvert.DeserializeObject<List<M0303Phong>>(phongJson);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            var khoaDict = _khoaList.ToDictionary(k => k.id, k => k.ten);
            var phongDict = _phongList.ToDictionary(p => p.id, p => p.ten);

            var reportData = _data.Select(x => new
            {
                x.BacSiChiDinh,
                x.ThuPhi,
                x.BHYT,
                x.No,
                x.MienGiam,
                x.IdKhoa,
                x.IdPhong,
                TenKhoa = x.IdKhoa.HasValue && khoaDict.ContainsKey((int)x.IdKhoa.Value) ? khoaDict[(int)x.IdKhoa.Value] : "",
                TenPhong = x.IdPhong.HasValue && phongDict.ContainsKey((int)x.IdPhong.Value) ? phongDict[(int)x.IdPhong.Value] : ""
            }).ToList();

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(10));


                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(60).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                                col.Item().Height(40).Image(_logoPath, ImageScaling.FitHeight);
                            else
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(9);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep.TenCSKCB).Bold().FontSize(12);
                            col.Item().Text(_thongTinDoanhNghiep.DiaChi).FontSize(9);
                            col.Item().Text("Điện thoại: " + _thongTinDoanhNghiep.DienThoai).FontSize(9);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("BÁO CÁO BÁC SĨ CHỈ ĐỊNH").Bold().FontSize(14);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}").FontSize(9);
                        });
                    });

                    headerCol.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                 
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);  
                            columns.RelativeColumn(3);   
                            columns.ConstantColumn(60);   
                            columns.ConstantColumn(60);   
                            columns.ConstantColumn(60);  
                            columns.ConstantColumn(60);   
                            columns.ConstantColumn(60);  
                        });

                       
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("STT").Bold();
                            header.Cell().Element(CellStyle).Text("Bác sĩ chỉ định").Bold();
                            header.Cell().Element(CellStyle).Text("Thu phí").Bold();
                            header.Cell().Element(CellStyle).Text("BHYT").Bold();
                            header.Cell().Element(CellStyle).Text("Nợ").Bold();
                            header.Cell().Element(CellStyle).Text("Miễn giảm").Bold();
                            header.Cell().Element(CellStyle).Text("Tổng số ca").Bold();

                            static IContainer CellStyle(IContainer c) =>
                                c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter();
                        });

                        int stt = 1;
                        var khoaGroups = reportData.GroupBy(x => x.IdKhoa).OrderBy(x => x.Key);
                        int sttKhoa = 1;

                        foreach (var khoa in khoaGroups)
                        {
                            int tongCaKhoa = khoa.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));

                            
                            table.Cell().ColumnSpan(2)
                                .Element(c => c.BorderBottom(1).BorderLeft(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                                               .Padding(3).AlignLeft().Text($"{sttKhoa:00}. {khoa.First().TenKhoa}").Bold());
                            table.Cell().Element(c => c.BorderBottom(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                            table.Cell().Element(c => c.BorderBottom(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                            table.Cell().Element(c => c.BorderBottom(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                            table.Cell().Element(c => c.BorderBottom(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                            table.Cell().Element(c => c.BorderBottom(1).BorderRight(1).BorderLeft(1).BorderTop(1)
                                               .BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongCaKhoa.ToString()).Bold());

                            var phongGroups = khoa.GroupBy(x => x.IdPhong).OrderBy(x => x.Key);
                            foreach (var phong in phongGroups)
                            {
                                int tongCaPhong = phong.Sum(x => (x.ThuPhi ?? 0) + (x.BHYT ?? 0) + (x.No ?? 0) + (x.MienGiam ?? 0));

                                
                                table.Cell().ColumnSpan(2)
                                    .Element(c => c.BorderBottom(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2)
                                                   .Padding(3).AlignLeft().Text($"{phong.First().TenPhong}").Bold());
                                table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                                table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                                table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                                table.Cell().Element(c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3));
                                table.Cell().Element(c => c.BorderBottom(1).BorderRight(1).BorderLeft(1)
                                                   .BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongCaPhong.ToString()).Bold());

                               
                                var bacSiGroups = phong.GroupBy(x => x.BacSiChiDinh);
                                foreach (var bacSiGroup in bacSiGroups)
                                {
                                    var bacSiData = bacSiGroup.First();
                                    int tongThuPhi = bacSiGroup.Sum(x => x.ThuPhi ?? 0);
                                    int tongBHYT = bacSiGroup.Sum(x => x.BHYT ?? 0);
                                    int tongNo = bacSiGroup.Sum(x => x.No ?? 0);
                                    int tongMienGiam = bacSiGroup.Sum(x => x.MienGiam ?? 0);
                                    int tongBacSi = tongThuPhi + tongBHYT + tongNo + tongMienGiam;

                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(stt.ToString()));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(bacSiData.BacSiChiDinh ?? ""));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongThuPhi.ToString()));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongBHYT.ToString()));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongNo.ToString()));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongMienGiam.ToString()));
                                    table.Cell().Element(c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().Text(tongBacSi.ToString()));

                                    stt++;
                                }
                            }

                            sttKhoa++;
                        }
                    });

                   
                    contentCol.Item().PaddingTop(20).AlignRight().Width(200).Column(nguoiLapCol =>
                    {
                        nguoiLapCol.Item().AlignCenter().Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}").Italic().FontSize(10);
                        nguoiLapCol.Item().PaddingTop(5).AlignCenter().Text("NGƯỜI LẬP BẢNG").Bold().FontSize(10);
                        nguoiLapCol.Item().PaddingTop(10).AlignCenter().Text("(Ký, họ tên)").Italic().FontSize(9);
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(9);
                    x.CurrentPageNumber().FontSize(9);
                    x.Span(" / ").FontSize(9);
                    x.TotalPages().FontSize(9);
                });
            });
        }
    }
}

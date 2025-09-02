using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;



namespace Nam_ThongKeSoLuongBNHenTaiKham.PDFDocuments
{
    public class P0303BaoCaoTongHopThuVienPhiTrucTiep : IDocument
    {
        private readonly List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO> _data;
        private readonly DateTime? _tuNgay;
        private readonly DateTime? _denNgay;
        private readonly long? _idNhomDichVu;
        private readonly string _logoPath;
        private readonly M0303ThongTinDoanhNghiep _thongTinDoanhNghiep;
        private const int TotalColumns = 29;

        private List<M0303NhomDichVuKyThuat> _nhomdichvukythuatList;

        public P0303BaoCaoTongHopThuVienPhiTrucTiep(List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO> data, DateTime? tuNgay, DateTime? denNgay, int idNhomDichVu, string logoPath, dynamic thongTinDoanhNghiep)
        {
            _data = data;
            _tuNgay = tuNgay;
            _denNgay = denNgay;
            _idNhomDichVu = idNhomDichVu;
            _logoPath = logoPath;
            _thongTinDoanhNghiep = thongTinDoanhNghiep;


            string nhomdichvukythuatJson = System.IO.File.ReadAllText(Path.Combine("wwwroot", "dist/data/json/DM_NhomDichVuKyThuat.json"));
            _nhomdichvukythuatList = JsonConvert.DeserializeObject<List<M0303NhomDichVuKyThuat>>(nhomdichvukythuatJson);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var tuNgayStr = _tuNgay?.ToString("dd-MM-yyyy") ?? "__";
            var denNgayStr = _denNgay?.ToString("dd-MM-yyyy") ?? "__";

            var reportData = (_data ?? new List<M0303BaoCaoTongHopThuVienPhiTrucTiepSTO>()).ToList();

           
            string[] fixedCols = {
        "STT", "Mã BN/Mã đợt", "Họ và tên", "Năm sinh", "Mã thẻ BHYT",
        "Đối tượng", "Ngày thu", "Quyển sổ", "Số biên lai", "Số chứng từ",
        "Miễn giảm", "Lý do miễn", "Nhập viện nhập miễn",
        "Ghi chú miễn", "Nợ", "Số tiền"
    };

            string[] fixedEndCols = { "Hủy", "Hoàn", "Ngày Hủy/Hoàn" };

          
            int totalCols = fixedCols.Length + _nhomdichvukythuatList.Count + 1 + 1 + fixedEndCols.Length;

            
            decimal totalMienGiam = reportData.Sum(x => x.MienGiam ?? 0);
            decimal totalNo = reportData.Sum(x => x.No ?? 0);
            decimal totalSoTien = reportData.Sum(x => x.SoTien ?? 0);

            
            Dictionary<int, decimal> tongTheoNhom = _nhomdichvukythuatList.ToDictionary(
                n => n.id,
                n => reportData.Where(x => x.IdNhomDichVu == n.id).Sum(x => x.SoTienChiTiet ?? 0)
            );

           
            decimal totalTongCongChiTiet = tongTheoNhom.Values.Sum();

            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(5f);
                page.DefaultTextStyle(x => x.FontFamily("Arial Narrow").FontSize(6f));

                
                page.Header().ShowOnce().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantColumn(40f).Column(col =>
                        {
                            if (File.Exists(_logoPath))
                                col.Item().Height(25f).Image(_logoPath, ImageScaling.FitHeight);
                            else
                                col.Item().Text("Không tìm thấy logo").Italic().FontSize(5f);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text(_thongTinDoanhNghiep?.TenCSKCB ?? "").Bold().FontSize(8f);
                            col.Item().Text(_thongTinDoanhNghiep?.DiaChi ?? "").FontSize(6f);
                            col.Item().Text("Điện thoại: " + (_thongTinDoanhNghiep?.DienThoai ?? "")).FontSize(6f);
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignRight().Text("BÁO CÁO TỔNG HỢP THU VIỆN PHÍ TRỰC TIẾP").Bold().FontSize(9f);
                            col.Item().AlignRight().Text($"Từ ngày: {tuNgayStr}   Đến ngày: {denNgayStr}").FontSize(6f);
                        });
                    });

                    headerCol.Item().PaddingVertical(2f).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                });

                
                page.Content().Column(contentCol =>
                {
                    contentCol.Item().Table(table =>
                    {
                        
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < totalCols; i++)
                                columns.RelativeColumn();
                        });

                        
                        table.Header(header =>
                        {
                            
                            foreach (var col in fixedCols)
                            {
                                header.Cell().RowSpan(2).Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text(col).Bold().FontSize(5f));
                            }

                            
                            header.Cell().ColumnSpan((uint)(_nhomdichvukythuatList.Count + 2)).Element(c =>
                                c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text("THÔNG TIN CHI TIẾT").Bold().FontSize(5f));

                            
                            foreach (var col in fixedEndCols)
                            {
                                header.Cell().RowSpan(2).Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text(col).Bold().FontSize(5f));
                            }

                            
                            header.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text("Thuốc").Bold().FontSize(5f));

                            foreach (var nhom in _nhomdichvukythuatList)
                            {
                                header.Cell().Element(c =>
                                    c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text(nhom.ten).WrapAnywhere().Bold().FontSize(4f));
                            }

                            header.Cell().Element(c =>
                                c.Background(Colors.Grey.Lighten3).Border(0.5f).Padding(1.5f).AlignCenter().Text("Tổng cộng").Bold().FontSize(5f));

                            
                            for (int i = 0; i < totalCols; i++)
                            {
                                string label = i == 0 ? "A" : i.ToString();
                                header.Cell().Element(c =>
                                    c.Background(Colors.Grey.Lighten4).Border(0.5f).Padding(1.5f).AlignCenter().Text(label).Bold().FontSize(4f));
                            }
                        });

                        int stt = 1;
                        foreach (var item in reportData)
                        {
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text((stt++).ToString()).FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.MaBN ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.HoTen ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text(item.NamSinh?.ToString() ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.MaTheBHYT ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.DoiTuong ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.NgayThu?.ToString("dd-MM-yyyy") ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.QuyenSo ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.SoBienLai ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(item.SoChungTu ?? "").FontSize(5f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(item.MienGiam)).FontSize(5f)); // Sửa ở đây
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(AbbreviateText(item.LyDoMien, 8)).FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(AbbreviateText(item.NhapVienNhapMien, 8)).FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text(AbbreviateText(item.GhiChuMien, 8)).FontSize(4f));
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(item.No)).FontSize(5f)); // Sửa ở đây
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(item.SoTien)).FontSize(5f)); // Sửa ở đây


                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text("-").FontSize(5f));


                            foreach (var nhom in _nhomdichvukythuatList)
                            {
                                if (item.IdNhomDichVu == nhom.id)
                                    table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(item.SoTienChiTiet)).FontSize(5f)); // Sửa ở đây
                                else
                                    table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text("-").FontSize(5f));
                            }


                            decimal tongCongChiTiet = item.SoTienChiTiet ?? 0;
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(tongCongChiTiet)).FontSize(5f)); // Sửa ở đây

                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text(FormatNumber(item.Huy, true)).FontSize(5f)); // Sửa ở đây
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text(FormatNumber(item.Hoan, true)).FontSize(5f)); // Sửa ở đây
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text(item.NgayHuyHoan?.ToString("dd-MM-yy") ?? "-").FontSize(5f));
                        }

               
                        table.Cell().ColumnSpan(10).Element(c => c.Border(0.5f).Padding(1.5f).AlignCenter().Text("TỔNG CỘNG").Bold().FontSize(5f));

                 
                        table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(totalMienGiam)).Bold().FontSize(5f)); // Sửa ở đây

             
                        for (int i = 0; i < 3; i++)
                        {
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text("").FontSize(5f));
                        }

                  
                        table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(totalNo)).Bold().FontSize(5f)); // Sửa ở đây

                      
                        table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(totalSoTien)).Bold().FontSize(5f)); // Sửa ở đây

                       
                        table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text("-").FontSize(5f));

                      
                        foreach (var nhom in _nhomdichvukythuatList)
                        {
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(tongTheoNhom[nhom.id])).Bold().FontSize(5f)); // Sửa ở đây
                        }

                        
                        table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).AlignRight().Text(FormatNumber(totalTongCongChiTiet)).Bold().FontSize(5f)); // Sửa ở đây

                        
                        for (int i = 0; i < 3; i++)
                        {
                            table.Cell().Element(c => c.Border(0.5f).Padding(1.5f).Text("").FontSize(5f));
                        }
                    });
                });

                
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Trang ").FontSize(6f);
                    x.CurrentPageNumber().FontSize(6f);
                    x.Span(" / ").FontSize(6f);
                    x.TotalPages().FontSize(6f);
                });
            });
        }

        private string AbbreviateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "-";
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength) + "...";
        }

        
        private string FormatNumber(decimal? value, bool isSimpleFormat = false)
        {
            if (value == null || value == 0) return "-";
            return isSimpleFormat ? value.Value.ToString("N0") : value.Value.ToString("N0");
        }

       
        private string FormatNumber(int? value, bool isSimpleFormat = false)
        {
            if (value == null || value == 0) return "-";
            return isSimpleFormat ? value.Value.ToString() : value.Value.ToString("N0");
        }

    }
}

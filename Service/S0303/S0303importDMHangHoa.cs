using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using QuestPDF.Infrastructure;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303importDMHangHoa : ControllerBase, I0303importDMHangHoa
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;


        public S0303importDMHangHoa(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }

        public async Task<List<string>> ReadExcelAndImport(IFormFile file, string connectionString)
        {
            var errors = new List<string>();

            if (file == null || file.Length == 0)
                throw new ArgumentException("Chưa chọn file Excel");

            var expectedHeaders = new List<string>
    {
        "STT", "MA_THUOC", "TEN_THUOC", "DON_VI_TINH", "DON_GIA", "BHYT"
    };

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    if (rows.Count() == 0)
                    {
                        errors.Add("File Excel không có dữ liệu. Vui lòng kiểm tra lại!");
                        return errors;
                    }

                    var headerRow = rows.First();
                    var actualHeaders = new List<string>();
                    for (int i = 1; i <= expectedHeaders.Count; i++)
                    {
                        var headerCell = headerRow.Cell(i).GetString()?.Trim();
                        actualHeaders.Add(headerCell);
                    }

                    bool isStructureValid = true;
                    for (int i = 0; i < expectedHeaders.Count; i++)
                    {
                        if (actualHeaders[i] != expectedHeaders[i])
                        {
                            isStructureValid = false;
                            break;
                        }
                    }

                    if (!isStructureValid)
                    {
                        errors.Add("SAI CẤU TRÚC BẢNG: File Excel không đúng template. Vui lòng tải template mẫu!");
                        return errors;
                    }

                    if (rows.Count() <= 1)
                    {
                        errors.Add("File Excel không có dữ liệu. Vui lòng kiểm tra lại!");
                        return errors;
                    }

                    var dt = new DataTable();
                    dt.Columns.Add("MA_THUOC", typeof(string));
                    dt.Columns.Add("TEN_THUOC", typeof(string));
                    dt.Columns.Add("DON_VI_TINH", typeof(string));
                    dt.Columns.Add("DON_GIA", typeof(double));
                    dt.Columns.Add("BHYT", typeof(bool));

                    int rowIndex = 1;

                    foreach (var row in rows.Skip(1))
                    {
                        rowIndex++;

                        string CleanData(string input)
                        {
                            if (string.IsNullOrEmpty(input))
                                return input;
                            return Regex.Replace(input.Trim(), @"\s+", " ");
                        }

                        string maThuoc = CleanData(row.Cell(2).GetString());
                        string tenThuoc = CleanData(row.Cell(3).GetString());
                        string donViTinh = CleanData(row.Cell(4).GetString());

                        double donGia = 0;
                        if (row.Cell(5).TryGetValue<double>(out var dg) && dg >= 0)
                            donGia = dg;

                        var bhytVal = CleanData(row.Cell(6).GetString());
                        bool isBHYT = bhytVal == "1" ||
                                      bhytVal.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                      bhytVal.Equals("có", StringComparison.OrdinalIgnoreCase) ||
                                      bhytVal.Equals("yes", StringComparison.OrdinalIgnoreCase);

                        var rowErrors = new List<string>();
                        if (string.IsNullOrEmpty(maThuoc))
                            rowErrors.Add("Mã thuốc không được để trống");
                        if (string.IsNullOrEmpty(tenThuoc))
                            rowErrors.Add("Tên thuốc không được để trống");
                        if (string.IsNullOrEmpty(donViTinh))
                            rowErrors.Add("Đơn vị tính không được để trống");

                        if (rowErrors.Any())
                        {
                            errors.Add($"Dòng {rowIndex}: {string.Join(", ", rowErrors)}");
                            continue;
                        }

                        var dr = dt.NewRow();
                        dr["MA_THUOC"] = maThuoc;
                        dr["TEN_THUOC"] = tenThuoc;
                        dr["DON_VI_TINH"] = donViTinh;
                        dr["DON_GIA"] = donGia;
                        dr["BHYT"] = isBHYT;
                        dt.Rows.Add(dr);
                    }

                   
                    if (errors.Any())
                        return errors;

                    using (var conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        using (var cmd = new SqlCommand("S0303_ImportHangHoa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            var param = cmd.Parameters.AddWithValue("@Data", dt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.T0303_ImportHangHoaType";

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            return errors;
        }







    }
}

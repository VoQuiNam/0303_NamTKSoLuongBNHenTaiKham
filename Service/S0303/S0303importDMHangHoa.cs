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
        "MA_THUOC", "TEN_HOAT_CHAT", "TEN_THUOC", "DON_VI_TINH",
        "HAM_LUONG", "DUONG_DUNG", "MA_DUONG_DUNG",
        "DANG_BAO_CHE", "SO_DANG_KY", "DON_GIA_BH",
        "QUY_CACH", "NHA_SX", "NUOC_SX",
        "NHA_THAU", "TT_THAU", "PP_CHEBIEN", "BHYT"
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
                    dt.Columns.Add("TEN_HOAT_CHAT", typeof(string));
                    dt.Columns.Add("TEN_THUOC", typeof(string));
                    dt.Columns.Add("DON_VI_TINH", typeof(string));
                    dt.Columns.Add("HAM_LUONG", typeof(string));
                    dt.Columns.Add("DUONG_DUNG", typeof(string));
                    dt.Columns.Add("MA_DUONG_DUNG", typeof(string));
                    dt.Columns.Add("DANG_BAO_CHE", typeof(string));
                    dt.Columns.Add("SO_DANG_KY", typeof(string));
                    dt.Columns.Add("DON_GIA_BH", typeof(double));
                    dt.Columns.Add("QUY_CACH", typeof(string));
                    dt.Columns.Add("NHA_SX", typeof(string));
                    dt.Columns.Add("NUOC_SX", typeof(string));
                    dt.Columns.Add("NHA_THAU", typeof(string));
                    dt.Columns.Add("TT_THAU", typeof(string));
                    dt.Columns.Add("PP_CHEBIEN", typeof(string));
                    dt.Columns.Add("BHYT", typeof(bool));

                    int rowIndex = 1;

                    foreach (var row in rows.Skip(1))
                    {
                        rowIndex++;

                        var dr = dt.NewRow();

                        dr["MA_THUOC"] = Clean(row.Cell(1).GetString());
                        dr["TEN_HOAT_CHAT"] = Clean(row.Cell(2).GetString());
                        dr["TEN_THUOC"] = Clean(row.Cell(3).GetString());
                        dr["DON_VI_TINH"] = Clean(row.Cell(4).GetString());
                        dr["HAM_LUONG"] = Clean(row.Cell(5).GetString());
                        dr["DUONG_DUNG"] = Clean(row.Cell(6).GetString());
                        dr["MA_DUONG_DUNG"] = Clean(row.Cell(7).GetString());
                        dr["DANG_BAO_CHE"] = Clean(row.Cell(8).GetString());
                        dr["SO_DANG_KY"] = Clean(row.Cell(9).GetString());

                        // DON_GIA_BH
                        double donGia = 0;
                        row.Cell(10).TryGetValue<double>(out donGia);
                        dr["DON_GIA_BH"] = donGia;

                        dr["QUY_CACH"] = Clean(row.Cell(11).GetString());
                        dr["NHA_SX"] = Clean(row.Cell(12).GetString());
                        dr["NUOC_SX"] = Clean(row.Cell(13).GetString());
                        dr["NHA_THAU"] = Clean(row.Cell(14).GetString());
                        dr["TT_THAU"] = Clean(row.Cell(15).GetString());
                        dr["PP_CHEBIEN"] = Clean(row.Cell(16).GetString());

                        // BHYT
                        var bhytStr = Clean(row.Cell(17).GetString());
                        dr["BHYT"] = bhytStr == "1" ||
                                     bhytStr?.ToLower() == "true" ||
                                     bhytStr?.ToLower() == "có" ||
                                     bhytStr?.ToLower() == "yes";

                        // Validate
                        var rowErrors = new List<string>();
                        if (string.IsNullOrWhiteSpace(dr["MA_THUOC"].ToString())) rowErrors.Add("MA_THUOC trống");
                        if (string.IsNullOrWhiteSpace(dr["TEN_THUOC"].ToString())) rowErrors.Add("TEN_THUOC trống");
                        if (string.IsNullOrWhiteSpace(dr["DON_VI_TINH"].ToString())) rowErrors.Add("DON_VI_TINH trống");

                        if (rowErrors.Any())
                        {
                            errors.Add($"Dòng {rowIndex}: {string.Join(", ", rowErrors)}");
                            continue;
                        }

                        dt.Rows.Add(dr);
                    }

                    if (errors.Any())
                        return errors;

                    // ======= 4. IMPORT VÀO DB =======
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

        private string Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }






    }
}

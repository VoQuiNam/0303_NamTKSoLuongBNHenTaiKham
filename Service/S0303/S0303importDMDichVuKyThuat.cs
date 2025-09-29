using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Nam_ThongKeSoLuongBNHenTaiKham.Models.M0303;
using Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303.SI0303;
using System.Data;
using System.Text.RegularExpressions;

namespace Nam_ThongKeSoLuongBNHenTaiKham.Service.S0303
{
    public class S0303importDMDichVuKyThuat : ControllerBase, I0303importDichVuKyThuat
    {
        private readonly Context0303 _localDb;
        private readonly IWebHostEnvironment _env;


        public S0303importDMDichVuKyThuat(Context0303 localDb, IWebHostEnvironment env)
        {
            _localDb = localDb;
            _env = env;
        }

        public async Task<List<string>> ReadExcelAndImport(IFormFile file, string connectionString)
        {
            var errors = new List<string>();

            if (file == null || file.Length == 0)
                throw new ArgumentException("Chưa chọn file Excel");

            // Định nghĩa cấu trúc header chuẩn cho dịch vụ kỹ thuật
            var expectedHeaders = new List<string>
    {
        "STT", "TENNHOMDICHVU", "MA_DICH_VU", "TEN_DICH_VU", "TEN_DICH_VU_BHYT", "DON_VI_TINH",
        "BHYT", "GIA_TRONG_GIO", "GIA_BAN_DEM", "GIA_BHYT", "TI_LE_THANH_TOAN",
        "PHU_THU_TRONG_GIO", "PHU_THU_NGOAI_GIO", "PHU_THU_BAN_DEM", "SO_THU_TU"
    };

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    // DEBUG: In thông tin file
                    Console.WriteLine("=== DEBUG EXCEL IMPORT ===");
                    Console.WriteLine($"Tổng số dòng: {rows.Count()}");

                    // Kiểm tra nếu file không có dữ liệu
                    if (rows.Count() == 0)
                    {
                        errors.Add("File Excel không có dữ liệu. Vui lòng kiểm tra lại!");
                        return errors;
                    }

                    // Lấy header từ file Excel
                    var headerRow = rows.First();
                    var actualHeaders = new List<string>();

                    // Đọc tất cả header
                    for (int i = 1; i <= headerRow.CellsUsed().Count(); i++)
                    {
                        var headerCell = headerRow.Cell(i).GetString()?.Trim();
                        actualHeaders.Add(headerCell);
                        Console.WriteLine($"Header cột {i}: '{headerCell}'");
                    }

                    // Kiểm tra cấu trúc header (bỏ qua cột STT trong validation)
                    var requiredHeaders = expectedHeaders.Skip(1).ToList(); // Bỏ "STT"
                    var actualHeadersWithoutSTT = actualHeaders.Skip(1).ToList();

                    bool isStructureValid = true;
                    if (actualHeadersWithoutSTT.Count != requiredHeaders.Count)
                    {
                        isStructureValid = false;
                    }
                    else
                    {
                        for (int i = 0; i < requiredHeaders.Count; i++)
                        {
                            if (actualHeadersWithoutSTT[i] != requiredHeaders[i])
                            {
                                isStructureValid = false;
                                break;
                            }
                        }
                    }

                    if (!isStructureValid)
                    {
                        errors.Add($"SAI CẤU TRÚC BẢNG: File Excel không đúng định dạng template dịch vụ kỹ thuật.");
                      
                        return errors;
                    }

                    // Kiểm tra nếu chỉ có header (1 dòng) hoặc không có dữ liệu
                    if (rows.Count() <= 1)
                    {
                        errors.Add("File Excel không có dữ liệu. Vui lòng kiểm tra lại!");
                        return errors;
                    }

                    // Tạo DataTable theo structure của type T0303_ImportDichVuKyThuatType
                    var dt = new DataTable();
                    dt.Columns.Add("TENNHOMDICHVU", typeof(string));
                    dt.Columns.Add("MA_DICH_VU", typeof(string));
                    dt.Columns.Add("TEN_DICH_VU", typeof(string));
                    dt.Columns.Add("TEN_DICH_VU_BHYT", typeof(string));
                    dt.Columns.Add("DON_VI_TINH", typeof(string));
                    dt.Columns.Add("BHYT", typeof(bool));
                    dt.Columns.Add("GIA_TRONG_GIO", typeof(double));
                    dt.Columns.Add("GIA_BAN_DEM", typeof(double));
                    dt.Columns.Add("GIA_BHYT", typeof(double));
                    dt.Columns.Add("TI_LE_THANH_TOAN", typeof(double));
                    dt.Columns.Add("PHU_THU_TRONG_GIO", typeof(double));
                    dt.Columns.Add("PHU_THU_NGOAI_GIO", typeof(double));
                    dt.Columns.Add("PHU_THU_BAN_DEM", typeof(double));
                    dt.Columns.Add("SO_THU_TU", typeof(string));

                    int rowIndex = 1;
                    int dataRowCount = 0;

                    foreach (var row in rows.Skip(1))
                    {
                        rowIndex++;
                        dataRowCount++;

                        // Hàm clean data
                        string CleanData(string input)
                        {
                            if (string.IsNullOrEmpty(input))
                                return input;

                            return Regex.Replace(input.Trim(), @"\s+", " ");
                        }

                        // DEBUG: In thông tin dòng
                        Console.WriteLine($"--- Xử lý dòng {rowIndex} ---");

                        // Đọc và clean data (ĐÚNG INDEX)
                        string tenNhomDichVu = CleanData(row.Cell(2).GetString());   // Cột B
                        string maDichVu = CleanData(row.Cell(3).GetString());        // Cột C  
                        string tenDichVu = CleanData(row.Cell(4).GetString());       // Cột D
                        string tenDichVuBHYT = CleanData(row.Cell(5).GetString());   // Cột E
                        string donViTinh = CleanData(row.Cell(6).GetString());       // Cột F

                        // Xử lý BHYT (Cột G - index 7)
                        var bhytVal = CleanData(row.Cell(7).GetString());
                        bool isBHYT = false;

                        if (!string.IsNullOrEmpty(bhytVal))
                        {
                            isBHYT = bhytVal == "1" ||
                                     bhytVal.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                     bhytVal.Equals("có", StringComparison.OrdinalIgnoreCase) ||
                                     bhytVal.Equals("yes", StringComparison.OrdinalIgnoreCase);
                        }

                        // DEBUG: In giá trị đọc được
                        Console.WriteLine($"TENNHOMDICHVU: '{tenNhomDichVu}'");
                        Console.WriteLine($"MA_DICH_VU: '{maDichVu}'");
                        Console.WriteLine($"TEN_DICH_VU: '{tenDichVu}'");
                        Console.WriteLine($"BHYT: '{bhytVal}' -> {isBHYT}");

                        // ==== VALIDATION ====
                        var rowErrors = new List<string>();

                        // Các trường bắt buộc
                        if (string.IsNullOrEmpty(tenNhomDichVu))
                            rowErrors.Add("Tên nhóm dịch vụ không được để trống");
                        if (string.IsNullOrEmpty(maDichVu))
                            rowErrors.Add("Mã dịch vụ không được để trống");
                        if (string.IsNullOrEmpty(tenDichVu))
                            rowErrors.Add("Tên dịch vụ không được để trống");
                        if (string.IsNullOrEmpty(donViTinh))
                            rowErrors.Add("Đơn vị tính không được để trống");

                        if (isBHYT)
                        {
                            if (string.IsNullOrEmpty(tenDichVuBHYT))
                                rowErrors.Add("Tên dịch vụ BHYT không được để trống khi BHYT = 1");
                            // Nếu có thêm cột quan trọng khác cần bắt buộc khi BHYT = 1, thêm vào đây
                        }

                        if (rowErrors.Any())
                        {
                            errors.Add($"Dòng {rowIndex}: {string.Join(", ", rowErrors)}");
                            continue;
                        }

                        // ==== MAP DATA ====
                        var dr = dt.NewRow();

                        dr["TENNHOMDICHVU"] = tenNhomDichVu;
                        dr["MA_DICH_VU"] = maDichVu;
                        dr["TEN_DICH_VU"] = tenDichVu;
                        dr["TEN_DICH_VU_BHYT"] = string.IsNullOrEmpty(tenDichVuBHYT) ? DBNull.Value : tenDichVuBHYT;
                        dr["DON_VI_TINH"] = donViTinh;
                        dr["BHYT"] = isBHYT;

                        // Xử lý các giá trị số (ĐÚNG INDEX)
                        if (row.Cell(8).TryGetValue<double>(out var giaTrongGio) && giaTrongGio >= 0)    // Cột H
                            dr["GIA_TRONG_GIO"] = giaTrongGio;
                        else
                            dr["GIA_TRONG_GIO"] = 0;

                        if (row.Cell(9).TryGetValue<double>(out var giaBanDem) && giaBanDem >= 0)        // Cột I
                            dr["GIA_BAN_DEM"] = giaBanDem;
                        else
                            dr["GIA_BAN_DEM"] = 0;

                        if (row.Cell(10).TryGetValue<double>(out var giaBHYT) && giaBHYT >= 0)           // Cột J
                            dr["GIA_BHYT"] = giaBHYT;
                        else
                            dr["GIA_BHYT"] = 0;

                        if (row.Cell(11).TryGetValue<double>(out var tiLeThanhToan) && tiLeThanhToan >= 0) // Cột K
                            dr["TI_LE_THANH_TOAN"] = tiLeThanhToan;
                        else
                            dr["TI_LE_THANH_TOAN"] = 0;

                        if (row.Cell(12).TryGetValue<double>(out var phuThuTrongGio) && phuThuTrongGio >= 0) // Cột L
                            dr["PHU_THU_TRONG_GIO"] = phuThuTrongGio;
                        else
                            dr["PHU_THU_TRONG_GIO"] = 0;

                        if (row.Cell(13).TryGetValue<double>(out var phuThuNgoaiGio) && phuThuNgoaiGio >= 0) // Cột M
                            dr["PHU_THU_NGOAI_GIO"] = phuThuNgoaiGio;
                        else
                            dr["PHU_THU_NGOAI_GIO"] = 0;

                        if (row.Cell(14).TryGetValue<double>(out var phuThuBanDem) && phuThuBanDem >= 0)   // Cột N
                            dr["PHU_THU_BAN_DEM"] = phuThuBanDem;
                        else
                            dr["PHU_THU_BAN_DEM"] = 0;

                        // Cột SO_THU_TU (Cột O - 15)
                        dr["SO_THU_TU"] = CleanData(row.Cell(15).GetString()) ?? "0";

                        // DEBUG: In giá trị số
                        Console.WriteLine($"GIA_TRONG_GIO: {dr["GIA_TRONG_GIO"]}");
                        Console.WriteLine($"GIA_BAN_DEM: {dr["GIA_BAN_DEM"]}");
                        Console.WriteLine($"SO_THU_TU: {dr["SO_THU_TU"]}");

                        dt.Rows.Add(dr);
                        Console.WriteLine($"Đã thêm dòng {rowIndex} vào DataTable");
                    }

                    // Kiểm tra nếu tất cả các dòng đều bị lỗi validation
                    if (dataRowCount == 0 && errors.Count > 0)
                    {
                        errors.Insert(0, "Tất cả các dòng dữ liệu đều có lỗi. Không có dữ liệu nào được import!");
                        return errors;
                    }

                    // DEBUG: In kết quả DataTable
                    Console.WriteLine($"Tổng số dòng trong DataTable: {dt.Rows.Count}");
                    Console.WriteLine($"Tổng số lỗi: {errors.Count}");

                    // Nếu có lỗi validation, trả về luôn
                    if (errors.Any())
                    {
                        Console.WriteLine("Có lỗi validation, không gọi stored procedure");
                        return errors;
                    }

                    // Gọi Store Procedure import dịch vụ kỹ thuật
                    try
                    {
                        Console.WriteLine("Bắt đầu gọi stored procedure...");
                        using (var conn = new SqlConnection(connectionString))
                        {
                            await conn.OpenAsync();
                            Console.WriteLine("Kết nối database thành công");

                            using (var cmd = new SqlCommand("S0303_ImportDichVuKyThuat", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                var param = cmd.Parameters.AddWithValue("@Data", dt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.T0303_ImportDichVuKyThuatType";

                                await cmd.ExecuteNonQueryAsync();
                                Console.WriteLine("Stored procedure chạy thành công");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi gọi stored procedure: {ex.Message}");
                        errors.Add($"Lỗi database: {ex.Message}");
                        return errors;
                    }
                }
            }

         

            Console.WriteLine("=== KẾT THÚC IMPORT ===");
            return errors;
        }
    }
}

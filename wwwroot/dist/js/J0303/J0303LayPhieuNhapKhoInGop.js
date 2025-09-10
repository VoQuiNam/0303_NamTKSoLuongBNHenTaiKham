
document.addEventListener('DOMContentLoaded', function () {
    const btnExportPDF = document.getElementById('btnExportPDFGoiKham');
    if (btnExportPDF) {
        btnExportPDF.addEventListener('click', function () {
            // Lấy các giá trị từ form hoặc các input tương ứng
            //const ngayGioNhap = '2025-09-07'; // Có thể thay bằng giá trị động
            //const idKhoHang = 2; // Có thể thay bằng giá trị động
            const idChiNhanh = 2; // Có thể thêm các tham số khác nếu cần
            //const idNhaCungCap = 3;
            //const idHangHoa = 6;
            //const idDonViTinhNhap = 1;
            const IDPhieuNhapKho = 1;

            // Tạo URL với các tham số
            const url = `/lay_phieu_nhap_kho_in_gop/export-pdf?idChiNhanh=${idChiNhanh}&IDPhieuNhapKho=${IDPhieuNhapKho}`;
            console.log(url);
            //const url = `/lay_phieu_nhap_kho_in_gop/export-pdf?ngayGioNhap=${encodeURIComponent(ngayGioNhap)}&idChiNhanh=${idChiNhanh}&idKhoHang=${idKhoHang}&idNhaCungCap=${idNhaCungCap}&idKhoHang=${idKhoHang}`;
            window.open(url, '_blank');
        });
    }
});
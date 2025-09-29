document.addEventListener('DOMContentLoaded', function () {
    const btnExportPDF = document.getElementById('btnExportPDFGoiKham');
    if (btnExportPDF) {
        btnExportPDF.addEventListener('click', async function () {
            const idChiNhanh = 2;
            const IDPhieuNhapKho = 1;
            const url = `/lay_phieu_nhap_kho_in_gop/export-pdf?idChiNhanh=${idChiNhanh}&IDPhieuNhapKho=${IDPhieuNhapKho}`;

            try {
                const response = await fetch(url);

                if (!response.ok) {
                    throw new Error("Lỗi xuất PDF");
                }

                const blob = await response.blob();

                if (blob.type === "application/pdf") {
                    // Tạo link download ẩn
                    const fileURL = URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = fileURL;
                    a.download = "phieu_nhap_kho.pdf"; // tên file tải về
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(fileURL);

                    toastr.success("Xuất PDF thành công!");
                } else {
                    toastr.error("Xuất PDF thất bại!");
                }
            } catch (error) {
                console.error(error);
                toastr.error("Có lỗi khi xuất PDF!");
            }
        });
    }
});

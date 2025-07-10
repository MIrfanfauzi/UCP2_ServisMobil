using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace ServisMobilApp
{
    public partial class ReportViewerForm : Form
    {
        private DataTable previewData;

        public ReportViewerForm(DataTable data)
        {
            InitializeComponent();
            previewData = data;
        }

        private void ReportViewerForm_Load(object sender, EventArgs e)
        {
            SetupReportViewer();
            this.reportViewer1.RefreshReport();
        }

        private void SetupReportViewer()
        {
            // Gunakan koneksi dinamis dari class Koneksi
            Koneksi koneksi = new Koneksi();
            string connectionString = koneksi.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Koneksi ke database gagal. Pastikan server SQL berjalan dan konfigurasi connection string sudah benar.",
                                "Koneksi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Query SQL lengkap dengan total harga
            string query = @"
        SELECT 
            p.Nama AS NamaPelanggan,
            p.Telepon AS TeleponPelanggan,
            k.NomorPlat,
            k.Merek,
            k.Model,
            lsrv.NamaLayanan,
            m.Nama AS NamaMekanik,
            lsrv.Harga AS HargaLayanan,
            ps.TanggalPesan,
            ls.BiayaTambahan,
            ls.TanggalSelesai,
            ps.Status,
            (lsrv.Harga + ls.BiayaTambahan) AS TotalHarga
        FROM LaporanServis ls
        INNER JOIN PemesananServis ps ON ls.ID_Pemesanan = ps.ID_Pemesanan
        INNER JOIN Pelanggan p ON ps.ID_Pelanggan = p.ID_Pelanggan
        INNER JOIN Kendaraan k ON ps.ID_Kendaraan = k.ID_Kendaraan
        INNER JOIN LayananServis lsrv ON ps.ID_Layanan = lsrv.ID_Layanan
        LEFT JOIN Mekanik m ON ps.ID_Mekanik = m.ID_Mekanik
        ORDER BY ls.TanggalSelesai DESC, ls.ID_Laporan;
        ";

            // Buat DataTable untuk menampung data
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }

                // Buat ReportDataSource dengan nama dataset sesuai RDLC
                ReportDataSource rds = new ReportDataSource("DataSet1", dt);

                // Bersihkan data source lama dan tambahkan yang baru
                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(rds);

                // Set the path to the report (.rdlc file)
                string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LaporanReport.rdlc");
                reportViewer1.LocalReport.ReportPath = reportPath;
                // Refresh the ReportViewer to show the updated report
                reportViewer1.RefreshReport();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {
            // Kosongkan jika tidak diperlukan
        }
    }
}

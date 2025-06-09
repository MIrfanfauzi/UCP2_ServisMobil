using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServisMobilApp
{
    public partial class ReportViewerForm : Form
    {
        public ReportViewerForm()
        {
            InitializeComponent();
        }

        private void ReportViewerForm_Load(object sender, EventArgs e)
        {
            SetupReportViewer();
            this.reportViewer1.RefreshReport();
        }
        private void SetupReportViewer()
        {
            // Connection string ke database
            string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True;";

            // Query SQL lengkap dengan total harga (Harga layanan + Biaya tambahan)
            string query = @"
        SELECT 
            ls.ID_Laporan,
            ls.BiayaTambahan,
            ls.TanggalSelesai,

            ps.ID_Pemesanan,
            ps.TanggalPesan,
            ps.TanggalServis,
            ps.Status,

            p.ID_Pelanggan,
            p.Nama AS NamaPelanggan,
            p.Telepon AS TeleponPelanggan,

            k.ID_Kendaraan,
            k.NomorPlat,

            lsrv.ID_Layanan,
            lsrv.NamaLayanan,
            lsrv.Harga AS HargaLayanan,

            m.ID_Mekanik,
            m.Nama AS NamaMekanik,

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

            // Isi DataTable dengan data dari database
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

            // Set path ke file report RDLC
            reportViewer1.LocalReport.ReportPath = @"F:\UMY\PABD\ServisMobilApp\ServisMobilApp\LaporanReport.rdlc";

            // Refresh report viewer untuk menampilkan data terbaru
            reportViewer1.RefreshReport();
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ServisMobilApp
{
    public partial class UC_LaporanServis : UserControl
    {
        private readonly string connectionString;
        private MemoryCache cache = MemoryCache.Default;
        private string cacheKey = "LaporanServisData";

        public UC_LaporanServis()
        {
            InitializeComponent();
            LoadComboBoxData();
            // Panggil dengan 'true' agar pesan muncul sekali saat inisialisasi.
            LoadLaporan(true);
            Koneksi koneksi = new Koneksi();
            connectionString = koneksi.GetConnectionString(); // Mengambil koneksi dari Koneksi.cs
            dgvLaporan.CellClick += dgvLaporan_CellClick;
            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;
        }

        private void HandleDatabaseError(Exception ex, string operation)
        {
            if (ex is SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case -1:
                    case 2:
                    case 53:
                    case 4060:
                    case 18456:
                        MessageBox.Show("Koneksi ke database gagal. Pastikan server SQL aktif dan dapat diakses.", "Kesalahan Koneksi 🔌", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Kesalahan saat {operation}: {sqlEx.Message}", "Kesalahan Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
            else
            {
                MessageBox.Show($"Terjadi kesalahan umum saat {operation}: {ex.Message}", "Eror Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadComboBoxData()
        {
            string query = @"
                SELECT ps.ID_Pemesanan, CAST(ps.ID_Pemesanan AS VARCHAR) + ' - ' + p.Nama AS DisplayText
                FROM PemesananServis ps
                JOIN Pelanggan p ON ps.ID_Pelanggan = p.ID_Pelanggan
                LEFT JOIN LaporanServis ls ON ps.ID_Pemesanan = ls.ID_Pemesanan
                WHERE ps.Status = 'Selesai' AND ls.ID_Laporan IS NULL";
            LoadCombo(cmbPemesanan, query, "DisplayText", "ID_Pemesanan");
        }

        private void LoadCombo(ComboBox combo, string query, string display, string value)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    combo.DataSource = dt;
                    combo.DisplayMember = display;
                    combo.ValueMember = value;
                    combo.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                HandleDatabaseError(ex, $"memuat data {combo.Name}");
            }
        }

        private void LoadLaporan(bool showSuccessMessage = false)
        {
            DataTable dt;
            if (cache.Contains(cacheKey))
            {
                dt = cache.Get(cacheKey) as DataTable;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                dt = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
                    {
                        string query = "EXEC GetAllLaporanServis";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) };
                    cache.Set(cacheKey, dt, policy);

                    stopwatch.Stop();
                    if (showSuccessMessage)
                    {
                        MessageBox.Show($"Data Laporan berhasil dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                        "Pemuatan Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data laporan");
                    return;
                }
            }
            dgvLaporan.DataSource = dt;
            KosongkanForm();
        }

        private void ClearCache()
        {
            if (cache.Contains(cacheKey)) cache.Remove(cacheKey);
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            cmbPemesanan.SelectedIndex = -1;
            cmbPemesanan.Enabled = true;
            txtDeskripsi.Clear();
            txtBiayaTambahan.Clear();
            dtpTanggalSelesai.Value = DateTime.Now;
            cmbPemesanan.Focus();
        }

        private bool ValidasiInput(out decimal biayaTambahan)
        {
            biayaTambahan = 0;
            if (cmbPemesanan.SelectedValue == null && string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih pemesanan terlebih dahulu!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            {
                MessageBox.Show("Deskripsi wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!string.IsNullOrEmpty(txtBiayaTambahan.Text) && (!decimal.TryParse(txtBiayaTambahan.Text, out biayaTambahan) || biayaTambahan < 0))
            {
                MessageBox.Show("Masukkan nilai yang valid untuk Biaya Tambahan!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(out decimal biayaTambahan)) return;
            var confirm = MessageBox.Show("Yakin ingin menambahkan laporan servis ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC InsertLaporanServis @ID_Pemesanan, @Deskripsi, @BiayaTambahan, @TanggalSelesai";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", cmbPemesanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@BiayaTambahan", biayaTambahan);
                        cmd.Parameters.AddWithValue("@TanggalSelesai", dtpTanggalSelesai.Value.Date);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();

                    ClearCache();
                    LoadLaporan();
                    LoadComboBoxData();
                    MessageBox.Show("Laporan servis berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah laporan");
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih laporan yang ingin diubah!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidasiInput(out decimal biayaTambahan)) return;
            var confirm = MessageBox.Show("Yakin ingin mengubah data laporan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC UpdateLaporanServis @ID_Laporan, @Deskripsi, @BiayaTambahan, @TanggalSelesai";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Laporan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@BiayaTambahan", biayaTambahan);
                        cmd.Parameters.AddWithValue("@TanggalSelesai", dtpTanggalSelesai.Value.Date);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();

                    MessageBox.Show("Laporan berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearCache();
                    LoadLaporan();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "memperbarui laporan");
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih laporan yang ingin dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show("Yakin ingin menghapus laporan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "DELETE FROM LaporanServis WHERE ID_Laporan = @ID_Laporan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Laporan", int.Parse(lblID.Text));
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Laporan berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearCache();
                            LoadLaporan();
                            LoadComboBoxData();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data tidak ditemukan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menghapus laporan");
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearCache();
            LoadLaporan(true); // Panggil dengan true untuk memberi feedback saat refresh manual
            LoadComboBoxData();
        }

        private void dgvLaporan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvLaporan.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Laporan"].Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"].Value?.ToString() ?? "";
                txtBiayaTambahan.Text = row.Cells["BiayaTambahan"].Value?.ToString() ?? "";

                cmbPemesanan.Enabled = false;
                object idPemesanan = row.Cells["ID_Pemesanan"].Value;

                if (idPemesanan != null)
                {
                    if (cmbPemesanan.Items.Cast<DataRowView>().All(item => !item[cmbPemesanan.ValueMember].Equals(idPemesanan)))
                    {
                        if (cmbPemesanan.DataSource is DataTable dt)
                        {
                            dt.Rows.Add(idPemesanan, $"{idPemesanan} - (Laporan sudah ada)");
                        }
                    }
                    cmbPemesanan.SelectedValue = idPemesanan;
                }

                if (DateTime.TryParse(row.Cells["TanggalSelesai"].Value?.ToString(), out DateTime tanggal))
                    dtpTanggalSelesai.Value = tanggal;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ImportFromExcel(ofd.FileName);
                }
            }
        }

        private void ImportFromExcel(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    DataTable dt = new DataTable();

                    IRow header = sheet.GetRow(0);
                    foreach (var cell in header.Cells)
                        dt.Columns.Add(cell.ToString());

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < header.Cells.Count; j++)
                            newRow[j] = row.GetCell(j)?.ToString() ?? "";

                        dt.Rows.Add(newRow);
                    }

                    PreviewForm preview = new PreviewForm(dt, "LaporanServis");
                    preview.ShowDialog();
                    ClearCache();
                    LoadLaporan();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengimpor file Excel: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvLaporan.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu baris untuk diekspor!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dgvLaporan.SelectedRows[0];

            DataTable dt = new DataTable();
            foreach (DataGridViewColumn col in dgvLaporan.Columns)
            {
                dt.Columns.Add(col.HeaderText);
            }

            DataRow dr = dt.NewRow();
            foreach (DataGridViewColumn col in dgvLaporan.Columns)
            {
                dr[col.Index] = selectedRow.Cells[col.Index].Value?.ToString() ?? "";
            }
            dt.Rows.Add(dr);

            ReportViewerForm reportForm = new ReportViewerForm(dt);
            reportForm.Show();
        }

        private void AnalyzeQuery(string sqlQuery)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString)) // Menggunakan connection string dari Koneksi.cs
                {
                    conn.InfoMessage += (s, e) =>
                    {
                        string messages = string.Join(Environment.NewLine, e.Errors.Cast<SqlError>().Select(err => err.Message));
                        MessageBox.Show(messages, "STATISTICS INFO");
                    };
                    conn.Open();
                    var wrappedQuery = $@"SET STATISTICS IO ON; SET STATISTICS TIME ON; {sqlQuery}; SET STATISTICS IO OFF; SET STATISTICS TIME OFF;";
                    using (var cmd = new SqlCommand(wrappedQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDatabaseError(ex, "menganalisis query");
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM dbo.LaporanServis WHERE BiayaTambahan < 100000";
            AnalyzeQuery(query);
        }
    }
}

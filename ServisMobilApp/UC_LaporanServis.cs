using System;
using System.Data;
using System.Data.SqlClient;
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
        private string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";
        private MemoryCache cache = MemoryCache.Default;
        private string cacheKey = "LaporanServisData";
        public DateTime TanggalServis { get; set; }

        public UC_LaporanServis()
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadLaporan();
            dgvLaporan.CellClick += dgvLaporan_CellClick;
            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnExport.Click += btnExport_Click;
            btnAnalyze.Click += btnAnalyze_Click;
        }

        private void LoadComboBoxData()
        {
            // Hanya memuat pemesanan yang sudah 'Selesai' dan belum punya laporan
            string query = @"
                SELECT ps.ID_Pemesanan 
                FROM PemesananServis ps
                LEFT JOIN LaporanServis ls ON ps.ID_Pemesanan = ls.ID_Pemesanan
                WHERE ps.Status = 'Selesai' AND ls.ID_Laporan IS NULL";
            LoadCombo(cmbPemesanan, query, "ID_Pemesanan", "ID_Pemesanan");
        }

        private void LoadCombo(ComboBox combo, string query, string display, string value)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    combo.DataSource = dt;
                    combo.DisplayMember = display;
                    combo.ValueMember = value;
                    combo.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal memuat {combo.Name}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadLaporan()
        {
            if (cache.Contains(cacheKey))
            {
                dgvLaporan.DataSource = cache.Get(cacheKey) as DataTable;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        string query = "EXEC GetAllLaporanServis";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvLaporan.DataSource = dt;

                        CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) };
                        cache.Set(cacheKey, dt, policy);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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
            cmbPemesanan.Enabled = true; // Pastikan combo box aktif saat form kosong
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

            if (!decimal.TryParse(txtBiayaTambahan.Text, out biayaTambahan) || biayaTambahan < 0)
            {
                MessageBox.Show("Masukkan nilai yang valid untuk Biaya Tambahan!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validasi tanggal dipindahkan ke Stored Procedure
            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(out decimal biayaTambahan)) return;

            var confirm = MessageBox.Show("Yakin ingin menambahkan laporan servis ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Menggunakan Stored Procedure yang sudah ada validasinya
                    string query = "EXEC InsertLaporanServis @ID_Pemesanan, @Deskripsi, @BiayaTambahan, @TanggalSelesai";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", cmbPemesanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@BiayaTambahan", biayaTambahan);
                        cmd.Parameters.AddWithValue("@TanggalSelesai", dtpTanggalSelesai.Value);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Laporan servis berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearCache();
                        LoadLaporan();
                        LoadComboBoxData(); // Muat ulang combo box agar pemesanan yang sudah dibuat laporan hilang
                    }
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.Message, "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC UpdateLaporanServis @ID_Laporan, @Deskripsi, @BiayaTambahan, @TanggalSelesai";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Laporan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@BiayaTambahan", biayaTambahan);
                        cmd.Parameters.AddWithValue("@TanggalSelesai", dtpTanggalSelesai.Value);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Laporan berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearCache();
                        LoadLaporan();
                    }
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.Message, "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal memperbarui laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Asumsi ada SP DeleteLaporanServis atau menggunakan query langsung
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
                            LoadComboBoxData(); // Muat ulang combo box
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
                    transaction.Rollback();
                    MessageBox.Show("Gagal menghapus laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearCache();
            LoadLaporan();
            LoadComboBoxData();
            MessageBox.Show("Data laporan dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvLaporan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvLaporan.Rows[e.RowIndex];

                lblID.Text = row.Cells["ID_Laporan"].Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"].Value?.ToString() ?? "";
                txtBiayaTambahan.Text = row.Cells["BiayaTambahan"].Value?.ToString() ?? "";

                // Saat mengubah, ID Pemesanan tidak bisa diubah, jadi ComboBox di-disable
                cmbPemesanan.Enabled = false;
                object idPemesanan = row.Cells["ID_Pemesanan"].Value;

                // Tampilkan ID Pemesanan di ComboBox
                if (idPemesanan != null)
                {
                    // Cek jika item sudah ada di datasource
                    bool itemExists = false;
                    if (cmbPemesanan.DataSource is DataTable dtSource)
                    {
                        foreach (DataRow dr in dtSource.Rows)
                        {
                            if (Convert.ToInt32(dr["ID_Pemesanan"]) == Convert.ToInt32(idPemesanan))
                            {
                                itemExists = true;
                                break;
                            }
                        }
                    }

                    // Jika tidak ada, tambahkan sementara agar bisa ditampilkan
                    if (!itemExists)
                    {
                        DataTable dt = cmbPemesanan.DataSource as DataTable;
                        if (dt != null)
                        {
                            // Buat baris baru yang sesuai dengan skema DataTable
                            DataRow newRow = dt.NewRow();
                            newRow["ID_Pemesanan"] = idPemesanan;
                            dt.Rows.Add(newRow);
                        }
                    }
                    cmbPemesanan.SelectedValue = idPemesanan;
                }

                if (DateTime.TryParse(row.Cells["TanggalSelesai"].Value?.ToString(), out DateTime tanggal))
                    dtpTanggalSelesai.Value = tanggal;
                else
                    dtpTanggalSelesai.Value = DateTime.Now;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx;*.xlsm";
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

        private void AnalyzeQuery(string sqlQuery)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.InfoMessage += (s, e) =>
                {
                    string messages = string.Join(Environment.NewLine, e.Errors.Cast<SqlError>().Select(err => err.Message));
                    MessageBox.Show(messages, "STATISTICS INFO");
                };

                conn.Open();

                string wrappedQuery = $@"
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
{sqlQuery};
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;";

                using (var cmd = new SqlCommand(wrappedQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM dbo.LaporanServis WHERE BiayaTambahan < 100000";
            AnalyzeQuery(query);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvLaporan.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu baris untuk diekspor!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ambil baris yang dipilih
            DataGridViewRow selectedRow = dgvLaporan.SelectedRows[0];

            // Buat DataTable dengan satu baris
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

            // Kirim DataTable ke ReportViewerForm
            ReportViewerForm reportForm = new ReportViewerForm(dt);
            reportForm.Show();
        }
    }
}
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Caching;
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
            
        }

        private void LoadComboBoxData()
        {
            LoadCombo(cmbPemesanan, "SELECT ID_Pemesanan FROM PemesananServis WHERE Status = 'Selesai'", "ID_Pemesanan", "ID_Pemesanan");
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
                    MessageBox.Show($"Gagal memuat {combo.Name}: " + ex.Message);
                }
            }
        }

        private void LoadLaporan()
        {
            if (cache.Contains(cacheKey))
            {
                dgvLaporan.DataSource = cache.Get(cacheKey) as DataTable;
                KosongkanForm();
                return;
            }

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

                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat laporan: " + ex.Message);
                }
            }
        }

        private void ClearCache()
        {
            if (cache.Contains(cacheKey)) cache.Remove(cacheKey);
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            cmbPemesanan.SelectedIndex = -1;
            txtDeskripsi.Clear();
            txtBiayaTambahan.Clear();
            dtpTanggalSelesai.Value = DateTime.Now;
            cmbPemesanan.Focus();
        }

        private bool ValidasiInput(out decimal biayaTambahan)
        {
            biayaTambahan = 0;

            if (cmbPemesanan.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtDeskripsi.Text))
            {
                MessageBox.Show("Pilih pemesanan dan isi deskripsi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtBiayaTambahan.Text, out biayaTambahan) || biayaTambahan < 0)
            {
                MessageBox.Show("Masukkan nilai yang valid untuk Biaya Tambahan!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            DateTime tanggalServis = GetTanggalServisFromPemesanan();
            if (dtpTanggalSelesai.Value.Date < tanggalServis.Date)
            {
                MessageBox.Show("Tanggal selesai tidak boleh lebih awal dari tanggal servis.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtpTanggalSelesai.Value.Date > tanggalServis.AddMonths(1))
            {
                MessageBox.Show("Tanggal selesai tidak boleh lebih dari satu bulan setelah tanggal servis.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

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
                    string insertQuery = @"
                INSERT INTO LaporanServis (ID_Pemesanan, Deskripsi, BiayaTambahan, TanggalSelesai)
                VALUES (@ID_Pemesanan, @Deskripsi, @BiayaTambahan, @TanggalSelesai)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", cmbPemesanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@BiayaTambahan", biayaTambahan);
                        cmd.Parameters.AddWithValue("@TanggalSelesai", dtpTanggalSelesai.Value);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Laporan servis berhasil ditambahkan!");
                            ClearCache();
                            LoadLaporan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data tidak berhasil disimpan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message);
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

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Laporan berhasil diperbarui!");
                            ClearCache();
                            LoadLaporan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada perubahan data.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal memperbarui laporan: " + ex.Message);
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
                    string query = "EXEC DeleteLaporanServis @ID_Laporan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Laporan", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Laporan berhasil dihapus!");
                            ClearCache();
                            LoadLaporan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data tidak ditemukan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menghapus laporan: " + ex.Message);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearCache();
            LoadLaporan();
            MessageBox.Show("Data laporan dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            this.Hide();
        }



        private void dgvLaporan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvLaporan.Rows[e.RowIndex];

                lblID.Text = row.Cells["ID_Laporan"].Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"].Value?.ToString() ?? "";
                txtBiayaTambahan.Text = row.Cells["BiayaTambahan"].Value?.ToString() ?? "";
                

                // Pastikan ID_Pemesanan ada di ComboBox sebelum di-set
                object idPemesanan = row.Cells["ID_Pemesanan"].Value;
                if (idPemesanan != null)
                {
                    int id = Convert.ToInt32(idPemesanan);
                    foreach (DataRowView item in cmbPemesanan.Items)
                    {
                        if (Convert.ToInt32(item["ID_Pemesanan"]) == id)
                        {
                            cmbPemesanan.SelectedValue = id;
                            break;
                        }
                    }
                }

                if (DateTime.TryParse(row.Cells["TanggalSelesai"].Value?.ToString(), out DateTime tanggal))
                    dtpTanggalSelesai.Value = tanggal;
                else
                    dtpTanggalSelesai.Value = DateTime.Now;
            }
        }


        private DateTime GetTanggalServisFromPemesanan()
        {
            DateTime tanggalServis = DateTime.Now;
            if (cmbPemesanan.SelectedValue != null)
            {
                int idPemesanan = (int)cmbPemesanan.SelectedValue;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "SELECT TanggalServis FROM PemesananServis WHERE ID_Pemesanan = @ID_Pemesanan";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", idPemesanan);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            tanggalServis = Convert.ToDateTime(result);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal mengambil TanggalServis: " + ex.Message);
                    }
                }
            }
            return tanggalServis;
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
                // Tangkap pesan Informasi dari SQL Server (hasil STATISTICS)
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
        private void ExportSelectedRowToExcel()
        {
            if (dgvLaporan.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu baris untuk diekspor!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvLaporan.SelectedRows[0];

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel File|*.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                    {
                        IWorkbook workbook = new XSSFWorkbook();
                        ISheet sheet = workbook.CreateSheet("LaporanServis");

                        IRow headerRow = sheet.CreateRow(0);
                        for (int i = 0; i < dgvLaporan.Columns.Count; i++)
                            headerRow.CreateCell(i).SetCellValue(dgvLaporan.Columns[i].HeaderText);

                        IRow dataRow = sheet.CreateRow(1);
                        for (int i = 0; i < dgvLaporan.Columns.Count; i++)
                            dataRow.CreateCell(i).SetCellValue(row.Cells[i].Value?.ToString() ?? "");

                        workbook.Write(fs);
                    }

                    MessageBox.Show("Data berhasil diekspor!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}

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
    public partial class UC_PemesananServis : UserControl
    {
        private string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) };
        private const string CacheKey = "PemesananData";

        public UC_PemesananServis()
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadPemesanan();

            cmbPelanggan.SelectedIndexChanged += cmbPelanggan_SelectedIndexChanged;
            dgvPemesanan.CellClick += dgvPemesanan_CellClick;

            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnTambah.Click += btnTambah_Click;
            btnUbah.Click += btnUbah_Click;

            EnsureIndexes();
        }

        private void LoadComboBoxData()
        {
            LoadCombo(cmbPelanggan, "SELECT ID_Pelanggan, Nama FROM Pelanggan", "Nama", "ID_Pelanggan");
            LoadCombo(cmbLayanan, "SELECT ID_Layanan, NamaLayanan FROM LayananServis", "NamaLayanan", "ID_Layanan");
            LoadCombo(cmbMekanik, "SELECT ID_Mekanik, Nama FROM Mekanik", "Nama", "ID_Mekanik");
            cmbStatus.Items.AddRange(new[] { "Pending", "Selesai" });
        }

        private void LoadCombo(ComboBox combo, string query, string display, string value)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
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

        private void cmbPelanggan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPelanggan.SelectedValue != null)
            {
                string query = $"SELECT ID_Kendaraan, NomorPlat FROM Kendaraan WHERE ID_Pelanggan = {cmbPelanggan.SelectedValue}";
                LoadCombo(cmbKendaraan, query, "NomorPlat", "ID_Kendaraan");
            }
        }

        private void LoadPemesanan()
        {
            DataTable dt;

            if (_cache.Contains(CacheKey))
            {
                dt = _cache.Get(CacheKey) as DataTable;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "EXEC GetAllPemesananServis";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    dt = new DataTable();
                    da.Fill(dt);
                    _cache.Add(CacheKey, dt, _policy);
                }
            }

            dgvPemesanan.DataSource = dt;
        }

        private void dgvPemesanan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvPemesanan.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Pemesanan"].Value?.ToString();
                cmbPelanggan.Text = row.Cells["NamaPelanggan"].Value?.ToString();
                cmbKendaraan.Text = row.Cells["NomorPlat"].Value?.ToString();
                cmbLayanan.Text = row.Cells["NamaLayanan"].Value?.ToString();
                cmbMekanik.Text = row.Cells["NamaMekanik"].Value?.ToString();
                cmbStatus.Text = row.Cells["Status"].Value?.ToString();

                if (DateTime.TryParse(row.Cells["TanggalServis"].Value?.ToString(), out DateTime tgl))
                    dtpTanggalServis.Value = tgl;
            }
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiForm()) return;

            var confirm = MessageBox.Show("Tambah pemesanan baru?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC InsertPemesananServis @ID_Pelanggan, @ID_Kendaraan, @ID_Layanan, @ID_Mekanik, @TanggalServis";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbPelanggan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", cmbKendaraan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Layanan", cmbLayanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Data berhasil ditambahkan.");
                            _cache.Remove(CacheKey);
                            LoadPemesanan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data gagal ditambahkan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambah data: " + ex.Message);
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diubah!");
                return;
            }

            if (!ValidasiForm()) return;

            var confirm = MessageBox.Show("Ubah data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC UpdatePemesananServis @ID_Pemesanan, @ID_Mekanik, @TanggalServis, @Status";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value);
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Data berhasil diperbarui.");
                            _cache.Remove(CacheKey);
                            LoadPemesanan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada data yang diubah.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal mengubah data: " + ex.Message);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus!");
                return;
            }

            var confirm = MessageBox.Show("Hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC DeletePemesananServis @ID_Pemesanan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Data berhasil dihapus.");
                            _cache.Remove(CacheKey);
                            LoadPemesanan();
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
                    MessageBox.Show("Gagal menghapus data: " + ex.Message);
                }
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
                    _cache.Remove(CacheKey);
                    LoadPemesanan();
                }
            }
        }

        private void ImportFromExcel(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
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

                    PreviewForm preview = new PreviewForm(dt, "PemesananServis");
                    preview.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengimpor file Excel: " + ex.Message);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadPemesanan();
            MessageBox.Show("Data pemesanan dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            cmbPelanggan.SelectedIndex = -1;
            cmbKendaraan.SelectedIndex = -1;
            cmbLayanan.SelectedIndex = -1;
            cmbMekanik.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            dtpTanggalServis.Value = DateTime.Now;
        }

        private bool ValidasiForm()
        {
            if (cmbPelanggan.SelectedIndex == -1 || cmbKendaraan.SelectedIndex == -1 ||
                cmbLayanan.SelectedIndex == -1 || cmbMekanik.SelectedIndex == -1 ||
                string.IsNullOrWhiteSpace(cmbStatus.Text))
            {
                MessageBox.Show("Semua data wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.PemesananServis','U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_IDPelanggan')
                        CREATE NONCLUSTERED INDEX idx_Pemesanan_IDPelanggan ON dbo.PemesananServis(ID_Pelanggan);
                    
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_IDKendaraan')
                        CREATE NONCLUSTERED INDEX idx_Pemesanan_IDKendaraan ON dbo.PemesananServis(ID_Kendaraan);
                    
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_IDLayanan')
                        CREATE NONCLUSTERED INDEX idx_Pemesanan_IDLayanan ON dbo.PemesananServis(ID_Layanan);
                    
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pemesanan_IDMekanik')
                        CREATE NONCLUSTERED INDEX idx_Pemesanan_IDMekanik ON dbo.PemesananServis(ID_Mekanik);
                END";

                using (var cmd = new SqlCommand(indexScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }
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

                var wrappedQuery = $@"
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
            string query = "SELECT * FROM dbo.PemesananServis WHERE Status = 'Pending'";
            AnalyzeQuery(query);
        }

    }
}


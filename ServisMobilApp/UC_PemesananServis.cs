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
    public partial class UC_PemesananServis : UserControl
    {
        private readonly string connectionString;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) };
        private const string CacheKey = "PemesananData";
        private bool _isInitialLoad = true;

        public UC_PemesananServis()
        {
            Koneksi koneksi = new Koneksi();
            connectionString = koneksi.GetConnectionString();
            InitializeComponent();
            LoadComboBoxData();
            LoadPemesanan(true); // Panggil dengan true untuk pesan awal
            cmbPelanggan.SelectedIndexChanged += cmbPelanggan_SelectedIndexChanged;
            dgvPemesanan.CellClick += dgvPemesanan_CellClick;
            btnRefresh.Click += btnRefresh_Click;
            btnImport.Click += btnImport_Click;
            btnAnalyze.Click += btnAnalyze_Click;
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
            LoadCombo(cmbPelanggan, "SELECT ID_Pelanggan, CAST(ID_Pelanggan AS VARCHAR) + ' - ' + Nama AS DisplayText FROM Pelanggan", "DisplayText", "ID_Pelanggan");
            LoadCombo(cmbLayanan, "SELECT ID_Layanan, CAST(ID_Layanan AS VARCHAR) + ' - ' + NamaLayanan AS DisplayText FROM LayananServis", "DisplayText", "ID_Layanan");
            LoadCombo(cmbMekanik, "SELECT ID_Mekanik, CAST(ID_Mekanik AS VARCHAR) + ' - ' + Nama AS DisplayText FROM Mekanik", "DisplayText", "ID_Mekanik");

            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new[] { "Pending", "Selesai" });

            cmbJam.Items.Clear();
            for (int hour = 9; hour <= 17; hour++)
            {
                cmbJam.Items.Add($"{hour:D2}:00");
            }
        }

        private void LoadCombo(ComboBox combo, string query, string display, string value)
        {
            try
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
            catch (Exception ex)
            {
                HandleDatabaseError(ex, $"memuat data {combo.Name}");
            }
        }

        private void cmbPelanggan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPelanggan.SelectedValue != null && cmbPelanggan.SelectedValue is int)
            {
                string query = $@"
                    SELECT ID_Kendaraan, 
                           CAST(ID_Kendaraan AS VARCHAR) + ' - ' + NomorPlat AS DisplayText
                    FROM Kendaraan 
                    WHERE ID_Pelanggan = {cmbPelanggan.SelectedValue}";
                LoadCombo(cmbKendaraan, query, "DisplayText", "ID_Kendaraan");
            }
            else
            {
                cmbKendaraan.DataSource = null;
            }
        }

        private void LoadPemesanan(bool showSuccessMessage = false)
        {
            DataTable dt;

            if (_cache.Contains(CacheKey))
            {
                dt = _cache.Get(CacheKey) as DataTable;
            }
            else
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                dt = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "EXEC GetAllPemesananServis";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    _cache.Add(CacheKey, dt, _policy);

                    stopwatch.Stop();
                    if (showSuccessMessage && _isInitialLoad)
                    {
                        MessageBox.Show($"Data pemesanan berhasil dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                        "Informasi Pemuatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _isInitialLoad = false;
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data pemesanan");
                    return;
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

                cmbPelanggan.SelectedValue = row.Cells["ID_Pelanggan"].Value ?? -1;
                cmbKendaraan.SelectedValue = row.Cells["ID_Kendaraan"].Value ?? -1;
                cmbLayanan.SelectedValue = row.Cells["ID_Layanan"].Value ?? -1;
                cmbMekanik.SelectedValue = row.Cells["ID_Mekanik"].Value ?? DBNull.Value;
                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString();

                if (DateTime.TryParse(row.Cells["TanggalServis"].Value?.ToString(), out DateTime tgl))
                    dtpTanggalServis.Value = tgl;

                if (row.Cells["JamServis"].Value != null && row.Cells["JamServis"].Value != DBNull.Value)
                {
                    TimeSpan jamServis = (TimeSpan)row.Cells["JamServis"].Value;
                    cmbJam.SelectedItem = jamServis.ToString(@"hh\:mm");
                }
                else
                {
                    cmbJam.SelectedIndex = -1;
                }
            }
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiForm()) return;

            var confirm = MessageBox.Show("Tambah pemesanan baru?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC InsertPemesananServis @ID_Pelanggan, @ID_Kendaraan, @ID_Layanan, @ID_Mekanik, @TanggalServis, @JamServis";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbPelanggan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", cmbKendaraan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Layanan", cmbLayanan.SelectedValue);
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value.Date);
                        cmd.Parameters.AddWithValue("@JamServis", TimeSpan.Parse(cmbJam.SelectedItem.ToString()));

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();

                    MessageBox.Show("Data berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cache.Remove(CacheKey);
                    LoadPemesanan();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah pemesanan");
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diubah!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidasiForm()) return;
            var confirm = MessageBox.Show("Ubah data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC UpdatePesananServis @ID_Pemesanan, @ID_Mekanik, @TanggalServis, @JamServis, @Status";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value.Date);
                        cmd.Parameters.AddWithValue("@JamServis", TimeSpan.Parse(cmbJam.SelectedItem.ToString()));
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.Text);

                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();

                    MessageBox.Show("Data berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _cache.Remove(CacheKey);
                    LoadPemesanan();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "mengubah pemesanan");
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var confirm = MessageBox.Show("Hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "DELETE FROM PemesananServis WHERE ID_Pemesanan = @ID_Pemesanan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", int.Parse(lblID.Text));
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Data berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            _cache.Remove(CacheKey);
                            LoadPemesanan();
                            KosongkanForm();
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
                    HandleDatabaseError(ex, "menghapus pemesanan");
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                ofd.Title = "Pilih File Excel untuk Diimpor";
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
                    if (header == null || header.Cells.Count < 6)
                    {
                        MessageBox.Show("Format file Excel tidak sesuai. Harus ada 6 kolom: ID_Pelanggan, ID_Kendaraan, ID_Layanan, ID_Mekanik, TanggalServis, JamServis", "Format Tidak Valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    foreach (var cell in header.Cells)
                        dt.Columns.Add(cell.ToString());

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null || row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < header.Cells.Count; j++)
                            newRow[j] = row.GetCell(j)?.ToString() ?? "";

                        dt.Rows.Add(newRow);
                    }

                    PreviewForm preview = new PreviewForm(dt, "PemesananServis");
                    preview.ShowDialog();

                    // Reload setelah preview selesai
                    _cache.Remove(CacheKey);
                    LoadPemesanan();
                }
            }
            catch (Exception ex)
            {
                HandleDatabaseError(ex, "membaca file Excel");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadComboBoxData();
            LoadPemesanan(true);
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            cmbPelanggan.SelectedIndex = -1;
            cmbKendaraan.DataSource = null;
            cmbLayanan.SelectedIndex = -1;
            cmbMekanik.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            cmbJam.SelectedIndex = -1;
            dtpTanggalServis.Value = DateTime.Now;
        }

        private bool ValidasiForm()
        {
            if (cmbPelanggan.SelectedValue == null || cmbKendaraan.SelectedValue == null ||
                cmbLayanan.SelectedValue == null || cmbJam.SelectedIndex == -1)
            {
                MessageBox.Show("Pelanggan, Kendaraan, Layanan, dan Jam Servis wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(lblID.Text) && cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Status wajib diisi saat mengubah data!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void AnalyzeQuery(string sqlQuery)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
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
            string query = "SELECT * FROM dbo.PemesananServis WHERE Status = 'Pending'";
            AnalyzeQuery(query);
        }

        private void UC_PemesananServis_Load(object sender, EventArgs e)
        {
            // Dibiarkan kosong, load sudah di constructor
        }
    }
}

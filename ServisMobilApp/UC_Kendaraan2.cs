using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Runtime.Caching;
using System.Diagnostics;

namespace ServisMobilApp
{
    public partial class UC_Kendaraan2 : UserControl
    {
        private readonly string connectionString;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "KendaraanData";

        public UC_Kendaraan2()
        {
            // IMPLEMENTASI KONEKSI DINAMIS
            Koneksi koneksi = new Koneksi();
            connectionString = koneksi.GetConnectionString();
            InitializeComponent();
            dgvKendaraan.CellClick += dgvKendaraan_CellClick;
            btnRefresh.Click += btnRefresh_Click;
            btnImport.Click += btnImport_Click;
            LoadTahun();
            LoadPelanggan();

            

            LoadKendaraan(true);
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
                        MessageBox.Show(
                            "Koneksi ke database gagal. Pastikan server SQL aktif dan dapat diakses.",
                            "Kesalahan Koneksi 🔌", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void LoadTahun()
        {
            int tahunSekarang = DateTime.Now.Year;
            for (int tahun = tahunSekarang; tahun >= 1980; tahun--)
            {
                cmbTahun.Items.Add(tahun.ToString());
            }
            if (cmbTahun.Items.Count > 0) cmbTahun.SelectedIndex = 0;
        }

        private void LoadPelanggan()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT ID_Pelanggan, Nama FROM Pelanggan";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbPelanggan.DataSource = dt;
                    cmbPelanggan.DisplayMember = "Nama";
                    cmbPelanggan.ValueMember = "ID_Pelanggan";
                }
            }
            catch (Exception ex)
            {
                HandleDatabaseError(ex, "memuat data pelanggan");
            }
        }

        private void LoadKendaraan(bool showSuccessMessage = false)
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
                        conn.Open();
                        string query = "EXEC GetAllKendaraan";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    _cache.Add(CacheKey, dt, _policy);

                    stopwatch.Stop();
                    if (showSuccessMessage)
                    {
                        double seconds = stopwatch.Elapsed.TotalSeconds;
                        MessageBox.Show($"Data berhasil dimuat dari database dalam {seconds:F2} detik.",
                                        "Informasi Pemuatan",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data kendaraan");
                    return;
                }
            }
            dgvKendaraan.DataSource = dt;
        }

        private bool ValidasiInput()
        {
            string plat = txtNoPlat.Text.Trim().Replace(" ", "").ToUpper();
            string merek = txtMerek.Text.Trim();
            string model = txtModel.Text.Trim();

            if (string.IsNullOrWhiteSpace(plat) || string.IsNullOrWhiteSpace(merek) ||
                string.IsNullOrWhiteSpace(model) || cmbTahun.SelectedIndex == -1 || cmbPelanggan.SelectedIndex == -1)
            {
                MessageBox.Show("Semua kolom wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(plat, @"^[A-Z]{1,2}\d{1,4}[A-Z]{0,3}$"))
            {
                MessageBox.Show("Format plat nomor tidak valid! Contoh: B1234XYZ", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            txtNoPlat.Text = plat;
            txtMerek.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(merek.ToLower());
            txtModel.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.ToLower());

            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Yakin ingin menambahkan kendaraan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC InsertKendaraan @ID_Pelanggan, @Merek, @Model, @Tahun, @NomorPlat";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbPelanggan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Merek", txtMerek.Text);
                        cmd.Parameters.AddWithValue("@Model", txtModel.Text);
                        cmd.Parameters.AddWithValue("@Tahun", cmbTahun.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@NomorPlat", txtNoPlat.Text);
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Kendaraan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadKendaraan();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah kendaraan");
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data kendaraan yang ingin diubah.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Yakin ingin mengubah data kendaraan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC UpdateKendaraan @ID_Kendaraan, @ID_Pelanggan, @Merek, @Model, @Tahun, @NomorPlat";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", lblID.Text);
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbPelanggan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Merek", txtMerek.Text);
                        cmd.Parameters.AddWithValue("@Model", txtModel.Text);
                        cmd.Parameters.AddWithValue("@Tahun", cmbTahun.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@NomorPlat", txtNoPlat.Text);
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data kendaraan berhasil diubah!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadKendaraan();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "mengubah kendaraan");
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data kendaraan yang ingin dihapus.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus kendaraan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "EXEC DeleteKendaraan @ID_Kendaraan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", int.Parse(lblID.Text));
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Kendaraan berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _cache.Remove(CacheKey);
                    LoadKendaraan();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menghapus kendaraan");
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                ofd.Title = "Pilih File Excel";

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

                    PreviewForm preview = new PreviewForm(dt, "Kendaraan");
                    preview.ShowDialog();
                    _cache.Remove(CacheKey);
                    LoadKendaraan();
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
            LoadPelanggan();
            LoadKendaraan(true);
        }

        private void dgvKendaraan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvKendaraan.Rows[e.RowIndex].Cells["ID_Kendaraan"].Value != null)
            {
                DataGridViewRow row = dgvKendaraan.Rows[e.RowIndex];

                lblID.Text = row.Cells["ID_Kendaraan"].Value?.ToString() ?? "";
                txtNoPlat.Text = row.Cells["NomorPlat"].Value?.ToString() ?? "";
                txtMerek.Text = row.Cells["Merek"].Value?.ToString() ?? "";
                txtModel.Text = row.Cells["Model"].Value?.ToString() ?? "";

                string tahun = row.Cells["Tahun"].Value?.ToString() ?? "";
                if (cmbTahun.Items.Contains(tahun))
                    cmbTahun.SelectedItem = tahun;

                object idPelanggan = row.Cells["ID_Pelanggan"].Value;
                if (idPelanggan != null && idPelanggan != DBNull.Value)
                {
                    cmbPelanggan.SelectedValue = idPelanggan;
                }
            }
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            txtNoPlat.Clear();
            txtMerek.Clear();
            txtModel.Clear();
            if (cmbTahun.Items.Count > 0) cmbTahun.SelectedIndex = 0;
            if (cmbPelanggan.Items.Count > 0) cmbPelanggan.SelectedIndex = 0;
        }

        private void AnalyzeQuery(string sqlQuery)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.InfoMessage += (s, ev) =>
                    {
                        string messages = string.Join(Environment.NewLine, ev.Errors.Cast<SqlError>().Select(err => err.Message));
                        MessageBox.Show(messages, "STATISTICS INFO");
                    };

                    conn.Open();
                    string wrappedQuery = $"SET STATISTICS IO ON; SET STATISTICS TIME ON; {sqlQuery}; SET STATISTICS IO OFF; SET STATISTICS TIME OFF;";
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
            string heavyQuery = "SELECT NomorPlat, Merek, Model FROM dbo.Kendaraan WHERE Merek LIKE 'H%'";
            AnalyzeQuery(heavyQuery);
        }

        private void label3_Click(object sender, EventArgs e) { }
        private void UC_Kendaraan2_Load(object sender, EventArgs e) { }
    }
}

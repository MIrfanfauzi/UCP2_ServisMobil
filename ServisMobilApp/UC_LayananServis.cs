using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ServisMobilApp
{
    public partial class UC_LayananServis : UserControl
    {
        private readonly string connectionString;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "LayananServisData";

        public UC_LayananServis()
        {
            InitializeComponent();
            dgvLayanan.CellClick += dgvLayanan_CellClick;
            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;
            Koneksi koneksi = new Koneksi();
            connectionString = koneksi.GetConnectionString();
            LoadLayanan(true);
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
                            "Kesalahan Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void LoadLayanan(bool showSuccessMessage = false)
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
                        string query = "EXEC GetAllLayanan";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    _cache.Add(CacheKey, dt, _policy);

                    stopwatch.Stop();
                    if (showSuccessMessage)
                    {
                        MessageBox.Show($"Data layanan berhasil dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                        "Informasi Pemuatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data layanan");
                    return;
                }
            }
            dgvLayanan.DataSource = dt;
            KosongkanForm();
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            txtNamaLayanan.Clear();
            txtDeskripsi.Clear();
            txtHarga.Clear();
            txtNamaLayanan.Focus();
        }

        private bool ValidasiInput(out decimal harga)
        {
            harga = 0;
            if (string.IsNullOrWhiteSpace(txtNamaLayanan.Text) || string.IsNullOrWhiteSpace(txtHarga.Text))
            {
                MessageBox.Show("Nama layanan dan harga wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtNamaLayanan.Text.Trim(), @"^[A-Za-z0-9\s\-%]+$"))
            {
                MessageBox.Show("Nama layanan hanya boleh mengandung huruf, angka, spasi, '-' dan '%'.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtHarga.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out harga) || harga < 0)
            {
                MessageBox.Show("Masukkan harga yang valid (angka positif)!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(out decimal harga)) return;
            var confirm = MessageBox.Show("Tambah layanan baru?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "EXEC InsertLayanan @NamaLayanan, @Deskripsi, @Harga";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@NamaLayanan", txtNamaLayanan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Deskripsi", string.IsNullOrWhiteSpace(txtDeskripsi.Text) ? (object)DBNull.Value : txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Harga", harga);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    _cache.Remove(CacheKey);

                    MessageBox.Show("Data layanan berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadLayanan();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah layanan");
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih layanan yang ingin diubah.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidasiInput(out decimal harga)) return;
            var confirm = MessageBox.Show("Ubah data layanan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC UpdateLayanan @ID_Layanan, @NamaLayanan, @Deskripsi, @Harga";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Layanan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@NamaLayanan", txtNamaLayanan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Deskripsi", string.IsNullOrWhiteSpace(txtDeskripsi.Text) ? (object)DBNull.Value : txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Harga", harga);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data layanan berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadLayanan();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "mengubah layanan");
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih layanan yang ingin dihapus.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var confirm = MessageBox.Show("Yakin ingin menghapus data layanan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "DELETE FROM LayananServis WHERE ID_Layanan = @ID_Layanan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Layanan", int.Parse(lblID.Text));
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data layanan berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadLayanan();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data layanan tidak ditemukan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menghapus layanan");
                }
            }
        }

        private void dgvLayanan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvLayanan.Rows[e.RowIndex].Cells["ID_Layanan"].Value != null)
            {
                DataGridViewRow row = dgvLayanan.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Layanan"].Value?.ToString() ?? "";
                txtNamaLayanan.Text = row.Cells["NamaLayanan"].Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"].Value?.ToString() ?? "";
                txtHarga.Text = string.Format(CultureInfo.InvariantCulture, "{0}", row.Cells["Harga"].Value);
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

                    PreviewForm preview = new PreviewForm(dt, "LayananServis");
                    preview.ShowDialog();
                    _cache.Remove(CacheKey);
                    LoadLayanan();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengimpor file Excel: " + ex.Message);
            }
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
                    var wrappedQuery = $"SET STATISTICS IO ON; SET STATISTICS TIME ON; {sqlQuery}; SET STATISTICS IO OFF; SET STATISTICS TIME OFF;";
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
            string query = "SELECT * FROM LayananServis WHERE NamaLayanan LIKE 'Servis%';";
            AnalyzeQuery(query);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadLayanan(true);
        }

        private void txtNamaLayanan_TextChanged(object sender, EventArgs e) { }
    }
}

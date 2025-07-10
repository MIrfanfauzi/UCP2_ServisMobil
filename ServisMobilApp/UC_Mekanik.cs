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
    public partial class UC_Mekanik : UserControl
    {
        private readonly string connectionString;
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5) };
        private const string CacheKey = "MekanikData";

        public UC_Mekanik()
        {
            InitializeComponent();

            // Gunakan koneksi dari class Koneksi
            connectionString = new Koneksi().GetConnectionString();

            cmbSpesialisasi.Items.AddRange(new string[] { "Tune Up", "Servis Berkala", "Mesin" });
            cmbSpesialisasi.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvData.AutoGenerateColumns = true;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.MultiSelect = false;
            dgvData.CellClick += dgvData_CellClick;

            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;

            LoadMekanik(true);
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
                        MessageBox.Show("Koneksi ke database gagal. Pastikan server SQL aktif dan dapat diakses.",
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

        private void LoadMekanik(bool showSuccessMessage = false)
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
                        string query = "EXEC GetAllMekanik";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    _cache.Add(CacheKey, dt, _policy);

                    stopwatch.Stop();

                    if (showSuccessMessage)
                    {
                        MessageBox.Show($"Data mekanik berhasil dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                        "Informasi Pemuatan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data mekanik");
                    return;
                }
            }
            dgvData.DataSource = dt;
        }

        private bool ValidasiInput()
        {
            if (string.IsNullOrWhiteSpace(txtNama.Text) ||
                string.IsNullOrWhiteSpace(txtNoTelp.Text) ||
                cmbSpesialisasi.SelectedIndex == -1)
            {
                MessageBox.Show("Semua field wajib diisi!", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Nama hanya boleh berisi huruf dan spasi.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtNoTelp.Text, @"^08\d{10,11}$"))
            {
                MessageBox.Show("Nomor telepon harus diawali 08 dan panjangnya 12-13 digit.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Tambah mekanik baru?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "EXEC InsertMekanik @Nama, @Telepon, @Spesialisasi";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Spesialisasi", cmbSpesialisasi.SelectedItem.ToString());
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMekanik();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah mekanik");
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diubah.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Ubah data mekanik?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "EXEC UpdateMekanik @ID_Mekanik, @Nama, @Telepon, @Spesialisasi";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Mekanik", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Spesialisasi", cmbSpesialisasi.SelectedItem.ToString());
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMekanik();
                    KosongkanForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "mengubah mekanik");
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus data mekanik?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "DELETE FROM Mekanik WHERE ID_Mekanik = @ID_Mekanik";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Mekanik", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadMekanik();
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
                    HandleDatabaseError(ex, "menghapus mekanik");
                }
            }
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Rows[e.RowIndex].Cells["ID_Mekanik"].Value != null)
            {
                DataGridViewRow row = dgvData.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Mekanik"].Value?.ToString() ?? "";
                txtNama.Text = row.Cells["Nama"].Value?.ToString() ?? "";
                txtNoTelp.Text = row.Cells["Telepon"].Value?.ToString() ?? "";
                cmbSpesialisasi.SelectedItem = row.Cells["Spesialisasi"].Value?.ToString();
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

                    PreviewForm preview = new PreviewForm(dt, "Mekanik");
                    preview.ShowDialog();
                    _cache.Remove(CacheKey);
                    LoadMekanik();
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
            catch (Exception ex)
            {
                HandleDatabaseError(ex, "menganalisis query");
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            string heavyQuery = "SELECT Nama, Telepon, Spesialisasi FROM dbo.Mekanik WHERE Nama LIKE 'A%'";
            AnalyzeQuery(heavyQuery);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadMekanik(true);
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            txtNama.Clear();
            txtNoTelp.Clear();
            cmbSpesialisasi.SelectedIndex = -1;
            txtNama.Focus();
        }
    }
}

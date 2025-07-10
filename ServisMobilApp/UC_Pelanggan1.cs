using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ServisMobilApp
{
    public partial class UC_Pelanggan1 : UserControl
    {
        // GANTI: Ambil connection string dari class Koneksi
        private readonly string connectionString;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "PelangganData";
        private bool _isInitialLoad = true;

        public UC_Pelanggan1()
        {
            InitializeComponent();

            // Ambil connection string dari class Koneksi
            Koneksi koneksi = new Koneksi();
            connectionString = koneksi.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Koneksi ke database gagal. Cek konfigurasi IP lokal atau SQL Server Anda.",
                                "Koneksi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadEmailDomains();
            LoadPelanggan(true);

            dgvData.CellClick += dgvData_CellClick;
        }

        private void HandleDatabaseError(Exception ex, string operation = "operasi database")
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
                            "Koneksi ke database gagal. Pastikan server SQL berjalan dan konfigurasi koneksi sudah benar.",
                            "Kesalahan Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show($"Kesalahan saat {operation}: {sqlEx.Message}", "Kesalahan Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
            else
            {
                MessageBox.Show($"Terjadi kesalahan umum saat {operation}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadEmailDomains()
        {
            cmbEmailDomain.Items.AddRange(new string[]
            {
                "@gmail.com", "@yahoo.com", "@hotmail.com", "@outlook.com",
                "@icloud.com", "@protonmail.com", "@aol.com", "@mail.com"
            });
            cmbEmailDomain.SelectedIndex = 0;
        }

        private void LoadPelanggan(bool showSuccessMessage = false)
        {
            DataTable dt;

            if (_cache.Contains(CacheKey))
            {
                dt = _cache.Get(CacheKey) as DataTable;
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                dt = new DataTable();
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "EXEC GetAllPelanggan";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    _cache.Add(CacheKey, dt, _policy);

                    stopwatch.Stop();
                    if (showSuccessMessage && _isInitialLoad)
                    {
                        MessageBox.Show($"Data Pelanggan berhasil dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                        "Pemuatan Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _isInitialLoad = false;
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    HandleDatabaseError(ex, "memuat data pelanggan");
                    return;
                }
            }
            dgvData.DataSource = dt;
        }

        private bool ValidasiInput()
        {
            if (string.IsNullOrWhiteSpace(txtNama.Text) ||
                string.IsNullOrWhiteSpace(txtNoTelp.Text) ||
                string.IsNullOrWhiteSpace(txtAlamat.Text) ||
                string.IsNullOrWhiteSpace(txtEmailPrefix.Text))
            {
                MessageBox.Show("Semua data wajib diisi!", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Nama hanya boleh huruf dan spasi.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtNoTelp.Text, @"^08\d{10,11}$"))
            {
                MessageBox.Show("Nomor telepon harus dimulai dengan 08 dan terdiri dari 12-13 digit.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Tambah pelanggan baru?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC InsertPelanggan @Nama, @Telepon, @Alamat, @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", (txtEmailPrefix.Text.Trim() + cmbEmailDomain.SelectedItem).ToLower());

                        cmd.ExecuteNonQuery();
                        transaction.Commit();

                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPelanggan(); // Panggil versi silent
                        KosongkanForm();
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "menambah data");
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

            var confirm = MessageBox.Show("Ubah data pelanggan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "EXEC UpdatePelanggan @ID_Pelanggan, @Nama, @Telepon, @Alamat, @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", (txtEmailPrefix.Text.Trim() + cmbEmailDomain.SelectedItem).ToLower());

                        cmd.ExecuteNonQuery();
                        transaction.Commit();

                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPelanggan(); // Panggil versi silent
                        KosongkanForm();
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    HandleDatabaseError(ex, "mengubah data");
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

            var confirm = MessageBox.Show("Yakin ingin menghapus data pelanggan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "DELETE FROM Pelanggan WHERE ID_Pelanggan = @ID_Pelanggan";

                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", int.Parse(lblID.Text));
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadPelanggan(); // Panggil versi silent
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
                    HandleDatabaseError(ex, "menghapus pelanggan");
                }
            }
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Rows[e.RowIndex].Cells["ID_Pelanggan"].Value != null)
            {
                DataGridViewRow row = dgvData.Rows[e.RowIndex];

                lblID.Text = row.Cells["ID_Pelanggan"].Value.ToString();
                txtNama.Text = row.Cells["Nama"].Value.ToString();
                txtNoTelp.Text = row.Cells["Telepon"].Value.ToString();
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();
                string email = row.Cells["Email"].Value.ToString();
                int atIndex = email.IndexOf('@');

                if (atIndex > 0)
                {
                    txtEmailPrefix.Text = email.Substring(0, atIndex);
                    string domain = email.Substring(atIndex);
                    if (!cmbEmailDomain.Items.Contains(domain))
                        cmbEmailDomain.Items.Add(domain);
                    cmbEmailDomain.SelectedItem = domain;
                }
            }
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            txtNama.Clear();
            txtNoTelp.Clear();
            txtAlamat.Clear();
            txtEmailPrefix.Clear();
            cmbEmailDomain.SelectedIndex = 0;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadPelanggan(); // Panggil versi silent
            MessageBox.Show("Data pelanggan berhasil dimuat ulang.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                openFile.Title = "Pilih File Excel untuk Diimpor";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    ImportFromExcel(openFile.FileName);
                }
            }
        }

        private void ImportFromExcel(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            DataTable dt = new DataTable();

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow == null)
                    {
                        MessageBox.Show("File Excel kosong atau tidak memiliki baris header.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    dt.Columns.Add("Nama");
                    dt.Columns.Add("Telepon");
                    dt.Columns.Add("Alamat");
                    dt.Columns.Add("Email");

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null || row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                        string nama = row.GetCell(0)?.ToString() ?? "";
                        string telepon = row.GetCell(1)?.ToString() ?? "";
                        string alamat = row.GetCell(2)?.ToString() ?? "";
                        string email = row.GetCell(3)?.ToString() ?? "";

                        if (!string.IsNullOrEmpty(nama))
                        {
                            dt.Rows.Add(nama, telepon, alamat, email);
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data valid yang ditemukan di file Excel.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                PreviewForm previewForm = new PreviewForm(dt, "Pelanggan");
                if (previewForm.ShowDialog() == DialogResult.OK)
                {
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                        {
                            bulkCopy.DestinationTableName = "Pelanggan";
                            bulkCopy.ColumnMappings.Add("Nama", "Nama");
                            bulkCopy.ColumnMappings.Add("Telepon", "Telepon");
                            bulkCopy.ColumnMappings.Add("Alamat", "Alamat");
                            bulkCopy.ColumnMappings.Add("Email", "Email");

                            bulkCopy.WriteToServer(dt);
                        }
                    }

                    _cache.Remove(CacheKey);
                    LoadPelanggan();
                    stopwatch.Stop();
                    MessageBox.Show($"{dt.Rows.Count} data dari Excel berhasil ditambahkan dan dimuat dalam {stopwatch.Elapsed.TotalSeconds:F2} detik.",
                                    "Impor Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                HandleDatabaseError(ex, "mengimpor data dari Excel");
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
                        MessageBox.Show(e.Message, "STATISTICS INFO");
                    };

                    conn.Open();
                    var wrapped = $@"
                        SET STATISTICS IO ON;
                        SET STATISTICS TIME ON;
                        {sqlQuery};
                        SET STATISTICS IO OFF;
                        SET STATISTICS TIME OFF;";

                    using (var cmd = new SqlCommand(wrapped, conn))
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
            var heavyQuery = "SELECT Nama, Telepon, Email FROM dbo.Pelanggan WHERE Nama LIKE 'A%'";
            AnalyzeQuery(heavyQuery);
        }

        private void UC_Pelanggan1_Load(object sender, EventArgs e)
        {
            // Sudah di-handle di constructor
        }
    }
}

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ServisMobilApp
{
    public partial class UC_Pelanggan1 : UserControl
    {
        private readonly string connectionString =
            "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

        // Konfigurasi MemoryCache
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "PelangganData";

        public UC_Pelanggan1()
        {
            InitializeComponent();
            LoadEmailDomains();
            LoadPelanggan();
            EnsureIndexes();
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

        private void LoadPelanggan()
        {
            DataTable dt;
            if (_cache.Contains(CacheKey))
            {
                dt = _cache.Get(CacheKey) as DataTable;
            }
            else
            {
                dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "EXEC GetAllPelanggan";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat data: " + ex.Message);
                        return;
                    }
                }
                _cache.Add(CacheKey, dt, _policy);
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
                MessageBox.Show("Semua data wajib diisi!");
                return false;
            }

            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Nama hanya boleh huruf dan spasi.");
                return false;
            }

            if (!Regex.IsMatch(txtNoTelp.Text, @"^08\d{10,11}$"))
            {
                MessageBox.Show("Nomor telepon harus dimulai dengan 08 dan terdiri dari 12-13 digit.");
                return false;
            }

            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Tambah pelanggan baru?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC InsertPelanggan @Nama, @Telepon, @Alamat, @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", (txtEmailPrefix.Text + cmbEmailDomain.SelectedItem).ToLower());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey); // Hapus cache
                            MessageBox.Show("Data berhasil ditambahkan.");
                            LoadPelanggan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data tidak berhasil disimpan.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    transaction.Rollback();
                    MessageBox.Show("Telepon atau email sudah digunakan!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambah: " + ex.Message);
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin diubah.");
                return;
            }

            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Ubah data pelanggan?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC UpdatePelanggan @ID_Pelanggan, @Nama, @Telepon, @Alamat, @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", (txtEmailPrefix.Text + cmbEmailDomain.SelectedItem).ToLower());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey); // Hapus cache
                            MessageBox.Show("Data berhasil diperbarui.");
                            LoadPelanggan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada perubahan data.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    transaction.Rollback();
                    MessageBox.Show("Telepon atau email sudah digunakan!");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal mengubah: " + ex.Message);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus.");
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus data pelanggan?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC DeletePelanggan @ID_Pelanggan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey); // Hapus cache
                            MessageBox.Show("Data berhasil dihapus.");
                            LoadPelanggan();
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
                    MessageBox.Show("Gagal menghapus: " + ex.Message);
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
            _cache.Remove(CacheKey); // Hapus cache secara manual
            LoadPelanggan();
            MessageBox.Show("Data pelanggan berhasil dimuat ulang.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Filter = "Excel Files|*.xlsx;*.xlsm";
                openFile.Title = "Pilih File Excel untuk Diimpor";

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    PreviewData(openFile.FileName);
                }
            }
        }

        private void PreviewData(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    DataTable dt = new DataTable();

                    IRow headerRow = sheet.GetRow(0);
                    foreach (var cell in headerRow.Cells)
                    {
                        dt.Columns.Add(cell.ToString());
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;

                        DataRow newRow = dt.NewRow();
                        for (int j = 0; j < headerRow.Cells.Count; j++)
                        {
                            newRow[j] = row.GetCell(j)?.ToString() ?? "";
                        }
                        dt.Rows.Add(newRow);
                    }

                    PreviewForm previewForm = new PreviewForm(dt, "Pelanggan");
                    previewForm.ShowDialog();
                    _cache.Remove(CacheKey); // Hapus cache setelah impor
                    LoadPelanggan();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
IF OBJECT_ID('dbo.Pelanggan', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pelanggan_Nama')
        CREATE NONCLUSTERED INDEX idx_Pelanggan_Nama ON dbo.Pelanggan(Nama);
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

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;
using System.Runtime.Caching;

namespace ServisMobilApp
{
    public partial class UC_Mekanik : UserControl
    {
        private string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "MekanikData";

        public UC_Mekanik()
        {
            InitializeComponent();

            cmbSpesialisasi.Items.AddRange(new string[] { "Tune Up", "Servis Berkala", "Mesin" });
            cmbSpesialisasi.DropDownStyle = ComboBoxStyle.DropDownList;

            dgvData.AutoGenerateColumns = true;
            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.MultiSelect = false;
            dgvData.CellClick += dgvData_CellClick;

            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;

            LoadMekanik();
            EnsureIndexes();
        }

        private void LoadMekanik()
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
                        string query = "EXEC GetAllMekanik";
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
                cmbSpesialisasi.SelectedIndex == -1)
            {
                MessageBox.Show("Semua field wajib diisi!");
                return false;
            }

            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Nama hanya boleh berisi huruf dan spasi.");
                return false;
            }

            if (!Regex.IsMatch(txtNoTelp.Text, @"^\d+$"))
            {
                MessageBox.Show("Nomor telepon hanya boleh berisi angka.");
                return false;
            }

            if (!txtNoTelp.Text.StartsWith("08") || txtNoTelp.Text.Length < 12 || txtNoTelp.Text.Length > 13)
            {
                MessageBox.Show("Nomor telepon harus diawali 08 dan panjangnya 12-13 digit.");
                return false;
            }

            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Tambah mekanik baru?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC InsertMekanik @Nama, @Telepon, @Spesialisasi";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Spesialisasi", cmbSpesialisasi.SelectedItem.ToString());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data berhasil ditambahkan.");
                            LoadMekanik();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data tidak berhasil disimpan.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    transaction.Rollback();
                    MessageBox.Show("Telepon sudah terdaftar!", "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            var confirm = MessageBox.Show("Ubah data mekanik?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC UpdateMekanik @ID_Mekanik, @Nama, @Telepon, @Spesialisasi";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Mekanik", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@Telepon", txtNoTelp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Spesialisasi", cmbSpesialisasi.SelectedItem.ToString());

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data berhasil diperbarui.");
                            LoadMekanik();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada perubahan data.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    transaction.Rollback();
                    MessageBox.Show("Telepon sudah terdaftar!", "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            var confirm = MessageBox.Show("Yakin ingin menghapus data mekanik?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC DeleteMekanik @ID_Mekanik";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Mekanik", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data berhasil dihapus.");
                            LoadMekanik();
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadMekanik();
            MessageBox.Show("Data mekanik dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvData.Rows[e.RowIndex].Cells.Count >= 4)
            {
                DataGridViewRow row = dgvData.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Mekanik"].Value?.ToString() ?? "";
                txtNama.Text = row.Cells["Nama"].Value?.ToString() ?? "";
                txtNoTelp.Text = row.Cells["Telepon"].Value?.ToString() ?? "";
                cmbSpesialisasi.SelectedItem = row.Cells["Spesialisasi"].Value?.ToString();
            }
        }

        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
IF OBJECT_ID('dbo.Mekanik', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Mekanik_Nama')
        CREATE NONCLUSTERED INDEX idx_Mekanik_Nama ON dbo.Mekanik(Nama);
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
            string heavyQuery = "SELECT Nama, Telepon, Spesialisasi FROM dbo.Mekanik WHERE Nama LIKE 'A%'";
            AnalyzeQuery(heavyQuery);
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

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Runtime.Caching;

namespace ServisMobilApp
{
    public partial class UC_LayananServis : UserControl
    {
        private string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string CacheKey = "LayananServisData";

        public UC_LayananServis()
        {
            InitializeComponent();
            LoadLayanan();

            dgvLayanan.CellClick += dgvLayanan_CellClick;
            btnImport.Click += btnImport_Click;
            btnRefresh.Click += btnRefresh_Click;
        }

        private void LoadLayanan()
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
                        string query = "EXEC GetAllLayanan";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat data layanan: " + ex.Message);
                        return;
                    }
                }
                _cache.Add(CacheKey, dt, _policy);
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

            if (!Regex.IsMatch(txtNamaLayanan.Text.Trim(), @"^[A-Za-z\s]+$"))
            {
                MessageBox.Show("Nama layanan hanya boleh huruf dan spasi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtHarga.Text, out harga) || harga < 0)
            {
                MessageBox.Show("Masukkan harga yang valid!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidasiInput(out decimal harga)) return;

            var confirm = MessageBox.Show("Tambah layanan baru?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC InsertLayanan @NamaLayanan, @Deskripsi, @Harga";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@NamaLayanan", txtNamaLayanan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Harga", harga);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data layanan berhasil ditambahkan.");
                            LoadLayanan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data layanan gagal disimpan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambah layanan: " + ex.Message);
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih layanan yang ingin diubah.");
                return;
            }

            if (!ValidasiInput(out decimal harga)) return;

            var confirm = MessageBox.Show("Ubah data layanan?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC UpdateLayanan @ID_Layanan, @NamaLayanan, @Deskripsi, @Harga";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Layanan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@NamaLayanan", txtNamaLayanan.Text.Trim());
                        cmd.Parameters.AddWithValue("@Deskripsi", txtDeskripsi.Text.Trim());
                        cmd.Parameters.AddWithValue("@Harga", harga);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data layanan berhasil diperbarui.");
                            LoadLayanan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada perubahan data layanan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal mengubah layanan: " + ex.Message);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblID.Text))
            {
                MessageBox.Show("Pilih layanan yang ingin dihapus.");
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus data layanan?", "Konfirmasi", MessageBoxButtons.YesNo);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = "EXEC DeleteLayanan @ID_Layanan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Layanan", int.Parse(lblID.Text));

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data layanan berhasil dihapus.");
                            LoadLayanan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data layanan tidak ditemukan.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menghapus layanan: " + ex.Message);
                }
            }
        }

        private void dgvLayanan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvLayanan.Rows[e.RowIndex].Cells.Count >= 4)
            {
                DataGridViewRow row = dgvLayanan.Rows[e.RowIndex];
                lblID.Text = row.Cells["ID_Layanan"].Value?.ToString() ?? "";
                txtNamaLayanan.Text = row.Cells["NamaLayanan"].Value?.ToString() ?? "";
                txtDeskripsi.Text = row.Cells["Deskripsi"].Value?.ToString() ?? "";
                txtHarga.Text = row.Cells["Harga"].Value?.ToString() ?? "";
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
            string query = "SELECT * FROM dbo.LayananServis WHERE Harga > 100000";
            AnalyzeQuery(query);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadLayanan();
            MessageBox.Show("Data layanan dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtNamaLayanan_TextChanged(object sender, EventArgs e) { }
    }
}
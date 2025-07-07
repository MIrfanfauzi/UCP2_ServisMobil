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
                MessageBox.Show($"Gagal memuat data untuk {combo.Name}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbPelanggan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPelanggan.SelectedValue != null && cmbPelanggan.SelectedValue is int)
            {
                string query = $"SELECT ID_Kendaraan, NomorPlat FROM Kendaraan WHERE ID_Pelanggan = {cmbPelanggan.SelectedValue}";
                LoadCombo(cmbKendaraan, query, "NomorPlat", "ID_Kendaraan");
            }
            else
            {
                cmbKendaraan.DataSource = null;
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
                dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        string query = "EXEC GetAllPemesananServis";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        da.Fill(dt);
                        _cache.Add(CacheKey, dt, _policy);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal memuat data pemesanan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Data berhasil ditambahkan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _cache.Remove(CacheKey);
                        LoadPemesanan();
                        KosongkanForm();
                    }
                }
                // --- PERUBAHAN: Menangkap semua pesan dari Stored Procedure ---
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.Message, "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambah data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC UpdatePesananServis @ID_Pemesanan, @ID_Mekanik, @TanggalServis, @Status";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pemesanan", int.Parse(lblID.Text));
                        cmd.Parameters.AddWithValue("@ID_Mekanik", cmbMekanik.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TanggalServis", dtpTanggalServis.Value);
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.Text);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MessageBox.Show("Data berhasil diperbarui.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _cache.Remove(CacheKey);
                        LoadPemesanan();
                        KosongkanForm();
                    }
                }
                // --- PERUBAHAN: Menangkap semua pesan dari Stored Procedure ---
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show(ex.Message, "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal mengubah data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    // Asumsi ada SP DeletePemesananServis atau menggunakan query langsung
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
                    transaction.Rollback();
                    MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Asumsi ada form PreviewForm
                    // PreviewForm preview = new PreviewForm(dt, "PemesananServis");
                    // preview.ShowDialog();
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
            cmbKendaraan.DataSource = null;
            cmbLayanan.SelectedIndex = -1;
            cmbMekanik.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            dtpTanggalServis.Value = DateTime.Now;
        }

        private bool ValidasiForm()
        {
            if (cmbPelanggan.SelectedValue == null || cmbKendaraan.SelectedValue == null ||
                cmbLayanan.SelectedValue == null)
            {
                MessageBox.Show("Pelanggan, Kendaraan, dan Layanan wajib diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

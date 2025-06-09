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

namespace ServisMobilApp
{
    public partial class UC_Kendaraan2 : UserControl
    {
        private string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

        public UC_Kendaraan2()
        {
            InitializeComponent();
            dgvKendaraan.CellClick += dgvKendaraan_CellClick;
            btnRefresh.Click += btnRefresh_Click;
            btnImport.Click += btnImport_Click;
            LoadTahun();
            LoadPelanggan();
            LoadKendaraan();
            EnsureIndexes();
        }

        private void LoadTahun()
        {
            int tahunSekarang = DateTime.Now.Year;
            for (int tahun = tahunSekarang; tahun >= 1980; tahun--)
            {
                cmbTahun.Items.Add(tahun.ToString());
            }
            cmbTahun.SelectedIndex = 0;
        }

        private void LoadPelanggan()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID_Pelanggan, Nama FROM Pelanggan";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    cmbPelanggan.DataSource = dt;
                    cmbPelanggan.DisplayMember = "Nama";
                    cmbPelanggan.ValueMember = "ID_Pelanggan";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat pelanggan: " + ex.Message);
                }
            }
        }

        private void LoadKendaraan()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "EXEC GetAllKendaraan";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvKendaraan.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat kendaraan: " + ex.Message);
                }
            }
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

            if (!Regex.IsMatch(merek, @"^[A-Za-z\s]+$"))
            {
                MessageBox.Show("Merek hanya boleh huruf dan spasi.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(model, @"^[A-Za-z0-9\s]+$"))
            {
                MessageBox.Show("Model hanya boleh huruf, angka, dan spasi.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC InsertKendaraan @ID_Pelanggan, @Merek, @Model, @Tahun, @NomorPlat";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Pelanggan", cmbPelanggan.SelectedValue);
                        cmd.Parameters.AddWithValue("@Merek", txtMerek.Text);
                        cmd.Parameters.AddWithValue("@Model", txtModel.Text);
                        cmd.Parameters.AddWithValue("@Tahun", cmbTahun.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@NomorPlat", txtNoPlat.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Kendaraan berhasil ditambahkan!");
                            LoadKendaraan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Data kendaraan gagal ditambahkan.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // duplicate key
                {
                    transaction.Rollback();
                    MessageBox.Show("Plat nomor sudah terdaftar. Gunakan yang lain.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal menambah kendaraan: " + ex.Message);
                }
            }
        }

        private void btnUbah_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data kendaraan yang ingin diubah.");
                return;
            }

            if (!ValidasiInput()) return;

            var confirm = MessageBox.Show("Yakin ingin mengubah data kendaraan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC UpdateKendaraan @ID_Kendaraan, @Merek, @Model, @Tahun, @NomorPlat";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", lblID.Text);
                        cmd.Parameters.AddWithValue("@Merek", txtMerek.Text);
                        cmd.Parameters.AddWithValue("@Model", txtModel.Text);
                        cmd.Parameters.AddWithValue("@Tahun", cmbTahun.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@NomorPlat", txtNoPlat.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Data kendaraan berhasil diubah!");
                            LoadKendaraan();
                            KosongkanForm();
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Tidak ada data yang diubah.");
                        }
                    }
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    transaction.Rollback();
                    MessageBox.Show("Plat nomor sudah digunakan kendaraan lain.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Gagal mengubah kendaraan: " + ex.Message);
                }
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblID.Text))
            {
                MessageBox.Show("Pilih data kendaraan yang ingin dihapus.");
                return;
            }

            var confirm = MessageBox.Show("Yakin ingin menghapus kendaraan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "EXEC DeleteKendaraan @ID_Kendaraan";
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ID_Kendaraan", lblID.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            transaction.Commit();
                            MessageBox.Show("Kendaraan berhasil dihapus.");
                            LoadKendaraan();
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
                    MessageBox.Show("Gagal menghapus kendaraan: " + ex.Message);
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

                    PreviewForm preview = new PreviewForm(dt, "Kendaraan");
                    preview.ShowDialog();
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
            LoadKendaraan();
            MessageBox.Show("Data kendaraan dimuat ulang.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvKendaraan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                DataGridViewRow row = dgvKendaraan.Rows[e.RowIndex];

                lblID.Text = row.Cells["ID_Kendaraan"].Value?.ToString() ?? "";
                txtNoPlat.Text = row.Cells["NomorPlat"].Value?.ToString() ?? "";
                txtMerek.Text = row.Cells["Merek"].Value?.ToString() ?? "";
                txtModel.Text = row.Cells["Model"].Value?.ToString() ?? "";

                
                string tahun = row.Cells["Tahun"].Value?.ToString() ?? "";
                if (cmbTahun.Items.Contains(tahun))
                    cmbTahun.SelectedItem = tahun;

                string idPelanggan = row.Cells["ID_Pelanggan"].Value?.ToString() ?? "";
                cmbPelanggan.SelectedValue = idPelanggan;
            }
        }

        private void KosongkanForm()
        {
            lblID.Text = "";
            txtNoPlat.Clear();
            txtMerek.Clear();
            txtModel.Clear();
            cmbTahun.SelectedIndex = 0;
            cmbPelanggan.SelectedIndex = 0;
        }


        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
IF OBJECT_ID('dbo.Kendaraan', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Kendaraan_IDPelanggan')
        CREATE NONCLUSTERED INDEX idx_Kendaraan_IDPelanggan ON dbo.Kendaraan(ID_Pelanggan);
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
                // Tangkap pesan InfoMessage dari SQL Server (STATISTICS IO dan TIME)
                conn.InfoMessage += (s, e) =>
                {
                    string messages = string.Join(Environment.NewLine, e.Errors.Cast<SqlError>().Select(err => err.Message));
                    MessageBox.Show(messages, "STATISTICS INFO");
                };

                conn.Open();

                // Bungkus query dengan SET STATISTICS IO dan TIME ON/OFF
                string wrappedQuery = $@"
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
{sqlQuery};
SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;";

                using (var cmd = new SqlCommand(wrappedQuery, conn))
                {
                    // Eksekusi tanpa mengambil hasil data, hanya untuk memicu STATISTICS
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            // Contoh query yang ingin dianalisis, bisa kamu ganti sesuai kebutuhan
            string heavyQuery = "SELECT Nama, Telepon, Spesialisasi FROM dbo.Mekanik WHERE Nama LIKE 'A%'";

            AnalyzeQuery(heavyQuery);
        }

        private void cmbPelanggan_SelectedIndexChanged(object sender, EventArgs e) { }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}

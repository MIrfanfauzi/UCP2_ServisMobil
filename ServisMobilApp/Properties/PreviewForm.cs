using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ServisMobilApp
{
    public partial class PreviewForm : Form
    {
        static string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True;";
        private string targetTable;
        private DataTable importData;

        public PreviewForm(DataTable dt, string tableName)
        {
            InitializeComponent();
            dgvPreview.DataSource = dt;
            importData = dt;
            targetTable = tableName;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Apakah anda ingin mengimpor data ini ke tabel " + targetTable + "?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ImportDataToDatabase();
            }
        }

        private bool ValidateRow(DataRow row)
        {
            if (targetTable == "Pelanggan")
            {
                string telepon = row["Telepon"].ToString();
                string email = row["Email"].ToString();

                if (!telepon.StartsWith("08") || telepon.Length < 12 || telepon.Length > 13)
                {
                    MessageBox.Show("Nomor telepon harus dimulai dengan 08 dan memiliki 12–13 digit.", "Validasi Telepon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!email.Contains("@") || !email.EndsWith(".com"))
                {
                    MessageBox.Show("Format email tidak valid.", "Validasi Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            // Bisa ditambahkan validasi lain per tabel jika perlu
            return true;
        }

        private void ImportDataToDatabase()
        {
            try
            {
                foreach (DataRow row in importData.Rows)
                {
                    if (!ValidateRow(row))
                        continue;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;

                        switch (targetTable)
                        {
                            case "Pelanggan":
                                cmd.CommandText = "EXEC InsertPelanggan @Nama, @Telepon, @Alamat, @Email";
                                cmd.Parameters.AddWithValue("@Nama", row["Nama"]);
                                cmd.Parameters.AddWithValue("@Telepon", row["Telepon"]);
                                cmd.Parameters.AddWithValue("@Alamat", row["Alamat"]);
                                cmd.Parameters.AddWithValue("@Email", row["Email"]);
                                break;

                            case "Mekanik":
                                cmd.CommandText = "EXEC InsertMekanik @Nama, @Telepon, @Spesialisasi";
                                cmd.Parameters.AddWithValue("@Nama", row["Nama"]);
                                cmd.Parameters.AddWithValue("@Telepon", row["Telepon"]);
                                cmd.Parameters.AddWithValue("@Spesialisasi", row["Spesialisasi"]);
                                break;

                            case "Kendaraan":
                                cmd.CommandText = "EXEC InsertKendaraan @ID_Pelanggan, @Merek, @Model, @Tahun, @NomorPlat";
                                cmd.Parameters.AddWithValue("@ID_Pelanggan", row["ID_Pelanggan"]);
                                cmd.Parameters.AddWithValue("@Merek", row["Merek"]);
                                cmd.Parameters.AddWithValue("@Model", row["Model"]);
                                cmd.Parameters.AddWithValue("@Tahun", row["Tahun"]);
                                cmd.Parameters.AddWithValue("@NomorPlat", row["NomorPlat"]);
                                break;

                            case "LayananServis":
                                cmd.CommandText = "EXEC InsertLayanan @NamaLayanan, @Deskripsi, @Harga";
                                cmd.Parameters.AddWithValue("@NamaLayanan", row["NamaLayanan"]);
                                cmd.Parameters.AddWithValue("@Deskripsi", row["Deskripsi"]);
                                cmd.Parameters.AddWithValue("@Harga", row["Harga"]);
                                break;

                            case "PemesananServis":
                                cmd.CommandText = "EXEC InsertPemesananServis @ID_Pelanggan, @ID_Kendaraan, @ID_Layanan, @ID_Mekanik, @TanggalServis";
                                cmd.Parameters.AddWithValue("@ID_Pelanggan", row["ID_Pelanggan"]);
                                cmd.Parameters.AddWithValue("@ID_Kendaraan", row["ID_Kendaraan"]);
                                cmd.Parameters.AddWithValue("@ID_Layanan", row["ID_Layanan"]);
                                cmd.Parameters.AddWithValue("@ID_Mekanik", row["ID_Mekanik"]);
                                cmd.Parameters.AddWithValue("@TanggalServis", row["TanggalServis"]);
                                break;

                            case "LaporanServis":
                                cmd.CommandText = "EXEC InsertLaporanServis @ID_Pemesanan, @Deskripsi, @BiayaTambahan, @TanggalSelesai";
                                cmd.Parameters.AddWithValue("@ID_Pemesanan", row["ID_Pemesanan"]);
                                cmd.Parameters.AddWithValue("@Deskripsi", row["Deskripsi"]);
                                cmd.Parameters.AddWithValue("@BiayaTambahan", row["BiayaTambahan"]);
                                cmd.Parameters.AddWithValue("@TanggalSelesai", row["TanggalSelesai"]);
                                break;

                            default:
                                MessageBox.Show("Tabel tidak dikenali.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data berhasil diimpor ke tabel " + targetTable + ".", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Tutup form setelah berhasil impor
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan saat impor: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewForm_Load(object sender, EventArgs e)
        {
            dgvPreview.AutoResizeColumns();
        }

        private void dgvPreview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kosong (opsional)
        }
    }
}

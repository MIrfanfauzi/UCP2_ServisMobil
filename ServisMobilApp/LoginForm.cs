using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ServisMobilApp
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Silakan isi username dan password.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ganti dengan koneksi database kamu
            string connString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"SELECT COUNT(*) 
                                 FROM Admin 
                                 WHERE Username = @username AND Password = @password AND Role = 'admin'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); // Belum hashed

                try
                {
                    conn.Open();
                    int result = (int)cmd.ExecuteScalar();

                    if (result == 1)
                    {
                        MessageBox.Show("Login berhasil sebagai admin!", "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close(); // Tutup form login
                    }
                    else
                    {
                        MessageBox.Show("Username, password, atau role salah.", "Login Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal login: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}

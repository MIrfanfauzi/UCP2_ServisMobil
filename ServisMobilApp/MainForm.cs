using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServisMobilApp
{
    public partial class MainForm : Form
    {
        string connectionString = "Data Source=LAPTOP-N8SLA3LN\\IRFANFAUZI;Initial Catalog=ServisMobil;Integrated Security=True";

        public MainForm()
        {
            InitializeComponent();
            this.btnPelanggan.Click += new System.EventHandler(this.btnPelanggan_Click);
            this.btnMekanik.Click += new System.EventHandler(this.btnMekanik_Click);
            this.btnKendaraan.Click += new System.EventHandler(this.btnKendaraan_Click);
            this.btnLayananServis.Click += new System.EventHandler(this.btnLayananServis_Click);
            this.btnPemesananServis.Click += new System.EventHandler(this.btnPemesananServis_Click);
            this.btnLaporanServis.Click += new System.EventHandler(this.btnLaporanServis_Click);

        }
        private void LoadUserControl(UserControl uc)
        {
            panelContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelContent.Controls.Add(uc);
        }
        private void LoadForm(Form form)
        {
            panelContent.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelContent.Controls.Add(form);
            form.Show();
        }
        private void btnPelanggan_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Pelanggan1());
        }

        private void btnKendaraan_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Kendaraan2());
        }
        private void btnMekanik_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Mekanik());
        }
        private void btnLayananServis_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_LayananServis());
        }
        private void btnPemesananServis_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_PemesananServis());
        }
        private void btnLaporanServis_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_LaporanServis());
        }
       

    }
}

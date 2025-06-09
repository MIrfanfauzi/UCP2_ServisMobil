using System.Windows.Forms;

namespace ServisMobilApp
{
    partial class UC_PemesananServis
    {
        private Label lblID;
        private ComboBox cmbPelanggan, cmbKendaraan, cmbLayanan, cmbMekanik, cmbStatus;
        private DateTimePicker dtpTanggalServis;
        private DataGridView dgvPemesanan;
        private Button btnTambah, btnUbah, btnHapus;
        private Label lblPelanggan, lblKendaraan, lblLayanan, lblMekanik, lblStatus, lblTanggal;


        private void InitializeComponent()
        {
            this.lblID = new System.Windows.Forms.Label();
            this.cmbPelanggan = new System.Windows.Forms.ComboBox();
            this.cmbKendaraan = new System.Windows.Forms.ComboBox();
            this.cmbLayanan = new System.Windows.Forms.ComboBox();
            this.cmbMekanik = new System.Windows.Forms.ComboBox();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.dtpTanggalServis = new System.Windows.Forms.DateTimePicker();
            this.dgvPemesanan = new System.Windows.Forms.DataGridView();
            this.btnTambah = new System.Windows.Forms.Button();
            this.btnUbah = new System.Windows.Forms.Button();
            this.btnHapus = new System.Windows.Forms.Button();
            this.lblPelanggan = new System.Windows.Forms.Label();
            this.lblKendaraan = new System.Windows.Forms.Label();
            this.lblLayanan = new System.Windows.Forms.Label();
            this.lblMekanik = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTanggal = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPemesanan)).BeginInit();
            this.SuspendLayout();
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(150, 20);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(0, 20);
            this.lblID.TabIndex = 0;
            // 
            // cmbPelanggan
            // 
            this.cmbPelanggan.Location = new System.Drawing.Point(358, 153);
            this.cmbPelanggan.Name = "cmbPelanggan";
            this.cmbPelanggan.Size = new System.Drawing.Size(200, 28);
            this.cmbPelanggan.TabIndex = 1;
            // 
            // cmbKendaraan
            // 
            this.cmbKendaraan.Location = new System.Drawing.Point(358, 201);
            this.cmbKendaraan.Name = "cmbKendaraan";
            this.cmbKendaraan.Size = new System.Drawing.Size(200, 28);
            this.cmbKendaraan.TabIndex = 2;
            // 
            // cmbLayanan
            // 
            this.cmbLayanan.Location = new System.Drawing.Point(358, 247);
            this.cmbLayanan.Name = "cmbLayanan";
            this.cmbLayanan.Size = new System.Drawing.Size(200, 28);
            this.cmbLayanan.TabIndex = 3;
            // 
            // cmbMekanik
            // 
            this.cmbMekanik.Location = new System.Drawing.Point(358, 298);
            this.cmbMekanik.Name = "cmbMekanik";
            this.cmbMekanik.Size = new System.Drawing.Size(200, 28);
            this.cmbMekanik.TabIndex = 4;
            // 
            // cmbStatus
            // 
            this.cmbStatus.Location = new System.Drawing.Point(358, 357);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(200, 28);
            this.cmbStatus.TabIndex = 5;
            // 
            // dtpTanggalServis
            // 
            this.dtpTanggalServis.Location = new System.Drawing.Point(358, 406);
            this.dtpTanggalServis.Name = "dtpTanggalServis";
            this.dtpTanggalServis.Size = new System.Drawing.Size(200, 26);
            this.dtpTanggalServis.TabIndex = 6;
            // 
            // dgvPemesanan
            // 
            this.dgvPemesanan.ColumnHeadersHeight = 34;
            this.dgvPemesanan.Location = new System.Drawing.Point(800, 153);
            this.dgvPemesanan.Name = "dgvPemesanan";
            this.dgvPemesanan.RowHeadersWidth = 62;
            this.dgvPemesanan.Size = new System.Drawing.Size(802, 514);
            this.dgvPemesanan.TabIndex = 11;
            // 
            // btnTambah
            // 
            this.btnTambah.Location = new System.Drawing.Point(336, 516);
            this.btnTambah.Name = "btnTambah";
            this.btnTambah.Size = new System.Drawing.Size(148, 42);
            this.btnTambah.TabIndex = 7;
            this.btnTambah.Text = "Tambah";
            this.btnTambah.Click += new System.EventHandler(this.btnTambah_Click);
            // 
            // btnUbah
            // 
            this.btnUbah.Location = new System.Drawing.Point(61, 516);
            this.btnUbah.Name = "btnUbah";
            this.btnUbah.Size = new System.Drawing.Size(148, 42);
            this.btnUbah.TabIndex = 8;
            this.btnUbah.Text = "Ubah";
            this.btnUbah.Click += new System.EventHandler(this.btnUbah_Click);
            // 
            // btnHapus
            // 
            this.btnHapus.Location = new System.Drawing.Point(563, 516);
            this.btnHapus.Name = "btnHapus";
            this.btnHapus.Size = new System.Drawing.Size(148, 42);
            this.btnHapus.TabIndex = 9;
            this.btnHapus.Text = "Hapus";
            this.btnHapus.Click += new System.EventHandler(this.btnHapus_Click);
            // 
            // lblPelanggan
            // 
            this.lblPelanggan.Location = new System.Drawing.Point(0, 0);
            this.lblPelanggan.Name = "lblPelanggan";
            this.lblPelanggan.Size = new System.Drawing.Size(100, 23);
            this.lblPelanggan.TabIndex = 12;
            // 
            // lblKendaraan
            // 
            this.lblKendaraan.Location = new System.Drawing.Point(0, 0);
            this.lblKendaraan.Name = "lblKendaraan";
            this.lblKendaraan.Size = new System.Drawing.Size(100, 23);
            this.lblKendaraan.TabIndex = 13;
            // 
            // lblLayanan
            // 
            this.lblLayanan.Location = new System.Drawing.Point(0, 0);
            this.lblLayanan.Name = "lblLayanan";
            this.lblLayanan.Size = new System.Drawing.Size(100, 23);
            this.lblLayanan.TabIndex = 14;
            // 
            // lblMekanik
            // 
            this.lblMekanik.Location = new System.Drawing.Point(0, 0);
            this.lblMekanik.Name = "lblMekanik";
            this.lblMekanik.Size = new System.Drawing.Size(100, 23);
            this.lblMekanik.TabIndex = 15;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 23);
            this.lblStatus.TabIndex = 16;
            // 
            // lblTanggal
            // 
            this.lblTanggal.Location = new System.Drawing.Point(0, 0);
            this.lblTanggal.Name = "lblTanggal";
            this.lblTanggal.Size = new System.Drawing.Size(100, 23);
            this.lblTanggal.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(512, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(560, 61);
            this.label3.TabIndex = 18;
            this.label3.Text = "PEMESANAN SERVIS";
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.SkyBlue;
            this.btnImport.Location = new System.Drawing.Point(40, 598);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(136, 52);
            this.btnImport.TabIndex = 95;
            this.btnImport.Text = "Import Data";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.BackColor = System.Drawing.Color.Honeydew;
            this.btnAnalyze.Location = new System.Drawing.Point(336, 598);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(136, 52);
            this.btnAnalyze.TabIndex = 94;
            this.btnAnalyze.Text = "Analisis";
            this.btnAnalyze.UseVisualStyleBackColor = false;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.Cornsilk;
            this.btnRefresh.Location = new System.Drawing.Point(575, 598);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(136, 52);
            this.btnRefresh.TabIndex = 93;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(117, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 26);
            this.label1.TabIndex = 96;
            this.label1.Text = "Pelanggan";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(117, 201);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 26);
            this.label2.TabIndex = 97;
            this.label2.Text = "Kendaraan";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(117, 245);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 26);
            this.label4.TabIndex = 98;
            this.label4.Text = "Layanan";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(116, 300);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 26);
            this.label5.TabIndex = 99;
            this.label5.Text = "Mekanik";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(117, 359);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 26);
            this.label6.TabIndex = 100;
            this.label6.Text = "Status";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(117, 406);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(164, 26);
            this.label7.TabIndex = 101;
            this.label7.Text = "Tanggal Servis";
            // 
            // UC_PemesananServis
            // 
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.cmbPelanggan);
            this.Controls.Add(this.cmbKendaraan);
            this.Controls.Add(this.cmbLayanan);
            this.Controls.Add(this.cmbMekanik);
            this.Controls.Add(this.cmbStatus);
            this.Controls.Add(this.dtpTanggalServis);
            this.Controls.Add(this.btnTambah);
            this.Controls.Add(this.btnUbah);
            this.Controls.Add(this.btnHapus);
            this.Controls.Add(this.dgvPemesanan);
            this.Controls.Add(this.lblPelanggan);
            this.Controls.Add(this.lblKendaraan);
            this.Controls.Add(this.lblLayanan);
            this.Controls.Add(this.lblMekanik);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblTanggal);
            this.Name = "UC_PemesananServis";
            this.Size = new System.Drawing.Size(1655, 762);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPemesanan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label label3;
        private Button btnImport;
        private Button btnAnalyze;
        private Button btnRefresh;
        private Label label1;
        private Label label2;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
    }
}

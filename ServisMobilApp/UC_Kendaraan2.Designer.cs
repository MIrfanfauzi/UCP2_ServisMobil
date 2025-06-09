using System;

namespace ServisMobilApp
{
    partial class UC_Kendaraan2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.dgvKendaraan = new System.Windows.Forms.DataGridView();
            this.lblTahun = new System.Windows.Forms.Label();
            this.lblModel = new System.Windows.Forms.Label();
            this.lblMerek = new System.Windows.Forms.Label();
            this.lblPelanggan = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.txtModel = new System.Windows.Forms.TextBox();
            this.txtMerek = new System.Windows.Forms.TextBox();
            this.cmbPelanggan = new System.Windows.Forms.ComboBox();
            this.lblNoPlat = new System.Windows.Forms.Label();
            this.txtNoPlat = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbTahun = new System.Windows.Forms.ComboBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKendaraan)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Yellow;
            this.button2.Location = new System.Drawing.Point(625, 661);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(173, 83);
            this.button2.TabIndex = 100;
            this.button2.Text = "Edit";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.btnUbah_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Red;
            this.button3.Location = new System.Drawing.Point(102, 661);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(161, 83);
            this.button3.TabIndex = 99;
            this.button3.Text = "Hapus";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.btnHapus_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Lime;
            this.button4.Location = new System.Drawing.Point(370, 661);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(164, 83);
            this.button4.TabIndex = 98;
            this.button4.Text = "Tambah";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.btnTambah_Click);
            // 
            // dgvKendaraan
            // 
            this.dgvKendaraan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKendaraan.Location = new System.Drawing.Point(946, 219);
            this.dgvKendaraan.Name = "dgvKendaraan";
            this.dgvKendaraan.RowHeadersWidth = 62;
            this.dgvKendaraan.RowTemplate.Height = 28;
            this.dgvKendaraan.Size = new System.Drawing.Size(993, 661);
            this.dgvKendaraan.TabIndex = 97;
            this.dgvKendaraan.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvKendaraan_CellClick);
            // 
            // lblTahun
            // 
            this.lblTahun.AutoSize = true;
            this.lblTahun.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTahun.Location = new System.Drawing.Point(122, 475);
            this.lblTahun.Name = "lblTahun";
            this.lblTahun.Size = new System.Drawing.Size(76, 26);
            this.lblTahun.TabIndex = 101;
            this.lblTahun.Text = "Tahun";
            // 
            // lblModel
            // 
            this.lblModel.AutoSize = true;
            this.lblModel.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModel.Location = new System.Drawing.Point(122, 410);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(72, 26);
            this.lblModel.TabIndex = 102;
            this.lblModel.Text = "Model";
            // 
            // lblMerek
            // 
            this.lblMerek.AutoSize = true;
            this.lblMerek.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMerek.Location = new System.Drawing.Point(122, 352);
            this.lblMerek.Name = "lblMerek";
            this.lblMerek.Size = new System.Drawing.Size(73, 26);
            this.lblMerek.TabIndex = 103;
            this.lblMerek.Text = "Merek";
            // 
            // lblPelanggan
            // 
            this.lblPelanggan.AutoSize = true;
            this.lblPelanggan.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPelanggan.Location = new System.Drawing.Point(122, 293);
            this.lblPelanggan.Name = "lblPelanggan";
            this.lblPelanggan.Size = new System.Drawing.Size(185, 26);
            this.lblPelanggan.TabIndex = 104;
            this.lblPelanggan.Text = "Nama Pelanggan";
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblID.Location = new System.Drawing.Point(478, 241);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(0, 26);
            this.lblID.TabIndex = 105;
            // 
            // txtModel
            // 
            this.txtModel.Location = new System.Drawing.Point(352, 410);
            this.txtModel.Name = "txtModel";
            this.txtModel.Size = new System.Drawing.Size(248, 26);
            this.txtModel.TabIndex = 106;
            // 
            // txtMerek
            // 
            this.txtMerek.Location = new System.Drawing.Point(352, 352);
            this.txtMerek.Name = "txtMerek";
            this.txtMerek.Size = new System.Drawing.Size(248, 26);
            this.txtMerek.TabIndex = 107;
            // 
            // cmbPelanggan
            // 
            this.cmbPelanggan.Location = new System.Drawing.Point(352, 295);
            this.cmbPelanggan.Name = "cmbPelanggan";
            this.cmbPelanggan.Size = new System.Drawing.Size(248, 28);
            this.cmbPelanggan.TabIndex = 5;
            this.cmbPelanggan.SelectedIndexChanged += new System.EventHandler(this.cmbPelanggan_SelectedIndexChanged);
            // 
            // lblNoPlat
            // 
            this.lblNoPlat.AutoSize = true;
            this.lblNoPlat.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoPlat.Location = new System.Drawing.Point(122, 539);
            this.lblNoPlat.Name = "lblNoPlat";
            this.lblNoPlat.Size = new System.Drawing.Size(86, 26);
            this.lblNoPlat.TabIndex = 4;
            this.lblNoPlat.Text = "No Plat";
            // 
            // txtNoPlat
            // 
            this.txtNoPlat.Location = new System.Drawing.Point(352, 540);
            this.txtNoPlat.Name = "txtNoPlat";
            this.txtNoPlat.Size = new System.Drawing.Size(248, 26);
            this.txtNoPlat.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(122, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 26);
            this.label1.TabIndex = 2;
            this.label1.Text = "ID_Kendaraan";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(782, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(357, 61);
            this.label3.TabIndex = 1;
            this.label3.Text = "KENDARAAN";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // cmbTahun
            // 
            this.cmbTahun.Location = new System.Drawing.Point(352, 472);
            this.cmbTahun.Name = "cmbTahun";
            this.cmbTahun.Size = new System.Drawing.Size(248, 28);
            this.cmbTahun.TabIndex = 0;
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnImport.Location = new System.Drawing.Point(102, 808);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(164, 79);
            this.btnImport.TabIndex = 110;
            this.btnImport.Text = "Import Data";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnAnalyze.Location = new System.Drawing.Point(370, 808);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(164, 79);
            this.btnAnalyze.TabIndex = 109;
            this.btnAnalyze.Text = "Analisis";
            this.btnAnalyze.UseVisualStyleBackColor = false;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnRefresh.Location = new System.Drawing.Point(634, 808);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(164, 79);
            this.btnRefresh.TabIndex = 108;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            // 
            // UC_Kendaraan2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cmbTahun);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNoPlat);
            this.Controls.Add(this.lblNoPlat);
            this.Controls.Add(this.cmbPelanggan);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.dgvKendaraan);
            this.Controls.Add(this.lblTahun);
            this.Controls.Add(this.lblModel);
            this.Controls.Add(this.lblMerek);
            this.Controls.Add(this.lblPelanggan);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.txtModel);
            this.Controls.Add(this.txtMerek);
            this.Name = "UC_Kendaraan2";
            this.Size = new System.Drawing.Size(2000, 1000);
            this.Load += new System.EventHandler(this.UC_Kendaraan2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKendaraan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridView dgvKendaraan;
        private System.Windows.Forms.Label lblTahun;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.Label lblMerek;
        private System.Windows.Forms.Label lblPelanggan;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox txtModel;
        private System.Windows.Forms.TextBox txtMerek;
        private System.Windows.Forms.ComboBox cmbPelanggan;
        private System.Windows.Forms.Label lblNoPlat;
        private System.Windows.Forms.TextBox txtNoPlat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbTahun;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Button btnRefresh;
    }
}

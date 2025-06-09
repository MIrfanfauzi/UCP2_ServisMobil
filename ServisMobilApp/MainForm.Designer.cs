namespace ServisMobilApp
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelmenu = new System.Windows.Forms.Panel();
            this.btnLaporanServis = new System.Windows.Forms.Button();
            this.btnPemesananServis = new System.Windows.Forms.Button();
            this.btnLayananServis = new System.Windows.Forms.Button();
            this.btnKendaraan = new System.Windows.Forms.Button();
            this.btnMekanik = new System.Windows.Forms.Button();
            this.btnPelanggan = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelmenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelmenu
            // 
            this.panelmenu.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panelmenu.Controls.Add(this.btnLaporanServis);
            this.panelmenu.Controls.Add(this.btnPemesananServis);
            this.panelmenu.Controls.Add(this.btnLayananServis);
            this.panelmenu.Controls.Add(this.btnKendaraan);
            this.panelmenu.Controls.Add(this.btnMekanik);
            this.panelmenu.Controls.Add(this.btnPelanggan);
            this.panelmenu.Location = new System.Drawing.Point(12, 23);
            this.panelmenu.Name = "panelmenu";
            this.panelmenu.Size = new System.Drawing.Size(326, 1400);
            this.panelmenu.TabIndex = 0;
            // 
            // btnLaporanServis
            // 
            this.btnLaporanServis.BackColor = System.Drawing.SystemColors.Info;
            this.btnLaporanServis.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLaporanServis.Location = new System.Drawing.Point(71, 592);
            this.btnLaporanServis.Name = "btnLaporanServis";
            this.btnLaporanServis.Size = new System.Drawing.Size(173, 65);
            this.btnLaporanServis.TabIndex = 5;
            this.btnLaporanServis.Text = "Laporan Servis";
            this.btnLaporanServis.UseVisualStyleBackColor = false;
            this.btnLaporanServis.Click += new System.EventHandler(this.btnLaporanServis_Click);
            // 
            // btnPemesananServis
            // 
            this.btnPemesananServis.BackColor = System.Drawing.SystemColors.Info;
            this.btnPemesananServis.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPemesananServis.Location = new System.Drawing.Point(71, 495);
            this.btnPemesananServis.Name = "btnPemesananServis";
            this.btnPemesananServis.Size = new System.Drawing.Size(173, 64);
            this.btnPemesananServis.TabIndex = 4;
            this.btnPemesananServis.Text = "Pemesanan Servis";
            this.btnPemesananServis.UseVisualStyleBackColor = false;
            this.btnPemesananServis.Click += new System.EventHandler(this.btnPemesananServis_Click);
            // 
            // btnLayananServis
            // 
            this.btnLayananServis.BackColor = System.Drawing.SystemColors.Info;
            this.btnLayananServis.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLayananServis.Location = new System.Drawing.Point(71, 381);
            this.btnLayananServis.Name = "btnLayananServis";
            this.btnLayananServis.Size = new System.Drawing.Size(173, 73);
            this.btnLayananServis.TabIndex = 3;
            this.btnLayananServis.Text = "Layanan Servis";
            this.btnLayananServis.UseVisualStyleBackColor = false;
            this.btnLayananServis.Click += new System.EventHandler(this.btnLayananServis_Click);
            // 
            // btnKendaraan
            // 
            this.btnKendaraan.BackColor = System.Drawing.SystemColors.Info;
            this.btnKendaraan.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnKendaraan.Location = new System.Drawing.Point(71, 268);
            this.btnKendaraan.Name = "btnKendaraan";
            this.btnKendaraan.Size = new System.Drawing.Size(173, 72);
            this.btnKendaraan.TabIndex = 2;
            this.btnKendaraan.Text = "Kendaraan";
            this.btnKendaraan.UseVisualStyleBackColor = false;
            this.btnKendaraan.Click += new System.EventHandler(this.btnKendaraan_Click);
            // 
            // btnMekanik
            // 
            this.btnMekanik.BackColor = System.Drawing.SystemColors.Info;
            this.btnMekanik.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMekanik.Location = new System.Drawing.Point(71, 159);
            this.btnMekanik.Name = "btnMekanik";
            this.btnMekanik.Size = new System.Drawing.Size(173, 66);
            this.btnMekanik.TabIndex = 1;
            this.btnMekanik.Text = "Mekanik";
            this.btnMekanik.UseVisualStyleBackColor = false;
            this.btnMekanik.Click += new System.EventHandler(this.btnMekanik_Click);
            // 
            // btnPelanggan
            // 
            this.btnPelanggan.BackColor = System.Drawing.SystemColors.Info;
            this.btnPelanggan.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPelanggan.Location = new System.Drawing.Point(71, 58);
            this.btnPelanggan.Name = "btnPelanggan";
            this.btnPelanggan.Size = new System.Drawing.Size(173, 67);
            this.btnPelanggan.TabIndex = 0;
            this.btnPelanggan.Text = "Pelanggan";
            this.btnPelanggan.UseVisualStyleBackColor = false;
            this.btnPelanggan.Click += new System.EventHandler(this.btnPelanggan_Click);
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.panelContent.Location = new System.Drawing.Point(359, 23);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(2193, 1400);
            this.panelContent.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(2564, 1435);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelmenu);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.panelmenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelmenu;
        private System.Windows.Forms.Button btnLaporanServis;
        private System.Windows.Forms.Button btnPemesananServis;
        private System.Windows.Forms.Button btnLayananServis;
        private System.Windows.Forms.Button btnKendaraan;
        private System.Windows.Forms.Button btnMekanik;
        private System.Windows.Forms.Button btnPelanggan;
        private System.Windows.Forms.Panel panelContent;
    }
}

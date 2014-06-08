namespace SpotifyStatusApplet
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelProdName = new System.Windows.Forms.Label();
            this.labelVer = new System.Windows.Forms.Label();
            this.labelCopy = new System.Windows.Forms.Label();
            this.labelComp = new System.Windows.Forms.Label();
            this.textBoxDesc = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDesc, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(432, 299);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(210, 143);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.labelComp, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.labelCopy, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelVer, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelProdName, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(219, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(210, 100);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // labelProdName
            // 
            this.labelProdName.Location = new System.Drawing.Point(3, 0);
            this.labelProdName.Name = "labelProdName";
            this.labelProdName.Size = new System.Drawing.Size(194, 25);
            this.labelProdName.TabIndex = 0;
            this.labelProdName.Text = "ProductName";
            this.labelProdName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelVer
            // 
            this.labelVer.Location = new System.Drawing.Point(3, 25);
            this.labelVer.Name = "labelVer";
            this.labelVer.Size = new System.Drawing.Size(204, 25);
            this.labelVer.TabIndex = 1;
            this.labelVer.Text = "Version";
            this.labelVer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCopy
            // 
            this.labelCopy.Location = new System.Drawing.Point(3, 50);
            this.labelCopy.Name = "labelCopy";
            this.labelCopy.Size = new System.Drawing.Size(204, 25);
            this.labelCopy.TabIndex = 2;
            this.labelCopy.Text = "Copyright";
            this.labelCopy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelComp
            // 
            this.labelComp.Location = new System.Drawing.Point(3, 75);
            this.labelComp.Name = "labelComp";
            this.labelComp.Size = new System.Drawing.Size(204, 25);
            this.labelComp.TabIndex = 3;
            this.labelComp.Text = "Company Name";
            this.labelComp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxDesc
            // 
            this.textBoxDesc.Location = new System.Drawing.Point(219, 152);
            this.textBoxDesc.Multiline = true;
            this.textBoxDesc.Name = "textBoxDesc";
            this.textBoxDesc.ReadOnly = true;
            this.textBoxDesc.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDesc.Size = new System.Drawing.Size(210, 144);
            this.textBoxDesc.TabIndex = 2;
            this.textBoxDesc.Text = "Description";
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 323);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AboutBox";
            this.Text = "About SpotifyStatusApplet";
            this.Load += new System.EventHandler(this.AboutBox_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelCopy;
        private System.Windows.Forms.Label labelVer;
        private System.Windows.Forms.Label labelProdName;
        private System.Windows.Forms.Label labelComp;
        private System.Windows.Forms.TextBox textBoxDesc;

    }
}
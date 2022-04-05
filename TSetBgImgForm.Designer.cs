namespace Riches.Visio
{
    partial class TSetBgImgForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BackGrndColorEdit = new DevExpress.XtraEditors.ColorEdit();
            this.OKBtn = new DevExpress.XtraEditors.SimpleButton();
            this.CancelBtn = new DevExpress.XtraEditors.SimpleButton();
            this.ImgLayOutcomboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.SelectImgBtn = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackGrndColorEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgLayOutcomboBoxEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(4, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(224, 149);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // BackGrndColorEdit
            // 
            this.BackGrndColorEdit.EditValue = System.Drawing.Color.Empty;
            this.BackGrndColorEdit.Location = new System.Drawing.Point(94, 216);
            this.BackGrndColorEdit.Name = "BackGrndColorEdit";
            this.BackGrndColorEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.BackGrndColorEdit.Size = new System.Drawing.Size(119, 21);
            this.BackGrndColorEdit.TabIndex = 1;
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(60, 251);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 2;
            this.OKBtn.Text = "确定";
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(141, 251);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 3;
            this.CancelBtn.Text = "取消";
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // ImgLayOutcomboBoxEdit
            // 
            this.ImgLayOutcomboBoxEdit.Location = new System.Drawing.Point(94, 189);
            this.ImgLayOutcomboBoxEdit.Name = "ImgLayOutcomboBoxEdit";
            this.ImgLayOutcomboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.ImgLayOutcomboBoxEdit.Properties.Items.AddRange(new object[] {
            "缺省",
            "平铺",
            "居中",
            "伸缩",
            "扩展"});
            this.ImgLayOutcomboBoxEdit.Size = new System.Drawing.Size(119, 21);
            this.ImgLayOutcomboBoxEdit.TabIndex = 4;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(16, 192);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(72, 14);
            this.labelControl1.TabIndex = 5;
            this.labelControl1.Text = "图片展示方式";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(40, 219);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(48, 14);
            this.labelControl2.TabIndex = 6;
            this.labelControl2.Text = "背景颜色";
            // 
            // SelectImgBtn
            // 
            this.SelectImgBtn.Location = new System.Drawing.Point(40, 157);
            this.SelectImgBtn.Name = "SelectImgBtn";
            this.SelectImgBtn.Size = new System.Drawing.Size(75, 23);
            this.SelectImgBtn.TabIndex = 7;
            this.SelectImgBtn.Text = "选择图片";
            this.SelectImgBtn.Click += new System.EventHandler(this.SelectImgBtn_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(138, 157);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 8;
            this.simpleButton1.Text = "清除图片";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // TSetBgImgForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(228, 280);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.SelectImgBtn);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.ImgLayOutcomboBoxEdit);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.BackGrndColorEdit);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.Name = "TSetBgImgForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "背景图设置";
            this.Shown += new System.EventHandler(this.TSetBgImgForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackGrndColorEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgLayOutcomboBoxEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraEditors.ColorEdit BackGrndColorEdit;
        private DevExpress.XtraEditors.SimpleButton OKBtn;
        private DevExpress.XtraEditors.SimpleButton CancelBtn;
        private DevExpress.XtraEditors.ComboBoxEdit ImgLayOutcomboBoxEdit;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.SimpleButton SelectImgBtn;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
    }
}
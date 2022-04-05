
namespace Riches.Visio
{
    partial class FormDraw
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDraw));
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBoxOpen = new System.Windows.Forms.PictureBox();
            this.pictureBoxSave = new System.Windows.Forms.PictureBox();
            this.ButtonFree = new System.Windows.Forms.PictureBox();
            this.buttonSelectAll = new System.Windows.Forms.PictureBox();
            this.buttonCanel = new System.Windows.Forms.PictureBox();
            this.buttonRedo = new System.Windows.Forms.PictureBox();
            this.buttonCircle = new System.Windows.Forms.PictureBox();
            this.buttonClear = new System.Windows.Forms.PictureBox();
            this.buttonText = new System.Windows.Forms.PictureBox();
            this.buttonPolygon = new System.Windows.Forms.PictureBox();
            this.buttonRectangle = new System.Windows.Forms.PictureBox();
            this.buttoTriangle = new System.Windows.Forms.PictureBox();
            this.buttonLine = new System.Windows.Forms.PictureBox();
            this.buttonDot = new System.Windows.Forms.PictureBox();
            this.buttonNone = new System.Windows.Forms.PictureBox();
            this.panel1 = new Riches.Visio.CanvasPanel();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonFree)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonSelectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonCanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRedo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonCircle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonClear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonPolygon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRectangle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttoTriangle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonLine)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonDot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonNone)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.panel2.Controls.Add(this.pictureBoxOpen);
            this.panel2.Controls.Add(this.pictureBoxSave);
            this.panel2.Controls.Add(this.ButtonFree);
            this.panel2.Controls.Add(this.buttonSelectAll);
            this.panel2.Controls.Add(this.buttonCanel);
            this.panel2.Controls.Add(this.buttonRedo);
            this.panel2.Controls.Add(this.buttonCircle);
            this.panel2.Controls.Add(this.buttonClear);
            this.panel2.Controls.Add(this.buttonText);
            this.panel2.Controls.Add(this.buttonPolygon);
            this.panel2.Controls.Add(this.buttonRectangle);
            this.panel2.Controls.Add(this.buttoTriangle);
            this.panel2.Controls.Add(this.buttonLine);
            this.panel2.Controls.Add(this.buttonDot);
            this.panel2.Controls.Add(this.buttonNone);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(940, 45);
            this.panel2.TabIndex = 1;
            // 
            // pictureBoxOpen
            // 
            this.pictureBoxOpen.Image = global::Riches.Visio.Properties.Resources.打开文件;
            this.pictureBoxOpen.Location = new System.Drawing.Point(530, 4);
            this.pictureBoxOpen.Name = "pictureBoxOpen";
            this.pictureBoxOpen.Size = new System.Drawing.Size(34, 38);
            this.pictureBoxOpen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxOpen.TabIndex = 14;
            this.pictureBoxOpen.TabStop = false;
            this.pictureBoxOpen.Tag = "7";
            this.pictureBoxOpen.Click += new System.EventHandler(this.pictureBoxOpen_Click);
            // 
            // pictureBoxSave
            // 
            this.pictureBoxSave.Image = global::Riches.Visio.Properties.Resources.保存;
            this.pictureBoxSave.Location = new System.Drawing.Point(589, 4);
            this.pictureBoxSave.Name = "pictureBoxSave";
            this.pictureBoxSave.Size = new System.Drawing.Size(34, 38);
            this.pictureBoxSave.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSave.TabIndex = 13;
            this.pictureBoxSave.TabStop = false;
            this.pictureBoxSave.Tag = "7";
            this.pictureBoxSave.Click += new System.EventHandler(this.pictureBoxSave_Click);
            // 
            // ButtonFree
            // 
            this.ButtonFree.Image = global::Riches.Visio.Properties.Resources.画笔;
            this.ButtonFree.Location = new System.Drawing.Point(415, 4);
            this.ButtonFree.Name = "ButtonFree";
            this.ButtonFree.Size = new System.Drawing.Size(34, 38);
            this.ButtonFree.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ButtonFree.TabIndex = 12;
            this.ButtonFree.TabStop = false;
            this.ButtonFree.Tag = "8";
            this.ButtonFree.Click += new System.EventHandler(this.ButtonFree_Click);
            this.ButtonFree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Image = global::Riches.Visio.Properties.Resources.全选;
            this.buttonSelectAll.Location = new System.Drawing.Point(725, 4);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(34, 38);
            this.buttonSelectAll.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonSelectAll.TabIndex = 11;
            this.buttonSelectAll.TabStop = false;
            this.buttonSelectAll.Tag = "7";
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonCanel
            // 
            this.buttonCanel.Image = global::Riches.Visio.Properties.Resources.撤销;
            this.buttonCanel.Location = new System.Drawing.Point(629, 4);
            this.buttonCanel.Name = "buttonCanel";
            this.buttonCanel.Size = new System.Drawing.Size(34, 38);
            this.buttonCanel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonCanel.TabIndex = 10;
            this.buttonCanel.TabStop = false;
            this.buttonCanel.Tag = "7";
            this.buttonCanel.Click += new System.EventHandler(this.buttonCanel_Click);
            // 
            // buttonRedo
            // 
            this.buttonRedo.Image = ((System.Drawing.Image)(resources.GetObject("buttonRedo.Image")));
            this.buttonRedo.Location = new System.Drawing.Point(677, 4);
            this.buttonRedo.Name = "buttonRedo";
            this.buttonRedo.Size = new System.Drawing.Size(34, 38);
            this.buttonRedo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonRedo.TabIndex = 9;
            this.buttonRedo.TabStop = false;
            this.buttonRedo.Tag = "7";
            this.buttonRedo.Click += new System.EventHandler(this.buttonRedo_Click);
            // 
            // buttonCircle
            // 
            this.buttonCircle.Image = global::Riches.Visio.Properties.Resources.圆;
            this.buttonCircle.Location = new System.Drawing.Point(253, 4);
            this.buttonCircle.Name = "buttonCircle";
            this.buttonCircle.Size = new System.Drawing.Size(34, 38);
            this.buttonCircle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonCircle.TabIndex = 8;
            this.buttonCircle.TabStop = false;
            this.buttonCircle.Tag = "5";
            this.buttonCircle.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonCircle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonClear
            // 
            this.buttonClear.Image = global::Riches.Visio.Properties.Resources.清空;
            this.buttonClear.Location = new System.Drawing.Point(773, 4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(34, 38);
            this.buttonClear.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonClear.TabIndex = 7;
            this.buttonClear.TabStop = false;
            this.buttonClear.Tag = "5";
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonText
            // 
            this.buttonText.Image = global::Riches.Visio.Properties.Resources.文本;
            this.buttonText.Location = new System.Drawing.Point(353, 4);
            this.buttonText.Name = "buttonText";
            this.buttonText.Size = new System.Drawing.Size(34, 38);
            this.buttonText.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonText.TabIndex = 6;
            this.buttonText.TabStop = false;
            this.buttonText.Tag = "7";
            this.buttonText.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonPolygon
            // 
            this.buttonPolygon.Image = global::Riches.Visio.Properties.Resources.多边形;
            this.buttonPolygon.Location = new System.Drawing.Point(303, 4);
            this.buttonPolygon.Name = "buttonPolygon";
            this.buttonPolygon.Size = new System.Drawing.Size(34, 38);
            this.buttonPolygon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonPolygon.TabIndex = 5;
            this.buttonPolygon.TabStop = false;
            this.buttonPolygon.Tag = "6";
            this.buttonPolygon.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonPolygon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonRectangle
            // 
            this.buttonRectangle.Image = global::Riches.Visio.Properties.Resources.矩形;
            this.buttonRectangle.Location = new System.Drawing.Point(203, 4);
            this.buttonRectangle.Name = "buttonRectangle";
            this.buttonRectangle.Size = new System.Drawing.Size(34, 38);
            this.buttonRectangle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonRectangle.TabIndex = 4;
            this.buttonRectangle.TabStop = false;
            this.buttonRectangle.Tag = "4";
            this.buttonRectangle.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonRectangle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttoTriangle
            // 
            this.buttoTriangle.Image = global::Riches.Visio.Properties.Resources.三角形;
            this.buttoTriangle.Location = new System.Drawing.Point(153, 4);
            this.buttoTriangle.Name = "buttoTriangle";
            this.buttoTriangle.Size = new System.Drawing.Size(34, 38);
            this.buttoTriangle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttoTriangle.TabIndex = 3;
            this.buttoTriangle.TabStop = false;
            this.buttoTriangle.Tag = "3";
            this.buttoTriangle.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttoTriangle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonLine
            // 
            this.buttonLine.Image = global::Riches.Visio.Properties.Resources.线;
            this.buttonLine.Location = new System.Drawing.Point(103, 4);
            this.buttonLine.Name = "buttonLine";
            this.buttonLine.Size = new System.Drawing.Size(34, 38);
            this.buttonLine.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonLine.TabIndex = 2;
            this.buttonLine.TabStop = false;
            this.buttonLine.Tag = "2";
            this.buttonLine.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonLine.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonDot
            // 
            this.buttonDot.Image = global::Riches.Visio.Properties.Resources.点;
            this.buttonDot.Location = new System.Drawing.Point(53, 4);
            this.buttonDot.Name = "buttonDot";
            this.buttonDot.Size = new System.Drawing.Size(34, 38);
            this.buttonDot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonDot.TabIndex = 1;
            this.buttonDot.TabStop = false;
            this.buttonDot.Tag = "1";
            this.buttonDot.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonDot.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // buttonNone
            // 
            this.buttonNone.BackColor = System.Drawing.Color.White;
            this.buttonNone.Image = global::Riches.Visio.Properties.Resources.指针;
            this.buttonNone.Location = new System.Drawing.Point(3, 4);
            this.buttonNone.Name = "buttonNone";
            this.buttonNone.Size = new System.Drawing.Size(34, 38);
            this.buttonNone.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.buttonNone.TabIndex = 0;
            this.buttonNone.TabStop = false;
            this.buttonNone.Tag = "0";
            this.buttonNone.Click += new System.EventHandler(this.ButtonFree_Click);
            this.buttonNone.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonPolygon_MouseDown);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 45);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(940, 405);
            this.panel1.TabIndex = 2;
            // 
            // FormEdu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(940, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormEdu";
            this.Text = " 动态几何";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOpen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonFree)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonSelectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonCanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRedo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonCircle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonClear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonPolygon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonRectangle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttoTriangle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonLine)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonDot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buttonNone)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox buttonNone;
        private CanvasPanel panel1;
        private System.Windows.Forms.PictureBox buttonPolygon;
        private System.Windows.Forms.PictureBox buttonRectangle;
        private System.Windows.Forms.PictureBox buttoTriangle;
        private System.Windows.Forms.PictureBox buttonLine;
        private System.Windows.Forms.PictureBox buttonDot;
        private System.Windows.Forms.PictureBox buttonText;
        private System.Windows.Forms.PictureBox buttonClear;
        private System.Windows.Forms.PictureBox buttonCircle;
        private System.Windows.Forms.PictureBox buttonCanel;
        private System.Windows.Forms.PictureBox buttonRedo;
        private System.Windows.Forms.PictureBox buttonSelectAll;
        private System.Windows.Forms.PictureBox ButtonFree;
        private System.Windows.Forms.PictureBox pictureBoxSave;
        private System.Windows.Forms.PictureBox pictureBoxOpen;
    }
}
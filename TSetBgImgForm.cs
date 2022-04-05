using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Riches.Visio
{
    public partial class TSetBgImgForm : Form
    {
        Control PPcontrol;
        public TSetBgImgForm(Control APForm)
        {
            InitializeComponent();
            PPcontrol = APForm;
        }

        private void SelectImgBtn_Click(object sender, EventArgs e)
        {
              System.Windows.Forms.OpenFileDialog ofd = new OpenFileDialog();
              if (ofd.ShowDialog() == DialogResult.OK)
              {
                  pictureBox1.Image = LoadImg(ofd.FileName);

                  string DestDir = Application.StartupPath + "\\IdePic\\";
                  if (!System.IO.Directory.Exists(DestDir))
                      System.IO.Directory.CreateDirectory(DestDir);

                  string LocalPathName = DestDir + PPcontrol.Name + System.IO.Path.GetExtension(ofd.FileName);
                  if (System.IO.File.Exists(LocalPathName))
                      System.IO.File.Delete(LocalPathName);

                  System.IO.File.Copy(ofd.FileName, LocalPathName);

                  PPcontrol.Tag = LocalPathName;
              }
        }

        public Image LoadImg(string FilePathName)
        {
            if (System.IO.File.Exists(FilePathName))
            {
                return Image.FromFile(FilePathName);
            }
            return null;
        }

        private void TSetBgImgForm_Shown(object sender, EventArgs e)
        {
            this.pictureBox1.Image = PPcontrol.BackgroundImage;
            this.ImgLayOutcomboBoxEdit.SelectedIndex = (int)(PPcontrol.BackgroundImageLayout);
            BackGrndColorEdit.Color = PPcontrol.BackColor;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            PPcontrol.BackgroundImageLayout = (ImageLayout)(this.ImgLayOutcomboBoxEdit.SelectedIndex);
            PPcontrol.BackgroundImage = this.pictureBox1.Image;
            PPcontrol.BackColor = this.BackGrndColorEdit.Color;
            this.DialogResult = DialogResult.OK;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = null;
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}

using Riches.Visio.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Riches.Visio
{
    public partial class FormDraw : Form
    {
        Geometry.CanvasHelper canvasHelper;
        public FormDraw()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            canvasHelper = new Geometry.CanvasHelper(this.panel1);
            this.Resize += FormEdu_Resize;
            canvasHelper.OnDrawModeChanged += CanvasHelper_OnDrawModeChanged;
            this.ManuCursor = GetCursor(Properties.Resources.画笔, 13, 34);
            //return;
            //canvasHelper.AddShape(new Dot() { X = 100, Y = 100 });
            //canvasHelper.AddShape(new Dot() { X = 200, Y = 200 });
            //var f = new Dot() { X = 120, Y = 120 };
            //var t = new Dot() { X = 180, Y = 180 };
            //canvasHelper.AddShape(f);
            //canvasHelper.AddShape(t);
            //var line = new Line() { From = f, To = t };
            //List<Line> lines = new List<Line>();
            //lines.Add(line);
            //lines.Add(line);
            //canvasHelper.AddShape(line);
        }

        private void CanvasHelper_OnDrawModeChanged(Geometry.DrawMode drawMode)
        {
            foreach (PictureBox ctl in panel2.Controls)
            {
                var dr = (Geometry.DrawMode)Convert.ToInt32(ctl.Tag.ToString());
                if (dr == drawMode)
                    ctl.BackColor = Color.WhiteSmoke;
                else
                    ctl.BackColor = panel2.BackColor;
            }
        }

        private void FormEdu_Resize(object sender, EventArgs e)
        {
            //  this.panel1.SetBounds(0, 0, this.Width * 2, this.Height * 2);
        }

        private void FormEdu_Shown(object sender, EventArgs e)
        {

        }

        private void buttonPolygon_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.canvasHelper.ClearShapes();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var g = Graphics.FromHwnd(this.panel1.Handle))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                List<PointF> points = new List<PointF>();
                float cx = 200f, cy = 200f;
                for (float a = 1; a < 3 * Math.PI; a += 0.05f)
                {
                    var r = 100 * Math.Cos(8 * a);
                    var x = r * Math.Cos(a) + cx;
                    var y = r * Math.Sin(a) + cy;
                    points.Add(new PointF((float)x, (float)y));
                }
                g.DrawLines(Pens.Red, points.ToArray());
                points.Clear();
                for (float a = 1; a < 30 * Math.PI; a += 0.05f)
                {
                    var r = (20 + 10 * a);
                    var x = r * Math.Cos(a) + cx;
                    var y = r * Math.Sin(a) + cy;
                    points.Add(new PointF((float)x, (float)y));
                }

                g.DrawLines(Pens.Red, points.ToArray());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void buttonCanel_Click(object sender, EventArgs e)
        {
            this.canvasHelper.Revoke();
        }

        private void buttonRedo_Click(object sender, EventArgs e)
        {
            this.canvasHelper.Redo();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            this.canvasHelper.SelectAll();
        }

        private void buttonPolygon_Click(object sender, EventArgs e)
        {

        }

        private void ButtonFree_Click(object sender, EventArgs e)
        {
            foreach (PictureBox ctl in panel2.Controls)
            {
                ctl.BackColor = panel2.BackColor;
            }
            PictureBox btn = sender as PictureBox;
            btn.BackColor = Color.WhiteSmoke;
            canvasHelper.DrawMode = (Geometry.DrawMode)(Convert.ToInt32(btn.Tag.ToString()));
            if (canvasHelper.DrawMode == Geometry.DrawMode.ManulWrite)
                this.panel1.Cursor = ManuCursor;// new Cursor(Properties.Resources.画笔.GetHicon());
            else
                this.panel1.Cursor = Cursors.Default;

        }
        private Cursor ManuCursor;
        public Cursor GetCursor(Bitmap img, int hotSpotX = 0, int hotSpotY = 0)
        {
            Bitmap curImg = new Bitmap(img.Width * 2, img.Height * 2);
            Graphics g = Graphics.FromImage(curImg);

            g.Clear(Color.FromArgb(0, 0, 0, 0));
           // g.DrawRectangle(Pens.Red, 0, 0, curImg.Width-1, curImg.Height-1);
            g.DrawImage(img, img.Width - hotSpotX, img.Height- hotSpotY, img.Width, img.Height);
            Cursor cur = new Cursor(curImg.GetHicon());

            g.Dispose();
            curImg.Dispose();

            return cur;

        }

        private void pictureBoxSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(*.json)|*.json";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                this.canvasHelper.SaveFile( saveFileDialog.FileName);
        }

        private void pictureBoxOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog  openFileDialog = new  OpenFileDialog();
            openFileDialog.Filter = "(*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                this.canvasHelper.LoadFile(openFileDialog.FileName);
        }
    }
}

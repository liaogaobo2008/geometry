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
    public partial class MDIChildForm : Form
    {
        public   Geometry.CanvasHelper canvasHelper;
        public event Action<Geometry.DrawMode> OnDrawModeChanged;
        public event MouseEventHandler OnMyMouseMove;

        

        public MDIChildForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.KeyPreview = true;
          
            canvasHelper = new Geometry.CanvasHelper(this.canvasPanel1);
            this.Resize += MDIChildForm_Resize; 
            canvasHelper.OnDrawModeChanged += CanvasHelper_OnDrawModeChanged;
            this.canvasPanel1.MouseMove += CanvasPanel1_MouseMove;
            this.Shown += MDIChildForm_Shown;
        }

        private void MDIChildForm_Shown(object sender, EventArgs e)
        {
            this.buttonMove.Left = this.canvasPanel1.Width - this.buttonMove.Width;// * 2;
            this.buttonMove.Top = this.canvasPanel1.Height - buttonMove.Height;// * 2;

            this.canvasHelper.MoveCenterTo((this.canvasPanel1.Width+this.canvasPanel1.HorizontalScroll.Value)/ 2,
                (this.canvasPanel1.Height+this.canvasPanel1.VerticalScroll.Value)/2);
        }

        private void CanvasPanel1_MouseMove(object sender, MouseEventArgs ee)
        {
            MouseEventArgs e = new MouseEventArgs(ee.Button, ee.Clicks, ee.X + this.canvasPanel1.HorizontalScroll.Value,
                  ee.Y + this.canvasPanel1.VerticalScroll.Value, ee.Delta);
            OnMyMouseMove?.Invoke(sender, e);
        }

        private void MDIChildForm_Resize(object sender, EventArgs e)
        {
           
        }

        private void CanvasHelper_OnDrawModeChanged( Geometry.DrawMode drawMode)
        {
            OnDrawModeChanged?.Invoke(drawMode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
           
        }

        private void canvasPanel1_Scroll(object sender, ScrollEventArgs e)
        {
            this.canvasHelper.Update();
        }

        private void canvasPanel1_Paint(object sender, PaintEventArgs e)
        {
          
        }
    }
}
// https://blog.csdn.net/xingyu_soft/article/details/50612565
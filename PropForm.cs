using Riches.Visio.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Riches.Visio
{
    public partial class PropForm : Form
    {
        private IShape _shape;
        public PropForm()
        {
            InitializeComponent();
        }
        public PropForm(IShape shape)
        {
            _shape = shape;
            InitializeComponent();
            textBoxRem.Text = shape.Text;

            if (shape.ShapeType== ShapeType.Circle)
            {
                Circle circle = shape as Circle;

                Label lab1 = new Label();
                lab1.Text = "圆心坐标";
                lab1.Anchor = AnchorStyles.Right;
                lab1.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab1, 0, 1);

                Label lab2 = new Label();
                lab2.Text = "x=" + circle.Center.X+",y=" + circle.Center.Y;
                lab2.Anchor = AnchorStyles.Left;
                lab2.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab2, 1, 1);

                Label lab3 = new Label();
                lab3.Text = "直径";
                lab3.Anchor = AnchorStyles.Right;
                lab3.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab3, 0, 2);

                Label lab4 = new Label();
                lab4.Text = circle.Diam.ToString();
                lab4.Anchor = AnchorStyles.Left;
                lab4.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab4, 1, 2);
            }
            else if(shape.ShapeType== ShapeType.Dot)
            {
                Dot dot = shape as Dot;

                Label lab = new Label();
                lab.Text = "点坐标";
                lab.Anchor = AnchorStyles.Right;
                lab.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab, 0, 1);

                lab = new Label();
                lab.Text ="x=" +dot.X + ",y=" + dot.Y;
                lab.Anchor = AnchorStyles.Left;
                lab.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab, 1, 1);
            }
            else if (shape.ShapeType == ShapeType.Line)
            {
                Line line = shape as Line;

                Label lab = new Label();
                lab.Text = "来源点" + line.From.Text;
                lab.Anchor = AnchorStyles.Right;
                lab.Dock = DockStyle.None;
                lab.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab, 0, 1);

                Label lab1 = new Label();
                lab1.Text ="x=" + line.From.X + ",y=" + line.From.Y;
              
                lab1.BackColor = Color.Transparent;
                lab1.ForeColor = Color.Blue;
                tableLayoutPanel1.Controls.Add(lab1, 1, 1);
                lab1.Anchor = AnchorStyles.Left;

                var lab2 = new Label();
                lab2.Text = "目标点" + line.To.Text;
              
                lab2.BackColor = Color.Transparent;
                tableLayoutPanel1.Controls.Add(lab2, 0, 2);
                lab2.Anchor = AnchorStyles.Right;

                var lab3 = new Label();
                lab3.Text = "x=" + line.To.X + ",y=" + line.To.Y;
               
                lab3.BackColor = Color.Transparent;
                lab3.ForeColor = Color.Blue;
                tableLayoutPanel1.Controls.Add(lab3, 1, 2);
                lab3.Anchor = AnchorStyles.Left;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _shape.Text = this.textBoxRem.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

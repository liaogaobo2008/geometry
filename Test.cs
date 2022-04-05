using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Riches.Visio
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private void Test_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            string s = "宋体宋体宋体宋体宋体宋体宋体宋体宋体";
            RectangleF rect = new RectangleF(350, 0, 400, 200);
            Font font = new Font("宋体",40f);
            StringFormat format = StringFormat.GenericTypographic;
            float dpi = g.DpiY;
            using (GraphicsPath path = GetStringPath(s, dpi, rect, font, format))
            {
                //阴影代码
                //RectangleF off = rect;
                //off.Offset(5, 5);//阴影偏移
                //using (GraphicsPath offPath = GetStringPath(s, dpi, off, font, format))
                //{
                //    Brush b = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                //    g.FillPath(b, offPath);
                //    b.Dispose();
                //}
                g.SmoothingMode = SmoothingMode.AntiAlias;//设置字体质量
                Point p1 = new Point(0, 0);
                Point p2 = new Point(200, 200);
                g.DrawLine(Pens.Red, p1, p2);
                g.DrawPath(Pens.Black, path);//绘制轮廓（描边）
                g.FillPath(Brushes.Red, path);//填充轮廓（填充）
            }
        }
        GraphicsPath GetStringPath(string s, float dpi, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            // Convert font size into appropriate coordinates
            float emSize = dpi * font.SizeInPoints / 72;
            path.AddString(s, font.FontFamily, (int)font.Style, emSize, rect, format);
            Point p1 = new Point(0, 0);
            Point p2 = new Point(200, 200);
            path.AddLine(p1, p2);
            return path;
        }
    }
}

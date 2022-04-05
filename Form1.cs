using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExpressionEvaluator;
using Newtonsoft.Json;

namespace Riches.Visio
{
    public partial class Form1 : Form
    {
     
        public Form1()
        {
            InitializeComponent();
            FlowDsg = new TVisioDraw(null, null, null);
            FlowDsg.LinkPanel(this);
            FlowDsg.CurrentDrawType = TVisioDrawType.DrawNode;
            FlowDsg.OnSetNodeText += form_SetOneText;

            // Bitmap bmp = (Bitmap)Bitmap.FromFile(@"......");
            //var json1 = JsonConvert.SerializeObject(new MyImg() { Image = Riches.Visio.Properties.Resources.redo, I = 666 },
            //                                       new ImageConverter());
            //var aclass = JsonConvert.DeserializeObject<MyImg>(json1, new ImageConverter());

            //// var json = JsonConvert.SerializeObject(Riches.Visio.Properties.Resources.redo, new ImageConverter());
            ////var aclass = JsonConvert.DeserializeObject<Bitmap>(json, new ImageConverter());

            //Console.WriteLine(json1);
            //List<List<Point>> points = new List<List<Point>>();
            //List<Point> pts1 = new List<Point>();
            //pts1.Add(new Point(1, 1));
            //pts1.Add(new Point(2, 2));
            //points.Add(pts1);

            //List<Point> pts2 = new List<Point>();
            //pts2.Add(new Point(1, 1));
            //pts2.Add(new Point(2, 2));
            //points.Add(pts2);
            //var str=JsonConvert.SerializeObject(points);
            //Console.WriteLine(str);
            var str=JsonConvert.SerializeObject(new Point(2, 2));
            Console.WriteLine(str);
        }
        TVisioDraw FlowDsg;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Paint += Form1_Paint;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
         
        }

        private void form_SetOneText(TVisioNodeBtn node)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            FlowDsg.HideLinePort();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FlowDsg.ShowLinePort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FlowDsg.CurrentDrawType = TVisioDrawType.DrawNode;
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
        private void CreateServiceFlow()
        {
            TVisioNodeBtn visioNode = new TVisioNodeBtn();
            visioNode.Name = "Start";
            visioNode.Text = "Web操作端";

            visioNode = new TVisioNodeBtn();
            visioNode.Name = "IOTInterface";
            visioNode.Text = "IOT访问服务";


            visioNode = new TVisioNodeBtn();
            visioNode.Name = "IOTService";
            visioNode.Text = "IOT平台";

            visioNode = new TVisioNodeBtn();
            visioNode.Name = "RDES";
            visioNode.Text = "RDES系统";


            visioNode = new TVisioNodeBtn();
            visioNode.Name = "RDES";
            visioNode.Text = "RDES系统";

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            List<TVisioNodeBtn> nodes = new List<TVisioNodeBtn>();
            TVisioNodeBtn fromBtn = null;
            for (int i = 0; i < 3; i++)
            {
                TVisioNodeBtn visioNode = new TVisioNodeBtn();
                visioNode.Left = i * 80;
                visioNode.Top = i * 80;
                this.Controls.Add(visioNode);
                nodes.Add(visioNode);
                if (fromBtn != null)
                {
                    Point f = new Point(fromBtn.Left + fromBtn.Width / 2, fromBtn.Top + fromBtn.Height / 2);
                    Point t = new Point(visioNode.Left + visioNode.Width / 2, visioNode.Top + visioNode.Height / 2);
                    TVisioLine line = new TVisioLine(f, t);
                    line.FromNode = fromBtn;
                    line.ToNode = visioNode;
                    fromBtn.WKLines.Add(line);
                    visioNode.WKLines.Add(line);
                }
                fromBtn = visioNode;
            }
            FlowDsg.ReDrawWKNodeLines();
        }
        public class testser
        {
            Font font = new Font("宋体",12f, FontStyle.Bold);
            public Font Font {
                get { return font; }
                set { this.font = value; }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
         //  Math.Round
          //  Math.Sqrt()
        }

       
    }
}

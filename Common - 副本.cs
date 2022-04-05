using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Riches.Visio
{
    namespace Common
    {
        public interface IMouseEvent
        {
            Control CanvasCtl { get; set; }
            void MouseDown(object sender, MouseEventArgs e);
            void MouseUp(object sender, MouseEventArgs e);
            void MouseLeave(object sender, EventArgs e);
            void MouseEnter(object sender, EventArgs e);
            void MouseMove(object sender, MouseEventArgs e);
            void MouseDoubleClick(object sender, MouseEventArgs e);
        }

        public enum ShapeType { None, Dot, Line, Triangle, FillArea, Circle, Text,RemAngle }

        public interface IShape : IMouseEvent
        {
            void Invalid(Graphics g);
            int Id { get; set; }
            void Delete();
            CanvasHelper Canvas { get; set; }
            bool IsCapture { get; }
            bool IsDraged { get; set; }
            bool IsActive { get; set; }
            bool IsDirty { get; set; }
            bool PtIn(Point p);
            ShapeType ShapeType { get; }
            String Text { get; set; }
            Point Location { get;  }
        }
        public class RemAngle : IShape
        {
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }

            public Point Location
            {
                get { return this.Dot.Location; }
            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
                this.Dot.RemAngles.Remove(this);
            }

            private Dot _dot;
            public Dot Dot
            {
                get
                {
                    return this._dot;
                }
                set
                {
                    this._dot = value;
                  //  _area = new Rectangle((int)this.Dot.X - Radios, (int)this.Dot.Y - Radios, Radios * 2, Radios * 2);
                }
            }
            public ShapeType ShapeType { get { return ShapeType.RemAngle; } }

            private Rectangle _area = Rectangle.Empty;
            private int _radio = 20;

            public int Radios 
            {
                get
                {
                    return _radio;
                }
                set
                {
                    _radio = value;
                }
            }
            private Rectangle Area
            {
                get
                {
                    return new Rectangle((int)this.Dot.X - Radios, (int)this.Dot.Y - Radios, Radios * 2, Radios * 2); ;
                }
            }
           
            public Line FromLine { get; set; }
            public Line ToLine { get; set; }
            public bool PtIn(Point p)
            {
                if (FromLine.From.PtIn(p))
                    return false;
                if (FromLine.To.PtIn(p))
                    return false;
                if (ToLine.From.PtIn(p))
                    return false;
                if (ToLine.To.PtIn(p))
                    return false;
                if (!MyMath.IsInCircle(this.Dot.X, this.Dot.Y, this.Radios, p.X, p.Y))
                    return false;
                var angle = Get0_360FromByPos(p, this.Dot.Location);
                
                if (angle > FromAngle && angle < this.FromAngle + Angle)
                    return true;
                else if(angle<FromAngle)
                {
                    var f = FromAngle - 360;
                    if (angle < f + Angle)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            private static Font Font = new Font("微软雅黑", 11f, FontStyle.Regular);
            private static Size TextSize;
            public string Text { get; set; } = "A";
        
            static RemAngle()
            {
                Font = new Font("微软雅黑", 12f, FontStyle.Bold);
                TextSize = TextRenderer.MeasureText("K", Font);
            }
            public float GetkByTwoDot(Point f,Point t)
            {
               
                if (f.X == t.X)
                    return float.MaxValue;
                return (float)(f.Y -t.Y) /(float) (f.X - t.X);
            }
            public float Get0_360FromByPos(Point p,Point c)
            {
                var t = new PointF(p.X - c.X, p.Y - c.Y);

                var k = GetkByTwoDot(p, c);
                var f =(float) (Math.Atan(k) * RadUnit);

                if (t.X > 0 && t.Y > 0)
                {

                }
                else if (t.X < 0 && t.Y > 0)
                {
                    f += 180;
                }
                else if (t.X < 0 && t.Y < 0)
                {
                    f += 180;
                }
                else if (t.X > 0 && t.Y < 0)
                {
                    f += 360;
                }
                return f % 360;
            }

            private void Refresh()
            {
                double angle = 0;
                var toPoint1 = this.FromLine.From != this.Dot ? this.FromLine.From.Pf : this.FromLine.To.Pf;//另外一段
                var toPoint2 = this.ToLine.From != this.Dot ? this.ToLine.From.Pf : this.ToLine.To.Pf;//另外一段

                toPoint1 = new PointF(toPoint1.X - this.Dot.X, toPoint1.Y - this.Dot.Y);

                toPoint2 = new PointF(toPoint2.X - this.Dot.X, toPoint2.Y - this.Dot.Y);

                var k1 = this.FromLine.K;
                var k2 = this.ToLine.K;
                var f = (Math.Atan(k1) * RadUnit);
                var t = (Math.Atan(k2) * RadUnit);
                if (toPoint1.X > 0 && toPoint1.Y > 0)
                {

                }
                else if (toPoint1.X < 0 && toPoint1.Y > 0)
                {
                    f += 180;
                }
                else if (toPoint1.X < 0 && toPoint1.Y < 0)
                {
                    f += 180;
                }
                else if (toPoint1.X > 0 && toPoint1.Y < 0)
                {
                    f += 360;
                }

                if (toPoint2.X > 0 && toPoint2.Y > 0)
                {

                }
                else if (toPoint2.X < 0 && toPoint2.Y > 0)
                {
                    t += 180;
                }
                else if (toPoint2.X < 0 && toPoint2.Y < 0)
                {
                    t += 180;
                }
                else if (toPoint2.X > 0 && toPoint2.Y < 0)
                {
                    t += 360;
                }

                if (f > t)
                    angle = 360 + t - f;
                else
                    angle = t - f;//思路是从起点转动多少度

                var cAngle = (f + angle / 2) % 360;
                float rad = (float)((float)Math.PI * cAngle / 180);
                var kc = (float)Math.Tan(rad);
                var x = this.Dot.X;
                var y = this.Dot.Y;
                float offsetX = Math.Abs(this.Radios * (float)Math.Cos(rad));//30
                if (cAngle > 0 && cAngle < 90)
                {
                    x += offsetX;
                    y += offsetX * kc;
                }
                else if (cAngle > 90 && cAngle < 180)
                {
                    x -= offsetX;
                    y -= offsetX * kc;
                }
                else if (cAngle > 180 && cAngle < 270)
                {//k>0
                    x -= offsetX;
                    y -= offsetX * kc;
                }
                else if (cAngle > 270 && cAngle < 360)
                {
                    x += offsetX;
                    y += offsetX * kc;
                }
                TextLoacation = new PointF(x, y);
                // g.DrawRectangle(Pens.Blue, this.Area);
                this.Angle = (float)angle;
                this.FromAngle = (float)f;
                this.IsDirty = false;
            }
            private PointF TextLoacation { get; set; } = PointF.Empty;
            public void Invalid(Graphics g)
            {
                if (IsDirty)
                    Refresh();
                SolidBrush b = new SolidBrush(Color.FromArgb(130, this.Id % 2 == 0 ? Color.Gray : Color.BlueViolet));
                g.FillPie(b, this.Area, this.FromAngle,this.Angle);//Brushes.Aqua
                g.DrawString(this.Angle.ToString("N0") + "°", Font, Brushes.Green, TextLoacation.X, TextLoacation.Y - TextSize.Height / 2);// x - offsetX / 2, y - offsetX / 2);
                //g.DrawString(this.Text, Font, Brushes.Green, x, y);
            }

            public bool IsDirty
            {
                get;
                set;
            } = true;

            private float _angle = 0;
            public float FromAngle
            {
                get;
                private set;
            }
            public float Angle
            {
                get
                {
                    return _angle;
                }
                private set
                {
                    _angle = value;
                }
            }
            public bool IsDraged { get; set; } = false;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                this.IsDraged = true;
            }

            private bool _isCapture = false;
            public bool IsCapture {
                get { return this._isCapture; }
                set
                {
                    if(this._isCapture!=value)
                    {
                        this._isCapture = value;
                       // this.CanvasCtl.Invalidate();
                    }
                }
            } 
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
               // this.CanvasCtl.Invalidate();
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                this.IsDraged = false;
            }
            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                if (!this.IsActive)
                {
                    this.IsCapture = true;
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }
            const double RadUnit = 360 / (2 * Math.PI);
            public bool IsActive { get; set; } = false;

            public int Width { get; set; } = 1;
            public Color Color { get; set; } = Color.Black;
        }

        public class TextArea : IShape
        {
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsDirty
            {
                get;
                set;
            } = false;
            public Point Location
            {
                get { return new Point((int)X,(int) Y); }
            }
            public void Delete()
            {

            }
            
            public ShapeType ShapeType { get { return ShapeType.Text; } }
            public void Move(float x, float y)
            {
                X += x;
                Y += y;
            }
            public void MoveTo(float x, float y)
            {
                X = x;
                Y = y;
            }
            private Rectangle Area
            {
                get
                {
                    var s = TextRenderer.MeasureText(this.Text, Font);
                    return new Rectangle((int)this.X, (int)this.Y, s.Width + 4, s.Height);
                }
            }
            public bool PtIn(Point p)
            {
                return this.Area.Contains(p);
            }
            private Font Font = new Font("宋体", 12f);
            public string Text { get; set; } = "A";
            public void Invalid(Graphics g)
            {
                g.DrawString(Text, Font, Brushes.Black, this.X, this.Y);
            }
            public bool IsDraged { get; set; } = false;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                this.IsDraged = true;
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var a = Area;
                    this.MoveTo(e.X - a.Width / 2, e.Y - a.Height / 2);
                }
                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
                this.CanvasCtl.Invalidate();
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                this.IsDraged = false;
            }
            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                if (!this.IsActive)
                {
                    this.IsCapture = true;
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

            public bool IsActive { get; set; } = false;

            public int Width { get; set; } = 1;
            public Color Color { get; set; } = Color.Black;
        }
      
        public class Dot : IShape
        {
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public string Text { get; set; }

            public void Delete()
            {
                DoUnit unit = new DoUnit();
                if (this.Lines.Count == 1)
                {
                    unit.Units.Push(new KeyValuePair<IShape, object>(this.Lines[0], true));
                    
                    this.Lines[0].Delete();
                    this.Lines.Clear();
                }

                foreach (var remAg in RemAngles)
                {
                    unit.Units.Push(new KeyValuePair<IShape, object>(remAg, true));
                    this.Canvas.RemoveShape(remAg);
                }
                foreach (var sh in this.Canvas.Shapes.Where(s => s.ShapeType == ShapeType.FillArea))
                {
                    FillArea fillArea = sh as FillArea;
                    unit.Units.Push(new KeyValuePair<IShape, object>(sh, true));
                    fillArea.Dots.Remove(this);
                }
                foreach (var sh in Circles)
                {
                    sh.Delete();
                }

                this.Circles.Clear();
                this.RemAngles.Clear();
                if (unit.Units.Count > 0)
                    this.Canvas.RedoAllObjects.Push(unit);
            }

            private bool _isDirty = false;
            public bool IsDirty
            {
                get { return _isDirty; }
                set
                {
                    if (this._isDirty != value)
                    {
                        this._isDirty = value;
                        if (value)
                        {
                            foreach (var sh in this.RemAngles)
                                sh.IsDirty = true;
                            foreach (var sh in this.Lines)
                                sh.IsDirty = true;

                            foreach(var sh in this.Canvas.Shapes.Where(s=>s.IsDirty==false && s.ShapeType== ShapeType.FillArea))
                            {
                                var fillArea = sh as FillArea;
                                if (fillArea.Dots.Contains(this))
                                    fillArea.IsDirty = true;
                            }
                            foreach (var sh in Circles)
                                sh.IsDirty = true;
                        }
                    }
                }
            }
          
            public Point Location
            {
                get { return new Point((int)X, (int)Y); }
            }
            public PointF Pf { get { return new PointF(X, Y); } }
            public Point P { get { return new Point((int)X, (int)Y); } }
            public List<Line> Lines { get; set; } = new List<Line>();
            public List<RemAngle> RemAngles { get; set; } = new List<RemAngle>();

            public List<Circle> Circles { get; set; } = new List<Circle>();
            public ShapeType ShapeType { get { return ShapeType.Dot; } }
            public bool PtIn(Point p)
            {
                var rect=this.Area;
                rect.Inflate(3, 3);
                return rect.Contains(p);
            }
            private Rectangle Area
            {
                get
                {
                    return new Rectangle((int)this.X - 3, (int)this.Y - 3, 6, 6);
                }
            }
            public static byte FromChar=0;
            public Dot()
            {
                var c =(char)( ('A') + FromChar++);
                this.Text = c.ToString();
            }
            public void Move(float x, float y)
            {              
                X += x;
                Y += y;
                this.IsDirty = true;
            }
            public void MoveTo(float x, float y)
            {
                X = x;
                Y = y;
                this.IsDirty = true;
            }
            private static Font Font = new Font("微软雅黑", 14f, FontStyle.Bold);
            const double RadUnit = 360 / (2 * Math.PI);
            public void Invalid(Graphics g)
            {
                if(this.posCaptureRect!=Rectangle.Empty)
                {
                    DrawPosCapture(g);
                }
                else  if (this.IsCapture)
                {
                    Rectangle rec = this.Area;
                 
                    rec.Inflate(-2, -2);
                    g.FillRectangle(Brushes.Black, rec);
                    rec.Inflate(4, 4);
                    g.DrawEllipse(Pens.Green, rec);
                }
                else if(this.IsActive)
                {
                    g.FillEllipse(Brushes.Red, this.Area);
                }
                else
                {
                    g.FillEllipse(Brushes.Black, this.Area);
                }

                #region bak
                //if(this.Lines.Count>1)
                //{
                //    float k1 = this.Lines[0].K;
                //    float k2 = this.Lines[1].K;
                //    double angle = 0;
                //    var toPoint1 = this.Lines[0].From != this ? this.Lines[0].From.Pf : this.Lines[0].To.Pf;//另外一段
                //    var toPoint2 = this.Lines[1].From != this ? this.Lines[1].From.Pf : this.Lines[1].To.Pf;//另外一段

                //    toPoint1 = new PointF  ( toPoint1.X-this.Pf.X, toPoint1.Y - this.Pf.Y);

                //    toPoint2 = new PointF(toPoint2.X - this.Pf.X, toPoint2.Y - this.Pf.Y);

                //    //测试阶段假设从k1->k2夹角

                //    //if(k1>k2)
                //    //{
                //    //    float k = k1;
                //    //    k1 = k2;
                //    //    k2 = k;
                //    //}
                //    //if (k1 * k2 == -1)
                //    //{//90
                //    //    angle = 90;// "90°";
                //    //}
                //    //else if((k1==0&& k2==float.MaxValue)||(k2==0&&k1==float.MaxValue))
                //    //{ //90
                //    //    angle = 90;// "90°";
                //    //}
                //    //else
                //    //{
                //    //    angle =  Math.Atan((k2 - k1) / (1 + k1 * k2))* HuDu;
                //    //   // angle = r;// r.ToString("N1") + "°";
                //    //}

                //    var f = (Math.Atan(k1) * RadUnit);
                //    var t = (Math.Atan(k2) * RadUnit);
                //    if(toPoint1.X>0&&toPoint1.Y>0)
                //    {

                //    }
                //    else if (toPoint1.X < 0 && toPoint1.Y > 0)
                //    {
                //        f += 180;
                //    }
                //    else if (toPoint1.X < 0 && toPoint1.Y < 0)
                //    {
                //        f += 180;
                //    }
                //    else if (toPoint1.X > 0 && toPoint1.Y < 0)
                //    {
                //        f += 360;
                //    }

                //    if (toPoint2.X > 0 && toPoint2.Y > 0)
                //    {

                //    }
                //    else if (toPoint2.X < 0 && toPoint2.Y > 0)
                //    {
                //        t += 180;
                //    }
                //    else if (toPoint2.X < 0 && toPoint2.Y < 0)
                //    {
                //        t += 180;
                //    }
                //    else if (toPoint2.X > 0 && toPoint2.Y < 0)
                //    {
                //        t += 360;
                //    }
                //    if (f > t)
                //        angle = 360 + t - f;
                //    else
                //        angle = t - f;
                //    var area = new Rectangle((int)this.X - 30, (int)this.Y - 30, 60, 60);
                //    var cAngle = f + angle / 2;
                //    float raido = (float)((float) Math.PI * cAngle / 180);
                //    var kc = (float)Math.Tan(raido);
                //    var x = this.X;
                //    var y = this.Y;
                //    float offsetX =Math.Abs( 30*(float)Math.Cos(raido));
                //    if (cAngle > 0&& cAngle < 90)
                //    {
                //        x += offsetX;
                //        y += offsetX * kc;
                //    }
                //    else  if (cAngle > 90 && cAngle < 180)
                //    {
                //        x -= offsetX;
                //        y -= offsetX * kc;
                //    }
                //    else if (cAngle > 180 && cAngle < 270)
                //    {//k>0
                //        x -= offsetX;
                //        y -= offsetX * kc;
                //    }
                //    else if (cAngle > 270 && cAngle < 360)
                //    {
                //        x += offsetX;
                //        y += offsetX * kc;
                //    }
                //    g.FillPie(Brushes.Aqua, area, (float)f,(float) angle);
                //    g.DrawString(angle.ToString("N0") + "°", Font, Brushes.Green, x- offsetX / 2, y- offsetX / 2);// area);// this.X - 30, this.Y - 30);
                //}
                #endregion

                g.DrawString(this.Text, Font, Brushes.Green, this.X, this.Y);
                this.IsDirty = false;
            }

            //public void Invalid()
            //{
            //    //   var s = new SolidBrush(Color.Red);
            //    using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
            //    {
            //       // g.FillPie(Brushes.Black, this.Area, 0, 360);
            //        g.FillRectangle(Brushes.Black, this.Area);
            //        Rectangle rec = this.Area;
            //        rec.Inflate(2, 2);
            //        if (this.IsActive)
            //        {
            //            g.DrawRectangle(Pens.Red, rec);
            //        }
            //        else if (this.IsCapture)
            //        {
            //            g.DrawRectangle(Pens.Green, rec);
            //        }
            //        else
            //            g.DrawRectangle(Pens.White, rec);
            //    }
            //}

            private bool _IsDraged = false;
            public bool IsDraged
            {
                get
                {
                    return _IsDraged;
                }
                set
                {
                    if (this._IsDraged != value)
                    {
                        this._IsDraged = value;
                    }
                }
            }
            Rectangle posCaptureRect = Rectangle.Empty;
            private void DrawPosCapture(Graphics g)
            {
                using(Pen p=new Pen(Color.Red,2))
                    g.DrawEllipse(p, this.posCaptureRect);
            }
            private Point PrePos = Point.Empty;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                Console.WriteLine(this.Id + " dot mouse down");
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = !this.IsActive;
                    var f = this.Canvas.Shapes.FirstOrDefault(t => t != this && t.ShapeType == ShapeType.Dot && t.PtIn(e.Location));
                    if (f != null)
                    {//有来源点                    
                        Console.WriteLine("this Id:" + this.Id + "  ,moved id:" + f.Id + ", xy:" + e.Location);
                        this.Canvas.Shapes.Remove(f);
                        var srcDot = f as Dot;
                        foreach (var l in srcDot.Lines)
                        {
                            if (l.From == srcDot)//捕获点是线段的源
                                l.From = this;
                            else
                                l.To = this;
                        }
                        posCaptureRect = Rectangle.Empty;
                    }
                    else
                    {
                        PrePos = e.Location;
                        this.IsDraged = true;
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {

                }
                this.CanvasCtl.Invalidate();
            }
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    this.MoveTo(e.X, e.Y);
                    var t = this.Canvas.Shapes.FirstOrDefault(dt => dt != this && dt.ShapeType == ShapeType.Dot && dt.PtIn(e.Location)) as Dot;
                    if (t != null)
                    {
                        Rectangle rec = this.Area;
                        rec.Inflate(6, 6);
                        posCaptureRect = rec;
                        // this.DrawCapture();
                    }
                    else
                        posCaptureRect = Rectangle.Empty;
                }

                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
                this.CanvasCtl.Invalidate();
            }
            
            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (this.IsDraged)
                {
                    if (PrePos.X != e.X || PrePos.Y != e.Y)
                    {
                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(this, PrePos));
                        Canvas.RevokeAllObjects.Push(unit);
                    }
                   
                    this.IsDraged = false;
                }
                Console.WriteLine(this.Id + " dot mouse up");
            }
            private bool _isCapture = false;
            public bool IsCapture
            {
                get { return this._isCapture; }
                private set { this._isCapture = value; }
            }

            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
                this.CanvasCtl.Invalidate();
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                if (!this.IsActive)
                {
                    this.IsCapture = true;
                    // Invalid();
                    this.CanvasCtl.Invalidate();
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

            public bool IsActive { get; set; } = false;

            public int Width { get; set; } = 1;
            public Color Color { get; set; } = Color.Black;
        }
       
        public class FillArea : IShape
        {
            private PointF[] Points;

            private List<Dot> _dots;
            public List<Dot> Dots { 
                get
                {
                    return _dots;
                }
                set
                {
                    if(this._dots!=value)
                    {
                        this._dots = value;
                        RefreshData(true);
                    }
                }
            }

            private List<Line> _lines { get; set; }
            public List<Line> Lines
            {
                get
                {
                    return _lines;
                }
            }
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }
            
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }

            public ShapeType ShapeType { get { return ShapeType.FillArea; } }
        
            public bool PtIn(Point p)
            {
                return MyMath.DotInArea(this.Lines,p);
            }
        
            public string Text { get; set; } = "A";

            private void RefreshData(bool order)
            {
                List<PointF> points = new List<PointF>();
                foreach (var d in Dots)
                    points.Add(d.Pf);
                if (order)
                {
                    this.Points = MyMath.ClockwisePoints(points.ToArray());
                    List<Line> lines = new List<Line>();
                    foreach (var d in Dots)
                    {//找到封闭区域的线段。
                        foreach (var l in d.Lines)
                            if (Dots.Contains(l.From) && Dots.Contains(l.To))
                                lines.Add(l);
                    }
                    this._lines= lines.Distinct().ToList();
                }
                else
                    this.Points = points.ToArray();

                IsDirty = false;
            }
            public void Invalid(Graphics g)
            {
                SolidBrush sb = new SolidBrush(Color.FromArgb(100, this.Color));
                if (IsDirty)
                    RefreshData(false);
                g.FillPolygon(sb, Points);
            }
            public bool IsDraged { get; set; } = false;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                this.IsDraged = true;
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
              
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                this.IsDraged = false;
            }
            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                if (!this.IsActive)
                {
                    this.IsCapture = true;
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

            public bool IsActive { get; set; } = false;

            public int Width { get; set; } = 1;
            public Color Color { get; set; } = Color.Red;
        }
        public class Line : IShape
        {
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            static int SelSq = 0;
            public int selIndex { get; set; }

            public string Text { get; set; }
            private Dot _from;
            public Dot From
            {
                get
                {
                    return _from;
                }
                set
                {
                    _from = value;
                    if (!_from.Lines.Contains(this))
                        _from.Lines.Add(this);
                }
            }

            public float K
            {
                get
                {
                    if (this.From.X == this.To.X)
                        return float.MaxValue;
                    return (this.From.Y - this.To.Y) / (this.From.X - this.To.X);
                }
            }
            public float B
            {
                get
                {
                    if (K == float.MaxValue)
                        return this.From.X;
                    return this.From.Y - this.K * this.From.X;
                }
            }
            private bool _isDirty = true;
            public bool IsDirty
            {
                get { return this._isDirty; }
                set {
                    if (this._isDirty != value)
                    {
                        _isDirty = value;
                        if(value)
                        {
                          var res=  Canvas.Shapes.Where(s=>
                            {
                                if(s.ShapeType== ShapeType.RemAngle)
                                {
                                    var remAng = s as RemAngle;
                                    if (remAng.FromLine == this)
                                        return true;
                                    if (remAng.ToLine == this)
                                        return true;
                                }
                                return false;
                            });
                            foreach(var sh in res)
                            {
                                sh.IsDirty = true;
                            }
                        }
                    }
                }
            }

            public Point Location
            {
                get {

                    int x = (int)(this.From.X + this.To.X);
                    int y = (int)(this.To.Y + this.From.Y);

                    var mx = x / 2;
                    var my = y / 2;

                    return   new Point(mx, my);
                }
            }
            private Dot _to;
            public Dot To
            {
                get
                {
                    return _to;
                }
                set
                {
                    _to = value;
                    if (!_to.Lines.Contains(this))
                        _to.Lines.Add(this);
                }
            }
            public int Id { get; set; }
            public void Delete()
            {
                var f = this.From.Lines.FirstOrDefault(l => l != this);
                var t = this.To.Lines.FirstOrDefault(l => l != this);
                if (f == null)
                {
                    this.Canvas.RemoveShape(this.From);
                  
                }
                if (t == null)
                {
                    this.Canvas.RemoveShape(this.To);
                  
                }
                this.From.Lines.Remove(this);
                this.To.Lines.Remove(this);
                this.Canvas.RemoveShape(this);

                var remAngs= this.Canvas.Shapes.Where(sh =>
                {
                    if (sh.ShapeType == ShapeType.RemAngle)
                    {
                        RemAngle ra = sh as RemAngle;
                        if (ra.FromLine == this || ra.ToLine == this)
                        {
                            return true;
                        }
                    }
                    return false;
                   
                }).ToList();

                foreach(var sh in remAngs)
                    this.Canvas.RemoveShape(sh);
            }

            public int Width { get; set; } = 2;
            public Color Color { get; set; } = Color.DarkBlue;
            public ShapeType ShapeType { get { return ShapeType.Line; } }
            public Line()
            {

            }
            public void Move(int x, int y)
            {
                this.From.Move(x, y);
                this.To.Move(x, y);
            }
            public void Move(float x, float y)
            {
                this.From.Move(x, y);
                this.To.Move(x, y);
            }

            public bool GetPointIsInLine(PointF pf, PointF p1, PointF p2, double range)
            {
                //range 判断的的误差，不需要误差则赋值0
                //点在线段首尾两端之外则return false
                // var p11 = new PointF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                //var p22 = new PointF(Math.Max(p1.X, p2.X), Math.Min(p1.Y, p2.Y));

                double cross = (p2.X - p1.X) * (pf.X - p1.X) + (p2.Y - p1.Y) * (pf.Y - p1.Y);
                if (cross <= 0)
                    return false;
                double d2 = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
                if (cross >= d2)
                    return false;

                double r = cross / d2;
                double px = p1.X + (p2.X - p1.X) * r;
                double py = p1.Y + (p2.Y - p1.Y) * r;

                //判断距离是否小于误差
                return Math.Sqrt((pf.X - px) * (pf.X - px) + (py - pf.Y) * (py - pf.Y)) <= range;
            }
            public bool PtIn(Point P)
            {
                if (this.From.PtIn(P) || this.To.PtIn(P))
                    return false;

                return GetPointIsInLine(P, this.From.Pf, this.To.Pf, 4);
            }

            private bool _isCapture = false;
            public bool IsCapture
            {
                get { return this._isCapture; }
                set
                {
                    if (this._isCapture != value)
                    {
                        this._isCapture = value;
                        using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            this.Invalid(g);
                        // this.CanvasCtl.Invalidate();
                    }
                }
            }

      
            private bool _IsDraged = false;
            public bool IsDraged
            {
                get
                {
                    return _IsDraged;
                }
                set
                {
                    this._IsDraged = value;
                }
            }
            Point movedFrom;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    movedFrom = e.Location;
                }

                this.IsDraged = true;
                Console.WriteLine(this.Id + " line mouse down");
                //var f = this.Canvas.Shapes.FirstOrDefault(t => t.ShapeType == ShapeType.Dot && t.PtIn(e.Location));
                //if (f != null && f.IsDraged)//线中插入点，该算法暂时不要删除。。。
                //{
                //    var md = new Dot() { X = e.X, Y = e.Y };
                //    Canvas.AddShape(md);
                //    Canvas.AddShape(new Line() { From = md, To = this.To });
                //    this.To = md;//插入了点。

                //    var dotTo = new Dot() { X = e.Location.X, Y = e.Location.Y };
                //    Canvas.AddShape(dotTo);
                //    Canvas.AddShape(new Line() { From = md, To = dotTo });
                //    dotTo.IsDraged = true;
                //}

                this.CanvasCtl.Invalidate();
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                Console.WriteLine(this.Id + " line mouse up");

                this.IsActive = !this.IsActive;
                if (this.IsActive)
                {
                   // this.selIndex = SelSq++;
                }
                else
                {
                }
                this.IsDraged = false;
            }

            private Color CaptureColor { get; set; } = Color.Green;
            private Color ActiveColor { get; set; } = Color.Red;
            static Font font = new Font("微软雅黑", 10f);
            public void Invalid(Graphics g)
            {
                if (this.IsActive)
                {//线段被选中
                    int x = (int)(this.From.X + this.To.X);
                    int y = (int)(this.To.Y + this.From.Y);

                    var mx = x / 2;
                    var my = y / 2;
                    var p = Control.MousePosition;
                    p = this.CanvasCtl.PointToClient(p);
                    bool midNear = (p.X > mx - 8 && p.X < mx + 8 && p.Y > my - 8 && p.Y < my + 8);
                    using (var pen = new Pen(ActiveColor, this.Width))
                    {
                        g.DrawLine(pen, this.From.Pf, this.To.Pf);
                        if (midNear)
                        {
                            g.DrawArc(Pens.Red, new Rectangle(mx - 5, my - 5, 10, 10), 0, 360);
                            g.DrawString("中点", font,Brushes.Red,mx,my);
                        }
                    }
                }
                else if (this.IsCapture)
                {
                    int x = (int)(this.From.X + this.To.X);
                    int y = (int)(this.To.Y + this.From.Y);

                    var mx = x / 2;
                    var my = y / 2;
                    var p = Control.MousePosition;
                    p = this.CanvasCtl.PointToClient(p);
                    bool midNear = (p.X > mx - 8 && p.X < mx + 8 && p.Y > my - 8 && p.Y < my + 8);
                    using (var pen = new Pen(this.CaptureColor, this.Width))
                    {
                        g.DrawLine(pen, this.From.Pf, this.To.Pf);
                        if (midNear)
                        {
                            g.DrawArc(Pens.Red, new Rectangle(mx - 6, my - 6, 12, 12), 0, 360);
                            g.DrawString("中点", font, Brushes.Red, mx, my);
                        }
                    }
                }
                else
                {
                    using (var p = new Pen(this.Color, this.Width))
                        g.DrawLine(p, this.From.Pf, this.To.Pf);
                }
                this.IsDirty = false;
            }
          
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;
                    List<Dot> moveDots = new List<Dot>();
                    foreach(var sh in  this.Canvas.SelectedShapes.Where(t=>t.ShapeType== ShapeType.Line&&t!=this))
                    {
                        var line = sh as Line;
                        if (!moveDots.Contains(line.From))
                            moveDots.Add(line.From);
                        if (!moveDots.Contains(line.To))
                            moveDots.Add(line.To);
                      //  line.Move(x, y);
                    }

                    if (!moveDots.Contains(this.From))
                        moveDots.Add(this.From);
                    if (!moveDots.Contains(this.To))
                        moveDots.Add(this.To);

                    foreach (var dot in moveDots)
                    {
                        dot.Move(x, y);
                       // dot.X += x;
                       // dot.Y += y;
                    }
                    movedFrom = e.Location;
                }
                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
                this.CanvasCtl.Invalidate();
            }
            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
                this.CanvasCtl.Invalidate();
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                this.IsCapture = true;
                this.CanvasCtl.Invalidate();
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

            private bool _isActive = false;
            public bool IsActive {
                get { return this._isActive; }
                set{
                    if(_isActive != value)
                    {
                        if (value)
                        {
                            this.selIndex = SelSq++;
                            this.Canvas.SelectedShapes.Add(this);
                        }
                        else
                            this.Canvas.SelectedShapes.Remove(this);
                        _isActive = value;
                        this.From.IsActive = value;
                        this.To.IsActive = value;
                    }
                }
            } 
        }

        public class Circle : IShape
        {
          
            public CanvasHelper Canvas { get; set; }
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }

            private Dot _center;
            public Dot Center {
                get
                {
                    return this._center;
                }
                set
                {

                    if(this._center!=value)
                    {
                        if (this._center != null)
                            this._center.Circles.Remove(this);

                        this._center = value;
                        this._center.Circles.Add(this);
                    }
                }
            }
            public float Diam { get; set; } = 0;

       
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }

            public ShapeType ShapeType { get { return ShapeType.Circle; } }

            public bool PtIn(Point p)
            {
                return MyMath.PointInCircle(p, this.Diam / 2, this.Center.Location) || (p.X == Center.X && p.Y == Center.Y);
            }

            public string Text { get; set; } = "A";

            private void RefreshData(bool order)
            {
                IsDirty = false;
            }
            private Color ActiveColor { get; set; } = Color.Red;

            public void Invalid(Graphics g)
            {
                if (Diam <= 0)
                    return;

                if (IsActive)
                {
                    using (var pen = new Pen(ActiveColor, this.Width))
                    {
                        g.DrawArc(pen, this.Center.X - Diam / 2, this.Center.Y - Diam / 2, this.Diam, this.Diam , 0f, 360f);
                    }
                }
                else
                {
                    using (var pen = new Pen(this.Color, this.Width))
                    {
                        g.DrawArc(pen, this.Center.X - Diam / 2, this.Center.Y - Diam / 2, this.Diam, this.Diam  , 0f, 360f);
                    }
                }
            }
            public bool IsDraged { get; set; } = false;

          //  private Point movedFrom;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                  //  movedFrom = e.Location;
                    this.IsDraged = true;
                }
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if(this.IsDraged)
                {
                    // var x = e.X - movedFrom.X;
                    // var y = e.Y - movedFrom.Y;
                    this.Diam=(float) (Math.Sqrt(Math.Pow((float)e.X - Center.X, 2) + Math.Pow((float)e.Y - Center.Y, 2)))*2;
                    this.CanvasCtl.Invalidate();
                }
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                this.IsDraged = false;
            }
            public void MouseLeave(object sender, EventArgs e)
            {
                this.IsCapture = false;
            }

            public void MouseEnter(object sender, EventArgs e)
            {
                if (!this.IsActive)
                {
                    this.IsCapture = true;
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

            public bool IsActive { get; set; } = false;

            public int Width { get; set; } = 2;
            public Color Color { get; set; } = Color.DarkBlue;
        }
    }
}

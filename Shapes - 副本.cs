using ExpressionEvaluator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Riches.Visio
{
    namespace Geometry
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
            void MouseWheel(object sender, MouseEventArgs args);
        }

        public enum ShapeType { None, Dot, Line, Triangle, FillArea, Circle, Polygon,TextArea,RemAngle,ManualPen, Parabolic, UserPicture,Last }

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
            int SelIndex { get; set; }
            string SerializeObject();
            Color Color { get; set; }
            event Action<IShape, bool> ActivChanged;
            IShape Clone();
        }
        public class RemAngle : IShape
        {
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }

            public int SelIndex { get; set; } = 0;
            public Point Location
            {
                get { return this.Dot.Location; }
            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
                this.Dot.RemAngles.Remove(this);
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<RemAngle>(this.SerializeObject());
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

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
                    if (this._dot != value)
                    {
                        if (this._dot != null)
                            this._dot.RemAngles.Remove(this);
                        this._dot = value;
                        this._dot.RemAngles.Add(this);
                    }
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
                    return new Rectangle((int)this.Dot.X - Radios, (int)this.Dot.Y - Radios, Radios * 2, Radios * 2); 
                }
            }

           

            public int Index { get; set; }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"FromLine\":{\"Id\":" + this.FromLine.Id + "},");
                sbd.Append("\"ToLine\":{\"Id\":" + this.ToLine.Id + "},");
                sbd.Append("\"Dot\":{\"Id\":" + this.Dot.Id + "},");
                
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Radios\":" + this.Radios + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append(string.Format("\"Font\":\"{0},{1}pt,style={2}\"", this.Font.Name, this.Font.Size, this.Font.Style));
                sbd.Append("}");
                return sbd.ToString();
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
                var angle =MyMath.Get0_360FromByPos(p, this.Dot.Location);
                
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
            private  Font _font = new Font("微软雅黑", 11f, FontStyle.Regular);


            public Font Font
            {
                get { return this._font; }
                set
                {
                    if (this.Font != value)
                    {
                        this._font = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }

            private static Size TextSize;
            public string Text { get; set; } = "A";
        
            public RemAngle()
            {
              //  Font = new Font("微软雅黑", 12f, FontStyle.Bold);
                TextSize = TextRenderer.MeasureText("K", Font);
            }
          
            private void Refresh()
            {
                double angle = 0;
                // 出发线段的起点
                var toPoint1 = this.FromLine.From != this.Dot ? this.FromLine.From.Pf : this.FromLine.To.Pf;
                //旋转到目标线段结束点
                var toPoint2 = this.ToLine.From != this.Dot ? this.ToLine.From.Pf : this.ToLine.To.Pf;

                //把当前点当成中心点后的相对坐标。
                toPoint1 = new PointF(toPoint1.X - this.Dot.X, toPoint1.Y - this.Dot.Y);
                toPoint2 = new PointF(toPoint2.X - this.Dot.X, toPoint2.Y - this.Dot.Y);

                var k1 = this.FromLine.K;
                var k2 = this.ToLine.K;
                var f = (Math.Atan(k1) *MyMath.RadUnit);//出发点与X正半轴的交角
                var t = (Math.Atan(k2) * MyMath.RadUnit);//结束点与X正半轴的交角
                if (toPoint1.X >= 0 && toPoint1.Y >=0)
                {

                }
                else if (toPoint1.X < 0 && toPoint1.Y >= 0)
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

                if (toPoint2.X >= 0 && toPoint2.Y >= 0)
                {

                }
                else if (toPoint2.X < 0 && toPoint2.Y >= 0)
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

                var cAngle = (f + angle / 2) % 360;//平分角的度数
                float rad = (float)((float)Math.PI * cAngle / 180);//平分角的弧度数
                var kc = (float)Math.Tan(rad);
                var x = this.Dot.X;
                var y = this.Dot.Y;
                float offsetX = Math.Abs(this.Radios * (float)Math.Cos(rad));//偏移量
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
                if (e.Button == MouseButtons.Left)
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
          //  const double RadUnit = 360 / (2 * Math.PI);
            private bool _isActive = false;
            public event Action<IShape, bool> ActivChanged;
            public bool IsActive
            {
                get { return this._isActive; }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public int Width { get; set; } = 1;
           

            private Color _color = Color.Black;
            public Color Color
            {
                get { return this._color; }
                set
                {
                    if (this._color != value)
                    {
                        this._color = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
        }


        public class TextArea : IShape
        {
            public int SelIndex { get; set; } = 0;
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }

            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsDirty
            {
                get;
                set;
            } = false;
            public Point Location
            {
                get { return new Point((int)X,(int) Y); }
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<TextArea>(this.SerializeObject());
            }
            public int Index { get; set; }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"X\":" + this.X + ",");
                sbd.Append("\"Y\":" + this.Y + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append(string.Format("\"Font\":\"{0},{1}pt,style={2}\"", this.Font.Name, this.Font.Size, this.Font.Style));

                sbd.Append("}");
                return sbd.ToString();
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }
            public ShapeType ShapeType { get { return ShapeType.TextArea; } }
            public void Move(int x, int y)
            {
                X += x;
                Y += y;
            }
            public void MoveTo(int x, int y)
            {
                X = x;
                Y = y;
            }
            private Rectangle _area = Rectangle.Empty;

            public Rectangle Area
            {
                get
                {
                    return _area;
                }
            }
            public bool PtIn(Point p)
            {
                return this.Area.Contains(p);
            }
            private Font _font = new Font("微软雅黑", 13f);

            public Font Font
            {
                get { return this._font; }
                set
                {
                    if (this.Font != value)
                    {
                        this._font = value;
                        this.IsDirty = true;
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                           // using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                          //  this.Invalid(g);
                    }
                }
            }

            private string _text = string.Empty;

            public string Text
            {
                get
                {
                    return this._text;
                }
                set
                {
                    if (string.Compare(this._text , value)!=0)
                    {
                        this._text = value;
                        this.IsDirty = true;
                        if(this.CanvasCtl!=null)
                        {
                            this.CanvasCtl.Invalidate();
                          //  using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                         //       Invalid(g);
                        }
                    }
                }
            } 
            private void RefreshData()
            {
                var s = TextRenderer.MeasureText(this.Text, Font);
                this._area = new Rectangle((int)this.X, (int)this.Y, s.Width + 4, s.Height+4);
                this.IsDirty = false;
            }
            public void Invalid(Graphics g)
            {
                if (this.IsDirty)
                    RefreshData();

                if (this.IsActive)
                {
                    using (var p = new Pen(Color.Black))
                    {
                        p.DashStyle = DashStyle.Dash;
                        var rect=this.Area;
                        rect.Inflate(5, 15);
                        g.DrawRectangle(Pens.Black, rect);
                    }
                }
                using (var b = new SolidBrush (this.Color))
                    g.DrawString(Text, Font, b, this.Area);
            }

            public bool IsDraged { get; set; } = false;
           

            Point movedFrom, prePos;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = true;
                    movedFrom = e.Location;
                    prePos = movedFrom;
                    this.IsDraged = true;
                }
            }

            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;

                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                    movedFrom = e.Location;
                    this.IsDirty = true;
                    this.CanvasCtl.Invalidate();
                }
            }

            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.IsDraged)
                    {
                        if (prePos.X != e.X || prePos.Y != e.Y)
                        {
                            this.Canvas.BatchMoveRevok(prePos, e.X, e.Y);
                            prePos = e.Location;
                        }
                        this.IsDirty = true;
                    }

                    this.IsDraged = false;
                    this.CanvasCtl.Invalidate();
                }
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
            private bool _isActive = false;
            public event Action<IShape, bool> ActivChanged;
            public bool IsActive
            {
                get { return this._isActive; }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }
            public int Width { get; set; } = 1;
            private Color _color = Color.Black;
            public Color Color
            {
                get { return this._color; }
                set{
                    if (this._color != value)
                    {
                        this._color = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                                this.Invalid(g);
                    }
                }
            } 
        }

        public class Dot : IShape
        {
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public string Text { get; set; }
            public Line LockLine { get; set; }
            public float LockLineRation { get; set; }

            public Circle LockCirle { get; set; }
            public float LockCircleAngle { get; set; }

            private static Size TextSize;

            public int Index { get; set; } = 0;
            public Dot()
            {
                var c = (char)(('A') + FromChar++);
                this.Text = c.ToString();
                TextSize = TextRenderer.MeasureText("K", Font);
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<Dot>(this.SerializeObject());
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            public void Delete()
            {
                DoUnit unit = new DoUnit();
                
               if (this.Lines.Count == 1)
             //   foreach(var l in this.Lines)
                {
                    unit.Units.Push(new KeyValuePair<IShape, object>(this.Lines[0], true));
                    this.Lines[0].Delete();
                }

                foreach (var remAg in RemAngles)
                {
                    unit.Units.Push(new KeyValuePair<IShape, object>(remAg, true));
                    remAg.Delete();
                   // this.Canvas.RemoveShape(remAg);
                }
                foreach (var sh in this.Canvas.Shapes.Where(s => s.ShapeType == ShapeType.FillArea))
                {
                    FillArea fillArea = sh as FillArea;
                    unit.Units.Push(new KeyValuePair<IShape, object>(sh, true));
                    // fillArea.Dots.Remove(this);
                    fillArea.Delete(this);
                }
                foreach (var sh in Circles)
                {
                    sh.Delete();
                }

                var ps = this.Canvas.Shapes.Where(s =>
                {
                    if (s.ShapeType == ShapeType.Polygon)
                    {
                        Polygon ply = s as Polygon;
                        if (ply.Dots.Contains(this))
                            return true;
                    }
                    return false;
                }).ToList();

                foreach (Polygon p in ps)
                {
                    p.Dots.Remove(this);
                    p.AdjustLine();
                    p.IsDirty = true;
                }
                this.Circles.Clear();
                this.RemAngles.Clear();
                this.Lines.Clear();
                if (unit.Units.Count > 0)
                    this.Canvas.RedoAllObjects.Push(unit);

                this.Canvas.RemoveShape(this);
                // this.IsDirty = true;
            }

            private bool _isDirty = true;
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

                            foreach (var sh in this.Canvas.Shapes.Where(s => s.IsDirty == false && s.ShapeType == ShapeType.FillArea))
                            {
                                var fillArea = sh as FillArea;
                                if (fillArea.Dots.Contains(this))
                                    fillArea.IsDirty = true;
                            }
                            var ps = this.Canvas.Shapes.Where(s =>
                             {
                                 if (s.ShapeType == ShapeType.Polygon)
                                 {
                                     Polygon ply = s as Polygon;
                                     if (ply.Dots.Contains(this))
                                         return true;
                                 }
                                 return false;
                             });
                            foreach (var p in ps)
                                p.IsDirty = true;

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

            private int _size = 6;
            public int Size
            {
                get
                {
                    return _size;
                }
                set
                {
                    if(this._size!=value)
                    {
                        this._size = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
            private Rectangle Area
            {
                get
                {
                    return new Rectangle((int)this.X - Size/2, (int)this.Y - Size/2, this.Size, this.Size);
                }
            }
            public static byte FromChar = 0;

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
            public void MoveTo(PointF p)
            {
                X = p.X;
                Y = p.Y;
                this.IsDirty = true;
            }
            private  Font _font = new Font("微软雅黑", 11f, FontStyle.Bold);

            public Font Font
            {
                get { return this._font; }
                set {
                    if (this.Font != value)
                    {
                        this._font = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
            public void Invalid(Graphics g)
            {
                if (IsDirty)
                {
                    RefreshData();
                }
                if (this.posCaptureRect != Rectangle.Empty)
                {
                    DrawPosCapture(g);
                }
                else if (this.IsCapture)
                {
                    Rectangle rec = this.Area;
                    g.FillEllipse(Brushes.Black, rec);
                    rec.Inflate(4, 4);
                    using (var pen = new Pen(Color.Green, 2))
                        g.DrawEllipse(pen, rec);
                }
                else if (this.IsActive)
                {
                    g.FillEllipse(Brushes.Red, this.Area);
                }
                else
                {
                    g.FillEllipse(Brushes.Black, this.Area);
                }


                if (this.Canvas.IsDrawCoordinate)
                {
                    //  using (var brush = new SolidBrush(Color.FromArgb(100, Color.Green)))
                    g.DrawString(String.Format(this.Text + "(x={0:N2},y={1:N2})", (this.X - this.Canvas.Center.X) / this.Canvas.ScaleRation,
                       (this.Canvas.Center.Y - this.Y) / this.Canvas.ScaleRation), Font, Brushes.Green, TextPos);
                }
                else
                    g.DrawString(this.Text, Font, Brushes.Green, TextPos);
                this.IsDirty = false;
            }
            private void RefreshData()
            {
                if (this.Lines.Count >= 2)
                {
                    var angle = MyMath.Get2LineTextPosAngle(this, this.Lines[0], this.Lines[1]);
                    var cs = Math.Cos(angle);
                    var offX = cs < 0 ? -TextSize.Width : 0;
                    TextPos = new Point((int)(this.X + 10 * cs + offX),(int) (this.Y + 10 * Math.Sin(angle)) - TextSize.Height / 2);
                }
                else
                    TextPos = new Point((int)(this.X + 5), (int)(this.Y - TextSize.Height / 2));

                if (this.LockLine != null)
                {
                    var dis = this.LockLine.Dis;//线段长度
                    var angle =  MyMath.Get0_360FromByPos(this.LockLine.To.P, this.LockLine.From.P);
                    var sd = Math.PI * angle / 180;
                    var offsetx =  LockLineRation * dis *( Math.Cos(sd));
                    var x = this.LockLine.From.X + offsetx;
                    var offsety = LockLineRation * dis *(Math.Sin(sd));
                    var y = this.LockLine.From.Y + offsety;
                    this.MoveTo((float)x, (float)y);
                }
                else if (this.LockCirle != null)
                {
                    var x = this.LockCirle.Center.X + this.LockCirle.Diam / 2 * Math.Cos(this.LockCircleAngle);
                    var y = this.LockCirle.Center.Y + this.LockCirle.Diam / 2 * Math.Sin(this.LockCircleAngle);
                    this.MoveTo((float)x, (float)y);
                }
                this.IsDirty = false;
            }
            public Point TextPos
            {
                get;
                private set;
            }

            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id+",");
                sbd.Append("\"X\":" + this.X + ",");
                sbd.Append("\"Y\":" + this.Y + ",");
              //  sbd.Append("\"P\":{\"X\":" + this.X + ",\"Y\":" + this.Y + "},");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");

                sbd.Append("\"LockLine\":{\"Id\":" + (this.LockLine==null?-1: this.LockLine.Id) + "},");
                sbd.Append("\"LockCirle\":{\"Id\":" + (this.LockCirle==null?-1: this.LockCirle.Id) + "},");

                sbd.Append("\"LockLineRation\":" + this.LockLineRation + ",");
                sbd.Append("\"Size\":" + this.Size + ","); 
                sbd.Append("\"LockCircleAngle\":" + this.LockCircleAngle + ",");
                sbd.Append(string.Format("\"Font\":\"{0},{1}pt,style={2}\"",this.Font.Name,this.Font.Size,this.Font.Style));
             
                //  "宋体, 9pt, style=Bold"

                if (this.Circles.Count > 0)
                {
                    sbd.Append(",\"Circles\":[");
                    foreach (var c in this.Circles)
                    {
                        sbd.Append("{\"Id\":" + c.Id + "},");
                    }
                    sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }

                if (this.Lines.Count > 0)
                {
                    sbd.Append(",\"Lines\":[");
                    foreach (var l in this.Lines)
                    {
                        sbd.Append("{\"Id\":" + l.Id + "},");
                    }
                    sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }

                if (this.RemAngles.Count > 0)
                {
                    sbd.Append(",\"RemAngles\":[");
                    foreach (var r in this.RemAngles)
                    {
                        sbd.Append("{\"Id\":" + r.Id + "},");
                    }
                    if (this.RemAngles.Count > 0)
                        sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }
               
                sbd.Append("}");

                return sbd.ToString();
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
                    if (this._IsDraged != value)
                    {
                        this._IsDraged = value;
                    }
                }
            }
            Rectangle posCaptureRect = Rectangle.Empty;
            private void DrawPosCapture(Graphics g)
            {
                using (Pen p = new Pen(Color.Red, 2))
                    g.DrawEllipse(p, this.posCaptureRect);
            }
            private Point PrePos = Point.Empty;
            public int Width { get; set; } = 1;

            private Color _color = Color.Black;

            public Color Color
            {
                get
                {
                    return _color;
                }
                set
                {
                    if(_color!=value)
                    {
                        this._color = value;
                        if(this.CanvasCtl!=null)
                        using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            this.Invalid(g);
                        }
                    }
                }
            }
            private bool _isActive = false;
            public int SelIndex { get; set; } = 0;
            public event Action<IShape, bool> ActivChanged;
            public bool IsActive
            {
                get { return this._isActive; }
                set
                {
                    if (_isActive != value)
                    {
                       
                        if (value)
                        {
                            this.SelIndex = this.Canvas.GetSelUID;
                            this.Canvas.SelectedShapes.Add(this);
                        }
                        else
                            this.Canvas.SelectedShapes.Remove(this);
                        _isActive = value;

                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public void MouseDown(object sender, MouseEventArgs e)
            {
                Console.WriteLine(this.Id + " dot mouse down");
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = true;
                    PrePos = e.Location;
                    this.IsDraged = true;
                }
                else if (e.Button == MouseButtons.Right)
                {

                }
                this.CanvasCtl.Invalidate();
            }
            public bool PtIn(Point p)
            {
                var rect = this.Area;
                rect.Inflate(3, 3);
                return rect.Contains(p);
            }
            private void RecalLockRation()
            {
                var dx = LockLine.To.X - LockLine.From.X;
                var dy = LockLine.To.Y - LockLine.From.Y;
                if (dx == 0)
                {
                    var ey = this.Y - LockLine.From.Y;
                    this.LockLineRation = ey / dy;
                }
                else if (dy == 0)
                {
                    var ex = this.X - LockLine.From.X;
                    this.LockLineRation = ex / dx;
                }
                else
                {
                    var ex = this.X - LockLine.From.X;
                    this.LockLineRation = ex / dx;
                }
            }
           
            private bool AttchPending(IShape[] shs, Point location)
            {
                foreach (var d in shs)
                {
                    if (d.ShapeType == ShapeType.Dot)
                    {
                        var destDot = d as Dot;
                        this.MoveTo(destDot.Pf);
                        return true;
                    }
                    else if (d.ShapeType == ShapeType.Line)
                    {
                        var destLine = d as Line;
                        this.MoveTo(location.X, destLine.GetY_ByX(location.X));
                        return false;
                    }
                    else if (d.ShapeType == ShapeType.Circle)
                    {
                        var destCirle = d as Circle;
                        var y = destCirle.getY_ByX(location.X, location.Y);
                        this.MoveTo(location.X, Y);
                        return true;
                    }
                }
                return false;
            }
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    if (this.LockLine != null)
                    {//根据lock line移动路径。
                        var angle = MyMath.Get0_360FromByPos(this.LockLine.To.P, this.LockLine.From.P);
                        var sd = Math.PI * angle / 180;
                        //  Console.WriteLine("dx=" + (e.X - PrePos.X));
                        var dx = (e.X - PrePos.X);
                        var dy = (e.Y - PrePos.Y);
                        // Console.WriteLine("dx=" + dx + ",dy=" + dy);
                        if (Math.Abs(this.LockLine.K) < 0.71)//Math.Abs(dx)>=Math.Abs(dy
                        {
                            var nx = this.X + dx;// * Math.Abs(Math.Sin(sd)); ;
                            var ny = this.LockLine.K * (nx) + this.LockLine.B;
                            // Console.WriteLine("111 nx=" + nx + ",ny=" + ny);
                            this.MoveTo((float)nx, (float)ny);
                        }
                        else
                        {
                            var ny = this.Y + dy;// *Math.Abs(Math.Cos(sd));
                            var nx = (ny - this.LockLine.B) / this.LockLine.K;
                            // Console.WriteLine("222 nx=" + nx + ",ny=" + ny);
                            this.MoveTo((float)nx, (float)ny);
                        }
                        RecalLockRation();
                        PrePos = this.P;// e.Location;
                    }
                    else if (this.LockCirle != null)
                    {
                        var angle = MyMath.Get0_360FromByPos(e.Location, this.LockCirle.Center.P);
                        // Console.WriteLine("angle=" + angle);
                        LockCircleAngle = (float)(Math.PI * angle / 180f);
                        var x = this.LockCirle.Center.X + this.LockCirle.Diam / 2 * Math.Cos(this.LockCircleAngle);
                        var y = this.LockCirle.Center.Y + this.LockCirle.Diam / 2 * Math.Sin(this.LockCircleAngle);
                        this.MoveTo((float)x, (float)y);
                        PrePos = e.Location;
                    }
                    else
                    {
                        var shs = this.Canvas.Shapes.Where(dt => dt != this && dt.PtIn(e.Location)).ToArray();

                        if (AttchPending(shs, e.Location))
                        {//拖动到了重叠的点上。
                            Rectangle rec = this.Area;
                            rec.Inflate(6, 6);
                            posCaptureRect = rec;
                        }
                        else
                        {
                            posCaptureRect = Rectangle.Empty;
                            this.MoveTo(e.X, e.Y);
                        }
                    }
                }

                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
                this.CanvasCtl.Invalidate();
            }

            private void Attched(IShape[] shapes, Point location)
            {
                DoUnit unit = new DoUnit();
                foreach (var d in shapes)
                {
                    if (d.ShapeType == ShapeType.Dot)
                    {
                        var destDot = d as Dot;
                        // Console.WriteLine("this Id:" + this.Id + "  ,moved id:" + f.Id + ", xy:" + e.Location);
                        this.Canvas.Shapes.Remove(d);
                        unit.Units.Push(new KeyValuePair<IShape, object>(d, false));

                        if (this.LockCirle == null)
                        {
                            this.LockCirle = destDot.LockCirle;
                            this.LockCircleAngle = destDot.LockCircleAngle;
                        }

                        if (this.LockLine == null&& destDot.LockLine!=null)
                        {
                            this.LockLine = destDot.LockLine;
                            this.LockLineRation = destDot.LockLineRation;
                            unit.Units.Push(new KeyValuePair<IShape, object>(this, AttchEnum.LockLine));
                        }
                        if (destDot.Circles != null)
                        {
                            var cs=destDot.Circles.ToArray();
                            foreach (var c in cs)
                                c.Center = this;
                        }
                        
                        if (destDot.RemAngles != null)
                        {
                            var remangles = destDot.RemAngles.ToArray();
                            foreach (var c in remangles)
                                c.Dot = this;
                        }
                        foreach(var f in this.Canvas.Shapes.Where(t=>t.ShapeType== ShapeType.FillArea))
                        {
                            var fill = f as FillArea;
                            var i=fill.Dots.IndexOf(destDot);
                            if(i!=-1)
                            {
                                fill.Dots[i] = this;
                                fill.IsDirty = true;
                            }
                        }

                        foreach (var l in destDot.Lines)
                        {
                            if (l.From == destDot)//捕获点是线段的源
                            {
                                unit.Units.Push(new KeyValuePair<IShape, object>(l, l.From));
                                l.From = this;
                            }
                            else
                            {
                                unit.Units.Push(new KeyValuePair<IShape, object>(l, l.To));
                                l.To = this;
                            }
                        }
                        destDot.IsDirty = true;
                    }
                    else if (d.ShapeType == ShapeType.Line)
                    {
                        var destLine = d as Line;
                        if (destLine.IsInMidRang())
                        {
                            this.LockLineRation = 0.5f;
                        }
                        else
                        {
                            var dx = destLine.To.X - destLine.From.X;
                            var ex = location.X - destLine.From.X;
                            this.LockLineRation = ex / dx;
                        }
                        this.MoveTo(location.X, destLine.GetY_ByX(location.X));
                        this.LockLine = destLine;
                    }
                    else if (d.ShapeType == ShapeType.Circle)
                    {
                        var destCircle = d as Circle;
                        this.LockCirle = destCircle;
                        this.LockCircleAngle = (float)(Math.PI * MyMath.Get0_360FromByPos(location, destCircle.Center.P) / 180f);
                    }
                    posCaptureRect = Rectangle.Empty;
                }
                if (unit.Units.Count > 0)
                    Canvas.RevokeAllObjects.Push(unit);
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (this.IsDraged)
                {
                    if (PrePos.X != e.X || PrePos.Y != e.Y)
                    {
                        this.Canvas.BatchMoveRevok(PrePos, e.X, e.Y);
                        PrePos = e.Location;
                    }
                    ////非绘画线模式下，移动点到目的点,目的节点被替换掉
                    var shs = this.Canvas.Shapes.Where(t => t != this && t.PtIn(e.Location)).ToArray();
                    Attched(shs, e.Location);
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
                    this.CanvasCtl.Invalidate();
                }
            }
            public void MouseDoubleClick(object sender, MouseEventArgs e)
            {

            }

        }
       
        public class FillArea : IShape
        {
            private PointF[] Points;

            private List<Dot> _dots;
            public int SelIndex { get; set; } = 0;
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
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }

            public int Index { get; set; }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");

                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + "");
               
                if (this.Dots.Count > 0)
                {
                    sbd.Append(",\"Dots\":[");
                    foreach (var d in this.Dots)
                    {
                        sbd.Append("{\"Id\":" + d.Id + "},");
                    }
                    sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }

                sbd.Append("}");
                return sbd.ToString();
            }

            public void Delete(Dot dot)
            {
                this.Dots.Remove(dot);
                this.IsDirty = true;
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<FillArea>(this.SerializeObject());
            }
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
                if (order)
                {
                    this.Points = MyMath.ClockwisePoints(this.Dots);
                    List<Line> lines = new List<Line>();
                    foreach (var d in Dots)
                    {//找到封闭区域的线段。
                        foreach (var l in d.Lines)
                            if (Dots.Contains(l.From) && Dots.Contains(l.To))
                                lines.Add(l);
                    }
                    this._lines = lines.Distinct().ToList();
                }
                else
                {//点的位置变了，Point需要刷新。
                    List<PointF> points = new List<PointF>();
                    foreach (var d in Dots.OrderBy(d => d.Index))
                        points.Add(d.Pf);
                    this.Points = points.ToArray();
                }
                IsDirty = false;
            }
            public void Invalid(Graphics g)
            {
                SolidBrush sb = new SolidBrush(Color.FromArgb(100, this.Color));
                if (IsDirty)
                    RefreshData(false);

                if (Points.Length >= 3)
                    g.FillPolygon(sb, Points);
            }
            public bool IsDraged { get; set; } = false;
            Point movedFrom;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    movedFrom = e.Location;
                    this.IsDraged = true;
                }
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;

                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                    movedFrom = e.Location;
                    this.CanvasCtl.Invalidate();
                }
                else if (!this.IsCapture)
                {
                    this.IsCapture = true;
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
            private bool _isActive = false;
            public event Action<IShape, bool> ActivChanged;
            public bool IsActive
            {
                get { return this._isActive; }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public int Width { get; set; } = 1;
          //  public Color Color { get; set; } = Color.Red;
            private Color _color = Color.Orange;
            public Color Color
            {
                get { return this._color; }
                set
                {
                    if (this._color != value)
                    {
                        this._color = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
        }
        public class Line : IShape
        {
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }

            public int SelIndex { get; set; } = 0;

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
                    this.DrawFrom = value.P;
                    if (!_from.Lines.Contains(this))
                    {
                        _from.IsDirty = true;
                        _from.Lines.Add(this);
                    }
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
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<Line>(this.SerializeObject());
            }

            private bool _isDirty = true;
            public bool IsDirty
            {
                get { return this._isDirty; }
                set {
                    if (this._isDirty != value)
                    {
                        _isDirty = value;
                        if (value)
                        {
                            UserClickPoint = Point.Empty;
                            var res = Canvas.Shapes.Where(s =>
                              {
                                  if (s.ShapeType == ShapeType.RemAngle)
                                  {
                                      var remAng = s as RemAngle;
                                      if (remAng.FromLine == this)
                                          return true;
                                      if (remAng.ToLine == this)
                                          return true;
                                  }
                                  return false;
                              });
                            foreach (var sh in res)
                            {
                                sh.IsDirty = true;
                            }

                            var dots = Canvas.Shapes.Where(sh =>
                              {
                                  if (sh.ShapeType == ShapeType.Dot && !sh.IsDirty)
                                  {
                                      var dot = sh as Dot;
                                      return dot.LockLine == this;
                                  }
                                  return false;
                              });

                            foreach (var dot in dots)
                                dot.IsDirty = true;//会死循环吗？
                          
                        }
                    }
                }
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

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
                    this.DrawTo = value.P;
                    this.DrawEnd = value.P;
                    if (!_to.Lines.Contains(this))
                    {
                        _to.Lines.Add(this);
                        _to.IsDirty = true;
                    }
                }
            }
            public int Index { get; set; }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"From\":{\"Id\":"+this.From.Id+ "},");
                sbd.Append("\"To\":{\"Id\":" + this.To.Id + "},");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append("}");
                return sbd.ToString();
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
                this.Canvas.RemoveShape(this);
            }
            public float GetY_ByX(int x)
            {
                if (this.K == float.MaxValue)
                    return this.B;
                return this.K * x + this.B;
            }
            private int _width = 2;
            public int Width
            {
                get
                {
                    return _width;
                }
                set
                {

                    if (this._width != value)
                    {
                        bool add = (value > this._width);
                        this._width = value;
                        if (this._width <= 0)
                            this._width = 1;

                        //if (add)
                        //    using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                        //    {
                        //        this.Invalid(g);
                        //    }
                        //else
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                    }
                }
            }

            private Color _color= Color.DarkBlue;
            public Color Color
            {
                get
                {
                    return this._color;
                }
                set
                {
                    if (this._color != value)
                    {
                        this._color = value;
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                        //  using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                        //      this.Invalid(g);
                    }
                }
            }

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
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
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
            Point movedFrom, prePos;
            Point UserClickPoint = Point.Empty;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    movedFrom = e.Location;
                    this.UserClickPoint = e.Location;
                    this.IsActive = true;
                    this.IsDraged = true;
                    prePos = e.Location;
                    this.SortDots();
                    this.CanvasCtl.Invalidate();
                }
            }

            private void SortDots()
            {
                if (UserClickPoint.IsEmpty)
                    return;

                var res = this.Canvas.Shapes.Where(sh =>
                 {
                     if (sh.ShapeType == ShapeType.Dot)
                     {
                         var dot = sh as Dot;
                         if (dot.LockLine == this)
                             return true;
                     }
                     return false;
                 }).ToList();

                List<Point> dots = new List<Point>();
                foreach (Dot d in res)
                    dots.Add(d.P);

                foreach (Line line in this.Canvas.Shapes.Where(sh=>sh.ShapeType== ShapeType.Line&&sh.Id!=this.Id))
                {
                    var p=MyMath.Intersect(this, line);
                    if (p != Point.Empty)
                        dots.Add(p);
                }
               
                if (dots.Count > 0)
                {
                    dots.Add(this.From.P);
                    dots.Add(this.To.P);
                    dots.Add(UserClickPoint);

                    if (this.K > 1)
                        dots = dots.OrderBy(d => d.Y).ToList();
                    else
                        dots = dots.OrderBy(d => d.X).ToList();
                    int idx = dots.IndexOf(this.UserClickPoint);
                    if (idx == -1 || idx == 0 || idx == dots.Count - 1)
                    {
                        DrawFrom = this.From.P;
                        DrawTo = this.To.P;
                    }
                    else
                    {
                        DrawFrom = dots[idx - 1];
                        DrawTo = dots[idx + 1];
                    }
                    DrawEnd = dots[dots.Count - 1];
                }
                else 
                    this.UserClickPoint = Point.Empty;

            }
            private Point DrawFrom { get; set; }
            private Point DrawTo { get; set; }
            private Point DrawEnd { get; set; }
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;
                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                   // this.IsDirty = true;
                    movedFrom = e.Location;
                    this.CanvasCtl.Invalidate();
                }

                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                    this.CanvasCtl.Invalidate();
                }

            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.IsDraged)
                    {
                        if (prePos.X != e.X || prePos.Y != e.Y)
                        {
                            this.Canvas.BatchMoveRevok(prePos, e.X, e.Y);
                            prePos = e.Location;
                        }
                    }
                    this.IsDraged = false;
                }
            }

            private Color CaptureColor { get; set; } = Color.Green;
            private Color ActiveColor { get; set; } = Color.Red;
            static Font font = new Font("微软雅黑", 10f);

            public Point MidPoint {
                get
                {
                    return new Point((int)((this.From.X + this.To.X)/2), (int)((this.To.Y + this.From.Y)/2));
                }
            }
           
            public PointF TwoPoint
            {
                get
                {
                    return new PointF((this.From.X + this.To.X) , (this.To.Y + this.From.Y) );
                }
            }
            public float Dis
            {
                get
                {
                   
                    return (float)Math.Sqrt(Math.Pow(From.X - To.X, 2) + Math.Pow(From.Y - To.Y, 2));
           
                }
            }
            public bool IsShowMidHint()
            {
                var mp = MidPoint;
                var p = this.Canvas.GetMousePos();
                return (p.X > mp.X - 8 && p.X < mp.X + 8 && p.Y > mp.Y - 8 && p.Y < mp.Y + 8);
            }
            public bool IsInMidRang()
            {
                var mp = MidPoint;
                var p = this.Canvas.GetMousePos();
                return (p.X > mp.X - 5 && p.X < mp.X + 5 && p.Y > mp.Y - 5 && p.Y < mp.Y + 5);
            }
            static Pen PenSelected = new Pen(Color.DarkGreen, 4);

            private DashStyle _dashStyle = DashStyle.Solid;
            public DashStyle DashStyle
            {
                get
                {
                    return this._dashStyle;
                }
                set
                {
                    if(this._dashStyle!=value)
                    {
                        this._dashStyle = value;
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                    }
                }
            }
         
            private void DrawSelectStatus(Graphics g)
            {
                using (var pen = new Pen(Color.DarkGray, 1f))
                {
                 //   pen.DashStyle = DashStyle.Dash;
                    PointF p1 = Point.Empty, p2 = Point.Empty;
                    //p1->p2(小-大）
                    if (this.From.P.X < this.To.X)
                    {
                        p1.X = this.From.P.X;// + 3;
                        p2.X = this.To.X;// - 3;
                    }
                    else
                    {
                        p1.X = this.From.P.X;// - 3;
                        p2.X = this.To.X;// + 3;
                    }
                    if (this.From.P.Y < this.To.Y)
                    {
                        p1.Y = this.From.P.Y;// + 3;
                        p2.Y = this.To.Y;// - 3;
                    }
                    else
                    {
                        p1.Y = this.From.P.Y;// - 3;
                        p2.Y = this.To.Y;// + 3;
                    }

                    float k1 = 0f, k2 = 0f,  b1 = 0f,b2 = 0f;
                    if (this.K == float.MaxValue)
                    {
                        k1 = 0;
                        b1 = p1.Y;

                        k2 = 0;
                        b2 = p2.Y;
                    }
                    else if (this.K == 0)
                    {
                        k1 = float.MaxValue;
                        b1 = p1.X;

                        k2 = float.MaxValue;
                        b2 = p2.X;
                    }
                    else
                    {
                        k1 = -1 / K;
                        b1 = p1.Y - k1 * p1.X;

                        k2 = k1;
                        b2 = p2.Y - k2 * p2.X;
                    }
                    
                    double angle =this.K==float.MaxValue?0: Math.Atan(this.K);
                   
                    var bias =Math.Abs( (float)(this.Width / Math.Cos(angle)))+1;

                    KBLine kb1 = new KBLine() { K = k1, B = b1 };
                    KBLine kb2 = new KBLine() { K = k2, B = b2 };

                    KBLine kb3 = new KBLine() { K = this.K, B = this.B + bias };
                    KBLine kb4 = new KBLine() { K = this.K, B = this.B - bias };

                    var p3 = MyMath.Intersect(kb1, kb3);
                    var p4 = MyMath.Intersect(kb2, kb3);
                    g.DrawLine(pen, p3, p4);

                    p3 = MyMath.Intersect(kb1, kb4);
                    p4 = MyMath.Intersect(kb2, kb4);
                    g.DrawLine(pen, p3, p4);
                }
            }

            public void Invalid(Graphics g)
            {
                if (this.IsDirty)
                    Refresh();

                if (this.IsCapture)
                {
                    int x = (int)(this.From.X + this.To.X);
                    int y = (int)(this.To.Y + this.From.Y);

                    var mx = x / 2;
                    var my = y / 2;
                    var p = this.Canvas.GetMousePos();
                    bool midNear = (p.X > mx - 8 && p.X < mx + 8 && p.Y > my - 8 && p.Y < my + 8);
                    using (var pen = new Pen(this.CaptureColor, this.Width + 2))
                    {
                        pen.DashStyle = DashStyle;
                        g.DrawLine(pen, this.From.Pf, this.To.Pf);

                        if (IsInMidRang())
                        {
                            g.FillEllipse(Brushes.Red, new Rectangle((int)mx - 5, (int)my - 5, 10, 10));
                            using (var pp = new Pen(Color.Blue, 2))
                            {
                                g.DrawArc(pp, new Rectangle((int)mx - 6, (int)my - 6, 12, 12), 0, 360);
                            }
                        }
                        else if (IsShowMidHint())
                        {
                            g.DrawArc(Pens.Red, new Rectangle(mx - 6, my - 6, 12, 12), 0, 360);
                            g.DrawString("中点", font, Brushes.Red, mx, my);
                        }
                        DrawSelectStatus(g);
                    }
                }
                else  if (this.IsActive)
                {//线段被选中
                    PointF mp = MidPoint;
                    
                    using (var pen = new Pen(this.ActiveColor, this.Width))
                    {
                        pen.DashStyle = DashStyle;
                        if (this.UserClickPoint != Point.Empty)
                        {
                            using (var pen2 = new Pen(this.Color, this.Width))
                            {
                                pen2.DashStyle = DashStyle;
                                g.DrawLine(pen2, this.From.Pf, this.To.Pf);
                            }
                            g.DrawLine(pen, this.DrawFrom, this.DrawTo);
                        }
                        else
                            g.DrawLine(pen, this.From.Pf, this.To.Pf);

                        if (IsInMidRang())
                        {
                            g.FillEllipse(Brushes.Red, new Rectangle((int)mp.X - 5, (int)mp.Y - 5, 10, 10));
                            g.DrawArc(Pens.Blue, new Rectangle((int)mp.X - 6, (int)mp.Y - 6, 12, 12), 0, 360);
                        }
                        else if (IsShowMidHint())
                        {
                            g.DrawArc(Pens.Red, new Rectangle((int)mp.X - 5,(int) mp.Y - 5, 10, 10), 0, 360);
                            g.DrawString("中点", font, Brushes.Red, mp.X, mp.Y);
                        }
                    }
                    DrawSelectStatus(g);
                }
                else
                {
                    using (var pen = new Pen(this.Color, this.Width))
                    {
                        pen.DashStyle = DashStyle;
                        g.DrawLine(pen, this.From.Pf, this.To.Pf);
                    }
                }
                this.IsDirty = false;
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
            private void Refresh()
            {
                this.SortDots();
                this.IsDirty = false;
            }
            public event Action<IShape, bool> ActivChanged;
            private bool _isActive = false;
            public bool IsActive {
                get { return this._isActive; }
                set{
                    if(_isActive != value)
                    {
                      
                        if (value)
                        {
                            this.SelIndex = Canvas.GetSelUID;
                            this.Canvas.SelectedShapes.Add(this);
                        }
                        else
                            this.Canvas.SelectedShapes.Remove(this);
                        _isActive = value;
                      
                        if (!value)
                        {
                            this.DrawTo = this.To.P;
                            this.DrawFrom = this.From.P;
                            UserClickPoint = Point.Empty;
                        }
                        else
                            this.SortDots();

                        using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            this.Invalid(g);
                        }
                        ActivChanged?.Invoke(this, value);
                    }
                }
            } 
        }

        public class Circle : IShape
        {
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public int SelIndex { get; set; } = 0;
            private bool _isDrity = true;

            public event Action<IShape, bool> ActivChanged;

            public bool IsDirty
            {
                get
                {
                    return this._isDrity;
                }
                set
                {
                    if(this._isDrity!=value)
                    {
                        ActivChanged?.Invoke(this, value);
                        this._isDrity = value;
                        if(value)
                        {
                            var dots = Canvas.Shapes.Where(sh =>
                            {
                                if (sh.ShapeType == ShapeType.Dot)
                                {
                                    var dot = sh as Dot;
                                    return dot.LockCirle == this;
                                }
                                return false;
                            });

                            foreach (var dot in dots)
                                dot.IsDirty = true;//会死循环吗？
                        }
                    }
                }
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
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
            private float _dim = 0;
            public float Diam
            {
                get { return this._dim; }
                set {
                    if (_dim != value)
                    {
                        _dim = value;
                        this.IsDirty = true;
                    }
                }
            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }

            public ShapeType ShapeType { get { return ShapeType.Circle; } }

            public bool PtIn(Point p)
            {
                return MyMath.PointInCircle(p, this.Diam / 2, this.Center.Location);// || (p.X == Center.X && p.Y == Center.Y);
            }

            public float getY_ByX(float x, float y)
            {
                var tx = x - Center.X;
                y = y - Center.Y;
                var ty = Math.Sqrt(Math.Abs(this.Diam * this.Diam / 4 - tx * tx));
                if (Math.Abs(ty - y) > Math.Abs(ty + y))
                    ty = -ty;
                ty += Center.Y;
                return (float)ty;
            }
            public int Index { get; set; }
            public string Text { get; set; } = "A";

            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Center\":{\"Id\":"+this.Center.Id+"},");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append("\"Diam\":" + this.Diam + "");
                sbd.Append("}");
                return sbd.ToString();
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<Circle>(this.SerializeObject());
            }

            private void RefreshData(bool order)
            {
                IsDirty = false;
            }
            private Color ActiveColor { get; set; } = Color.Red;
            private Color CaptureColor { get; set; } = Color.Green;
            public void Invalid(Graphics g)
            {
                IsDirty = false;
                if (Diam <= 0)
                    return;

                if (IsActive)
                {
                    using (var pen = new Pen(ActiveColor, this.Width+2))
                    {
                        g.DrawArc(pen, this.Center.X - Diam / 2, this.Center.Y - Diam / 2, this.Diam, this.Diam , 0f, 360f);
                    }
                }
                else if(this.IsCapture)
                {
                    using (var pen = new Pen(CaptureColor, this.Width + 2))
                    {
                        g.DrawArc(pen, this.Center.X - Diam / 2, this.Center.Y - Diam / 2, this.Diam, this.Diam, 0f, 360f);
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

            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsDraged = true;
                }
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
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if(this.IsDraged)
                {
                    this.Diam=(float) (Math.Sqrt(Math.Pow((float)e.X - Center.X, 2) + Math.Pow((float)e.Y - Center.Y, 2)))*2;
                    this.CanvasCtl.Invalidate();
                }
                if (!this.IsCapture)
                {
                    this.IsCapture = true;
                }
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                this.IsDraged = false;
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
            public bool IsActive
            {
                get { return this._isActive; }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public int Width { get; set; } = 2;
            public Color Color { get; set; } = Color.DarkBlue;
        }

        public class Polygon : IShape
        {
            public List<Dot> Dots { get; set; } = new List<Dot>();
            private List<Line> _lines = new List<Line>();
            public int SelIndex { get; set; } = 0;
            public List<Line> Lines
            {
                get
                {
                    return _lines;
                }
                set
                {
                    _lines = value;
                }
            }

            public Point pendingPoint = Point.Empty;
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }
            public int Index { get; set; }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + "");
                
             
                if (this.Dots.Count > 0)
                {
                    sbd.Append(",\"Dots\":[");
                    foreach (var d in this.Dots)
                    {
                        sbd.Append("{\"Id\":" + d.Id + "},");
                    }

                        sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }
                if (this.Lines.Count > 0)
                {
                    sbd.Append(",\"Lines\":[");
                    foreach (var l in this.Lines)
                    {
                        sbd.Append("{\"Id\":" + l.Id + "},");
                    }

                        sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }

                sbd.Append("}");
                return sbd.ToString();
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<Polygon>(this.SerializeObject());
            }
            public void AddDot(Dot dot)
            {
                DoUnit unit = new DoUnit();
                unit.Units.Push(new KeyValuePair<IShape, object>(dot, true));

                this.Canvas.AddShape(dot);

                if(this.Dots.Count>=2)
                {
                    if (this.Dots.Count >= 3)
                    {
                        this.Canvas.RemoveShape(this.Lines[this.Lines.Count - 1]);
                        this.Lines.RemoveAt(this.Lines.Count - 1);
                    }

                    var line1 = this.Canvas.AddShape(new Line() { From = this.Dots[Dots.Count - 1], To = dot });
                    var line2 = this.Canvas.AddShape(new Line() { From = dot , To = this.Dots[0] });
                    this.Dots.Add(dot);
                    this.Lines.Add(line1 as Line);
                    this.Lines.Add(line2 as Line);

                    unit.Units.Push(new KeyValuePair<IShape, object>(line1, true));
                    unit.Units.Push(new KeyValuePair<IShape, object>(line2, true));
                }
                else if (this.Dots.Count > 0)
                {
                    var line = this.Canvas.AddShape(new Line() { From = this.Dots[Dots.Count - 1], To = dot });
                    this.Dots.Add(dot);
                    this.Lines.Add(line as Line);
                    unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                }
                else
                    this.Dots.Add(dot);
                this.Canvas.RevokeAllObjects.Push(unit);
                this.IsDirty = true;
            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }
            private Point[] Points = null;
            public ShapeType ShapeType { get { return ShapeType.Polygon; } }

            public bool PtIn(Point p)
            {
                return MyMath.DotInArea(this.Lines, p);
            }

            public string Text { get; set; } = "A";

            public void AdjustLine()
            {
                if (this.Dots.Count >= 3)
                {
                    var l = this.Lines[this.Lines.Count - 1];
                    var f = this.Dots[0];
                    var t = this.Dots[Dots.Count - 1];
                    if (l.From != f && l.To != t)
                    {
                        var line1 = new Line() { From = f, To = t };
                        this.Canvas.AddShape(line1);
                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(line1, true));
                        this.Canvas.RevokeAllObjects.Push(unit);
                    }
                }
            }
            private void RefreshData()
            {
                var g = from d in this.Dots select d.P;
                Points = g.ToArray();
               

                IsDirty = false;
            }
            public void Invalid(Graphics g)
            {
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(100, this.Color)))
                {
                    if (IsDirty)
                        RefreshData();
                    if (Points!=null&& Points.Length >= 3)
                        g.FillPolygon(sb, Points);
                }
            }
            public bool IsDraged { get; set; } = false;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
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
            private bool _isActive = false;
            public event Action<IShape, bool> ActivChanged;
            public bool IsActive {
                get { return this._isActive; }
                set { if (this._isActive != value)
                    {
                        this._isActive = value;
                        ActivChanged?.Invoke(this, value);
                    } 
                }
            }

            public int Width { get; set; } = 1;
            public Color Color { get; set; } = Color.Red;
        }


        public class ManualPen : IShape
        {
            public List<Point> Points { get; set; } = new List<Point>();
            public int SelIndex { get; set; } = 0;
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }


            public int Index { get; set; }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + "");

                if (this.Points.Count > 0)
                {
                    sbd.Append(",\"Points\":[");
                    foreach (var c in this.Points)
                    {
                        // sbd.Append("{\"X\":" + c.X + ",\"Y\":" + c.Y + "},");
                        sbd.Append("\"" + c.X + "," + c.Y + "\",");
                    }
                    sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }
                sbd.Append("}");
                return sbd.ToString();
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<ManualPen>(this.SerializeObject());
            }

            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            public ShapeType ShapeType { get { return ShapeType.ManualPen; } }

            public bool PtIn(Point p)
            {
                foreach (var p1 in this.Points)
                    if (Math.Abs(p1.X - p.X) <= 6 && Math.Abs(p1.Y - p.Y) <= 6)
                        return true;
                return false;
            }
            public bool IntersetRect(Rectangle rec)
            {
                foreach (var p in this.Points)
                    if (rec.Contains(p))
                        return true;
                return false;
            }
            public string Text { get; set; } = "A";
           
            private void RefreshData()
            {
                IsDirty = false;
            }
            public void Invalid(Graphics g)
            {
                if (Points.Count < 2)
                    return;
                if (IsActive)
                {
                    using (Pen pen = new Pen(Color.FromArgb(200, Color.Red), this.Width))
                    {
                        g.DrawLines(pen, Points.ToArray());
                    }
                }
                else
                {
                    using (Pen pen = new Pen(Color.FromArgb(200, this.Color), this.Width))
                    {
                        g.DrawLines(pen, Points.ToArray());
                    }
                }
            }
            public bool IsDraged { get; set; } = false;
            Point movedFrom, prePos;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = true;
                    movedFrom = e.Location;
                    prePos = movedFrom;
                    this.IsDraged = true;
                }
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;

                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                    movedFrom = e.Location;
                    this.CanvasCtl.Invalidate();
                }
            }
            public void Move(int x,int y)
            {
                List<Point> tmpPoints = new List<Point>();
                for(int i=0;i<this.Points.Count;i++)
                {
                    var p = this.Points[i];
                    p.X+= x;
                    p.Y+= y;
                    tmpPoints.Add(p);
                }
                this.Points = tmpPoints;
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.IsDraged)
                    {
                        if (prePos.X != e.X || prePos.Y != e.Y)
                        {
                            this.Canvas.BatchMoveRevok(prePos, e.X, e.Y);
                            prePos = e.Location;
                        }
                    }
                    this.IsDraged = false;
                    this.CanvasCtl.Invalidate();
                }
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
            public event Action<IShape, bool> ActivChanged;
            private bool _isActive = false;
            public bool IsActive
            {
                get
                {
                    return this._isActive;
                }
                set
                {
                    if(this._isActive!=value)
                    {
                        this._isActive = value;
                        if(this.CanvasCtl!=null)
                        using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                        {
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            this.Invalid(g);
                        }
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public int Width { get; set; } = 2;
            public Color Color { get; set; } = Color.Black;
        }

        public class Parabolic : IShape
        {
            public List<List<Point>> Points { get; set; } = new List<List<Point>>();
            public int SelIndex { get; set; } = 0;
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = false;
            public Point Location { get; set; }
            public int Index { get; set; }
         
            public void ParseExpress(string express)
            {

            }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append("\"Express\":\"" + this.Express + "\"");

                if (this.Points.Count > 0)
                {// [["1, 1","2, 2"],["1, 1","2, 2"]]
                    sbd.Append(",\"Points\":[");
                    foreach (var pts in this.Points)
                    {
                        sbd.Append("[");
                        //  foreach (var c in pts)
                        for (int i = 0; i < pts.Count; i++)
                        {
                            if (i != 0)
                                sbd.Append(",");
                           sbd.Append("\"" + pts[i].X + "," + pts[i].Y + "\"");
                        }
                        sbd.Append("],");
                    }
                    sbd.Remove(sbd.Length - 1, 1);
                    sbd.Append("]");
                }
                sbd.Append("}");
                return sbd.ToString();
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {

            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<Parabolic>(this.SerializeObject());
            }
            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }

            public ShapeType ShapeType { get { return ShapeType.Parabolic; } }

            public bool PtIn(Point p)
            {
                foreach(var pts in this.Points)
                foreach (var p1 in pts)
                    if (Math.Abs(p1.X - p.X) <= 6 && Math.Abs(p1.Y - p.Y) <= 6)
                        return true;
                return false;
            }
            public bool IntersetRect(Rectangle rec)
            {
                foreach (var pts in this.Points)
                    foreach (var p in pts)
                        if (rec.Contains((int)p.X, (int)p.Y))
                            return true;
                return false;
            }
            public string Text { get; set; } = "A";

            //private void RefreshData2()
            //{
            //    IsDirty = false;
            //    float cx = this.Canvas.Center.X;
            //    float cy = this.Canvas.Center.Y;
            //    this.Points.Clear();
            //    DataTable dt = new DataTable();
            //    Stack<char> stack = new Stack<char>();
            //    for (int i = 0; i < this.CanvasCtl.Width; i++)
            //    {
            //        //var x = i - cx;//转换为中心点的x电脑屏幕坐标
            //        //x = x / this.Canvas.ScaleRation;//转换为当前坐标系的真实坐标
            //        //var y = (float)(this.A*Math.Pow(x, 2)+this.B*x+this.C) * this.Canvas.ScaleRation;

            //        var x = i - cx;//转换为中心点的x电脑屏幕坐标
            //        x = x / this.Canvas.ScaleRation;//转换为函数真实坐标
            //        if (x > int.MaxValue || x < int.MinValue)
            //            break;
            //        var exp = this.Express.Replace("X", x.ToString()).Replace("x", x.ToString()).Replace("**", "?");
            //        var y = Convert.ToSingle(ParaseExpress(dt, stack,exp));
                   
            //        y = y * this.Canvas.ScaleRation;
            //        if (y > int.MaxValue || y < int.MinValue)
            //            break;
            //        y=(int)(cy - y);
                   
            //        this.Points.Add(new Point(i,(int)y));
            //    }
            //}

            private void RefreshData()
            {
                IsDirty = false;
                float cx = this.Canvas.Center.X;
                float cy = this.Canvas.Center.Y;
                this.Points.Clear();
                DataTable dt = new DataTable();
                Stack<char> stack = new Stack<char>();
                float oldY = 0;
                List<Point> pts = new List<Point>();
                bool begin = true;
                for (int i = 0; i < this.CanvasCtl.Width; i++)
                {
                    var x = i - cx;//转换为中心点的x电脑屏幕坐标
                    if(x>3)
                    {

                    }
                    x = x / this.Canvas.ScaleRation;//转换为数学函数真实坐标,原因是通过函数变换后，Y的比率是不一样的。
                   
                    var exp = this.Express.Replace("X", "x").Replace("x", "(" + x.ToString() + ")").Replace("**", "?");
                   // var exp = this.Express.Replace("X",  "x").Replace("x",  x.ToString()).Replace("**", "?");

                    var y = Convert.ToSingle(ParaseExpress(dt, stack, exp));

                    y = y * this.Canvas.ScaleRation;
                    y = (int)(cy - y);//数学函数Y坐标转换为屏幕Y坐标。

                    if (pts.Count > 0 && Math.Abs(oldY - y) > 9000)
                    {
                        begin = true;
                        this.Points.Add(pts);
                        pts = new List<Point>();
                    }
                    else if (y >= 0 && y <= this.CanvasCtl.Height)
                    {
                        pts.Add(new Point(i, (int)y));
                    }
                    else if (begin)
                    {
                        if (y < 10000 && y > -10000)
                        {
                            pts.Add(new Point(i, (int)y));
                            begin = false;
                        }
                    }
                    oldY = y;
                }

                if(pts.Count>0)
                {
                    this.Points.Add(pts);
                }
            }
           
            private object ParaseExpress1(DataTable dt, Stack<char> stack, string str)
            {
                try
                {
                    double res = 0f;
                    while (Regex.Matches(str, "[a-zA-Z]").Count > 0)
                    {
                        stack.Clear();
                        bool f = false;
                        for (int i = 0; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (!f)
                                if (char.IsLetter(c))
                                    f = true;
                                else
                                    continue;

                            if (c == ')' || i == str.Length - 1)
                            {
                                List<char> w = new List<char>();
                                char t;
                                while (stack.Count > 0 && (t = stack.Pop()) != '(')
                                {
                                    w.Add(t);
                                }

                                w.Reverse();
                                string strExp = "(" + new string(w.ToArray()) + ")";
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;
                                res = Convert.ToDouble(dt.Compute(strExp, ""));

                                t = stack.Peek();
                                //Console.WriteLine(strExp);
                                w.Clear();
                                while (stack.Count > 0 && char.IsLetter(stack.Peek()))
                                {
                                    t = stack.Pop();
                                    w.Add(t);
                                }

                                if (w.Count > 0)
                                {
                                    w.Reverse();
                                    string strFun = new string(w.ToArray());
                                    if (strFun == "Cos")
                                    {
                                        res = Math.Cos(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Sin")
                                    {
                                        res = Math.Sin(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Tan")
                                    {
                                        res = Math.Tan(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Sqrt")
                                    {
                                        res = Math.Sqrt(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Asin")
                                    {
                                        res = Math.Asin(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Acos")
                                    {
                                        res = Math.Acos(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Atan")
                                    {
                                        res = Math.Atan(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Abs")
                                    {
                                        res = Math.Abs(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Lg")
                                    {
                                        if (res < 0)
                                            return int.MaxValue;
                                        res = Math.Log10(res);
                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else if (strFun == "Ln")
                                    {
                                        if (res < 0)
                                            return int.MaxValue;
                                        res = Math.Log(res);

                                        str = str.Replace(strFun + strExp, res.ToString());
                                        break;
                                    }
                                    else
                                    {
                                        // Console.WriteLine("未识别的函数:" + strFun);
                                        throw new Exception("未识别的函数:" + strFun);
                                    }
                                }
                                else
                                {
                                    str = str.Replace(strExp, res.ToString());
                                    break;
                                }
                            }
                            else
                                stack.Push(c);
                        }
                    }

                    int idx = 0;
                    while ((idx = str.IndexOf('?')) != -1)
                    {
                        StringBuilder sbd = new StringBuilder();
                        for (int i = idx + 1; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (char.IsDigit(c) || ((i == idx + 1 && c == '-') || c == '.'))
                            {
                                sbd.Append(c);
                            }
                            else 
                                break;
                        }
                        double.TryParse(sbd.ToString(), out double me);
                        var fang = sbd.ToString();
                        sbd.Clear();
                        int m = 0;
                        stack.Clear();
                        for (int i = idx - 1; i >= 0; i--)
                        {
                            var c = str[i];
                            stack.Push(c);
                            if (c == ')')
                                m++;
                            if (c == '(')
                            {
                                m--;
                            }

                            if (m == 0)
                            {
                                string strExp = new string(stack.ToArray());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;

                                res = Convert.ToDouble(dt.Compute(strExp, ""));
                                res = Math.Pow(res, me);
                                str = str.Replace(strExp + "?" + fang, res.ToString());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;
                                break;
                            }

                        }
                    }
                    if (str.Contains("-∞"))
                        return int.MinValue;
                    else if (str.Contains("∞"))
                        return int.MaxValue;

                    return dt.Compute(str, "");
                }
                catch (System.DivideByZeroException  )
                {
                    return int.MaxValue;
                }
               
            }


            private object ParaseExpress2(DataTable dt, Stack<char> stack, string str)
            {
                try
                {
                    StringBuilder blanceExpress = new StringBuilder();
                    double res = 0f;
                    {
                        List<char> gongshi = new List<char>();
                        List<char> digitList = new List<char>();
                        for (int i = 0; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (char.IsLetter(c))
                            {
                                int p = 0;
                                do
                                {
                                    gongshi.Add(c);
                                    i++;
                                    c = str[i];
                                } while (char.IsLetter(c) && i < str.Length);

                                do
                                {
                                    c = str[i];

                                    if (c == '(')
                                        p++;
                                    else if (c == ')')
                                        p--;
                                    digitList.Add(c);
                                    if (p == 0)
                                        break;
                                    i++;
                                } while (p != 0 && i < str.Length);

                                string strFun = new string(gongshi.ToArray());
                                string strVal = new string(digitList.ToArray());
                                res = Convert.ToSingle(dt.Compute(strVal, ""));

                                if (strFun == "Cos")
                                {
                                    res = Math.Cos(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Sin")
                                {
                                    res = Math.Sin(res);
                                    blanceExpress.Append(res.ToString());
                                }

                                else if (strFun == "Tan")
                                {
                                    res = Math.Tan(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Sqrt")
                                {
                                    res = Math.Sqrt(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Asin")
                                {
                                    res = Math.Asin(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Acos")
                                {
                                    res = Math.Acos(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Atan")
                                {
                                    res = Math.Atan(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Abs")
                                {
                                    res = Math.Abs(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Lg")
                                {
                                    res = Math.Log10(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Ln")
                                {
                                    res = Math.Log(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("未识别的函数:" + strFun);
                                    throw new Exception("未识别的函数:" + strFun);
                                }
                            }
                            else
                            {
                                blanceExpress.Append(c);
                            }
                        }
                    }
                    str = blanceExpress.ToString();
                    int idx = 0;
                    while ((idx = str.IndexOf('?')) != -1)
                    {
                        StringBuilder sbd = new StringBuilder();
                        for (int i = idx + 1; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (char.IsDigit(c) || ((i == idx + 1 && c == '-') || c == '.'))
                            {
                                sbd.Append(c);
                            }
                            else
                                break;
                        }
                        double.TryParse(sbd.ToString(), out double me);
                        var fang = sbd.ToString();
                        sbd.Clear();
                        int m = 0;
                        stack.Clear();
                        for (int i = idx - 1; i >= 0; i--)
                        {
                            var c = str[i];
                            stack.Push(c);
                            if (c == ')')
                                m++;
                            if (c == '(')
                            {
                                m--;
                            }

                            if (m == 0)
                            {
                                string strExp = new string(stack.ToArray());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;

                                res = Convert.ToDouble(dt.Compute(strExp, ""));
                                res = Math.Pow(res, me);
                                str = str.Replace(strExp + "?" + fang, res.ToString());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;
                                break;
                            }

                        }
                    }
                    if (str.Contains("-∞"))
                        return int.MinValue;
                    else if (str.Contains("∞"))
                        return int.MaxValue;

                    return dt.Compute(str, "");
                }
                catch (System.DivideByZeroException)
                {
                    return int.MaxValue;
                }

            }


            private object ParaseExpress(DataTable dt, Stack<char> stack, string str)
            {
                try
                {
                    StringBuilder blanceExpress = new StringBuilder();
                    double res = 0f;
                    while (Regex.Matches(str, "[a-zA-Z]").Count > 0)
                    {
                        bool f = false;
                        List<char> gongshi = new List<char>();
                        List<char> digitList = new List<char>();
                        blanceExpress.Clear();
                        for (int i = 0; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (char.IsLetter(c))
                            {
                                int p = 0;
                                do
                                {
                                    blanceExpress.Append(gongshi.ToArray());

                                    gongshi.Clear();
                                    c = str[i];
                                    while (char.IsLetter(c) && i < str.Length)
                                    {
                                        gongshi.Add(c);
                                        i++;
                                        c = str[i];
                                    }
                                    c = str[i + 1];//'('后面还是公式
                                    if (char.IsLetter(c))
                                    {
                                        gongshi.Add(str[i]);
                                        i++;
                                    }
                                }
                                while (char.IsLetter(c));//如果还是公式

                                do
                                {
                                    c = str[i];

                                    if (c == '(')
                                        p++;
                                    else if (c == ')')
                                        p--;
                                    digitList.Add(c);
                                    if (p == 0)
                                        break;
                                    i++;
                                } while (p != 0 && i < str.Length);

                                string strFun = new string(gongshi.ToArray());
                                string strVal = new string(digitList.ToArray());
                                res = Convert.ToSingle(dt.Compute(strVal, ""));
                                var resStr = res.ToString();
                                if (resStr.Contains("-∞"))
                                    return int.MinValue;
                                else if (resStr.Contains("∞"))
                                    return int.MaxValue;

                                if (strFun == "Cos")
                                {
                                    res = Math.Cos(res);
                                    // blanceExpress.Append("(" + res.ToString() + ")");
                                    blanceExpress.Append( res.ToString() );
                                    //  break;
                                }
                                else if (strFun == "Sin")
                                {
                                    res = Math.Sin(res);
                                    blanceExpress.Append(res.ToString());
                                    // break;
                                }

                                else if (strFun == "Tan")
                                {
                                    res = Math.Tan(res);
                                    blanceExpress.Append(res.ToString());
                                    //  break;
                                }
                                else if (strFun == "Sqrt")
                                {
                                    res = Math.Sqrt(res);
                                    blanceExpress.Append(res.ToString());
                                    // break;
                                }
                                else if (strFun == "Asin")
                                {
                                    res = Math.Asin(res);
                                    blanceExpress.Append(res.ToString());
                                    //  break;
                                }
                                else if (strFun == "Acos")
                                {
                                    res = Math.Acos(res);
                                    blanceExpress.Append(res.ToString());
                                    //  break;
                                }
                                else if (strFun == "Atan")
                                {
                                    res = Math.Atan(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Abs")
                                {
                                    res = Math.Abs(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Lg")
                                {
                                    if (res <= 0)
                                        return int.MaxValue;
                                    res = Math.Log10(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else if (strFun == "Ln")
                                {
                                    if (res <= 0)
                                        return int.MaxValue;
                                    res = Math.Log(res);
                                    blanceExpress.Append(res.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("未识别的函数:" + strFun);
                                    throw new Exception("未识别的函数:" + strFun);
                                }
                            }
                            else
                            {
                                blanceExpress.Append(c);
                            }
                        }
                        str = blanceExpress.ToString();
                    }

                    str = blanceExpress.Length>0? blanceExpress.ToString():str;
                    int idx = 0;
                    while ((idx = str.IndexOf('?')) != -1)
                    {
                        StringBuilder sbd = new StringBuilder();
                        for (int i = idx + 1; i < str.Length; i++)
                        {
                            var c = str[i];
                            if (char.IsDigit(c) || ((i == idx + 1 && c == '-') || c == '.'))
                            {
                                sbd.Append(c);
                            }
                            else
                                break;
                        }
                        double.TryParse(sbd.ToString(), out double me);
                        var fang = sbd.ToString();
                        sbd.Clear();
                        int m = 0;
                        stack.Clear();
                        for (int i = idx - 1; i >= 0; i--)
                        {
                            var c = str[i];
                            stack.Push(c);
                            if (c == ')')
                                m++;
                            if (c == '(')
                            {
                                m--;
                            }

                            if (m == 0)
                            {
                                string strExp = new string(stack.ToArray());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;

                                res = Convert.ToDouble(dt.Compute(strExp, ""));
                                res = Math.Pow(res, me);
                                str = str.Replace(strExp + "?" + fang, res.ToString());
                                if (strExp.Contains("-∞"))
                                    return int.MinValue;
                                else if (strExp.Contains("∞"))
                                    return int.MaxValue;
                                break;
                            }

                        }
                    }
                    if (str.Contains("-∞"))
                        return int.MinValue;
                    else if (str.Contains("∞"))
                        return int.MaxValue;

                    return dt.Compute(str, "");
                }
                catch (System.DivideByZeroException)
                {
                    return int.MaxValue;
                }
            }

            public string _express;
            public string Express 
            {
                get
                {
                    return this._express;
                }

                set
                {
                    if(this._express!=value)
                    {
                        this._express = value;
                        this.IsDirty = true;
                    }
                }
            }
            //public void Move(float dx, float dy)
            //{

            //    List<PointF> pts = new List<PointF>();
            //    this.Points.ForEach(p =>
            //    {
            //        pts.Add(new PointF(p.X - dx, p.Y + dy));
            //    });
            //}
            public void Move(int x, int y)
            {
                List<List<Point>> tempPoints = new List<List<Point>>();
                foreach (var pts in this.Points)
                {
                    List<Point> temPts = new List<Point>();
                    for (int i = 0; i < pts.Count; i++)
                    {
                        var p = pts[i];
                        p.X += x;
                        p.Y += y;
                        temPts.Add(p);
                    }
                    tempPoints.Add(temPts);
                }
                this.Points = tempPoints;
            }
            public void Invalid(Graphics g)
            {
                if (this.IsDirty)
                    RefreshData();
                if (Points.Count <= 0)
                    return;

                if (IsActive)
                {
                    using (Pen pen = new Pen(Color.FromArgb(200, Color.Red), this.Width))
                    {
                        foreach (var pts in this.Points)
                            if (pts.Count > 2)
                                g.DrawLines(pen, pts.ToArray());
                    }
                }
                else
                {
                    using (Pen pen = new Pen(Color.FromArgb(200, this.Color), this.Width))
                    {
                        foreach (var pts in this.Points)
                            if (pts.Count > 2)
                                g.DrawLines(pen, pts.ToArray());
                    }
                }

                var rPts = Points[Points.Count - 1];
                var txtPos = rPts[rPts.Count * 9 / 10];
                g.DrawString("f(x)=" + this.Express, this.Font, Brushes.Black, txtPos);
            }
            private Font _font = new Font("微软雅黑", 11f, FontStyle.Bold);

            public Font Font
            {
                get { return this._font; }
                set
                {
                    if (this.Font != value)
                    {
                        this._font = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                    }
                }
            }
            public bool IsDraged { get; set; } = false;
            Point movedFrom, prePos;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = true;
                    movedFrom = e.Location;
                    prePos = movedFrom;
                    this.IsDraged = true;
                }
            }
          
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;

                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                    movedFrom = e.Location;
                    this.CanvasCtl.Invalidate();
                }
            }
           

            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.IsDraged)
                    {
                        if (prePos.X != e.X || prePos.Y != e.Y)
                        {
                            this.Canvas.BatchMoveRevok(prePos, e.X, e.Y);
                            prePos = e.Location;
                        }
                    }
                    this.IsDraged = false;
                    this.CanvasCtl.Invalidate();
                }
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
            public event Action<IShape, bool> ActivChanged;
            private bool _isActive = false;
            public bool IsActive
            {
                get
                {
                    return this._isActive;
                }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            private int _width = 2;
            public int Width
            {
                get
                {
                    return _width;
                }
                set
                {

                    if (this._width != value)
                    {
                        bool add = (value > this._width);
                        this._width = value;
                        if (this._width <= 0)
                            this._width = 1;

                       
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                    }
                }
            }

            private Color _color = Color.DarkBlue;
            public Color Color
            {
                get
                {
                    return this._color;
                }
                set
                {
                    if (this._color != value)
                    {
                        this._color = value;
                        if (this.CanvasCtl != null)
                            this.CanvasCtl.Invalidate();
                    }
                }
            }
        }


        public class UserPicture : IShape
        {
            public int SelIndex { get; set; } = 0;
            [JsonIgnore]
            public CanvasHelper Canvas { get; set; }
            [JsonIgnore]
            public Control CanvasCtl { get; set; }
            public int Id { get; set; }
            public bool IsDirty { get; set; } = true;
            public Point Location { get; set; }


            public int Index { get; set; }
            private string _imgBase64Str = string.Empty;
            public string ImgBase64Str
            {
                get { return this._imgBase64Str; }
                set
                {
                    if (this._imgBase64Str != value)
                    {
                        this._imgBase64Str = value;
                        var m = new MemoryStream(Convert.FromBase64String(value));
                        this.Image= (Bitmap)Bitmap.FromStream(m);
                        m.Dispose();
                    }
                }
            }
            public void MouseWheel(object sender, MouseEventArgs e)
            {
                if (e.Delta > 0)
                {
                    this.Width += (int)(this.Width / 10f);
                    this.Height += (int)(this.Height / 10f);
                }
                else
                {
                    this.Width -= (int)(this.Width / 10f);
                    this.Height -= (int)(this.Height / 10f);
                }
                this.Canvas.Update();
            }
            public string SerializeObject()
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("{");
                sbd.Append("\"Id\":" + this.Id + ",");
                sbd.Append("\"Index\":" + this.Index + ",");
                sbd.Append("\"Text\":\"" + this.Text + "\",");
                sbd.Append("\"ShapeType\":" + (int)this.ShapeType + ",");
                sbd.Append("\"Color\":\"" + this.Color.Name + "\",");
                sbd.Append("\"X\":" + this.X + ",");
                sbd.Append("\"Y\":" + this.Y + ",");
                sbd.Append("\"Width\":" + this.Width + ",");
                sbd.Append("\"Height\":" + this.Height + ",");

                using (MemoryStream m = new MemoryStream())
                {
                    var img = this.Image.Clone() as Image;
                    img.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
                    sbd.Append("\"ImgBase64Str\":\"" + Convert.ToBase64String(m.ToArray()) + "\"");
                    sbd.Append("}");
                }
                return sbd.ToString();
            }
            public IShape Clone()
            {
                return JsonConvert.DeserializeObject<UserPicture>(this.SerializeObject());
            }

            public void Delete()
            {
                this.Canvas.RemoveShape(this);
            }
            public int Height { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public Rectangle Rectangle
            {
                get
                {
                    return new Rectangle(X, Y, this.Width, this.Height);
                }
            }
            public ShapeType ShapeType { get { return ShapeType.UserPicture; } }

            public bool PtIn(Point p)
            {
                return this.Rectangle.Contains(p);
            }
            public bool IntersetRect(Rectangle rec)
            {
                 rec.Intersect(this.Rectangle);
                 return rec.IsEmpty;
            }
            public string Text { get; set; } = "A";

            private void RefreshData()
            {
                IsDirty = false;
            }
            public Image Image { get; set; }
            public void Invalid(Graphics g)
            {
                if (this.Image != null)
                {
                    var rect = this.Rectangle;
                    g.DrawImage(this.Image, rect);
                    rect.Inflate(2, 2);
                    if (IsActive)
                    {
                      
                        Pen pen = new Pen(Color.Red);
                        pen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(pen, rect);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.White, rect);
                    }
                }
            }
            public bool IsDraged { get; set; } = false;
            private Point movedFrom, prePos;
            public void MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.IsActive = true;
                    movedFrom = e.Location;
                    prePos = movedFrom;
                    this.IsDraged = !this.IsDraged;
                }
            }
            public bool IsCapture { get; set; } = false;
            public void MouseMove(object sender, MouseEventArgs e)
            {
                if (IsDraged)
                {
                    var x = e.X - movedFrom.X;
                    var y = e.Y - movedFrom.Y;

                    if (x == 0 && y == 0)
                        return;

                    this.Canvas.MovedAll(x, y);
                    movedFrom = e.Location;
                    this.CanvasCtl.Invalidate();
                }
            }
            public void Move(int dx, int dy)
            {
                this.X += dx;
                this.Y += dy;
            }
            public void MouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (this.IsDraged)
                    {
                        if (prePos.X != e.X || prePos.Y != e.Y)
                        {
                            this.Canvas.BatchMoveRevok(prePos, e.X, e.Y);
                            prePos = e.Location;
                        }
                    }
                    this.IsDraged = false;
                    this.CanvasCtl.Invalidate();
                }
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
            public event Action<IShape, bool> ActivChanged;
            private bool _isActive = false;
            public bool IsActive
            {
                get
                {
                    return this._isActive;
                }
                set
                {
                    if (this._isActive != value)
                    {
                        this._isActive = value;
                        if (this.CanvasCtl != null)
                            using (var g = Graphics.FromHwnd(this.CanvasCtl.Handle))
                            {
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                this.Invalid(g);
                            }
                        ActivChanged?.Invoke(this, value);
                    }
                }
            }

            public int Width { get; set; } = 2;
            public Color Color { get; set; } = Color.Black;
        }

    }
}

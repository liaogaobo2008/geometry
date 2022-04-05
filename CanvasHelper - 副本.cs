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
        public enum DrawMode { None,Dot,Line,Triangle,Rect,Circle,Polygon}
        public class CanvasHelper : IDisposable
        {
            static int id = 0;
            private Point PrePoint = Point.Empty;
            private Point CurrentPoint = Point.Empty;

            private Point RectPointFrom = Point.Empty;
            private Point SelectPointFrom = Point.Empty;
            public DrawMode DrawMode = DrawMode.Line;
            public RichTextBox EditBox = new RichTextBox();

            private List<IShape> DownShapes = new List<IShape>();

            public static  int GetUID { get { return id++; } }

            Control WorkPanel;

            public  List<IShape> Shapes = new List<IShape>();
            public  List<IShape> SelectedShapes = new List<IShape>();

            public Stack<DoUnit> RevokeAllObjects = new Stack<DoUnit>();
            public Stack<DoUnit> RedoAllObjects = new Stack<DoUnit>();

         

            public bool IsDraged = false;
            public CanvasHelper(Control ctl)
            {
                WorkPanel = ctl;
                
                WorkPanel.Paint += WorkPanel_Paint;
                WorkPanel.MouseMove += WorkPanel_MouseMove;
                WorkPanel.MouseDown += WorkPanel_MouseDown;
                WorkPanel.MouseUp += WorkPanel_MouseUp;
                WorkPanel.DoubleClick += WorkPanel_DoubleClick;
                WorkPanel.Resize += WorkPanel_Resize;

                EditBox.BorderStyle = BorderStyle.None;
                EditBox.MouseLeave += EditBox_MouseLeave;
                EditBox.KeyDown += EditBox_KeyDown;
            }
            private void WorkPanel_Resize(object sender, EventArgs e)
            {
                WorkPanel.Invalidate();
            }
            public void Revoke()
            {
                if (this.RevokeAllObjects.Count > 0)
                {
                    var unit = this.RevokeAllObjects.Pop();
                    var redoUnit = new DoUnit();
                    while (unit.Units.Count > 0)
                    {
                        var kv = unit.Units.Pop();
                        var tp = kv.Value.GetType();
                        if (tp == typeof(PointF))
                        {
                            var p = (PointF)kv.Value;
                            Dot dot = kv.Key as Dot;
                            redoUnit.Units.Push(new KeyValuePair<IShape, object>(dot, dot.Location));
                            dot.X = p.X;
                            dot.Y = p.Y;
                            dot.IsDirty = true;
                        }
                        else if (tp == typeof(Point))
                        {
                            var p = (Point)kv.Value;
                            Dot dot = kv.Key as Dot;
                            redoUnit.Units.Push(new KeyValuePair<IShape, object>(dot, dot.Location));

                            dot.X = p.X;
                            dot.Y = p.Y;
                            dot.IsDirty = true;
                        }
                        else if (tp == typeof(bool))
                        {
                            redoUnit.Units.Push(kv);

                            if (Convert.ToBoolean(kv.Value))
                            {
                                kv.Key.Delete();
                            }
                            else
                            {
                                if(kv.Key.ShapeType== ShapeType.Line)
                                {

                                }
                                this.AddShape(kv.Key);
                            }
                        }
                    }
                    if(redoUnit.Units.Count>0)
                        RedoAllObjects.Push(redoUnit);
                    WorkPanel.Invalidate();
                }
            }
            public void Redo()
            {
                if (this.RedoAllObjects.Count > 0)
                {
                    var unit = this.RedoAllObjects.Pop();
                    var revUnit = new DoUnit();
                    while (unit.Units.Count > 0)
                    {
                        var kv = unit.Units.Pop();
                        var tp = kv.Value.GetType();
                        if (tp == typeof(PointF))
                        {
                            var p = (PointF)kv.Value;
                            Dot dot = kv.Key as Dot;
                            dot.X = p.X;
                            dot.Y = p.Y;
                            dot.IsDirty = true;
                        }
                        else if (tp == typeof(Point))
                        {
                            var p = (Point)kv.Value;
                            Dot dot = kv.Key as Dot;
                            dot.X = p.X;
                            dot.Y = p.Y;
                            dot.IsDirty = true;
                        }
                        else if (tp == typeof(bool))
                        {
                            if (Convert.ToBoolean(kv.Value))
                            {
                                if (kv.Key.ShapeType == ShapeType.Line)
                                {

                                }
                                this.AddShape(kv.Key);
                            }
                            else
                                kv.Key.Delete();
                        }
                        revUnit.Units.Push(kv);
                    }
                    if (revUnit.Units.Count > 0)
                        RevokeAllObjects.Push(revUnit);
                    WorkPanel.Invalidate();
                }
            }

            internal void SelectAll()
            {
                foreach (var s in Shapes)
                    s.IsActive = true;
                WorkPanel.Invalidate();
            }

            public static T CloneModel<T>(T oModel)
            {
                var oRes = default(T);
                var oType = typeof(T);
                //create new obj
                oRes = (T)Activator.CreateInstance(oType);
                //pass 1 property
                var lstPro = oType.GetProperties();
                foreach (var oPro in lstPro)
                {
                    var oValue = oPro.GetValue(oModel, null);
                    oPro.SetValue(oRes, oValue, null);
                }

                var lstField = oType.GetFields();
                foreach (var oField in lstField)
                {
                    var oValue = oField.GetValue(oModel);
                    oField.SetValue(oRes, oValue);
                }
                return oRes;
            }
            private void EditBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyData == Keys.Enter)
                    EditBox_MouseLeave(EditBox, EventArgs.Empty);
            }
            private void EditBox_MouseLeave(object sender, EventArgs e)
            {
                var sh = EditBox.Tag as IShape;
                if (sh != null)
                    sh.Text = EditBox.Text;
                this.WorkPanel.Controls.Remove(EditBox);
            }
            public void ShowEdit(IShape sh)
            {
                this.WorkPanel.Controls.Add(this.EditBox);
                this.EditBox.Text = sh.Text;
                this.EditBox.Location = sh.Location;
                this.EditBox.Tag = sh;
            }

            private PointF[] FindCloseArea()
            {
                List<PointF> res = new List<PointF>();
                foreach (var sh in this.Shapes.Where(s => s.IsActive && s.ShapeType == ShapeType.Dot))
                {
                    var dot = sh as Dot;
                    res.Add(dot.Pf);
                }
                return res.ToArray();
            }
            private List<Dot> FindCloseAreaDot()
            {
                List<Dot> res = new List<Dot>();
                foreach (var sh in this.Shapes.Where(s => s.IsActive && s.ShapeType == ShapeType.Dot))
                {
                    var dot = sh as Dot;
                    res.Add(dot);
                }
                return res;
            }
          
            private Dot GetCanDrawRemAngDot()
            {
                var shs = Shapes.Where(s => s.ShapeType == ShapeType.Line && s.IsActive).ToList<IShape>();
                if (shs.Count == 2)
                {
                    var line1 = shs[0] as Line;
                    var line2 = shs[1] as Line;
                    if(line1.selIndex>line2.selIndex)
                    {
                        Line l = line1;
                        line1 = line2;
                        line2 = l;
                    }

                    List<Dot> dots = new List<Dot>();
                    dots.Add(line1.From);
                    dots.Add(line1.To);
                    if (dots.Contains(line2.From))
                    {
                        var ag = line2.From.RemAngles.FirstOrDefault(sh => sh.FromLine == line1 && sh.ToLine == line2);
                        if (ag != null)
                            return null;
                        return line2.From;
                    }
                    else if (dots.Contains(line2.To))
                    {
                        var ag = line2.To.RemAngles.FirstOrDefault(sh => sh.FromLine == line1 && sh.ToLine == line2);
                        if (ag != null)
                            return null;
                        return line2.To;
                    }
                }
                return null;
            }
            private void WorkPanel_DoubleClick(object sender, EventArgs e)
            {
                var p = Control.MousePosition;
                p= WorkPanel.PointToClient(p);
                foreach (var sh in Shapes)
                {
                    if (sh.PtIn(p))
                    {
                        if (sh.ShapeType == ShapeType.Dot)
                        {
                            ShowEdit(sh);
                            break;
                        }
                        else 
                        {//双击删除
                            sh.Delete();
                            WorkPanel.Invalidate();
                            var unit = new DoUnit();
                            unit.Units.Push(new KeyValuePair<IShape, object>(sh, false));
                            this.RevokeAllObjects.Push(unit);
                            return;
                        }
                    }
                }

                var dot= GetCanDrawRemAngDot();
                if (dot != null)
                {
                    var lines = dot.Lines.Where(l => l.IsActive == true).OrderBy(l => l.selIndex).ToList();
                    {
                        var remAng = new RemAngle()
                        {
                            Radios = (int)Math.Sqrt(Math.Pow(dot.X - p.X, 2) + Math.Pow(dot.Y - p.Y, 2)),
                            Dot = dot,
                            FromLine = lines[0],
                            ToLine = lines[1],
                        };
                        var unit = new DoUnit();
                        dot.RemAngles.Add(remAng);
                        this.AddShape(remAng);
                     
                        unit.Units.Push(new KeyValuePair<IShape, object>(remAng, false));
                      
                        this.RevokeAllObjects.Push(unit);
                        WorkPanel.Invalidate();
                    }
                }
                else
                {
                    var dots = FindCloseAreaDot();
                    var lines = LinesByDotsOfArea(dots);
                    if (MyMath.DotInArea(lines, p))
                    {
                        if (dots.Count >= 3)
                        {
                            var fillArea = new FillArea()
                            {
                                Dots = dots,
                            };
                            this.AddShape(fillArea);
                            var unit = new DoUnit();
                            unit.Units.Push(new KeyValuePair<IShape, object>(fillArea, false));
                            this.RevokeAllObjects.Push(unit);
                            WorkPanel.Invalidate();
                        }
                    }
                }

                //var text = new TextArea() { X = p.X, Y = p.Y, Text = "A" };
                //this.AddShape(text);
                //ShowEdit(text);
            }

            public bool IsSuspend { get; set; } = false;
            public List<Line> LinesByDotsOfArea(List<Dot> dots)
            {
                List<Line> lines = new List<Line>();
                foreach (var d in dots)
                {//找到封闭区域的线段。
                    foreach (var l in d.Lines)
                        if (dots.Contains(l.From) && dots.Contains(l.To))
                            lines.Add(l);
                }
                return lines.Distinct().ToList();
            }
            public void AddShape(IShape shape)
            {
                Shapes.Add(shape);
                shape.Id = GetUID;
                Console.WriteLine("create shape " + shape.Id +","+shape.ShapeType);
                shape.CanvasCtl = this.WorkPanel;
                shape.Canvas = this;
                if (!IsSuspend)
                    WorkPanel.Invalidate();
            }
            public void RemoveShape(IShape shape)
            {
                Console.WriteLine("remove shape Id=" + shape.Id + "," + shape.ShapeType);
                Shapes.Remove(shape);
                SelectedShapes.Remove(shape);
                if (Shapes.Count == 0)
                    id = 0;
                WorkPanel.Invalidate();
            }
            public void ClearShapes()
            {
                id = 0;
                Dot.FromChar = 0;
                Shapes.Clear();
                this.RevokeAllObjects.Clear();
                this.RedoAllObjects.Clear();
                WorkPanel.Invalidate();
            }
          
            public void DrawGrid(Graphics g)
            {
                int row = this.WorkPanel.Height / 30;
                int col = this.WorkPanel.Width / 30;

                int x = row / 2;
                int y = col / 2;
                for (int i = 0; i < row; i++)
                {
                    g.DrawString(i.ToString(), this.WorkPanel.Font, Brushes.Black, 0, i * 30);
                    g.DrawLine(Pens.LightGray, 0, i * 30, this.WorkPanel.Width, i * 30);
                }
                for (int j = 0; j < col; j++)
                {
                    g.DrawString(j.ToString(), this.WorkPanel.Font, Brushes.Black, j * 30, 0);
                    g.DrawLine(Pens.LightGray, j * 30, 0, j * 30,this.WorkPanel.Height);
                }

                Pen p = new Pen(Color.Green, 2);
                var cap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 6);//4,4
                p.CustomEndCap = cap;
                g.DrawLine(p, 10, x * 30, this.WorkPanel.Width-10, x * 30);
                g.DrawLine(p, y * 30, this.WorkPanel.Height-10, y * 30, 10);
            }
            private void WorkPanel_MouseUp(object sender, MouseEventArgs e)
            {
                if (SelectPointFrom != Point.Empty)
                {
                    SelectPointFrom = Point.Empty;
                    if (!this.SelectRectangle.IsEmpty)
                    {
                        this.SelectRectangle = Rectangle.Empty;
                        this.WorkPanel.Invalidate();
                    }
                    return;
                }

                IsDraged = false;
                foreach (var sh in DownShapes)
                    sh.MouseUp(sh, e);
                DownShapes.Clear();
            }

            private Dot srcDot = null;
            private void WorkPanel_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left )
                {
                    if (DrawMode == DrawMode.Line)
                    {
                        if (PrePoint.IsEmpty)
                        {
                            srcDot = Shapes.Where(s => s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                            if (srcDot == null)
                            {
                                PrePoint = e.Location;
                            }
                            else
                            {
                                PrePoint = srcDot.P;
                            }
                        }
                        else
                        {
                            DoUnit unit = new DoUnit();
                            if (srcDot == null)
                            {
                                srcDot = new Dot() { X = PrePoint.X, Y = PrePoint.Y };
                                this.AddShape(srcDot);
                                unit.Units.Push(new KeyValuePair<IShape, object>(srcDot, true));
                            }

                            var destDot = Shapes.Where(s => s.ShapeType== ShapeType.Dot&& s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                            if (destDot != null)//连接到了目地Dot
                            {
                                var line= new Line() { From = srcDot, To = destDot };
                                this.AddShape(line);

                                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                                this.RevokeAllObjects.Push(unit);
                               
                                PrePoint = e.Location;
                                srcDot = destDot;
                              //  DrawMode = DrawMode.None;
                            }
                            else
                            {
                                destDot = new Dot() { X = e.X, Y = e.Y };
                                var line = new Line() { From = srcDot, To = destDot };
                                this.AddShape(destDot);
                                this.AddShape(line);

                             
                                unit.Units.Push(new KeyValuePair<IShape, object>(destDot, true));
                                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));

                                this.RevokeAllObjects.Push(unit);
                                PrePoint = e.Location;
                                srcDot = destDot;
                            }
                        }
                    }
                    else if (DrawMode == DrawMode.Dot)
                    {
                        var dotTo = new Dot() { X = e.Location.X, Y = e.Location.Y };
                        this.AddShape(dotTo);
                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(dotTo, true));
                        this.RevokeAllObjects.Push(unit);
                    }
                    else if(DrawMode== DrawMode.Circle)
                    {
                        if(PrePoint.IsEmpty)
                        {
                            PrePoint = e.Location;
                            srcDot = Shapes.Where(s => s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                            if (srcDot == null)
                                PrePoint = e.Location;
                        }
                        else
                        {
                            if(srcDot==null)
                            {
                                srcDot = new Dot() { X = PrePoint.X, Y = PrePoint.Y };
                                this.AddShape(srcDot);
                            }
                            this.AddShape(new Circle() { Center= srcDot,  Location = srcDot.P, Diam = pendingCircle.Diam  });
                            PrePoint = Point.Empty;
                            DrawMode = DrawMode.None;
                            pendingCircle = null;
                        }
                    }
                    else if(DrawMode== DrawMode.Rect)
                    {
                        if (DrawRectangle ==  Rectangle.Empty)
                        {
                            Console.WriteLine("rect mouse down");
                            RectPointFrom = e.Location;
                            DrawRectangle = new Rectangle(e.Location.X, e.Location.Y, 1, 1);
                        }
                        else
                        {
                      
                            var lt = new Dot() { X = DrawRectangle.Left, Y = DrawRectangle.Top };
                            var rt=  new Dot() { X = DrawRectangle.Right, Y = DrawRectangle.Top };
                            var rb = new Dot() { X = DrawRectangle.Right, Y = DrawRectangle.Bottom };
                            var lb = new Dot() { X = DrawRectangle.Left, Y = DrawRectangle.Bottom };

                            this.IsSuspend = true;
                            this.AddShape(lt);
                            this.AddShape(rt);
                            this.AddShape(rb);
                            this.AddShape(lb);
                            this.AddShape( new Line() { From = lt, To = rt });
                            this.AddShape(new Line() { From = rt, To = rb });
                            this.AddShape(new Line() { From = rb, To = lb });
                            this.IsSuspend = false;
                            this.AddShape(new Line() { From = lb, To = lt });

                            DrawRectangle = Rectangle.Empty;
                        }
                    }
                    else
                    {
                        var sh = Shapes.Where(s => s.IsDraged == false && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault();
                        if (sh != null)
                        {
                            DownShapes.Add(sh);
                            sh.MouseDown(sh, e);
                        }
                        else
                        {//空白除点鼠标左键，画选择区域
                            SelectPointFrom = e.Location;
                            this.SelectRectangle = Rectangle.Empty;
                            //var preSelects = this.Shapes.Where(s => s.ShapeType == ShapeType.Line && s.IsActive == true).ToList();
                            //foreach (var l in preSelects)
                            //    l.IsActive = false;
                            //if (preSelects.Count > 0)
                            //    this.WorkPanel.Invalidate();
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right )
                {//结束当前画线,直接返回
                    if (DrawMode == DrawMode.Line)
                    {
                        if (!PrePoint.IsEmpty)
                        {
                            PrePoint = Point.Empty;
                        }
                        srcDot = null;
                        DrawMode = DrawMode.None;
                    }
                    else if(DrawMode== DrawMode.Circle)
                    {
                        pendingCircle = null;
                        PrePoint=Point.Empty;
                        DrawMode = DrawMode.None;
                    }
                    else//正常触发鼠标右键事件
                    {
                        var sh = Shapes.Where(s => s.IsDraged == false && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault();
                        if (sh != null)
                        {
                            DownShapes.Add(sh);
                            sh.MouseDown(sh, e);
                        }
                    }
                }
            }

            PendingCircle pendingCircle = null;

            private class PendingCircle
            {
                public float Diam;
                public PointF LT;
                public Rectangle Rec;
            }
            private Rectangle DrawRectangle = Rectangle.Empty;
            private Rectangle SelectRectangle = Rectangle.Empty;
            private Rectangle RefeshRectBy2Dot(Point p1, Point p2)
            {
                    Point f = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                    int w = Math.Abs(p1.X - p2.X);
                    int h = Math.Abs(p1.Y - p2.Y);
                    return new Rectangle(f.X, f.Y, w, h);
            }
            private void WorkPanel_MouseMove(object sender, MouseEventArgs e)
            {
                //    return;
                if(DrawMode== DrawMode.Line)
                {
                    if (!PrePoint.IsEmpty&&(e.Location.X!=PrePoint.X||e.Y!=PrePoint.Y))
                    {
                        CurrentPoint = e.Location;
                        this.WorkPanel.Invalidate();
                    }
                }
                else if (DrawMode == DrawMode.Rect)
                {
                    if (DrawRectangle != Rectangle.Empty)
                    {
                        DrawRectangle = RefeshRectBy2Dot(RectPointFrom, e.Location);
                        this.WorkPanel.Invalidate();
                        return;
                    }
                }
                else if (DrawMode == DrawMode.Circle)
                {
                    if (!PrePoint.IsEmpty)
                    {
                        if (pendingCircle == null)
                        {
                            pendingCircle = new PendingCircle();
                            pendingCircle.Rec= new Rectangle(PrePoint.X - 3, PrePoint.Y - 3, 6, 6);
                        }

                        using (var g = Graphics.FromHwnd(this.WorkPanel.Handle))
                        {
                            var r = (Math.Sqrt(Math.Pow(e.X - PrePoint.X, 2) + Math.Pow(e.Y - PrePoint.Y, 2)));//2 ;
                            if (r > 0)
                            {
                                var x = PrePoint.X - r;
                                var y = PrePoint.Y - r;
                                pendingCircle.LT.X = (float)x;
                                pendingCircle.LT.Y = (float)y;
                                pendingCircle.Diam = (float)(r * 2);
                                this.WorkPanel.Invalidate();
                            //    Console.WriteLine("r=" + r + ",x=" + x + ",y=" + y);
                                //   g.Clear(Color.White);
                                //  g.DrawArc(Pens.Black, (float)x, (float)y, (float)(r * 2), (float)(r * 2), 0f, 360f);
                                return;
                            }
                        }
                    }
                }
                else if (SelectPointFrom != Point.Empty)
                {//绘制选择区域
                    this.SelectRectangle = RefeshRectBy2Dot(SelectPointFrom, e.Location);

                    foreach (var l in this.Shapes.Where(s => s.ShapeType == ShapeType.Line))
                    {
                        l.IsActive = MyMath.LineInRect(l as Line, this.SelectRectangle);
                    }
                    foreach (var l in this.Shapes.Where(s => s.ShapeType == ShapeType.Dot))
                    {
                        l.IsActive = this.SelectRectangle.Contains(l.Location);
                    }

                    if (this.SelectRectangle.Width >= 2 && this.SelectRectangle.Height >= 2)
                        this.WorkPanel.Invalidate();
                    return;
                }


                var all = Shapes.Where(s => s.PtIn(e.Location)).ToList();
                var shDest = all.Where(s => s.IsDraged == false).OrderByDescending(t => t.Id).FirstOrDefault();
                var shSrc = Shapes.Where(s => s.IsDraged == true).OrderByDescending(t => t.Id).FirstOrDefault();

                if (shSrc != null)
                    shSrc.MouseMove(shSrc, e);
                if (shDest != null)
                {
                    if (!shDest.IsCapture)
                        shDest.MouseEnter(shDest, e);
                    shDest.MouseMove(shDest, e);
                }
                var leaveShs = Shapes.Where(s => s.IsCapture && !s.PtIn(e.Location)).ToList();
                foreach (var sh in leaveShs)
                    sh.MouseLeave(sh, e);
                DrawXY(e.X, e.Y);
            }
          
            private void DrawXY(int x, int y)
            {
                using (var g = Graphics.FromHwnd(this.WorkPanel.Handle))
                {

                    g.FillRectangle(Brushes.Black, 0, 0, 100, 20);
                    g.DrawString(string.Format("x={0},y={1}", x, y), new Font("宋体", 12f), Brushes.Yellow, 0, 0);
                }
            }
            private void DrawXY(int x, int y, Graphics g)
            {
                {

                    g.FillRectangle(Brushes.Black, 0, 0, 100, 20);
                    g.DrawString(string.Format("x={0},y={1}", x, y), new Font("宋体", 12f), Brushes.Yellow, 0, 0);
                }
            }
            private void WorkPanel_Paint(object sender, PaintEventArgs e)
            {
                Console.WriteLine("WorkPanel_Paint");
                // Bitmap bufferimage = new Bitmap((int)rect.Width, (int)rect.Height);
                // e.Graphics.Clear(this.WorkPanel.BackColor);
                // using (var g=Graphics.FromImage(bufferimage))
                //  DrawGrid(e.Graphics);
                // Console.WriteLine("DrawGrid");
                // return;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                foreach (var sh in Shapes)
                    sh.Invalid(e.Graphics);

                var p = Control.MousePosition;
                p = WorkPanel.PointToClient(p);
                DrawXY(p.X, p.Y, e.Graphics);
                if (this.DrawRectangle != Rectangle.Empty)
                {
                    Pen pen = new Pen(Color.DarkBlue, 2);
                    e.Graphics.DrawRectangle(pen, this.DrawRectangle);
                }
                
                if(!this.SelectPointFrom.IsEmpty)
                {
                    Pen pen = new Pen(Color.Green, 1);
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, this.SelectRectangle);
                }
                if(this.DrawMode== DrawMode.Line)
                {
                    if (this.PrePoint != null && this.CurrentPoint != null)
                        e.Graphics.DrawLine(Pens.Green, this.PrePoint, this.CurrentPoint);
                }
                if (this.pendingCircle != null && this.pendingCircle.Diam > 0)
                {
                    e.Graphics.FillEllipse(Brushes.Red,pendingCircle.Rec);
                    e.Graphics.DrawArc(Pens.Black, this.pendingCircle.LT.X, this.pendingCircle.LT.Y, this.pendingCircle.Diam, this.pendingCircle.Diam, 0f, 360f);
                }
                //   e.Graphics.DrawImage(bufferimage, rect);
            }

            public void Dispose()
            {
                WorkPanel.Paint -= WorkPanel_Paint;
                WorkPanel.MouseMove -= WorkPanel_MouseMove;
                WorkPanel.MouseDown -= WorkPanel_MouseDown;
                WorkPanel.MouseUp -= WorkPanel_MouseUp;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Riches.Visio
{
    namespace Geometry
    {
        public enum DrawMode { None,Dot,Line,Triangle,Rect,Circle,Polygon,TextArea,ManulWrite}
        public enum AttchEnum { None,LockLine,LockCircle,LineFrom,LineTo};
        public class CanvasHelper : IDisposable
        {
            public  int ID = 0;
            public  int SelSq = 0;
            private Point PrePoint = Point.Empty;
            private Point CurrentPoint = Point.Empty;

            private Point RectPointFrom = Point.Empty;
            private Point SelectPointFrom = Point.Empty;

            private DrawMode _drawMode = DrawMode.None;
            public event Action<Geometry.DrawMode> OnDrawModeChanged;

            ContextMenuStrip ContextPopMenu;
            ToolStripMenuItem drawLine, drawCircle, deleteItem, CopyItem, CutItem, pasteItem,
                firstItem, lastItem, remAngleItem, fillAreaItem,propItem;
            private void CreatePopmenu()
            {
                this.ContextPopMenu = new System.Windows.Forms.ContextMenuStrip();

                drawLine = new System.Windows.Forms.ToolStripMenuItem();
                drawLine.Text = "画线";
                drawLine.Image = Properties.Resources.线;
                drawLine.Click += DrawShape_Click;
                drawLine.Tag = 2;
                drawCircle = new System.Windows.Forms.ToolStripMenuItem();
                drawCircle.Text = "画圆";
                drawCircle.Tag = 5;
                drawCircle.Image = Properties.Resources.圆;
                drawCircle.Click += DrawShape_Click;

                ToolStripSeparator line1 = new System.Windows.Forms.ToolStripSeparator();
                line1.Text = "-";

                deleteItem = new System.Windows.Forms.ToolStripMenuItem();

                deleteItem.Image = Properties.Resources.复制;
                deleteItem.Text = "删除";
                deleteItem.Click += DeleteItem_Click;

                CopyItem = new System.Windows.Forms.ToolStripMenuItem();
                CopyItem.Image = Properties.Resources.复制;
                CopyItem.Text = "复制";
                CopyItem.Click += CopyItem_Click;
                CutItem = new System.Windows.Forms.ToolStripMenuItem();
                CutItem.Text = "剪切";
                CutItem.Click += CutItem_Click;
                pasteItem = new System.Windows.Forms.ToolStripMenuItem();
                pasteItem.Text = "粘贴";
                pasteItem.Click += PasteItem_Click;

                remAngleItem = new System.Windows.Forms.ToolStripMenuItem();
                remAngleItem.Text = "标注角度";
                remAngleItem.ShortcutKeys = Keys.Control | Keys.G;
                remAngleItem.Click += RemAngleItem_Click;
                fillAreaItem = new System.Windows.Forms.ToolStripMenuItem();
                fillAreaItem.Text = "填充区域";
                fillAreaItem.Click += FillAreaItem_Click;
                fillAreaItem.ShortcutKeys = Keys.Control | Keys.B;
                ToolStripSeparator line2 = new System.Windows.Forms.ToolStripSeparator();
                line2.Text = "-";

                firstItem = new System.Windows.Forms.ToolStripMenuItem();
                firstItem.Text = "移到最底层";
                firstItem.Image = Properties.Resources.移到底层;
                firstItem.Click += FirstItem_Click;
                lastItem = new System.Windows.Forms.ToolStripMenuItem();
                lastItem.Text = "移到最上层";
                lastItem.Image = Properties.Resources.移到顶层;
                lastItem.Click += LastItem_Click;

                ToolStripSeparator line3 = new System.Windows.Forms.ToolStripSeparator();
                line3.Text = "-";

                propItem = new ToolStripMenuItem();
                propItem.Text = "属性";
                propItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.P;
                propItem.Click += PropItem_Click;

                this.ContextPopMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                drawLine,drawCircle,line1,CopyItem,CutItem,deleteItem,pasteItem,remAngleItem,fillAreaItem,line2,firstItem,lastItem,line3,propItem
                });

                this.ContextPopMenu.Opening += ContextPopMenu_Opening;
            }

            private void PropItem_Click(object sender, EventArgs e)
            {

                var sh = Shapes.FirstOrDefault(s => s.PtIn(this.openingPos) && (s.ShapeType == ShapeType.Dot || s.ShapeType == ShapeType.Line || s.ShapeType == ShapeType.Circle));
                if (sh == null)
                {
                    sh = this.Shapes.FirstOrDefault(s => s.IsActive && (s.ShapeType == ShapeType.Dot || s.ShapeType == ShapeType.Line || s.ShapeType == ShapeType.Circle));
                }

                if (sh!=null)
                {
                    PropForm propForm = new PropForm(sh);
                    propForm.ShowDialog();
                }
            }

            public Cursor Cursor {
                get {
                    return this.WorkPanel.Cursor;
                }
                set { this.WorkPanel.Cursor = value; }
            } 

            public DrawMode DrawMode {
                get
                {
                    return this._drawMode;
                }
                set
                {
                    if(this._drawMode!=value)
                    {
                        this._drawMode = value;
                        OnDrawModeChanged?.Invoke(value);
                    }
                }
            }


            public TextBox EditBox = new TextBox();
            public RichTextBox EditRichBox = new RichTextBox();

            private List<IShape> DownShapes = new List<IShape>();
            public   int GetUID { get { return ID++; } }
            public   int GetSelUID { get { return SelSq++; } }
            Panel WorkPanel;

            public  List<IShape> Shapes = new List<IShape>();
            public  List<IShape> SelectedShapes = new List<IShape>();

            public Stack<DoUnit> RevokeAllObjects = new Stack<DoUnit>();
            public Stack<DoUnit> RedoAllObjects = new Stack<DoUnit>();
            public System.Windows.Forms.Button buttonMove = new Button();
            public bool IsDraged = false;
            public CanvasHelper(Panel ctl)
            {
                CreatePopmenu();
                WorkPanel = ctl;
                TextSize = TextRenderer.MeasureText("8", Font);
                WorkPanel.Paint += WorkPanel_Paint;
                WorkPanel.MouseMove += WorkPanel_MouseMove;
                WorkPanel.MouseDown += WorkPanel_MouseDown;
                WorkPanel.MouseUp += WorkPanel_MouseUp;
                WorkPanel.DoubleClick += WorkPanel_DoubleClick;
                WorkPanel.Resize += WorkPanel_Resize;
                WorkPanel.MouseWheel += WorkPanel_MouseWheel;
                
                // WorkPanel.PreviewKeyDown += WorkPanel_PreviewKeyDown;

                EditBox.BorderStyle = BorderStyle.FixedSingle;
                EditBox.MouseLeave += EditBox_MouseLeave;
                //   EditBox.ScrollBars = RichTextBoxScrollBars.None;
                EditBox.KeyDown += EditBox_KeyDown;
                EditBox.Font = new Font("微软雅黑", 14f, FontStyle.Bold);

                EditRichBox.BorderStyle = BorderStyle.None;
                EditRichBox.ScrollBars = RichTextBoxScrollBars.None;
                EditRichBox.Leave += EditRichBox_Leave;
                EditRichBox.Font = new Font("微软雅黑", 12f);
                EditRichBox.KeyDown += EditRichBox_KeyDown;
                EditRichBox.WordWrap = false;
                EditRichBox.TextChanged += EditRichBox_TextChanged;
                EditRichBox.HideSelection = false;
                EditRichBox.Margin = new Padding(0);

                this.WorkPanel.Font = EditRichBox.Font;
                this.Center.X = this.WorkPanel.Width / 2;
                this.Center.Y = this.WorkPanel.Height / 2;
                CenterRect = new Rectangle(Center.X - 4, Center.Y - 4, 8, 8);
                ctl.Controls.Add(buttonMove);
                this.buttonMove.Size = new Size(0, 0);
                this.buttonMove.Left = this.WorkPanel.Width - this.buttonMove.Width;// * 2;
                this.buttonMove.Top = this.WorkPanel.Height - buttonMove.Height;// * 2;
                System.Windows.Forms.Timer timer = new Timer();
                timer.Interval = 1000;
             //   timer.Enabled = true;
                timer.Tick += Timer_Tick;
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                this.adverPos.X+=2;
                this.adverPos.Y+=2;
                if (this.adverPos.X > this.WorkPanel.Width)
                    this.adverPos.X = 0;
                if (this.adverPos.Y > this.WorkPanel.Height)
                    this.adverPos.Y = 0;
                this.Update();
            }

            private void EditRichBox_TextChanged(object sender, EventArgs e)
            {
                var textArea = this.EditRichBox.Tag as TextArea;
                if (textArea != null)
                    textArea.Text = this.EditRichBox.Text;

                var size = TextRenderer.MeasureText(this.EditRichBox.Text, this.EditRichBox.Font);
                this.EditRichBox.Width = size.Width;
                this.EditRichBox.Height = size.Height+20;
                textArea.Width = size.Width + 15;
                
            }

            private void EditRichBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    EditRichBox.Height += 25;
                }
                var s = TextRenderer.MeasureText(this.EditRichBox.Text, this.EditRichBox.Font);
                if (s.Width > this.EditRichBox.Width)
                    this.EditRichBox.Width += 40;
            }

            private void EditRichBox_Leave(object sender, EventArgs e)
            {
                this.WorkPanel.Controls.Remove(this.EditRichBox);
                var textArea = this.EditRichBox.Tag as TextArea;
                if (textArea != null)
                {
                    textArea.Text = this.EditRichBox.Text;
                    textArea.IsActive = false;
                    textArea.IsDraged = false;
                    EditRichBox.TextChanged -= EditRichBox_TextChanged;
                    this.EditRichBox.Text = "";
                    EditRichBox.TextChanged += EditRichBox_TextChanged;
                    this.EditRichBox.Width = 100;
                    this.EditRichBox.Height = TextRenderer.MeasureText("我", this.EditRichBox.Font).Height + 5;
                    this.EditRichBox.Font = this.WorkPanel.Font;
                }
            }
            public void DeleteSelected()
            {
                var delList = this.Shapes.Where(sh => sh.IsActive).ToList();
                foreach (var sh in delList)
                {
                    this.Shapes.Remove(sh);
                    sh.Delete();
                }
                if (Shapes.Count == 0)
                    ID = 0;
                if (delList.Count > 0)
                    this.Update();
            }
            private void WorkPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteSelected();
                }
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
                            var otp = kv.Key.GetType();
                            if (otp == typeof(Dot))
                            {
                                Dot dot = kv.Key as Dot;
                                redoUnit.Units.Push(new KeyValuePair<IShape, object>(dot, p));

                                dot.X -= p.X;
                                dot.Y -= p.Y;
                                dot.IsDirty = true;
                            }
                            else if (otp == typeof(ManualPen))
                            {
                                ManualPen manualPen = kv.Key as ManualPen;
                               
                                redoUnit.Units.Push(new KeyValuePair<IShape, object>(manualPen, p));

                                manualPen.Move(-p.X, -p.Y);
                                manualPen.IsDirty = true;
                            }
                            else if (otp == typeof(TextArea))
                            {
                                TextArea textarea = kv.Key as TextArea;

                                redoUnit.Units.Push(new KeyValuePair<IShape, object>(textarea, p));

                                textarea.Move(-p.X, -p.Y);
                                textarea.IsDirty = true;
                            }
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
                                if (kv.Key.ShapeType == ShapeType.Line)
                                {
                                    var line = kv.Key as Line;
                                    if (!line.From.Lines.Contains(line))
                                        line.From.Lines.Add(line);
                                    if (!line.To.Lines.Contains(line))
                                        line.To.Lines.Add(line);
                                }
                                else if (kv.Key.ShapeType == ShapeType.RemAngle)
                                {
                                    var remAngle = kv.Key as RemAngle;
                                   // if (!remAngle.Dot.RemAngles.Contains(remAngle))
                                   //     remAngle.Dot.RemAngles.Add(remAngle);
                                }
                                this.AddShape(kv.Key);
                            }
                        }
                        else if(tp==typeof(AttchEnum))
                        {
                            AttchEnum kndAttch = (AttchEnum)kv.Value;

                            if(kndAttch== AttchEnum.LockLine)
                            {
                                var dot = kv.Key as Dot;
                                if (dot != null)
                                    dot.LockLine = null;
                            }
                           
                        }
                        else if(tp==typeof(Dot))
                        {
                            var line = kv.Key as Line;
                            var dot = kv.Value as Dot;
                            line.From = dot;
                        }
                    }
                    if (redoUnit.Units.Count > 0)
                    {
                        RedoAllObjects.Push(redoUnit);
                        this.Update();
                    }
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
                            if (kv.Key.GetType() == typeof(Dot))
                            {
                                Dot dot = kv.Key as Dot;
                                dot.X += p.X;
                                dot.Y += p.Y;
                                dot.IsDirty = true;
                            }
                            else if (kv.Key.GetType() == typeof(ManualPen))
                            {
                                ManualPen manualPen = kv.Key as ManualPen;
                                manualPen.Move(p.X, p.Y);
                                manualPen.IsDirty = true;
                            }
                            else if (kv.Key.GetType() == typeof(TextArea))
                            {
                                TextArea textarea = kv.Key as TextArea;
                                textarea.Move(p.X, p.Y);
                                textarea.IsDirty = true;
                            }
                        }
                       
                        else if (tp == typeof(bool))
                        {
                            if (Convert.ToBoolean(kv.Value))
                            {
                                if (kv.Key.ShapeType == ShapeType.Line)
                                {
                                    var line = kv.Key as Line;
                                    if (!line.From.Lines.Contains(line))
                                        line.From.Lines.Add(line);
                                    if (!line.To.Lines.Contains(line))
                                        line.To.Lines.Add(line);
                                }
                                else if (kv.Key.ShapeType == ShapeType.RemAngle)
                                {
                                    var remAngle = kv.Key as RemAngle;
                                   // if (!remAngle.Dot.RemAngles.Contains(remAngle))
                                  //      remAngle.Dot.RemAngles.Add(remAngle);
                                }
                                else if(kv.Key.ShapeType== ShapeType.Polygon)
                                {
                                    var ply = kv.Key as Polygon;

                                }
                                this.AddShape(kv.Key);
                            }
                            else
                                kv.Key.Delete();
                        }
                        revUnit.Units.Push(kv);
                    }
                    if (revUnit.Units.Count > 0)
                    {
                        RevokeAllObjects.Push(revUnit);
                        this.Update();
                    }
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
            [DllImport("User32.dll")]
            private static extern bool SetCursorPos(int x, int y);

            public void ShowEdit(IShape sh)
            {
                this.WorkPanel.Controls.Add(this.EditBox);
                this.EditBox.Text = sh.Text;
                if (sh.ShapeType == ShapeType.Dot)
                {
                    var dot = sh as Dot;
                    var s = TextRenderer.MeasureText(dot.Text, EditBox.Font);
                    this.EditBox.SetBounds(dot.TextPos.X, dot.TextPos.Y, s.Width + 5, s.Height + 5);
                    this.EditBox.SelectAll();
                  //   var p=  this.EditBox.PointToScreen(this.EditBox.Location);
                  //  SetCursorPos(EditBox.Location.X, EditBox.Location.Y);
                }
                else
                    this.EditBox.Location = sh.Location;
                this.EditBox.Tag = sh;
                this.EditBox.Focus();
            }
            private void ShowRichEdit(IShape sh)
            {
                this.WorkPanel.Controls.Add(this.EditRichBox);
                this.EditRichBox.Text = sh.Text;
                if (sh.ShapeType == ShapeType.TextArea)
                {
                    var tx = sh as TextArea;
                    var s = TextRenderer.MeasureText(tx.Text, EditRichBox.Font);

                    var size = TextRenderer.MeasureText(this.EditRichBox.Text, this.EditRichBox.Font);
                  //  this.EditRichBox.Width = size.Width;
                  //  this.EditRichBox.Height = size.Height;

                    this.EditRichBox.SetBounds(tx.X, tx.Y, size.Width+5, size.Height+15);
                    this.EditRichBox.SelectAll();
                    this.EditRichBox.Font = tx.Font;
                }
                else
                    this.EditRichBox.Location = sh.Location;
                this.EditRichBox.Tag = sh;
                this.EditRichBox.Focus();
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
                    if(line1.SelIndex>line2.SelIndex)
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
                       var ag= this.Shapes.FirstOrDefault(sh =>
                        {
                            if(sh.ShapeType== ShapeType.RemAngle)
                            {
                                var remangle = sh as RemAngle;
                                if (remangle.FromLine == line1 && remangle.ToLine == line2)
                                    return true;
                            }
                            return false;
                        });

                        //var ag = line2.From.RemAngles.FirstOrDefault(sh => sh.FromLine == line1 && sh.ToLine == line2);
                        if (ag != null)
                            return null;
                        return line2.From;
                    }
                    else if (dots.Contains(line2.To))
                    {
                        var ag = this.Shapes.FirstOrDefault(sh =>
                        {
                            if (sh.ShapeType == ShapeType.RemAngle)
                            {
                                var remangle = sh as RemAngle;
                                if (remangle.FromLine == line1 && remangle.ToLine == line2)
                                    return true;
                            }
                            return false;
                        });

                        //var ag = line2.To.RemAngles.FirstOrDefault(sh => sh.FromLine == line1 && sh.ToLine == line2);
                        if (ag != null)
                            return null;
                        return line2.To;
                    }
                }
                return null;
            }
            public Point GetMousePos()
            {
                var p = Control.MousePosition;
                p = WorkPanel.PointToClient(p);
                p.X = p.X + this.WorkPanel.HorizontalScroll.Value;
                p.Y = p.Y + this.WorkPanel.VerticalScroll.Value;
                return p;
            }
            private void WorkPanel_DoubleClick(object sender, EventArgs e)
            {
                var p = GetMousePos();
                foreach (var sh in Shapes)
                {
                    if (sh.PtIn(p))
                    {
                        if (sh.ShapeType == ShapeType.Dot)
                        {
                            //  ShowEdit(sh);
                            break;
                        }
                        else if (sh.ShapeType == ShapeType.TextArea)
                        {
                            ShowRichEdit(sh);
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
            }

            public void RemoAngle(Point p)
            {
                var dot = GetCanDrawRemAngDot();
                if (dot != null)
                {
                    var lines = dot.Lines.Where(l => l.IsActive == true).OrderBy(l => l.SelIndex).ToList();
                    {
                        var remAng = new RemAngle()
                        {
                            Radios = (int)Math.Sqrt(Math.Pow(dot.X - p.X, 2) + Math.Pow(dot.Y - p.Y, 2)),
                            Dot = dot,
                            FromLine = lines[0],
                            ToLine = lines[1],
                        };
                        var unit = new DoUnit();
                        this.AddShape(remAng);
                        this.Update();
                        unit.Units.Push(new KeyValuePair<IShape, object>(remAng, true));
                        this.RevokeAllObjects.Push(unit);
                    }
                }
            }
            public void FillArea(Point p)
            {
                List<Dot> dots = new List<Dot>();
                foreach (Dot d in Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.Dot).OrderBy(t => t.SelIndex))
                {
                    dots.Add(d);
                }

                if (dots.Count >= 3)
                {
                    var lines = LinesByDotsOfArea(dots);
                    if (MyMath.DotInArea(lines, p))
                    {
                        var fillArea = new FillArea()
                        {
                            Dots = dots,
                        };
                        this.AddShape(fillArea);
                        this.Update();
                        var unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(fillArea, true));
                        this.RevokeAllObjects.Push(unit);
                    }
                }
            }
            public bool IsSuspend { get; set; } = false;
            public List<Line> LinesByDotsOfArea(List<Dot> dots)
            {
                List<Line> lines = new List<Line>();
                foreach (var d in dots)
                {//找到封闭区域的线段。
                    foreach (var l in d.Lines)
                    {
                        if (dots.Contains(l.From) && dots.Contains(l.To))
                            lines.Add(l);
                        else  if (d.LockLine == l)
                            lines.Add(l);
                    }
                }
                return lines.Distinct().ToList();
            }
            public void Update()
            {
                WorkPanel.Invalidate();
            }
            public IShape AddShape(IShape shape)
            {
                Shapes.Add(shape);
                shape.Id = GetUID;
              //  Console.WriteLine("create shape " + shape.Id +","+shape.ShapeType);
                shape.CanvasCtl = this.WorkPanel;
                shape.Canvas = this;
                shape.ActivChanged += Shape_ActivChanged;
                IsMoified = true;
                return shape;
            }
            public IShape AddShape2(IShape shape)
            {
                Shapes.Add(shape);
                shape.Id = GetUID;
                Console.WriteLine("create shape " + shape.Id + "," + shape.ShapeType);
                shape.CanvasCtl = this.WorkPanel;
                shape.Canvas = this;
                shape.ActivChanged += Shape_ActivChanged;
                IsMoified = true;
                DoUnit unit = new DoUnit();
                unit.Units.Push(new KeyValuePair<IShape, object>(shape, true));
                this.RevokeAllObjects.Push(unit);
                return shape;
            }
          
            public event Action<CanvasHelper, IShape, bool> ShapeActivChanged;
            private void Shape_ActivChanged(IShape shape, bool isActive)
            {
                ShapeActivChanged?.Invoke(this,shape, isActive);
            }

            public void RemoveShape(IShape shape)
            {
                Console.WriteLine("remove shape Id=" + shape.Id + "," + shape.ShapeType);
                Shapes.Remove(shape);
                SelectedShapes.Remove(shape);
                if (Shapes.Count == 0)
                    ID = 0;
                IsMoified = true;
                shape.ActivChanged -= Shape_ActivChanged;
            }
            public void ClearShapes()
            {
                ID = 0;
                Dot.FromChar = 0;
                foreach(var sh in Shapes)
                    sh.ActivChanged -= Shape_ActivChanged;
                Shapes.Clear();

               // this.RevokeAllObjects.Clear();
              //  this.RedoAllObjects.Clear();
                WorkPanel.Invalidate();
            }

            private bool _isDrawCoordinate = true;
            private bool _isDrawGrid = true;

            public bool IsDrawCoordinate
            {
                get { return this._isDrawCoordinate; }
                set
                {
                    if(this._isDrawCoordinate!=value)
                    {
                        this._isDrawCoordinate = value;
                        this.Update();
                    }
                }
            }
            private bool _isDisplayXYCoordinator = false;
            public bool IsDisplayXYCoord
            {
                get
                {
                    return this._isDisplayXYCoordinator;
                }
                set
                {
                    if (this._isDisplayXYCoordinator != value)
                    {
                        this._isDisplayXYCoordinator = value;
                        this.Update();
                    }
                }
            }

            public bool IsDrawGrid
            {
                get { return this._isDrawGrid; }
                set
                {
                    if (this._isDrawGrid != value)
                    {
                        this._isDrawGrid = value;
                        this.Update();
                    }
                }
            }
            private int GridWH { get; set; } = 40;
            public void DrawGrid(Graphics g)
            {

                double miniQty;
               // int oneH = 40;
              //  NewMethod(out miniQty, out oneH);

                int w = this.WorkPanel.Width + this.WorkPanel.HorizontalScroll.Value;
                int h = this.WorkPanel.Height + this.WorkPanel.VerticalScroll.Value;
                int cy = (h) / 2;

                g.DrawLine(Pens.LightGray, 0, cy, w, cy);//draw // x
                while (true)// 从中间往上走
                {
                    cy -= GridWH;
                    if (cy < 0)
                        break;
                    g.DrawLine(Pens.LightGray, 0, cy, w, cy);
                }
                cy = (h) / 2;
                while (true)
                {
                    cy += GridWH;
                    if (cy > h)
                        break;
                    g.DrawLine(Pens.LightGray, 0, cy, w, cy);
                }
                //=====================================
                int cx = (w) / 2;
                g.DrawLine(Pens.LightGray, cx, 0, cx, h);
                while (true)
                {//右走起
                    cx -= GridWH;
                    if (cx < 0)
                        break;
                    g.DrawLine(Pens.LightGray, cx, 0, cx, h);

                }
                cx = (w) / 2;

                while (true)
                {// | ->
                    cx += GridWH;
                    if (cx > w)
                        break;
                    g.DrawLine(Pens.LightGray, cx, 0, cx, h);
                }
            }
            private float _scaleRation = 40f;

            public float ScaleRation {
                get
                {
                    return this._scaleRation;
                }
                set
                {
                    if (this._scaleRation != value)
                    {
                        this._scaleRation = value;
                       // this.Update();
                    }
                }
            }
          
            public Point Center = Point.Empty;

            private Rectangle CenterRect { get; set; } = Rectangle.Empty;
            public void MoveCenter(int dx,int dy)
            {
                Center.X += dx;
                Center.Y += dy;
                CenterRect = new Rectangle(Center.X - 4, Center.Y - 4, 8, 8);
                foreach (var sh in this.Shapes)//.Where(t => t.ShapeType == ShapeType.Parabolic))
                {
                    sh.Move(dx, dy);
                }

                this.WorkPanel.Width += dx;
                this.WorkPanel.Height += dy;
                this.Update();
            }
            public void MoveCenterTo(int x,int y)
            {
                Center.X = x;// this.WorkPanel.Width / 2;
                Center.Y = y;// this.WorkPanel.Height / 2;
                CenterRect = new Rectangle(Center.X - 4, Center.Y - 4, 8, 8);
                foreach (var sh in this.Shapes.Where(t => t.ShapeType == ShapeType.Parabolic))
                    sh.IsDirty = true;
                this.Update();
            }
            private void DrawTest(Graphics g)
            {
                float cx = this.Center.X;
                float cy = this.Center.Y;
                List<PointF> pts = new List<PointF>();
                for(float i=0;i<this.WorkPanel.Width;i++)
                {
                    var x=i - cx;//转换为中心点的x电脑屏幕坐标
                    x = x / ScaleRation;//转换为函数真实坐标
                    var y =(float)(Math.Pow(x,2)) * ScaleRation;

                    pts.Add(new PointF(i,cy- y));
                }
                g.DrawLines(Pens.Black, pts.ToArray());
            }
            private void WorkPanel_MouseWheel(object sender, MouseEventArgs ev)
            {
                MouseEventArgs e = new MouseEventArgs(ev.Button, ev.Clicks, ev.X + this.WorkPanel.HorizontalScroll.Value,
                                         ev.Y + this.WorkPanel.VerticalScroll.Value, ev.Delta);
                IShape findsh = null;
                foreach (var sh in Shapes.Where(t=>t.ShapeType== ShapeType.UserPicture).OrderByDescending(t => t.Id))
                {
                    if (sh.PtIn(e.Location))
                    {
                        findsh = sh;
                        break;
                    }
                }

                if (findsh != null)
                    findsh.MouseWheel(findsh, e);
                else   if (this.IsDrawCoordinate)
                {
                    float ration = 1;
                    if (ev.Delta > 0)
                    {
                        GridWH++;
                        ration = ScaleRation;
                        ScaleRation *= 1.1f;// 1.1f;//2
                        ration= ScaleRation / ration;
                    }
                    else
                    {
                        ration = ScaleRation;
                        ScaleRation *= 0.95f;//2
                        ration = ScaleRation / ration;
                        GridWH--;
                    }
                    if (GridWH > 80)
                        GridWH = 40;
                    if (GridWH < 40)
                        GridWH = 80;
                    if (ScaleRation <= 0)
                        ScaleRation = 1f;
                    foreach (var sh in this.Shapes.Where(t => t.ShapeType == ShapeType.Parabolic))
                        sh.IsDirty = true;

                    foreach (Dot dot in Shapes.Where(t => t.ShapeType == ShapeType.Dot))
                    {
                        dot.Scale(ration, this.Center);
                    }

                    this.Update();
                }
                else//test
                {
                    float ration = 0.2f;
                    if (ev.Delta > 0)
                    {
                        ration = 1.1f;
                        GridWH++;
                    }
                    else
                    {
                        ration = 0.9f;
                        GridWH--;
                    }
                    if (GridWH > 80)
                        GridWH = 40;
                    if (GridWH < 40)
                        GridWH = 80;

                    Point p = new Point(this.WorkPanel.Width / 2, this.WorkPanel.Height / 2);
                    foreach (Dot dot in Shapes.Where(t => t.ShapeType == ShapeType.Dot))
                    {
                        dot.Scale(ration, e.Location);
                    }
                    this.Update();
                }
            }

           
            private void DrawCoordinate(Graphics g)
            {
                Pen p = new Pen(Color.Black, 1);

                int w = this.WorkPanel.Width + this.WorkPanel.HorizontalScroll.Value;
                int h = this.WorkPanel.Height + this.WorkPanel.VerticalScroll.Value;

                int cx = Center.X;
                int cy = Center.Y;

                g.FillEllipse(Brushes.Red, CenterRect);

                double miniQty;
                int oneH;
                NewMethod(out miniQty, out oneH);

                int offsetY = 15;
                int i = 1;
                while (true)// 从中间往上走
                {
                    cy -= oneH;
                    if (cy < 0)
                        break;
                    g.DrawLine(Pens.Black, cx - 6, cy, cx + 6, cy);
                    if (i % 2 == 0)
                    {
                        var txt = miniQty * i + "";// (i * oneH / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width, cy - size.Height / 2);
                    }
                    i++;
                }

                // return;

                cx = Center.X;
                cy = Center.Y;
                i = -1;
                while (true)
                {
                    cy += oneH;
                    if (cy > h)
                        break;
                    g.DrawLine(Pens.Black, cx - 6, cy, cx + 6, cy);
                    if (i % 2 == 0)
                    {
                        var txt = miniQty * i + "";// (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width, cy - size.Height / 2);
                    }
                    i--;
                }
                //=====================================
                cx = Center.X;
                cy = Center.Y;
                i = -1;
                while (true)
                {//右走起
                    cx -= oneH;
                    if (cx < 0)
                        break;
                    g.DrawLine(Pens.Black, cx, cy - 6, cx, cy + 6);
                    if (i % 5 == 0)
                    {

                        var txt = miniQty * i + "";// (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width / 2, cy + offsetY);
                    }
                    i--;
                }
                i = 1;
                cx = Center.X;
                cy = Center.Y;
                while (true)
                {// | ->
                    cx += oneH;
                    if (cx > w)
                        break;
                    g.DrawLine(Pens.Black, cx, cy - 6, cx, cy + 6);
                    if (i % 5 == 0)
                    {
                        var txt = miniQty * i + "";// (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width / 2, cy + offsetY);
                    }
                    i++;
                }
                g.DrawLine(p, 0, Center.Y, w, Center.Y);//x
                g.DrawLine(p, Center.X, h, Center.X, 0);//y
                                                        //  DrawTest(g);
            }

            private void NewMethod(out double miniQty, out int oneH)
            {
                var pixPer_1 = 1 / ScaleRation;//一个像素代表数学上的多少，开始的是1代表1，放大，1->0.5
                var maxH = pixPer_1 * this.WorkPanel.Height / 2;//数学上最大的Y坐标
                var maxh2 = maxH;
                int c = 0;
                miniQty = 0;
                if (maxh2 <= 10)
                {
                    while (maxh2 < 10)
                    {
                        maxh2 *= 10;
                        c++;
                    }
                    miniQty = Math.Round(maxH / 10, c);//尽量去10份中的每一份的的数学高度的整数部分
                }
                else
                {
                    c = 0;
                  //  maxh2 /= 10;
                    while (maxh2 > 10)
                    {
                        maxh2 /= 10;
                        c++;
                    }
                    miniQty = Math.Round(maxH / Math.Pow(10, c)) / 10 * Math.Pow(10, c);//尽量去10份中的每一份的的数学高度的整数部分
                }

                oneH = (int)(miniQty / pixPer_1);
            }

            private void DrawCoordinatebak(Graphics g)
            {
                Pen p = new Pen(Color.Black, 1);
              
                int w = this.WorkPanel.Width + this.WorkPanel.HorizontalScroll.Value;
                int h = this.WorkPanel.Height + this.WorkPanel.VerticalScroll.Value;

                int cx = Center.X;
                int cy = Center.Y;

                g.FillEllipse(Brushes.Red, CenterRect);

             //   var Maxy=  this.WorkPanel.Height / 2f / ScaleRation;

                var pixPer_1=1/ScaleRation;//一个像素代表数学上的多少，开始的是1代表1，放大，1->0.5

                Console.WriteLine("pixPer_1="+ pixPer_1);
                int offsetY = 15;

                int i = 1;
                while (true)// 从中间往上走
                {
                    cy -= 40;
                    if (cy < 0)
                        break;
                    g.DrawLine(Pens.Black, cx - 6, cy, cx + 6, cy);
                   // if (i % 2 == 0)
                    {
                        var txt = (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width, cy - size.Height / 2);
                    }
                    i++;
                }


                cx = Center.X;
                cy = Center.Y;
                i = -1;
                while (true)
                {
                    cy += 40;
                    if (cy > h)
                        break;
                    g.DrawLine(Pens.Black, cx - 6, cy, cx + 6, cy);
                    if (i % 2 == 0)
                    {
                        var txt = (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width, cy - size.Height / 2);
                    }
                    i--;
                }
                //=====================================
                cx = Center.X;
                cy = Center.Y;
                i = -1;
                while (true)
                {//右走起
                    cx -= 40;
                    if (cx < 0)
                        break;
                    g.DrawLine(Pens.Black, cx, cy - 6, cx, cy + 6);
                    if (i % 5 == 0)
                    {
                     
                        var txt = (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width / 2, cy + offsetY);
                    }
                    i--;
                }
                i = 1;
                cx = Center.X;
                cy = Center.Y;
                while (true)
                {// | ->
                    cx += 40;
                    if (cx > w)
                        break;
                    g.DrawLine(Pens.Black, cx, cy - 6, cx, cy + 6);
                    if (i % 5 == 0)
                    {
                        var txt = (i * 40 / ScaleRation).ToString();
                        var size = TextRenderer.MeasureText(txt, Font);
                        g.DrawString(txt, Font, Brushes.Black, cx - size.Width / 2, cy + offsetY);
                    }
                    i++;
                }
                g.DrawLine(p, 0, Center.Y, w, Center.Y);//x
                g.DrawLine(p, Center.X,h, Center.X, 0);//y
              //  DrawTest(g);
            }
           
            
            private void WorkPanel_MouseUp(object sender, MouseEventArgs ee)
            {
                MouseEventArgs e = new MouseEventArgs(ee.Button, ee.Clicks, ee.X + this.WorkPanel.HorizontalScroll.Value,
                  ee.Y + this.WorkPanel.VerticalScroll.Value, ee.Delta);

                if (e.Button == MouseButtons.Left)
                {
                    if (this.DrawMode == DrawMode.ManulWrite)
                    {
                        if (ManulG != null)
                        {
                            ManulG.Dispose();
                            ManulG = null;
                        }
                        ManualPen = null;
                    }

                    if (IsMoveCenter)
                    {
                        IsMoveCenter = false;
                        SelectPointFrom = Point.Empty;
                        foreach (Parabolic sh in this.Shapes.Where(t => t.ShapeType == ShapeType.Parabolic))
                        {
                            sh.IsDirty = true;
                        }
                        this.Update();
                        return;
                    }

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
                }

                IsDraged = false;
                foreach (var sh in DownShapes)
                    sh.MouseUp(sh, e);
                DownShapes.Clear();
            }

            private Dot srcDot = null;
            private Polygon DrawingPolygon = null;
            private ManualPen ManualPen = null;
            private Graphics ManulG = null;
            private void WorkPanel_MouseDown(object sender, MouseEventArgs ee)
            {
                this.WorkPanel.Focus();
                MouseEventArgs e = new MouseEventArgs(ee.Button, ee.Clicks, ee.X+this.WorkPanel.HorizontalScroll.Value,
                    ee.Y+this.WorkPanel.VerticalScroll.Value, ee.Delta);

                if (e.Button == MouseButtons.Left )
                {
                    if (DrawMode == DrawMode.Line)
                    {
                        LineAtthced(e.Location);
                    }
                    else if (DrawMode == DrawMode.Dot)
                    {
                        var dotTo = new Dot() { X = e.Location.X, Y = e.Location.Y };
                        this.AddShape(dotTo);
                        this.Update();
                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(dotTo, true));
                        this.RevokeAllObjects.Push(unit);
                    }
                   
                    else if (DrawMode == DrawMode.Circle)
                    {
                        AttchhedCircle(e.Location);
                    }
                    else if (DrawMode == DrawMode.Triangle)
                    {
                        if (PrePoint.IsEmpty)
                        {
                            PrePoint = e.Location;
                            srcDot = Shapes.Where(s => s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                        }
                        else
                        {
                            DrawTriangle();
                            PrePoint = Point.Empty;
                            pendingTriangle = null;
                        }
                    }
                    else if (DrawMode == DrawMode.TextArea)
                    {
                        this.EditRichBox.Location = e.Location;
                        this.EditRichBox.Height =  TextRenderer.MeasureText("我", this.EditRichBox.Font).Height+15;
                        var text = new TextArea() { X = e.X, Y = e.Y, Text = " ",};
                        this.EditRichBox.Tag = text;
                        this.WorkPanel.Controls.Add(this.EditRichBox);
                        this.EditRichBox.Show();
                        this.AddShape(text);
                        this.EditRichBox.Focus();
                        text.IsActive = true;
                        DrawMode = DrawMode.None;
                    }
                    else if (DrawMode == DrawMode.Rect)
                    {
                        if (DrawRectangle == Rectangle.Empty)
                        {
                            RectPointFrom = e.Location;
                            DrawRectangle = new Rectangle(e.Location.X, e.Location.Y, 1, 1);
                        }
                        else
                        {
                            DrawRect();
                            DrawRectangle = Rectangle.Empty;
                        }
                    }
                    else if (DrawMode == DrawMode.Polygon)
                    {
                        if (DrawingPolygon == null)
                        {
                            DrawingPolygon = new Polygon();
                            DoUnit unit = new DoUnit();
                            unit.Units.Push(new KeyValuePair<IShape, object>(DrawingPolygon, true));
                            this.RevokeAllObjects.Push(unit);
                            this.AddShape(DrawingPolygon);
                            this.Update();
                            DrawingPolygon.AddDot(new Dot() { X = e.X, Y = e.Y});
                            DrawingPolygon.pendingPoint = e.Location;
                        }
                        else
                        {
                            DrawingPolygon.AddDot(new Dot() { X = e.X, Y = e.Y });
                            DrawingPolygon.pendingPoint = e.Location;
                        }                      
                    }
                    else if(DrawMode== DrawMode.ManulWrite)
                    {
                        ManulG = Graphics.FromHwnd(this.WorkPanel.Handle);
                        ManulG.SmoothingMode = SmoothingMode.HighQuality;
                        ManualPen = new ManualPen();
                        ManualPen.Points.Add(e.Location);
                        this.AddShape(ManualPen);
                        this.Update();
                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(ManualPen, true));
                        this.RevokeAllObjects.Push(unit);
                    }
                    else  if (this.IsDrawCoordinate && this.CenterRect.Contains(e.Location))
                    {
                        IsMoveCenter = true;
                        SelectPointFrom = e.Location;
                    }
                    else
                    {
                        var sh = Shapes.Where(s => s.IsDraged == false && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault();
                        if (sh != null)
                        {
                            DownShapes.Add(sh);
                            sh.MouseDown(sh, e);
                            if(sh.ShapeType== ShapeType.Dot)
                            {
                                this.SelectRectangle = Rectangle.Empty;
                                foreach (var s in this.Shapes)
                                    s.IsActive = false;
                            }
                        }
                        else
                        {//空白除点鼠标左键，画选择区域
                            SelectPointFrom = e.Location;

                            //if (this.IsDrawCoordinate && this.CenterRect.Contains(e.Location))
                            //{
                            //    IsMoveCenter = true;
                            //}
                            //else
                            {

                                this.SelectRectangle = Rectangle.Empty;
                                foreach (var s in this.Shapes)
                                    s.IsActive = false;
                            }
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
                        this.WorkPanel.Invalidate();
                    }
                    else if(DrawMode== DrawMode.Circle)
                    {
                        pendingCircle = null;
                        PrePoint=Point.Empty;
                        this.WorkPanel.Invalidate();
                    }
                    else if(DrawMode== DrawMode.Polygon)
                    {
                        DrawingPolygon = null;
                        this.WorkPanel.Invalidate();
                    }
                    else if(DrawMode== DrawMode.Rect)
                    {
                        DrawRectangle = Rectangle.Empty;
                        this.WorkPanel.Invalidate();
                    }
                    else if (DrawMode == DrawMode.Triangle)
                    {
                        pendingTriangle = null;
                    }
                    else if(DrawMode== DrawMode.ManulWrite)
                    {
                        ManualPen = null;
                    }
                    else//正常触发鼠标右键事件
                    {
                        DrawMode = DrawMode.None;
                        this.WorkPanel.Cursor = Cursors.Default;
                        var sh = Shapes.Where(s =>  s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault();
                        if (sh != null)
                        {
                            //if (sh.ShapeType == ShapeType.Dot)
                            //{
                            //    ShowEdit(sh);//先去掉右键鼠标编辑点功能，冲突啊
                            //    return;
                            //}
                            //else
                            {
                                DownShapes.Add(sh);
                                sh.MouseDown(sh, e);
                            }
                        }
                        this.ContextPopMenu.Show(e.Location);
                    }

                    DrawMode = DrawMode.None;
                    this.WorkPanel.Cursor = Cursors.Default;
                }
            }

            private void AttchhedCircle(Point p)
            {
                if (PrePoint.IsEmpty)
                {
                    PrePoint = p;
                    srcDot = Shapes.Where(s => s.PtIn(p)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                }
                else
                {
                    DoUnit unit = new DoUnit();

                    if (srcDot == null)
                    {
                        srcDot = new Dot() { X = PrePoint.X, Y = PrePoint.Y };
                        unit.Units.Push(new KeyValuePair<IShape, object>(srcDot, true));
                        this.AddShape(srcDot);
                    }

                    var circle = new Circle() { Center = srcDot, Location = srcDot.P, Diam = pendingCircle.Diam };
                    unit.Units.Push(new KeyValuePair<IShape, object>(circle, true));
                    this.RevokeAllObjects.Push(unit);
                    this.AddShape(circle);
                    this.Update();
                    PrePoint = Point.Empty;
                    pendingCircle = null;
                }
            }

            private void FillAreaItem_Click(object sender, EventArgs e)
            {
                this.FillArea(openingPos);
            }

            private void RemAngleItem_Click(object sender, EventArgs e)
            {
                this.RemoAngle(openingPos);
            }

            private Point openingPos = Point.Empty;
            private void ContextPopMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
            {
                openingPos = Control.MousePosition;
                openingPos = WorkPanel.PointToClient(openingPos);
                pasteItem.Enabled= Clipboard.GetDataObject() != null;
                bool isselected=this.Shapes.FirstOrDefault(t => t.IsActive)!=null;
                deleteItem.Visible = isselected;
                CopyItem.Visible = isselected;
                CutItem.Visible = isselected;
                firstItem.Visible = isselected;
                lastItem.Visible = isselected;
            }

            private void DeleteItem_Click(object sender, EventArgs e)
            {
                DeleteSelected();
            }

            private void PasteItem_Click(object sender, EventArgs e)
            {
                this.Paste();
            }

            private void CutItem_Click(object sender, EventArgs e)
            {
                this.Cut();
            }

            private void CopyItem_Click(object sender, EventArgs e)
            {
                this.CloneData();
            }

            private void LastItem_Click(object sender, EventArgs e)
            {
                var sh=this.Shapes.FirstOrDefault(s => s.IsActive);
                sh.Id = -1;
                int id = 0;
                foreach (var s in Shapes.OrderBy(t => t.Id))
                {
                    s.Id = id;
                    ++id;
                }
                this.Update();
            }

            private void FirstItem_Click(object sender, EventArgs e)
            {
                var sh = this.Shapes.FirstOrDefault(s => s.IsActive);
                sh.Id = int.MaxValue;
                int id = 0;
                foreach (var s in Shapes.OrderBy(t => t.Id))
                {
                    s.Id = id;
                    ++id;
                }
                this.Update();
            }

            private void DrawShape_Click(object sender, EventArgs e)
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                this.DrawMode = (DrawMode)Convert.ToInt32(item.Tag.ToString());
                if(this.DrawMode== DrawMode.Line)
                {
                    LineAtthced(openingPos);
                }
                else if (this.DrawMode == DrawMode.Circle)
                {
                    AttchhedCircle(openingPos);
                }
            }

            private bool IsMoveCenter = false;
            public string FilePathName { get; set; } = "未命名.json";

            public void LoadFile(string filePathName)
            {
                var jsonstr = System.IO.File.ReadAllText(filePathName);
               
                var kvs = JsonConvert.DeserializeObject<Dictionary<ShapeType, string>>(jsonstr);
                List<IShape> shapes = new List<IShape>();
              
                foreach (var kv in kvs)
                {
                    if (kv.Key == ShapeType.Dot)
                    {
                        var dots = JsonConvert.DeserializeObject<List<Dot>>(kv.Value);
                        shapes.AddRange(dots);
                    }
                    else if (kv.Key == ShapeType.Line)
                    {
                        var lines = JsonConvert.DeserializeObject<List<Line>>(kv.Value);
                        shapes.AddRange(lines);
                    }
                    else if (kv.Key == ShapeType.Circle)
                    {
                        var circles = JsonConvert.DeserializeObject<List<Circle>>(kv.Value);
                        shapes.AddRange(circles);
                    }
                    else if (kv.Key == ShapeType.Polygon)
                    {
                        var plys = JsonConvert.DeserializeObject<List<Polygon>>(kv.Value);
                        shapes.AddRange(plys);
                    }
                    else if (kv.Key == ShapeType.RemAngle)
                    {
                        var remAngles = JsonConvert.DeserializeObject<List<RemAngle>>(kv.Value);
                        shapes.AddRange(remAngles);
                    }
                    else if (kv.Key == ShapeType.TextArea)
                    {
                        var ts = JsonConvert.DeserializeObject<List<TextArea>>(kv.Value);
                        shapes.AddRange(ts);
                    }
                    else if(kv.Key==ShapeType.FillArea)
                    {
                        var fillAreas = JsonConvert.DeserializeObject<List<FillArea>>(kv.Value);
                        shapes.AddRange(fillAreas);
                    }
                    else if (kv.Key == ShapeType.ManualPen)
                    {
                        var mpens = JsonConvert.DeserializeObject<List<ManualPen>>(kv.Value);
                        shapes.AddRange(mpens);
                    }
                    else if (kv.Key == ShapeType.Parabolic)
                    {
                        var mpens = JsonConvert.DeserializeObject<List<Parabolic>>(kv.Value);
                        shapes.AddRange(mpens);
                    }
                    else if (kv.Key == ShapeType.UserPicture)
                    {
                        var useImgs = JsonConvert.DeserializeObject<List<UserPicture>>(kv.Value);
                        shapes.AddRange(useImgs);
                    }
                }

                foreach (var sh in shapes.Where(t=>t.ShapeType== ShapeType.Dot))
                {
                    var dot = sh as Dot;
                    if (dot.Lines != null && dot.Lines.Count > 0)
                    {
                        List<Line> lines = new List<Line>();
                        foreach (var l in dot.Lines)
                        {
                            var line = shapes.FirstOrDefault(t => t.Id == l.Id) as Line;
                            if (line != null)
                                lines.Add(line);
                        }
                        dot.Lines = lines;
                    }

                    if (dot.Circles != null && dot.Circles.Count > 0)
                    {
                        List<Circle> circles = new List<Circle>();
                        foreach (var c in dot.Circles)
                        {
                            var circle = shapes.FirstOrDefault(t => t.Id == c.Id) as Circle;
                            if (circle != null)
                                circles.Add(circle);
                        }
                        dot.Circles = circles;
                    }

                    //if (dot.RemAngles != null && dot.RemAngles.Count > 0)
                    //{
                    //    List<RemAngle> remAngles = new List<RemAngle>();
                    //    foreach (var c in dot.Circles)
                    //    {
                    //        var remAngle = shapes.FirstOrDefault(t => t.Id == c.Id) as RemAngle;
                    //        if (remAngle != null)
                    //            remAngles.Add(remAngle);
                    //    }
                    //    dot.RemAngles = remAngles;
                    //}

                    if (dot.LockCirle != null)
                    {
                        dot.LockCirle = shapes.FirstOrDefault(t => t.Id == dot.LockCirle.Id) as Circle;
                    }

                    if (dot.LockLine != null)
                    {
                        dot.LockLine = shapes.FirstOrDefault(t => t.Id == dot.LockLine.Id) as Line;
                    }
                }
              
                foreach (var sh in shapes.Where(t => t.ShapeType == ShapeType.Line))
                {
                    var line = sh as Line;
                    if (line.From != null)
                        line.From = shapes.FirstOrDefault(t => t.Id == line.From.Id) as Dot;
                    if (line.To != null)
                    {
                        var to= shapes.FirstOrDefault(t => t.Id == line.To.Id) as Dot;
                        if(to==null)
                        {

                        }
                        else 
                            line.To = to; 
                    }
                }
               
                foreach (var sh in shapes)
                {
                    sh.Canvas = this;
                    sh.IsDirty = true;
                    sh.CanvasCtl = this.WorkPanel;

                    if (sh.ShapeType == ShapeType.Circle)
                    {
                        var circle = sh as Circle;
                        if (circle.Center != null)
                            circle.Center = shapes.FirstOrDefault(t => t.Id == circle.Center.Id) as Dot;
                    }
                    else if (sh.ShapeType == ShapeType.Polygon)
                    {
                        var poly = sh as Polygon;
                        if (poly.Lines != null && poly.Lines.Count > 0)
                        {
                            List<Line> lines = new List<Line>();
                            foreach (var l in poly.Lines)
                            {
                                var line = shapes.FirstOrDefault(t => t.Id == l.Id) as Line;
                                if (line != null)
                                    lines.Add(line);
                            }
                            poly.Lines = lines;
                        }
                        if (poly.Dots != null && poly.Dots.Count > 0)
                        {
                            List<Dot> dots = new List<Dot>();
                            foreach (var d in poly.Dots)
                            {
                                var dot = shapes.FirstOrDefault(t => t.Id == d.Id) as Dot;
                                if (dot != null)
                                    dots.Add(dot);
                            }
                            poly.Dots = dots;
                        }
                    }

                    else if (sh.ShapeType == ShapeType.RemAngle)
                    {
                        var remAngle = sh as RemAngle;
                        if (remAngle.FromLine != null)
                            remAngle.FromLine = shapes.FirstOrDefault(t => t.Id == remAngle.FromLine.Id) as Line;
                        if (remAngle.ToLine != null)
                            remAngle.ToLine = shapes.FirstOrDefault(t => t.Id == remAngle.ToLine.Id) as Line;
                        if (remAngle.Dot != null)
                            remAngle.Dot = shapes.FirstOrDefault(t => t.Id == remAngle.Dot.Id) as Dot;
                    }
                    else if (sh.ShapeType == ShapeType.FillArea)
                    {
                        var fill = sh as FillArea;
                        if (fill.Dots != null && fill.Dots.Count > 0)
                        {
                            List<Dot> dots = new List<Dot>();
                            foreach (var dot in fill.Dots)
                            {
                                var d = shapes.FirstOrDefault(t => t.Id == dot.Id) as Dot;
                                if (d != null)
                                    dots.Add(d);
                            }
                            fill.Dots = dots;
                        }
                    }
                    else if (sh.ShapeType == ShapeType.ManualPen)
                    {
                        var mpen = sh as ManualPen;
                    }
                }
                
                this.ClearShapes();

                if (shapes.Count > 0)
                {
                    var maxObj = shapes.OrderByDescending(t => t.Id).FirstOrDefault();
                    ID = maxObj.Id + 1;
                }
                else
                    ID = 0;

                this.Shapes.AddRange(shapes);
                foreach(var sh in shapes)
                    sh.ActivChanged += this.Shape_ActivChanged;

                this.Update();
                FilePathName = filePathName;
                this.IsAdd = false;
                this.WorkPanel.FindForm().Text = this.FilePathName;
            }

            public bool IsMoified { get; private set; } = false;
            public bool IsAdd { get; private set; } = true;

            public void Save()
            {
                this.SaveFile(this.FilePathName);
            }
            public void Paste()
            {
                if (Clipboard.ContainsImage())
                {
                    Image img = Clipboard.GetImage();
                    var p = Control.MousePosition;
                    p = WorkPanel.PointToClient(p);
                    this.AddShape(new UserPicture() { Image = img, X = p.X, Y = p.Y ,Width=img.Width,Height=img.Height});
                    this.Update();
                    return;
                }

                string format = typeof(Dictionary<ShapeType, string>).FullName;
                var dataObj = Clipboard.GetDataObject();
                Dictionary<ShapeType, string> kvs = null;
                if (dataObj.GetDataPresent(format))
                {
                    kvs = dataObj.GetData(format) as Dictionary<ShapeType, string>;
                }
                else 
                    return;

                if (kvs == null)
                    return;
                List<IShape> shapes = new List<IShape>();
                foreach (var kv in kvs)
                {
                    if (kv.Key == ShapeType.Dot)
                    {
                        var dots = JsonConvert.DeserializeObject<List<Dot>>(kv.Value);
                        shapes.AddRange(dots);
                    }
                    else if (kv.Key == ShapeType.Line)
                    {
                        var lines = JsonConvert.DeserializeObject<List<Line>>(kv.Value);
                        shapes.AddRange(lines);
                    }
                    else if (kv.Key == ShapeType.Circle)
                    {
                        var circles = JsonConvert.DeserializeObject<List<Circle>>(kv.Value);
                        shapes.AddRange(circles);
                    }
                    else if (kv.Key == ShapeType.Polygon)
                    {
                        var plys = JsonConvert.DeserializeObject<List<Polygon>>(kv.Value);
                        shapes.AddRange(plys);
                    }
                    else if (kv.Key == ShapeType.RemAngle)
                    {
                        var remAngles = JsonConvert.DeserializeObject<List<RemAngle>>(kv.Value);
                        shapes.AddRange(remAngles);
                    }
                    else if (kv.Key == ShapeType.TextArea)
                    {
                        var ts = JsonConvert.DeserializeObject<List<TextArea>>(kv.Value);
                        shapes.AddRange(ts);
                    }
                    else if (kv.Key == ShapeType.FillArea)
                    {
                        var fillAreas = JsonConvert.DeserializeObject<List<FillArea>>(kv.Value);
                        shapes.AddRange(fillAreas);
                    }
                    else if (kv.Key == ShapeType.ManualPen)
                    {
                        var mpens = JsonConvert.DeserializeObject<List<ManualPen>>(kv.Value);
                        shapes.AddRange(mpens);
                    }
                    else if (kv.Key == ShapeType.Parabolic)
                    {
                        var mpens = JsonConvert.DeserializeObject<List<Parabolic>>(kv.Value);
                        shapes.AddRange(mpens);
                    }
                }

                foreach (var sh in shapes.Where(t => t.ShapeType == ShapeType.Dot))
                {
                    var dot = sh as Dot;
                    if (dot.Lines != null && dot.Lines.Count > 0)
                    {
                        List<Line> lines = new List<Line>();
                        foreach (var l in dot.Lines)
                        {
                            var line = shapes.FirstOrDefault(t => t.Id == l.Id) as Line;
                            if (line != null)
                                lines.Add(line);
                        }
                        dot.Lines = lines;
                    }

                    if (dot.Circles != null && dot.Circles.Count > 0)
                    {
                        List<Circle> circles = new List<Circle>();
                        foreach (var c in dot.Circles)
                        {
                            var circle = shapes.FirstOrDefault(t => t.Id == c.Id) as Circle;
                            if (circle != null)
                                circles.Add(circle);
                        }
                        dot.Circles = circles;
                    }

                    //if (dot.RemAngles != null && dot.RemAngles.Count > 0)
                    //{
                    //    List<RemAngle> remAngles = new List<RemAngle>();
                    //    foreach (var c in dot.Circles)
                    //    {
                    //        var remAngle = shapes.FirstOrDefault(t => t.Id == c.Id) as RemAngle;
                    //        if (remAngle != null)
                    //            remAngles.Add(remAngle);
                    //    }
                    //    dot.RemAngles = remAngles;
                    //}

                    if (dot.LockCirle != null)
                    {
                        dot.LockCirle = shapes.FirstOrDefault(t => t.Id == dot.LockCirle.Id) as Circle;
                    }

                    if (dot.LockLine != null)
                    {
                        dot.LockLine = shapes.FirstOrDefault(t => t.Id == dot.LockLine.Id) as Line;
                    }
                }

                List<IShape> shouldShapes = new List<IShape>();
                foreach (var sh in shapes.Where(t => t.ShapeType == ShapeType.Line))
                {
                    var line = sh as Line;
                    if (line.From != null)
                    {
                        var from = shapes.FirstOrDefault(t => t.Id == line.From.Id) as Dot;
                        if (from == null)
                        {
                            from=this.Shapes.FirstOrDefault(t => t.Id == line.From.Id) as Dot;
                            if (from != null)
                            {
                                line.From = from.Clone() as Dot;
                                line.From.LockLine = null;
                                line.From.LockCirle = null;
                                line.From.Lines.Clear();
                                line.From.Lines.Add(line);
                                shouldShapes.Add(line.From);
                            }
                        }
                        else
                            line.From = from;
                    }
                    if (line.To != null)
                    {
                        var to = shapes.FirstOrDefault(t => t.Id == line.To.Id) as Dot;
                        if (to == null)
                        {
                            to = this.Shapes.FirstOrDefault(t => t.Id == line.To.Id) as Dot;
                            if (to != null)
                            {
                                line.To = to.Clone() as Dot;
                                line.To.LockLine = null;
                                line.To.LockCirle = null;
                                line.To.Lines.Clear();
                                line.To.Lines.Add(line);
                                shouldShapes.Add(line.To);
                            }
                        }
                        else
                            line.To = to;
                    }
                }
                shapes.AddRange(shouldShapes);

                foreach (var sh in shapes)
                {
                    sh.Canvas = this;
                    sh.IsDirty = true;
                    sh.CanvasCtl = this.WorkPanel;

                    if (sh.ShapeType == ShapeType.Circle)
                    {
                        var circle = sh as Circle;
                        if (circle.Center != null)
                            circle.Center = shapes.FirstOrDefault(t => t.Id == circle.Center.Id) as Dot;
                    }
                    else if (sh.ShapeType == ShapeType.Polygon)
                    {
                        var poly = sh as Polygon;
                        if (poly.Lines != null && poly.Lines.Count > 0)
                        {
                            List<Line> lines = new List<Line>();
                            foreach (var l in poly.Lines)
                            {
                                var line = shapes.FirstOrDefault(t => t.Id == l.Id) as Line;
                                if (line != null)
                                    lines.Add(line);
                            }
                            poly.Lines = lines;
                        }
                        if (poly.Dots != null && poly.Dots.Count > 0)
                        {
                            List<Dot> dots = new List<Dot>();
                            foreach (var d in poly.Dots)
                            {
                                var dot = shapes.FirstOrDefault(t => t.Id == d.Id) as Dot;
                                if (dot != null)
                                    dots.Add(dot);
                            }
                            poly.Dots = dots;
                        }
                    }

                    else if (sh.ShapeType == ShapeType.RemAngle)
                    {
                        var remAngle = sh as RemAngle;
                        if (remAngle.FromLine != null)
                            remAngle.FromLine = shapes.FirstOrDefault(t => t.Id == remAngle.FromLine.Id) as Line;
                        if (remAngle.ToLine != null)
                            remAngle.ToLine = shapes.FirstOrDefault(t => t.Id == remAngle.ToLine.Id) as Line;
                        if (remAngle.Dot != null)
                            remAngle.Dot = shapes.FirstOrDefault(t => t.Id == remAngle.Dot.Id) as Dot;
                    }
                    else if (sh.ShapeType == ShapeType.FillArea)
                    {
                        var fill = sh as FillArea;
                        if (fill.Dots != null && fill.Dots.Count > 0)
                        {
                            List<Dot> dots = new List<Dot>();
                            foreach (var dot in fill.Dots)
                            {
                                var d = shapes.FirstOrDefault(t => t.Id == dot.Id) as Dot;
                                if (d != null)
                                    dots.Add(d);
                            }
                            fill.Dots = dots;
                        }
                    }
                    else if (sh.ShapeType == ShapeType.ManualPen)
                    {
                        var mpen = sh as ManualPen;
                    }
                }

                DoUnit unit = new DoUnit();
                foreach (var sh in shapes.OrderByDescending(t => t.Id))
                {//重建立ID及对应关系
                    sh.Id = GetUID;
                    unit.Units.Push(new KeyValuePair<IShape, object>(sh, true));
                }
               
                var dotList = shapes.Where(t => t.ShapeType == ShapeType.Dot).ToList();
                if (dotList.Count > 0)
                {
                    float x = 0, y = 0;
                    int count = 0;
                    foreach (Dot sh in dotList)
                    {//重xin
                        x += sh.X;
                        y += sh.Y;
                        ++count;
                    }
                    var cx = x / count;
                    var cy = y / count;

                    var p = Control.MousePosition;
                    p = WorkPanel.PointToClient(p);

                    var dx = p.X - cx;
                    var dy = p.Y - cy;
                    foreach (Dot d in dotList)
                    {
                        d.Move(dx, dy);
                    }
                }

                this.Shapes.AddRange(shapes);
                foreach (var sh in shapes)
                    sh.ActivChanged += this.Shape_ActivChanged;
                this.RevokeAllObjects.Push(unit);
                this.Update();
            }
            public bool Cut()
            {
                if (CloneData())
                {
                    var list = this.Shapes.Where(t => t.IsActive).ToList();
                    foreach (var sh in list)
                    {
                        sh.ActivChanged -= Shape_ActivChanged;
                        this.Shapes.Remove(sh);

                        DoUnit unit = new DoUnit();
                        unit.Units.Push(new KeyValuePair<IShape, object>(sh, false));
                        this.RevokeAllObjects.Push(unit);
                    }
                    this.Update();
                    return true;
                }
                return false;
            }
            public bool CloneData()
            {
                if (this.Shapes.Where(t => t.IsActive).FirstOrDefault() == null)
                    return false;
                StringBuilder sbd = new StringBuilder();
                Dictionary<ShapeType, string> wrapObject = new Dictionary<ShapeType, string>();

                for (ShapeType i = ShapeType.Dot; i < ShapeType.Last; i++)
                {
                    sbd.Clear();
                    sbd.Append("[");
                    foreach (var d in this.Shapes.Where(t => t.ShapeType == i&&t.IsActive))
                    {
                        if (sbd.Length > 1)
                            sbd.Append(",");
                        sbd.Append(d.SerializeObject());
                    }
                    sbd.Append("]");
                    if (sbd.Length > 2)
                        wrapObject.Add(i, sbd.ToString());
                }
                Clipboard.SetDataObject(wrapObject);
                return true;
            }

            public void SaveFile(string filePathName)
            {
                //JsonSerializerSettings settings = new JsonSerializerSettings();
                //settings.Formatting = Formatting.Indented;
                //settings.MaxDepth = 8; //设置序列化的最大层数
                //settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;//指定如何处理循环引用，None--不序列化，Error-抛出异常，Serialize--仍要序列化

                ////   Newtonsoft.Json.ReferenceLoopHandling.Ignore
                ////  options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                //var jsStr = JsonConvert.SerializeObject(this.Shapes, settings);
                // JsonConvert.SerializeObject(this.Shapes);

                //var d= this.Shapes.FirstOrDefault(sh => sh.ShapeType == ShapeType.Dot) as Dot;
                //var jsStr = d.SerializeObject();
                //var res = JsonConvert.DeserializeObject<Dot>(jsStr);

                //List<Point> ps = new List<Point>();
                //ps.Add(new Point(1, 2));
                //ps.Add(new Point(3, 4));
                //var tt= JsonConvert.SerializeObject(ps);

                StringBuilder sbd = new StringBuilder();
                Dictionary<ShapeType, string> wrapObject = new Dictionary<ShapeType, string>();

                for (ShapeType i = ShapeType.Dot; i < ShapeType.Last; i++)
                {
                    sbd.Clear();
                    sbd.Append("[");
                    foreach (var d in this.Shapes.Where(t => t.ShapeType == i))
                    {
                        if (sbd.Length > 1)
                            sbd.Append(",");
                        sbd.Append(d.SerializeObject());
                    }
                    sbd.Append("]");
                    if (sbd.Length > 2)
                        wrapObject.Add(i, sbd.ToString());
                }

                var jsStr = JsonConvert.SerializeObject(wrapObject);
                System.IO.File.WriteAllText(filePathName, jsStr);
                FilePathName = filePathName;
                this.WorkPanel.FindForm().Text = this.FilePathName;
                IsAdd = false;
                this.IsMoified = false;
            }

            private void DrawTriangle()
            {
                var lt = srcDot==null? new Dot() { X = pendingTriangle.F.X, Y = pendingTriangle.F.Y } :srcDot;
                var rt = new Dot() { X = pendingTriangle.T.X, Y = pendingTriangle.T.Y };
                var rb = new Dot() { X = pendingTriangle.L.X, Y = pendingTriangle.L.Y };
                DoUnit unit = new DoUnit();
                this.AddShape(lt);
                this.AddShape(rt);
                this.AddShape(rb);
                if (srcDot == null)
                    unit.Units.Push(new KeyValuePair<IShape, object>(lt, true));

                unit.Units.Push(new KeyValuePair<IShape, object>(rt, true));
                unit.Units.Push(new KeyValuePair<IShape, object>(rb, true));

                var line = new Line() { From = lt, To = rt };
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                this.AddShape(line);
                line = new Line() { From = rt, To = rb };
                this.AddShape(line);
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                line = new Line() { From = rb, To = lt };
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                this.AddShape(line);
                this.Update();
                this.RevokeAllObjects.Push(unit);
            }
            private void DrawRect()
            {
                var lt = new Dot() { X = DrawRectangle.Left, Y = DrawRectangle.Top};
                var rt = new Dot() { X = DrawRectangle.Right, Y = DrawRectangle.Top};
                var rb = new Dot() { X = DrawRectangle.Right, Y = DrawRectangle.Bottom };
                var lb = new Dot() { X = DrawRectangle.Left, Y = DrawRectangle.Bottom };

                DoUnit unit = new DoUnit();

                this.AddShape(lt);
                this.AddShape(rt);
                this.AddShape(rb);
                this.AddShape(lb);
                unit.Units.Push(new KeyValuePair<IShape, object>(lt, true));
                unit.Units.Push(new KeyValuePair<IShape, object>(rt, true));
                unit.Units.Push(new KeyValuePair<IShape, object>(rb, true));
                unit.Units.Push(new KeyValuePair<IShape, object>(lb, true));

                var line = new Line() { From = lt, To = rt };
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                this.AddShape(line);
                line = new Line() { From = rt, To = rb };
                this.AddShape(line);
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                line = new Line() { From = rb, To = lb };
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                this.AddShape(line);

                line = new Line() { From = lb, To = lt };
                unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                this.AddShape(line);
                this.Update();
                this.RevokeAllObjects.Push(unit);
            }

            private void LineAtthced(Point p)
            {
                if (PrePoint.IsEmpty)
                {
                    srcDot = Shapes.Where(s => s.PtIn(p)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                    if (srcDot == null)
                    {
                        PrePoint = p;
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
                        srcDot = new Dot() { X = PrePoint.X, Y = PrePoint.Y};
                        this.AddShape(srcDot);
                       
                        unit.Units.Push(new KeyValuePair<IShape, object>(srcDot, true));
                    }

                    var destDot = Shapes.Where(s => s.ShapeType == ShapeType.Dot && s.PtIn(p)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                    if (destDot != null)//连接到了目地Dot
                    {
                        var line = new Line() { From = srcDot, To = destDot };
                        destDot.IsDirty = true;
                        this.AddShape(line);

                        unit.Units.Push(new KeyValuePair<IShape, object>(line, true));
                        this.RevokeAllObjects.Push(unit);

                        PrePoint = destDot.P;// e.Location;
                        srcDot = destDot;
                    }
                    else
                    {
                        var destLine = Shapes.Where(s => s.ShapeType == ShapeType.Line && s.PtIn(p)).OrderByDescending(t => t.Id).FirstOrDefault() as Line;
                        if (destLine != null)
                        {
                            if (destLine.IsInMidRang())
                            {
                                destDot = new Dot() { X = destLine.MidPoint.X, Y = destLine.MidPoint.Y };
                                destDot.LockLineRation = 0.5f;
                                PrePoint = destLine.MidPoint;
                            }
                            else
                            {
                                destDot = new Dot() { X = p.X, Y = p.Y };
                                var dx = destLine.To.X - destLine.From.X;
                                var dy = destLine.To.Y - destLine.From.Y;
                                if (dx == 0)
                                {
                                    var ey = p.Y - destLine.From.Y;
                                    destDot.LockLineRation = ey / dy;
                                }
                                else if (dy == 0)
                                {
                                    var ex = p.X - destLine.From.X;
                                    destDot.LockLineRation = ex / dx;
                                }
                                else
                                {
                                    var ex = p.X - destLine.From.X;
                                    destDot.LockLineRation = ex / dx;
                                }
                                PrePoint = p;
                            }
                            destDot.LockLine = destLine;
                        }
                        else
                        {
                            destDot = new Dot() { X = p.X, Y = p.Y };
                            var destCircle = Shapes.Where(s => s.ShapeType == ShapeType.Circle && s.PtIn(p)).OrderByDescending(t => t.Id).FirstOrDefault() as Circle;
                            if (destCircle != null)
                            {//判断圆上
                                destDot.LockCirle = destCircle;
                                destDot.LockCircleAngle = (float)(Math.PI * MyMath.Get0_360FromByPos(p, destCircle.Center.P) / 180f);
                            }
                            PrePoint = p;
                        }

                        var line = new Line() { From = srcDot, To = destDot };
                        this.AddShape(destDot);
                        this.AddShape(line);

                        unit.Units.Push(new KeyValuePair<IShape, object>(destDot, true));
                        unit.Units.Push(new KeyValuePair<IShape, object>(line, true));

                        this.RevokeAllObjects.Push(unit);
                        srcDot = destDot;
                        this.Update();
                    }
                }
            }

            PendingCircle pendingCircle = null;
            PendingTriangle pendingTriangle = null;
            private class PendingCircle
            {
                public float Diam;
                public PointF LT;//包括的矩形lefttop
                public Rectangle Rec;//圆心区域
                public Point Center;//
                public Point MousePt;//当前鼠标位置
            }
            private class PendingTriangle
            {
                public Point F;
                public Point T;
                public Point L;
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
           
            private void WorkPanel_MouseMove(object sender, MouseEventArgs ev)
            {
                MouseEventArgs e = new MouseEventArgs(ev.Button, ev.Clicks, ev.X + this.WorkPanel.HorizontalScroll.Value,
                    ev.Y + this.WorkPanel.VerticalScroll.Value, ev.Delta);

                if (DrawMode== DrawMode.Line)
                {
                    if (!PrePoint.IsEmpty&&(e.Location.X!=PrePoint.X||e.Y!=PrePoint.Y))
                    {
                        DotAttching(e);
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
                else if(DrawMode== DrawMode.Polygon)
                {
                    if (DrawingPolygon != null)
                    {
                        DrawingPolygon.pendingPoint = e.Location;
                        this.WorkPanel.Invalidate();
                    }
                }
                else if(DrawMode== DrawMode.Triangle)
                {
                    if (!PrePoint.IsEmpty)
                    {
                        if (pendingTriangle == null)
                        {
                            pendingTriangle = new PendingTriangle();
                            pendingTriangle.F = PrePoint;
                        }
                        pendingTriangle.T = e.Location;
                        pendingTriangle.L = new Point(PrePoint.X - (e.Location.X - PrePoint.X), e.Location.Y);
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
                            pendingCircle.Rec = new Rectangle(PrePoint.X - 3, PrePoint.Y - 3, 6, 6);
                            pendingCircle.Center = PrePoint;
                        }

                        var r = (Math.Sqrt(Math.Pow(e.X - PrePoint.X, 2) + Math.Pow(e.Y - PrePoint.Y, 2)));
                        if (r > 0)
                        {
                            pendingCircle.MousePt = e.Location;
                            var x = PrePoint.X - r;
                            var y = PrePoint.Y - r;
                            pendingCircle.LT.X = (float)x;
                            pendingCircle.LT.Y = (float)y;
                            pendingCircle.Diam = (float)(r * 2);
                            this.WorkPanel.Invalidate();
                            return;
                        }
                    }
                }
                else if(this.DrawMode== DrawMode.ManulWrite)
                {
                    if (ManualPen != null)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(200, Color.Black), 2))
                        {
                            ManulG.DrawLine(pen, ManualPen.Points[ManualPen.Points.Count - 1], e.Location);
                        }
                        ManualPen.Points.Add(e.Location);
                    }
                }
                else if(IsMoveCenter)
                {
                    if (!this.SelectPointFrom.IsEmpty)
                    {
                        var dx = e.Location.X - this.SelectPointFrom.X;
                        var dy = e.Location.Y - this.SelectPointFrom.Y;
                        buttonMove.Left += dx;
                        buttonMove.Top += dy;
                        if (this.WorkPanel.Width-ev.X<=50&&dx>0)
                        {
                            this.WorkPanel.HorizontalScroll.Value = WorkPanel.HorizontalScroll.Maximum;
                        }
                        if ( this.WorkPanel.Height-ev.Y <= 50&&dy>0)
                        {
                            this.WorkPanel.VerticalScroll.Value = WorkPanel.VerticalScroll.Maximum;
                        }
                        SelectPointFrom = e.Location;
                        this.MoveCenter(dx,dy);
                    }
                    return;
                }
                else if (SelectPointFrom != Point.Empty)
                {//绘制选择区域
                    this.SelectRectangle = RefeshRectBy2Dot(SelectPointFrom, e.Location);
                   
                    foreach (var l in this.Shapes.Where(s => s.ShapeType == ShapeType.Line))
                    {
                        l.IsActive = MyMath.LineIntersectRect(l as Line, this.SelectRectangle);
                    }
                    foreach (var d in this.Shapes.Where(s => s.ShapeType == ShapeType.Dot))
                    {
                        d.IsActive = this.SelectRectangle.Contains(d.Location);
                    }

                    foreach (Circle c in this.Shapes.Where(s => s.ShapeType == ShapeType.Circle))
                    {
                        c.IsActive = MyMath.CircleIntersectRect(this.SelectRectangle, c);
                    }
                    foreach (ManualPen m in this.Shapes.Where(s => s.ShapeType == ShapeType.ManualPen))
                    {
                        m.IsActive = m.IntersetRect(this.SelectRectangle);
                    }

                    foreach (TextArea tr in this.Shapes.Where(s => s.ShapeType == ShapeType.TextArea))
                    {
                        var rect = tr.Area;
                        rect.Inflate(-10, -2);
                        tr.IsActive = this.SelectRectangle.Contains(rect);
                    }

                    if (this.SelectRectangle.Width >= 2 && this.SelectRectangle.Height >= 2)
                        this.Update();
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
            }

            private void DotAttching(MouseEventArgs e)
            {
                var destDot = Shapes.Where(s => s.ShapeType == ShapeType.Dot && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Dot;
                if (destDot != null && destDot.IsCapture)
                    CurrentPoint = destDot.P;
                else
                {
                    var destLine = Shapes.Where(s => s.ShapeType == ShapeType.Line && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Line;
                    if (destLine != null)
                    {
                        if (destLine.IsInMidRang())
                            CurrentPoint = destLine.MidPoint;
                        else
                        {
                            CurrentPoint.X = e.X;
                            CurrentPoint.Y = (int)destLine.GetY_ByX(e.X);
                        }
                    }
                    else
                    {
                        var destCirle = Shapes.Where(s => s.ShapeType == ShapeType.Circle && s.PtIn(e.Location)).OrderByDescending(t => t.Id).FirstOrDefault() as Circle;
                        if (destCirle != null)
                        {
                            var y = destCirle.getY_ByX(e.X, e.Y);
                            CurrentPoint.X = e.X;
                            CurrentPoint.Y = (int)y;
                        }
                        else
                            CurrentPoint = e.Location;
                    }
                }
                this.WorkPanel.Invalidate();
            }
            
            private static Font Font = new Font("微软雅黑", 10f, FontStyle.Regular);
            private static Size TextSize = Size.Empty;
            private void WorkPanel_Paint(object sender, PaintEventArgs e)
            {
                var p = Control.MousePosition;
                p = WorkPanel.PointToClient(p);

                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                e.Graphics.DrawString("珠海高波工作室，欢迎大家免费试用！", adverFont, Brushes.WhiteSmoke, adverPos);

                e.Graphics.TranslateTransform(-this.WorkPanel.HorizontalScroll.Value, -this.WorkPanel.VerticalScroll.Value);

                if (IsDrawGrid)
                    DrawGrid(e.Graphics);
                if (IsDrawCoordinate)
                    DrawCoordinate(e.Graphics);
                foreach (var sh in Shapes.OrderBy(t => t.Id))
                    sh.Invalid(e.Graphics);


                if (this.DrawRectangle != Rectangle.Empty)
                {
                    Pen pen = new Pen(Color.DarkBlue, 2);
                    e.Graphics.DrawRectangle(pen, this.DrawRectangle);
                }

                if (!this.SelectPointFrom.IsEmpty)
                {
                    Pen pen = new Pen(Color.Green, 1);
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, this.SelectRectangle);
                }

                if (this.DrawMode == DrawMode.Line)
                {
                    if (!this.PrePoint.IsEmpty && !this.CurrentPoint.IsEmpty)
                        using (var pen = new Pen(Color.DarkBlue, 2))
                        {
                            e.Graphics.FillEllipse(Brushes.Green, new Rectangle((int)PrePoint.X - 3, (int)PrePoint.Y - 3, 6, 6));
                            e.Graphics.DrawLine(pen, this.PrePoint, this.CurrentPoint);
                        }
                }

                if (this.DrawingPolygon != null)
                {
                    using (var pen = new Pen(Color.DarkBlue, 2))
                    {
                        e.Graphics.DrawLine(pen, DrawingPolygon.Dots[DrawingPolygon.Dots.Count - 1].P, DrawingPolygon.pendingPoint);
                    }
                }
                if (this.pendingCircle != null && this.pendingCircle.Diam > 0)
                    using (var pen = new Pen(Color.DarkBlue, 2))
                    {
                        e.Graphics.FillEllipse(Brushes.Red, pendingCircle.Rec);//圆心
                        var r = Math.Sqrt(Math.Pow(pendingCircle.Center.X - pendingCircle.MousePt.X, 2)
                            + Math.Pow(pendingCircle.Center.Y - pendingCircle.MousePt.Y, 2));
                        e.Graphics.DrawLine(pen, pendingCircle.Center, pendingCircle.MousePt);
                        var pMid = new Point((pendingCircle.Center.X + pendingCircle.MousePt.X) / 2, (pendingCircle.Center.Y + pendingCircle.MousePt.Y) / 2);

                        e.Graphics.DrawString(r.ToString("N1"), Font, Brushes.Green, pMid.X, pMid.Y);
                        e.Graphics.DrawArc(pen, this.pendingCircle.LT.X, this.pendingCircle.LT.Y, this.pendingCircle.Diam, this.pendingCircle.Diam, 0f, 360f);
                    }
                if (pendingTriangle != null)
                    using (var pen = new Pen(Color.DarkBlue, 2))
                    {
                        e.Graphics.DrawLine(pen, pendingTriangle.F, pendingTriangle.T);
                        e.Graphics.DrawLine(pen, pendingTriangle.T, pendingTriangle.L);
                        e.Graphics.DrawLine(pen, pendingTriangle.L, pendingTriangle.F);
                    }

             //   DrawExpress2(e.Graphics, new Point(100, 100), "(x+2)/(x+1)+(x+6)");
                // e.Graphics.DrawString("X", Font, Brushes.Black, 100, 100);
                //  e.Graphics.DrawString("2", new Font(Font.FontFamily, 8f), Brushes.Black, 110, 90);
                //   e.Graphics.DrawImage(bufferimage, rect);
            }
            private Point adverPos = new Point(100, 100);
            Font adverFont = new Font("微软雅黑",25f, FontStyle.Bold);
            private void DrawExpress2(Graphics g, Point p, string express)
            {   //  x/2/2+x/2
                // (x+2)/2+3/4
                //(x+2)/(x+1)/
                StringBuilder preWord = new StringBuilder();
                Point prefixP;
                int wp = 0;
                for (int i = 0; i < express.Length; i++)
                {
                    var c = express[i];
                    if (c == '/')
                    {
                        prefixP = p;
                        g.DrawString(preWord.ToString(), Font, Brushes.Black, prefixP);

                        p.Y += 20;
                        int wp1 = 0;
                        i++;
                        c = express[i];
                        if (c == '(')
                            wp1 = 1;

                        StringBuilder word = new StringBuilder();
                        word.Append(c);
                        while (wp1 > 0 && i < express.Length)
                        {
                            i++;
                            c = express[i];
                            word.Append(c);

                            if (c == '(')
                                wp1++;
                            else if (c == ')')
                                wp1--;
                        }//读出/ 后面的完整表达式

                        //  g.DrawLine(Pens.Black, new Point(preX,p.X),new Point(1,2) );
                        g.DrawString(word.ToString(), Font, Brushes.Black, p);
                        p.Y = prefixP.Y;
                        p.X+= Math.Max(TextRenderer.MeasureText(word.ToString(), Font).Width,
                            TextRenderer.MeasureText(word.ToString(), Font).Width);
                        preWord.Clear();
                    }
                    else if (c == '(')
                    {
                        preWord.Append(c);
                        wp++;
                    }
                    else if (c == ')')
                    {
                        preWord.Append(c);
                        wp--;
                    }
                    else
                    {
                        preWord.Append(c);
                    }
                }

                if (wp == 0)
                {
                    g.DrawString(preWord.ToString(), Font, Brushes.Black, p);
                }
            }
            private void DrawExpress(Graphics g,Point p,string express)
            {
                int x =p.X, y = p.Y;
                for(int i=0;i<express.Length;i++)
                {
                    var c = express[i];
                    if (c== '?')
                    {
                        i++;
                        y -= 10;
                        List<char> sqrt = new List<char>();
                        c = express[i];
                        while (Char.IsDigit(c))
                        {
                            sqrt.Add(c);
                            c = express[i];
                            i++;
                            if (i > express.Length)
                                break;
                        }
                        g.DrawString(new string(sqrt.ToArray()), Font, Brushes.Black, x, y);
                        y += 10;
                    }
                    else if(c=='/')
                    {
                        i++;
                        y += 20;
                        List<char> sqrt = new List<char>();
                        c = express[i];
                        while (Char.IsDigit(c))
                        {
                            sqrt.Add(c);
                            c = express[i];
                            i++;
                            if (i > express.Length)
                                break;
                        }
                        g.DrawString(new string(sqrt.ToArray()), Font, Brushes.Black, x, y);
                        y -= 20;
                    }
                    x += 10;

                    g.DrawString(c.ToString(), Font, Brushes.Black, x, y);
                }
            }
            private void WorkPanel_PaintBuffer(object sender, PaintEventArgs e)
            {

                Bitmap bufferimage = new Bitmap(this.WorkPanel.Width,this.WorkPanel.Height);
                using (Graphics g = Graphics.FromImage(bufferimage))
                {
                    g.Clear(this.WorkPanel.BackColor);
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    g.TranslateTransform(-this.WorkPanel.HorizontalScroll.Value, -this.WorkPanel.VerticalScroll.Value);

                    if (IsDrawGrid)
                        DrawGrid(g);
                    if (IsDrawCoordinate)
                        DrawCoordinate(g);


                    foreach (var sh in Shapes)
                        sh.Invalid(g);

                    var p = Control.MousePosition;
                    p = WorkPanel.PointToClient(p);
                    //   DrawXY(p.X, p.Y, g);
                    if (this.DrawRectangle != Rectangle.Empty)
                    {
                        Pen pen = new Pen(Color.DarkBlue, 2);
                        g.DrawRectangle(pen, this.DrawRectangle);
                    }

                    if (!this.SelectPointFrom.IsEmpty)
                    {
                        Pen pen = new Pen(Color.Green, 1);
                        pen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(pen, this.SelectRectangle);
                    }

                    if (this.DrawMode == DrawMode.Line)
                    {
                        if (!this.PrePoint.IsEmpty && !this.CurrentPoint.IsEmpty)
                            using (var pen = new Pen(Color.DarkBlue, 2))
                            {
                                g.FillEllipse(Brushes.Green, new Rectangle((int)PrePoint.X - 3, (int)PrePoint.Y - 3, 6, 6));
                                g.DrawLine(pen, this.PrePoint, this.CurrentPoint);
                            }
                    }

                    if (this.DrawingPolygon != null)
                    {
                        using (var pen = new Pen(Color.DarkBlue, 2))
                        {
                            g.DrawLine(pen, DrawingPolygon.Dots[DrawingPolygon.Dots.Count - 1].P, DrawingPolygon.pendingPoint);
                        }
                    }
                    if (this.pendingCircle != null && this.pendingCircle.Diam > 0)
                        using (var pen = new Pen(Color.DarkBlue, 2))
                        {
                            g.FillEllipse(Brushes.Red, pendingCircle.Rec);//圆心
                            var r = Math.Sqrt(Math.Pow(pendingCircle.Center.X - pendingCircle.MousePt.X, 2)
                                + Math.Pow(pendingCircle.Center.Y - pendingCircle.MousePt.Y, 2));
                            g.DrawLine(pen, pendingCircle.Center, pendingCircle.MousePt);
                            var pMid = new Point((pendingCircle.Center.X + pendingCircle.MousePt.X) / 2, (pendingCircle.Center.Y + pendingCircle.MousePt.Y) / 2);

                            g.DrawString(r.ToString("N1"), Font, Brushes.Green, pMid.X, pMid.Y);
                            g.DrawArc(pen, this.pendingCircle.LT.X, this.pendingCircle.LT.Y, this.pendingCircle.Diam, this.pendingCircle.Diam, 0f, 360f);
                        }
                    if (pendingTriangle != null)
                        using (var pen = new Pen(Color.DarkBlue, 2))
                        {
                            g.DrawLine(pen, pendingTriangle.F, pendingTriangle.T);
                            g.DrawLine(pen, pendingTriangle.T, pendingTriangle.L);
                            g.DrawLine(pen, pendingTriangle.L, pendingTriangle.F);
                        }

                    g.DrawString("X", Font, Brushes.Black, 100, 100);
                    g.DrawString("2", new Font(Font.FontFamily, 8f), Brushes.Black, 110, 90);
                }

                 e.Graphics.DrawImage(bufferimage, 0,0);
            }

            List<Image> Images = new List<Image>();
            public void BatchMoveRevok(Point prePos,int x,int y)
            {
                DoUnit unit = new DoUnit();
                var dots = getSelectedDots();
                foreach (var d in dots)
                {
                    var mp = new Point(x - prePos.X, y - prePos.Y);
                    unit.Units.Push(new KeyValuePair<IShape, object>(d, mp));
                }
                foreach (ManualPen m in this.Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.ManualPen))
                {
                    var mp = new Point(x - prePos.X, y - prePos.Y);
                    unit.Units.Push(new KeyValuePair<IShape, object>(m, mp));
                }
                foreach (TextArea t in this.Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.TextArea))
                {
                    var mp = new Point(x - prePos.X, y - prePos.Y);
                    unit.Units.Push(new KeyValuePair<IShape, object>(t, mp));
                }
                this.RevokeAllObjects.Push(unit);
            }
            private List<Dot> getSelectedDots()
            {
                List<Dot> moveDots = new List<Dot>();
                var selects = (from sh in this.Shapes where sh.IsActive select sh).ToList();
                foreach (var sh in selects.Where(t => t.IsActive && t.ShapeType == ShapeType.Line))
                {
                    var line = sh as Line;
                    if (!moveDots.Contains(line.From))
                        moveDots.Add(line.From);
                    if (!moveDots.Contains(line.To))
                        moveDots.Add(line.To);
                }

                foreach (Circle c in selects.Where(t => t.IsActive && t.ShapeType == ShapeType.Circle))
                {
                    if (!moveDots.Contains(c.Center))
                        moveDots.Add(c.Center);
                }

                foreach (Dot d in selects.Where(t => t.IsActive && t.ShapeType == ShapeType.Dot))
                {
                    if (!moveDots.Contains(d))
                        moveDots.Add(d);
                }
                return moveDots;
            }
            public void MovedAll(int x, int y)
            {
                foreach (ManualPen m in this.Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.ManualPen))
                {
                    m.Move(x, y);
                }
                foreach (TextArea t in this.Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.TextArea))
                {
                    t.Move(x, y);
                }
                foreach (UserPicture t in this.Shapes.Where(t => t.IsActive && t.ShapeType == ShapeType.UserPicture))
                {
                    t.Move(x, y);
                }
                foreach (var dot in getSelectedDots())
                {
                    dot.Move(x, y);
                }
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
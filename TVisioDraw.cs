using System;
using System.Collections.Generic;

using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.Text;
using System.ComponentModel;
using System.IO;



namespace Riches.Visio
{
    #region 可视化流程设计部分

    public  class Instrunction
    {
        public string SessionId { get; set; }
        public int FlowType { get; set; }

        public string DataType { get; set; }
        
        public string AckWay { get; set; }
        public DateTime senddateTime;
        public int DoType;

        public object Data;
    }
    public class TVisioLine
    {
        public Point FromPos;
        public Point ToPos;
        public Point FromPosReal;
        public Point ToPosReal;
        public TVisioNodeBtn FromNode = null;
        public TVisioNodeBtn ToNode = null;
        public int Width=3;//线段宽度
        public DashStyle PenStyle;
        public Color LineColor = Color.CornflowerBlue;
        public int CapStyle = 1;
        public TVisioLine(Point A, Point B)
        {
            FromPos = A;
            ToPos = B;
           // FromPosReal = A;
          //  ToPosReal = B;
        }
       
    }

    public enum TVisioNodeType { StartBtn, NodeBtn, RemBtn, EndBtn,PointBtn, OtherBtn };
    [ToolboxItem(false)]
    public class TVisioNodeBtn : Button
    {
        private int mNodeId;
        public TVisioNodeType NodeType;
        public string Menu;
        public int CheckCount = 0;
        public string RoleId = "";
        public string SpaceClassName= "";
        public List<TVisioLine> WKLines;
        public TVisioNodeBtn()
        {
            WKLines = new List<TVisioLine>();
           // this.BackColor = Color.Transparent;
            this.Size = new Size(60, 60);
            this.FlatAppearance.BorderSize = 1;
            this.FlatAppearance.MouseDownBackColor = Color.Transparent;
            this.FlatAppearance.MouseOverBackColor = Color.Transparent;
            //CreateRectRgn、CreateEllpticRgn、CreateRoundRectRgn、CreatePolygonRgn 和 CreatePolyPolygonRgn。
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.Region != null)
                this.Region.Dispose();
            this.Region = new Region(得到当前区域圆角(0, 0, this.Width, this.Height, 4));
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            System.Drawing.Pen pen = new Pen(Color.Black, 1.5f);
            pevent.Graphics.SmoothingMode = SmoothingMode.HighQuality;// AntiAlias;
            DrawRoundRect(pevent.Graphics, pen, 1, 1, Width - 2, this.Height - 2, 4);
            pen.Dispose();
        }
        public void DrawRoundRect(Graphics g, Pen p, float X, float Y, float width, float height, float radius)
        {
            GraphicsPath gp = 得到当前区域圆角(X, Y, width, height, radius);
            g.DrawPath(p, gp);
        }
        private GraphicsPath 得到当前区域圆角(float X, float Y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(X + radius, Y, X + width - (radius * 2), Y);

            gp.AddArc(X + width - (radius * 2), Y, radius * 2, radius * 2, 270, 90);

            gp.AddLine(X + width, Y + radius, X + width, Y + height - (radius * 2));

            gp.AddArc(X + width - (radius * 2), Y + height - (radius * 2), radius * 2, radius * 2, 0, 90);

            gp.AddLine(X + width - (radius * 2), Y + height, X + radius, Y + height);

            gp.AddArc(X, Y + height - (radius * 2), radius * 2, radius * 2, 90, 90);

            gp.AddLine(X, Y + height - (radius * 2), X, Y + radius);

            gp.AddArc(X, Y, radius * 2, radius * 2, 180, 90);
            gp.CloseFigure();

            return gp;
        }
        public int NodeId
        {
            get
            {
                return mNodeId;
            }
            set
            {
                mNodeId = value;
            }
        }
    }

    #endregion

    public class TMenuItemForIde
    {
        public string Name;
        public string Caption;
        public string SpaceNameClass;
        public string Tag;
        public string Image;
        public object Source;
        public bool CanRun;
        public Image MyImage;
        public override string ToString()
        {
            return Caption;
        }
    }
    public enum  TVisioDrawType { DrawStart, DrawNode, DrawRouter, DrawEndBtn, DrawLine, DoNothing };
    public class TVisioLineCheckArgs : EventArgs
    {
        private TVisioNodeBtn m_FromNode, m_ToNode;
        private bool m_IsPass=true;
        
        public TVisioLineCheckArgs(TVisioNodeBtn AFromNode,TVisioNodeBtn AToNode)
        {
            this.m_FromNode = AFromNode;
            this.m_ToNode = AToNode;
        }
        public TVisioNodeBtn FromNode
        {
            get
            {
                return this.m_FromNode;
            }
            set
            {
                this.m_FromNode = value;
            }
        }
        public TVisioNodeBtn ToNode
        {
            get
            {
                return this.m_ToNode;
            }
            set
            {
                this.m_ToNode = value;
            }
        }
        public bool IsPass
        {
            get
            {
                return this.m_IsPass;
            }
            set
            {
                this.m_IsPass = value;
            }
        }
      
    }
    public delegate void TSetNodeTextEventHandler(TVisioNodeBtn Node);
    public delegate void SaveOrLoadDelegate();
    public delegate void SaveOrLoadOnePageDelegate(System.Windows.Forms.Control WorkPanel);
    public delegate bool SetLineStyleDelegate(TVisioLine VisioLine);
    /// <summary>
    /// 可视化工作流的核心代码--lgb2008-12-10 zhuhai
    /// </summary>
    public class TVisioDraw : IDisposable   
    {

        private System.Windows.Forms.ContextMenuStrip  ContextPopMenu;
        private System.Windows.Forms.ToolStripMenuItem DrawLineMenuItemSolid;
        private System.Windows.Forms.ToolStripMenuItem DeleteLineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteNodeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteAllNodeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PictureNodeMenuItem, ClearPictureNodeMenuItem, DrawRemMenuItem, DrawDotMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SetNodeTextMenuItem, CfgLineStyle,SaveIDEMenuItem, ToDesingMenuItem,ReleaseIdeItem
                                                       ,ShowHideMenuItem ,LoadOneFile,SaveToOneFile,LoadOnePageFile,SaveOnePageFile;
        ToolStripSeparator LineItem;

        private bool IsDrawingLine = false;
        private bool IsMovingNode = false;
        private Point Pf, Pt;
        private Control WorkPanel;
        private TVisioLine CurrentDrawingLine;
        private bool m_IsModify = false;
        public  bool IsReadOnly = true;
        public  TVisioDrawType CurrentDrawType = TVisioDrawType.DoNothing;
        public  event MouseEventHandler WorkFlowNodeMouseDown;

        public SaveOrLoadDelegate SaveIntface;
        public SaveOrLoadDelegate ReLoadIntface;
        public SetLineStyleDelegate SetLineStyle;
        public  TSetNodeTextEventHandler OnSetNodeText;
        public  SaveOrLoadDelegate LoadIdeFromOneFile;
        public  SaveOrLoadDelegate SaveIdeToOneFile;
        public DragEventHandler OnDragDop;
        public SaveOrLoadOnePageDelegate LoadOnePageIDE;
        public SaveOrLoadOnePageDelegate SaveOnePageIDE;
        public SaveOrLoadDelegate MyRelaseItemIDE;
        public SaveOrLoadDelegate MyShowHideMenuItem;
        public HScrollProperties HScrollCtl = null;
        public VScrollProperties VScrollCtl = null;

        public TVisioDraw(SaveOrLoadDelegate AInf, SetLineStyleDelegate ASetLineStyle, TSetNodeTextEventHandler ASetNodeText)
        {
            CreatePopMemItem();
            SaveIntface = AInf;
            SetLineStyle = ASetLineStyle;
            OnSetNodeText = ASetNodeText;
        }
        public Image LoadImg(string FilePathName)
        {
            if (System.IO.File.Exists(FilePathName))
            {
                Stream s = File.Open(FilePathName, FileMode.Open);
                Image img = Image.FromStream(s);
                s.Close();
                return img;
            }
            return null;
        }
        private void AWorkPanel_DoubleClick(object sender, EventArgs e)
        {
            if (IsReadOnly)
                return;

            //TSetBgImgForm SetBgImgForm = new TSetBgImgForm(WorkPanel);
            //if (SetBgImgForm.ShowDialog() == DialogResult.OK)
            //{

            //}


            //System.Windows.Forms.OpenFileDialog ofd = new OpenFileDialog();
            //if (ofd.ShowDialog() == DialogResult.OK)
            //{
            //    try
            //    {
            //       // WorkPanel.BackgroundImageLayout = ImageLayout.Center;
            //        WorkPanel.BackgroundImage = Image.FromFile(ofd.FileName);
            //        if (WorkPanel.BackgroundImage.Width > WorkPanel.Width)
            //            WorkPanel.BackgroundImageLayout = ImageLayout.Zoom;

            //        string DestDir = Application.StartupPath + "\\IdePic\\";
            //        if (!System.IO.Directory.Exists(DestDir))
            //            System.IO.Directory.CreateDirectory(DestDir);

            //        string LocalPathName = DestDir + WorkPanel.Name + System.IO.Path.GetExtension(ofd.FileName);
            //        if (System.IO.File.Exists(LocalPathName))
            //            System.IO.File.Delete(LocalPathName);

            //        System.IO.File.Copy(ofd.FileName, LocalPathName);

            //        WorkPanel.Tag = LocalPathName;
            //    }
            //    catch
            //    {
            //        MessageBox.Show("图片格式无法识别");
            //    }
            //}
        }

        private void CreatePopMemItem()
        {
            this.ContextPopMenu = new System.Windows.Forms.ContextMenuStrip();
            this.DrawLineMenuItemSolid = new System.Windows.Forms.ToolStripMenuItem();

            ToolStripMenuItem DrawLineMenuItemDot = new System.Windows.Forms.ToolStripMenuItem();
            DrawLineMenuItemDot.Text = "画虚线";
            DrawLineMenuItemDot.Name = "DrawLineMenuItemDot";
            DrawLineMenuItemDot.Click += new EventHandler(MenuItem_Click);
            ToolStripMenuItem DrawLineMenuItemDashDot = new System.Windows.Forms.ToolStripMenuItem();
            DrawLineMenuItemDashDot.Name = "DrawLineMenuItemDashDot";
            DrawLineMenuItemDashDot.Text = "画点线";
            DrawLineMenuItemDashDot.Click += new EventHandler(MenuItem_Click);

            this.DeleteLineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteNodeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteAllNodeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetNodeTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            LoadOneFile=new ToolStripMenuItem();
            SaveToOneFile = new ToolStripMenuItem();
            LoadOnePageFile = new ToolStripMenuItem();
            SaveOnePageFile = new ToolStripMenuItem();
            LineItem = new  ToolStripSeparator();
            this.PictureNodeMenuItem = new ToolStripMenuItem();
            this.ClearPictureNodeMenuItem = new ToolStripMenuItem();
            this.SaveIDEMenuItem = new ToolStripMenuItem();
            this.ToDesingMenuItem = new ToolStripMenuItem();
            this.ReleaseIdeItem = new ToolStripMenuItem();
            this.ShowHideMenuItem = new ToolStripMenuItem();

            CfgLineStyle = new ToolStripMenuItem();
            CfgLineStyle.Text = "修改线段样式";
            CfgLineStyle.Name = "CfgLineStyle";
            CfgLineStyle.Click += new EventHandler(MenuItem_Click);
            DrawRemMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            DrawDotMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextPopMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DrawLineMenuItemSolid,
            DrawLineMenuItemDot,
            DrawLineMenuItemDashDot,
            CfgLineStyle,
            this.DeleteLineMenuItem,
            this.DeleteNodeMenuItem,
            DeleteAllNodeMenuItem,
            PictureNodeMenuItem,
            DrawRemMenuItem,
            DrawDotMenuItem,
            ClearPictureNodeMenuItem,
            SetNodeTextMenuItem,
            ToDesingMenuItem,
            SaveIDEMenuItem,
            ReleaseIdeItem,
            ShowHideMenuItem,
            LoadOneFile,
            SaveToOneFile,
            LineItem,
            LoadOnePageFile,
            SaveOnePageFile
            });
            
            SaveIDEMenuItem.Enabled = false;

            this.ContextPopMenu.Opening += new System.ComponentModel.CancelEventHandler(ContextPopMenu_Opening);
            this.ContextPopMenu.Name = "ContextPopMenu";
            this.ContextPopMenu.Size = new System.Drawing.Size(153, 92);

            // 
            // DrawLineMenuItemSolid
            // 
            this.DrawLineMenuItemSolid.Name = "DrawLineMenuItemSolid";
            this.DrawLineMenuItemSolid.Size = new System.Drawing.Size(152, 22);
            this.DrawLineMenuItemSolid.Text = "画实线";
            this.DrawLineMenuItemSolid.Click += new EventHandler(MenuItem_Click);
            // 
            // DeleteLineMenuItem
            // 
            this.DeleteLineMenuItem.Name = "DeleteLineMenuItem";
            this.DeleteLineMenuItem.Size = new System.Drawing.Size(152, 22);
            this.DeleteLineMenuItem.Text = "删除连线";
            this.DeleteLineMenuItem.Click += new EventHandler(MenuItem_Click);
            // 
            // DeleteNodeMenuItem
            // 
            this.DeleteNodeMenuItem.Name = "DeleteNodeMenuItem";
            this.DeleteNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this.DeleteNodeMenuItem.Text = "删除节点";
            this.DeleteNodeMenuItem.Click += new EventHandler(MenuItem_Click);
            // 


            this.DeleteAllNodeMenuItem.Name = "DeleteNodeMenuItem";
            this.DeleteAllNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this.DeleteAllNodeMenuItem.Text = "删除所有节点";
            this.DeleteAllNodeMenuItem.Click += new EventHandler(MenuItem_Click);

            this.PictureNodeMenuItem.Name = "PictureNodeMenuItem";
            this.PictureNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this.PictureNodeMenuItem.Text = "设置图片";
            this.PictureNodeMenuItem.Click += new EventHandler(MenuItem_Click);

            ClearPictureNodeMenuItem.Name = "ClearPictureNodeMenuItem";
            this.ClearPictureNodeMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ClearPictureNodeMenuItem.Text = "清除图片";
            this.ClearPictureNodeMenuItem.Click += new EventHandler(MenuItem_Click);


            this.SetNodeTextMenuItem.Name = "SetNodeTextMenuItem";
            this.SetNodeTextMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SetNodeTextMenuItem.Text = "修改节点特性";
            this.SetNodeTextMenuItem.Click += new EventHandler(MenuItem_Click);

            this.SaveIDEMenuItem.Name = "SaveIDE";
            this.SaveIDEMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SaveIDEMenuItem.Text = "返回流程运行模式";
            this.SaveIDEMenuItem.Click += new EventHandler(MenuItem_Click);


            //this.ReleaseIdeItem.Name = "ReleaseIdeItem";
            //this.ReleaseIdeItem.Size = new System.Drawing.Size(152, 22);
            //this.ReleaseIdeItem.Text = "发布IDE界面";
            //this.ReleaseIdeItem.Click += new EventHandler(MenuItem_Click);


            //this.ShowHideMenuItem.Name = "ShowHideMenuItem";
            //this.ShowHideMenuItem.Size = new System.Drawing.Size(152, 22);
            //this.ShowHideMenuItem.Text = "打开删除菜单及报表";
            //this.ShowHideMenuItem.Click += new EventHandler(MenuItem_Click);

            this.ToDesingMenuItem.Name = "DesignIDE";
            this.ToDesingMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ToDesingMenuItem.Text = "进入设计流程模式";
            this.ToDesingMenuItem.Click += new EventHandler(MenuItem_Click);

            //this.LoadOneFile.Name = "LoadOneFile";
            //this.LoadOneFile.Size = new System.Drawing.Size(152, 22);
            //this.LoadOneFile.Text = "从文件中加载界面";
            //this.LoadOneFile.Click += new EventHandler(MenuItem_Click);

            //this.SaveToOneFile.Name = "SaveToOneFile";
            //this.SaveToOneFile.Size = new System.Drawing.Size(152, 22);
            //this.SaveToOneFile.Text = "保存界面到文件";
            //this.SaveToOneFile.Click += new EventHandler(MenuItem_Click);

            //LineItem.Name = "LineItem";
            //LineItem.Text = "-";

            //this.LoadOnePageFile.Name = "LoadOnePageFile";
            //this.LoadOnePageFile.Size = new System.Drawing.Size(152, 22);
            //this.LoadOnePageFile.Text = "从文件中加载当前界面";
            //this.LoadOnePageFile.Click += new EventHandler(MenuItem_Click);

            //this.SaveOnePageFile.Name = "SaveOnePageFile";
            //this.SaveOnePageFile.Size = new System.Drawing.Size(152, 22);
            //this.SaveOnePageFile.Text = "保存当前界面到文件";
            //this.SaveOnePageFile.Click += new EventHandler(MenuItem_Click);

            DrawRemMenuItem.Name = "DrawRemMenuItem";
            DrawRemMenuItem.Text = "画备注内容";
            DrawRemMenuItem.Click += new EventHandler(DrawRemMenuItem_Click);

            DrawDotMenuItem.Name = "DrawDotMenuItem";
            DrawDotMenuItem.Text = "画端点";
            DrawDotMenuItem.Click += new EventHandler(DrawDotMenuItem_Click);
        }

        private void DrawDotMenuItem_Click(object sender, EventArgs e)
        {
            if (MouseOldPos == null)
                MouseOldPos = Cursor.Position;
            MouseOldPos = WorkPanel.PointToClient(MouseOldPos);
            CreateOneNode(TVisioNodeType.PointBtn, MouseOldPos);
        }

        private void DrawRemMenuItem_Click(object sender, EventArgs e)
        {
            if (MouseOldPos == null)
                MouseOldPos = Cursor.Position;
            MouseOldPos = WorkPanel.PointToClient(MouseOldPos);
            CreateOneNode(TVisioNodeType.RemBtn, MouseOldPos);
        }


        public void UnLinkPanel()
        {
            if (this.WorkPanel != null)
            {
                WorkPanel.Paint -= WorkPanel_Paint;
                WorkPanel.MouseDown -= new MouseEventHandler(WorkPanel_MouseDown);
                WorkPanel.MouseMove -= new MouseEventHandler(WorkPanel_MouseMove);
                WorkPanel.DoubleClick -= new EventHandler(AWorkPanel_DoubleClick);
                WorkPanel.MouseUp -= new MouseEventHandler(WorkPanel_MouseUp);
                WorkPanel.DragDrop -= new DragEventHandler(WorkPanel_DragDrop);
                WorkPanel.DragEnter -= new DragEventHandler(WorkPanel_DragEnter);

                for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
                {
                    if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                    {
                        TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                      //  WKNode.FlatAppearance.BorderSize = 0;
                        if (WKNode.NodeType == TVisioNodeType.NodeBtn)
                            WKNode.Cursor = Cursors.Hand;
                        else
                            WKNode.Cursor = Cursors.Default;

                        WKNode.MouseDown -= this.Node_MouseDown;
                        WKNode.MouseMove -= this.Node_MouseMove;
                        WKNode.MouseLeave -= this.Node_MouseLeave;
                    }
                }
            }
        }
        public void UnLinkPanel(Control MPanel)
        {
            if (MPanel != null)
            {
               // Panel MainPP = MPanel;
                //if (MPanel is Panel)
                //    MainPP = MPanel as Panel;
                //else
                //    MainPP = MPanel.Tag as Panel;

                MPanel.Paint -= WorkPanel_Paint;
                MPanel.MouseDown -= new MouseEventHandler(WorkPanel_MouseDown);
                MPanel.MouseMove -= new MouseEventHandler(WorkPanel_MouseMove);
                MPanel.DoubleClick -= new EventHandler(AWorkPanel_DoubleClick);
                MPanel.MouseUp -= new MouseEventHandler(WorkPanel_MouseUp);
                MPanel.DragDrop -= new DragEventHandler(WorkPanel_DragDrop);
                MPanel.DragEnter -= new DragEventHandler(WorkPanel_DragEnter);
                MPanel.MouseUp -= new MouseEventHandler(WorkPanel_MouseUp);
                for (int i = 0; i < MPanel.Controls.Count; i++)
                {
                    if (MPanel.Controls[i] is TVisioNodeBtn)
                    {
                        TVisioNodeBtn WKNode = MPanel.Controls[i] as TVisioNodeBtn;
                       // WKNode.FlatAppearance.BorderSize = 0;
                       // WKNode.Cursor = Cursors.Hand;

                        WKNode.MouseDown -= this.Node_MouseDown;
                        WKNode.MouseMove -= this.Node_MouseMove;
                        WKNode.MouseLeave -= this.Node_MouseLeave;
                        WKNode.MouseUp -= new MouseEventHandler(WKNode_MouseUp);
                    }
                }
            }
        }
        public void LinkPanel(Control AWorkPanel)
        {
          //  Panel MainPP = null;
            if (AWorkPanel is Panel)
            {
                Panel p = AWorkPanel as Panel;
                p.AutoScroll = true;
                HScrollCtl = p.HorizontalScroll;
                VScrollCtl = p.VerticalScroll;
            }
            else if (AWorkPanel is Form)
            {
                Form p = AWorkPanel as Form;
                p.AutoScroll = true;
                HScrollCtl = p.HorizontalScroll;
                VScrollCtl = p.VerticalScroll;
            }
            
          
            UnLinkPanel();

            this.WorkPanel = AWorkPanel;
            this.WorkPanel.Paint += WorkPanel_Paint;
            WorkPanel.MouseDown += new MouseEventHandler(WorkPanel_MouseDown);
            WorkPanel.DoubleClick += new EventHandler(AWorkPanel_DoubleClick);
            WorkPanel.MouseUp += new MouseEventHandler(WorkPanel_MouseUp);

            if (!IsReadOnly)
            {
                WorkPanel.MouseMove += new MouseEventHandler(WorkPanel_MouseMove);
                WorkPanel.MouseUp += new MouseEventHandler(WorkPanel_MouseUp);
                WorkPanel.DragDrop += new DragEventHandler(WorkPanel_DragDrop);
                WorkPanel.DragEnter += new DragEventHandler(WorkPanel_DragEnter);

                for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
                {
                    if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                    {
                        TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                        WKNode.MouseDown += this.Node_MouseDown;
                        WKNode.MouseMove += this.Node_MouseMove;
                        WKNode.MouseLeave += this.Node_MouseLeave;
                        WKNode.MouseUp += new MouseEventHandler(WKNode_MouseUp);
                        WKNode.Cursor = Cursors.Default ;
                     //   WKNode.FlatAppearance.BorderSize = 1;
                    }
                }
            }
        }
        private void WKNode_MouseUp(object sender, MouseEventArgs e)
        {
            // this.CurrentDrawType = TCurrentDrawType.DoNothing;
            this.IsMovingNode = false;
        }
        private void ContextPopMenu_Opening(object sender, CancelEventArgs e)
        {
            if (!m_IsIDE)
            {
                this.SetNodeTextMenuItem.Visible = false;
            }
        }

        private  bool ThumbnailCallback()
        {
            return false;
        }
        private  Image GetReducedImage(Image BigImage, int Width, int Height)
        {
            try
            {
                if (BigImage.Width < Width && BigImage.Height < Height)
                    return BigImage;

                Image ReducedImage;
                Image.GetThumbnailImageAbort callb = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                ReducedImage = BigImage.GetThumbnailImage(Width, Height - 25, callb, IntPtr.Zero);
                return ReducedImage;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem MenuItem = sender as ToolStripMenuItem;

            if (MenuItem == this.DrawLineMenuItemSolid)
            {
                this.CurrentDrawType = TVisioDrawType.DrawLine;
               
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                    TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;

                    IsDrawingLine = true;
                    Pf = CurrNode.PointToScreen(new Point(CurrNode.Width / 2, CurrNode.Height / 2));
                    Pf = WorkPanel.PointToClient(Pf);
                    Pf = new Point(Pf.X + HScrollCtl.Value, Pf.Y + VScrollCtl.Value);//lgb 25
                    Pt = Pf;
                  
                    CurrentDrawingLine = new TVisioLine(Pf, Pt);
                    CurrentDrawingLine.FromPosReal = new Point(Pf.X, Pf.Y);
                    CurrentDrawingLine.PenStyle = DashStyle.Solid;
                    CurrentDrawingLine.ToPosReal = new Point(Pt.X, Pt.Y);
                    CurrentDrawingLine.FromNode = CurrNode;
                }
                this.IsMovingNode = false;
            }
            else if (MenuItem.Name == "DrawLineMenuItemDot")
            {
                this.CurrentDrawType = TVisioDrawType.DrawLine;
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                    TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;

                    IsDrawingLine = true;
                    Pf = CurrNode.PointToScreen(new Point(CurrNode.Width / 2, CurrNode.Height / 2));
                    Pf = WorkPanel.PointToClient(Pf);
                    Pf = new Point(Pf.X + HScrollCtl.Value, Pf.Y + VScrollCtl.Value);//lgb 25
                    Pt = Pf;

                    CurrentDrawingLine = new TVisioLine(Pf, Pt);
                    CurrentDrawingLine.FromPosReal = new Point(Pf.X, Pf.Y);
                    CurrentDrawingLine.PenStyle = DashStyle.Dot;
                    CurrentDrawingLine.ToPosReal = new Point(Pt.X, Pt.Y);
                    CurrentDrawingLine.FromNode = CurrNode;
                }
                this.IsMovingNode = false;
            }
            else if (MenuItem.Name == "DrawLineMenuItemDashDot")
            {
                this.CurrentDrawType = TVisioDrawType.DrawLine;
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                    TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;
                   
                    IsDrawingLine = true;
                    Pf = CurrNode.PointToScreen(new Point(CurrNode.Width / 2, CurrNode.Height / 2));
                    Pf = WorkPanel.PointToClient(Pf);
                    Pf = new Point(Pf.X + HScrollCtl.Value, Pf.Y + VScrollCtl.Value);//lgb 25
                    Pt = Pf;

                    CurrentDrawingLine = new TVisioLine(Pf, Pt);
                    CurrentDrawingLine.FromPosReal = new Point(Pf.X, Pf.Y);
                    CurrentDrawingLine.PenStyle = DashStyle.DashDot;
                    CurrentDrawingLine.ToPosReal = new Point(Pt.X, Pt.Y);
                    CurrentDrawingLine.FromNode = CurrNode;
                }
                this.IsMovingNode = false;
            }
            else if (MenuItem == this.DeleteLineMenuItem)
            {
                if (this.ContextPopMenu.Tag is TVisioLine)
                {
                    TVisioLine WKLine = this.ContextPopMenu.Tag as TVisioLine;
                    ReDrawOneWKNodeLines(WKLine.FromNode, true);
                    WKLine.FromNode.WKLines.Remove(WKLine);
                    WKLine.ToNode.WKLines.Remove(WKLine);
                }
            }
            else if (MenuItem == this.DeleteNodeMenuItem)
            {
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                     TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;
                     DeleteNode(CurrNode);
                }
            }
            else if (MenuItem == DeleteAllNodeMenuItem)
            {   string TmpFilePathName = System.Guid.NewGuid().ToString();
                this.ClearNodes();
            }
            else if (PictureNodeMenuItem == MenuItem)
            {
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                    TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;
                    System.Windows.Forms.OpenFileDialog PFDlg = new OpenFileDialog();
                    PFDlg.Filter = "图片文件|*.bmp;*.jpg;*.png;*.ico";
                    if (PFDlg.ShowDialog() == DialogResult.OK)
                    {
                        Image IImg = Image.FromFile(PFDlg.FileName);
                        decimal ration = Math.Min(((decimal)CurrNode.Width) / ((decimal)IImg.Width),
                                      ((decimal)CurrNode.Height) / ((decimal)IImg.Height));
                        CurrNode.Image = GetReducedImage(IImg, (int)(IImg.Width * ration), (int)(IImg.Height * ration));
                        CurrNode.Font = new Font(CurrNode.Font, FontStyle.Regular);
                        CurrNode.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
                    }
                    PFDlg.Dispose();
                }
            }
            else if (ClearPictureNodeMenuItem == MenuItem)
            {
                if (this.ContextPopMenu.Tag is TVisioNodeBtn)
                {
                    TVisioNodeBtn CurrNode = this.ContextPopMenu.Tag as TVisioNodeBtn;
                    CurrNode.Image = null;
                }
            }
            else if (MenuItem.Name == "SaveOnePageFile")
            {
                if (this.SaveOnePageFile != null)
                    SaveOnePageIDE(this.WorkPanel);
            }
            else if (MenuItem.Name == "LoadOnePageFile")
            {
                if (this.LoadOnePageIDE != null)
                    LoadOnePageIDE(this.WorkPanel);
            }
            else if (MenuItem.Name == "SaveIDE")
            {
                if (MessageBox.Show("确实要保存此次界面的修改吗？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (this.SaveIntface != null)
                        this.SaveIntface();
                }
                else if (ReLoadIntface != null)
                {
                    IsReadOnly = true;
                    ReLoadIntface();
                    MenuItem.Enabled = false;
                    this.ToDesingMenuItem.Enabled = true;
                    return;
                }
                this.HScrollCtl.Value = 0;
                this.VScrollCtl.Value = 0;
                IsReadOnly = true;
                this.ToDesingMenuItem.Enabled = true;
                MenuItem.Enabled = false;
                LinkPanel(this.WorkPanel);
                HideLinePort();

                for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
                {
                    if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                    {
                        TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                        WKNode.FlatAppearance.BorderSize = 0;
                        WKNode.MouseDown -= this.Node_MouseDown;
                        WKNode.MouseMove -= this.Node_MouseMove;
                        WKNode.MouseLeave -= this.Node_MouseLeave;
                    }
                }
            }
            else if (MenuItem.Name == "ReleaseIdeItem")
            {
                if (MyRelaseItemIDE != null)
                    MyRelaseItemIDE();
            }
            else if (MenuItem.Name == "ShowHideMenuItem")
            {
                if (MyShowHideMenuItem != null)
                    MyShowHideMenuItem();
            }     
            else if (MenuItem.Name == "DesignIDE")
            {
                IsReadOnly = false;
                this.HScrollCtl.Value = 0;
                this.VScrollCtl.Value = 0;

                ShowLinePort();
                MenuItem.Enabled = false;
                LinkPanel(this.WorkPanel);
                this.SaveIDEMenuItem.Enabled = true;
                this.ShowHideMenuItem.Visible = true;
                for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
                {
                    if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                    {
                        TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                        //  WKNode.FlatAppearance.BorderSize = 1;
                        WKNode.MouseDown -= this.Node_MouseDown;
                        WKNode.MouseMove -= this.Node_MouseMove;
                        WKNode.MouseLeave -= this.Node_MouseLeave;

                        WKNode.MouseDown += this.Node_MouseDown;
                        WKNode.MouseMove += this.Node_MouseMove;
                        WKNode.MouseLeave += this.Node_MouseLeave;
                    }
                }
            }
            else if (MenuItem.Equals(this.SaveToOneFile))
            {
                if (this.SaveIdeToOneFile != null)
                    this.SaveIdeToOneFile();
            }
            else if (MenuItem.Equals(this.LoadOneFile))
            {
                if (this.LoadIdeFromOneFile != null)
                    this.LoadIdeFromOneFile();
            }
            else if (MenuItem.Name == "CfgLineStyle")
            {
                TVisioLine Line = this.ContextPopMenu.Tag as TVisioLine;
                if (Line != null && SetLineStyle != null)
                {
                    if (SetLineStyle(Line))
                    {
                        this.DrawLine(Line, false);
                        this.WorkPanel.Invalidate(new Rectangle(Line.FromPos.X, Line.FromPos.Y, Line.ToPos.X - Line.FromPos.X, Line.ToPos.Y - Line.FromPos.Y));

                    }
                }
            }
            else if (SetNodeTextMenuItem == MenuItem)
            {
                if ((this.ContextPopMenu.Tag is TVisioNodeBtn) && (this.OnSetNodeText != null))
                {
                    TVisioNodeBtn Node = this.ContextPopMenu.Tag as TVisioNodeBtn;
                    this.OnSetNodeText(Node);
                }
            }
        }

        private void DeleteNode(TVisioNodeBtn Node)
        {
            ReDrawOneWKNodeLines(Node, true);
            if( Node.WKLines.Count>0)
            for (int i = Node.WKLines.Count - 1; i >= 0; i--)
            {
                TVisioLine WKLine = Node.WKLines[i];
                if ((WKLine.ToNode!=null)&&(WKLine.ToNode != Node))
                    WKLine.ToNode.WKLines.Remove(WKLine);
                if ((WKLine.FromNode!=null)&&(WKLine.FromNode != Node))
                    WKLine.FromNode.WKLines.Remove(WKLine);
            }
            Node.WKLines.Clear();
            this.m_IsModify = true;//销售管理
            Node.Dispose();
        }
        private void WorkPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.IsMovingNode)
                this.IsMovingNode = false;
        }
        private void DrawLine(TVisioLine AWkLine, bool IsClear)
        {
            using (Pen p = new Pen(WorkPanel.BackColor, AWkLine.Width))
            {
                Graphics g = WorkPanel.CreateGraphics();
             
                if (IsClear)
                    p.Color = WorkPanel.BackColor;
                else
                    p.Color = AWkLine.LineColor;

               // if (this.IsReadOnly)
               //     g.SmoothingMode = SmoothingMode.HighQuality;

                p.DashStyle = AWkLine.PenStyle;
                System.Drawing.Drawing2D.AdjustableArrowCap aac = null;
                {
                    aac = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 6);//4,4
                    if (AWkLine.CapStyle == 1)
                        p.CustomEndCap = aac;
                    else if (AWkLine.CapStyle == 0)
                    {
                    }
                    else if (AWkLine.CapStyle == 2)
                        p.CustomStartCap = aac;
                    else
                    {
                        p.CustomStartCap = aac;
                        p.CustomEndCap = aac;
                    }
                }
                Point Fp, Tp;
                Fp = new Point(AWkLine.FromPosReal.X - HScrollCtl.Value, AWkLine.FromPosReal.Y - VScrollCtl.Value);
                Tp = new Point(AWkLine.ToPosReal.X - HScrollCtl.Value, AWkLine.ToPosReal.Y - VScrollCtl.Value);

                g.DrawLine(p, Fp, Tp);
                if (aac != null)
                    aac.Dispose();
                g.Dispose();
            }
        }

        private void WorkPanel_MouseMove(object sender, MouseEventArgs e)
        {//画到某个点
            if (this.IsDrawingLine)
            {
                this.CurrentDrawingLine.FromPosReal = Pf;
                this.CurrentDrawingLine.ToPosReal = Pt;

                this.DrawLine(this.CurrentDrawingLine, true);
                Pt = new Point(e.X, e.Y);
                Pt = new Point(e.X + this.HScrollCtl.Value, e.Y + this.VScrollCtl.Value);//lgb add 25
                this.CurrentDrawingLine.FromPosReal = Pf;
                this.CurrentDrawingLine.ToPosReal = Pt;

                this.DrawLine(this.CurrentDrawingLine, false);
                ReDrawWKNodeLines();
            }
            else
            {
               
            }
        }

        private void WorkPanel_Paint(object sender, PaintEventArgs e)
        {
            ReDrawWKNodeLines();
        }
        private void ClearOneWKNodeLines(TVisioNodeBtn WKNode)
        {
            for (int i = 0; i < WKNode.WKLines.Count; i++)
            {
                this.DrawLine(WKNode.WKLines[i], true);
            }
        }
        public void HideLinePort()
        {
            TVisioNodeBtn OneNode;
            foreach (Control Acontrol in WorkPanel.Controls)
            {
                if (Acontrol is TVisioNodeBtn)
                {
                    OneNode = Acontrol as TVisioNodeBtn;
                    if(OneNode.NodeType== TVisioNodeType.PointBtn)
                        OneNode.Visible = false; ;
                }
            }
        }
        public void ShowLinePort()
        {
            TVisioNodeBtn OneNode;
            foreach (Control Acontrol in WorkPanel.Controls)
            {
                if (Acontrol is TVisioNodeBtn)
                {
                    OneNode = Acontrol as TVisioNodeBtn;
                  //  OneNode.Left -= this.HScrollCtl.Value;
                  //  OneNode.Top -= this.VScrollCtl.Value;
                    if (OneNode.NodeType == TVisioNodeType.PointBtn)
                    {
                        OneNode.Visible = true;
                        OneNode.BackColor = Color.Black;
                    }
                }
            }
        }
        private void ReSeTVisioNodeBtnCenterPointPos(TVisioNodeBtn WKNode, Point P)
        {
            for (int i = 0; i < WKNode.WKLines.Count; i++)
            {
                if (WKNode.WKLines[i].FromNode == WKNode) //out
                {
                    WKNode.WKLines[i].FromPos = P;
                }
                else WKNode.WKLines[i].ToPos = P;
            }

            for (int i = 0; i < WKNode.WKLines.Count; i++)
            {
                CalNodeIntersect(WKNode.WKLines[i]);
            }
        }
        public void ReDrawOneWKNodeLines(TVisioNodeBtn WKNode, bool IsClear)
        {
            for (int i = 0; i < WKNode.WKLines.Count; i++)
            {
                this.DrawLine(WKNode.WKLines[i], IsClear);
            }
        }

        public void ReDrawWKNodeLines()
        {
            foreach (Control Acontrol in WorkPanel.Controls)
            {
                if (Acontrol is TVisioNodeBtn)
                {
                    TVisioNodeBtn OneNode = Acontrol as TVisioNodeBtn;
                    for (int i = 0; i < OneNode.WKLines.Count; i++)
                    {
                      
                        if (OneNode.WKLines[i].FromNode == OneNode)
                        {
                            CalNodeIntersect(OneNode.WKLines[i]);//
                            this.DrawLine(OneNode.WKLines[i], false);
                        }
                    }
                }
            }
        }
        private Dictionary<TVisioNodeBtn, Point> LinePortDic = new Dictionary<TVisioNodeBtn, Point>();
        public void CalNodeIntersect(TVisioLine WKLine)
        {

            Point P1, P2;

            P1 = WKLine.FromPos;
            P2 = WKLine.ToPos;
            //线处理出来的线
            if (WKLine.FromNode.NodeType == TVisioNodeType.PointBtn)
            {
                WKLine.FromPosReal = new Point(WKLine.FromNode.Left + WKLine.FromNode.Width / 2 + HScrollCtl.Value
                    , WKLine.FromNode.Top + WKLine.FromNode.Height / 2 + VScrollCtl.Value);
            }
            else
                WKLine.FromPosReal = CalRectInetractLine(P1, P2,
                    new Rectangle(WKLine.FromNode.Left + HScrollCtl.Value,
                        WKLine.FromNode.Top + VScrollCtl.Value,
                        WKLine.FromNode.Width, WKLine.FromNode.Height));
            if (WKLine.ToNode.NodeType == TVisioNodeType.PointBtn)
            {
                WKLine.ToPosReal = new Point(WKLine.ToNode.Left + WKLine.ToNode.Width / 2 + HScrollCtl.Value
                            , WKLine.ToNode.Top + WKLine.ToNode.Height / 2 + VScrollCtl.Value);
            }
            else
                WKLine.ToPosReal = CalRectInetractLine(P1, P2, new Rectangle(WKLine.ToNode.Left + HScrollCtl.Value
                    , WKLine.ToNode.Top + VScrollCtl.Value,
                    WKLine.ToNode.Width, WKLine.ToNode.Height));
            
        }

        private bool IsInLine(Point P0, Point P1, Point P2, Rectangle Rect)
        {
            if ((P0.X >= Math.Min(P1.X, P2.X)) && (P0.X <= Math.Max(P1.X, P2.X)))
                if ((P0.Y >= Math.Min(P1.Y, P2.Y)) && (P0.Y <= Math.Max(P1.Y, P2.Y)))
                    if (P0.X >= Rect.Left)
                        if (P0.X <= Rect.Left + Rect.Width)
                            if (P0.Y >= Rect.Top)
                                if (P0.Y <= Rect.Top + Rect.Height)
                                    return true;

            return false;

        }
        private Point CalRectInetractLine(Point P1, Point P2, Rectangle Rect)
        {

            Point rP1 = new Point(0, 0);
            Point rP2 = new Point(0, 0);
            double X = 0, Y = 0, K = 0;

            X = Rect.Left;//  求左边的|线的交点
            if ((P2.X - P1.X) == 0)
            {
                Y = Rect.Top + Rect.Height / 2;
            }
            else
            {
                K = (double)(P2.Y - P1.Y) / (double)(P2.X - P1.X);
                Y = K * (X - (double)P1.X) + P1.Y;
            }

            rP2 = new Point((int)X, (int)Y);
            if (IsInLine(rP2, P1, P2, Rect))
            {
                rP1 = new Point((int)X, (int)Y);
            }

            X = Rect.Left + Rect.Width;///  求右边的|线的交点
            if ((P2.X - P1.X) == 0)
            {
                Y = Rect.Top + Rect.Height / 2;
            }
            else
            {
                Y = K * (X - (double)P1.X) + P1.Y;
            }
            rP2 = new Point((int)X, (int)Y);
            if (IsInLine(rP2, P1, P2, Rect))
            {
                rP1 = new Point((int)X, (int)Y);
            }

            Y = Rect.Top;//上边-线交点
            if (K != 0)
            {
                X = (Y - (double)P1.Y) / K + P1.X;
            }
            else
                X = P2.X;// 90000;

            rP2 = new Point((int)X, (int)Y);
            if (IsInLine(rP2, P1, P2, Rect))
            {
                rP1 = new Point((int)X, (int)Y);
            }

            Y = Rect.Top + Rect.Height;//下边-线的交点
            if (K != 0)
                X = (Y - (double)P1.Y) / K + P1.X;
            else
                X = P2.X;// 90000;
            rP2 = new Point((int)X, (int)Y);
            if (IsInLine(rP2, P1, P2, Rect))
            {
                rP1 = new Point((int)X, (int)Y);
            }
            if (rP1.X == 0 || rP1.Y == 0)
            {
                rP1 = new Point((int)X, (int)Y);
            }
            if (rP1.X > 500)
            {
            }
            return rP1;
        }
        private enum EnumMousePointPosition
        {
            MouseSizeNone = 0, //'无

            MouseSizeRight = 1, //'拉伸右边框

            MouseSizeLeft = 2, //'拉伸左边框

            MouseSizeBottom = 3, //'拉伸下边框

            MouseSizeTop = 4, //'拉伸上边框

            MouseSizeTopLeft = 5, //'拉伸左上角

            MouseSizeTopRight = 6, //'拉伸右上角

            MouseSizeBottomLeft = 7, //'拉伸左下角

            MouseSizeBottomRight = 8, //'拉伸右下角

            MouseDrag = 9   // '鼠标拖动
        }
        const int Band = 5;
        const int MinWidth = 10;
        const int MinHeight = 10;
        private EnumMousePointPosition m_MousePointPosition;

        public void Node_MouseLeave(object sender, System.EventArgs e)
        {
            m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
            Control lCtrl = sender as Control;
            lCtrl.Cursor = Cursors.Arrow;
           
        }
        private EnumMousePointPosition MousePointPosition(Size size, System.Windows.Forms.MouseEventArgs e)
        {

            if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
            {
                if (e.X < Band)
                {
                    if (e.Y < Band) { return EnumMousePointPosition.MouseSizeTopLeft; }
                    else
                    {
                        if (e.Y > -1 * Band + size.Height)
                        { return EnumMousePointPosition.MouseSizeBottomLeft; }
                        else
                        { return EnumMousePointPosition.MouseSizeLeft; }
                    }
                }
                else
                {
                    if (e.X > -1 * Band + size.Width)
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTopRight; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottomRight; }
                            else
                            { return EnumMousePointPosition.MouseSizeRight; }
                        }
                    }
                    else
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTop; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottom; }
                            else
                                return EnumMousePointPosition.MouseSizeNone;
                          //  else
                            //  { return EnumMousePointPosition.MouseDrag; }
                        }
                    }
                }
            }
            else
            {
                return EnumMousePointPosition.MouseSizeNone;
            }
        }

        public void Node_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.IsReadOnly)
                return;
            TVisioNodeBtn OneNode = sender as TVisioNodeBtn;
            if (e.Button == MouseButtons.Left)
            {
                if (this.IsMovingNode)
                {
                    switch (m_MousePointPosition)
                    {
                        case EnumMousePointPosition.MouseSizeBottom:
                            OneNode.Height = OneNode.Height + e.Y - Pf.Y;
                            Pf.X = e.X;
                            Pf.Y = e.Y; //'记录光标拖动的当前点
                          
                            break;
                        case EnumMousePointPosition.MouseSizeBottomRight:
                            OneNode.Width = OneNode.Width + e.X - Pf.X;
                            OneNode.Height = OneNode.Height + e.Y - Pf.Y;
                            Pf.X = e.X;
                            Pf.Y = e.Y; //'记录光标拖动的当前点

                          

                            break;
                        case EnumMousePointPosition.MouseSizeRight:
                            OneNode.Width = OneNode.Width + e.X - Pf.X;
                            //       OneNode.Height = OneNode.Height + e.Y - p1.Y;
                            Pf.X = e.X;
                            Pf.Y = e.Y; //'记录光标拖动的当前点\
                          

                            break;
                        case EnumMousePointPosition.MouseSizeTop:
                            OneNode.Top = OneNode.Top + (e.Y - Pf.Y);
                            OneNode.Height = OneNode.Height - (e.Y - Pf.Y);
                           

                            break;
                        case EnumMousePointPosition.MouseSizeLeft:
                            OneNode.Left = OneNode.Left + e.X - Pf.X;
                            OneNode.Width = OneNode.Width - (e.X - Pf.X);
                         
                            break;
                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            OneNode.Left = OneNode.Left + e.X - Pf.X;
                            OneNode.Width = OneNode.Width - (e.X - Pf.X);
                            OneNode.Height = OneNode.Height + e.Y - Pf.Y;
                            Pf.X = e.X;
                            Pf.Y = e.Y; //'记录光标拖动的当前点
                          
                            break;
                        case EnumMousePointPosition.MouseSizeTopRight:
                            OneNode.Top = OneNode.Top + (e.Y - Pf.Y);
                            OneNode.Width = OneNode.Width + (e.X - Pf.X);
                            OneNode.Height = OneNode.Height - (e.Y - Pf.Y);
                            Pf.X = e.X;
                            Pf.Y = e.Y; //'记录光标拖动的当前点
                           
                            break;
                        case EnumMousePointPosition.MouseSizeTopLeft:
                            OneNode.Left = OneNode.Left + e.X - Pf.X;
                            OneNode.Top = OneNode.Top + (e.Y - Pf.Y);
                            OneNode.Width = OneNode.Width - (e.X - Pf.X);
                            OneNode.Height = OneNode.Height - (e.Y - Pf.Y);
                          
                            break;

                        default:
                            if ((OneNode.Left + e.X - Pf.X) > 0)
                                OneNode.Left = OneNode.Left + e.X - Pf.X;

                            if ((OneNode.Top + e.Y - Pf.Y) > 0)
                            {
                                OneNode.Top = OneNode.Top + e.Y - Pf.Y;
                            }

                            //ClearOneWKNodeLines(OneNode);
                            //Point PP = new Point(OneNode.Left + OneNode.Width / 2 + HScrollCtl.Value, OneNode.Top + OneNode.Height / 2 + VScrollCtl.Value);
                            //ReSeTVisioNodeBtnCenterPointPos(OneNode, PP);

                            break;

                    }

                    ClearOneWKNodeLines(OneNode);
                    Point PP = new Point(OneNode.Left + OneNode.Width / 2 + HScrollCtl.Value, OneNode.Top + OneNode.Height / 2 + VScrollCtl.Value);
                    ReSeTVisioNodeBtnCenterPointPos(OneNode, PP);

                    if (OneNode.NodeType != TVisioNodeType.PointBtn)
                    {
                        if (OneNode.Width < MinWidth) OneNode.Width = MinWidth;
                        if (OneNode.Height < MinHeight) OneNode.Height = MinHeight;
                    }

                    ReDrawOneWKNodeLines(OneNode, false);
                    this.m_IsModify = true;
                }
            }
            else
            {
                m_MousePointPosition = MousePointPosition(OneNode.Size, e);   //'判断光标的位置状态
                if (OneNode.NodeType != TVisioNodeType.PointBtn)
                    switch (m_MousePointPosition)   //'改变光标
                    {
                        case EnumMousePointPosition.MouseSizeNone:
                            OneNode.Cursor = Cursors.Arrow;        //'箭头
                            break;
                        case EnumMousePointPosition.MouseDrag:
                            OneNode.Cursor = Cursors.SizeAll;      //'四方向

                            break;
                        case EnumMousePointPosition.MouseSizeBottom:
                            OneNode.Cursor = Cursors.SizeNS;       //'南北
                            break;
                        case EnumMousePointPosition.MouseSizeTop:
                            OneNode.Cursor = Cursors.SizeNS;       //'南北
                            break;
                        case EnumMousePointPosition.MouseSizeLeft:
                            OneNode.Cursor = Cursors.SizeWE;       //'东西
                            break;
                        case EnumMousePointPosition.MouseSizeRight:
                            OneNode.Cursor = Cursors.SizeWE;       //'东西
                            break;
                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            OneNode.Cursor = Cursors.SizeNESW;     //'东北到南西

                            break;
                        case EnumMousePointPosition.MouseSizeBottomRight:
                            OneNode.Cursor = Cursors.SizeNWSE;     //'东南到西北

                            break;
                        case EnumMousePointPosition.MouseSizeTopLeft:
                            OneNode.Cursor = Cursors.SizeNWSE;     //'东南到西北

                            break;
                        case EnumMousePointPosition.MouseSizeTopRight:
                            OneNode.Cursor = Cursors.SizeNESW;     //'东北到南西

                            break;
                        default:
                            break;
                    }
                else
                {
                    OneNode.Cursor = Cursors.Hand;
                    m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
                }
            }
        }

        private TVisioLine MouseSelectLine(Point P)
        {
            foreach (Control Acontrol in WorkPanel.Controls)
            {
                if (Acontrol is TVisioNodeBtn)
                {
                    TVisioNodeBtn OneNode = Acontrol as TVisioNodeBtn;
                    double K = 0, Y = 0;
                    for (int i = 0; i < OneNode.WKLines.Count; i++)
                        if (OneNode.WKLines[i].FromNode == OneNode)
                        {
                            Point P1 = new Point(OneNode.WKLines[i].FromPosReal.X - this.HScrollCtl.Value,
                                              OneNode.WKLines[i].FromPosReal.Y - this.VScrollCtl.Value);
                            Point P2 = new Point(OneNode.WKLines[i].ToPosReal.X - this.HScrollCtl.Value
                                , OneNode.WKLines[i].ToPosReal.Y - this.VScrollCtl.Value);
                            if (Math.Abs(P2.X - P1.X) <= 3)//垂直|线
                            {
                                if (Math.Abs(P.X - P1.X) <= 4)
                                    if ((P.Y >= Math.Min(P1.Y, P2.Y)) && (P.Y <= Math.Max(P1.Y, P2.Y)))
                                        return OneNode.WKLines[i];
                            }
                            else
                            {
                                K = (double)(P2.Y - P1.Y) / (double)(P2.X - P1.X);
                                Y = K * (P.X - (double)P1.X) + P1.Y;
                                if (Math.Abs(P.Y - Y) <= 4)
                                    if ((P.X >= Math.Min(P1.X, P2.X)) && (P.X <= Math.Max(P1.X, P2.X)))
                                    {
                                        return OneNode.WKLines[i];
                                    }
                            }
                        }
                }
            }
            return null;
        }
        private void WorkPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.IsReadOnly)
            {
                if (e.Button == MouseButtons.Right)
                {

                    this.ContextPopMenu.Tag = null;
                    Point P = this.WorkPanel.PointToScreen(e.Location);
                    for (int i = 0; i < this.ContextPopMenu.Items.Count ; i++)//4
                    {
                        this.ContextPopMenu.Items[i].Visible = false;
                    }
                    this.SaveIDEMenuItem.Visible = true;
                    this.ToDesingMenuItem.Visible = true;

                    this.ReleaseIdeItem.Visible = !this.SaveIDEMenuItem.Enabled;

                

                    this.ContextPopMenu.Show(P);
                }
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (this.CurrentDrawType == TVisioDrawType.DrawLine)
                {
                    if (this.IsDrawingLine)//正在画
                    {
                        this.DrawLine(CurrentDrawingLine, true);
                        this.IsDrawingLine = false;
                        /////////////////////////////////////////////////////////////////////////////////////////////
                        Pt = e.Location;
                        TVisioNodeBtn ToNode = CreateOneNode(TVisioNodeType.PointBtn, Pt);// e.Location);//new add lgb
                        Pt = e.Location;

                      //  Pt = new Point(e.Location.X - this.HScrollCtl.Value, e.Location.Y - this.VScrollCtl.Value);//lgb 25
                        CurrentDrawingLine.FromPos = Pf;
                        CurrentDrawingLine.FromPosReal = Pf;

                        CurrentDrawingLine.ToPos = Pt;

                        CurrentDrawingLine.ToPosReal = Pt;
                        CurrentDrawingLine.ToNode = ToNode;
                        CurrentDrawingLine.CapStyle = 0;
                        CalNodeIntersect(CurrentDrawingLine);

                        ToNode.WKLines.Add(CurrentDrawingLine);
                        CurrentDrawingLine.FromNode.WKLines.Add(CurrentDrawingLine);
                        this.DrawLine(CurrentDrawingLine, false);

                        #region 继续画
                        this.CurrentDrawType = TVisioDrawType.DrawLine;

                        IsDrawingLine = true;

                       // Pf = ToNode.PointToScreen(new Point(ToNode.Width / 2, ToNode.Height / 2));
                      //  Pf = WorkPanel.PointToClient(Pf);
                      //  Pt = Pf;

                        Pf = new Point(Pt.X + this.HScrollCtl.Value, Pt.Y + this.VScrollCtl.Value);

                        CurrentDrawingLine = new TVisioLine(Pf, Pt);
                        CurrentDrawingLine.FromPosReal = new Point(Pf.X, Pf.Y);
                        CurrentDrawingLine.ToPosReal = new Point(Pt.X, Pt.Y);
                        CurrentDrawingLine.FromNode = ToNode;
                        this.IsMovingNode = false;
                        #endregion

                    }
                    else
                    {

                    }
                }
                else if (this.CurrentDrawType == TVisioDrawType.DrawStart)
                {
                    CreateOneNode(TVisioNodeType.StartBtn, e.Location);
                    this.CurrentDrawType = TVisioDrawType.DoNothing;
                }
                else if (this.CurrentDrawType == TVisioDrawType.DrawNode)
                {
                    CreateOneNode(TVisioNodeType.NodeBtn, e.Location);
                    this.CurrentDrawType = TVisioDrawType.DoNothing;
                }
                else if (this.CurrentDrawType == TVisioDrawType.DrawRouter)
                {
                    CreateOneNode(TVisioNodeType.RemBtn, e.Location);
                    this.CurrentDrawType = TVisioDrawType.DoNothing;
                }
                else if (this.CurrentDrawType == TVisioDrawType.DrawEndBtn)
                {
                    CreateOneNode(TVisioNodeType.EndBtn, e.Location);
                    this.CurrentDrawType = TVisioDrawType.DoNothing;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (IsDrawingLine)
                {
                    IsDrawingLine = false;
                    this.CurrentDrawType = TVisioDrawType.DoNothing;
                    this.DrawLine(CurrentDrawingLine,true);
                    return;
                }
                TVisioLine WKLine = MouseSelectLine(e.Location);
                if (WKLine != null)
                {
                    this.ContextPopMenu.Tag = WKLine;
                    Point P = this.WorkPanel.PointToScreen(e.Location);
                   
                    for (int i = 0; i < this.ContextPopMenu.Items.Count - 1; i++)
                    {
                        if (!this.ContextPopMenu.Items[i].Equals(CfgLineStyle))
                            this.ContextPopMenu.Items[i].Visible = false;
                    }
                    this.CfgLineStyle.Visible = true;
                    this.DeleteLineMenuItem.Visible = true;
                    this.ContextPopMenu.Show(P);
                }
                else
                {
                    this.ContextPopMenu.Tag = null;
                    Point P = this.WorkPanel.PointToScreen(e.Location);
                    for (int i = 0; i < this.ContextPopMenu.Items.Count ; i++)
                    {
                        this.ContextPopMenu.Items[i].Visible = false;
                    }
                    MouseOldPos = Cursor.Position;
                    this.SaveIDEMenuItem.Visible = true;
                    this.ToDesingMenuItem.Visible = true;
                    this.DrawRemMenuItem.Visible = true;
                    this.LoadOneFile.Visible = true;
                    this.LineItem.Visible = true;
                    this.LoadOnePageFile.Visible = true;
                    this.SaveOnePageFile.Visible = true;
                    DrawDotMenuItem.Visible = true;
                    this.SaveToOneFile.Visible = true;

                    this.ReleaseIdeItem.Visible = !this.SaveIDEMenuItem.Enabled;

                    if (this.SaveIDEMenuItem.Enabled)
                        this.ShowHideMenuItem.Visible = true;
                    else
                        this.ShowHideMenuItem.Visible = false;
                    this.ContextPopMenu.Show(P);
                }
            }
        }
        public Point MouseOldPos;
        public bool m_IsIDE = false;

        
        public TVisioNodeBtn CreateOneNode(TVisioNodeType NodeType, Point P)
        {
            int UniqNodeId = 0;
            for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
            {
                if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                {
                    TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                    UniqNodeId = Math.Max(UniqNodeId, WKNode.NodeId);
                }
            }
            UniqNodeId++;
       
            TVisioNodeBtn Node = new TVisioNodeBtn();
            Node.Tag = "";
            Node.NodeId = UniqNodeId;
            Node.MouseDown += this.Node_MouseDown;
            Node.MouseMove += new MouseEventHandler(this.Node_MouseMove);

            Node.FlatAppearance.BorderSize = 0;
            Node.FlatAppearance.MouseDownBackColor = Color.Transparent;
            Node.FlatAppearance.MouseOverBackColor = Color.Transparent;
            Node.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            Node.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            Node.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            if (WorkPanel is Form && NodeType != TVisioNodeType.PointBtn&&NodeType != TVisioNodeType.RemBtn)
            {
                P = WorkPanel.PointToClient(P);
                P = new Point(P.X + HScrollCtl.Value, P.Y + VScrollCtl.Value);//lgb 25
            }
            else
            {
               // P = new Point(P.X - HScrollCtl.Value, P.Y - VScrollCtl.Value);//lgb 25
            }

         
           
            Node.NodeType = NodeType;
         
            if (this.m_IsIDE)
            {
                Node.Cursor = Cursors.Hand;
            }

            WorkPanel.Controls.Add(Node);

            //if (Node.NodeType == TVisioNodeType.RemBtn)
            //{
            //    //Node.Image = Riches.Visio.Properties.Resources.route;
            //   // Node.Text = "备注信息";
            //}
            //else if (Node.NodeType == TVisioNodeType.StartBtn)
            //    Node.Image = Riches.Visio.Properties.Resources.start;
            //else if (Node.NodeType == TVisioNodeType.NodeBtn)
            //    Node.Image = Riches.Visio.Properties.Resources.node;
            //else if (Node.NodeType == TVisioNodeType.EndBtn)
            //    Node.Image = Riches.Visio.Properties.Resources.end;
            //else 
            if (Node.NodeType == TVisioNodeType.PointBtn)
            {
                Node.Size = new Size(8, 8);
                Node.BackColor = Color.Black;
            }

            //if (Node.NodeType == TVisioNodeType.StartBtn)
            //{
            //    Node.Text = "开始";
            //}
            //else if (Node.NodeType == TVisioNodeType.NodeBtn)
            //{
            //    Node.Text = "角色";
            //}
            //else if (Node.NodeType == TVisioNodeType.EndBtn)
            //{
            //    Node.Text = "结束";
            //}
            //else
            if (Node.NodeType == TVisioNodeType.RemBtn)
            {
                Node.Text = "备注信息";
            }

            if (Node.NodeType == TVisioNodeType.PointBtn)
            {
                Node.Text = "";
                if (IsReadOnly)
                    Node.Visible = false;
            }
           
            Node.Top = P.Y - Node.Height / 2;
            Node.Left = P.X - Node.Width / 2;
            this.m_IsModify = true;
            return Node;
        }

        private bool CheckLineIsPass(TVisioNodeBtn FromNode, TVisioNodeBtn ToNode)
        {
            foreach (TVisioLine WKLine in ToNode.WKLines)
            {
                if (FromNode == WKLine.FromNode)
                    if (ToNode == WKLine.ToNode)
                    {
                       
                        return false;
                    }
                if (FromNode == WKLine.ToNode)
                    if (ToNode == WKLine.FromNode)
                    {
                       
                        return false;
                    }
            }
            return true;
        }
        public void Node_MouseDown(object sender, MouseEventArgs e)
        {
          //  if (this.IsReadOnly) return;
            if (this.WorkFlowNodeMouseDown != null)
                this.WorkFlowNodeMouseDown(sender, e);
            TVisioNodeBtn ToNode = sender as TVisioNodeBtn;

            if (this.IsReadOnly)
            {
                ToNode.Cursor = Cursors.Default;
                return;
            }

            #region left button down
            if (e.Button == MouseButtons.Left)
            {
              
                if (this.CurrentDrawType == TVisioDrawType.DrawLine)
                {
                    if (this.IsDrawingLine)//画将完成任务.
                    {
                        if (!CheckLineIsPass(CurrentDrawingLine.FromNode, ToNode))
                            return;

                      
                        this.DrawLine(this.CurrentDrawingLine, true);
                       // Pt = ToNode.PointToScreen(new Point(ToNode.Width / 2, ToNode.Height / 2));
                       // Pt = new Point(e.Location.X - this.HScrollCtl.Value, e.Location.Y - this.VScrollCtl.Value);//lgb 25
                      //  Pt = WorkPanel.PointToClient(Pt);
                        Pt = ToNode.PointToScreen(new Point(ToNode.Width / 2, ToNode.Height / 2));
                        Pt = WorkPanel.PointToClient(Pt);
                        Pt = new Point(Pt.X + HScrollCtl.Value, Pt.Y + VScrollCtl.Value);


                        CurrentDrawingLine.ToPos = Pt;
                        ToNode.WKLines.Add(CurrentDrawingLine);
                        CurrentDrawingLine.ToNode = ToNode;
                        CurrentDrawingLine.FromNode.WKLines.Add(CurrentDrawingLine);
                        CurrentDrawingLine.CapStyle = 1;
                        CalNodeIntersect(CurrentDrawingLine);
                        this.DrawLine(CurrentDrawingLine, false);
                        this.IsDrawingLine = false;
                        this.CurrentDrawType = TVisioDrawType.DoNothing;
                        this.m_IsModify = true;
                    }
                    else//开始Drawing
                    {
                        //if (this.OnBeforeDrawOutLine != null)
                        //{
                        //    TVisioLineCheckArgs Args = new TVisioLineCheckArgs((TVisioNodeBtn)sender, null);
                        //    this.OnBeforeDrawOutLine(this, Args);
                        //    if (!Args.IsPass)
                        //        return;
                        //}

                        //if ((CurrentDrawingLine!=null)&&(this.OnBeforeDrawOutLine != null))
                        //{
                        //    TVisioLineCheckArgs Args = new TVisioLineCheckArgs(CurrentDrawingLine.FromNode, ToNode);
                        //    this.OnBeforeDrawOutLine(this, Args);
                        //    if (!Args.IsPass)
                        //        return;
                        //}
                        IsDrawingLine = true;
                        Pf = ToNode.PointToScreen(new Point(ToNode.Width / 2, ToNode.Height / 2));
                        Pf = WorkPanel.PointToClient(Pf);
                        Pt = Pf;
                        CurrentDrawingLine = new TVisioLine(Pf, Pt);
                        CurrentDrawingLine.FromPosReal = new Point(Pf.X, Pf.Y);
                        CurrentDrawingLine.ToPosReal = new Point(Pt.X, Pt.Y);
                        CurrentDrawingLine.FromNode = ToNode;
                    }
                    this.IsMovingNode = false;
                }
                else if (this.CurrentDrawType == TVisioDrawType.DrawStart)
                {
                    this.IsMovingNode = false;
                }
                else if (!this.IsMovingNode)
                {
                    Pf.X = e.X;
                    Pf.Y = e.Y;
                    this.IsMovingNode = true;
                }
                else
                {
                    Pf.X = e.X;
                    Pf.Y = e.Y;
                }
            }
            #endregion

            else if (e.Button == MouseButtons.Right)
            {
                TVisioNodeBtn Node = sender as TVisioNodeBtn;
                ContextMenuStrip BakStrip = Node.FindForm().ContextMenuStrip;
                Node.FindForm().ContextMenuStrip = null;
                this.ContextPopMenu.Tag = sender;
                Point P = Node.PointToScreen(e.Location);
                for (int i = 0; i < this.ContextPopMenu.Items.Count - 3; i++)
                {
                   
                    this.ContextPopMenu.Items[i].Visible = true;
                }
                CfgLineStyle.Visible = false;
                this.DeleteLineMenuItem.Visible = false;
                this.SetNodeTextMenuItem.Visible = true;

                this.ContextPopMenu.Show(P);
                Node.FindForm().ContextMenuStrip = BakStrip;
            }
        }
      
     
        private void WorkPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (OnDragDop != null)
            {
                OnDragDop(sender, e);
                if (e.Data == null)
                    return;
            }
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                return;

            object obj = e.Data.GetData(typeof(TMenuItemForIde));
            TMenuItemForIde Itm = obj as TMenuItemForIde;
            if (Itm == null)
                return;
            Point pp = new Point(e.X-this.HScrollCtl.Value, e.Y-this.VScrollCtl.Value); //new add
            TVisioNodeBtn WkNode = CreateOneNode(TVisioNodeType.NodeBtn, pp);
            WkNode.Text = Itm.Caption;
            WkNode.Menu = Itm.Name;
            WkNode.Tag = Itm.Tag;
            WkNode.SpaceClassName = Itm.SpaceNameClass;
          //  if (Itm.MyImage != null)
          //      WkNode.Image = Itm.MyImage;
            //WkNode.FlatAppearance.BorderSize = 1;
            Graphics g = this.WorkPanel.CreateGraphics();
            SizeF s = g.MeasureString(WkNode.Text, WkNode.Font);
            if (s.Width > WkNode.Width)
                WkNode.Width = (int)s.Width + 4;

            if (WkNode.Text.Length >= 4)
                WkNode.Height = 70;
            g.Dispose();
            this.CurrentDrawType = TVisioDrawType.DoNothing;
        }
        private void WorkPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;    
        }
        public void SetNodeImage(TVisioNodeBtn Node)
        {
            if (Node.NodeType == TVisioNodeType.RemBtn)
            {
               // Node.Image = Riches.Visio.Properties.Resources.route;
            }
            else if (Node.NodeType == TVisioNodeType.StartBtn)
                Node.Image = Riches.Visio.Properties.Resources.start;
            else if (Node.NodeType == TVisioNodeType.NodeBtn)
                Node.Image = Riches.Visio.Properties.Resources.node;
            else if (Node.NodeType == TVisioNodeType.EndBtn)
                Node.Image = Riches.Visio.Properties.Resources.end; 
        }
        public bool IsModified
        {
            get
            {
                return this.m_IsModify;
            }
            set
            {
                this.m_IsModify = value;
            }
        }
        public void ClearNodes()
        {
            for (int i = WorkPanel.Controls.Count - 1; i >= 0;i--)
            {
                if (WorkPanel.Controls[i] is TVisioNodeBtn)
                {
                    TVisioNodeBtn OneNode = WorkPanel.Controls[i] as TVisioNodeBtn;
                    DeleteNode(OneNode);
                }
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            for (int i = 0; i < this.WorkPanel.Controls.Count; i++)
            {
                if (this.WorkPanel.Controls[i] is TVisioNodeBtn)
                {
                    TVisioNodeBtn WKNode = this.WorkPanel.Controls[i] as TVisioNodeBtn;
                    WKNode.MouseDown -= this.Node_MouseDown;
                    WKNode.MouseMove -= this.Node_MouseMove;
                    WKNode.MouseLeave -= this.Node_MouseLeave;
                }
            }
            WorkPanel.MouseMove -= new MouseEventHandler(WorkPanel_MouseMove);
            WorkPanel.MouseDown -= new MouseEventHandler(WorkPanel_MouseDown);
            WorkPanel.MouseUp -= new MouseEventHandler(WorkPanel_MouseUp);
            WorkPanel.DragDrop -= new DragEventHandler(WorkPanel_DragDrop);
            WorkPanel.DragEnter -= new DragEventHandler(WorkPanel_DragEnter);

            this.DrawLineMenuItemSolid.Dispose();
            this.DeleteLineMenuItem.Dispose();
            this.DeleteNodeMenuItem.Dispose();
            this.DeleteAllNodeMenuItem.Dispose();
            this.ContextPopMenu.Dispose();
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }
}

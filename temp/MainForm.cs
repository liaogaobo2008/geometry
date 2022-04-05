using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using Riches.Visio.Geometry;
using Newtonsoft.Json;

namespace Riches.Visio
{
    public partial class MainForm : Form
    {

        List<ToolStripItem> MenuItemsOfLine = new List<ToolStripItem>();
        List<ToolStripMenuItem> MenuItemsOfDot = new List<ToolStripMenuItem>();
        
        public MainForm()
        {
            InitializeComponent();
            this.ManuCursor = GetCursor(Properties.Resources.画笔, 13, 34);
            DataTable dt = new DataTable();
           // string express = "Pow(x,2)*3+4*x+10";
           // express = express.Replace(" ", "");
           // express.

            //var t= dt.Compute("Cos(60)*(1+2+3)/5", "");

            this.MdiChildActivate += MainForm_MdiChildActivate;

            this.线型ToolStripMenuItem.Enabled = false;
            MenuItemsOfLine.Add(this.线型ToolStripMenuItem);
            this.MainMenuStrip.ItemAdded += MainMenuStrip_ItemAdded;
            foreach (ToolStripItem item in this.线型ToolStripMenuItem.DropDownItems)
            {  // ToolStripSeparator
                item.Enabled = false;
                MenuItemsOfLine.Add(item);
            }

            this.点型ToolStripMenuItem.Enabled = false;
            this.MenuItemsOfDot.Add(this.点型ToolStripMenuItem);
            foreach (ToolStripMenuItem item in this.点型ToolStripMenuItem.DropDownItems)
            {
                item.Enabled = false;
                MenuItemsOfDot.Add(item);
            }

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.Orange);
            colors.Add(Color.Yellow);
            colors.Add(Color.Green);
            colors.Add(Color.Blue);
            colors.Add(Color.Purple);
            foreach (var c in colors)
            {
                var item = new ToolStripMenuItem() { Name = c.ToString(), Text = c.ToString() };
                item.Click += ColorMenuitem_Click;
                item.Tag = c;
                颜色ToolStripMenuItem.DropDownItems.Add(item);
            }

            //InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            //FontFamily[] fontFamilies = installedFontCollection.Families;
            //foreach (var f in fontFamilies)
            //{
            //    var item = new ToolStripMenuItem() { Name = f.Name, Text = f.Name };
            //    item.Click += FontItem_Click;
            //    item.Tag = f;
            //    this.文本ToolStripMenuItem.DropDownItems.Add(item);
            //}

            this.文本ToolStripMenuItem.Enabled = false;
            this.颜色ToolStripMenuItem.Enabled = false;
            this.Shown += MainForm_Shown;
            LoadRecentFileList();
        }

        private void MainMenuStrip_ItemAdded(object sender, ToolStripItemEventArgs e)
        {
            if (e.Item.Text.Length == 0) //隐藏子窗体图标
           //|| e.Item.Text == "最小化(&N)" //隐藏最小化按钮
           //|| e.Item.Text == "还原(&R)" //隐藏还原按钮
           //|| e.Item.Text == "关闭(&C)") //隐藏最关闭按钮
            {
                e.Item.Visible = false;
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
           
            CreateMdiChild();
        }

        public MDIChildForm CreateMdiChild()
        {
            MDIChildForm mdiChild = new MDIChildForm();
            mdiChild.MdiParent = this;
            mdiChild.OnDrawModeChanged += MdiChild_OnDrawModeChanged;
            mdiChild.OnMyMouseMove += MdiChild_OnMyMouseMove;
            mdiChild.canvasHelper.ShapeActivChanged += CanvasHelper_ShapeActivChanged;
            mdiChild.FormClosing += MdiChild_FormClosing;
            mdiChild.Show();
            var itemForm = new ToolStripMenuItem()
            {
                Tag = mdiChild,
                Text = mdiChild.Text
            };
            itemForm.Click += ItemForm_Click;
            窗口ToolStripMenuItem.DropDownItems.Add(itemForm);
            return mdiChild;
        }

        private void ItemForm_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            var child = item.Tag as MDIChildForm;
            child.Activate();

            foreach (ToolStripMenuItem win in 窗口ToolStripMenuItem.DropDownItems)
            {
                if (win.Tag == child)
                {
                    win.Checked = true;
                }
                else win.Checked = false;
            }
        }

        private void MdiChild_OnMyMouseMove(object sender, MouseEventArgs e)
        {
            this.toolStripStatusLabelMoueXY.Text = String.Format("x={0},y={1}", e.X, e.Y);
        }

        private void ColorMenuitem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Color c = (Color)item.Tag;
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child != null)
            {
                foreach (var sh in child.canvasHelper.Shapes.Where(s => s.IsActive))
                    sh.Color = c;
            }
        }

        private void CanvasHelper_ShapeActivChanged(Geometry.CanvasHelper canvas, Geometry.IShape shape, bool isActive)
        {
            foreach (var item in MenuItemsOfLine)
                item.Enabled = false;
            foreach (var item in MenuItemsOfDot)
                item.Enabled = false;
            this.文本ToolStripMenuItem.Enabled = false;

            this.颜色ToolStripMenuItem.Enabled = canvas.Shapes.FirstOrDefault(sh => sh.IsActive) != null;

            bool line = canvas.Shapes.FirstOrDefault(sh => sh.IsActive && (sh.ShapeType == Geometry.ShapeType.Line || sh.ShapeType == Geometry.ShapeType.Line || sh.ShapeType == Geometry.ShapeType.ManualPen)) != null;
            if (line)
            {

                foreach (var item in MenuItemsOfLine)
                    item.Enabled = true;
            }
            bool textArea = canvas.Shapes.FirstOrDefault(sh => sh.IsActive && (sh.ShapeType == Geometry.ShapeType.Dot || sh.ShapeType == Geometry.ShapeType.RemAngle || sh.ShapeType == Geometry.ShapeType.TextArea)) != null;
            if (textArea)
            {
                this.文本ToolStripMenuItem.Enabled = true;
            }

            bool dot = canvas.Shapes.FirstOrDefault(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot) != null;
            if (dot)
            {
                foreach (var item in MenuItemsOfDot)
                {
                    item.Enabled = true;
                }
            }
            var p = canvas.Shapes.FirstOrDefault(sh => sh.ShapeType == ShapeType.Parabolic && sh.IsActive);
            修改绘制函数ToolStripMenuItem.Enabled = p != null;
        }
       
        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            MdiChild_OnDrawModeChanged(child.canvasHelper.DrawMode);
            this.定义坐标系ToolStripMenuItem.Checked=child.canvasHelper.IsDrawCoordinate;
            this.显示网格ToolStripMenuItem.Checked= child.canvasHelper.IsDrawGrid;
        }

        public Cursor GetCursor(Bitmap img, int hotSpotX = 0, int hotSpotY = 0)
        {
            Bitmap curImg = new Bitmap(img.Width * 2, img.Height * 2);
            Graphics g = Graphics.FromImage(curImg);

            g.Clear(Color.FromArgb(0, 0, 0, 0));
            // g.DrawRectangle(Pens.Red, 0, 0, curImg.Width-1, curImg.Height-1);
            g.DrawImage(img, img.Width - hotSpotX, img.Height - hotSpotY, img.Width, img.Height);
            Cursor cur = new Cursor(curImg.GetHicon());

            g.Dispose();
            curImg.Dispose();

            return cur;

        }
        private void 新建文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateMdiChild();
        }

     
        private void MdiChild_OnDrawModeChanged(Geometry.DrawMode drawMode)
        {
            foreach (var ctl in panel2.Controls)
            {
                if (ctl.GetType() == typeof(PictureBox))
                {
                    PictureBox pic = ctl as PictureBox;
                    var dr = (Geometry.DrawMode)Convert.ToInt32(pic.Tag.ToString());
                    if (dr == drawMode)
                        pic.BackColor = Color.DarkGray;
                    else
                        pic.BackColor = panel2.BackColor;
                }
            }
        }

        private void CanvasHelper_OnDrawModeChanged(Geometry.DrawMode drawMode)
        {
          
        }
        public MDIChildForm CurrentChildForm
        {
            get
            {
                var child = this.ActiveMdiChild as MDIChildForm;
                
                return child;
            }
        }
        private void ButtonShapes_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;

            foreach ( var ctl in panel2.Controls)
            {
                if (ctl.GetType() == typeof(PictureBox))
                {
                    PictureBox pic = ctl as PictureBox;
                    pic.BackColor = panel2.BackColor;
                }
            }
            PictureBox btn = sender as PictureBox;
            btn.BackColor = Color.WhiteSmoke;
            child.canvasHelper.DrawMode = (Geometry.DrawMode)(Convert.ToInt32(btn.Tag.ToString()));

            if (child.canvasHelper.DrawMode == Geometry.DrawMode.ManulWrite)
                child.canvasHelper.Cursor = ManuCursor;
            else
                child.canvasHelper.Cursor = Cursors.Default;
        }

        private void pictureBoxOpen_Click(object sender, EventArgs e)
        {
            OpenFile();
        }
        private void OpenFile()
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            var mdiChild = child!=null&&child.canvasHelper.Shapes.Count==0? child:this.CreateMdiChild();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "(*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                mdiChild.canvasHelper.LoadFile(openFileDialog.FileName);
                foreach (ToolStripMenuItem item in 窗口ToolStripMenuItem.DropDownItems)
                {
                    if (item.Tag == mdiChild)
                    {
                        item.Text = mdiChild.Text;
                        item.Checked =true ;
                    }
                    else
                        item.Checked = false;
                }

               
                if ((最近文件ToolStripMenuItem.DropDownItems.Count > 0 &&
                      最近文件ToolStripMenuItem.DropDownItems[0].Text != mdiChild.Text)
                      || 最近文件ToolStripMenuItem.DropDownItems.Count==0)
                {
                    var item = new ToolStripMenuItem() { Text = mdiChild.Text };
                    最近文件ToolStripMenuItem.DropDownItems.Add(item);
                    item.Click += RecentFileItem_Click;
                }
                SaveRecentFileList();
            }
        }

        private void OpenFile(string fileName)
        {
           
            var child = this.ActiveMdiChild as MDIChildForm;
            var mdiChild = child != null && child.canvasHelper.Shapes.Count == 0 ? child : this.CreateMdiChild();
          
            {
                mdiChild.canvasHelper.LoadFile(fileName);
                foreach (ToolStripMenuItem item in 窗口ToolStripMenuItem.DropDownItems)
                {
                    if (item.Tag == mdiChild)
                    {
                        item.Text = mdiChild.Text;
                        item.Checked = true;
                    }
                    else
                        item.Checked = false;
                }


                if ((最近文件ToolStripMenuItem.DropDownItems.Count > 0 &&
                      最近文件ToolStripMenuItem.DropDownItems[0].Text != mdiChild.Text)
                      || 最近文件ToolStripMenuItem.DropDownItems.Count == 0)
                {
                    var item = new ToolStripMenuItem() { Text = mdiChild.Text };
                    最近文件ToolStripMenuItem.DropDownItems.Add(item);
                    item.Click += RecentFileItem_Click;
                }
                SaveRecentFileList();
            }
        }

        private void SaveRecentFileList()
        {
            StringBuilder sbd = new StringBuilder();
            foreach (ToolStripMenuItem f in 最近文件ToolStripMenuItem.DropDownItems)
            {
                sbd.Append(f.Text + "\n");
            }
            System.IO.File.WriteAllText(Application.StartupPath + "\\his.ini", sbd.ToString());

        }
        private void LoadRecentFileList()
        {
            if (!System.IO.File.Exists(Application.StartupPath + "\\his.ini"))
                return;

            var text = System.IO.File.ReadAllText(Application.StartupPath + "\\his.ini");
            var files = text.Split('\n');
            foreach (var f in files)
            {
                if (f.Length == 0)
                    continue;
                var item = new ToolStripMenuItem() { Text = f };
                最近文件ToolStripMenuItem.DropDownItems.Add(item);
                item.Click += RecentFileItem_Click;
            }
        }
        private Cursor ManuCursor;
        private void pictureBoxSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        private bool Save()
        { 
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return false;

            if (child.canvasHelper.IsAdd)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "(*.json)|*.json";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    child.canvasHelper.SaveFile(saveFileDialog.FileName);
                    foreach (ToolStripMenuItem item in 窗口ToolStripMenuItem.DropDownItems)
                    {
                        if (item.Tag == child)
                            item.Text = child.Text;
                    }

                    if ((最近文件ToolStripMenuItem.DropDownItems.Count > 0 &&
                       最近文件ToolStripMenuItem.DropDownItems[0].Text != child.Text)
                       || 最近文件ToolStripMenuItem.DropDownItems.Count == 0)
                    {
                        var item = new ToolStripMenuItem() { Text = child.Text };
                        最近文件ToolStripMenuItem.DropDownItems.Add(item);
                        item.Click += RecentFileItem_Click;
                    }
                    SaveRecentFileList();
                }
                else
                    return false;
            }
            else  
                child.canvasHelper.SaveFile(child.canvasHelper.FilePathName);
            return true; 
        }

        private void RecentFileItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            OpenFile(item.Text);
        }

        private void buttonCanel_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Revoke();
        }

        private void buttonRedo_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Redo();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.SelectAll();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.ClearShapes();
        }

        private void 另存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            if (child.canvasHelper.IsAdd)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "(*.json)|*.json";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    child.canvasHelper.SaveFile(saveFileDialog.FileName);
                    if ((最近文件ToolStripMenuItem.DropDownItems.Count > 0 &&
                     最近文件ToolStripMenuItem.DropDownItems[0].Text != child.Text)
                     || 最近文件ToolStripMenuItem.DropDownItems.Count == 0)
                    {
                        var item = new ToolStripMenuItem() { Text = child.Text };
                        最近文件ToolStripMenuItem.DropDownItems.Add(item);
                        item.Click += RecentFileItem_Click;
                    }
                    SaveRecentFileList();
                }
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 撤销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Revoke();
        }

        private void 重做ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Redo();
        }

        private void 全选中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.SelectAll();
        }
        private void MdiChild_FormClosing(object sender, FormClosingEventArgs e)
        {
            MDIChildForm child = sender as MDIChildForm;
            if (child.canvasHelper.IsMoified)
            {
                var res = MessageBox.Show("数据发生改变，是否保存?", "提示", MessageBoxButtons.YesNoCancel);
                if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (res == DialogResult.Yes)
                {
                    if (!Save())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            foreach (ToolStripMenuItem item in 窗口ToolStripMenuItem.DropDownItems)
            {
                if (item.Tag == child)
                {
                    item.Click -= ItemForm_Click;
                    窗口ToolStripMenuItem.DropDownItems.Remove(item);
                    break;
                }
            }
            child.OnDrawModeChanged -= MdiChild_OnDrawModeChanged;
            child.canvasHelper.ShapeActivChanged -= CanvasHelper_ShapeActivChanged;
            child.OnMyMouseMove -= MdiChild_OnMyMouseMove;
        }

        private void 编辑ToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            MDIChildForm child = this.ActiveMdiChild as MDIChildForm;
            if(child!=null)
            {
                撤销ToolStripMenuItem.Enabled= child.canvasHelper.RevokeAllObjects.Count > 0;
                重做ToolStripMenuItem.Enabled = child.canvasHelper.RedoAllObjects.Count > 0;
                复制ToolStripMenuItem.Enabled = child.canvasHelper.Shapes.FirstOrDefault(t => t.IsActive) != null;
                剪切ToolStripMenuItem.Enabled = 复制ToolStripMenuItem.Enabled;
                删除ToolStripMenuItem.Enabled = 复制ToolStripMenuItem.Enabled;
                粘贴ToolStripMenuItem.Enabled = canPaste|| Clipboard.ContainsImage();
            }
            else
            {
                撤销ToolStripMenuItem.Enabled = false;
                重做ToolStripMenuItem.Enabled = false;
                复制ToolStripMenuItem.Enabled = false;
                剪切ToolStripMenuItem.Enabled = false;
                删除ToolStripMenuItem.Enabled = false;
                粘贴ToolStripMenuItem.Enabled=false;
            }
        }

        private bool canPaste = false;
        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Cut();
            canPaste = true;
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.CloneData();
            canPaste = true;
        }
        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.Paste();
        }
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            child.canvasHelper.DeleteSelected();
        }

        private void 实线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            foreach (Geometry.Line line in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Line))
                 line.DashStyle = (System.Drawing.Drawing2D.DashStyle)(Convert.ToInt32(item.Tag.ToString()));
        }

        private void 极细ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            foreach (Geometry.Line line in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Line))
                line.Width = (Convert.ToInt32(item.Tag.ToString()));
        }

        private void 最小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            foreach (Geometry.Dot line in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
                line.Size = (Convert.ToInt32(item.Tag.ToString()));
        }

        private void adjustFontsize(Geometry.CanvasHelper canvasHelper,  bool isAdd)
        {
            foreach (Geometry.Dot dot in canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
            {
                var s = dot.Font.Size + (isAdd ? 1f : -1f);
                if (s > 0)
                    dot.Font = new Font(dot.Font.FontFamily, s);
            }
            foreach (Geometry.RemAngle r in canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.RemAngle))
            {
                var s = r.Font.Size + (isAdd ? 1f : -1f);
                if (s > 0)
                    r.Font = new Font(r.Font.FontFamily, s);
            }
            foreach (Geometry.TextArea t in canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.TextArea))
            {
                var s = t.Font.Size + (isAdd ? 1f : -1f);
                if ( s> 0)
                    t.Font = new Font(t.Font.FontFamily, s);
            }
        }
        private void 增大尺寸ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            adjustFontsize(child.canvasHelper,true);
        }

        private void 减少尺寸ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            adjustFontsize(child.canvasHelper, false);
        }

        private void 粗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            foreach (Geometry.Dot dot in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
                dot.Font = new Font(dot.Font,  FontStyle.Bold);
            foreach (Geometry.RemAngle r in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.RemAngle))
                r.Font = new Font(r.Font, FontStyle.Bold);
            foreach (Geometry.TextArea t in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.TextArea))
                t.Font = new Font(t.Font, FontStyle.Bold);
        }

        private void 斜体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            foreach (Geometry.Dot dot in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
                dot.Font = new Font(dot.Font, FontStyle.Italic);
            foreach (Geometry.RemAngle r in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.RemAngle))
                r.Font = new Font(r.Font, FontStyle.Italic);
            foreach (Geometry.TextArea t in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.TextArea))
                t.Font = new Font(t.Font, FontStyle.Italic);
        }

        private void 下画线ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            foreach (Geometry.Dot dot in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
                dot.Font = new Font(dot.Font, FontStyle.Underline);
            foreach (Geometry.RemAngle r in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.RemAngle))
                r.Font = new Font(r.Font, FontStyle.Underline);
            foreach (Geometry.TextArea t in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.TextArea))
                t.Font = new Font(t.Font, FontStyle.Underline);
        }
        private void FontItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;

            ToolStripMenuItem item = sender as ToolStripMenuItem;
            FontFamily f = (FontFamily)item.Tag;

            foreach (Geometry.Dot dot in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Dot))
                dot.Font = new Font(f, dot.Font.Size, dot.Font.Style);
            foreach (Geometry.RemAngle r in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.RemAngle))
                r.Font = new Font(f, r.Font.Size, r.Font.Style);
            foreach (Geometry.TextArea t in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.TextArea))
                t.Font = new Font(f, t.Font.Size, t.Font.Style);
        }

        private void 加粗ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            foreach (Geometry.Line l in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Line))
            {
                l.Width += 1;
            }
        }

        private void 减细ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            foreach (Geometry.Line l in child.canvasHelper.Shapes.Where(sh => sh.IsActive && sh.ShapeType == Geometry.ShapeType.Line))
            {
                l.Width -= 1;
            }
        }

        private void 定义坐标系ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            this.定义坐标系ToolStripMenuItem.Checked = !this.定义坐标系ToolStripMenuItem.Checked;
            child.canvasHelper.IsDrawCoordinate = this.定义坐标系ToolStripMenuItem.Checked;
        }

        private void 显示网格ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            this.显示网格ToolStripMenuItem.Checked = !this.显示网格ToolStripMenuItem.Checked;
            child.canvasHelper.IsDrawGrid = this.显示网格ToolStripMenuItem.Checked;
        }

        private void 绘制函数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            FormAddParabolic formAddParabolic = new FormAddParabolic();
            if(formAddParabolic.ShowDialog()== DialogResult.OK)
            {
                child.canvasHelper.AddShape2(new Geometry.Parabolic() { Express= formAddParabolic.Express });
                child.canvasHelper.Update();
            }
        }

        private void 修改绘制函数ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            var p = child.canvasHelper.Shapes.FirstOrDefault(sh => sh.ShapeType == ShapeType.Parabolic && sh.IsActive);
            if(p==null)
            {
                MessageBox.Show("请先选择需要修改的曲线");
                return;
            }
            
            FormAddParabolic formAddParabolic = new FormAddParabolic(p as Parabolic);
            
            if (formAddParabolic.ShowDialog() == DialogResult.OK)
            {
                p.IsDirty = true;
                child.canvasHelper.Update();
            }
        }

        private void buttonFront_Click(object sender, EventArgs e)
        {
          
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {

        }

        private void buttonBottom_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            var sh=child.canvasHelper.Shapes.FirstOrDefault(t => t.IsActive);
            if (sh == null)
                return;

            sh.Id = -1;
            int id = 0;
            foreach (var s in child.canvasHelper.Shapes.OrderBy(t=>t.Id))
            {
                s.Id = id;
                ++id;
            }
            child.canvasHelper.Update();
        }

        private void buttonTop_Click(object sender, EventArgs e)
        {
            var child = this.ActiveMdiChild as MDIChildForm;
            if (child == null)
                return;
            var sh = child.canvasHelper.Shapes.FirstOrDefault(t => t.IsActive);
            if (sh == null)
                return;
            sh.Id = int.MaxValue;
            int id = 0;
            foreach (var s in child.canvasHelper.Shapes.OrderBy(t => t.Id))
            {
                s.Id = id;
                ++id;
            }
            child.canvasHelper.Update();
        }
    }
}

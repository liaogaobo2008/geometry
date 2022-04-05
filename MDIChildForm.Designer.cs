
namespace Riches.Visio
{
    partial class MDIChildForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.canvasPanel1 = new Riches.Visio.CanvasPanel();
            this.buttonMove = new System.Windows.Forms.Button();
            this.canvasPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // canvasPanel1
            // 
            this.canvasPanel1.AutoScroll = true;
            this.canvasPanel1.BackColor = System.Drawing.Color.White;
            this.canvasPanel1.Controls.Add(this.buttonMove);
            this.canvasPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvasPanel1.Location = new System.Drawing.Point(0, 0);
            this.canvasPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.canvasPanel1.Name = "canvasPanel1";
            this.canvasPanel1.Size = new System.Drawing.Size(1059, 504);
            this.canvasPanel1.TabIndex = 0;
            this.canvasPanel1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.canvasPanel1_Scroll);
            this.canvasPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.canvasPanel1_Paint);
            // 
            // buttonMove
            // 
            this.buttonMove.Location = new System.Drawing.Point(804, 408);
            this.buttonMove.Name = "buttonMove";
            this.buttonMove.Size = new System.Drawing.Size(0, 0);
            this.buttonMove.TabIndex = 0;
            this.buttonMove.UseVisualStyleBackColor = true;
            this.buttonMove.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // MDIChildForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 504);
            this.Controls.Add(this.canvasPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MDIChildForm";
            this.ShowIcon = false;
            this.Text = "未命名";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.canvasPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CanvasPanel canvasPanel1;
        public System.Windows.Forms.Button buttonMove;
    }
}
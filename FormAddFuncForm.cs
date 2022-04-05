using ExpressionEvaluator;
using Riches.Visio.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Riches.Visio
{
    public partial class FormAddParabolic : Form
    {
        private Parabolic Parabolic;
        public FormAddParabolic()
        {
            InitializeComponent();
        }

        

        public FormAddParabolic(Parabolic  parabolic)
        {
            Parabolic = parabolic;
            InitializeComponent();
            this.richTextBoxExpress.Text = Parabolic.Express;
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && e.KeyChar != 46 && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

      
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if(this.richTextBoxExpress.Text.Trim().Length==0)
            {         
                MessageBox.Show("表达式不能为空");
            }

            var t = this.richTextBoxExpress.Text.Replace("X", this.textBoxTestValue.Text).Replace("**", "?");
            t =t.Replace("x", this.textBoxTestValue.Text);
           // try
            {
                var res = ParaseExpress(t);
                if(Parabolic!=null)
                    Parabolic.Express= this.richTextBoxExpress.Text;
            }
            //catch (Exception ex)
            //{
            //    MessageBox.Show("表达式有错误:\n" + ex.Message);
            //    return;
            //}
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonNumber_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Clipboard.SetText(btn.Text);
            this.richTextBoxExpress.Paste();
           
        }

        private void buttonPow_Click(object sender, EventArgs e)
        {
            Clipboard.SetText("**");
            this.richTextBoxExpress.Paste();
        }
        private void buttonExpress_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Clipboard.SetText(btn.Text+"(");
            this.richTextBoxExpress.Paste();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            //获取textBox2 中的光标

          //  string newstr = richTextBoxExpress.Text;
            int index = richTextBoxExpress.SelectionStart;
            if (index > 0)
            {
                richTextBoxExpress.Text = richTextBoxExpress.Text.Remove(index - 1, 1);
                richTextBoxExpress.SelectionStart = index - 1;
            }
         //   richTextBoxExpress.Focus();

            // this.richTextBoxExpress.Text= this.richTextBoxExpress.Text.Remove(richTextBoxExpress.SelectionStart-1, 1);
        }
       

        public string Express
        {
            get
            {
                return this.richTextBoxExpress.Text;
            }
            set
            {
                this.richTextBoxExpress.Text = value;
            }
        }

        private object ParaseExpress2(string str)
        {
            DataTable dt = new DataTable();
            Stack<char> stack = new Stack<char>();
            double res = 0f;// (-3)?2-2*Abs((-3))-3
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
                        res = Convert.ToDouble(dt.Compute(strExp, ""));

                        t = stack.Peek();
                        Console.WriteLine(strExp);
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
                            Console.WriteLine(strFun);

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
                                res = Math.Log10(res);
                                str = str.Replace(strFun + strExp, res.ToString());
                                break;
                            }
                            else if (strFun == "Ln")
                            {
                                res = Math.Log(res);
                                str = str.Replace(strFun + strExp, res.ToString());
                                break;
                            }
                            else
                            {
                                Console.WriteLine("未识别的函数:" + strFun);
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
                stack.Clear();
                int m = 0;
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
                        res = Convert.ToDouble(dt.Compute(strExp, ""));
                        res = Math.Pow(res, me);
                        str = str.Replace(strExp + "?" + fang, res.ToString());
                        break;
                    }
                }

            }

            return dt.Compute(str, "");
        }


        private object ParaseExpress(string str)
        {
            DataTable dt = new DataTable();

            str = ParasExp(str,dt);
            double res = 0f;//(abs(2**2)
            StringBuilder blanceExpress = new StringBuilder();
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
                            c = str[i+1];//'('后面还是公式
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
                        var resStr = res.ToString("N10").Replace(",","");
                        if (resStr.Contains("-∞"))
                            return int.MinValue;
                        else if (resStr.Contains("∞"))
                            return int.MaxValue;
                        if (strFun == "Cos")
                        {
                            res = Math.Cos(res);
                            blanceExpress.Append(res.ToString());
                          //  break;
                        }
                        else if (strFun == "Sin")
                        {
                            res = Math.Sin(res);
                            blanceExpress.Append( res.ToString());
                           // break;
                        }

                        else if (strFun == "Tan")
                        {
                            res = Math.Tan(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                            //  break;
                        }
                        else if (strFun == "Sqrt")
                        {
                            res = Math.Sqrt(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                            // break;
                        }
                        else if (strFun == "Asin")
                        {
                            res = Math.Asin(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                            //  break;
                        }
                        else if (strFun == "Acos")
                        {
                            res = Math.Acos(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                            //  break;
                        }
                        else if (strFun == "Atan")
                        {
                            res = Math.Atan(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                        }
                        else if (strFun == "Abs")
                        {
                            res = Math.Abs(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                        }
                        else if (strFun == "Lg")
                        {
                            if (res <= 0)
                                return int.MaxValue;
                            res = Math.Log10(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                        }
                        else if (strFun == "Ln")
                        {
                            if (res <= 0)
                                return int.MaxValue;
                            res =  Math.Log(res);
                            blanceExpress.Append("(" + res.ToString() + ")");
                        }
                        else
                        {
                            Console.WriteLine("未识别的函数:" + strFun);
                            throw new Exception("未识别的函数:" + strFun);
                        }
                       // break;
                    }
                    else
                    {
                        blanceExpress.Append(c);
                    }
                }

                str = blanceExpress.ToString();
            }


            str = ParasExp(str,dt);

            if (str.Contains("-∞"))
                return int.MinValue;
            else if (str.Contains("∞"))
                return int.MaxValue;
            return dt.Compute(str, "");
        }

        private string ParasExp(string str,DataTable dt)
        {
            double res = 0f;//(abs(2**2)
            int idx = 0;
            Stack<char> stack = new Stack<char>();
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
                stack.Clear();
                int m = 0;
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
                        res = Convert.ToDouble(dt.Compute(strExp, ""));
                        res = Math.Pow(res, me);
                        str = str.Replace(strExp + "?" + fang, res.ToString("N10").Replace(",", ""));
                        break;
                    }
                }
            }
            return str;
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            var t = this.richTextBoxExpress.Text.Replace("X","x").Replace("x","("+ this.textBoxTestValue.Text+")").Replace("**", "?");
            t = t.Replace("x", this.textBoxTestValue.Text);
           // Math.lo
            try
            {
                var res = ParaseExpress(t);
                MessageBox.Show(res.ToString());
            }
            catch(Exception ex)
            {
                MessageBox.Show("表达式有错误:\n" + ex.Message);
            }
        }
        private void DrawExpress(string express,Graphics g)
        {
            Point txtPos = new Point(10, 10);
            string powStr = express.Replace("**", "?");
            if (powStr.Contains('?'))
            {
                g.DrawString("f(x)=", this.Font, Brushes.Black, txtPos);
                txtPos.X += TextRenderer.MeasureText("f(x)=", this.Font).Width;
                StringBuilder sbdPrex = new StringBuilder();
                for (int i = 0; i < powStr.Length; i++)
                {
                    char c = powStr[i];
                    if (c == '?')
                    {
                        var str = sbdPrex.ToString();
                        g.DrawString(str, this.Font, Brushes.Black, txtPos);
                        sbdPrex.Clear();

                        txtPos.X += TextRenderer.MeasureText(str, this.Font).Width - 5;

                        StringBuilder sbd = new StringBuilder();
                        i++;
                        while (i < powStr.Length)
                        {
                            c = powStr[i];
                            if (char.IsDigit(c))
                                sbd.Append(c);
                            else
                            {
                                sbdPrex.Append(c);
                                break;
                            }
                            i++;
                        }
                        str = sbd.ToString();
                        txtPos.Y -= 5;
                        var font = new Font(this.Font.FontFamily, 8f);
                        g.DrawString(str, font, Brushes.Black, txtPos);
                        txtPos.X += TextRenderer.MeasureText(str, font).Width - 5;
                        txtPos.Y += 5;
                    }
                    else
                    {
                        sbdPrex.Append(c);
                    }
                }
                var str2 = sbdPrex.ToString();
                if (str2.Length > 0)
                    g.DrawString(str2, this.Font, Brushes.Black, txtPos);
            }
            else
                g.DrawString("f(x)=" + this.Express, this.Font, Brushes.Black, txtPos);

        }

        private void richTextBoxExpress_TextChanged(object sender, EventArgs e)
        {
            using (var g = Graphics.FromHwnd(this.panel1.Handle))
            {
                g.Clear(this.panel1.BackColor);
                DrawExpress(this.richTextBoxExpress.Text, g);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.panel1.BackColor);
            DrawExpress(this.richTextBoxExpress.Text, e.Graphics);
        }
    }
}

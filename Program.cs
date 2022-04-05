using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Riches.Visio
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

          //  DrawExpress2(Point.Empty, "((x+2)/(x+1))/2+(x+6)/9");
           // return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());//MainForm 
        }

        private static void DrawExpress2(Point p, string express)
        {   //  x/2/2+x/2
            // (x+2)/2+3/4
            //(x+2)/(x+1)/
            StringBuilder preWord = new StringBuilder();
            Point prefixP;
            Point pos = new Point(0, 0);
            int wp = 0;
            for (int i = 0; i < express.Length; i++)
            {
                var c = express[i];
                if (c == '/')
                {
                    prefixP = p;
                    if (preWord.Length > 0)
                        Console.WriteLine("pos:"+pos+"->"+ preWord.ToString());
                   
                    //   g.DrawString(preWord.ToString(), Font, Brushes.Black, prefixP);

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

                    pos.Y++;
                    if (word.Length > 0)
                        Console.WriteLine("pos:" + pos + "->" + word.ToString());

                    p.Y = prefixP.Y;

                    preWord.Clear();
                    pos.Y--;
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
                if (preWord.Length > 0)
                    Console.WriteLine("pos:" + pos + "->" + preWord.ToString());
                //  g.DrawString(preWord.ToString(), Font, Brushes.Black, p);
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Riches.Visio
{
    namespace Geometry
    {
        public struct KBLine
        {
            public float K;
            public float B;
        }
        public class MyMath
        {
            // 设圆心(x0, y0)，半径r
            public static bool IsInCircle(
               float x0, float y0,
               float r,
               float x, float y)
            {
                return Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0)) <= r;
            }
            public static bool PointInCircle(PointF p, float r, Point c, int diff = 3)
            {
                var d = Math.Sqrt((c.X - p.X) * (c.X - p.X) + (c.Y - p.Y) * (c.Y - p.Y));
                return Math.Abs(d - r) <= diff;
            }


            public static Point Intersect(KBLine l1, KBLine l2)//l2是框框的线
            {
                if (l1.K == l2.K)//平行
                    return Point.Empty;

                if (l2.K == float.MaxValue)//l2垂直X
                {
                    return new Point((int)l2.B, (int)(l1.K * l2.B + l1.B));
                }
                else if (l1.K == float.MaxValue)
                {//2个相互垂直，l2平行x
                 // return new Point((int)l1.B, (int)l2.B);
                    return new Point((int)l1.B, (int)(l2.K * l1.B + l2.B));
                }
                else if (l2.K == 0)
                {// 前面排除了垂直X，l1肯定不为0

                    var y = l2.B;
                    var x = (y - l1.B) / l1.K;
                    return new Point((int)x, (int)y);
                }
                else if (l1.K == 0)
                {//l2肯定不为0
                 //  return new Point((int)l2.B,(int)l1.B);
                    var y = l1.B;
                    var x = (y - l2.B) / l2.K;
                    return new Point((int)x, (int)y);
                }
                else
                {//都不垂直
                    var x = (l2.B - l1.B) / (l1.K - l2.K);
                    var y = l1.K * x + l1.B;
                    return new Point((int)x, (int)y);
                }
            }

            public static Point Intersect(Line l1, Line l2)//l2是框框的线
            {
                if (l1.K == l2.K)//平行
                    return Point.Empty;

                if (l2.K == float.MaxValue)//l2垂直X
                {
                    return new Point((int)l2.B, (int)(l1.K * l2.B + l1.B));
                }
                else if (l1.K == float.MaxValue)
                {//2个相互垂直，l2平行x
                 // return new Point((int)l1.B, (int)l2.B);
                    return new Point((int)l1.B, (int)(l2.K * l1.B + l2.B));
                }
                else if (l2.K == 0)
                {// 前面排除了垂直X，l1肯定不为0

                    var y = l2.B;
                    var x = (y - l1.B) / l1.K;
                    return new Point((int)x, (int)y);
                }
                else if (l1.K == 0)
                {//l2肯定不为0
                 //  return new Point((int)l2.B,(int)l1.B);
                    var y = l1.B;
                    var x = (y - l2.B) / l2.K;
                    return new Point((int)x, (int)y);
                }
                else
                {//都不垂直
                    var x = (l2.B - l1.B) / (l1.K - l2.K);
                    var y = l1.K * x + l1.B;

                    if (x > Math.Min(l1.From.X, l1.To.X) && x < Math.Max(l1.To.X, l1.From.X))
                        return new Point((int)x, (int)y);
                    else
                        return Point.Empty;
                }
            }

            private static KBLine GetKBLineFrom2Point(PointF f, PointF t)
            {
                KBLine kb = new KBLine();
                if (f.X == t.X)
                {
                    kb.K = float.MaxValue;
                    kb.B = f.X;
                }
                else
                {
                    kb.K = (f.Y - t.Y) / (f.X - t.X);
                    kb.B = f.Y - kb.K * f.X;
                }
                return kb;
            }
            private static bool PInLine(Point p, Line line)
            {
                return p.X >= Math.Min(line.From.X, line.To.X) && p.X <= Math.Max(line.From.X, line.To.X)
                  && p.Y >= Math.Min(line.From.Y, line.To.Y) && p.Y <= Math.Max(line.From.Y, line.To.Y);
            }

            public static bool CircleIntersectRect(Rectangle rect, Circle circle)
            {
                if (rect.Contains(circle.Center.P))
                    return true;
              
                var rect2 = new Rectangle((int)(rect.Left - circle.Center.X), (int)(rect.Top - circle.Center.Y), rect.Width, rect.Height);
                if (Math.Abs(rect2.Top) < circle.Diam / 2)
                {
                    var x1 = Math.Sqrt(circle.Diam * circle.Diam / 4 - rect2.Top * rect2.Top);
                    var x2 = -x1;
                    if ((x1 >= rect2.Left && x1 <= rect2.Right)
                        || (x2 >= rect2.Left && x2 <= rect2.Right))
                    {
                        return true;
                    }
                }
                if (Math.Abs(rect2.Bottom) < circle.Diam / 2)
                {
                    var x1 = Math.Sqrt(circle.Diam * circle.Diam / 4 - rect2.Bottom * rect2.Bottom);
                    var x2 = -x1;
                    if ((x1 >= rect2.Left && x1 <= rect2.Right) || (x2 >= rect2.Left && x2 <= rect2.Right))
                    {
                        return true;
                    }
                }
                return false;

            }
            public static bool LineIntersectRect(Line line, Rectangle rectangle)
            {
                var scRect = rectangle;
                scRect.Inflate(1, 1);

                if (scRect.Contains(line.From.P) || scRect.Contains(line.To.P))
                    return true;
                var kb0 = GetKValueFromLine(line);
                var kb1 = GetKBLineFrom2Point(rectangle.Location, new PointF(rectangle.Right, rectangle.Top));
                var p1 = Intersect(kb0, kb1);
                // Console.WriteLine("p1:" + p1);
                if (scRect.Contains(p1))
                {
                    // Console.WriteLine("p1 ok");
                    if (PInLine(p1, line))
                        return true;
                }

                var kb2 = GetKBLineFrom2Point(new PointF(rectangle.Right, rectangle.Top), new PointF(rectangle.Right, rectangle.Bottom));
                var p2 = Intersect(kb0, kb2);
                if (p2.X > 190)
                {

                }
                Console.WriteLine("p2=" + p2);
                if (scRect.Contains(p2))
                {
                    if (PInLine(p2, line))
                        return true;
                }

                var kb3 = GetKBLineFrom2Point(new PointF(rectangle.Left, rectangle.Bottom), new PointF(rectangle.Right, rectangle.Bottom));
                var p3 = Intersect(kb0, kb3);
                if (scRect.Contains(p3) && PInLine(p3, line))
                    return true;

                var kb4 = GetKBLineFrom2Point(new PointF(rectangle.Left, rectangle.Bottom), rectangle.Location);
                var p4 = Intersect(kb0, kb4);
                if (scRect.Contains(p4) && PInLine(p4, line))
                    return true;
                return false;
            }
            private static KBLine GetKValueFromLine(Line line)
            {
                KBLine kBLine;
                kBLine.K = line.K;
                kBLine.B = line.B;

                //if (line.K == float.MaxValue)
                //    kBLine.B = line.From.X;
                //else
                //    kBLine.B = (line.From.Y - kBLine.K * line.From.X);

                return kBLine;
            }
           public const double RadUnit = 360 / (2 * Math.PI);
            //线段和X轴的夹角，C为起点，P为终点。
            private static float AngleFrom(PointF c, PointF p)
            {//p-c顺时针方向的角度（0-360）
                var p1 = new PointF(p.X - c.X, p.Y - c.Y);
                var k = p.Y == c.Y ? int.MaxValue : (p.Y - c.Y) / (p.X - c.X);
                var f = (Math.Atan(k) * RadUnit);

                if (p1.X > 0 && p1.Y > 0)
                {

                }
                else if (p1.X < 0 && p1.Y > 0)
                {
                    f += 180;
                }
                else if (p1.X < 0 && p1.Y < 0)
                {
                    f += 180;
                }
                else if (p1.X > 0 && p1.Y < 0)
                {
                    f += 360;
                }
                if (f < 0)
                    f += 360;
                f = f % 360;
                return (float)f;
            }

            public class OrderPoint
            {
                public Dot dot { get; set; }
                public PointF P { get; set; }
                public float Angle { get; set; }
            }
            public static PointF[] ClockwisePoints(PointF[] points)
            {//按与x的角度计算顺序。
                double x = 0;
                double y = 0;
                foreach (var p in points)
                {
                    x += p.X;
                    y += p.Y;
                }
                x = x / points.Length;
                y = y / points.Length;
                PointF c = new PointF((float)x, (float)y);
                List<OrderPoint> res = new List<OrderPoint>();

                foreach (var p in points)
                {
                    OrderPoint o = new OrderPoint() { Angle = AngleFrom(c, p), P = p };
                    Console.WriteLine(o.Angle);
                    res.Add(o);
                }

                List<PointF> res2 = new List<PointF>();
                foreach (var p in res.OrderBy(t => t.Angle))
                    res2.Add(p.P);
                return res2.ToArray();
            }

            public static PointF[] ClockwisePoints(List<Dot> points)
            {//按与x的角度计算顺序。
                double x = 0;
                double y = 0;
                foreach (var p in points)
                {
                    x += p.X;
                    y += p.Y;
                }
                x = x / points.Count;
                y = y / points.Count;
                PointF c = new PointF((float)x, (float)y);
                List<OrderPoint> res = new List<OrderPoint>();

               // foreach (var p in points)
               foreach(var d in points)
                {
                    OrderPoint o = new OrderPoint() { dot=d,  Angle = AngleFrom(c, d.Pf), P = d.P };
                    Console.WriteLine(o.Angle);
                    res.Add(o);
                }

                List<PointF> res2 = new List<PointF>();
                int idx = 0;
                foreach (var p in res.OrderBy(t => t.Angle))
                {
                    p.dot.Index = idx++;
                    res2.Add(p.P);
                }
                return res2.ToArray();
            }
            //https://blog.csdn.net/hjh2005/article/details/9246967
            public static bool DotInArea(List<Line> lines, Point p)
            {
                KBLine line2 = new KBLine() { K = 0, B = p.Y };
                List<Point> pts = new List<Point>();
                foreach (var l in lines)
                {
                    KBLine line1 = GetKValueFromLine(l);
                    var p1 = Intersect(line2, line1);
                    if (PInLine(p1, l))
                    {
                        pts.Add(p1);
                    }
                }
                if (pts.Count == 0)
                    return false;

                int left = 0, right = 0;
                foreach (var p1 in pts)
                {
                    if (p1.X > p.X)
                        left++;
                    else
                        right++;
                }
                return (left % 2 == 1 && right % 2 == 1);
            }

            public static float Get2LineTextPosAngle(Dot c, Line line1, Line line2)
            {
                var toPoint1 = line1.From != c ? line1.From.Pf : line1.To.Pf;
                //旋转到目标线段结束点
                var toPoint2 = line2.From != c ? line2.From.Pf : line2.To.Pf;

                //把当前点当成中心点后的相对坐标。
                toPoint1 = new PointF(toPoint1.X - c.X, toPoint1.Y - c.Y);
                toPoint2 = new PointF(toPoint2.X - c.X, toPoint2.Y - c.Y);

                var k1 = line1.K;
                var k2 = line2.K;
                var f = (Math.Atan(k1) * RadUnit);//出发点与X正半轴的交角
                var t = (Math.Atan(k2) * RadUnit);//结束点与X正半轴的交角
                if (toPoint1.X >= 0 && toPoint1.Y >= 0)
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
                double angle = (t + f) / 2 % 360;

                var m = Math.Max(f, t);
                if (m - angle < 90)//根据和角的线段夹角是否小于90度来调整夹角度数。
                    angle -= 180;

                float rad = (float)((float)Math.PI * angle / 180);//平分角的弧度数
                return rad;
            }

            public static float GetkByTwoDot(Point f, Point t)
            {
                if (f.X == t.X)
                    return float.MaxValue;
                return (float)(f.Y - t.Y) / (float)(f.X - t.X);
            }
            public static float Get0_360FromByPos(Point p, Point c)
            {
                var t = new PointF(p.X - c.X, p.Y - c.Y);

                var k = GetkByTwoDot(p, c);
                var f = (float)(Math.Atan(k) * RadUnit);

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

            public static double cross(Point p0, Point p1, Point p2)
            {
                return (p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);
            }
            static double eps = 0.000000001;
           // https://blog.csdn.net/acm_cxq/article/details/51285463
            public static bool Compl_inside_convex(Point p, Point[] con)
            {
                if (cross(con[0], p, con[1]) > -eps) return false;
                if (cross(con[0], p, con[con.Length - 1]) < eps) return false;

                int i = 2, j = con.Length - 1;
                int line = -1;

                while (i <= j)
                {
                    int mid = (i + j) >> 1;
                    if (cross(con[0], p, con[mid]) > -eps)
                    {
                        line = mid;
                        j = mid - 1;
                    }
                    else i = mid + 1;
                }
                return cross(con[line - 1], p, con[line]) < -eps;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace m0.UIWpf
{
    public class Line2D
    {
        public double A;
        public double B;
        public double C;
    }

    public class LinearEquation
    {
        public double a;
        public double c;

        public LinearEquation(Line2D line)
        {
            double B = line.B;

            if (B == 0)
                B = 0.0001;

            a = -line.A / B;

            c = -line.C / B;
        }

        public double GetValue(double x)
        {
            return a * x + c;
        }
    }

    public class QuadraticEquation
    {
        public double a;
        public double b;
        public double c;

        public QuadraticEquation(double _a, double _b, double _c)
        {
            a = _a;
            b = _b;
            c = _c;
        }
    }

    public class Circle
    {
        public double x;
        public double y;
        public double r;

        public Circle(double _p,double _q,double _r)
        {
            x = _p;
            y = _q;
            r = _r;
        }
    }

    public class Oval
    {
        public double h;
        public double k;

        public double a;
        public double b;

        public Oval(double _h, double _k, double _a, double _b)
        {
            h = _h;
            k = _k;
            a = _a;
            b = _b;
        }
    }

    public class Geometry2D
    {

        public static Point[] GetOvalLineCross(Line2D line, Oval oval)
        {
            Point[] cross=new Point[2];

            LinearEquation lin = new LinearEquation(line);

            double a = oval.a;
            double b = oval.b;
            double h = oval.h;
            double k = oval.k;

            double m = lin.a;
            double c = lin.c;


            double x1 = (Math.Sqrt(Math.Pow(2 * a * a * h - 2 * b * b * c * m + 2 * b * b * k * m, 2) - 4 * (-a * a - b * b * m * m) * (a * a * b * b - a * a * h * h - b * b * c * c + 2 * b * b * c * k - b * b * k * k)) - 2 * a * a * h + 2 * b * b * c * m - 2 * b * b * k * m) / (2 * (-a * a - b * b * m * m));                

            double x2 = (-Math.Sqrt(Math.Pow(2 * a * a * h - 2 * b * b * c * m + 2 * b * b * k * m, 2) - 4 * (-a * a - b * b * m * m) * (a * a * b * b - a * a * h * h - b * b * c * c + 2 * b * b * c * k - b * b * k * k)) - 2 * a * a * h + 2 * b * b * c * m - 2 * b * b * k * m) / (2 * (-a * a - b * b * m * m));

            cross[0] = new Point(x1, lin.GetValue(x1));

            cross[1] = new Point(x2, lin.GetValue(x2));

            return cross;
        }

        public static Point[] SolveQuadraticEquation(QuadraticEquation e)
        {
            Point[] zeros = new Point[2];

            double delta = Math.Sqrt((e.b * e.b) - 4 * e.a * e.c);

            zeros[0] = new Point((-e.b - delta) / (2 * e.a),0);

            zeros[1] = new Point((-e.b + delta) / (2 * e.a),0);

            return zeros;
        }

        public static Point[] GetCircleLineCross(Line2D line,Circle circle)
        {
            Point[] cross;

            LinearEquation lin = new LinearEquation(line);

            double a = (lin.a * lin.a) + 1;

            double b = 2 * ((lin.a * lin.c) - (lin.a * circle.y) - circle.x);

            double c = (circle.y * circle.y) - (circle.r * circle.r) + (circle.x * circle.x) - (2 * lin.c * circle.y) + (lin.c * lin.c);

            QuadraticEquation eq = new QuadraticEquation(a, b, c);

            cross = SolveQuadraticEquation(eq);

            cross[0].Y = lin.GetValue(cross[0].X);
            cross[1].Y = lin.GetValue(cross[1].X);


            return cross;
        }

        public static Point FindLineCross(Line2D one, Line2D two)
        {
            Point p = new Point();

            double W = (one.A * two.B) - (two.A * one.B);
            double Wx = (-one.C * two.B) - (-two.C * one.B);
            double Wy = (one.A * -two.C) - (two.A * -one.C);

            p.X = Wx / W;
            p.Y = Wy / W;


            return p;
        }

        public static Line2D GetLine2DFromPoints(Point p1, Point p2)
        {
            return GetLine2DFromPoints(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static Line2D GetLine2DFromPoints(double X1, double Y1, double X2, double Y2)
        {
            if ((X1 - X2) == 0)
                X1 += 0.0001;

            if ((X1 - X2) != 0)
            {
                double a1 = (Y1 - Y2);
                double a2 = (X1 - X2);

                Line2D l = new Line2D();

                double a = (a1 / a2);
                double b = Y2 - (a * X2);

                l.A = a;
                l.B = -1;
                l.C = b;

                return l;
            }
            else
            {
                Line2D l = new Line2D();
                
                l.A = 1;
                l.B = 0;
                l.C = 0;

                return l;
            }
        }

        public static double GetPointDistanceFrom2DLine(Line2D line, Point p)
        {
            return Math.Abs(line.A * p.X + line.B * p.Y + line.C) / Math.Sqrt(line.A * line.A + line.B * line.B);
        }
    }
}

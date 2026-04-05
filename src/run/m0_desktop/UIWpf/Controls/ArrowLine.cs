using System;
using System.Windows;
using System.Windows.Media;

namespace m0.UIWpf.Controls
{
    /// <summary>
    ///     Draws a straight line between two points with 
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowLine : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the X1 dependency property.
        /// </summary>
        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1",
                typeof(double), typeof(ArrowLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine start point.
        /// </summary>
        public double X1
        {
            set { SetValue(X1Property, value); }
            get { return (double)GetValue(X1Property); }
        }

        /// <summary>
        ///     Identifies the Y1 dependency property.
        /// </summary>
        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1",
                typeof(double), typeof(ArrowLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine start point.
        /// </summary>
        public double Y1
        {
            set { SetValue(Y1Property, value); }
            get { return (double)GetValue(Y1Property); }
        }

        /// <summary>
        ///     Identifies the X2 dependency property.
        /// </summary>
        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2",
                typeof(double), typeof(ArrowLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine end point.
        /// </summary>
        public double X2
        {
            set { SetValue(X2Property, value); }
            get { return (double)GetValue(X2Property); }
        }

        /// <summary>
        ///     Identifies the Y2 dependency property.
        /// </summary>
        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2",
                typeof(double), typeof(ArrowLine),
                new FrameworkPropertyMetadata(0.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine end point.
        /// </summary>
        public double Y2
        {
            set { SetValue(Y2Property, value); }
            get { return (double)GetValue(Y2Property); }
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                // Clear out the PathGeometry.
                pathgeo.Figures.Clear();

                // Define a single PathFigure with the points.

              
                pathfigLine.StartPoint = new Point(X1, Y1);

                polysegLine.Points.Clear();
                polysegLine.Points.Add(new Point(X2, Y2));
                pathgeo.Figures.Add(pathfigLine);

                // Call the base property to add arrows on the ends.
                //return base.DefiningGeometry;

                Geometry g=base.DefiningGeometry;

                if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                {
                    Point pt1 = pathfigLine.StartPoint;
                    Point pt2 = polysegLine.Points[0];

                    Point A = CalculateArrowPointA(pt2, pt1);
                    Point B = CalculateArrowPointB(pt2, pt1);

                    pathfigLine.StartPoint = Geometry2D.FindLineCross(Geometry2D.GetLine2DFromPoints(X1, Y1, X2, Y2), Geometry2D.GetLine2DFromPoints(A.X,A.Y,B.X,B.Y));

                    //pathfigLine.StartPoint = Geometry2D.FindLineCross(Geometry2D.GetLine2DFromPoints(0, 0, 100, 100), Geometry2D.GetLine2DFromPoints(0, 100, 100, 0));
                }
              

                return pathgeo;
            }
        }
    }
}

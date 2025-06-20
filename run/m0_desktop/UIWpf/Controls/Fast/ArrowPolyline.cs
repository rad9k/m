using System;
using System.Windows;
using System.Windows.Media;

namespace m0.UIWpf.Controls.Fast
{
    /// <summary>
    ///     Draws a series of connected straight lines with
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowPolyline : ArrowLineBase
    {
       

        /// <summary>
        ///     Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                typeof(PointCollection), typeof(ArrowPolyline),
                new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets a collection that contains the 
        ///     vertex points of the ArrowPolyline.
        /// </summary>
        public PointCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (PointCollection)GetValue(PointsProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of the ArrowPolyline class. 
        /// </summary>
        public ArrowPolyline()
        {
            Points = new PointCollection();
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowPolyline.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get 
            {
                // Clear out the PathGeometry.
                pathgeo.Figures.Clear();

                // Try to avoid unnecessary indexing exceptions.
                if (Points.Count > 0)
                {
                    // Define a PathFigure containing the points.
                    pathfigLine.StartPoint = Points[0];
                    polysegLine.Points.Clear();

                    for (int i = 1; i < Points.Count; i++)
                        polysegLine.Points.Add(Points[i]);

                    

                    // Call the base property to add arrows on the ends.
                    Geometry g=base.DefiningGeometry;

                    if (StartEnding == LineEndEnum.Triangle)
                    {
                        Point pt1 = pathfigLine.StartPoint;
                        Point pt2 = polysegLine.Points[0]; 

                        Point A = CalculateArrowPointA(pt2, pt1);
                        Point B = CalculateArrowPointB(pt2, pt1);

                        pathfigLine.StartPoint = Geometry2D.FindLineCross(Geometry2D.GetLine2DFromPoints(pt1.X, pt1.Y, pt2.X, pt2.Y), Geometry2D.GetLine2DFromPoints(A.X, A.Y, B.X, B.Y));
                    }

                    if (EndEnding == LineEndEnum.Triangle)
                    {
                        Point pt1 = polysegLine.Points.Count == 1 ? pathfigLine.StartPoint :
                                                     polysegLine.Points[polysegLine.Points.Count - 2];
                        Point pt2 = polysegLine.Points[polysegLine.Points.Count - 1];

                        Point A = CalculateArrowPointA(pt1, pt2);
                        Point B = CalculateArrowPointB(pt1, pt2);

                        polysegLine.Points[Points.Count - 2] = Geometry2D.FindLineCross(Geometry2D.GetLine2DFromPoints(pt1.X, pt1.Y, pt2.X, pt2.Y), Geometry2D.GetLine2DFromPoints(A.X, A.Y, B.X, B.Y));
                    }

                    if (StartEnding == LineEndEnum.Diamond)
                    {
                        Point pt1 = pathfigLine.StartPoint;
                        Point pt2 = polysegLine.Points[0]; 

                        pathfigLine.StartPoint =  CalculateDiamondPoint(pt2, pt1);
                    }

                    if (EndEnding == LineEndEnum.Diamond)
                    {
                        Point pt1 = polysegLine.Points.Count == 1 ? pathfigLine.StartPoint :
                                                    polysegLine.Points[polysegLine.Points.Count - 2];
                        Point pt2 = polysegLine.Points[polysegLine.Points.Count - 1];

                        polysegLine.Points[Points.Count - 2] = CalculateDiamondPoint(pt1, pt2);
                    }

                    

                    pathgeo.Figures.Add(pathfigLine);
                }

                
                return pathgeo;
            }
        }
    }
}

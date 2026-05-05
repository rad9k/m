using m0.ZeroTypes.UX;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0.UIWpf.Controls
{
    /// <summary>
    ///     Provides a base class for ArrowLine and ArrowPolyline.
    ///     This class is abstract.
    /// </summary>
    public abstract class ArrowLineBase : Shape
    {

        public bool IsDashed = false;
        public bool IsEndings;

        protected PathGeometry pathgeo;
        protected PathFigure pathfigLine;
        protected PolyLineSegment polysegLine;

        PathFigure pathfigHead1;
        PolyLineSegment polysegHead1;
        PathFigure pathfigHead2;
        PolyLineSegment polysegHead2;

        // Extra pre-allocated figures for CrowFoot's two side prongs at each end.
        // The middle prong reuses pathfigHead1 / pathfigHead2.
        PathFigure pathfigHead1Side1;
        PolyLineSegment polysegHead1Side1;
        PathFigure pathfigHead1Side2;
        PolyLineSegment polysegHead1Side2;
        PathFigure pathfigHead2Side1;
        PolyLineSegment polysegHead2Side1;
        PathFigure pathfigHead2Side2;
        PolyLineSegment polysegHead2Side2;

        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(45.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(12.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowEnds",
                typeof(ArrowEnds), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(ArrowEnds.End,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }
        }

        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty =
            DependencyProperty.Register("IsArrowClosed",
                typeof(bool), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(false,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of ArrowLineBase.
        /// </summary>
        public ArrowLineBase()
        {
            pathgeo = new PathGeometry();

            pathfigLine = new PathFigure();
            polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);

            pathfigHead1 = new PathFigure();
            polysegHead1 = new PolyLineSegment();
            pathfigHead1.Segments.Add(polysegHead1);

            pathfigHead2 = new PathFigure();
            polysegHead2 = new PolyLineSegment();
            pathfigHead2.Segments.Add(polysegHead2);

            pathfigHead1Side1 = new PathFigure();
            polysegHead1Side1 = new PolyLineSegment();
            pathfigHead1Side1.Segments.Add(polysegHead1Side1);

            pathfigHead1Side2 = new PathFigure();
            polysegHead1Side2 = new PolyLineSegment();
            pathfigHead1Side2.Segments.Add(polysegHead1Side2);

            pathfigHead2Side1 = new PathFigure();
            polysegHead2Side1 = new PolyLineSegment();
            pathfigHead2Side1.Segments.Add(polysegHead2Side1);

            pathfigHead2Side2 = new PathFigure();
            polysegHead2Side2 = new PolyLineSegment();
            pathfigHead2Side2.Segments.Add(polysegHead2Side2);
        }

        public LineEndEnum StartEnding;

        public LineEndEnum EndEnding;

        // Optional CrowFoot side prong tip overrides. When set, the side
        // prongs are drawn from the convergence point to the supplied tip
        // (computed by the caller using a shape-aware ray/edge intersection).
        // When null, ArrowLineBase falls back to the angular formula
        // (convergence + rotated unit vector * ArrowLength), which lands the
        // tip in the air and not on any entity edge.
        public Point? StartCrowFootSide1Tip;
        public Point? StartCrowFootSide2Tip;
        public Point? EndCrowFootSide1Tip;
        public Point? EndCrowFootSide2Tip;

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                int count = polysegLine.Points.Count;

                if (count > 0 && IsEndings)
                {
                    // Draw the arrow at the start of the line.
                    if (StartEnding == LineEndEnum.Arrow || StartEnding == LineEndEnum.Triangle || StartEnding == LineEndEnum.FilledTriangle)
                    {
                        Point pt1 = pathfigLine.StartPoint;
                        Point pt2 = polysegLine.Points[0];

                        bool isClosed = false;

                        if (StartEnding == LineEndEnum.FilledTriangle || StartEnding == LineEndEnum.Triangle)
                            isClosed = true;

                        pathgeo.Figures.Add(CalculateArrow(pathfigHead1, pt2, pt1,isClosed));
                    }

                    if (StartEnding == LineEndEnum.Diamond || StartEnding == LineEndEnum.FilledDiamond)
                    {
                        Point pt1 = pathfigLine.StartPoint;
                        Point pt2 = polysegLine.Points[0];                        

                        pathgeo.Figures.Add(CalculateDiamond(pathfigHead1, pt2, pt1));
                    }

                    if (StartEnding == LineEndEnum.CrowFoot)
                    {
                        Point pt1 = pathfigLine.StartPoint;
                        Point pt2 = polysegLine.Points[0];

                        pathgeo.Figures.Add(CalculateCrowFootMiddle(pathfigHead1, pt2, pt1));
                        pathgeo.Figures.Add(CalculateCrowFootSide(pathfigHead1Side1, pt2, pt1, +1, StartCrowFootSide1Tip));
                        pathgeo.Figures.Add(CalculateCrowFootSide(pathfigHead1Side2, pt2, pt1, -1, StartCrowFootSide2Tip));
                    }

                    // Draw the arrow at the end of the line.
                    if (EndEnding == LineEndEnum.Arrow || EndEnding == LineEndEnum.Triangle || EndEnding == LineEndEnum.FilledTriangle)
                    {
                        Point pt1 = count == 1 ? pathfigLine.StartPoint :
                                                 polysegLine.Points[count - 2];
                        Point pt2 = polysegLine.Points[count - 1];

                        bool isClosed = false;

                        if (StartEnding == LineEndEnum.FilledTriangle || EndEnding == LineEndEnum.Triangle)
                            isClosed = true;

                        pathgeo.Figures.Add(CalculateArrow(pathfigHead2, pt1, pt2,isClosed));
                    }

                    if (EndEnding == LineEndEnum.Diamond || EndEnding == LineEndEnum.FilledDiamond)
                    {
                        Point pt1 = count == 1 ? pathfigLine.StartPoint :
                                                 polysegLine.Points[count - 2];
                        Point pt2 = polysegLine.Points[count - 1];

                        pathgeo.Figures.Add(CalculateDiamond(pathfigHead2, pt1, pt2));
                    }

                    if (EndEnding == LineEndEnum.CrowFoot)
                    {
                        Point pt1 = count == 1 ? pathfigLine.StartPoint :
                                                 polysegLine.Points[count - 2];
                        Point pt2 = polysegLine.Points[count - 1];

                        pathgeo.Figures.Add(CalculateCrowFootMiddle(pathfigHead2, pt1, pt2));
                        pathgeo.Figures.Add(CalculateCrowFootSide(pathfigHead2Side1, pt1, pt2, +1, EndCrowFootSide1Tip));
                        pathgeo.Figures.Add(CalculateCrowFootSide(pathfigHead2Side2, pt1, pt2, -1, EndCrowFootSide2Tip));
                    }
                }
                return pathgeo;
            }
        }

        PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2, bool isClosed)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();
            matx.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = pt2 + vect * matx;
            polyseg.Points.Add(pt2);

            matx.Rotate(-ArrowAngle);
            polyseg.Points.Add(pt2 + vect * matx);
                        
            pathfig.IsClosed = isClosed;

            return pathfig;
        }

        int diamondLength = 10;

        PathFigure CalculateDiamond(PathFigure pathfig, Point pt1, Point pt2)
        {
            

            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= diamondLength;

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();
            matx.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = pt2 + vect * matx;
            polyseg.Points.Add(pt2);

            matx.Rotate(-ArrowAngle);
            polyseg.Points.Add(pt2 + vect * matx);

            matx.Rotate(ArrowAngle/2);

            polyseg.Points.Add(pt2 + (double)1.75*vect * matx);

            pathfig.IsClosed = true;

            return pathfig;
        }

        // CrowFoot ("many" symbol in ERD): three open strokes converging at a
        // point pulled back from pt2 by ArrowLength along the line, and fanning
        // out so they reach (or border on) the entity edge at pt2. The middle
        // prong hits exactly pt2; the two side prongs are rotated by
        // +/- ArrowAngle/2 from the line direction.
        // The line itself must be shortened by ArrowLength at the matching end
        // so it stops at the convergence point - this shortening lives in
        // ArrowPolyline.DefiningGeometry, in tandem with this method.
        // Pre-allocated PathFigure / PolyLineSegment instances are passed in to
        // avoid per-frame allocations.
        PathFigure CalculateCrowFootMiddle(PathFigure pathfig, Point pt1, Point pt2)
        {
            Vector vect = pt2 - pt1;
            vect.Normalize();
            vect *= ArrowLength;

            Point convergence = pt2 - vect;

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();

            pathfig.StartPoint = convergence;
            polyseg.Points.Add(pt2);

            pathfig.IsClosed = false;

            return pathfig;
        }

        // angleSign: +1 for one side prong, -1 for the other.
        // overrideTip: if non-null, drawn from convergence to overrideTip
        // (caller computed it via shape-aware ray/edge intersection so the
        // tip lands exactly on the entity boundary). If null, falls back to
        // the angular formula which leaves the tip in the air.
        PathFigure CalculateCrowFootSide(PathFigure pathfig, Point pt1, Point pt2, double angleSign, Point? overrideTip)
        {
            Vector vect = pt2 - pt1;
            vect.Normalize();
            vect *= ArrowLength;

            Point convergence = pt2 - vect;

            Point tip;

            if (overrideTip.HasValue)
            {
                tip = overrideTip.Value;
            }
            else
            {
                Matrix matx = new Matrix();
                matx.Rotate(angleSign * ArrowAngle / 2);
                tip = convergence + vect * matx;
            }

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();

            pathfig.StartPoint = convergence;
            polyseg.Points.Add(tip);

            pathfig.IsClosed = false;

            return pathfig;
        }

        protected Point CalculateArrowPointA(Point pt1, Point pt2)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            matx.Rotate(ArrowAngle / 2);
          
            return pt2 + vect * matx;        
        }

        protected Point CalculateArrowPointB(Point pt1, Point pt2)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            matx.Rotate(ArrowAngle / 2);
           
            matx.Rotate(-ArrowAngle);
           
            return pt2 + vect * matx;
        }

        protected Point CalculateDiamondPoint(Point pt1, Point pt2)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= diamondLength;
            
            matx.Rotate(ArrowAngle / 2);
            
            matx.Rotate(-ArrowAngle);
            
            matx.Rotate(ArrowAngle / 2);
            
            return pt2 + (double)1.75 * vect * matx;
        }

    }
}

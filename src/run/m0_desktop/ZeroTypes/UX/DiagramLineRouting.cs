using m0.UIWpf.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.ZeroTypes.UX
{
    public enum DiagramLineRouteStatus
    {
        Direct,
        Routed,
        ReducedClearance,
        Fallback
    }

    public enum DiagramLineRouteSegmentKind
    {
        Line,
        QuadraticBezier
    }

    enum EndpointShapeKind
    {
        Rectangle,
        Rhombus,
        Ellipse
    }

    struct EndpointCollisionGeometry
    {
        public EndpointShapeKind Shape;
        public Rect Bounds;
        public bool HasBounds;
    }

    public sealed class DiagramLineRouteSegment
    {
        public DiagramLineRouteSegmentKind Kind { get; private set; }
        public Point Start { get; private set; }
        public Point Control { get; private set; }
        public Point End { get; private set; }

        private DiagramLineRouteSegment(
            DiagramLineRouteSegmentKind kind,
            Point start,
            Point control,
            Point end)
        {
            Kind = kind;
            Start = start;
            Control = control;
            End = end;
        }

        public static DiagramLineRouteSegment CreateLine(Point start, Point end)
        {
            return new DiagramLineRouteSegment(
                DiagramLineRouteSegmentKind.Line,
                start,
                new Point(),
                end);
        }

        public static DiagramLineRouteSegment CreateQuadraticBezier(
            Point start,
            Point control,
            Point end)
        {
            return new DiagramLineRouteSegment(
                DiagramLineRouteSegmentKind.QuadraticBezier,
                start,
                control,
                end);
        }
    }

    public sealed class DiagramLineRoute
    {
        readonly List<Point> skeletonPoints;
        readonly List<DiagramLineRouteSegment> segments;
        readonly List<Point> flattenedPoints;
        readonly List<double> flattenedCumulativeLengths;

        public IReadOnlyList<Point> SkeletonPoints
        {
            get { return skeletonPoints; }
        }

        public IReadOnlyList<DiagramLineRouteSegment> Segments
        {
            get { return segments; }
        }

        public IReadOnlyList<Point> FlattenedPoints
        {
            get { return flattenedPoints; }
        }

        public string SourcePortId { get; private set; }
        public string TargetPortId { get; private set; }
        public DiagramLineRouteStatus Status { get; private set; }
        public double Cost { get; private set; }
        public double TotalLength { get; private set; }
        public Rect Bounds { get; private set; }

        public Point StartPoint
        {
            get
            {
                return flattenedPoints.Count == 0
                    ? new Point()
                    : flattenedPoints[0];
            }
        }

        public Point EndPoint
        {
            get
            {
                return flattenedPoints.Count == 0
                    ? new Point()
                    : flattenedPoints[flattenedPoints.Count - 1];
            }
        }

        internal DiagramLineRoute(
            IEnumerable<Point> routeSkeletonPoints,
            IEnumerable<DiagramLineRouteSegment> routeSegments,
            string sourcePortId,
            string targetPortId,
            DiagramLineRouteStatus status,
            double cost)
        {
            skeletonPoints = new List<Point>(routeSkeletonPoints);
            segments = new List<DiagramLineRouteSegment>(routeSegments);
            SourcePortId = sourcePortId;
            TargetPortId = targetPortId;
            Status = status;
            Cost = cost;

            flattenedPoints = FlattenSegments(segments);
            flattenedCumulativeLengths = new List<double>(
                flattenedPoints.Count);

            double totalLength = 0;
            Rect bounds = Rect.Empty;

            for (int pointIndex = 0;
                pointIndex < flattenedPoints.Count;
                pointIndex++)
            {
                Point point = flattenedPoints[pointIndex];

                if (pointIndex > 0)
                    totalLength += Distance(
                        flattenedPoints[pointIndex - 1],
                        point);

                flattenedCumulativeLengths.Add(totalLength);

                if (bounds.IsEmpty)
                    bounds = new Rect(point, point);
                else
                    bounds.Union(point);
            }

            TotalLength = totalLength;
            Bounds = bounds;
        }

        public Vector GetStartTangent()
        {
            for (int pointIndex = 1;
                pointIndex < flattenedPoints.Count;
                pointIndex++)
            {
                Vector tangent =
                    flattenedPoints[pointIndex] -
                    flattenedPoints[pointIndex - 1];

                if (tangent.Length > DiagramLineRouter.GeometryEpsilon)
                {
                    tangent.Normalize();
                    return tangent;
                }
            }

            return new Vector(1, 0);
        }

        public Vector GetEndTangent()
        {
            for (int pointIndex = flattenedPoints.Count - 1;
                pointIndex > 0;
                pointIndex--)
            {
                Vector tangent =
                    flattenedPoints[pointIndex] -
                    flattenedPoints[pointIndex - 1];

                if (tangent.Length > DiagramLineRouter.GeometryEpsilon)
                {
                    tangent.Normalize();
                    return tangent;
                }
            }

            return new Vector(1, 0);
        }

        public void GetPointAndTangentAtFraction(
            double fraction,
            out Point point,
            out Vector tangent)
        {
            if (flattenedPoints.Count == 0)
            {
                point = new Point();
                tangent = new Vector(1, 0);
                return;
            }

            if (flattenedPoints.Count == 1 || TotalLength <= 0)
            {
                point = flattenedPoints[0];
                tangent = new Vector(1, 0);
                return;
            }

            fraction = Math.Max(0, Math.Min(1, fraction));
            double targetLength = TotalLength * fraction;

            for (int pointIndex = 1;
                pointIndex < flattenedPoints.Count;
                pointIndex++)
            {
                double segmentEndLength =
                    flattenedCumulativeLengths[pointIndex];

                if (segmentEndLength < targetLength &&
                    pointIndex < flattenedPoints.Count - 1)
                {
                    continue;
                }

                Point segmentStart = flattenedPoints[pointIndex - 1];
                Point segmentEnd = flattenedPoints[pointIndex];
                double segmentStartLength =
                    flattenedCumulativeLengths[pointIndex - 1];
                double segmentLength =
                    segmentEndLength - segmentStartLength;

                double localFraction = segmentLength <= 0
                    ? 0
                    : (targetLength - segmentStartLength) /
                        segmentLength;

                localFraction = Math.Max(
                    0,
                    Math.Min(1, localFraction));

                point = new Point(
                    segmentStart.X +
                        (segmentEnd.X - segmentStart.X) *
                        localFraction,
                    segmentStart.Y +
                        (segmentEnd.Y - segmentStart.Y) *
                        localFraction);

                tangent = segmentEnd - segmentStart;
                if (tangent.Length >
                    DiagramLineRouter.GeometryEpsilon)
                {
                    tangent.Normalize();
                }
                else
                    tangent = new Vector(1, 0);

                return;
            }

            point = flattenedPoints[flattenedPoints.Count - 1];
            tangent = GetEndTangent();
        }

        public double GetDistanceFromPoint(Point point)
        {
            double minimumDistance = double.MaxValue;

            for (int pointIndex = 0;
                pointIndex < flattenedPoints.Count - 1;
                pointIndex++)
            {
                double distance = GetPointToSegmentDistance(
                    point,
                    flattenedPoints[pointIndex],
                    flattenedPoints[pointIndex + 1]);

                if (distance < minimumDistance)
                    minimumDistance = distance;
            }

            return minimumDistance;
        }

        public PathGeometry CreateBodyGeometry(
            double startInset,
            double endInset)
        {
            PathGeometry geometry = new PathGeometry();

            if (segments.Count == 0)
                return geometry;

            Point adjustedStart = segments[0].Start;
            Point adjustedEnd = segments[segments.Count - 1].End;

            if (segments.Count == 1 &&
                segments[0].Kind ==
                    DiagramLineRouteSegmentKind.Line)
            {
                double lineLength = Distance(
                    segments[0].Start,
                    segments[0].End);
                double requestedInset =
                    Math.Max(0, startInset) +
                    Math.Max(0, endInset);
                double availableInset =
                    Math.Max(0, lineLength - 0.5);

                if (requestedInset > availableInset &&
                    requestedInset > 0)
                {
                    double insetScale =
                        availableInset / requestedInset;
                    startInset *= insetScale;
                    endInset *= insetScale;
                }
            }

            DiagramLineRouteSegment firstSegment = segments[0];
            if (firstSegment.Kind ==
                DiagramLineRouteSegmentKind.Line)
            {
                Vector direction =
                    firstSegment.End - firstSegment.Start;
                double length = direction.Length;

                if (length > DiagramLineRouter.GeometryEpsilon)
                {
                    direction.Normalize();
                    adjustedStart += direction * Math.Min(
                        Math.Max(0, startInset),
                        Math.Max(0, length - 0.5));
                }
            }

            DiagramLineRouteSegment lastSegment =
                segments[segments.Count - 1];
            if (lastSegment.Kind ==
                DiagramLineRouteSegmentKind.Line)
            {
                Vector direction =
                    lastSegment.End - lastSegment.Start;
                double length = direction.Length;

                if (length > DiagramLineRouter.GeometryEpsilon)
                {
                    direction.Normalize();
                    adjustedEnd -= direction * Math.Min(
                        Math.Max(0, endInset),
                        Math.Max(0, length - 0.5));
                }
            }

            PathFigure figure = new PathFigure
            {
                StartPoint = adjustedStart,
                IsClosed = false,
                IsFilled = false
            };

            for (int segmentIndex = 0;
                segmentIndex < segments.Count;
                segmentIndex++)
            {
                DiagramLineRouteSegment segment =
                    segments[segmentIndex];
                Point segmentEnd =
                    segmentIndex == segments.Count - 1
                        ? adjustedEnd
                        : segment.End;

                if (segment.Kind ==
                    DiagramLineRouteSegmentKind.Line)
                {
                    figure.Segments.Add(
                        new LineSegment(segmentEnd, true));
                }
                else
                {
                    figure.Segments.Add(
                        new QuadraticBezierSegment(
                            segment.Control,
                            segmentEnd,
                            true));
                }
            }

            geometry.Figures.Add(figure);

            if (geometry.CanFreeze)
                geometry.Freeze();

            return geometry;
        }

        static List<Point> FlattenSegments(
            IList<DiagramLineRouteSegment> routeSegments)
        {
            List<Point> points = new List<Point>();

            if (routeSegments.Count == 0)
                return points;

            points.Add(routeSegments[0].Start);

            foreach (DiagramLineRouteSegment segment in routeSegments)
            {
                if (segment.Kind ==
                    DiagramLineRouteSegmentKind.Line)
                {
                    AddIfDifferent(points, segment.End);
                    continue;
                }

                double estimatedLength =
                    Distance(segment.Start, segment.Control) +
                    Distance(segment.Control, segment.End);
                int stepCount = Math.Max(
                    6,
                    Math.Min(
                        32,
                        (int)Math.Ceiling(
                            estimatedLength / 3.0)));

                for (int step = 1; step <= stepCount; step++)
                {
                    double t = (double)step / stepCount;
                    double oneMinusT = 1 - t;
                    Point point = new Point(
                        oneMinusT * oneMinusT *
                            segment.Start.X +
                        2 * oneMinusT * t *
                            segment.Control.X +
                        t * t * segment.End.X,
                        oneMinusT * oneMinusT *
                            segment.Start.Y +
                        2 * oneMinusT * t *
                            segment.Control.Y +
                        t * t * segment.End.Y);

                    AddIfDifferent(points, point);
                }
            }

            return points;
        }

        static void AddIfDifferent(
            IList<Point> points,
            Point point)
        {
            if (points.Count == 0 ||
                Distance(points[points.Count - 1], point) >
                    DiagramLineRouter.GeometryEpsilon)
            {
                points.Add(point);
            }
        }

        static double GetPointToSegmentDistance(
            Point point,
            Point segmentStart,
            Point segmentEnd)
        {
            Vector segment = segmentEnd - segmentStart;
            double squaredLength =
                segment.X * segment.X +
                segment.Y * segment.Y;

            if (squaredLength <=
                DiagramLineRouter.GeometryEpsilon *
                DiagramLineRouter.GeometryEpsilon)
            {
                return Distance(point, segmentStart);
            }

            Vector fromStart = point - segmentStart;
            double projection =
                (fromStart.X * segment.X +
                 fromStart.Y * segment.Y) /
                squaredLength;
            projection = Math.Max(
                0,
                Math.Min(1, projection));

            Point closest = new Point(
                segmentStart.X + segment.X * projection,
                segmentStart.Y + segment.Y * projection);

            return Distance(point, closest);
        }

        internal static double Distance(Point first, Point second)
        {
            double deltaX = second.X - first.X;
            double deltaY = second.Y - first.Y;
            return Math.Sqrt(
                deltaX * deltaX +
                deltaY * deltaY);
        }
    }

    internal static class DiagramLineRouter
    {
        internal const double GeometryEpsilon = 0.001;

        const double MinimumClearance = 20;
        const double PreferredCornerRadius = 10;
        const int MaximumPortPairs = 25;
        const int MaximumVisibilitySearchPortPairs = 2;
        const int MaximumSearchExpansions = 120;

        sealed class RoutingObstacle
        {
            public IUXItem Item;
            public Rect RawBounds;
            public Rect InflatedBounds;
            public string StableKey;
        }

        sealed class PortCandidate
        {
            public Point Anchor;
            public Point Escape;
            public string Id;
            public double Penalty;
        }

        sealed class PortPair
        {
            public PortCandidate Source;
            public PortCandidate Target;
            public double Rank;
        }

        sealed class RouteCandidate
        {
            public List<Point> Points;
            public PortCandidate SourcePort;
            public PortCandidate TargetPort;
            public double Cost;
            public double Length;
            public double BendCost;
            public int IntersectedItemCount;
            public double RawPenetrationLength;
            public double InflatedPenetrationLength;
        }

        struct SearchState : IEquatable<SearchState>
        {
            public int Previous;
            public int Current;

            public SearchState(int previous, int current)
            {
                Previous = previous;
                Current = current;
            }

            public bool Equals(SearchState other)
            {
                return Previous == other.Previous &&
                    Current == other.Current;
            }

            public override bool Equals(object obj)
            {
                return obj is SearchState &&
                    Equals((SearchState)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Previous * 397 ^ Current;
                }
            }
        }

        struct SearchQueueEntry
        {
            public SearchState State;
            public double Cost;

            public SearchQueueEntry(
                SearchState state,
                double cost)
            {
                State = state;
                Cost = cost;
            }
        }

        public static DiagramLineRoute CreateRoute(
            IUXItem sourceItem,
            IUXItem targetItem,
            IUXVisualiser owningVisualiser,
            Point requestedStart,
            Point requestedEnd,
            bool isSelfRelation,
            double selfRelationX,
            double selfRelationY,
            double lineWidth,
            double startMarkerExtent,
            double endMarkerExtent,
            DiagramLineRoute previousRoute)
        {
            if (sourceItem == null ||
                targetItem == null ||
                owningVisualiser == null ||
                owningVisualiser.Canvas == null)
            {
                return CreateLegacyRoute(
                    requestedStart,
                    requestedEnd,
                    isSelfRelation,
                    selfRelationX,
                    selfRelationY);
            }

            if (!IsFinite(lineWidth) ||
                lineWidth < 0)
            {
                lineWidth = 1;
            }

            Rect sourceBounds;
            Rect targetBounds;

            bool isLiveDrag = IsLiveDrag(owningVisualiser);

            if (isLiveDrag)
            {
                return CreateLegacyRoute(
                    requestedStart,
                    requestedEnd,
                    isSelfRelation,
                    selfRelationX,
                    selfRelationY);
            }

            if (!TryGetVisibleBounds(
                    sourceItem,
                    owningVisualiser.Canvas,
                    out sourceBounds) ||
                !TryGetVisibleBounds(
                    targetItem,
                    owningVisualiser.Canvas,
                    out targetBounds))
            {
                return CreateLegacyRoute(
                    requestedStart,
                    requestedEnd,
                    isSelfRelation,
                    selfRelationX,
                    selfRelationY);
            }

            EndpointCollisionGeometry sourceGeometry =
                CreateEndpointCollisionGeometry(
                    sourceItem,
                    sourceBounds);
            EndpointCollisionGeometry targetGeometry =
                CreateEndpointCollisionGeometry(
                    targetItem,
                    targetBounds);

            double clearance = Math.Max(
                MinimumClearance,
                lineWidth / 2 + 8);

            List<RoutingObstacle> obstacles =
                BuildObstacles(
                    sourceItem,
                    targetItem,
                    owningVisualiser,
                    clearance,
                    lineWidth);

            if (isSelfRelation ||
                object.ReferenceEquals(sourceItem, targetItem))
            {
                return CreateSelfRoute(
                    sourceItem,
                    sourceBounds,
                    owningVisualiser,
                    requestedStart,
                    requestedEnd,
                    selfRelationX,
                    selfRelationY,
                    clearance,
                    obstacles,
                    startMarkerExtent,
                    endMarkerExtent,
                    previousRoute);
            }

            Point sourceCenter = GetCenter(sourceBounds);
            Point targetCenter = GetCenter(targetBounds);

            if (!IsFinite(requestedStart))
            {
                requestedStart = GetAutomaticAnchor(
                    sourceItem,
                    sourceBounds,
                    targetCenter,
                    clearance);
            }

            if (!IsFinite(requestedEnd))
            {
                requestedEnd = GetAutomaticAnchor(
                    targetItem,
                    targetBounds,
                    sourceCenter,
                    clearance);
            }

            List<PortCandidate> sourcePorts = BuildPorts(
                sourceItem,
                sourceBounds,
                targetCenter,
                requestedStart,
                clearance,
                "S",
                UsesRectangularLinePorts(sourceItem));
            List<PortCandidate> targetPorts = BuildPorts(
                targetItem,
                targetBounds,
                sourceCenter,
                requestedEnd,
                clearance,
                "T",
                UsesRectangularLinePorts(targetItem));

            List<PortPair> portPairs =
                BuildRankedPortPairs(
                    sourcePorts,
                    targetPorts,
                    previousRoute);

            List<RouteCandidate> candidates =
                new List<RouteCandidate>();
            HashSet<string> candidateSignatures =
                new HashSet<string>();

            AddPreviousRouteCandidate(
                candidates,
                candidateSignatures,
                previousRoute,
                portPairs);

            bool hasInflatedClearCheapRoute = false;
            bool hasRawClearCheapRoute = false;

            for (int portPairIndex = 0;
                portPairIndex < portPairs.Count;
                portPairIndex++)
            {
                PortPair portPair =
                    portPairs[portPairIndex];

                if (TerminalDirectionsAreValid(portPair))
                {
                    List<Point> directPoints =
                        new List<Point>
                        {
                            portPair.Source.Anchor,
                            portPair.Target.Anchor
                        };
                    AddCandidate(
                        candidates,
                        candidateSignatures,
                        directPoints,
                        portPair.Source,
                        portPair.Target);
                    RecordCheapRouteClearance(
                        directPoints,
                        obstacles,
                        sourceGeometry,
                        targetGeometry,
                        ref hasInflatedClearCheapRoute,
                        ref hasRawClearCheapRoute);
                }

                List<Point> escapePoints =
                    ComposeFullRoute(
                        portPair.Source,
                        new List<Point>
                        {
                            portPair.Source.Escape,
                            portPair.Target.Escape
                        },
                        portPair.Target);
                AddCandidate(
                    candidates,
                    candidateSignatures,
                    escapePoints,
                    portPair.Source,
                    portPair.Target);
                RecordCheapRouteClearance(
                    escapePoints,
                    obstacles,
                    sourceGeometry,
                    targetGeometry,
                    ref hasInflatedClearCheapRoute,
                    ref hasRawClearCheapRoute);

                Point horizontalThenVertical = new Point(
                    portPair.Target.Escape.X,
                    portPair.Source.Escape.Y);
                List<Point> horizontalThenVerticalPoints =
                    ComposeFullRoute(
                        portPair.Source,
                        new List<Point>
                        {
                            portPair.Source.Escape,
                            horizontalThenVertical,
                            portPair.Target.Escape
                        },
                        portPair.Target);
                AddCandidate(
                    candidates,
                    candidateSignatures,
                    horizontalThenVerticalPoints,
                    portPair.Source,
                    portPair.Target);
                RecordCheapRouteClearance(
                    horizontalThenVerticalPoints,
                    obstacles,
                    sourceGeometry,
                    targetGeometry,
                    ref hasInflatedClearCheapRoute,
                    ref hasRawClearCheapRoute);

                Point verticalThenHorizontal = new Point(
                    portPair.Source.Escape.X,
                    portPair.Target.Escape.Y);
                List<Point> verticalThenHorizontalPoints =
                    ComposeFullRoute(
                        portPair.Source,
                        new List<Point>
                        {
                            portPair.Source.Escape,
                            verticalThenHorizontal,
                            portPair.Target.Escape
                        },
                        portPair.Target);
                AddCandidate(
                    candidates,
                    candidateSignatures,
                    verticalThenHorizontalPoints,
                    portPair.Source,
                    portPair.Target);
                RecordCheapRouteClearance(
                    verticalThenHorizontalPoints,
                    obstacles,
                    sourceGeometry,
                    targetGeometry,
                    ref hasInflatedClearCheapRoute,
                    ref hasRawClearCheapRoute);
            }

            bool visibilitySearchIsBlocked =
                PointIsInsideEndpointInterior(
                    targetGeometry,
                    sourceCenter) ||
                PointIsInsideEndpointInterior(
                    sourceGeometry,
                    targetCenter);

            // A* only when cheap L/direct still hits a raw item interior.
            // ReducedClearance cheap routes are accepted instead of a
            // doomed visibility search through overlapping items.
            if (!hasRawClearCheapRoute &&
                !visibilitySearchIsBlocked)
            {
                int visibilityPairCount = Math.Min(
                    MaximumVisibilitySearchPortPairs,
                    portPairs.Count);

                for (int portPairIndex = 0;
                    portPairIndex < visibilityPairCount;
                    portPairIndex++)
                {
                    PortPair portPair =
                        portPairs[portPairIndex];

                    if (VisibilitySearchEscapesAreBlocked(
                            portPair.Source.Escape,
                            portPair.Target.Escape,
                            sourceGeometry,
                            targetGeometry) ||
                        IsSegmentClear(
                            portPair.Source.Escape,
                            portPair.Target.Escape,
                            obstacles,
                            true,
                            sourceGeometry,
                            targetGeometry))
                    {
                        continue;
                    }

                    List<Point> visibilityPath =
                        FindVisibilityPath(
                            portPair.Source.Escape,
                            portPair.Target.Escape,
                            obstacles,
                            true,
                            sourceGeometry,
                            targetGeometry);

                    if (visibilityPath == null)
                    {
                        visibilityPath =
                            FindVisibilityPath(
                                portPair.Source.Escape,
                                portPair.Target.Escape,
                                obstacles,
                                false,
                                sourceGeometry,
                                targetGeometry);
                    }

                    if (visibilityPath != null)
                    {
                        AddCandidate(
                            candidates,
                            candidateSignatures,
                            ComposeFullRoute(
                                portPair.Source,
                                visibilityPath,
                                portPair.Target),
                            portPair.Source,
                            portPair.Target);
                    }
                }
            }

            return SelectAndCreateRoute(
                candidates,
                obstacles,
                requestedStart,
                requestedEnd,
                clearance,
                previousRoute,
                isLiveDrag,
                0,
                startMarkerExtent,
                endMarkerExtent,
                sourceItem,
                targetItem,
                sourceGeometry,
                targetGeometry);
        }

        static DiagramLineRoute CreateSelfRoute(
            IUXItem item,
            Rect itemBounds,
            IUXVisualiser owningVisualiser,
            Point requestedStart,
            Point requestedEnd,
            double selfRelationX,
            double selfRelationY,
            double clearance,
            IList<RoutingObstacle> obstacles,
            double startMarkerExtent,
            double endMarkerExtent,
            DiagramLineRoute previousRoute)
        {
            double loopDistance = clearance;

            if (IsFinite(selfRelationX) &&
                selfRelationX > itemBounds.Right)
            {
                loopDistance = Math.Max(
                    loopDistance,
                    selfRelationX - itemBounds.Right);
            }

            if (IsFinite(selfRelationY) &&
                selfRelationY < itemBounds.Top)
            {
                loopDistance = Math.Max(
                    loopDistance,
                    itemBounds.Top - selfRelationY);
            }

            List<PortCandidate> ports = BuildPorts(
                item,
                itemBounds,
                new Point(
                    itemBounds.Right + loopDistance,
                    itemBounds.Top - loopDistance),
                new Point(double.NaN, double.NaN),
                clearance,
                "L",
                true);

            Dictionary<string, PortCandidate> portsBySide =
                ports
                    .Where(port => port.Id.Length > 0)
                    .GroupBy(port => port.Id.Substring(
                        port.Id.Length - 1))
                    .ToDictionary(
                        group => group.Key,
                        group => group.First());

            List<RouteCandidate> candidates =
                new List<RouteCandidate>();
            HashSet<string> signatures =
                new HashSet<string>();

            double[] loopDistances =
            {
                loopDistance,
                loopDistance + 2 * clearance,
                loopDistance + 4 * clearance
            };

            foreach (double candidateLoopDistance in
                loopDistances)
            {
                AddSelfLoopCandidate(
                    candidates,
                    signatures,
                    portsBySide,
                    "T",
                    "R",
                    new Point(
                        itemBounds.Right +
                            candidateLoopDistance,
                        itemBounds.Top -
                            candidateLoopDistance));
                AddSelfLoopCandidate(
                    candidates,
                    signatures,
                    portsBySide,
                    "R",
                    "B",
                    new Point(
                        itemBounds.Right +
                            candidateLoopDistance,
                        itemBounds.Bottom +
                            candidateLoopDistance));
                AddSelfLoopCandidate(
                    candidates,
                    signatures,
                    portsBySide,
                    "B",
                    "L",
                    new Point(
                        itemBounds.Left -
                            candidateLoopDistance,
                        itemBounds.Bottom +
                            candidateLoopDistance));
                AddSelfLoopCandidate(
                    candidates,
                    signatures,
                    portsBySide,
                    "L",
                    "T",
                    new Point(
                        itemBounds.Left -
                            candidateLoopDistance,
                        itemBounds.Top -
                            candidateLoopDistance));
            }

            if (IsFinite(requestedStart) &&
                IsFinite(requestedEnd) &&
                IsFinite(selfRelationX) &&
                IsFinite(selfRelationY))
            {
                PortCandidate legacySource =
                    new PortCandidate
                    {
                        Anchor = requestedStart,
                        Escape = new Point(
                            requestedStart.X,
                            selfRelationY),
                        Id = "SelfLegacyStart",
                        Penalty = 0
                    };
                PortCandidate legacyTarget =
                    new PortCandidate
                    {
                        Anchor = requestedEnd,
                        Escape = new Point(
                            selfRelationX,
                            requestedEnd.Y),
                        Id = "SelfLegacyEnd",
                        Penalty = 0
                    };
                AddCandidate(
                    candidates,
                    signatures,
                    new List<Point>
                    {
                        requestedStart,
                        new Point(
                            requestedStart.X,
                            selfRelationY),
                        new Point(
                            selfRelationX,
                            selfRelationY),
                        new Point(
                            selfRelationX,
                            requestedEnd.Y),
                        requestedEnd
                    },
                    legacySource,
                    legacyTarget);
            }

            EndpointCollisionGeometry selfGeometry =
                CreateEndpointCollisionGeometry(
                    item,
                    itemBounds);

            return SelectAndCreateRoute(
                candidates,
                obstacles,
                requestedStart,
                requestedEnd,
                clearance,
                previousRoute,
                IsLiveDrag(owningVisualiser),
                Math.Max(
                    itemBounds.Width,
                    itemBounds.Height),
                startMarkerExtent,
                endMarkerExtent,
                item,
                item,
                selfGeometry,
                selfGeometry);
        }

        static void AddSelfLoopCandidate(
            IList<RouteCandidate> candidates,
            ISet<string> signatures,
            IDictionary<string, PortCandidate> portsBySide,
            string sourceSide,
            string targetSide,
            Point outsideCorner)
        {
            PortCandidate source;
            PortCandidate target;

            if (!portsBySide.TryGetValue(sourceSide, out source) ||
                !portsBySide.TryGetValue(targetSide, out target))
            {
                return;
            }

            AddCandidate(
                candidates,
                signatures,
                new List<Point>
                {
                    source.Anchor,
                    source.Escape,
                    outsideCorner,
                    target.Escape,
                    target.Anchor
                },
                source,
                target);
        }

        static DiagramLineRoute SelectAndCreateRoute(
            IList<RouteCandidate> candidates,
            IList<RoutingObstacle> obstacles,
            Point requestedStart,
            Point requestedEnd,
            double clearance,
            DiagramLineRoute previousRoute,
            bool isLiveDrag,
            double directDistanceOverride = 0,
            double startMarkerExtent = 0,
            double endMarkerExtent = 0,
            IUXItem sourceItem = null,
            IUXItem targetItem = null,
            EndpointCollisionGeometry sourceGeometry =
                default(EndpointCollisionGeometry),
            EndpointCollisionGeometry targetGeometry =
                default(EndpointCollisionGeometry))
        {
            if (candidates.Count == 0)
            {
                return CreateLegacyRoute(
                    requestedStart,
                    requestedEnd,
                    false,
                    0,
                    0);
            }

            double directDistance = directDistanceOverride > 0
                ? directDistanceOverride
                : Math.Max(
                    1,
                    DiagramLineRoute.Distance(
                        requestedStart,
                        requestedEnd));

            foreach (RouteCandidate candidate in candidates)
            {
                EvaluateCandidate(
                    candidate,
                    obstacles,
                    directDistance,
                    clearance,
                    previousRoute,
                    isLiveDrag,
                    startMarkerExtent,
                    endMarkerExtent,
                    sourceItem,
                    targetItem,
                    sourceGeometry,
                    targetGeometry);
            }

            RouteCandidate selectedCandidate =
                candidates
                    .Where(candidate =>
                        candidate.IntersectedItemCount == 0 &&
                        candidate.Length <= Math.Max(
                            directDistance * 2.5,
                            directDistance + 300) &&
                        candidate.BendCost <= 6)
                    .OrderBy(candidate => candidate.Cost)
                    .ThenBy(candidate => candidate.Points.Count)
                    .ThenBy(candidate =>
                        candidate.SourcePort.Id,
                        StringComparer.Ordinal)
                    .ThenBy(candidate =>
                        candidate.TargetPort.Id,
                        StringComparer.Ordinal)
                    .FirstOrDefault();

            if (selectedCandidate == null)
            {
                selectedCandidate = candidates
                    .OrderBy(candidate => candidate.Cost)
                    .ThenBy(candidate =>
                        candidate.IntersectedItemCount)
                    .ThenBy(candidate => candidate.Points.Count)
                    .ThenBy(candidate =>
                        candidate.SourcePort.Id,
                        StringComparer.Ordinal)
                    .ThenBy(candidate =>
                        candidate.TargetPort.Id,
                        StringComparer.Ordinal)
                    .First();
            }

            DiagramLineRouteStatus status;
            if (selectedCandidate.IntersectedItemCount > 0)
                status = DiagramLineRouteStatus.Fallback;
            else if (selectedCandidate.InflatedPenetrationLength >
                GeometryEpsilon)
            {
                status =
                    DiagramLineRouteStatus.ReducedClearance;
            }
            else if (selectedCandidate.BendCost < 0.01)
                status = DiagramLineRouteStatus.Direct;
            else
                status = DiagramLineRouteStatus.Routed;

            List<DiagramLineRouteSegment> segments =
                BuildRoundedSegments(
                    selectedCandidate.Points,
                    obstacles,
                    status,
                    sourceGeometry,
                    targetGeometry);

            return new DiagramLineRoute(
                selectedCandidate.Points,
                segments,
                selectedCandidate.SourcePort.Id,
                selectedCandidate.TargetPort.Id,
                status,
                selectedCandidate.Cost);
        }

        static List<DiagramLineRouteSegment>
            BuildRoundedSegments(
                IList<Point> originalPoints,
                IList<RoutingObstacle> obstacles,
                DiagramLineRouteStatus status,
                EndpointCollisionGeometry sourceGeometry,
                EndpointCollisionGeometry targetGeometry)
        {
            List<Point> points =
                RemoveConsecutiveDuplicatePoints(originalPoints);
            List<DiagramLineRouteSegment> result =
                new List<DiagramLineRouteSegment>();

            if (points.Count < 2)
                return result;

            Point current = points[0];

            for (int pointIndex = 1;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                Point previous = points[pointIndex - 1];
                Point corner = points[pointIndex];
                Point next = points[pointIndex + 1];

                bool preserveTerminalStub =
                    pointIndex == 1 ||
                    pointIndex == points.Count - 2;

                Vector incoming = corner - previous;
                Vector outgoing = next - corner;
                double incomingLength = incoming.Length;
                double outgoingLength = outgoing.Length;

                if (preserveTerminalStub ||
                    incomingLength < 2 ||
                    outgoingLength < 2)
                {
                    AddLineSegment(result, current, corner);
                    current = corner;
                    continue;
                }

                incoming.Normalize();
                outgoing.Normalize();

                double cross =
                    incoming.X * outgoing.Y -
                    incoming.Y * outgoing.X;
                double dot =
                    incoming.X * outgoing.X +
                    incoming.Y * outgoing.Y;

                if (Math.Abs(cross) < 0.01 && dot > 0)
                {
                    AddLineSegment(result, current, corner);
                    current = corner;
                    continue;
                }

                double trim = Math.Min(
                    PreferredCornerRadius,
                    Math.Min(
                        incomingLength * 0.35,
                        outgoingLength * 0.35));

                Point curveStart =
                    corner - incoming * trim;
                Point curveEnd =
                    corner + outgoing * trim;

                bool useInflatedBounds =
                    status != DiagramLineRouteStatus.Fallback &&
                    status !=
                        DiagramLineRouteStatus.ReducedClearance;

                if (!IsQuadraticClear(
                        curveStart,
                        corner,
                        curveEnd,
                        obstacles,
                        useInflatedBounds,
                        sourceGeometry,
                        targetGeometry))
                {
                    AddLineSegment(result, current, corner);
                    current = corner;
                    continue;
                }

                AddLineSegment(result, current, curveStart);
                result.Add(
                    DiagramLineRouteSegment
                        .CreateQuadraticBezier(
                            curveStart,
                            corner,
                            curveEnd));
                current = curveEnd;
            }

            AddLineSegment(
                result,
                current,
                points[points.Count - 1]);

            return result;
        }

        static void AddLineSegment(
            ICollection<DiagramLineRouteSegment> segments,
            Point start,
            Point end)
        {
            if (DiagramLineRoute.Distance(start, end) <=
                GeometryEpsilon)
            {
                return;
            }

            segments.Add(
                DiagramLineRouteSegment.CreateLine(start, end));
        }

        static bool IsQuadraticClear(
            Point start,
            Point control,
            Point end,
            IList<RoutingObstacle> obstacles,
            bool useInflatedBounds,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            Point previous = start;
            const int stepCount = 12;

            for (int step = 1; step <= stepCount; step++)
            {
                double t = (double)step / stepCount;
                double oneMinusT = 1 - t;
                Point point = new Point(
                    oneMinusT * oneMinusT * start.X +
                    2 * oneMinusT * t * control.X +
                    t * t * end.X,
                    oneMinusT * oneMinusT * start.Y +
                    2 * oneMinusT * t * control.Y +
                    t * t * end.Y);

                if (!IsSegmentClear(
                        previous,
                        point,
                        obstacles,
                        useInflatedBounds,
                        sourceGeometry,
                        targetGeometry))
                {
                    return false;
                }

                previous = point;
            }

            return true;
        }

        static void EvaluateCandidate(
            RouteCandidate candidate,
            IList<RoutingObstacle> obstacles,
            double directDistance,
            double clearance,
            DiagramLineRoute previousRoute,
            bool isLiveDrag,
            double startMarkerExtent,
            double endMarkerExtent,
            IUXItem sourceItem,
            IUXItem targetItem,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            candidate.Length = GetPolylineLength(
                candidate.Points);
            candidate.BendCost = GetBendCost(
                candidate.Points);

            HashSet<RoutingObstacle> intersectedObstacles =
                new HashSet<RoutingObstacle>();
            HashSet<IUXItem> intersectedEndpoints =
                new HashSet<IUXItem>();
            double rawPenetration = 0;
            double inflatedPenetration = 0;

            for (int pointIndex = 0;
                pointIndex < candidate.Points.Count - 1;
                pointIndex++)
            {
                Point start = candidate.Points[pointIndex];
                Point end = candidate.Points[pointIndex + 1];

                foreach (RoutingObstacle obstacle in obstacles)
                {
                    double rawLength =
                        GetSegmentRectanglePenetrationLength(
                            start,
                            end,
                            obstacle.RawBounds);
                    if (rawLength > GeometryEpsilon)
                    {
                        intersectedObstacles.Add(obstacle);
                        rawPenetration += rawLength;
                    }

                    inflatedPenetration +=
                        GetSegmentRectanglePenetrationLength(
                            start,
                            end,
                            obstacle.InflatedBounds);
                }

                if (SegmentCrossesEndpointInterior(
                        sourceGeometry,
                        start,
                        end) &&
                    intersectedEndpoints.Add(sourceItem))
                {
                    rawPenetration +=
                        DiagramLineRoute.Distance(start, end);
                }

                if (!object.ReferenceEquals(
                        sourceItem,
                        targetItem) &&
                    SegmentCrossesEndpointInterior(
                        targetGeometry,
                        start,
                        end) &&
                    intersectedEndpoints.Add(targetItem))
                {
                    rawPenetration +=
                        DiagramLineRoute.Distance(start, end);
                }
            }

            candidate.IntersectedItemCount =
                intersectedObstacles.Count +
                intersectedEndpoints.Count;
            candidate.RawPenetrationLength =
                rawPenetration;
            candidate.InflatedPenetrationLength =
                inflatedPenetration;

            double nodeViolation =
                candidate.IntersectedItemCount +
                rawPenetration / directDistance;
            double clearanceViolation =
                Math.Max(
                    0,
                    inflatedPenetration -
                    rawPenetration) /
                directDistance;
            double detour =
                Math.Max(
                    0,
                    candidate.Length / directDistance - 1);
            double portPenalty =
                candidate.SourcePort.Penalty +
                candidate.TargetPort.Penalty;
            double instability = GetInstability(
                candidate,
                previousRoute,
                clearance);
            double stabilityWeight = isLiveDrag ? 3 : 0.75;
            double firstSegmentLength =
                candidate.Points.Count >= 2
                    ? DiagramLineRoute.Distance(
                        candidate.Points[0],
                        candidate.Points[1])
                    : 0;
            double lastSegmentLength =
                candidate.Points.Count >= 2
                    ? DiagramLineRoute.Distance(
                        candidate.Points[
                            candidate.Points.Count - 2],
                        candidate.Points[
                            candidate.Points.Count - 1])
                    : 0;
            double terminalShortfall =
                Math.Max(
                    0,
                    startMarkerExtent -
                    firstSegmentLength) +
                Math.Max(
                    0,
                    endMarkerExtent -
                    lastSegmentLength);

            if (candidate.Points.Count == 2)
            {
                terminalShortfall = Math.Max(
                    terminalShortfall,
                    Math.Max(
                        0,
                        startMarkerExtent +
                        endMarkerExtent +
                        2 -
                        firstSegmentLength));
            }

            candidate.Cost =
                30 * nodeViolation +
                6 * clearanceViolation +
                4 * detour +
                2 * candidate.BendCost +
                20 * terminalShortfall /
                    Math.Max(1, clearance) +
                portPenalty +
                stabilityWeight * instability;
        }

        static double GetInstability(
            RouteCandidate candidate,
            DiagramLineRoute previousRoute,
            double clearance)
        {
            if (previousRoute == null ||
                previousRoute.FlattenedPoints.Count < 2)
            {
                return 0;
            }

            double instability = 0;

            if (!string.Equals(
                    candidate.SourcePort.Id,
                    previousRoute.SourcePortId,
                    StringComparison.Ordinal))
            {
                instability += 1.5;
            }

            if (!string.Equals(
                    candidate.TargetPort.Id,
                    previousRoute.TargetPortId,
                    StringComparison.Ordinal))
            {
                instability += 1.5;
            }

            instability += Math.Min(
                2,
                Math.Abs(
                    candidate.Points.Count -
                    previousRoute.SkeletonPoints.Count) * 0.2);

            double geometryDifference = 0;
            for (int sample = 1; sample < 4; sample++)
            {
                double fraction = sample / 4.0;
                Point candidatePoint =
                    GetPointOnPolyline(
                        candidate.Points,
                        fraction);
                Point previousPoint;
                Vector previousTangent;
                previousRoute.GetPointAndTangentAtFraction(
                    fraction,
                    out previousPoint,
                    out previousTangent);
                geometryDifference += Math.Min(
                    4,
                    DiagramLineRoute.Distance(
                        candidatePoint,
                        previousPoint) /
                    Math.Max(1, clearance));
            }

            return instability + geometryDifference / 3;
        }

        static void AddPreviousRouteCandidate(
            IList<RouteCandidate> candidates,
            ISet<string> signatures,
            DiagramLineRoute previousRoute,
            IList<PortPair> portPairs)
        {
            if (previousRoute == null ||
                previousRoute.SkeletonPoints.Count < 4)
            {
                return;
            }

            PortPair matchingPair = portPairs.FirstOrDefault(
                pair =>
                    string.Equals(
                        pair.Source.Id,
                        previousRoute.SourcePortId,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        pair.Target.Id,
                        previousRoute.TargetPortId,
                        StringComparison.Ordinal));

            if (matchingPair == null)
                return;

            List<Point> points =
                new List<Point>(previousRoute.SkeletonPoints);
            points[0] = matchingPair.Source.Anchor;
            points[1] = matchingPair.Source.Escape;
            points[points.Count - 2] =
                matchingPair.Target.Escape;
            points[points.Count - 1] =
                matchingPair.Target.Anchor;

            AddCandidate(
                candidates,
                signatures,
                points,
                matchingPair.Source,
                matchingPair.Target);
        }

        static List<PortPair> BuildRankedPortPairs(
            IList<PortCandidate> sourcePorts,
            IList<PortCandidate> targetPorts,
            DiagramLineRoute previousRoute)
        {
            List<PortPair> pairs = new List<PortPair>();

            foreach (PortCandidate source in sourcePorts)
                foreach (PortCandidate target in targetPorts)
                {
                    double previousPortBonus = 0;

                    if (previousRoute != null &&
                        string.Equals(
                            source.Id,
                            previousRoute.SourcePortId,
                            StringComparison.Ordinal))
                    {
                        previousPortBonus -= 20;
                    }

                    if (previousRoute != null &&
                        string.Equals(
                            target.Id,
                            previousRoute.TargetPortId,
                            StringComparison.Ordinal))
                    {
                        previousPortBonus -= 20;
                    }

                    pairs.Add(new PortPair
                    {
                        Source = source,
                        Target = target,
                        Rank = DiagramLineRoute.Distance(
                            source.Escape,
                            target.Escape) +
                            10 * (
                                source.Penalty +
                                target.Penalty) +
                            previousPortBonus
                    });
                }

            return pairs
                .OrderBy(pair => pair.Rank)
                .ThenBy(
                    pair => pair.Source.Id,
                    StringComparer.Ordinal)
                .ThenBy(
                    pair => pair.Target.Id,
                    StringComparer.Ordinal)
                .Take(MaximumPortPairs)
                .ToList();
        }

        static bool TerminalDirectionsAreValid(
            PortPair portPair)
        {
            Vector routeDirection =
                portPair.Target.Anchor -
                portPair.Source.Anchor;
            Vector sourceOutward =
                portPair.Source.Escape -
                portPair.Source.Anchor;
            Vector targetOutward =
                portPair.Target.Escape -
                portPair.Target.Anchor;

            if (routeDirection.Length < GeometryEpsilon ||
                sourceOutward.Length < GeometryEpsilon ||
                targetOutward.Length < GeometryEpsilon)
            {
                return false;
            }

            routeDirection.Normalize();
            sourceOutward.Normalize();
            targetOutward.Normalize();

            double sourceAlignment =
                routeDirection.X * sourceOutward.X +
                routeDirection.Y * sourceOutward.Y;
            double targetAlignment =
                -routeDirection.X * targetOutward.X -
                routeDirection.Y * targetOutward.Y;

            return sourceAlignment > 0.2 &&
                targetAlignment > 0.2;
        }

        static List<PortCandidate> BuildPorts(
            IUXItem item,
            Rect itemBounds,
            Point destination,
            Point requestedAnchor,
            double clearance,
            string idPrefix,
            bool includeCardinalPorts)
        {
            List<PortCandidate> ports =
                new List<PortCandidate>();
            Point center = GetCenter(itemBounds);
            Vector desiredDirection = destination - center;

            if (desiredDirection.Length < GeometryEpsilon)
                desiredDirection = new Vector(1, -1);

            desiredDirection.Normalize();

            if (IsFinite(requestedAnchor))
            {
                Vector requestedNormal =
                    requestedAnchor - center;

                if (requestedNormal.Length <
                    GeometryEpsilon)
                {
                    requestedNormal = desiredDirection;
                }

                requestedNormal.Normalize();
                AddPortIfUnique(
                    ports,
                    new PortCandidate
                    {
                        Anchor = requestedAnchor,
                        Escape =
                            requestedAnchor +
                            requestedNormal * clearance,
                        Id = idPrefix + "D",
                        Penalty = 0
                    });
            }

            if (!includeCardinalPorts)
                return ports;

            AddCardinalPort(
                ports,
                item,
                itemBounds,
                center,
                desiredDirection,
                new Vector(-1, 0),
                clearance,
                idPrefix + "L",
                requestedAnchor);
            AddCardinalPort(
                ports,
                item,
                itemBounds,
                center,
                desiredDirection,
                new Vector(1, 0),
                clearance,
                idPrefix + "R",
                requestedAnchor);
            AddCardinalPort(
                ports,
                item,
                itemBounds,
                center,
                desiredDirection,
                new Vector(0, -1),
                clearance,
                idPrefix + "T",
                requestedAnchor);
            AddCardinalPort(
                ports,
                item,
                itemBounds,
                center,
                desiredDirection,
                new Vector(0, 1),
                clearance,
                idPrefix + "B",
                requestedAnchor);

            return ports;
        }

        static void AddCardinalPort(
            IList<PortCandidate> ports,
            IUXItem item,
            Rect itemBounds,
            Point center,
            Vector desiredDirection,
            Vector outwardNormal,
            double clearance,
            string id,
            Point requestedAnchor)
        {
            double distanceToOutside =
                Math.Abs(outwardNormal.X) > 0
                    ? itemBounds.Width / 2 + clearance
                    : itemBounds.Height / 2 + clearance;
            Point escape =
                center +
                outwardNormal * distanceToOutside;
            Point anchor = item.GetLineEdgeIntersection(
                escape,
                center - escape);

            if (!IsFinite(anchor) ||
                DiagramLineRoute.Distance(anchor, escape) <
                    GeometryEpsilon)
            {
                anchor = new Point(
                    center.X +
                        outwardNormal.X *
                        itemBounds.Width / 2,
                    center.Y +
                        outwardNormal.Y *
                        itemBounds.Height / 2);
            }

            escape = anchor + outwardNormal * clearance;

            double directionPenalty =
                1 - (
                    outwardNormal.X * desiredDirection.X +
                    outwardNormal.Y * desiredDirection.Y);
            double requestedAnchorPenalty =
                IsFinite(requestedAnchor)
                    ? DiagramLineRoute.Distance(
                        anchor,
                        requestedAnchor) /
                        Math.Max(
                            1,
                            Math.Max(
                                itemBounds.Width,
                                itemBounds.Height))
                    : 0;

            AddPortIfUnique(
                ports,
                new PortCandidate
                {
                    Anchor = anchor,
                    Escape = escape,
                    Id = id,
                    Penalty =
                        1.5 * directionPenalty +
                        0.5 * requestedAnchorPenalty
                });
        }

        static void AddPortIfUnique(
            IList<PortCandidate> ports,
            PortCandidate candidate)
        {
            foreach (PortCandidate existing in ports)
                if (DiagramLineRoute.Distance(
                        existing.Anchor,
                        candidate.Anchor) < 0.5)
                {
                    return;
                }

            ports.Add(candidate);
        }

        static List<RoutingObstacle> BuildObstacles(
            IUXItem sourceItem,
            IUXItem targetItem,
            IUXVisualiser owningVisualiser,
            double clearance,
            double lineWidth)
        {
            UXVisualiser concreteVisualiser =
                owningVisualiser as UXVisualiser;

            if (concreteVisualiser == null)
            {
                return new List<RoutingObstacle>();
            }

            HashSet<IUXItem> excludedItems =
                new HashSet<IUXItem>();
            AddItemAndAncestors(sourceItem, excludedItems);
            AddItemAndAncestors(targetItem, excludedItems);

            List<RoutingObstacle> obstacles =
                new List<RoutingObstacle>();
            IList<IUXItem> routingItems =
                concreteVisualiser
                    .GetDiagramRoutingItems();

            foreach (IUXItem item in
                routingItems
                    .OrderBy(GetStableItemKey, StringComparer.Ordinal))
            {
                if (item == null ||
                    excludedItems.Contains(item) ||
                    item is IUXDecorator ||
                    item is ILineDecoratorBase)
                {
                    continue;
                }

                Rect bounds;
                if (!TryGetVisibleBounds(
                        item,
                        owningVisualiser.Canvas,
                        out bounds))
                {
                    continue;
                }

                Rect rawRoutingBounds = bounds;
                rawRoutingBounds.Inflate(
                    Math.Max(0.5, lineWidth / 2 + 0.5),
                    Math.Max(0.5, lineWidth / 2 + 0.5));

                Rect inflatedBounds = bounds;
                inflatedBounds.Inflate(
                    clearance,
                    clearance);

                obstacles.Add(new RoutingObstacle
                {
                    Item = item,
                    RawBounds = rawRoutingBounds,
                    InflatedBounds = inflatedBounds,
                    StableKey = GetStableItemKey(item)
                });
            }

            return obstacles;
        }

        static void AddItemAndAncestors(
            IUXItem item,
            ISet<IUXItem> result)
        {
            IItem current = item;

            while (current != null)
            {
                IUXItem currentItem = current as IUXItem;
                if (currentItem != null)
                    result.Add(currentItem);

                current = current.ParentItem;
            }
        }

        static string GetStableItemKey(IUXItem item)
        {
            if (item == null ||
                item.Vertex == null ||
                item.Vertex.Identifier == null)
            {
                return string.Empty;
            }

            return item.Vertex.Identifier.ToString();
        }

        internal static bool TryGetVisibleBounds(
            IUXItem item,
            Canvas canvas,
            out Rect bounds)
        {
            bounds = Rect.Empty;
            FrameworkElement element =
                item as FrameworkElement;

            if (element == null ||
                canvas == null ||
                element.Visibility != Visibility.Visible ||
                !element.IsVisible ||
                element.ActualWidth <= 0 ||
                element.ActualHeight <= 0)
            {
                return false;
            }

            try
            {
                GeneralTransform transform =
                    element.TransformToAncestor(canvas);
                bounds = transform.TransformBounds(
                    new Rect(
                        0,
                        0,
                        element.ActualWidth,
                        element.ActualHeight));

                DependencyObject ancestor =
                    VisualTreeHelper.GetParent(element);

                while (ancestor != null &&
                    !object.ReferenceEquals(ancestor, canvas))
                {
                    FrameworkElement ancestorElement =
                        ancestor as FrameworkElement;

                    if (ancestorElement != null &&
                        (ancestorElement.ClipToBounds ||
                         ancestorElement.Clip != null) &&
                        ancestorElement.ActualWidth > 0 &&
                        ancestorElement.ActualHeight > 0)
                    {
                        GeneralTransform ancestorTransform =
                            ancestorElement.TransformToAncestor(
                                canvas);
                        Rect ancestorBounds =
                            ancestorTransform.TransformBounds(
                                new Rect(
                                    0,
                                    0,
                                    ancestorElement.ActualWidth,
                                    ancestorElement.ActualHeight));
                        bounds.Intersect(ancestorBounds);

                        if (bounds.IsEmpty)
                            return false;
                    }

                    ancestor = VisualTreeHelper.GetParent(
                        ancestor);
                }
            }
            catch
            {
                try
                {
                    Point leftTop = element.TranslatePoint(
                        new Point(),
                        canvas);
                    bounds = new Rect(
                        leftTop,
                        new System.Windows.Size(
                            element.ActualWidth,
                            element.ActualHeight));
                }
                catch
                {
                    return false;
                }
            }

            return !bounds.IsEmpty &&
                IsFinite(bounds.TopLeft) &&
                IsFinite(bounds.BottomRight);
        }

        static List<Point> FindVisibilityPath(
            Point start,
            Point end,
            IList<RoutingObstacle> obstacles,
            bool useInflatedBounds,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            int expansionCount = 0;

            List<Point> nodes = new List<Point>
                {
                    start,
                    end
                };

                foreach (RoutingObstacle obstacle in obstacles)
                {
                    Rect bounds = useInflatedBounds
                        ? obstacle.InflatedBounds
                        : obstacle.RawBounds;
                    nodes.Add(bounds.TopLeft);
                    nodes.Add(bounds.TopRight);
                    nodes.Add(bounds.BottomRight);
                    nodes.Add(bounds.BottomLeft);
                }

                SearchState initialState =
                    new SearchState(-1, 0);
                PriorityQueue<SearchQueueEntry, double> queue =
                    new PriorityQueue<SearchQueueEntry, double>();
                Dictionary<SearchState, double> bestCosts =
                    new Dictionary<SearchState, double>();
                Dictionary<SearchState, SearchState> parents =
                    new Dictionary<SearchState, SearchState>();

                bestCosts[initialState] = 0;
                queue.Enqueue(
                    new SearchQueueEntry(initialState, 0),
                    DiagramLineRoute.Distance(start, end));

                SearchState finalState = new SearchState();
                bool found = false;

                while (queue.Count > 0 &&
                    expansionCount < MaximumSearchExpansions)
                {
                    SearchQueueEntry queueEntry =
                        queue.Dequeue();
                    SearchState state = queueEntry.State;
                    double currentCost;

                    if (!bestCosts.TryGetValue(
                            state,
                            out currentCost) ||
                        Math.Abs(
                            currentCost -
                            queueEntry.Cost) >
                            GeometryEpsilon)
                    {
                        continue;
                    }

                    if (state.Current == 1)
                    {
                        finalState = state;
                        found = true;
                        break;
                    }

                    expansionCount++;

                    for (int nextIndex = 0;
                        nextIndex < nodes.Count;
                        nextIndex++)
                    {
                        if (nextIndex == state.Current ||
                            nextIndex == state.Previous)
                        {
                            continue;
                        }

                        Point currentPoint =
                            nodes[state.Current];
                        Point nextPoint = nodes[nextIndex];
                        double edgeLength =
                            DiagramLineRoute.Distance(
                                currentPoint,
                                nextPoint);

                        if (edgeLength <= GeometryEpsilon ||
                            !IsSegmentClear(
                                currentPoint,
                                nextPoint,
                                obstacles,
                                useInflatedBounds,
                                sourceGeometry,
                                targetGeometry))
                        {
                            continue;
                        }

                        double turnPenalty = 0;
                        if (state.Previous >= 0)
                        {
                            turnPenalty = 12 *
                                GetTurnFraction(
                                    nodes[state.Previous],
                                    currentPoint,
                                    nextPoint);
                        }

                        double nextCost =
                            currentCost +
                            edgeLength +
                            turnPenalty;
                        SearchState nextState =
                            new SearchState(
                                state.Current,
                                nextIndex);
                        double knownCost;

                        if (bestCosts.TryGetValue(
                                nextState,
                                out knownCost) &&
                            knownCost <= nextCost)
                        {
                            continue;
                        }

                        bestCosts[nextState] = nextCost;
                        parents[nextState] = state;

                        double estimatedTotal =
                            nextCost +
                            DiagramLineRoute.Distance(
                                nextPoint,
                                end);
                        queue.Enqueue(
                            new SearchQueueEntry(
                                nextState,
                                nextCost),
                            estimatedTotal);
                    }
                }

                if (!found)
                    return null;

                List<int> nodeIndexes = new List<int>();
                SearchState currentState = finalState;
                nodeIndexes.Add(currentState.Current);

                while (!currentState.Equals(initialState))
                {
                    SearchState parent;
                    if (!parents.TryGetValue(
                            currentState,
                            out parent))
                    {
                        return null;
                    }

                    currentState = parent;
                    nodeIndexes.Add(currentState.Current);
                }

                nodeIndexes.Reverse();

                List<Point> result = nodeIndexes
                    .Select(nodeIndex => nodes[nodeIndex])
                    .ToList();

                return result;
        }

        static bool IsPolylineClear(
            IList<Point> points,
            IList<RoutingObstacle> obstacles,
            bool useInflatedBounds,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            if (points == null || points.Count < 2)
                return false;

            for (int pointIndex = 0;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                if (!IsSegmentClear(
                        points[pointIndex],
                        points[pointIndex + 1],
                        obstacles,
                        useInflatedBounds,
                        sourceGeometry,
                        targetGeometry))
                {
                    return false;
                }
            }

            return true;
        }

        static void RecordCheapRouteClearance(
            IList<Point> points,
            IList<RoutingObstacle> obstacles,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry,
            ref bool hasInflatedClearCheapRoute,
            ref bool hasRawClearCheapRoute)
        {
            if (hasInflatedClearCheapRoute)
                return;

            if (IsPolylineClear(
                    points,
                    obstacles,
                    true,
                    sourceGeometry,
                    targetGeometry))
            {
                hasInflatedClearCheapRoute = true;
                hasRawClearCheapRoute = true;
                return;
            }

            if (hasRawClearCheapRoute)
                return;

            if (IsPolylineClear(
                    points,
                    obstacles,
                    false,
                    sourceGeometry,
                    targetGeometry))
            {
                hasRawClearCheapRoute = true;
            }
        }

        static bool VisibilitySearchEscapesAreBlocked(
            Point sourceEscape,
            Point targetEscape,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            return PointIsInsideEndpointInterior(
                    targetGeometry,
                    sourceEscape) ||
                PointIsInsideEndpointInterior(
                    sourceGeometry,
                    targetEscape);
        }

        static bool IsSegmentClear(
            Point start,
            Point end,
            IEnumerable<RoutingObstacle> obstacles,
            bool useInflatedBounds,
            EndpointCollisionGeometry sourceGeometry,
            EndpointCollisionGeometry targetGeometry)
        {
            foreach (RoutingObstacle obstacle in obstacles)
            {
                Rect bounds = useInflatedBounds
                    ? obstacle.InflatedBounds
                    : obstacle.RawBounds;

                if (SegmentIntersectsRectangleInterior(
                        start,
                        end,
                        bounds))
                {
                    return false;
                }
            }

            if (SegmentCrossesEndpointInterior(
                    sourceGeometry,
                    start,
                    end))
            {
                return false;
            }

            if (SegmentCrossesEndpointInterior(
                    targetGeometry,
                    start,
                    end))
            {
                return false;
            }

            return true;
        }

        static bool UsesRectangularLinePorts(IUXItem item)
        {
            UXItem concreteItem = item as UXItem;
            return concreteItem == null ||
                concreteItem.UsesRectangularLinePorts;
        }

        static EndpointCollisionGeometry CreateEndpointCollisionGeometry(
            IUXItem item,
            Rect bounds)
        {
            EndpointCollisionGeometry geometry =
                new EndpointCollisionGeometry();
            geometry.Bounds = bounds;
            geometry.HasBounds =
                !bounds.IsEmpty &&
                IsFinite(bounds.TopLeft) &&
                IsFinite(bounds.BottomRight);

            if (item is RhombusItem)
                geometry.Shape = EndpointShapeKind.Rhombus;
            else if (item is OvalItem)
                geometry.Shape = EndpointShapeKind.Ellipse;
            else
                geometry.Shape = EndpointShapeKind.Rectangle;

            return geometry;
        }

        static bool PointIsInsideEndpointInterior(
            EndpointCollisionGeometry endpoint,
            Point point)
        {
            if (!endpoint.HasBounds)
                return false;

            switch (endpoint.Shape)
            {
                case EndpointShapeKind.Rhombus:
                    return PointIsInsideInscribedRhombusInterior(
                        point,
                        endpoint.Bounds);
                case EndpointShapeKind.Ellipse:
                    return PointIsInsideEllipseInterior(
                        point,
                        endpoint.Bounds);
                default:
                    return PointIsInsideRectangleInterior(
                        point,
                        endpoint.Bounds);
            }
        }

        static bool PointIsInsideRectangleInterior(
            Point point,
            Rect bounds)
        {
            return point.X > bounds.X + GeometryEpsilon &&
                point.X < bounds.Right - GeometryEpsilon &&
                point.Y > bounds.Y + GeometryEpsilon &&
                point.Y < bounds.Bottom - GeometryEpsilon;
        }

        static bool SegmentCrossesEndpointInterior(
            EndpointCollisionGeometry endpoint,
            Point start,
            Point end)
        {
            if (!endpoint.HasBounds)
                return false;

            switch (endpoint.Shape)
            {
                case EndpointShapeKind.Rhombus:
                    return SegmentCrossesInscribedRhombusInterior(
                        start,
                        end,
                        endpoint.Bounds);
                case EndpointShapeKind.Ellipse:
                    return SegmentCrossesEllipseInterior(
                        start,
                        end,
                        endpoint.Bounds);
                default:
                    return SegmentIntersectsRectangleInterior(
                        start,
                        end,
                        endpoint.Bounds);
            }
        }

        internal static bool SegmentCrossesInscribedRhombusInterior(
            Point start,
            Point end,
            Rect bounds)
        {
            return SegmentSamplesShapeInterior(
                start,
                end,
                point => PointIsInsideInscribedRhombusInterior(
                    point,
                    bounds));
        }

        internal static bool SegmentCrossesEllipseInterior(
            Point start,
            Point end,
            Rect bounds)
        {
            return SegmentSamplesShapeInterior(
                start,
                end,
                point => PointIsInsideEllipseInterior(
                    point,
                    bounds));
        }

        internal static bool PointIsInsideInscribedRhombusInterior(
            Point point,
            Rect bounds)
        {
            if (bounds.Width <= GeometryEpsilon ||
                bounds.Height <= GeometryEpsilon)
            {
                return false;
            }

            double nx = Math.Abs(
                point.X - (bounds.X + bounds.Width / 2)) /
                (bounds.Width / 2);
            double ny = Math.Abs(
                point.Y - (bounds.Y + bounds.Height / 2)) /
                (bounds.Height / 2);
            return nx + ny < 0.98;
        }

        internal static bool PointIsInsideEllipseInterior(
            Point point,
            Rect bounds)
        {
            if (bounds.Width <= GeometryEpsilon ||
                bounds.Height <= GeometryEpsilon)
            {
                return false;
            }

            double nx =
                (point.X - (bounds.X + bounds.Width / 2)) /
                (bounds.Width / 2);
            double ny =
                (point.Y - (bounds.Y + bounds.Height / 2)) /
                (bounds.Height / 2);
            return nx * nx + ny * ny < 0.96;
        }

        static bool SegmentSamplesShapeInterior(
            Point start,
            Point end,
            Func<Point, bool> containsInterior)
        {
            const int sampleCount = 9;

            for (int sampleIndex = 1;
                sampleIndex < sampleCount;
                sampleIndex++)
            {
                double t = (double)sampleIndex / sampleCount;
                Point sample = new Point(
                    start.X + (end.X - start.X) * t,
                    start.Y + (end.Y - start.Y) * t);

                if (containsInterior(sample))
                    return true;
            }

            return false;
        }

        internal static bool PolylineIntersectsBounds(
            IReadOnlyList<Point> points,
            Rect bounds)
        {
            if (points == null ||
                points.Count < 2 ||
                bounds.IsEmpty)
            {
                return false;
            }

            for (int pointIndex = 0;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                if (SegmentIntersectsRectangleInterior(
                        points[pointIndex],
                        points[pointIndex + 1],
                        bounds))
                {
                    return true;
                }
            }

            return false;
        }

        static bool SegmentIntersectsRectangleInterior(
            Point start,
            Point end,
            Rect bounds)
        {
            Rect interior = bounds;
            const double boundaryTolerance = 0.01;

            if (interior.Width > 2 * boundaryTolerance &&
                interior.Height > 2 * boundaryTolerance)
            {
                interior.Inflate(
                    -boundaryTolerance,
                    -boundaryTolerance);
            }

            double entry;
            double exit;
            return TryClipSegmentToRectangle(
                start,
                end,
                interior,
                out entry,
                out exit) &&
                DiagramLineRoute.Distance(start, end) *
                    Math.Max(0, exit - entry) >
                    GeometryEpsilon;
        }

        static double GetSegmentRectanglePenetrationLength(
            Point start,
            Point end,
            Rect bounds)
        {
            Rect interior = bounds;
            const double boundaryTolerance = 0.01;

            if (interior.Width > 2 * boundaryTolerance &&
                interior.Height > 2 * boundaryTolerance)
            {
                interior.Inflate(
                    -boundaryTolerance,
                    -boundaryTolerance);
            }

            double entry;
            double exit;

            if (!TryClipSegmentToRectangle(
                    start,
                    end,
                    interior,
                    out entry,
                    out exit))
            {
                return 0;
            }

            return DiagramLineRoute.Distance(start, end) *
                Math.Max(0, exit - entry);
        }

        static bool TryClipSegmentToRectangle(
            Point start,
            Point end,
            Rect bounds,
            out double entry,
            out double exit)
        {
            entry = 0;
            exit = 1;

            double deltaX = end.X - start.X;
            double deltaY = end.Y - start.Y;

            if (!ClipTest(
                    -deltaX,
                    start.X - bounds.Left,
                    ref entry,
                    ref exit) ||
                !ClipTest(
                    deltaX,
                    bounds.Right - start.X,
                    ref entry,
                    ref exit) ||
                !ClipTest(
                    -deltaY,
                    start.Y - bounds.Top,
                    ref entry,
                    ref exit) ||
                !ClipTest(
                    deltaY,
                    bounds.Bottom - start.Y,
                    ref entry,
                    ref exit))
            {
                return false;
            }

            return exit >= entry;
        }

        static bool ClipTest(
            double denominator,
            double numerator,
            ref double entry,
            ref double exit)
        {
            if (Math.Abs(denominator) < GeometryEpsilon)
                return numerator >= 0;

            double ratio = numerator / denominator;

            if (denominator < 0)
            {
                if (ratio > exit)
                    return false;
                if (ratio > entry)
                    entry = ratio;
            }
            else
            {
                if (ratio < entry)
                    return false;
                if (ratio < exit)
                    exit = ratio;
            }

            return true;
        }

        static double GetBendCost(IList<Point> points)
        {
            double bendCost = 0;

            for (int pointIndex = 1;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                bendCost += GetTurnFraction(
                    points[pointIndex - 1],
                    points[pointIndex],
                    points[pointIndex + 1]);
            }

            return bendCost;
        }

        static double GetTurnFraction(
            Point previous,
            Point current,
            Point next)
        {
            Vector incoming = current - previous;
            Vector outgoing = next - current;

            if (incoming.Length < GeometryEpsilon ||
                outgoing.Length < GeometryEpsilon)
            {
                return 0;
            }

            incoming.Normalize();
            outgoing.Normalize();

            double dot =
                incoming.X * outgoing.X +
                incoming.Y * outgoing.Y;
            dot = Math.Max(-1, Math.Min(1, dot));

            return Math.Acos(dot) / Math.PI;
        }

        static double GetPolylineLength(IList<Point> points)
        {
            double length = 0;

            for (int pointIndex = 0;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                length += DiagramLineRoute.Distance(
                    points[pointIndex],
                    points[pointIndex + 1]);
            }

            return length;
        }

        static Point GetPointOnPolyline(
            IList<Point> points,
            double fraction)
        {
            if (points.Count == 0)
                return new Point();
            if (points.Count == 1)
                return points[0];

            double totalLength = GetPolylineLength(points);
            if (totalLength <= GeometryEpsilon)
                return points[0];

            double targetLength =
                Math.Max(0, Math.Min(1, fraction)) *
                totalLength;
            double accumulated = 0;

            for (int pointIndex = 0;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                Point start = points[pointIndex];
                Point end = points[pointIndex + 1];
                double segmentLength =
                    DiagramLineRoute.Distance(start, end);

                if (accumulated + segmentLength <
                    targetLength &&
                    pointIndex < points.Count - 2)
                {
                    accumulated += segmentLength;
                    continue;
                }

                double localFraction =
                    segmentLength <= GeometryEpsilon
                        ? 0
                        : (targetLength - accumulated) /
                            segmentLength;
                localFraction = Math.Max(
                    0,
                    Math.Min(1, localFraction));

                return new Point(
                    start.X +
                        (end.X - start.X) * localFraction,
                    start.Y +
                        (end.Y - start.Y) * localFraction);
            }

            return points[points.Count - 1];
        }

        static List<Point> ComposeFullRoute(
            PortCandidate source,
            IEnumerable<Point> middlePoints,
            PortCandidate target)
        {
            List<Point> result = new List<Point>
            {
                source.Anchor,
                source.Escape
            };

            foreach (Point point in middlePoints)
                AddPointIfDifferent(result, point);

            AddPointIfDifferent(result, target.Escape);
            AddPointIfDifferent(result, target.Anchor);

            return result;
        }

        static void AddCandidate(
            ICollection<RouteCandidate> candidates,
            ISet<string> signatures,
            IList<Point> points,
            PortCandidate source,
            PortCandidate target)
        {
            List<Point> cleanedPoints =
                RemoveConsecutiveDuplicatePoints(points);

            if (cleanedPoints.Count < 2)
                return;

            string signature = source.Id + "|" +
                target.Id + "|" +
                string.Join(
                    ";",
                    cleanedPoints.Select(point =>
                        Math.Round(point.X, 1) + "," +
                        Math.Round(point.Y, 1)));

            if (!signatures.Add(signature))
                return;

            candidates.Add(new RouteCandidate
            {
                Points = cleanedPoints,
                SourcePort = source,
                TargetPort = target
            });
        }

        static List<Point> RemoveConsecutiveDuplicatePoints(
            IEnumerable<Point> points)
        {
            List<Point> result = new List<Point>();

            foreach (Point point in points)
                AddPointIfDifferent(result, point);

            return result;
        }

        static void AddPointIfDifferent(
            IList<Point> points,
            Point point)
        {
            if (!IsFinite(point))
                return;

            if (points.Count == 0 ||
                DiagramLineRoute.Distance(
                    points[points.Count - 1],
                    point) > GeometryEpsilon)
            {
                points.Add(point);
            }
        }

        static DiagramLineRoute CreateLegacyRoute(
            Point start,
            Point end,
            bool isSelfRelation,
            double selfRelationX,
            double selfRelationY)
        {
            List<Point> points = new List<Point>
            {
                start
            };

            if (isSelfRelation)
            {
                points.Add(new Point(start.X, selfRelationY));
                points.Add(new Point(
                    selfRelationX,
                    selfRelationY));
                points.Add(new Point(
                    selfRelationX,
                    end.Y));
            }

            points.Add(end);
            points = RemoveConsecutiveDuplicatePoints(points);

            List<DiagramLineRouteSegment> segments =
                new List<DiagramLineRouteSegment>();

            for (int pointIndex = 0;
                pointIndex < points.Count - 1;
                pointIndex++)
            {
                AddLineSegment(
                    segments,
                    points[pointIndex],
                    points[pointIndex + 1]);
            }

            return new DiagramLineRoute(
                points,
                segments,
                "LegacyStart",
                "LegacyEnd",
                DiagramLineRouteStatus.Fallback,
                0);
        }

        static bool IsLiveDrag(
            IUXVisualiser owningVisualiser)
        {
            UXVisualiser concreteVisualiser =
                owningVisualiser as UXVisualiser;

            return concreteVisualiser != null &&
                concreteVisualiser
                    .IsItemMoveGraphInteractionActive;
        }

        static Point GetCenter(Rect bounds)
        {
            return new Point(
                bounds.Left + bounds.Width / 2,
                bounds.Top + bounds.Height / 2);
        }

        static Point GetAutomaticAnchor(
            IUXItem item,
            Rect bounds,
            Point destination,
            double clearance)
        {
            Point center = GetCenter(bounds);
            Vector outward = destination - center;

            if (outward.Length < GeometryEpsilon)
                outward = new Vector(1, 0);

            outward.Normalize();
            Point outside =
                center +
                outward * (
                    Math.Max(bounds.Width, bounds.Height) +
                    clearance);
            Point anchor = item.GetLineEdgeIntersection(
                outside,
                center - outside);

            if (IsFinite(anchor) &&
                DiagramLineRoute.Distance(
                    anchor,
                    outside) >
                    GeometryEpsilon)
            {
                return anchor;
            }

            return new Point(
                center.X +
                    outward.X * bounds.Width / 2,
                center.Y +
                    outward.Y * bounds.Height / 2);
        }

        static bool IsFinite(Point point)
        {
            return IsFinite(point.X) &&
                IsFinite(point.Y);
        }

        static bool IsFinite(double value)
        {
            return !double.IsNaN(value) &&
                !double.IsInfinity(value);
        }
    }
}

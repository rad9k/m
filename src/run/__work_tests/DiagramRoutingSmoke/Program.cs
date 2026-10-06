using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        VerifyMixedRouteGeometry();
        VerifyRectangleIntersectionKernel();
        VerifyRhombusAndEllipseInterior();
        Console.WriteLine("Diagram routing smoke tests passed.");
    }

    static void VerifyMixedRouteGeometry()
    {
        List<Point> skeleton = new List<Point>
        {
            new Point(0, 0),
            new Point(20, 0),
            new Point(20, 20),
            new Point(40, 20)
        };
        List<DiagramLineRouteSegment> segments =
            new List<DiagramLineRouteSegment>
            {
                DiagramLineRouteSegment.CreateLine(
                    new Point(0, 0),
                    new Point(15, 0)),
                DiagramLineRouteSegment
                    .CreateQuadraticBezier(
                        new Point(15, 0),
                        new Point(20, 0),
                        new Point(20, 5)),
                DiagramLineRouteSegment.CreateLine(
                    new Point(20, 5),
                    new Point(20, 20)),
                DiagramLineRouteSegment.CreateLine(
                    new Point(20, 20),
                    new Point(40, 20))
            };

        ConstructorInfo constructor =
            typeof(DiagramLineRoute).GetConstructor(
                BindingFlags.Instance |
                    BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(IEnumerable<Point>),
                    typeof(IEnumerable<
                        DiagramLineRouteSegment>),
                    typeof(string),
                    typeof(string),
                    typeof(DiagramLineRouteStatus),
                    typeof(double)
                },
                null);

        Assert(constructor != null, "Route constructor missing.");

        DiagramLineRoute route =
            (DiagramLineRoute)constructor.Invoke(
                new object[]
                {
                    skeleton,
                    segments,
                    "S",
                    "T",
                    DiagramLineRouteStatus.Routed,
                    0d
                });

        Assert(
            route.FlattenedPoints.Count > segments.Count,
            "Bezier segment was not flattened.");
        Assert(
            route.TotalLength > 50 &&
                route.TotalLength < 70,
            "Unexpected mixed-route length.");
        Assert(
            route.CreateBodyGeometry(5, 5)
                .Figures.Count == 1,
            "Body geometry was not created.");
        Assert(
            route.GetDistanceFromPoint(
                new Point(10, 1)) < 1.1,
            "Route hit distance is incorrect.");

        Point middle;
        Vector tangent;
        route.GetPointAndTangentAtFraction(
            0.5,
            out middle,
            out tangent);
        Assert(
            IsFinite(middle) &&
                tangent.Length > 0.99,
            "Arc-length lookup returned invalid data.");
    }

    static void VerifyRectangleIntersectionKernel()
    {
        Type routerType =
            typeof(DiagramLineRoute).Assembly.GetType(
                "m0.ZeroTypes.UX.DiagramLineRouter",
                true);
        MethodInfo intersectsMethod =
            routerType.GetMethod(
                "SegmentIntersectsRectangleInterior",
                BindingFlags.Static |
                    BindingFlags.NonPublic);

        Assert(
            intersectsMethod != null,
            "Intersection kernel missing.");

        Rect obstacle = new Rect(40, 40, 20, 20);

        bool crossing = (bool)intersectsMethod.Invoke(
            null,
            new object[]
            {
                new Point(0, 50),
                new Point(100, 50),
                obstacle
            });
        bool tangent = (bool)intersectsMethod.Invoke(
            null,
            new object[]
            {
                new Point(0, 40),
                new Point(100, 40),
                obstacle
            });
        bool longShallowCrossing =
            (bool)intersectsMethod.Invoke(
                null,
                new object[]
                {
                    new Point(0, 50),
                    new Point(10000, 50),
                    new Rect(5000, 40, 0.5, 20)
                });

        Assert(crossing, "Crossing was not detected.");
        Assert(!tangent, "Boundary tangent was blocked.");
        Assert(
            longShallowCrossing,
            "Short penetration on a long segment was missed.");
    }

    static void VerifyRhombusAndEllipseInterior()
    {
        Type routerType =
            typeof(DiagramLineRoute).Assembly.GetType(
                "m0.ZeroTypes.UX.DiagramLineRouter",
                true);
        MethodInfo rhombusPointMethod =
            routerType.GetMethod(
                "PointIsInsideInscribedRhombusInterior",
                BindingFlags.Static |
                    BindingFlags.NonPublic);
        MethodInfo rhombusSegmentMethod =
            routerType.GetMethod(
                "SegmentCrossesInscribedRhombusInterior",
                BindingFlags.Static |
                    BindingFlags.NonPublic);
        MethodInfo ellipsePointMethod =
            routerType.GetMethod(
                "PointIsInsideEllipseInterior",
                BindingFlags.Static |
                    BindingFlags.NonPublic);

        Assert(
            rhombusPointMethod != null &&
                rhombusSegmentMethod != null &&
                ellipsePointMethod != null,
            "Shape interior helpers missing.");

        Rect diamond = new Rect(0, 0, 100, 100);

        bool centerInside = (bool)rhombusPointMethod.Invoke(
            null,
            new object[] { new Point(50, 50), diamond });
        bool vertexOutside = (bool)rhombusPointMethod.Invoke(
            null,
            new object[] { new Point(0, 50), diamond });
        bool cornerOutside = (bool)rhombusPointMethod.Invoke(
            null,
            new object[] { new Point(5, 5), diamond });
        bool chordThroughCenter =
            (bool)rhombusSegmentMethod.Invoke(
                null,
                new object[]
                {
                    new Point(-20, 50),
                    new Point(120, 50),
                    diamond
                });
        bool outwardStub =
            (bool)rhombusSegmentMethod.Invoke(
                null,
                new object[]
                {
                    new Point(0, 50),
                    new Point(-30, 50),
                    diamond
                });
        bool ellipseCenter = (bool)ellipsePointMethod.Invoke(
            null,
            new object[] { new Point(50, 50), diamond });
        bool ellipseCorner = (bool)ellipsePointMethod.Invoke(
            null,
            new object[] { new Point(5, 5), diamond });

        Assert(centerInside, "Rhombus center was not interior.");
        Assert(!vertexOutside, "Rhombus vertex was treated as interior.");
        Assert(
            !cornerOutside,
            "AABB corner of a rhombus was treated as interior.");
        Assert(
            chordThroughCenter,
            "Horizontal chord through a rhombus was not detected.");
        Assert(
            !outwardStub,
            "Outward stub from a rhombus vertex was blocked.");
        Assert(ellipseCenter, "Ellipse center was not interior.");
        Assert(
            !ellipseCorner,
            "AABB corner of an ellipse was treated as interior.");
    }

    static bool IsFinite(Point point)
    {
        return !double.IsNaN(point.X) &&
            !double.IsInfinity(point.X) &&
            !double.IsNaN(point.Y) &&
            !double.IsInfinity(point.Y);
    }

    static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}

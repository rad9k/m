using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Commands;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using m0.ZeroUML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace m0.UIWpf.Visualisers
{
    // Tag stored on each 3D edge so highlight/selection mirrors the 2D GraphVisualiser.
    public class LineTagStore3D
    {
        public VertexNode3D FromNode;
        public VertexNode3D ToNode;
        public IVertex MetaVertex;
        public TextBlock MetaLabel;
    }

    // 3D counterpart of SimpleVisualiserWrapper. One instance per vertex on the scene.
    // Owns the sphere body, the 2D billboard label, edges pointing in/out of it,
    // and a GraphChangeTrigger listener so value/edge changes rebuild the graph.
    public class VertexNode3D : ModelVisual3D, IDisposable
    {
        public IVertex BaseVertex;
        public GraphVisualiser3D ParentVisualiser;

        public Point3D Position;

        public GeometryModel3D BodyModel;
        public DiffuseMaterial BodyMaterial;
        public EmissiveMaterial BodyEmissive;

        public Viewport2DVisual3D LabelVisual;
        public FrameworkElement LabelContent;

        public TranslateTransform3D Translate;
        public ScaleTransform3D Scale;

        public List<EdgeLine3D> Lines = new List<EdgeLine3D>();

        public bool IsSelected;
        public bool IsHighlighted;

        private IEdge listenerEdge;

        public VertexNode3D(IVertex baseVertex, GraphVisualiser3D parent)
        {
            BaseVertex = baseVertex;
            ParentVisualiser = parent;

            Translate = new TranslateTransform3D(0, 0, 0);
            Scale = new ScaleTransform3D(1, 1, 1);

            Transform3DGroup group = new Transform3DGroup();
            group.Children.Add(Scale);
            group.Children.Add(Translate);
            this.Transform = group;

            if (baseVertex != null)
                listenerEdge = ExecutionFlowHelper.AddTriggerAndListener(baseVertex,
                    new List<string> { },
                    new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                        GraphChangeFilterEnum.OutputEdgeAdded,
                        GraphChangeFilterEnum.OutputEdgeRemoved,
                        GraphChangeFilterEnum.OutputEdgeDisposed},
                    "Basic3DTrigger",
                    VertexChange);
        }

        protected INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (BaseVertex == null || BaseVertex.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

            IVertex valueChange = exe.Stack.Get(false, @"event:\Type:ValueChange");

            if (valueChange == null)
                ParentVisualiser.RequestRepaint();

            return exe.Stack;
        }

        public void SetWorldPosition(Point3D p)
        {
            Position = p;
            Translate.OffsetX = p.X;
            Translate.OffsetY = p.Y;
            Translate.OffsetZ = p.Z;
        }

        public void Select()
        {
            IsSelected = true;
            ApplyBodyColors();
        }

        public void Unselect()
        {
            IsSelected = false;
            ApplyBodyColors();
        }

        public void HighlightThisAndDescendants()
        {
            IsHighlighted = true;
            ApplyBodyColors();

            foreach (EdgeLine3D line in Lines)
            {
                line.SetHighlighted(true);

                LineTagStore3D tag = line.Tag as LineTagStore3D;
                if (tag != null)
                {
                    if (tag.ToNode != this) tag.ToNode.ApplyLocalHighlight(true);
                    if (tag.FromNode != this) tag.FromNode.ApplyLocalHighlight(true);
                }
            }
        }

        public void UnhighlightThisAndDescendants()
        {
            IsHighlighted = false;
            ApplyBodyColors();

            foreach (EdgeLine3D line in Lines)
            {
                line.SetHighlighted(false);

                LineTagStore3D tag = line.Tag as LineTagStore3D;
                if (tag != null)
                {
                    if (tag.ToNode != this) tag.ToNode.ApplyLocalHighlight(false);
                    if (tag.FromNode != this) tag.FromNode.ApplyLocalHighlight(false);
                }
            }
        }

        // Only touches the visual state of a single node - used to light up the
        // neighbors of a highlighted node without cascading further.
        public void ApplyLocalHighlight(bool on)
        {
            IsHighlighted = on;
            ApplyBodyColors();
        }

        private void ApplyBodyColors()
        {
            if (BodyMaterial == null || BodyEmissive == null) return;

            Color baseColor;
            Color emissive;

            if (IsSelected)
            {
                baseColor = ParentVisualiser.GetThemeColor("0SelectionBrush", Colors.DodgerBlue);
                emissive  = Darken(baseColor, 0.35);
            }
            else if (IsHighlighted)
            {
                baseColor = ParentVisualiser.GetThemeColor("0HighlightBrush", Colors.OrangeRed);
                emissive  = Darken(baseColor, 0.5);
            }
            else
            {
                baseColor = ParentVisualiser.GetThemeColor("0LightGrayBrush", Colors.LightGray);
                emissive  = Colors.Black;
            }

            BodyMaterial.Brush = new SolidColorBrush(baseColor);
            BodyEmissive.Brush = new SolidColorBrush(emissive);
        }

        private static Color Darken(Color c, double factor)
        {
            return Color.FromRgb(
                (byte)(c.R * factor),
                (byte)(c.G * factor),
                (byte)(c.B * factor));
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            if (listenerEdge != null)
                GraphChangeTrigger.RemoveListener(listenerEdge);

            if (LabelContent is IDisposable)
                ((IDisposable)LabelContent).Dispose();
        }
    }

    // 3D counterpart of ArrowLine. Contains a thin cylinder (trunk) and a cone (arrow tip)
    // connecting two VertexNode3D instances. Transforms are used so the same canonical
    // cylinder mesh is reused across all edges.
    public class EdgeLine3D : ModelVisual3D
    {
        public LineTagStore3D Tag;

        private GeometryModel3D trunk;
        private GeometryModel3D tip;
        private DiffuseMaterial trunkMaterial;
        private DiffuseMaterial tipMaterial;

        private Transform3DGroup trunkTransform;
        private Transform3DGroup tipTransform;

        private GraphVisualiser3D parent;

        public EdgeLine3D(GraphVisualiser3D parentVisualiser)
        {
            parent = parentVisualiser;

            Model3DGroup group = new Model3DGroup();

            trunkMaterial = new DiffuseMaterial(new SolidColorBrush(parent.GetThemeColor("0LightGrayBrush", Colors.LightGray)));
            tipMaterial   = new DiffuseMaterial(new SolidColorBrush(parent.GetThemeColor("0LightGrayBrush", Colors.LightGray)));

            trunk = new GeometryModel3D(GeometryFactory3D.UnitCylinder, trunkMaterial);
            tip   = new GeometryModel3D(GeometryFactory3D.UnitCone,     tipMaterial);

            trunkTransform = new Transform3DGroup();
            tipTransform   = new Transform3DGroup();

            trunk.Transform = trunkTransform;
            tip.Transform   = tipTransform;

            group.Children.Add(trunk);
            group.Children.Add(tip);

            Content = group;
        }

        public void UpdateGeometry(Point3D from, Point3D to, double trunkRadius, double tipLength, double tipRadius)
        {
            Vector3D direction = to - from;
            double length = direction.Length;
            if (length < 0.001) return;

            Vector3D dirNormalized = direction / length;

            double trunkLength = Math.Max(0, length - tipLength);
            Point3D trunkStart = from;
            Point3D trunkEnd   = from + dirNormalized * trunkLength;
            Point3D tipStart   = trunkEnd;

            trunkTransform.Children.Clear();
            trunkTransform.Children.Add(new ScaleTransform3D(trunkRadius, trunkLength, trunkRadius));
            trunkTransform.Children.Add(AlignYToDirectionTransform(dirNormalized));
            trunkTransform.Children.Add(new TranslateTransform3D(trunkStart.X, trunkStart.Y, trunkStart.Z));

            tipTransform.Children.Clear();
            tipTransform.Children.Add(new ScaleTransform3D(tipRadius, tipLength, tipRadius));
            tipTransform.Children.Add(AlignYToDirectionTransform(dirNormalized));
            tipTransform.Children.Add(new TranslateTransform3D(tipStart.X, tipStart.Y, tipStart.Z));
        }

        // Rotation that maps the canonical cylinder/cone Y-axis to the given direction.
        private static Transform3D AlignYToDirectionTransform(Vector3D dir)
        {
            Vector3D y = new Vector3D(0, 1, 0);
            double dot = Vector3D.DotProduct(y, dir);

            if (dot >= 0.9999)
                return Transform3D.Identity;

            if (dot <= -0.9999)
                return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180));

            Vector3D axis = Vector3D.CrossProduct(y, dir);
            axis.Normalize();
            double angleDeg = Math.Acos(dot) * 180.0 / Math.PI;
            return new RotateTransform3D(new AxisAngleRotation3D(axis, angleDeg));
        }

        public void SetColor(Color c)
        {
            trunkMaterial.Brush = new SolidColorBrush(c);
            tipMaterial.Brush   = new SolidColorBrush(c);
        }

        public void SetHighlighted(bool on)
        {
            Color c = on
                ? parent.GetThemeColor("0LightHighlightBrush", Colors.Gold)
                : parent.GetThemeColor("0LightGrayBrush", Colors.LightGray);

            SetColor(c);

            if (Tag != null && Tag.MetaLabel != null)
                Tag.MetaLabel.Foreground = new SolidColorBrush(on
                    ? parent.GetThemeColor("0HighlightBrush", Colors.OrangeRed)
                    : parent.GetThemeColor("0LightGrayBrush", Colors.LightGray));
        }

        public GeometryModel3D TrunkModel => trunk;
        public GeometryModel3D TipModel   => tip;
    }

    // Canonical, frozen meshes reused across all vertices and edges for performance.
    // The sphere is placed at origin with radius 1; the cylinder runs from (0,0,0)
    // to (0,1,0) with radius 1; the cone has its base at y=0 and apex at y=1.
    internal static class GeometryFactory3D
    {
        public static readonly MeshGeometry3D UnitSphere;
        public static readonly MeshGeometry3D UnitCylinder;
        public static readonly MeshGeometry3D UnitCone;

        static GeometryFactory3D()
        {
            UnitSphere   = BuildSphere(16, 12);
            UnitCylinder = BuildCylinder(18);
            UnitCone     = BuildCone(18);

            UnitSphere.Freeze();
            UnitCylinder.Freeze();
            UnitCone.Freeze();
        }

        private static MeshGeometry3D BuildSphere(int longitudeSegments, int latitudeSegments)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                double theta = lat * Math.PI / latitudeSegments;
                double sinTheta = Math.Sin(theta);
                double cosTheta = Math.Cos(theta);

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    double phi = lon * 2 * Math.PI / longitudeSegments;
                    double sinPhi = Math.Sin(phi);
                    double cosPhi = Math.Cos(phi);

                    double x = cosPhi * sinTheta;
                    double y = cosTheta;
                    double z = sinPhi * sinTheta;

                    mesh.Positions.Add(new Point3D(x, y, z));
                    mesh.Normals.Add(new Vector3D(x, y, z));
                    mesh.TextureCoordinates.Add(new Point((double)lon / longitudeSegments, (double)lat / latitudeSegments));
                }
            }

            for (int lat = 0; lat < latitudeSegments; lat++)
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int first = lat * (longitudeSegments + 1) + lon;
                    int second = first + longitudeSegments + 1;

                    mesh.TriangleIndices.Add(first);
                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(first + 1);

                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(second + 1);
                    mesh.TriangleIndices.Add(first + 1);
                }

            return mesh;
        }

        private static MeshGeometry3D BuildCylinder(int segments)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            for (int i = 0; i <= segments; i++)
            {
                double a = i * 2 * Math.PI / segments;
                double x = Math.Cos(a);
                double z = Math.Sin(a);

                mesh.Positions.Add(new Point3D(x, 0, z));
                mesh.Positions.Add(new Point3D(x, 1, z));

                mesh.Normals.Add(new Vector3D(x, 0, z));
                mesh.Normals.Add(new Vector3D(x, 0, z));
            }

            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(b + 2);
                mesh.TriangleIndices.Add(b + 1);

                mesh.TriangleIndices.Add(b + 1);
                mesh.TriangleIndices.Add(b + 2);
                mesh.TriangleIndices.Add(b + 3);
            }

            return mesh;
        }

        private static MeshGeometry3D BuildCone(int segments)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            Point3D apex = new Point3D(0, 1, 0);

            for (int i = 0; i <= segments; i++)
            {
                double a = i * 2 * Math.PI / segments;
                double x = Math.Cos(a);
                double z = Math.Sin(a);

                mesh.Positions.Add(new Point3D(x, 0, z));
                mesh.Positions.Add(apex);

                mesh.Normals.Add(new Vector3D(x, 0.5, z));
                mesh.Normals.Add(new Vector3D(0, 1, 0));
            }

            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(b + 2);
                mesh.TriangleIndices.Add(b + 1);
            }

            return mesh;
        }
    }

    // Main 3D graph visualiser. Mirrors the structure of GraphVisualiser (2D):
    //
    // - ListVisualiserHelper registration under "System\Meta\Visualiser\Graph3D"
    // - Dictionary<IVertex, VertexNode3D> DisplayedVerticesUIElements
    // - PaintGraph() full rebuild, driven by BaseEdge + meta attributes
    // - Selection/highlighting via mouse + SelectedEdges subgraph
    // - Double-click changes BaseEdge (with animated transition)
    //
    // Extra 3D pieces:
    // - Viewport3D + orbital camera + lighting + scene root transform
    // - Layout algorithms placing vertices in spheres/orbits/volumes
    // - BaseEdge-change animations (Cut / OrbitTransition / FlyToAndSwap / ...)
    public class GraphVisualiser3D : Grid, IListVisualiser, IHasSelectableEdges, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        private Viewport3D viewport;
        private PerspectiveCamera camera;
        private ModelVisual3D sceneRoot;
        private ModelVisual3D lightingRoot;

        private ScaleTransform3D sceneScale;
        private AxisAngleRotation3D sceneRotation;
        private RotateTransform3D sceneRotate;

        private double cameraYaw   = 0;
        private double cameraPitch = 0.4;
        private double cameraDistance = 900;

        private Dictionary<IVertex, VertexNode3D> DisplayedVerticesUIElements;
        private Dictionary<Model3D, VertexNode3D> modelToNode;
        private List<EdgeLine3D> edgeLines;

        private VertexNode3D highlighted;

        private IVertex previousBaseEdgeTo;

        private bool isPainting;
        private bool isFirstPainted;
        private bool animationInProgress;

        // Meta attributes that trigger a full rebuild when they change (mirrors the 2D list,
        // extended with 3D-specific keys).
        private static readonly string[] _MetaTriggeringUpdateVertex = new string[] {
            "VisualiserCircleSize", "NumberOfCircles",
            "ShowOutEdges", "ShowInEdges", "FastMode", "MetaLabels",
            "LayoutMode3D", "TransitionStyle", "TransitionDurationMs",
            "SphereSize", "ShowLabels3D"
        };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        private static readonly string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        // TypedEdge START

        public GraphVisualiser3D(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public GraphVisualiser3D(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            DisplayedVerticesUIElements = new Dictionary<IVertex, VertexNode3D>();
            modelToNode = new Dictionary<Model3D, VertexNode3D>();
            edgeLines = new List<EdgeLine3D>();

            SetupHost();

            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Graph3D"),
                this,
                "GraphVisualiser3D",
                this,
                false,
                new List<string> { "" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            this.PreviewMouseLeftButtonDown += Host_PreviewMouseLeftButtonDown;
            this.PreviewMouseMove           += Host_PreviewMouseMove;
            this.MouseWheel                 += Host_MouseWheel;

            SetVertexDefaultValues();
        }

        private void SetupHost()
        {
            Brush background = TryFindResource("0BackgroundBrush") as Brush;
            this.Background = background ?? new SolidColorBrush(Color.FromRgb(24, 24, 28));
            this.ClipToBounds = true;

            viewport = new Viewport3D();
            this.Children.Add(viewport);

            camera = new PerspectiveCamera
            {
                FieldOfView = 60,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 100000
            };
            viewport.Camera = camera;

            sceneScale   = new ScaleTransform3D(1, 1, 1);
            sceneRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            sceneRotate   = new RotateTransform3D(sceneRotation);

            Transform3DGroup sceneTransform = new Transform3DGroup();
            sceneTransform.Children.Add(sceneScale);
            sceneTransform.Children.Add(sceneRotate);

            sceneRoot = new ModelVisual3D { Transform = sceneTransform };
            viewport.Children.Add(sceneRoot);

            lightingRoot = new ModelVisual3D();
            Model3DGroup lightsGroup = new Model3DGroup();
            lightsGroup.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 90)));
            lightsGroup.Children.Add(new DirectionalLight(Color.FromRgb(200, 200, 210), new Vector3D(-1, -1, -1)));
            lightsGroup.Children.Add(new DirectionalLight(Color.FromRgb(80, 80, 100), new Vector3D(1, 0.5, 1)));
            lightingRoot.Content = lightsGroup;
            viewport.Children.Add(lightingRoot);

            UpdateCamera();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();

            PaintGraph();

            if (isFirstPainted)
                this.Loaded -= OnLoad;
        }

        private bool fastMode;
        private bool metaLabels;
        private bool showOutEdges;
        private bool showInEdges;
        private bool showLabels3D = true;

        public void PaintGraph()
        {
            if (Vertex.DisposedState != DisposeStateEnum.Live) return;

            if (ActualWidth == 0 || ActualHeight == 0) return;

            isPainting = true;

            fastMode     = GeneralUtil.CompareStrings(Vertex.Get(false, "FastMode:"),     "True");
            metaLabels   = GeneralUtil.CompareStrings(Vertex.Get(false, "MetaLabels:"),   "True");
            showOutEdges = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowOutEdges:"), "True");
            showInEdges  = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowInEdges:"),  "True");
            showLabels3D = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowLabels3D:"), "True");

            // Dispose existing nodes and clear the scene.
            foreach (VertexNode3D node in DisplayedVerticesUIElements.Values.Distinct())
                node.Dispose();
            DisplayedVerticesUIElements.Clear();

            modelToNode.Clear();
            edgeLines.Clear();

            List<Visual3D> toRemove = new List<Visual3D>();
            foreach (Visual3D v in sceneRoot.Children) toRemove.Add(v);
            foreach (Visual3D v in toRemove) sceneRoot.Children.Remove(v);

            // Reset scene transform after a potential animation.
            sceneRotation.Angle = 0;
            sceneScale.ScaleX = sceneScale.ScaleY = sceneScale.ScaleZ = 1;

            LayoutAlgorithm3DEnum layout = LayoutAlgorithm3DEnumHelper.GetEnum(Vertex.Get(false, "LayoutMode3D:"));

            Stopwatch sw = Stopwatch.StartNew();

            IVertex baseTo = Vertex.Get(false, @"BaseEdge:\To:");
            if (baseTo != null)
            {
                BuildGraphForLayout(baseTo, layout);
                SelectWrappersForSelectedVertices();
            }

            previousBaseEdgeTo = baseTo;
            isFirstPainted = true;
            isPainting = false;

            sw.Stop();
            MinusZero.Instance.Log(1, "GraphVisualiser3D.PaintGraph",
                "layout=" + layout + " vertices=" + DisplayedVerticesUIElements.Count +
                " edges=" + edgeLines.Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        // Internal trigger used by VertexNode3D listeners when a displayed vertex
        // changes - equivalent to the 2D SimpleVisualiserWrapper.VertexChange path.
        public void RequestRepaint()
        {
            if (!isPainting) PaintGraph();
        }

        // Place the BaseEdge at origin then expand the graph outwards according to the
        // chosen algorithm. Always BFS by out/in edges - same contract as 2D AddCircle.
        private void BuildGraphForLayout(IVertex baseTo, LayoutAlgorithm3DEnum layout)
        {
            int numberOfCircles = GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")) ?? 2;
            int circleSize      = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "VisualiserCircleSize:"));
            if (circleSize <= 0) circleSize = 200;

            VertexNode3D root = AddVertex(new Point3D(0, 0, 0), baseTo);

            List<IVertex> previousLevel = new List<IVertex> { baseTo };

            for (int level = 1; level <= numberOfCircles; level++)
            {
                List<IVertex> thisLevel = new List<IVertex>();
                List<(IEdge edge, bool outgoing)> edgesFromPrevious = new List<(IEdge, bool)>();

                foreach (IVertex v in previousLevel)
                {
                    if (showOutEdges)
                        foreach (IEdge e in v)
                            if (CanAddEdge(e) && !DisplayedVerticesUIElements.ContainsKey(e.To))
                                edgesFromPrevious.Add((e, true));

                    if (showInEdges)
                        foreach (IEdge e in v.InEdges.ToList())
                            if (CanAddEdge(e) && !DisplayedVerticesUIElements.ContainsKey(e.From))
                                edgesFromPrevious.Add((e, false));
                }

                PlaceNewLevel(level, numberOfCircles, circleSize, layout, edgesFromPrevious, thisLevel);

                // Connect every edge that touches already-displayed vertices, including
                // duplicates / back-edges / sibling links.
                foreach (IVertex v in previousLevel)
                {
                    if (showOutEdges)
                        foreach (IEdge e in v)
                            if (CanAddEdge(e) && DisplayedVerticesUIElements.ContainsKey(e.To))
                                AddEdge(DisplayedVerticesUIElements[v], DisplayedVerticesUIElements[e.To], e.Meta);

                    if (showInEdges)
                        foreach (IEdge e in v.InEdges.ToList())
                            if (CanAddEdge(e) && DisplayedVerticesUIElements.ContainsKey(e.From))
                                AddEdge(DisplayedVerticesUIElements[e.From], DisplayedVerticesUIElements[v], e.Meta);
                }

                previousLevel = thisLevel;
            }

            // Final closing sweep - connect edges between vertices of the last level
            // that both happen to be on the scene (mirrors the post-loop pass in 2D).
            foreach (IVertex v in previousLevel)
            {
                if (showOutEdges)
                    foreach (IEdge e in v)
                        if (CanAddEdge(e) && DisplayedVerticesUIElements.ContainsKey(e.To))
                            AddEdge(DisplayedVerticesUIElements[v], DisplayedVerticesUIElements[e.To], e.Meta);
                if (showInEdges)
                    foreach (IEdge e in v.InEdges.ToList())
                        if (CanAddEdge(e) && DisplayedVerticesUIElements.ContainsKey(e.From))
                            AddEdge(DisplayedVerticesUIElements[e.From], DisplayedVerticesUIElements[v], e.Meta);
            }

            ApplyPostLayoutRefinement(layout);
        }

        private void PlaceNewLevel(int level, int maxLevel, int shellStep, LayoutAlgorithm3DEnum layout,
            List<(IEdge edge, bool outgoing)> edges, List<IVertex> accumulator)
        {
            if (edges.Count == 0) return;

            double radius = shellStep * level;
            int count = edges.Count;

            for (int i = 0; i < count; i++)
            {
                (IEdge e, bool outgoing) = edges[i];
                IVertex target = outgoing ? e.To : e.From;

                if (DisplayedVerticesUIElements.ContainsKey(target)) continue;

                Point3D p = ComputeLevelPosition(layout, level, maxLevel, i, count, radius, e.Meta);

                VertexNode3D node = AddVertex(p, target);
                accumulator.Add(target);

                VertexNode3D peer = outgoing
                    ? DisplayedVerticesUIElements[e.From]
                    : DisplayedVerticesUIElements[e.To];

                if (outgoing) AddEdge(peer, node, e.Meta);
                else          AddEdge(node, peer, e.Meta);
            }
        }

        private Point3D ComputeLevelPosition(LayoutAlgorithm3DEnum layout, int level, int maxLevel,
            int index, int count, double radius, IVertex metaVertex)
        {
            switch (layout)
            {
                case LayoutAlgorithm3DEnum.FibonacciSphereShells:
                    return FibonacciPoint(index, count, radius);

                case LayoutAlgorithm3DEnum.OrbitalPlanes:
                    return OrbitalPoint(index, count, radius, metaVertex);

                case LayoutAlgorithm3DEnum.ConcentricSpiral3D:
                    return SpiralPoint(index, count, radius, level, maxLevel);

                case LayoutAlgorithm3DEnum.Sugiyama3DLayers:
                    return Sugiyama3DPoint(index, count, radius, level, maxLevel);

                case LayoutAlgorithm3DEnum.Force3D:
                    // Force3D starts from a Fibonacci seed and then relaxes in
                    // ApplyPostLayoutRefinement to avoid expensive recomputes.
                    return FibonacciPoint(index, count, radius);

                default:
                    return FibonacciPoint(index, count, radius);
            }
        }

        private static Point3D FibonacciPoint(int index, int total, double radius)
        {
            if (total < 1) total = 1;
            double k = total == 1 ? 0 : (2.0 * index) / (total - 1) - 1.0;
            double phi = index * Math.PI * (3 - Math.Sqrt(5));
            double r = Math.Sqrt(Math.Max(0, 1 - k * k));
            return new Point3D(Math.Cos(phi) * r * radius,
                               k * radius,
                               Math.Sin(phi) * r * radius);
        }

        // Each meta vertex defines a unique tilted orbital plane. Children sharing the
        // same meta sit on the same ring around the BaseEdge.
        private Dictionary<IVertex, int> metaOrbitIndex = new Dictionary<IVertex, int>();

        private Point3D OrbitalPoint(int index, int total, double radius, IVertex metaVertex)
        {
            if (metaVertex == null) return FibonacciPoint(index, total, radius);

            if (!metaOrbitIndex.TryGetValue(metaVertex, out int orbitId))
            {
                orbitId = metaOrbitIndex.Count;
                metaOrbitIndex[metaVertex] = orbitId;
            }

            double tiltYaw   = orbitId * 0.618 * Math.PI;         // golden-angle tilt
            double tiltPitch = (orbitId % 5) * Math.PI / 10.0;
            double angle     = (double)index / Math.Max(1, total) * Math.PI * 2;

            double x = Math.Cos(angle) * radius;
            double y = 0;
            double z = Math.Sin(angle) * radius;

            double cy = Math.Cos(tiltYaw),   sy = Math.Sin(tiltYaw);
            double cp = Math.Cos(tiltPitch), sp = Math.Sin(tiltPitch);

            double x1 =  x * cp + y * sp;
            double y1 = -x * sp + y * cp;
            double z1 =  z;

            double x2 =  x1 * cy + z1 * sy;
            double z2 = -x1 * sy + z1 * cy;

            return new Point3D(x2, y1, z2);
        }

        private static Point3D SpiralPoint(int index, int total, double radius, int level, int maxLevel)
        {
            double t = (double)index / Math.Max(1, total);
            double angle = t * Math.PI * 4;
            double yOffset = (level - (maxLevel + 1) / 2.0) * radius * 0.8;
            double r = radius * (0.4 + 0.6 * t);
            return new Point3D(Math.Cos(angle) * r, yOffset + t * radius * 0.6, Math.Sin(angle) * r);
        }

        private static Point3D Sugiyama3DPoint(int index, int total, double radius, int level, int maxLevel)
        {
            if (total < 1) total = 1;
            double longitude = (double)index / total * Math.PI * 2;
            double latitude  = Math.PI / (maxLevel + 1) * level - Math.PI / 2;
            double r = radius;
            return new Point3D(Math.Cos(latitude) * Math.Cos(longitude) * r,
                               Math.Sin(latitude) * r,
                               Math.Cos(latitude) * Math.Sin(longitude) * r);
        }

        // Refinement passes (lightweight force relaxation + edge endpoint recalc).
        private void ApplyPostLayoutRefinement(LayoutAlgorithm3DEnum layout)
        {
            if (layout == LayoutAlgorithm3DEnum.Force3D)
                RelaxForce3D(60);

            foreach (EdgeLine3D line in edgeLines)
                UpdateEdgeEndpoints(line);
        }

        private void RelaxForce3D(int iterations)
        {
            List<VertexNode3D> nodes = DisplayedVerticesUIElements.Values.Distinct().ToList();
            int n = nodes.Count;
            if (n < 2) return;

            double volumeK = 200; // characteristic spring length
            double temperature = 80;

            Dictionary<VertexNode3D, Vector3D> disp = new Dictionary<VertexNode3D, Vector3D>();

            Dictionary<VertexNode3D, HashSet<VertexNode3D>> adj = new Dictionary<VertexNode3D, HashSet<VertexNode3D>>();
            foreach (VertexNode3D node in nodes) adj[node] = new HashSet<VertexNode3D>();
            foreach (EdgeLine3D line in edgeLines)
            {
                LineTagStore3D tag = line.Tag as LineTagStore3D;
                if (tag == null) continue;
                if (tag.FromNode == tag.ToNode) continue;
                adj[tag.FromNode].Add(tag.ToNode);
                adj[tag.ToNode].Add(tag.FromNode);
            }

            VertexNode3D pinned = DisplayedVerticesUIElements.Values.FirstOrDefault();

            for (int iter = 0; iter < iterations; iter++)
            {
                foreach (VertexNode3D node in nodes) disp[node] = new Vector3D(0, 0, 0);

                for (int i = 0; i < n; i++)
                    for (int j = i + 1; j < n; j++)
                    {
                        Vector3D delta = nodes[i].Position - nodes[j].Position;
                        double dist = delta.Length;
                        if (dist < 0.001) { delta = new Vector3D(0.1, 0.1, 0.1); dist = delta.Length; }
                        double repulsion = (volumeK * volumeK) / dist;
                        Vector3D push = delta / dist * repulsion;
                        disp[nodes[i]] += push;
                        disp[nodes[j]] -= push;
                    }

                foreach (VertexNode3D a in nodes)
                    foreach (VertexNode3D b in adj[a])
                    {
                        if (a.GetHashCode() >= b.GetHashCode()) continue; // process each pair once
                        Vector3D delta = a.Position - b.Position;
                        double dist = delta.Length;
                        if (dist < 0.001) continue;
                        double attraction = (dist * dist) / volumeK;
                        Vector3D pull = delta / dist * attraction;
                        disp[a] -= pull;
                        disp[b] += pull;
                    }

                foreach (VertexNode3D node in nodes)
                {
                    if (node == pinned) continue;
                    Vector3D d = disp[node];
                    double len = d.Length;
                    if (len > 0)
                    {
                        Vector3D step = d / len * Math.Min(len, temperature);
                        node.SetWorldPosition(node.Position + step);
                    }
                }

                temperature *= 0.95;
            }
        }

        private bool CanAddEdge(IEdge e)
        {
            if (e == null || e.Meta == null) return true;
            if (GeneralUtil.CompareStrings(e.Meta, "$GraphChangeTrigger")) return false;
            return true;
        }

        private VertexNode3D AddVertex(Point3D p, IVertex baseVertex)
        {
            VertexNode3D node = new VertexNode3D(baseVertex, this);

            double sphereSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "SphereSize:"));
            if (sphereSize <= 0) sphereSize = 24;

            node.Scale.ScaleX = node.Scale.ScaleY = node.Scale.ScaleZ = sphereSize;

            Color baseColor = GetThemeColor("0LightGrayBrush", Colors.LightGray);
            node.BodyMaterial = new DiffuseMaterial(new SolidColorBrush(baseColor));
            node.BodyEmissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black));

            MaterialGroup matGroup = new MaterialGroup();
            matGroup.Children.Add(node.BodyMaterial);
            matGroup.Children.Add(node.BodyEmissive);

            node.BodyModel = new GeometryModel3D(GeometryFactory3D.UnitSphere, matGroup);
            node.BodyModel.BackMaterial = node.BodyMaterial;

            Model3DGroup group = new Model3DGroup();
            group.Children.Add(node.BodyModel);
            node.Content = group;

            modelToNode[node.BodyModel] = node;

            node.SetWorldPosition(p);

            if (showLabels3D)
                AttachLabel(node, baseVertex, sphereSize);

            DisplayedVerticesUIElements[baseVertex] = node;

            sceneRoot.Children.Add(node);

            return node;
        }

        private void AttachLabel(VertexNode3D node, IVertex baseVertex, double sphereSize)
        {
            FrameworkElement labelElement = CreateLabelElement(baseVertex);
            if (labelElement == null) return;

            node.LabelContent = labelElement;

            // Billboard rectangle placed just above the sphere. Uses Viewport2DVisual3D
            // so the embedded WPF element participates in hit-testing and rendering.
            double w = 120;
            double h = 30;

            MeshGeometry3D planeMesh = new MeshGeometry3D();
            planeMesh.Positions.Add(new Point3D(-w / 2, 0, 0));
            planeMesh.Positions.Add(new Point3D( w / 2, 0, 0));
            planeMesh.Positions.Add(new Point3D( w / 2, h, 0));
            planeMesh.Positions.Add(new Point3D(-w / 2, h, 0));
            planeMesh.TextureCoordinates.Add(new Point(0, 1));
            planeMesh.TextureCoordinates.Add(new Point(1, 1));
            planeMesh.TextureCoordinates.Add(new Point(1, 0));
            planeMesh.TextureCoordinates.Add(new Point(0, 0));
            planeMesh.TriangleIndices.Add(0);
            planeMesh.TriangleIndices.Add(1);
            planeMesh.TriangleIndices.Add(2);
            planeMesh.TriangleIndices.Add(0);
            planeMesh.TriangleIndices.Add(2);
            planeMesh.TriangleIndices.Add(3);

            DiffuseMaterial labelMaterial = new DiffuseMaterial { Brush = Brushes.White };
            Viewport2DVisual3D.SetIsVisualHostMaterial(labelMaterial, true);

            Viewport2DVisual3D viewport2D = new Viewport2DVisual3D
            {
                Geometry = planeMesh,
                Material = labelMaterial,
                Visual = labelElement
            };

            // Position the label plane in node-local space, above the sphere body.
            TranslateTransform3D labelOffset = new TranslateTransform3D(0, 1.2, 0);
            viewport2D.Transform = labelOffset;

            // Scale the whole label with the sphere so it stays readable.
            // The plane already carries its Translate; we need to place it in world
            // space by adding it as a child of the node. ModelVisual3D children get
            // their parent's Transform, but Viewport2DVisual3D can be added directly.
            ModelVisual3D labelHost = new ModelVisual3D();
            labelHost.Transform = new TranslateTransform3D(node.Position.X, node.Position.Y + sphereSize * 1.1, node.Position.Z);
            labelHost.Children.Add(viewport2D);

            node.LabelVisual = viewport2D;
            sceneRoot.Children.Add(labelHost);
        }

        private FrameworkElement CreateLabelElement(IVertex v)
        {
            if (v == null) return null;

            TextBlock tb = new TextBlock
            {
                Text = v.Value != null ? v.Value.ToString() : "Ø",
                Foreground = new SolidColorBrush(GetThemeColor("0ForegroundBrush", Colors.White)),
                Background = new SolidColorBrush(GetThemeColor("0BackgroundBrush", Color.FromArgb(200, 0, 0, 0))),
                Padding = new Thickness(4, 1, 4, 1),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            return tb;
        }

        private void AddEdge(VertexNode3D from, VertexNode3D to, IVertex meta)
        {
            if (from == null || to == null || from == to) return;

            EdgeLine3D line = new EdgeLine3D(this);
            LineTagStore3D tag = new LineTagStore3D
            {
                FromNode = from,
                ToNode   = to,
                MetaVertex = meta
            };
            line.Tag = tag;

            UpdateEdgeEndpoints(line);

            Color metaColor = HashToColor(meta);
            line.SetColor(metaColor);

            from.Lines.Add(line);
            to.Lines.Add(line);
            edgeLines.Add(line);

            sceneRoot.Children.Add(line);
        }

        private void UpdateEdgeEndpoints(EdgeLine3D line)
        {
            LineTagStore3D tag = line.Tag as LineTagStore3D;
            if (tag == null || tag.FromNode == null || tag.ToNode == null) return;

            double sphereSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "SphereSize:"));
            if (sphereSize <= 0) sphereSize = 24;

            // Trim the ends so the cylinder starts and stops at the sphere surface.
            Vector3D direction = tag.ToNode.Position - tag.FromNode.Position;
            double len = direction.Length;
            if (len < 0.001) return;

            Vector3D unit = direction / len;
            Point3D from = tag.FromNode.Position + unit * sphereSize;
            Point3D to   = tag.ToNode.Position   - unit * sphereSize;

            double trunkRadius = Math.Max(1, sphereSize * 0.08);
            double tipLength   = sphereSize * 0.8;
            double tipRadius   = sphereSize * 0.25;

            line.UpdateGeometry(from, to, trunkRadius, tipLength, tipRadius);
        }

        private Color HashToColor(IVertex meta)
        {
            if (meta == null || meta.Value == null)
                return GetThemeColor("0LightGrayBrush", Colors.LightGray);

            int h = meta.Value.ToString().GetHashCode();
            double hue = ((uint)h % 360) / 360.0;
            return HsvToRgb(hue, 0.45, 0.85);
        }

        private static Color HsvToRgb(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
            double m = v - c;
            double r, g, b;
            if      (h < 1.0 / 6) { r = c; g = x; b = 0; }
            else if (h < 2.0 / 6) { r = x; g = c; b = 0; }
            else if (h < 3.0 / 6) { r = 0; g = c; b = x; }
            else if (h < 4.0 / 6) { r = 0; g = x; b = c; }
            else if (h < 5.0 / 6) { r = x; g = 0; b = c; }
            else                  { r = c; g = 0; b = x; }
            return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }

        // BASE EDGE CHANGE ======================================================

        public void BaseEdgeToUpdated()
        {
            if (!isFirstPainted) { PaintGraph(); return; }
            if (animationInProgress) return;

            IVertex newBaseTo = Vertex.Get(false, @"BaseEdge:\To:");

            TransitionStyle3DEnum style = TransitionStyle3DEnumHelper.GetEnum(Vertex.Get(false, "TransitionStyle:"));

            // Only BaseEdge:\To change where the new target is already on the scene
            // triggers an animated transition. Everything else is a plain rebuild.
            bool targetOnScene = newBaseTo != null
                && DisplayedVerticesUIElements.ContainsKey(newBaseTo)
                && newBaseTo != previousBaseEdgeTo;

            if (!targetOnScene || style == TransitionStyle3DEnum.Cut)
            {
                PaintGraph();
                return;
            }

            VertexNode3D targetNode = DisplayedVerticesUIElements[newBaseTo];
            int durationMs = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "TransitionDurationMs:"));
            if (durationMs <= 0) durationMs = 600;

            MinusZero.Instance.Log(1, "GraphVisualiser3D.BaseEdgeToUpdated",
                "transition=" + style + " duration_ms=" + durationMs);

            switch (style)
            {
                case TransitionStyle3DEnum.OrbitTransition:
                    RunOrbitTransition(targetNode, durationMs);
                    break;

                case TransitionStyle3DEnum.FlyToAndSwap:
                    RunFlyToAndSwapTransition(targetNode, durationMs);
                    break;

                case TransitionStyle3DEnum.HyperspaceJump:
                    RunHyperspaceJumpTransition(durationMs);
                    break;

                case TransitionStyle3DEnum.GravityMorph:
                    // v1 placeholder: reuse OrbitTransition visuals.
                    RunOrbitTransition(targetNode, durationMs);
                    break;

                default:
                    PaintGraph();
                    break;
            }
        }

        private void RunOrbitTransition(VertexNode3D target, int durationMs)
        {
            Vector3D toTarget = (Vector3D)target.Position;
            if (toTarget.Length < 0.001) { PaintGraph(); return; }

            // Rotation axis perpendicular to (origin -> target) and the Y axis.
            Vector3D axis = Vector3D.CrossProduct(new Vector3D(0, 1, 0), toTarget);
            if (axis.Length < 0.001) axis = new Vector3D(1, 0, 0);
            axis.Normalize();
            sceneRotation.Axis = axis;

            double targetAngle = 90; // quarter turn, enough to sell the motion

            DoubleAnimation rotate = new DoubleAnimation(0, targetAngle, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animationInProgress = true;
            rotate.Completed += (s, e) =>
            {
                animationInProgress = false;
                PaintGraph();
            };

            sceneRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotate);
        }

        private void RunFlyToAndSwapTransition(VertexNode3D target, int durationMs)
        {
            // Phase 1: camera flies toward the target.
            Point3D currentCamera = camera.Position;
            Point3D approach = target.Position + (currentCamera - target.Position) * 0.25;

            Point3DAnimation flyIn = new Point3DAnimation(currentCamera, approach,
                TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            animationInProgress = true;

            fadeOut.Completed += (s, e) =>
            {
                // Phase 2: rebuild with new BaseEdge, reset opacity, fly back to default.
                PaintGraph();
                this.Opacity = 1;

                Point3D defaultPos = CurrentCameraPositionFromAngles();
                Point3DAnimation flyOut = new Point3DAnimation(camera.Position, defaultPos,
                    TimeSpan.FromMilliseconds(durationMs / 2))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                flyOut.Completed += (s2, e2) => { animationInProgress = false; };
                camera.BeginAnimation(ProjectionCamera.PositionProperty, flyOut);
            };

            camera.BeginAnimation(ProjectionCamera.PositionProperty, flyIn);
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void RunHyperspaceJumpTransition(int durationMs)
        {
            DoubleAnimation scaleDown = new DoubleAnimation(1, 0.01, TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseIn }
            };

            animationInProgress = true;
            scaleDown.Completed += (s, e) =>
            {
                PaintGraph();

                DoubleAnimation scaleUp = new DoubleAnimation(0.01, 1, TimeSpan.FromMilliseconds(durationMs / 2))
                {
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                };
                scaleUp.Completed += (s2, e2) => { animationInProgress = false; };

                // Need to clear the previous animation hold before starting a new one.
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, null);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, null);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, null);

                sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleUp);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleUp);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleUp);
            };

            sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleDown);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleDown);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleDown);
        }

        // CAMERA =================================================================

        private Point3D CurrentCameraPositionFromAngles()
        {
            double x = cameraDistance * Math.Cos(cameraPitch) * Math.Sin(cameraYaw);
            double y = cameraDistance * Math.Sin(cameraPitch);
            double z = cameraDistance * Math.Cos(cameraPitch) * Math.Cos(cameraYaw);
            return new Point3D(x, y, z);
        }

        private void UpdateCamera()
        {
            Point3D pos = CurrentCameraPositionFromAngles();
            camera.Position = pos;
            camera.LookDirection = new Vector3D(-pos.X, -pos.Y, -pos.Z);
            camera.UpDirection = new Vector3D(0, 1, 0);
        }

        public void ScaleChange()
        {
            double scale = ((double)(GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:")) ?? 100)) / 100.0;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        // MOUSE HANDLING =========================================================

        private Point dragStart;
        private bool isDragging;
        private double yawAtDragStart;
        private double pitchAtDragStart;

        private void Host_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // On double-click we delegate to OnMouseDown below, single-click performs
            // selection or starts a camera drag depending on what was hit.
            dragStart = e.GetPosition(this);
            yawAtDragStart = cameraYaw;
            pitchAtDragStart = cameraPitch;
            isDragging = true;
            this.CaptureMouse();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            isDragging = false;
            base.OnPreviewMouseLeftButtonUp(e);
        }

        private void Host_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);
                double dx = p.X - dragStart.X;
                double dy = p.Y - dragStart.Y;

                cameraYaw = yawAtDragStart - dx * 0.005;
                cameraPitch = Math.Max(-1.4, Math.Min(1.4, pitchAtDragStart + dy * 0.005));

                UpdateCamera();
                return;
            }

            // Hover highlight.
            VertexNode3D hit = HitTestVertexAt(e.GetPosition(viewport));
            if (hit != null && !hit.IsHighlighted)
            {
                if (highlighted != null) highlighted.UnhighlightThisAndDescendants();
                hit.HighlightThisAndDescendants();
                highlighted = hit;
            }
        }

        private void Host_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 0.9 : 1.1;
            cameraDistance = Math.Max(50, Math.Min(20000, cameraDistance * factor));
            UpdateCamera();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(viewport);
            VertexNode3D node = HitTestVertexAt(p);

            if (node == null) { base.OnMouseDown(e); return; }

            if (e.ClickCount == 2)
            {
                RestoreSelectedVertices();
                if (node.BaseVertex != null)
                    GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", node.BaseVertex);
                e.Handled = true;
                base.OnMouseDown(e);
                return;
            }

            if (e.ClickCount == 1)
            {
                Interaction.BeginInteractionWithGraph();

                CopySelectedVerticesToTemp();

                bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                IVertex sv = Vertex.Get(false, "SelectedEdges:");

                if (isCtrl)
                {
                    if (node.IsSelected)
                    {
                        node.Unselect();
                        EdgeHelper.DeleteVertexByEdgeTo(sv, node.BaseVertex);
                    }
                    else
                    {
                        node.Select();
                        EdgeHelper.AddEdgeVertexByToVertex(sv, node.BaseVertex);
                    }
                }
                else
                {
                    UnselectAllSelected();
                    GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);
                    node.Select();
                    EdgeHelper.AddEdgeVertexByToVertex(sv, node.BaseVertex);
                }

                Interaction.EndInteractionWithGraph();

                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        // Ray-casts into the 3D scene and returns the VertexNode3D that owns the
        // hit geometry, if any.
        private VertexNode3D HitTestVertexAt(Point p)
        {
            if (viewport == null) return null;

            VertexNode3D result = null;

            HitTestResultCallback callback = htr =>
            {
                RayHitTestResult rhtr = htr as RayHitTestResult;
                if (rhtr == null) return HitTestResultBehavior.Continue;

                Model3D model = rhtr.ModelHit;
                if (model != null && modelToNode.TryGetValue(model, out VertexNode3D node))
                {
                    result = node;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            };

            VisualTreeHelper.HitTest(viewport, null, callback, new PointHitTestParameters(p));

            return result;
        }

        // SELECTION =============================================================

        private void UnselectAllSelected()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge v in sv)
            {
                IVertex target = v.To.Get(false, "To:");
                if (target != null && DisplayedVerticesUIElements.ContainsKey(target))
                    DisplayedVerticesUIElements[target].Unselect();
            }
        }

        private IVertex tempSelectedVertices;

        private void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();
            GraphUtil.CopyShallow(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        private void RestoreSelectedVertices()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");
            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);
                GraphUtil.CopyShallow(tempSelectedVertices, sv);
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(tempSelectedVertices);
            }
        }

        private void UnselectAll()
        {
            foreach (VertexNode3D node in DisplayedVerticesUIElements.Values)
                node.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            Interaction.BeginInteractionWithGraph();

            IVertex sv = Vertex.Get(false, "SelectedEdges:");
            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            Interaction.EndInteractionWithGraph();
        }

        public void SelectedVerticesUpdated()
        {
            if (isFirstPainted)
            {
                UnselectAll();
                SelectWrappersForSelectedVertices();
            }

            SelectedEdgesChange?.Invoke();
        }

        private void SelectWrappersForSelectedVertices()
        {
            IVertex sv = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge e in sv)
            {
                IVertex target = e.To.Get(false, "To:");
                if (target != null && DisplayedVerticesUIElements.ContainsKey(target))
                    DisplayedVerticesUIElements[target].Select();
            }
        }

        // IHasLocalizableEdges ==================================================

        public IVertex GetEdgeByPoint(Point p)
        {
            VertexNode3D hit = HitTestVertexAt(p);

            if (hit != null && hit.BaseVertex != null)
            {
                IVertex v = MinusZero.Instance.CreateTempVertex();
                EdgeHelper.AddEdgeVertexEdgesOnlyTo(v, hit.BaseVertex);
                return v;
            }

            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false,
                @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                return Vertex.Get(false, @"BaseEdge:");

            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement) { throw new NotImplementedException(); }
        public FrameworkElement GetVisualElementByEdge(IVertex vertex)        { throw new NotImplementedException(); }

        // DEFAULTS ==============================================================

        protected void SetVertexDefaultValues()
        {
            Vertex.Get(false, "Scale:").Value = 100;
            Vertex.Get(false, "VisualiserCircleSize:").Value = 200;
            Vertex.Get(false, "NumberOfCircles:").Value = 2;
            Vertex.Get(false, "FastMode:").Value = "True";
            Vertex.Get(false, "MetaLabels:").Value = "True";
            Vertex.Get(false, "ShowOutEdges:").Value = "True";
            Vertex.Get(false, "ShowInEdges:").Value = "False";

            Vertex.Get(false, "LayoutMode3D:").Value = "FibonacciSphereShells";
            Vertex.Get(false, "TransitionStyle:").Value = "OrbitTransition";
            Vertex.Get(false, "TransitionDurationMs:").Value = 600;
            Vertex.Get(false, "SphereSize:").Value = 24;
            Vertex.Get(false, "ShowLabels3D:").Value = "True";
        }

        // THEME HELPERS =========================================================

        public Brush GetThemeBrush(string key, Brush fallback)
        {
            object resource = TryFindResource(key) ?? Application.Current?.TryFindResource(key);
            if (resource is Brush brush) return brush;
            return fallback;
        }

        public Color GetThemeColor(string key, Color fallback)
        {
            Brush b = GetThemeBrush(key, null);
            if (b is SolidColorBrush scb) return scb.Color;
            return fallback;
        }

        // VERTEX / DISPOSE =======================================================

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            foreach (VertexNode3D node in DisplayedVerticesUIElements.Values.Distinct())
                node.Dispose();
            DisplayedVerticesUIElements.Clear();
            modelToNode.Clear();
            edgeLines.Clear();

            VisualiserHelper.Dispose();
        }
    }
}

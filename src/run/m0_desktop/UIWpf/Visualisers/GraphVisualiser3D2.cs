using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace m0.UIWpf.Visualisers
{
    internal class GraphVisualiser3D2EdgeTag
    {
        public GraphVisualiser3D2Node FromNode;
        public GraphVisualiser3D2Node ToNode;
        public IVertex MetaVertex;
    }

    internal class GraphVisualiser3D2Node : ModelVisual3D, IDisposable
    {
        public IVertex BaseVertex;
        public Point3D Position;
        public List<GraphVisualiser3D2Edge> Edges = new List<GraphVisualiser3D2Edge>();

        private readonly GraphVisualiser3D2 parentVisualiser;
        private readonly TranslateTransform3D translate;
        private readonly ScaleTransform3D scale;
        private readonly DiffuseMaterial bodyMaterial;
        private readonly EmissiveMaterial bodyEmissive;
        private readonly TextBlock labelText;
        private IEdge listenerEdge;
        private bool isDisposed;

        public GeometryModel3D BodyModel { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsHighlighted { get; private set; }

        public GraphVisualiser3D2Node(IVertex baseVertex, GraphVisualiser3D2 parent, double sphereSize, bool showLabel)
        {
            BaseVertex = baseVertex;
            parentVisualiser = parent;

            translate = new TranslateTransform3D();
            scale = new ScaleTransform3D(sphereSize, sphereSize, sphereSize);

            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(scale);
            transform.Children.Add(translate);
            Transform = transform;

            bodyMaterial = new DiffuseMaterial(new SolidColorBrush(parentVisualiser.GetThemeColor("0LightGrayBrush", Colors.LightGray)));
            bodyEmissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black));

            MaterialGroup material = new MaterialGroup();
            material.Children.Add(bodyMaterial);
            material.Children.Add(bodyEmissive);

            BodyModel = new GeometryModel3D(GraphVisualiser3D2MeshFactory.UnitSphere, material);
            BodyModel.BackMaterial = bodyMaterial;

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(BodyModel);
            Content = modelGroup;

            if (showLabel)
            {
                labelText = CreateLabelText();
                Children.Add(CreateLabelVisual(labelText));
            }

            ApplyBodyColors();
            RegisterGraphListener();
        }

        private TextBlock CreateLabelText()
        {
            return new TextBlock
            {
                Text = BaseVertex != null && BaseVertex.Value != null ? BaseVertex.Value.ToString() : "Ø",
                Foreground = new SolidColorBrush(parentVisualiser.GetThemeColor("0ForegroundBrush", Colors.White)),
                Background = new SolidColorBrush(parentVisualiser.GetThemeColor("0BackgroundBrush", Color.FromArgb(220, 0, 0, 0))),
                Padding = new Thickness(4, 1, 4, 1),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static Viewport2DVisual3D CreateLabelVisual(FrameworkElement label)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(-2.5, 1.35, 0));
            mesh.Positions.Add(new Point3D(2.5, 1.35, 0));
            mesh.Positions.Add(new Point3D(2.5, 2.45, 0));
            mesh.Positions.Add(new Point3D(-2.5, 2.45, 0));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            DiffuseMaterial material = new DiffuseMaterial { Brush = Brushes.White };
            Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);

            return new Viewport2DVisual3D
            {
                Geometry = mesh,
                Material = material,
                Visual = label
            };
        }

        private void RegisterGraphListener()
        {
            if (BaseVertex == null) return;

            listenerEdge = ExecutionFlowHelper.AddTriggerAndListener(BaseVertex,
                new List<string> { },
                new List<GraphChangeFilterEnum> {
                    GraphChangeFilterEnum.ValueChange,
                    GraphChangeFilterEnum.OutputEdgeAdded,
                    GraphChangeFilterEnum.OutputEdgeRemoved,
                    GraphChangeFilterEnum.OutputEdgeDisposed },
                "GraphVisualiser3D2Node",
                VertexChange);
        }

        private INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (BaseVertex == null || BaseVertex.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

            if (labelText != null && BaseVertex.Value != null)
                labelText.Text = BaseVertex.Value.ToString();

            parentVisualiser.RequestRepaint();
            return exe.Stack;
        }

        public void SetWorldPosition(Point3D position)
        {
            Position = position;
            translate.OffsetX = position.X;
            translate.OffsetY = position.Y;
            translate.OffsetZ = position.Z;
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

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
            ApplyBodyColors();
        }

        private void ApplyBodyColors()
        {
            Color color;
            Color emissive;

            if (IsSelected)
            {
                color = parentVisualiser.GetThemeColor("0SelectionBrush", Colors.DodgerBlue);
                emissive = Darken(color, 0.35);
            }
            else if (IsHighlighted)
            {
                color = parentVisualiser.GetThemeColor("0HighlightBrush", Colors.OrangeRed);
                emissive = Darken(color, 0.50);
            }
            else
            {
                color = parentVisualiser.GetThemeColor("0LightGrayBrush", Colors.LightGray);
                emissive = Colors.Black;
            }

            bodyMaterial.Brush = new SolidColorBrush(color);
            bodyEmissive.Brush = new SolidColorBrush(emissive);
        }

        private static Color Darken(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)(color.R * factor),
                (byte)(color.G * factor),
                (byte)(color.B * factor));
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            if (listenerEdge != null)
                GraphChangeTrigger.RemoveListener(listenerEdge);
        }
    }

    internal class GraphVisualiser3D2Edge : ModelVisual3D
    {
        public GraphVisualiser3D2EdgeTag Tag;

        private readonly GeometryModel3D trunk;
        private readonly GeometryModel3D tip;
        private readonly DiffuseMaterial trunkMaterial;
        private readonly DiffuseMaterial tipMaterial;
        private readonly Transform3DGroup trunkTransform = new Transform3DGroup();
        private readonly Transform3DGroup tipTransform = new Transform3DGroup();

        public GraphVisualiser3D2Edge(Color color)
        {
            trunkMaterial = new DiffuseMaterial(new SolidColorBrush(color));
            tipMaterial = new DiffuseMaterial(new SolidColorBrush(color));

            trunk = new GeometryModel3D(GraphVisualiser3D2MeshFactory.UnitCylinder, trunkMaterial);
            tip = new GeometryModel3D(GraphVisualiser3D2MeshFactory.UnitCone, tipMaterial);

            trunk.Transform = trunkTransform;
            tip.Transform = tipTransform;

            Model3DGroup group = new Model3DGroup();
            group.Children.Add(trunk);
            group.Children.Add(tip);
            Content = group;
        }

        public void SetHighlighted(bool highlighted, GraphVisualiser3D2 parentVisualiser)
        {
            Color color = highlighted
                ? parentVisualiser.GetThemeColor("0LightHighlightBrush", Colors.Gold)
                : parentVisualiser.HashToColor(Tag != null ? Tag.MetaVertex : null);

            trunkMaterial.Brush = new SolidColorBrush(color);
            tipMaterial.Brush = new SolidColorBrush(color);
        }

        public void UpdateGeometry(double sphereSize)
        {
            if (Tag == null || Tag.FromNode == null || Tag.ToNode == null) return;

            Vector3D direction = Tag.ToNode.Position - Tag.FromNode.Position;
            double length = direction.Length;
            if (length < 0.001) return;

            Vector3D unit = direction / length;
            Point3D from = Tag.FromNode.Position + unit * sphereSize;
            Point3D to = Tag.ToNode.Position - unit * sphereSize;

            Vector3D trimmedDirection = to - from;
            double trimmedLength = trimmedDirection.Length;
            if (trimmedLength < 0.001) return;

            Vector3D trimmedUnit = trimmedDirection / trimmedLength;
            double tipLength = Math.Min(sphereSize * 0.85, trimmedLength * 0.35);
            double trunkLength = Math.Max(0.001, trimmedLength - tipLength);

            trunkTransform.Children.Clear();
            trunkTransform.Children.Add(new ScaleTransform3D(Math.Max(1, sphereSize * 0.07), trunkLength, Math.Max(1, sphereSize * 0.07)));
            trunkTransform.Children.Add(AlignYToDirection(trimmedUnit));
            trunkTransform.Children.Add(new TranslateTransform3D(from.X, from.Y, from.Z));

            Point3D tipStart = from + trimmedUnit * trunkLength;

            tipTransform.Children.Clear();
            tipTransform.Children.Add(new ScaleTransform3D(sphereSize * 0.25, tipLength, sphereSize * 0.25));
            tipTransform.Children.Add(AlignYToDirection(trimmedUnit));
            tipTransform.Children.Add(new TranslateTransform3D(tipStart.X, tipStart.Y, tipStart.Z));
        }

        private static Transform3D AlignYToDirection(Vector3D direction)
        {
            Vector3D y = new Vector3D(0, 1, 0);
            double dot = Vector3D.DotProduct(y, direction);

            if (dot >= 0.9999)
                return Transform3D.Identity;

            if (dot <= -0.9999)
                return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180));

            Vector3D axis = Vector3D.CrossProduct(y, direction);
            axis.Normalize();
            double angle = Math.Acos(dot) * 180.0 / Math.PI;
            return new RotateTransform3D(new AxisAngleRotation3D(axis, angle));
        }
    }

    internal static class GraphVisualiser3D2MeshFactory
    {
        public static readonly MeshGeometry3D UnitSphere = BuildSphere(16, 12);
        public static readonly MeshGeometry3D UnitCylinder = BuildCylinder(18);
        public static readonly MeshGeometry3D UnitCone = BuildCone(18);

        static GraphVisualiser3D2MeshFactory()
        {
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
                double angle = i * 2 * Math.PI / segments;
                double x = Math.Cos(angle);
                double z = Math.Sin(angle);

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
                double angle = i * 2 * Math.PI / segments;
                double x = Math.Cos(angle);
                double z = Math.Sin(angle);

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

    public class GraphVisualiser3D2 : Grid, IListVisualiser, IHasSelectableEdges, ITypedEdge
    {
        private class FragmentEdge
        {
            public IEdge Edge;
            public IVertex From;
            public IVertex To;
            public bool IsOutgoingFromFrontier;
        }

        private struct SemanticGroupKey : IEquatable<SemanticGroupKey>
        {
            public IVertex Meta;
            public bool IsOutgoing;

            public bool Equals(SemanticGroupKey other)
            {
                return ReferenceEquals(Meta, other.Meta) && IsOutgoing == other.IsOutgoing;
            }

            public override bool Equals(object obj)
            {
                return obj is SemanticGroupKey && Equals((SemanticGroupKey)obj);
            }

            public override int GetHashCode()
            {
                int metaHash = Meta != null ? Meta.GetHashCode() : 0;
                return (metaHash * 397) ^ IsOutgoing.GetHashCode();
            }
        }

        public event Notify SelectedEdgesChange;
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        private readonly Dictionary<IVertex, GraphVisualiser3D2Node> displayedNodes = new Dictionary<IVertex, GraphVisualiser3D2Node>();
        private readonly Dictionary<Model3D, GraphVisualiser3D2Node> modelToNode = new Dictionary<Model3D, GraphVisualiser3D2Node>();
        private readonly List<GraphVisualiser3D2Edge> displayedEdges = new List<GraphVisualiser3D2Edge>();
        private readonly Dictionary<SemanticGroupKey, int> semanticGroupIndexes = new Dictionary<SemanticGroupKey, int>();
        private readonly HashSet<string> displayedEdgeKeys = new HashSet<string>();

        private Viewport3D viewport;
        private PerspectiveCamera camera;
        private ModelVisual3D sceneRoot;
        private AxisAngleRotation3D sceneRotation;
        private ScaleTransform3D sceneScale;

        private GraphVisualiser3D2Node highlightedNode;
        private bool isPainting;
        private bool isFirstPainted;

        private double cameraYaw = 0;
        private double cameraPitch = 0.45;
        private double cameraDistance = 900;
        private Point dragStart;
        private bool isDragging;
        private double yawAtDragStart;
        private double pitchAtDragStart;

        private static readonly string[] _MetaTriggeringUpdateVertex = new string[] {
            "VisualiserCircleSize", "NumberOfCircles", "ShowOutEdges", "ShowInEdges",
            "SphereSize", "ShowLabels3D"
        };

        private static readonly string[] _MetaTriggeringUpdateView = new string[] { };

        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }
        public void ViewAttributesUpdated() { }

        public GraphVisualiser3D2(IEdge edge)
        {
            Edge = edge;
            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }

        public GraphVisualiser3D2(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            SetupHost();

            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Graph3D2"),
                this,
                "GraphVisualiser3D2",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            PreviewMouseLeftButtonDown += HostPreviewMouseLeftButtonDown;
            PreviewMouseMove += HostPreviewMouseMove;
            MouseWheel += HostMouseWheel;

            SetVertexDefaultValues();
        }        

        private void SetupHost()
        {
            Brush background = TryFindResource("0BackgroundBrush") as Brush;
            Background = background ?? new SolidColorBrush(Color.FromRgb(24, 24, 28));
            ClipToBounds = true;

            viewport = new Viewport3D();
            Children.Add(viewport);

            camera = new PerspectiveCamera
            {
                FieldOfView = 60,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 100000
            };
            viewport.Camera = camera;

            sceneScale = new ScaleTransform3D(1, 1, 1);
            sceneRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

            Transform3DGroup sceneTransform = new Transform3DGroup();
            sceneTransform.Children.Add(sceneScale);
            sceneTransform.Children.Add(new RotateTransform3D(sceneRotation));

            sceneRoot = new ModelVisual3D { Transform = sceneTransform };
            viewport.Children.Add(sceneRoot);

            Model3DGroup lights = new Model3DGroup();
            lights.Children.Add(new AmbientLight(Color.FromRgb(85, 85, 95)));
            lights.Children.Add(new DirectionalLight(Color.FromRgb(210, 210, 220), new Vector3D(-1, -1, -1)));
            lights.Children.Add(new DirectionalLight(Color.FromRgb(90, 90, 110), new Vector3D(1, 0.4, 1)));
            viewport.Children.Add(new ModelVisual3D { Content = lights });

            UpdateCamera();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
            PaintGraph();

            if (isFirstPainted)
                Loaded -= OnLoad;
        }

        public void RequestRepaint()
        {
            if (!isPainting)
                PaintGraph();
        }

        public void BaseEdgeToUpdated()
        {
            PaintGraph();
        }

        private void PaintGraph()
        {
            if (Vertex == null || Vertex.DisposedState != DisposeStateEnum.Live) return;
            if (ActualWidth == 0 || ActualHeight == 0) return;

            Stopwatch sw = Stopwatch.StartNew();
            isPainting = true;

            ClearScene();

            IVertex baseTo = Vertex.Get(false, @"BaseEdge:\To:");
            if (baseTo != null)
            {
                BuildSemanticNucleus(baseTo);
                SelectWrappersForSelectedVertices();
            }

            isFirstPainted = true;
            isPainting = false;

            sw.Stop();
            MinusZero.Instance.Log(1, "GraphVisualiser3D2.PaintGraph",
                "vertices=" + displayedNodes.Count + " edges=" + displayedEdges.Count +
                " groups=" + semanticGroupIndexes.Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        private void ClearScene()
        {
            foreach (GraphVisualiser3D2Node node in displayedNodes.Values.Distinct())
                node.Dispose();

            displayedNodes.Clear();
            modelToNode.Clear();
            displayedEdges.Clear();
            semanticGroupIndexes.Clear();
            displayedEdgeKeys.Clear();
            highlightedNode = null;

            List<Visual3D> toRemove = new List<Visual3D>();
            foreach (Visual3D child in sceneRoot.Children)
                toRemove.Add(child);
            foreach (Visual3D child in toRemove)
                sceneRoot.Children.Remove(child);

            sceneRotation.Angle = 0;
            sceneScale.ScaleX = sceneScale.ScaleY = sceneScale.ScaleZ = 1;
        }

        private void BuildSemanticNucleus(IVertex baseTo)
        {
            int depth = GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")) ?? 2;
            int radiusStep = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "VisualiserCircleSize:"));
            if (radiusStep <= 0) radiusStep = 220;

            bool showOutEdges = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowOutEdges:"), "True");
            bool showInEdges = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowInEdges:"), "True");

            AddNode(baseTo, new Point3D(0, 0, 0));

            List<IVertex> frontier = new List<IVertex> { baseTo };

            for (int level = 1; level <= depth; level++)
            {
                List<FragmentEdge> newEdges = CollectNewLevelEdges(frontier, showOutEdges, showInEdges);
                if (newEdges.Count == 0) break;

                List<IVertex> nextFrontier = PlaceSemanticLevel(newEdges, level, radiusStep);
                ConnectDisplayedEdges(frontier, showOutEdges, showInEdges);
                frontier = nextFrontier;
            }

            ConnectDisplayedEdges(displayedNodes.Keys.ToList(), showOutEdges, showInEdges);

            foreach (GraphVisualiser3D2Edge edge in displayedEdges)
                edge.UpdateGeometry(GetSphereSize());
        }

        private List<FragmentEdge> CollectNewLevelEdges(List<IVertex> frontier, bool showOutEdges, bool showInEdges)
        {
            List<FragmentEdge> result = new List<FragmentEdge>();
            HashSet<IVertex> plannedTargets = new HashSet<IVertex>();

            foreach (IVertex vertex in frontier)
            {
                if (showOutEdges)
                    foreach (IEdge edge in vertex)
                        AddCandidate(result, plannedTargets, edge, vertex, edge.To, true);

                if (showInEdges)
                    foreach (IEdge edge in vertex.InEdges.ToList())
                        AddCandidate(result, plannedTargets, edge, edge.From, vertex, false);
            }

            return result;
        }

        private void AddCandidate(List<FragmentEdge> result, HashSet<IVertex> plannedTargets,
            IEdge edge, IVertex from, IVertex to, bool outgoingFromFrontier)
        {
            if (!CanAddEdge(edge)) return;

            IVertex newVertex = outgoingFromFrontier ? to : from;
            if (newVertex == null) return;
            if (displayedNodes.ContainsKey(newVertex)) return;
            if (!plannedTargets.Add(newVertex)) return;

            result.Add(new FragmentEdge
            {
                Edge = edge,
                From = from,
                To = to,
                IsOutgoingFromFrontier = outgoingFromFrontier
            });
        }

        private List<IVertex> PlaceSemanticLevel(List<FragmentEdge> edges, int level, int radiusStep)
        {
            List<IVertex> nextFrontier = new List<IVertex>();
            var groups = edges.GroupBy(e => new SemanticGroupKey { Meta = e.Edge.Meta, IsOutgoing = e.IsOutgoingFromFrontier });

            foreach (IGrouping<SemanticGroupKey, FragmentEdge> group in groups)
            {
                List<FragmentEdge> groupEdges = group.ToList();
                Vector3D direction = GetSemanticDirection(group.Key);
                Vector3D tangentA;
                Vector3D tangentB;
                GetPerpendicularBasis(direction, out tangentA, out tangentB);

                double branchRadius = Math.Min(radiusStep * 0.45, 35 + groupEdges.Count * 8);

                for (int i = 0; i < groupEdges.Count; i++)
                {
                    FragmentEdge fragmentEdge = groupEdges[i];
                    IVertex newVertex = fragmentEdge.IsOutgoingFromFrontier ? fragmentEdge.To : fragmentEdge.From;
                    GraphVisualiser3D2Node anchor = GetAnchorNode(fragmentEdge);
                    if (anchor == null) continue;

                    double angle = groupEdges.Count == 1 ? 0 : i * Math.PI * 2 / groupEdges.Count;
                    Vector3D ringOffset = tangentA * Math.Cos(angle) * branchRadius + tangentB * Math.Sin(angle) * branchRadius;
                    Vector3D levelPush = direction * (radiusStep * (0.75 + level * 0.15));
                    Point3D position = anchor.Position + levelPush + ringOffset;

                    AddNode(newVertex, position);
                    nextFrontier.Add(newVertex);

                    AddEdgeOnce(fragmentEdge.From, fragmentEdge.To, fragmentEdge.Edge.Meta);
                }
            }

            return nextFrontier;
        }

        private GraphVisualiser3D2Node GetAnchorNode(FragmentEdge edge)
        {
            IVertex anchorVertex = edge.IsOutgoingFromFrontier ? edge.From : edge.To;
            GraphVisualiser3D2Node anchor;
            return displayedNodes.TryGetValue(anchorVertex, out anchor) ? anchor : null;
        }

        private Vector3D GetSemanticDirection(SemanticGroupKey key)
        {
            int index;
            if (!semanticGroupIndexes.TryGetValue(key, out index))
            {
                index = semanticGroupIndexes.Count;
                semanticGroupIndexes[key] = index;
            }

            double goldenAngle = Math.PI * (3 - Math.Sqrt(5));
            double y = 1.0 - 2.0 * ((index + 0.5) / Math.Max(1.0, semanticGroupIndexes.Count + 1.0));
            double radius = Math.Sqrt(Math.Max(0, 1 - y * y));
            double angle = index * goldenAngle;

            Vector3D direction = new Vector3D(Math.Cos(angle) * radius, y, Math.Sin(angle) * radius);
            if (!key.IsOutgoing)
                direction *= -1;

            if (direction.Length < 0.001)
                direction = key.IsOutgoing ? new Vector3D(1, 0, 0) : new Vector3D(-1, 0, 0);

            direction.Normalize();
            return direction;
        }

        private static void GetPerpendicularBasis(Vector3D direction, out Vector3D tangentA, out Vector3D tangentB)
        {
            Vector3D up = Math.Abs(Vector3D.DotProduct(direction, new Vector3D(0, 1, 0))) > 0.9
                ? new Vector3D(1, 0, 0)
                : new Vector3D(0, 1, 0);

            tangentA = Vector3D.CrossProduct(direction, up);
            tangentA.Normalize();

            tangentB = Vector3D.CrossProduct(direction, tangentA);
            tangentB.Normalize();
        }

        private void ConnectDisplayedEdges(IEnumerable<IVertex> vertices, bool showOutEdges, bool showInEdges)
        {
            foreach (IVertex vertex in vertices)
            {
                if (!displayedNodes.ContainsKey(vertex))
                    continue;

                if (showOutEdges)
                    foreach (IEdge edge in vertex)
                        if (CanAddEdge(edge) && displayedNodes.ContainsKey(edge.To))
                            AddEdgeOnce(vertex, edge.To, edge.Meta);

                if (showInEdges)
                    foreach (IEdge edge in vertex.InEdges.ToList())
                        if (CanAddEdge(edge) && displayedNodes.ContainsKey(edge.From))
                            AddEdgeOnce(edge.From, vertex, edge.Meta);
            }
        }

        private GraphVisualiser3D2Node AddNode(IVertex vertex, Point3D position)
        {
            GraphVisualiser3D2Node existing;
            if (displayedNodes.TryGetValue(vertex, out existing))
                return existing;

            GraphVisualiser3D2Node node = new GraphVisualiser3D2Node(vertex, this, GetSphereSize(), GetShowLabels());
            node.SetWorldPosition(position);

            displayedNodes[vertex] = node;
            modelToNode[node.BodyModel] = node;
            sceneRoot.Children.Add(node);

            return node;
        }

        private void AddEdgeOnce(IVertex fromVertex, IVertex toVertex, IVertex meta)
        {
            if (fromVertex == null || toVertex == null || fromVertex == toVertex) return;

            GraphVisualiser3D2Node from;
            GraphVisualiser3D2Node to;
            if (!displayedNodes.TryGetValue(fromVertex, out from)) return;
            if (!displayedNodes.TryGetValue(toVertex, out to)) return;

            string edgeKey = fromVertex.GetHashCode() + "|" + toVertex.GetHashCode() + "|" + (meta != null ? meta.GetHashCode() : 0);
            if (!displayedEdgeKeys.Add(edgeKey)) return;

            GraphVisualiser3D2Edge edge = new GraphVisualiser3D2Edge(HashToColor(meta));
            edge.Tag = new GraphVisualiser3D2EdgeTag
            {
                FromNode = from,
                ToNode = to,
                MetaVertex = meta
            };
            edge.UpdateGeometry(GetSphereSize());

            from.Edges.Add(edge);
            to.Edges.Add(edge);
            displayedEdges.Add(edge);
            sceneRoot.Children.Add(edge);
        }

        private bool CanAddEdge(IEdge edge)
        {
            if (edge == null) return false;
            if (edge.Meta != null && GeneralUtil.CompareStrings(edge.Meta, "$GraphChangeTrigger")) return false;
            return true;
        }

        private double GetSphereSize()
        {
            double sphereSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "SphereSize:"));
            return sphereSize > 0 ? sphereSize : 24;
        }

        private bool GetShowLabels()
        {
            return GeneralUtil.CompareStrings(Vertex.Get(false, "ShowLabels3D:"), "True");
        }

        private void HostPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStart = e.GetPosition(this);
            yawAtDragStart = cameraYaw;
            pitchAtDragStart = cameraPitch;
            isDragging = true;
            CaptureMouse();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
            isDragging = false;
            base.OnPreviewMouseLeftButtonUp(e);
        }

        private void HostPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                cameraYaw = yawAtDragStart - (current.X - dragStart.X) * 0.005;
                cameraPitch = Math.Max(-1.4, Math.Min(1.4, pitchAtDragStart + (current.Y - dragStart.Y) * 0.005));
                UpdateCamera();
                return;
            }

            GraphVisualiser3D2Node hit = HitTestNodeAt(e.GetPosition(viewport));
            if (hit != highlightedNode)
            {
                ClearHighlight();
                if (hit != null)
                    HighlightNodeAndNeighborhood(hit);
            }
        }

        private void HostMouseWheel(object sender, MouseWheelEventArgs e)
        {
            cameraDistance = Math.Max(80, Math.Min(20000, cameraDistance * (e.Delta > 0 ? 0.9 : 1.1)));
            UpdateCamera();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            GraphVisualiser3D2Node node = HitTestNodeAt(e.GetPosition(viewport));
            if (node == null)
            {
                base.OnMouseDown(e);
                return;
            }

            if (e.ClickCount == 2)
            {
                GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", node.BaseVertex);
                BaseEdgeToUpdated();
                e.Handled = true;
                base.OnMouseDown(e);
                return;
            }

            if (e.ClickCount == 1)
            {
                UpdateSelectionFromClick(node);
                e.Handled = true;
            }

            base.OnMouseDown(e);
        }

        private void UpdateSelectionFromClick(GraphVisualiser3D2Node node)
        {
            Interaction.BeginInteractionWithGraph();

            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (!isCtrl)
            {
                UnselectAll();
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                node.Select();
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, node.BaseVertex);
            }
            else if (node.IsSelected)
            {
                node.Unselect();
                EdgeHelper.DeleteVertexByEdgeTo(selectedEdges, node.BaseVertex);
            }
            else
            {
                node.Select();
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, node.BaseVertex);
            }

            Interaction.EndInteractionWithGraph();
        }

        private void HighlightNodeAndNeighborhood(GraphVisualiser3D2Node node)
        {
            highlightedNode = node;
            node.SetHighlighted(true);

            foreach (GraphVisualiser3D2Edge edge in node.Edges)
            {
                edge.SetHighlighted(true, this);
                if (edge.Tag != null)
                {
                    if (edge.Tag.FromNode != null) edge.Tag.FromNode.SetHighlighted(true);
                    if (edge.Tag.ToNode != null) edge.Tag.ToNode.SetHighlighted(true);
                }
            }
        }

        private void ClearHighlight()
        {
            if (highlightedNode == null) return;

            foreach (GraphVisualiser3D2Node node in displayedNodes.Values)
                node.SetHighlighted(false);

            foreach (GraphVisualiser3D2Edge edge in displayedEdges)
                edge.SetHighlighted(false, this);

            highlightedNode = null;
        }

        private GraphVisualiser3D2Node HitTestNodeAt(Point point)
        {
            GraphVisualiser3D2Node result = null;

            HitTestResultCallback callback = hit =>
            {
                RayHitTestResult rayHit = hit as RayHitTestResult;
                if (rayHit == null) return HitTestResultBehavior.Continue;

                GraphVisualiser3D2Node node;
                if (rayHit.ModelHit != null && modelToNode.TryGetValue(rayHit.ModelHit, out node))
                {
                    result = node;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            };

            VisualTreeHelper.HitTest(viewport, null, callback, new PointHitTestParameters(point));
            return result;
        }

        private void UpdateCamera()
        {
            double x = cameraDistance * Math.Cos(cameraPitch) * Math.Sin(cameraYaw);
            double y = cameraDistance * Math.Sin(cameraPitch);
            double z = cameraDistance * Math.Cos(cameraPitch) * Math.Cos(cameraYaw);
            Point3D position = new Point3D(x, y, z);

            camera.Position = position;
            camera.LookDirection = new Vector3D(-position.X, -position.Y, -position.Z);
            camera.UpDirection = new Vector3D(0, 1, 0);
        }

        public void ScaleChange()
        {
            double scale = ((double)(GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:")) ?? 100)) / 100.0;
            LayoutTransform = scale != 1.0 ? new ScaleTransform(scale, scale) : null;
        }

        public void SelectedVerticesUpdated()
        {
            if (isFirstPainted)
            {
                UnselectAll();
                SelectWrappersForSelectedVertices();
            }

            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        private void SelectWrappersForSelectedVertices()
        {
            IVertex selected = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            foreach (IEdge edge in selected)
            {
                IVertex target = edge.To.Get(false, "To:");
                if (target != null && displayedNodes.ContainsKey(target))
                    displayedNodes[target].Select();
            }
        }

        private void UnselectAll()
        {
            foreach (GraphVisualiser3D2Node node in displayedNodes.Values)
                node.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            Interaction.BeginInteractionWithGraph();
            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(Vertex.Get(false, "SelectedEdges:"));
            UnselectAll();
            Interaction.EndInteractionWithGraph();
        }

        public IVertex GetEdgeByPoint(Point point)
        {
            GraphVisualiser3D2Node hit = HitTestNodeAt(point);
            if (hit != null && hit.BaseVertex != null)
            {
                IVertex vertex = MinusZero.Instance.CreateTempVertex();
                EdgeHelper.AddEdgeVertexEdgesOnlyTo(vertex, hit.BaseVertex);
                return vertex;
            }

            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false,
                @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                return Vertex.Get(false, @"BaseEdge:");

            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        protected void SetVertexDefaultValues()
        {
            Vertex.Get(false, "Scale:").Value = 100;
            Vertex.Get(false, "VisualiserCircleSize:").Value = 220;
            Vertex.Get(false, "NumberOfCircles:").Value = 2;
            Vertex.Get(false, "ShowOutEdges:").Value = "True";
            Vertex.Get(false, "ShowInEdges:").Value = "True";
            Vertex.Get(false, "SphereSize:").Value = 24;
            Vertex.Get(false, "ShowLabels3D:").Value = "True";
        }

        public Brush GetThemeBrush(string key, Brush fallback)
        {
            object resource = TryFindResource(key) ?? Application.Current?.TryFindResource(key);
            Brush brush = resource as Brush;
            return brush ?? fallback;
        }

        public Color GetThemeColor(string key, Color fallback)
        {
            SolidColorBrush brush = GetThemeBrush(key, null) as SolidColorBrush;
            return brush != null ? brush.Color : fallback;
        }

        public Color HashToColor(IVertex meta)
        {
            if (meta == null || meta.Value == null)
                return GetThemeColor("0LightGrayBrush", Colors.LightGray);

            int hash = meta.Value.ToString().GetHashCode();
            double hue = ((uint)hash % 360) / 360.0;
            return HsvToRgb(hue, 0.50, 0.90);
        }

        private static Color HsvToRgb(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
            double m = v - c;
            double r;
            double g;
            double b;

            if (h < 1.0 / 6) { r = c; g = x; b = 0; }
            else if (h < 2.0 / 6) { r = x; g = c; b = 0; }
            else if (h < 3.0 / 6) { r = 0; g = c; b = x; }
            else if (h < 4.0 / 6) { r = 0; g = x; b = c; }
            else if (h < 5.0 / 6) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255));
        }

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

            foreach (GraphVisualiser3D2Node node in displayedNodes.Values.Distinct())
                node.Dispose();

            displayedNodes.Clear();
            modelToNode.Clear();
            displayedEdges.Clear();

            VisualiserHelper.Dispose();
        }
    }
}

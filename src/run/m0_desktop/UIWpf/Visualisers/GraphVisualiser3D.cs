using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace m0.UIWpf.Visualisers
{
    internal sealed class GraphVisualiser3DLabel
    {
        public TextBlock Element;
        public Func<Point3D> GetWorldPosition;
        public int BaseZIndex;
        public bool IsEdgeLabel;
    }

    internal sealed class GraphVisualiser3DEdgeTag
    {
        public GraphVisualiser3DNode FromNode;
        public GraphVisualiser3DNode ToNode;
        public IEdge Edge;
        public TextBlock MetaLabel;
    }

    internal sealed class GraphVisualiser3DNode : ModelVisual3D, IDisposable
    {
        private readonly GraphVisualiser3D parentVisualiser;
        private readonly TranslateTransform3D translate;
        private readonly ScaleTransform3D scale;
        private readonly DiffuseMaterial bodyMaterial;
        private readonly EmissiveMaterial bodyEmissive;
        private IEdge listenerEdge;
        private bool isDisposed;

        public IVertex BaseVertex;
        public Point3D Position;
        public GeometryModel3D BodyModel;
        public readonly List<GraphVisualiser3DEdgeVisual> OutgoingEdges = new List<GraphVisualiser3DEdgeVisual>();
        public readonly List<GraphVisualiser3DEdgeVisual> IncidentEdges = new List<GraphVisualiser3DEdgeVisual>();
        public TextBlock Label;
        public bool IsSelected;
        public bool IsHighlighted;

        public GraphVisualiser3DNode(IVertex baseVertex, GraphVisualiser3D parent, double sphereSize)
        {
            BaseVertex = baseVertex;
            parentVisualiser = parent;

            translate = new TranslateTransform3D();
            scale = new ScaleTransform3D(sphereSize, sphereSize, sphereSize);

            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(scale);
            transform.Children.Add(translate);
            Transform = transform;

            bodyMaterial = new DiffuseMaterial(new SolidColorBrush(parent.GetThemeColor("0LightGrayBrush", Colors.LightGray)));
            bodyEmissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black));

            MaterialGroup material = new MaterialGroup();
            material.Children.Add(bodyMaterial);
            material.Children.Add(bodyEmissive);

            BodyModel = new GeometryModel3D(GraphVisualiser3DMeshFactory.UnitSphere, material);
            BodyModel.BackMaterial = material;
            Content = BodyModel;

            ApplyBodyColors();
            RegisterGraphListener();
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
                "GraphVisualiser3DNode",
                VertexChange);
        }

        private INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (BaseVertex == null || BaseVertex.DisposedState != DisposeStateEnum.Live)
                return exe.Stack;

            IVertex valueChange = exe.Stack.Get(false, @"event:\Type:ValueChange");
            if (valueChange != null)
                parentVisualiser.RequestLabelRefresh();
            else
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
            ApplyLabelState();
        }

        public void Unselect()
        {
            IsSelected = false;
            ApplyBodyColors();
            ApplyLabelState();
        }

        public void SetHighlighted(bool highlighted)
        {
            IsHighlighted = highlighted;
            ApplyBodyColors();
            ApplyLabelState();
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

        public void ApplyLabelState()
        {
            if (Label == null) return;

            if (IsSelected)
            {
                Label.Foreground = new SolidColorBrush(parentVisualiser.GetThemeColor("0BackgroundBrush", Colors.Black));
                Label.Background = new SolidColorBrush(parentVisualiser.GetThemeColor("0SelectionBrush", Colors.DodgerBlue));
                Panel.SetZIndex(Label, 9000);
            }
            else if (IsHighlighted)
            {
                Label.Foreground = new SolidColorBrush(parentVisualiser.GetThemeColor("0HighlightBrush", Colors.OrangeRed));
                Label.Background = parentVisualiser.GetLabelBackgroundBrush(230);
                Panel.SetZIndex(Label, 8000);
            }
            else
            {
                Label.Foreground = new SolidColorBrush(parentVisualiser.GetThemeColor("0ForegroundBrush", Colors.White));
                Label.Background = parentVisualiser.GetLabelBackgroundBrush(205);
                Panel.SetZIndex(Label, 1000);
            }
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

    internal sealed class GraphVisualiser3DEdgeVisual : ModelVisual3D
    {
        private readonly GraphVisualiser3D parentVisualiser;
        private readonly DiffuseMaterial shaftMaterial;
        private readonly EmissiveMaterial shaftEmissive;
        private readonly DiffuseMaterial headMaterial;
        private readonly EmissiveMaterial headEmissive;
        private readonly ScaleTransform3D shaftScale;
        private readonly RotateTransform3D shaftRotate;
        private readonly TranslateTransform3D shaftTranslate;
        private readonly ScaleTransform3D headScale;
        private readonly RotateTransform3D headRotate;
        private readonly TranslateTransform3D headTranslate;

        public GraphVisualiser3DNode FromNode;
        public GraphVisualiser3DNode ToNode;
        public IEdge Edge;
        public TextBlock MetaLabel;

        public GraphVisualiser3DEdgeVisual(GraphVisualiser3D parent)
        {
            parentVisualiser = parent;

            shaftMaterial = new DiffuseMaterial(new SolidColorBrush(parent.GetThemeColor("0LightGrayBrush", Colors.LightGray)));
            shaftEmissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black));
            headMaterial = new DiffuseMaterial(new SolidColorBrush(parent.GetThemeColor("0LightGrayBrush", Colors.LightGray)));
            headEmissive = new EmissiveMaterial(new SolidColorBrush(Colors.Black));

            MaterialGroup shaftGroup = new MaterialGroup();
            shaftGroup.Children.Add(shaftMaterial);
            shaftGroup.Children.Add(shaftEmissive);

            MaterialGroup headGroup = new MaterialGroup();
            headGroup.Children.Add(headMaterial);
            headGroup.Children.Add(headEmissive);

            shaftScale = new ScaleTransform3D();
            shaftRotate = new RotateTransform3D();
            shaftTranslate = new TranslateTransform3D();

            Transform3DGroup shaftTransform = new Transform3DGroup();
            shaftTransform.Children.Add(shaftScale);
            shaftTransform.Children.Add(shaftRotate);
            shaftTransform.Children.Add(shaftTranslate);

            headScale = new ScaleTransform3D();
            headRotate = new RotateTransform3D();
            headTranslate = new TranslateTransform3D();

            Transform3DGroup headTransform = new Transform3DGroup();
            headTransform.Children.Add(headScale);
            headTransform.Children.Add(headRotate);
            headTransform.Children.Add(headTranslate);

            Model3DGroup group = new Model3DGroup();
            group.Children.Add(new GeometryModel3D(GraphVisualiser3DMeshFactory.UnitCylinder, shaftGroup)
            {
                BackMaterial = shaftGroup,
                Transform = shaftTransform
            });
            group.Children.Add(new GeometryModel3D(GraphVisualiser3DMeshFactory.UnitCone, headGroup)
            {
                BackMaterial = headGroup,
                Transform = headTransform
            });

            Content = group;
        }

        public void Connect(GraphVisualiser3DNode fromNode, GraphVisualiser3DNode toNode, IEdge edge, double nodeRadius)
        {
            FromNode = fromNode;
            ToNode = toNode;
            Edge = edge;

            Vector3D delta = toNode.Position - fromNode.Position;
            double length = delta.Length;
            if (length < 0.001) return;

            Vector3D direction = delta;
            direction.Normalize();

            Point3D start = fromNode.Position + direction * (nodeRadius * 1.20);
            Point3D end = toNode.Position - direction * (nodeRadius * 1.35);
            Vector3D visibleDelta = end - start;
            double visibleLength = Math.Max(1, visibleDelta.Length);
            Point3D middle = start + visibleDelta * 0.5;

            Quaternion rotation = RotationFromYAxis(direction);
            QuaternionRotation3D shaftRotation = new QuaternionRotation3D(rotation);
            QuaternionRotation3D headRotation = new QuaternionRotation3D(rotation);

            shaftScale.ScaleX = parentVisualiser.EdgeRadius;
            shaftScale.ScaleY = visibleLength;
            shaftScale.ScaleZ = parentVisualiser.EdgeRadius;
            shaftRotate.Rotation = shaftRotation;
            shaftTranslate.OffsetX = middle.X;
            shaftTranslate.OffsetY = middle.Y;
            shaftTranslate.OffsetZ = middle.Z;

            headScale.ScaleX = parentVisualiser.ArrowRadius;
            headScale.ScaleY = parentVisualiser.ArrowLength;
            headScale.ScaleZ = parentVisualiser.ArrowRadius;
            headRotate.Rotation = headRotation;
            headTranslate.OffsetX = end.X;
            headTranslate.OffsetY = end.Y;
            headTranslate.OffsetZ = end.Z;
        }

        public Point3D GetLabelPosition()
        {
            if (FromNode == null || ToNode == null)
                return new Point3D();

            Point3D p = FromNode.Position + (ToNode.Position - FromNode.Position) * 0.56;
            return p;
        }

        public void SetHighlighted(bool highlighted)
        {
            Color color = highlighted
                ? parentVisualiser.GetThemeColor("0HighlightBrush", Colors.OrangeRed)
                : parentVisualiser.GetThemeColor("0LightGrayBrush", Colors.LightGray);
            Color glow = highlighted ? color : Colors.Black;

            shaftMaterial.Brush = new SolidColorBrush(color);
            headMaterial.Brush = new SolidColorBrush(color);
            shaftEmissive.Brush = new SolidColorBrush(glow);
            headEmissive.Brush = new SolidColorBrush(glow);

            if (MetaLabel != null)
            {
                MetaLabel.Foreground = new SolidColorBrush(highlighted
                    ? parentVisualiser.GetThemeColor("0HighlightBrush", Colors.OrangeRed)
                    : parentVisualiser.GetThemeColor("0LightGrayBrush", Colors.LightGray));
                Panel.SetZIndex(MetaLabel, highlighted ? 8500 : 900);
            }
        }

        private static Quaternion RotationFromYAxis(Vector3D direction)
        {
            Vector3D yAxis = new Vector3D(0, 1, 0);
            Vector3D axis = Vector3D.CrossProduct(yAxis, direction);
            double dot = Vector3D.DotProduct(yAxis, direction);

            if (axis.Length < 0.0001)
            {
                if (dot > 0) return Quaternion.Identity;
                return new Quaternion(new Vector3D(1, 0, 0), 180);
            }

            axis.Normalize();
            double angle = Math.Acos(Math.Max(-1, Math.Min(1, dot))) * 180.0 / Math.PI;
            return new Quaternion(axis, angle);
        }
    }

    internal static class GraphVisualiser3DMeshFactory
    {
        public static readonly MeshGeometry3D UnitSphere = CreateSphere(1.0, 16, 12);
        public static readonly MeshGeometry3D UnitCylinder = CreateCylinder(1.0, 1.0, 14);
        public static readonly MeshGeometry3D UnitCone = CreateCone(1.0, 1.0, 16);

        private static MeshGeometry3D CreateSphere(double radius, int slices, int stacks)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            for (int stack = 0; stack <= stacks; stack++)
            {
                double phi = Math.PI * stack / stacks;
                double y = Math.Cos(phi) * radius;
                double ringRadius = Math.Sin(phi) * radius;

                for (int slice = 0; slice <= slices; slice++)
                {
                    double theta = 2.0 * Math.PI * slice / slices;
                    double x = Math.Cos(theta) * ringRadius;
                    double z = Math.Sin(theta) * ringRadius;
                    Point3D p = new Point3D(x, y, z);
                    mesh.Positions.Add(p);
                    mesh.Normals.Add(new Vector3D(p.X, p.Y, p.Z));
                    mesh.TextureCoordinates.Add(new Point((double)slice / slices, (double)stack / stacks));
                }
            }

            int row = slices + 1;
            for (int stack = 0; stack < stacks; stack++)
                for (int slice = 0; slice < slices; slice++)
                {
                    int a = stack * row + slice;
                    int b = a + row;
                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(a + 1);
                    mesh.TriangleIndices.Add(a + 1);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(b + 1);
                }

            mesh.Freeze();
            return mesh;
        }

        private static MeshGeometry3D CreateCylinder(double radius, double height, int slices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            double half = height / 2.0;

            for (int i = 0; i <= slices; i++)
            {
                double angle = 2.0 * Math.PI * i / slices;
                double x = Math.Cos(angle) * radius;
                double z = Math.Sin(angle) * radius;
                Vector3D normal = new Vector3D(x, 0, z);
                normal.Normalize();

                mesh.Positions.Add(new Point3D(x, -half, z));
                mesh.Normals.Add(normal);
                mesh.Positions.Add(new Point3D(x, half, z));
                mesh.Normals.Add(normal);
            }

            for (int i = 0; i < slices; i++)
            {
                int a = i * 2;
                int b = a + 1;
                int c = a + 2;
                int d = a + 3;
                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);
                mesh.TriangleIndices.Add(d);
            }

            mesh.Freeze();
            return mesh;
        }

        private static MeshGeometry3D CreateCone(double radius, double height, int slices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            double half = height / 2.0;
            int tipIndex = 0;
            mesh.Positions.Add(new Point3D(0, half, 0));
            mesh.Normals.Add(new Vector3D(0, 1, 0));

            for (int i = 0; i <= slices; i++)
            {
                double angle = 2.0 * Math.PI * i / slices;
                double x = Math.Cos(angle) * radius;
                double z = Math.Sin(angle) * radius;
                Vector3D normal = new Vector3D(x, radius, z);
                normal.Normalize();
                mesh.Positions.Add(new Point3D(x, -half, z));
                mesh.Normals.Add(normal);
            }

            for (int i = 1; i <= slices; i++)
            {
                mesh.TriangleIndices.Add(tipIndex);
                mesh.TriangleIndices.Add(i);
                mesh.TriangleIndices.Add(i + 1);
            }

            mesh.Freeze();
            return mesh;
        }
    }

    public class GraphVisualiser3D : Grid, IListVisualiser, IHasSelectableEdges, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        private readonly Viewport3D viewport;
        private readonly Canvas labelOverlay;
        private readonly PerspectiveCamera camera;
        private readonly ModelVisual3D sceneRoot;
        private readonly ModelVisual3D lightRoot;
        private readonly ScaleTransform3D visualisationScale;
        private readonly ScaleTransform3D sceneScale;
        private readonly AxisAngleRotation3D sceneRotation;
        private readonly RotateTransform3D sceneRotate;
        private readonly Dictionary<IVertex, GraphVisualiser3DNode> displayedNodes;
        private readonly Dictionary<Model3D, GraphVisualiser3DNode> modelToNode;
        private readonly List<GraphVisualiser3DEdgeVisual> edgeVisuals;
        private readonly List<GraphVisualiser3DLabel> labels;
        private readonly Dictionary<string, double> metaAngleCache = new Dictionary<string, double>();

        private GraphVisualiser3DNode highlightedNode;
        private IVertex previousBaseEdgeTo;
        private IVertex tempSelectedVertices;
        private bool isPainting;
        private bool isFirstPainted;
        private bool repaintQueued;
        private bool labelRefreshQueued;
        private bool animationInProgress;
        private bool mouseIsDown;
        private bool cameraDragActive;
        private bool doubleClickHandled;
        private Point dragStart;
        private double yawAtDragStart;
        private double pitchAtDragStart;
        private double cameraYaw;
        private double cameraPitch = 0.45;
        private double cameraDistance = 900;

        private bool metaLabels;
        private bool showOutEdges;
        private bool showInEdges;
        private int maxVertices;
        private double sphereSize;
        private double scale = 1.0;
        private double labelScale = 1.0;

        internal double EdgeRadius { get; private set; }
        internal double ArrowRadius { get; private set; }
        internal double ArrowLength { get; private set; }

        private static readonly string[] _MetaTriggeringUpdateVertex = new string[] {
            "EdgeLength", "NumberOfCircles", "ShowOutEdges", "ShowInEdges",
            "MetaLabels", "LayoutMode3D", "TransitionStyle",
            "TransitionDurationMs", "SphereSize", "MaxVertices3D", "LabelSize"
        };
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        private static readonly string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        public GraphVisualiser3D(IEdge _edge)
        {
            Edge = _edge;
            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }

        public GraphVisualiser3D(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            displayedNodes = new Dictionary<IVertex, GraphVisualiser3DNode>();
            modelToNode = new Dictionary<Model3D, GraphVisualiser3DNode>();
            edgeVisuals = new List<GraphVisualiser3DEdgeVisual>();
            labels = new List<GraphVisualiser3DLabel>();

            Brush background = TryFindResource("0BackgroundBrush") as Brush;
            Background = background ?? new SolidColorBrush(Color.FromRgb(24, 24, 28));
            ClipToBounds = true;

            viewport = new Viewport3D();
            labelOverlay = new Canvas
            {
                IsHitTestVisible = false
            };

            Children.Add(viewport);
            Children.Add(labelOverlay);

            camera = new PerspectiveCamera
            {
                FieldOfView = 58,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 100000
            };
            viewport.Camera = camera;

            visualisationScale = new ScaleTransform3D(1, 1, 1);
            sceneScale = new ScaleTransform3D(1, 1, 1);
            sceneRotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            sceneRotate = new RotateTransform3D(sceneRotation);

            Transform3DGroup sceneTransform = new Transform3DGroup();
            sceneTransform.Children.Add(visualisationScale);
            sceneTransform.Children.Add(sceneScale);
            sceneTransform.Children.Add(sceneRotate);

            sceneRoot = new ModelVisual3D { Transform = sceneTransform };
            viewport.Children.Add(sceneRoot);

            lightRoot = new ModelVisual3D();
            Model3DGroup lightGroup = new Model3DGroup();
            lightGroup.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 95)));
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(220, 220, 230), new Vector3D(-1, -1.2, -1)));
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(90, 100, 130), new Vector3D(1, 0.4, 1)));
            lightRoot.Content = lightGroup;
            viewport.Children.Add(lightRoot);

            UpdateCamera();

            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Graph3D"),
                this,
                "GraphVisualiser3D",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            SizeChanged += GraphVisualiser3D_SizeChanged;
            PreviewMouseLeftButtonDown += GraphVisualiser3D_PreviewMouseLeftButtonDown;
            PreviewMouseMove += GraphVisualiser3D_PreviewMouseMove;
            PreviewMouseLeftButtonUp += GraphVisualiser3D_PreviewMouseLeftButtonUp;
            MouseWheel += GraphVisualiser3D_MouseWheel;
            MouseLeave += GraphVisualiser3D_MouseLeave;

            SetVertexDefaultValues();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
            PaintGraph();

            if (isFirstPainted)
                Loaded -= OnLoad;
        }

        private void GraphVisualiser3D_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLabels();
        }

        public void PaintGraph()
        {
            if (Vertex.DisposedState != DisposeStateEnum.Live)
                return;

            if (ActualWidth == 0 || ActualHeight == 0)
                return;

            Stopwatch sw = Stopwatch.StartNew();
            isPainting = true;

            metaLabels = !GeneralUtil.CompareStrings(Vertex.Get(false, "MetaLabels:"), "False");
            showOutEdges = !GeneralUtil.CompareStrings(Vertex.Get(false, "ShowOutEdges:"), "False");
            showInEdges = GeneralUtil.CompareStrings(Vertex.Get(false, "ShowInEdges:"), "True");
            maxVertices = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "MaxVertices3D:"));
            if (maxVertices <= 0) maxVertices = 250;
            sphereSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "SphereSize:"));
            if (sphereSize <= 0) sphereSize = 22;
            labelScale = ((double)(GraphUtil.GetIntegerValue(Vertex.Get(false, "LabelSize:")) ?? 100)) / 100.0;
            if (labelScale <= 0) labelScale = 1.0;
            EdgeRadius = Math.Max(1.5, sphereSize * 0.08);
            ArrowRadius = Math.Max(4, sphereSize * 0.22);
            ArrowLength = Math.Max(12, sphereSize * 0.8);

            ClearScene();

            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
            sceneRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
            sceneRotation.Angle = 0;
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, null);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, null);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, null);
            sceneScale.ScaleX = sceneScale.ScaleY = sceneScale.ScaleZ = 1;
            ApplyScaleFromVertex(false);

            IVertex baseTo = Vertex.Get(false, @"BaseEdge:\To:");
            if (baseTo != null)
            {
                LayoutAlgorithm3DEnum layout = LayoutAlgorithm3DEnumHelper.GetEnum(Vertex.Get(false, "LayoutMode3D:"));
                BuildGraph(baseTo, layout);
                UpdateLabelSizes();
                SelectWrappersForSelectedVertices();
            }

            previousBaseEdgeTo = baseTo;
            isFirstPainted = true;
            isPainting = false;
            UpdateLabels();

            sw.Stop();
            MinusZero.Instance.Log(1, "GraphVisualiser3D.PaintGraph",
                "vertices=" + displayedNodes.Count + " edges=" + edgeVisuals.Count + " elapsed_ms=" + sw.ElapsedMilliseconds);
        }

        public void RequestRepaint()
        {
            if (isPainting || repaintQueued) return;

            repaintQueued = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                repaintQueued = false;
                PaintGraph();
            }));
        }

        public void RequestLabelRefresh()
        {
            if (labelRefreshQueued) return;

            labelRefreshQueued = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                labelRefreshQueued = false;
                RefreshLabelText();
                UpdateLabels();
            }));
        }

        private void ClearScene()
        {
            foreach (GraphVisualiser3DNode node in displayedNodes.Values)
                node.Dispose();

            displayedNodes.Clear();
            modelToNode.Clear();
            edgeVisuals.Clear();
            labels.Clear();
            labelOverlay.Children.Clear();

            List<Visual3D> toRemove = new List<Visual3D>();
            foreach (Visual3D visual in sceneRoot.Children)
                toRemove.Add(visual);
            foreach (Visual3D visual in toRemove)
                sceneRoot.Children.Remove(visual);
        }

        private void BuildGraph(IVertex baseTo, LayoutAlgorithm3DEnum layout)
        {
            int numberOfCircles = GraphUtil.GetIntegerValue(Vertex.Get(false, "NumberOfCircles:")) ?? 2;
            int circleSize = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "EdgeLength:"));
            if (circleSize <= 0) circleSize = 200;

            AddNode(baseTo, new Point3D(0, 0, 0), true);

            HashSet<IVertex> visited = new HashSet<IVertex>();
            visited.Add(baseTo);
            List<IVertex> currentLevel = new List<IVertex> { baseTo };

            for (int level = 1; level <= numberOfCircles && currentLevel.Count > 0; level++)
            {
                List<Tuple<IEdge, IVertex, IVertex>> candidates = new List<Tuple<IEdge, IVertex, IVertex>>();

                foreach (IVertex from in currentLevel)
                {
                    foreach (IEdge edge in GetVisibleEdges(from))
                    {
                        IVertex target = edge.From == from ? edge.To : edge.From;
                        if (target == null) continue;

                        if (!displayedNodes.ContainsKey(target) && displayedNodes.Count + candidates.Count < maxVertices)
                            candidates.Add(Tuple.Create(edge, from, target));
                    }
                }

                List<IVertex> nextLevel = PlaceLevel(level, numberOfCircles, circleSize, layout, candidates, visited);

                foreach (IVertex from in currentLevel)
                    ConnectDisplayedEdges(from);

                currentLevel = nextLevel;
            }

            foreach (IVertex vertex in displayedNodes.Keys.ToList())
                ConnectDisplayedEdges(vertex);
        }

        private IEnumerable<IEdge> GetVisibleEdges(IVertex vertex)
        {
            List<IEdge> result = new List<IEdge>();

            if (showOutEdges)
                result.AddRange(VisualiserUtil.FilterEdges(vertex.OutEdges, Vertex));

            if (showInEdges)
                result.AddRange(VisualiserUtil.FilterEdges(vertex.InEdges, Vertex));

            return result.Where(CanAddEdge);
        }

        private bool CanAddEdge(IEdge edge)
        {
            if (edge == null || edge.Meta == null)
                return false;

            if (GeneralUtil.CompareStrings(edge.Meta, "$GraphChangeTrigger"))
                return false;

            return true;
        }

        private List<IVertex> PlaceLevel(int level, int maxLevel, int circleSize, LayoutAlgorithm3DEnum layout,
            List<Tuple<IEdge, IVertex, IVertex>> candidates, HashSet<IVertex> visited)
        {
            List<IVertex> added = new List<IVertex>();
            List<Tuple<IEdge, IVertex, IVertex>> unique = new List<Tuple<IEdge, IVertex, IVertex>>();

            foreach (Tuple<IEdge, IVertex, IVertex> candidate in candidates)
            {
                if (visited.Contains(candidate.Item3)) continue;
                visited.Add(candidate.Item3);
                unique.Add(candidate);
            }

            int count = unique.Count;
            if (count == 0) return added;

            double radius = circleSize * level;
            for (int i = 0; i < count; i++)
            {
                Tuple<IEdge, IVertex, IVertex> item = unique[i];
                double metaAngle = GetMetaAngle(item.Item1.Meta);
                double localOffset = ((double)i / Math.Max(1, count)) * Math.PI * 2.0;
                Point3D position = GetLayoutPosition(layout, level, maxLevel, radius, metaAngle, localOffset, i, count);
                AddNode(item.Item3, position, false);
                added.Add(item.Item3);
            }

            return added;
        }

        private double GetMetaAngle(IVertex meta)
        {
            string key = meta != null && meta.Value != null ? meta.Value.ToString() : "$Empty";
            if (metaAngleCache.TryGetValue(key, out double angle))
                return angle;

            int hash = key.GetHashCode();
            double normalized = Math.Abs(hash % 10000) / 10000.0;
            angle = normalized * Math.PI * 2.0;
            metaAngleCache[key] = angle;
            return angle;
        }

        private Point3D GetLayoutPosition(LayoutAlgorithm3DEnum layout, int level, int maxLevel, double radius,
            double metaAngle, double localOffset, int index, int count)
        {
            switch (layout)
            {
                case LayoutAlgorithm3DEnum.OrbitalPlanes:
                    return new Point3D(
                        Math.Cos(metaAngle + localOffset * 0.18) * radius,
                        (level - (maxLevel / 2.0)) * radius * 0.32,
                        Math.Sin(metaAngle + localOffset * 0.18) * radius);

                case LayoutAlgorithm3DEnum.ConcentricSpiral3D:
                    {
                        double angle = metaAngle + index * 0.72;
                        return new Point3D(
                            Math.Cos(angle) * radius,
                            (index - count / 2.0) * sphereSize * 1.8,
                            Math.Sin(angle) * radius);
                    }

                case LayoutAlgorithm3DEnum.Sugiyama3DLayers:
                    {
                        double angle = localOffset;
                        return new Point3D(
                            Math.Cos(angle) * radius * 0.8,
                            -level * radius * 0.55,
                            Math.Sin(angle) * radius * 0.45);
                    }

                case LayoutAlgorithm3DEnum.Force3D:
                case LayoutAlgorithm3DEnum.FibonacciSphereShells:
                default:
                    return FibonacciSpherePosition(radius, index, count, metaAngle);
            }
        }

        private Point3D FibonacciSpherePosition(double radius, int index, int count, double metaAngle)
        {
            double goldenAngle = Math.PI * (3.0 - Math.Sqrt(5.0));
            double y = 1.0 - (2.0 * (index + 0.5) / Math.Max(1, count));
            double ring = Math.Sqrt(Math.Max(0, 1.0 - y * y));
            double theta = goldenAngle * index + metaAngle * 0.35;

            return new Point3D(
                Math.Cos(theta) * ring * radius,
                y * radius,
                Math.Sin(theta) * ring * radius);
        }

        private GraphVisualiser3DNode AddNode(IVertex vertex, Point3D position, bool isRoot)
        {
            if (displayedNodes.TryGetValue(vertex, out GraphVisualiser3DNode existing))
                return existing;

            double size = isRoot ? sphereSize * 1.25 : sphereSize;
            GraphVisualiser3DNode node = new GraphVisualiser3DNode(vertex, this, size);
            node.SetWorldPosition(position);

            displayedNodes[vertex] = node;
            modelToNode[node.BodyModel] = node;
            sceneRoot.Children.Add(node);

            TextBlock label = CreateLabel(vertex.Value != null ? vertex.Value.ToString() : "Ø", false);
            node.Label = label;
            node.ApplyLabelState();
            labelOverlay.Children.Add(label);
            labels.Add(new GraphVisualiser3DLabel
            {
                Element = label,
                GetWorldPosition = () => node.Position + new Vector3D(0, size * 1.35, 0),
                BaseZIndex = isRoot ? 3000 : 1000,
                IsEdgeLabel = false
            });

            return node;
        }

        private void ConnectDisplayedEdges(IVertex vertex)
        {
            if (!displayedNodes.TryGetValue(vertex, out GraphVisualiser3DNode fromNode))
                return;

            foreach (IEdge edge in GetVisibleEdges(vertex))
            {
                if (!showInEdges && edge.From != vertex)
                    continue;

                IVertex toVertex = edge.From == vertex ? edge.To : edge.From;
                if (toVertex == null || !displayedNodes.TryGetValue(toVertex, out GraphVisualiser3DNode toNode))
                    continue;

                GraphVisualiser3DNode actualFrom = edge.From == vertex ? fromNode : toNode;
                GraphVisualiser3DNode actualTo = edge.From == vertex ? toNode : fromNode;
                if (EdgeAlreadyDisplayed(edge, actualFrom, actualTo))
                    continue;

                AddEdgeVisual(actualFrom, actualTo, edge);
            }
        }

        private bool EdgeAlreadyDisplayed(IEdge edge, GraphVisualiser3DNode from, GraphVisualiser3DNode to)
        {
            foreach (GraphVisualiser3DEdgeVisual visual in edgeVisuals)
                if (visual.Edge == edge && visual.FromNode == from && visual.ToNode == to)
                    return true;
            return false;
        }

        private void AddEdgeVisual(GraphVisualiser3DNode from, GraphVisualiser3DNode to, IEdge edge)
        {
            if (from == null || to == null || from == to || edge == null)
                return;

            GraphVisualiser3DEdgeVisual visual = new GraphVisualiser3DEdgeVisual(this);
            visual.Connect(from, to, edge, sphereSize);

            GraphVisualiser3DEdgeTag tag = new GraphVisualiser3DEdgeTag
            {
                FromNode = from,
                ToNode = to,
                Edge = edge
            };
            from.OutgoingEdges.Add(visual);
            from.IncidentEdges.Add(visual);
            to.IncidentEdges.Add(visual);
            edgeVisuals.Add(visual);
            sceneRoot.Children.Add(visual);

            if (metaLabels && edge.Meta != null && edge.Meta.Value != null)
            {
                TextBlock metaLabel = CreateLabel(edge.Meta.Value.ToString(), true);
                visual.MetaLabel = metaLabel;
                tag.MetaLabel = metaLabel;
                labelOverlay.Children.Add(metaLabel);
                labels.Add(new GraphVisualiser3DLabel
                {
                    Element = metaLabel,
                    GetWorldPosition = visual.GetLabelPosition,
                    BaseZIndex = 900,
                    IsEdgeLabel = true
                });
            }
        }

        private TextBlock CreateLabel(string text, bool isEdge)
        {
            TextBlock label = new TextBlock
            {
                Text = string.IsNullOrEmpty(text) ? "Ø" : text,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(isEdge
                    ? GetThemeColor("0LightGrayBrush", Colors.LightGray)
                    : GetThemeColor("0ForegroundBrush", Colors.White)),
                Background = GetLabelBackgroundBrush((byte)(isEdge ? 160 : 205)),
                IsHitTestVisible = false
            };

            ApplyLabelSize(label, isEdge);
            return label;
        }

        private void UpdateLabelSizes()
        {
            foreach (GraphVisualiser3DLabel label in labels)
                ApplyLabelSize(label.Element, label.IsEdgeLabel);
        }

        private void ApplyLabelSize(TextBlock label, bool isEdge)
        {
            double effectiveLabelScale = Math.Max(0.01, labelScale * scale);
            label.Padding = isEdge
                ? new Thickness(3 * effectiveLabelScale, 0, 3 * effectiveLabelScale, 0)
                : new Thickness(5 * effectiveLabelScale, 1 * effectiveLabelScale, 5 * effectiveLabelScale, 1 * effectiveLabelScale);
            label.FontSize = (isEdge ? 10 : 12) * effectiveLabelScale;
        }

        private void RefreshLabelText()
        {
            foreach (GraphVisualiser3DNode node in displayedNodes.Values)
                if (node.Label != null)
                    node.Label.Text = node.BaseVertex != null && node.BaseVertex.Value != null
                        ? node.BaseVertex.Value.ToString()
                        : "Ø";

            foreach (GraphVisualiser3DEdgeVisual edge in edgeVisuals)
                if (edge.MetaLabel != null)
                    edge.MetaLabel.Text = edge.Edge != null && edge.Edge.Meta != null && edge.Edge.Meta.Value != null
                        ? edge.Edge.Meta.Value.ToString()
                        : "Ø";
        }

        private void UpdateLabels()
        {
            if (labelOverlay == null || camera == null)
                return;

            foreach (GraphVisualiser3DLabel label in labels)
            {
                Point screen;
                double depth;
                bool visible = TryProject(label.GetWorldPosition(), out screen, out depth);

                if (!visible)
                {
                    label.Element.Visibility = Visibility.Collapsed;
                    continue;
                }

                label.Element.Visibility = Visibility.Visible;
                label.Element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                System.Windows.Size desired = label.Element.DesiredSize;
                Canvas.SetLeft(label.Element, screen.X - desired.Width / 2.0);
                Canvas.SetTop(label.Element, screen.Y - desired.Height / 2.0);

                double opacity = Math.Max(0.35, Math.Min(1.0, 1200.0 / Math.Max(250.0, depth)));
                label.Element.Opacity = label.Element == highlightedNode?.Label ? 1.0 : opacity;
                Panel.SetZIndex(label.Element, label.BaseZIndex + (int)Math.Max(0, 2000 - depth));
            }
        }

        private bool TryProject(Point3D localPoint, out Point screen, out double depth)
        {
            Matrix3D sceneMatrix = sceneRoot.Transform != null ? sceneRoot.Transform.Value : Matrix3D.Identity;
            Point3D worldPoint = sceneMatrix.Transform(localPoint);

            Vector3D forward = camera.LookDirection;
            if (forward.Length < 0.0001)
            {
                screen = new Point();
                depth = 0;
                return false;
            }
            forward.Normalize();

            Vector3D up = camera.UpDirection;
            if (up.Length < 0.0001) up = new Vector3D(0, 1, 0);
            up.Normalize();

            Vector3D right = Vector3D.CrossProduct(forward, up);
            if (right.Length < 0.0001) right = new Vector3D(1, 0, 0);
            right.Normalize();
            up = Vector3D.CrossProduct(right, forward);
            up.Normalize();

            Vector3D fromCamera = worldPoint - camera.Position;
            double x = Vector3D.DotProduct(fromCamera, right);
            double y = Vector3D.DotProduct(fromCamera, up);
            depth = Vector3D.DotProduct(fromCamera, forward);

            if (depth <= camera.NearPlaneDistance)
            {
                screen = new Point();
                return false;
            }

            double width = Math.Max(1, ActualWidth);
            double height = Math.Max(1, ActualHeight);
            double focal = width / (2.0 * Math.Tan(camera.FieldOfView * Math.PI / 360.0));

            screen = new Point(
                width / 2.0 + x * focal / depth,
                height / 2.0 - y * focal / depth);

            if (screen.X < -400 || screen.X > width + 400 || screen.Y < -200 || screen.Y > height + 200)
                return false;

            return true;
        }

        private void GraphVisualiser3D_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GraphVisualiser3DNode hit = HitTestVertexAt(e.GetPosition(viewport));
            if (e.ClickCount == 2 && hit != null)
            {
                doubleClickHandled = true;
                mouseIsDown = false;
                cameraDragActive = false;

                if (IsMouseCaptured)
                    ReleaseMouseCapture();

                ChangeBaseEdge(hit);
                e.Handled = true;
                return;
            }

            doubleClickHandled = false;
            mouseIsDown = true;
            cameraDragActive = false;
            dragStart = e.GetPosition(this);
            yawAtDragStart = cameraYaw;
            pitchAtDragStart = cameraPitch;
            CaptureMouse();
        }

        private void GraphVisualiser3D_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this);

            if (mouseIsDown && e.LeftButton == MouseButtonState.Pressed)
            {
                Vector delta = point - dragStart;
                if (cameraDragActive || Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
                {
                    cameraDragActive = true;
                    cameraYaw = yawAtDragStart - delta.X * 0.006;
                    cameraPitch = Math.Max(-1.35, Math.Min(1.35, pitchAtDragStart + delta.Y * 0.006));
                    UpdateCamera();
                    UpdateLabels();
                    e.Handled = true;
                    return;
                }
            }

            GraphVisualiser3DNode hit = HitTestVertexAt(e.GetPosition(viewport));
            SetHoverNode(hit);
        }

        private void GraphVisualiser3D_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
                ReleaseMouseCapture();

            mouseIsDown = false;

            if (doubleClickHandled)
            {
                doubleClickHandled = false;
                cameraDragActive = false;
                e.Handled = true;
                return;
            }

            GraphVisualiser3DNode hit = HitTestVertexAt(e.GetPosition(viewport));
            if (!cameraDragActive && hit != null)
            {
                ToggleSelection(hit);
                e.Handled = true;
            }

            cameraDragActive = false;
        }

        private void GraphVisualiser3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 0.88 : 1.14;
            cameraDistance = Math.Max(80, Math.Min(30000, cameraDistance * factor));
            UpdateCamera();
            UpdateLabels();
            e.Handled = true;
        }

        private void GraphVisualiser3D_MouseLeave(object sender, MouseEventArgs e)
        {
            SetHoverNode(null);
        }

        private void SetHoverNode(GraphVisualiser3DNode node)
        {
            if (highlightedNode == node)
                return;

            if (highlightedNode != null)
            {
                highlightedNode.SetHighlighted(false);
                foreach (GraphVisualiser3DEdgeVisual edge in highlightedNode.OutgoingEdges)
                {
                    edge.SetHighlighted(false);
                    if (edge.ToNode != null && !edge.ToNode.IsSelected)
                        edge.ToNode.SetHighlighted(false);
                }
            }

            highlightedNode = node;

            if (highlightedNode != null)
            {
                highlightedNode.SetHighlighted(true);
                foreach (GraphVisualiser3DEdgeVisual edge in highlightedNode.OutgoingEdges)
                {
                    edge.SetHighlighted(true);
                    if (edge.ToNode != null)
                        edge.ToNode.SetHighlighted(true);
                }
            }

            UpdateLabels();
        }

        private GraphVisualiser3DNode HitTestVertexAt(Point point)
        {
            GraphVisualiser3DNode result = null;

            VisualTreeHelper.HitTest(viewport, null, hit =>
            {
                RayHitTestResult rayHit = hit as RayHitTestResult;
                if (rayHit != null && rayHit.ModelHit != null && modelToNode.TryGetValue(rayHit.ModelHit, out GraphVisualiser3DNode node))
                {
                    result = node;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(point));

            return result;
        }

        private void ToggleSelection(GraphVisualiser3DNode node)
        {
            if (node == null || node.BaseVertex == null)
                return;

            Interaction.BeginInteractionWithGraph();

            CopySelectedVerticesToTemp();
            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (isCtrl)
            {
                if (node.IsSelected)
                {
                    node.Unselect();
                    EdgeHelper.DeleteVertexByEdgeTo(selectedEdges, node.BaseVertex);
                }
                else
                {
                    node.Select();
                    EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, node.BaseVertex);
                }
            }
            else
            {
                UnselectAllSelected();
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                node.Select();
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, node.BaseVertex);
            }

            Interaction.EndInteractionWithGraph();
            UpdateLabels();
        }

        private void ChangeBaseEdge(GraphVisualiser3DNode node)
        {
            if (node == null || node.BaseVertex == null)
                return;

            RestoreSelectedVertices();
            GraphUtil.ReplaceEdge(Vertex.Get(false, "BaseEdge:"), "To", node.BaseVertex);

            IVertex updatedBaseTo = Vertex.Get(false, @"BaseEdge:\To:");
            if (updatedBaseTo == node.BaseVertex)
                BaseEdgeToUpdated();
        }

        public void BaseEdgeToUpdated()
        {
            if (!isFirstPainted)
            {
                PaintGraph();
                return;
            }

            if (animationInProgress)
                return;

            IVertex newBaseTo = Vertex.Get(false, @"BaseEdge:\To:");
            bool targetOnScene = newBaseTo != null
                && displayedNodes.ContainsKey(newBaseTo)
                && newBaseTo != previousBaseEdgeTo;

            TransitionStyle3DEnum style = TransitionStyle3DEnumHelper.GetEnum(Vertex.Get(false, "TransitionStyle:"));
            if (!targetOnScene || style == TransitionStyle3DEnum.Cut)
            {
                ResetTransitionVisualState(false);
                PaintGraph();
                return;
            }

            int durationMs = GraphUtil.GetIntegerValueOr0(Vertex.Get(false, "TransitionDurationMs:"));
            if (durationMs <= 0) durationMs = 600;

            GraphVisualiser3DNode target = displayedNodes[newBaseTo];

            switch (style)
            {
                case TransitionStyle3DEnum.FlyToAndSwap:
                    RunFocusFlight(target, durationMs);
                    break;
                case TransitionStyle3DEnum.HyperspaceJump:
                    RunWarpTransition(durationMs);
                    break;
                case TransitionStyle3DEnum.GravityMorph:
                case TransitionStyle3DEnum.OrbitTransition:
                default:
                    RunOrbitTransition(target, durationMs);
                    break;
            }
        }

        private void RunOrbitTransition(GraphVisualiser3DNode target, int durationMs)
        {
            Vector3D targetVector = (Vector3D)target.Position;
            if (targetVector.Length < 0.001)
            {
                PaintGraph();
                return;
            }

            Vector3D axis = Vector3D.CrossProduct(new Vector3D(0, 1, 0), targetVector);
            if (axis.Length < 0.001)
                axis = new Vector3D(1, 0, 0);
            axis.Normalize();
            sceneRotation.Axis = axis;

            DoubleAnimation rotate = new DoubleAnimation(0, 90, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            animationInProgress = true;
            rotate.Completed += (s, e) =>
            {
                animationInProgress = false;
                PaintGraph();
                ResetTransitionVisualState(false);
            };

            sceneRotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotate);
        }

        private void RunFocusFlight(GraphVisualiser3DNode target, int durationMs)
        {
            animationInProgress = true;

            Point3D startPosition = camera.Position;
            Point3D targetCameraPosition = target.Position + (camera.Position - target.Position) * 0.28;
            Point3DAnimation flyIn = new Point3DAnimation(startPosition, targetCameraPosition,
                TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            DoubleAnimation fade = new DoubleAnimation(1, 0.15, TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fade.Completed += (s, e) =>
            {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
                PaintGraph();
                Point3D defaultCamera = CurrentCameraPositionFromAngles();
                Point3DAnimation flyOut = new Point3DAnimation(camera.Position, defaultCamera,
                    TimeSpan.FromMilliseconds(durationMs / 2))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                flyOut.Completed += (s2, e2) =>
                {
                    animationInProgress = false;
                    ResetTransitionVisualState(true);
                    UpdateLabels();
                };
                camera.BeginAnimation(ProjectionCamera.PositionProperty, flyOut);
            };

            camera.BeginAnimation(ProjectionCamera.PositionProperty, flyIn);
            BeginAnimation(OpacityProperty, fade);
        }

        private void RunWarpTransition(int durationMs)
        {
            animationInProgress = true;
            DoubleAnimation scaleDown = new DoubleAnimation(1, 0.02, TimeSpan.FromMilliseconds(durationMs / 2))
            {
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseIn }
            };

            scaleDown.Completed += (s, e) =>
            {
                PaintGraph();

                DoubleAnimation scaleUp = new DoubleAnimation(0.02, 1, TimeSpan.FromMilliseconds(durationMs / 2))
                {
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                };
                scaleUp.Completed += (s2, e2) =>
                {
                    animationInProgress = false;
                    ResetTransitionVisualState(false);
                    UpdateLabels();
                };

                sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleUp);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleUp);
                sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleUp);
            };

            sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, scaleDown);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, scaleDown);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, scaleDown);
        }

        private void ResetTransitionVisualState(bool resetCameraAnimation)
        {
            BeginAnimation(OpacityProperty, null);
            Opacity = 1;

            sceneScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, null);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, null);
            sceneScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, null);
            sceneScale.ScaleX = 1;
            sceneScale.ScaleY = 1;
            sceneScale.ScaleZ = 1;

            if (resetCameraAnimation)
                UpdateCamera();
        }

        private Point3D CurrentCameraPositionFromAngles()
        {
            double x = cameraDistance * Math.Cos(cameraPitch) * Math.Sin(cameraYaw);
            double y = cameraDistance * Math.Sin(cameraPitch);
            double z = cameraDistance * Math.Cos(cameraPitch) * Math.Cos(cameraYaw);
            return new Point3D(x, y, z);
        }

        private void UpdateCamera()
        {
            Point3D position = CurrentCameraPositionFromAngles();
            camera.BeginAnimation(ProjectionCamera.PositionProperty, null);
            camera.Position = position;
            camera.LookDirection = new Vector3D(-position.X, -position.Y, -position.Z);
            camera.UpDirection = new Vector3D(0, 1, 0);
        }

        public void ScaleChange()
        {
            ApplyScaleFromVertex(true);
        }

        private void ApplyScaleFromVertex(bool updateExistingLabels)
        {
            scale = ((double)(GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:")) ?? 100)) / 100.0;
            if (scale <= 0)
                scale = 1.0;

            visualisationScale.ScaleX = scale;
            visualisationScale.ScaleY = scale;
            visualisationScale.ScaleZ = scale;

            if (updateExistingLabels)
                UpdateLabelSizes();

            UpdateLabels();
        }

        private void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();
            GraphUtil.CopyShallow(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        private void RestoreSelectedVertices()
        {
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                GraphUtil.CopyShallow(tempSelectedVertices, selectedEdges);
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(tempSelectedVertices);
            }
        }

        private void UnselectAllSelected()
        {
            foreach (GraphVisualiser3DNode node in displayedNodes.Values)
                node.Unselect();
        }

        public void UnselectAllSelectedEdges()
        {
            Interaction.BeginInteractionWithGraph();
            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");
            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
            Interaction.EndInteractionWithGraph();
        }

        public void SelectedVerticesUpdated()
        {
            if (isFirstPainted)
            {
                UnselectAllSelected();
                SelectWrappersForSelectedVertices();
                UpdateLabels();
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

        public IVertex GetEdgeByPoint(Point p)
        {
            GraphVisualiser3DNode hit = HitTestVertexAt(p);
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
            Vertex.Get(false, "EdgeLength:").Value = 210;
            Vertex.Get(false, "NumberOfCircles:").Value = 1;
            Vertex.Get(false, "LabelSize:").Value = 100;
            Vertex.Get(false, "MetaLabels:").Value = "True";
            Vertex.Get(false, "ShowOutEdges:").Value = "True";
            Vertex.Get(false, "ShowInEdges:").Value = "False";
            Vertex.Get(false, "LayoutMode3D:").Value = "FibonacciSphereShells";
            Vertex.Get(false, "TransitionStyle:").Value = "OrbitTransition";
            Vertex.Get(false, "TransitionDurationMs:").Value = 600;
            Vertex.Get(false, "SphereSize:").Value = 22;
            Vertex.Get(false, "MaxVertices3D:").Value = 250;
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            ClearScene();
            VisualiserHelper.Dispose();
        }

        internal Color GetThemeColor(string key, Color fallback)
        {
            object resource = TryFindResource(key) ?? Application.Current?.TryFindResource(key);
            if (resource is SolidColorBrush brush)
                return brush.Color;
            return fallback;
        }

        internal Brush GetLabelBackgroundBrush(byte alpha)
        {
            Color background = GetThemeColor("0BackgroundBrush", Colors.Black);
            return new SolidColorBrush(Color.FromArgb(alpha, background.R, background.G, background.B));
        }
    }
}

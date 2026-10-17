using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace m0.UIWpf.Forms
{
    sealed partial class AboutLogoScene
    {
        void SetupRoleSwap()
        {
            _dynamicRingGeometry = CreateDynamicRingGeometry(48, 16);
            _ringModel.Geometry = _dynamicRingGeometry;
            MeshGeometry3D secondTube = CreateDynamicRingGeometry(48, 16);
            AddEffectVisual(
                secondTube,
                Color.FromRgb(84, 166, 232),
                Color.FromRgb(22, 72, 142),
                1);
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = 0.001;
        }

        void UpdateRoleSwap(double progress)
        {
            double amount = EaseInOut(progress);
            UpdateRoleSwapTube(_dynamicRingGeometry, amount);
            UpdateRoleSwapTube((MeshGeometry3D)_effectVisuals[0].Model.Geometry, 1 - amount);
            _ringSpin.Emissive.Color = LerpColor(
                RestGlow,
                Color.FromRgb(38, 118, 224),
                Math.Sin(progress * Math.PI));
            _effectVisuals[0].Rotation.Axis = new Vector3D(0, 1, 0);
            _effectVisuals[0].Rotation.Angle = Math.Sin(progress * Math.PI) * 180;
            _camera.Position = new Point3D(
                Math.Sin(progress * Math.PI * 2) * 0.20,
                0.2,
                5.25 - Math.Sin(progress * Math.PI) * 0.42);
        }

        void SetupPolygonForge()
        {
            _dynamicRingGeometry = CreateDynamicRingGeometry(48, 16);
            _ringModel.Geometry = _dynamicRingGeometry;
        }

        void UpdatePolygonForge(double progress)
        {
            UpdatePolygonRing(_dynamicRingGeometry, progress);
            double pulse = Math.Sin(progress * Math.PI * 5);
            _barHeroScale.ScaleX = 1 + pulse * 0.12;
            _barHeroScale.ScaleY = _barHeroScale.ScaleZ = 1 - pulse * 0.06;
            _barHeroRotation.Axis = new Vector3D(0, 0, 1);
            _barHeroRotation.Angle = pulse * 10;
            _ringSpin.Emissive.Color = LerpColor(
                RestGlow,
                Color.FromRgb(36, 116, 225),
                Math.Sin(progress * Math.PI) * (0.45 + 0.45 * Math.Abs(pulse)));
        }

        void SetupFractalLogos()
        {
            MeshGeometry3D ring = CreateTorus(RingMajor, RingMinor, 36, 10);
            MeshGeometry3D bar = CreateCapsule(BarRadius, BarCylinderLength, 14, 4);
            const int logoCount = 18;
            for (int logo = 0; logo < logoCount; logo++)
            {
                EffectVisual miniRing = AddEffectVisual(
                    ring,
                    logo < 6 ? Color.FromRgb(92, 180, 242) : Color.FromRgb(58, 132, 214),
                    Color.FromRgb(20, 74, 158),
                    0.82);
                miniRing.Strength = 0;
                miniRing.Phase = logo;

                EffectVisual miniBar = AddEffectVisual(
                    bar,
                    logo < 6 ? Color.FromRgb(116, 202, 250) : Color.FromRgb(67, 148, 224),
                    Color.FromRgb(25, 88, 176),
                    0.82);
                miniBar.Strength = 1;
                miniBar.Phase = logo;
            }
        }

        void UpdateFractalLogos(double progress, double time)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.55);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual visual = _effectVisuals[i];
                int logo = (int)visual.Phase;
                bool secondGeneration = logo >= 6;
                double delay = secondGeneration ? 0.16 : 0;
                double appearance = Clamp01((envelope - delay) * 4.2);
                double size = (secondGeneration ? 0.13 : 0.25) * appearance;
                Point3D center = FractalLogoCenter(logo, time * 0.22);
                if (visual.Strength > 0.5)
                    center.Y += BarRestY * size;
                SetMove(visual, center);
                visual.Scale.ScaleX = visual.Scale.ScaleY = visual.Scale.ScaleZ = Math.Max(0.001, size);
                visual.Rotation.Axis = new Vector3D(0.2, 1, 0.1);
                visual.Rotation.Angle = (logo % 2 == 0 ? 1 : -1) * progress * 360;
            }

            double baseScale = 1 - envelope * 0.28;
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = baseScale;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = baseScale;
            _glint.Color = LerpColor(
                Color.FromRgb(88, 155, 210),
                Color.FromRgb(135, 214, 255),
                envelope);
        }

        static Point3D FractalLogoCenter(int index, double rotation)
        {
            if (index < 6)
            {
                double angle = rotation + index * Math.PI / 3;
                return new Point3D(Math.Cos(angle) * 1.12, Math.Sin(angle) * 0.92 + 0.20, Math.Sin(angle * 2) * 0.22);
            }

            int child = index - 6;
            int parent = child / 2;
            int side = child % 2 == 0 ? -1 : 1;
            double parentAngle = rotation + parent * Math.PI / 3;
            Point3D parentCenter = new Point3D(
                Math.Cos(parentAngle) * 1.12,
                Math.Sin(parentAngle) * 0.92 + 0.20,
                Math.Sin(parentAngle * 2) * 0.22);
            double childAngle = parentAngle + side * 0.55;
            return parentCenter + new Vector3D(
                Math.Cos(childAngle) * 0.46,
                Math.Sin(childAngle) * 0.38,
                side * 0.16);
        }

        void SetupMitosis()
        {
            MeshGeometry3D ring = CreateTorus(RingMajor, RingMinor, 40, 10);
            MeshGeometry3D bar = CreateCapsule(BarRadius, BarCylinderLength, 14, 4);
            for (int clone = 0; clone < 8; clone++)
            {
                EffectVisual cloneRing = AddEffectVisual(
                    ring,
                    clone % 2 == 0 ? Color.FromRgb(65, 150, 228) : Color.FromRgb(118, 205, 252),
                    Color.FromRgb(24, 86, 178),
                    0.92);
                cloneRing.Strength = 0;
                cloneRing.Phase = clone;

                EffectVisual cloneBar = AddEffectVisual(
                    bar,
                    clone % 2 == 0 ? Color.FromRgb(82, 170, 238) : Color.FromRgb(140, 216, 255),
                    Color.FromRgb(30, 104, 198),
                    0.92);
                cloneBar.Strength = 1;
                cloneBar.Phase = clone;
            }
        }

        void UpdateMitosis(double progress)
        {
            double split = Math.Pow(Math.Sin(progress * Math.PI), 0.62);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual visual = _effectVisuals[i];
                int clone = (int)visual.Phase;
                double delay = clone < 2 ? 0 : clone < 4 ? 0.16 : 0.32;
                double activation = EaseOutCubic(Clamp01((progress - delay) / 0.14));
                double cloneScale = split * activation * 0.205;
                double angle = clone * Math.PI * 0.25 + progress * Math.PI * 1.4;
                double radius = split * activation * 1.02;
                Point3D center = new Point3D(
                    Math.Cos(angle) * radius,
                    0.18 + Math.Sin(angle) * radius * 0.78,
                    Math.Sin(angle * 2) * 0.28);
                if (visual.Strength > 0.5)
                    center.Y += BarRestY * cloneScale;
                SetMove(visual, center);
                visual.Scale.ScaleX =
                    visual.Scale.ScaleY =
                    visual.Scale.ScaleZ = Math.Max(0.001, cloneScale);
                visual.Rotation.Axis = new Vector3D(0.2, 1, 0.15);
                visual.Rotation.Angle = progress * (clone % 2 == 0 ? 360 : -360);
            }

            double original = Math.Max(0.001, 1 - split * 1.18);
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = original;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = original;
        }

        void SetupDna()
        {
            MeshGeometry3D sphere = CreateSphere(0.095, 9, 7);
            MeshGeometry3D rung = CreateCapsule(0.035, 0.72, 12, 4);
            for (int index = 0; index < 12; index++)
            {
                EffectVisual left = AddEffectVisual(
                    sphere,
                    Color.FromRgb(64, 150, 232),
                    Color.FromRgb(28, 96, 195),
                    1);
                left.Strength = 0;
                left.Phase = index;

                EffectVisual right = AddEffectVisual(
                    sphere,
                    Color.FromRgb(136, 214, 255),
                    Color.FromRgb(52, 142, 235),
                    1);
                right.Strength = 1;
                right.Phase = index;

                EffectVisual connector = AddEffectVisual(
                    rung,
                    Color.FromRgb(82, 172, 238),
                    Color.FromRgb(25, 92, 188),
                    0.72);
                connector.Strength = 2;
                connector.Phase = index;
            }
        }

        void UpdateDna(double progress, double time)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.52);
            for (int rungIndex = 0; rungIndex < 12; rungIndex++)
            {
                double y = -1.04 + rungIndex * (2.08 / 11);
                double angle = rungIndex * 0.78 + time * 1.8;
                Point3D leftPoint = new Point3D(Math.Cos(angle) * 0.60, y, Math.Sin(angle) * 0.60);
                Point3D rightPoint = new Point3D(-leftPoint.X, y, -leftPoint.Z);

                EffectVisual left = _effectVisuals[rungIndex * 3];
                EffectVisual right = _effectVisuals[rungIndex * 3 + 1];
                EffectVisual connector = _effectVisuals[rungIndex * 3 + 2];
                SetMove(left, leftPoint);
                SetMove(right, rightPoint);
                double beadSize = Math.Max(0.001, envelope);
                left.Scale.ScaleX = left.Scale.ScaleY = left.Scale.ScaleZ = beadSize;
                right.Scale.ScaleX = right.Scale.ScaleY = right.Scale.ScaleZ = beadSize;

                Point3D middle = LerpPoint(leftPoint, rightPoint, 0.5);
                SetMove(connector, middle);
                double length = (rightPoint - leftPoint).Length;
                OrientAlongX(connector, rightPoint - leftPoint);
                connector.Scale.ScaleX = Math.Max(0.001, envelope * length / 1.02);
                connector.Scale.ScaleY = connector.Scale.ScaleZ = Math.Max(0.001, envelope);
            }

            double baseVisibility = Math.Max(0.001, 1 - envelope * 1.25);
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = baseVisibility;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = baseVisibility;
            _camera.FieldOfView = 30 + envelope * 7;
        }

        void SetupVoxels()
        {
            var cube = new MeshGeometry3D();
            AddBox(cube, new Point3D(), new Vector3D(0.16, 0.16, 0.16));
            cube.Freeze();
            const int voxelCount = 48;
            for (int i = 0; i < voxelCount; i++)
            {
                EffectVisual voxel = AddEffectVisual(
                    cube,
                    i % 3 == 0
                        ? Color.FromRgb(140, 218, 255)
                        : i % 3 == 1
                            ? Color.FromRgb(70, 156, 235)
                            : Color.FromRgb(36, 104, 206),
                    Color.FromRgb(24, 90, 184),
                    0.94);
                voxel.Strength = i;
                voxel.Home = VoxelLogoPoint(i);
                voxel.SpinAxis = RandomAxis();
                SetMove(voxel, voxel.Home);
            }
        }

        void UpdateVoxels(double progress)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.45);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual voxel = _effectVisuals[i];
                Point3D target;
                double local;
                if (progress < 0.28)
                {
                    local = EaseInOut(progress / 0.28);
                    target = LerpPoint(voxel.Home, VoxelCubePoint(i), local);
                }
                else if (progress < 0.52)
                {
                    target = VoxelCubePoint(i);
                    local = 1;
                }
                else if (progress < 0.72)
                {
                    local = EaseInOut((progress - 0.52) / 0.20);
                    target = LerpPoint(VoxelCubePoint(i), VoxelPyramidPoint(i), local);
                }
                else
                {
                    local = EaseInOut((progress - 0.72) / 0.28);
                    target = LerpPoint(VoxelPyramidPoint(i), voxel.Home, local);
                }

                SetMove(voxel, target);
                voxel.Rotation.Axis = voxel.SpinAxis;
                voxel.Rotation.Angle = Math.Sin(progress * Math.PI) * (90 + i * 7);
                double scale = envelope * (0.72 + 0.20 * Math.Sin(progress * Math.PI * 2 + i));
                voxel.Scale.ScaleX = voxel.Scale.ScaleY = voxel.Scale.ScaleZ = scale;
            }
            double sourceScale = Math.Max(0.001, 1 - envelope * 1.18);
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = sourceScale;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = sourceScale;
            _groupRoll.Angle = Math.Sin(progress * Math.PI) * 12;
        }

        static Point3D VoxelLogoPoint(int index)
        {
            if (index < 36)
            {
                double angle = 2 * Math.PI * index / 36;
                return new Point3D(Math.Cos(angle), Math.Sin(angle), Math.Sin(angle * 3) * 0.05);
            }
            int barIndex = index - 36;
            return new Point3D(-0.84 + barIndex * (1.68 / 11), BarRestY, 0);
        }

        static Point3D VoxelCubePoint(int index)
        {
            int x = index % 4;
            int y = (index / 4) % 4;
            int z = index / 16;
            return new Point3D(
                (x - 1.5) * 0.34,
                (y - 1.5) * 0.34 + 0.20,
                (z - 1) * 0.34);
        }

        static Point3D VoxelPyramidPoint(int index)
        {
            int layer = index / 12;
            int within = index % 12;
            double width = 1.05 - layer * 0.22;
            double angle = 2 * Math.PI * within / 12;
            return new Point3D(
                Math.Cos(angle) * width,
                -0.82 + layer * 0.48,
                Math.Sin(angle) * width * 0.58);
        }

        void SetupAtomMachine()
        {
            MeshGeometry3D nodeGeometry = CreateSphere(0.11, 9, 7);
            MeshGeometry3D edgeGeometry = CreateCapsule(0.025, 0.78, 10, 3);
            for (int node = 0; node < 8; node++)
            {
                EffectVisual visual = AddEffectVisual(
                    nodeGeometry,
                    node % 2 == 0 ? Color.FromRgb(136, 216, 255) : Color.FromRgb(55, 142, 228),
                    Color.FromRgb(36, 122, 228),
                    0.96);
                visual.Strength = 0;
                visual.Phase = node;
            }

            int[,] edges =
            {
                { 0, 1 }, { 0, 2 }, { 0, 4 }, { 1, 3 }, { 1, 5 }, { 2, 3 },
                { 2, 6 }, { 3, 7 }, { 4, 5 }, { 4, 6 }, { 5, 7 }, { 6, 7 }
            };
            for (int i = 0; i < edges.GetLength(0); i++)
            {
                EffectVisual edge = AddEffectVisual(
                    edgeGeometry,
                    Color.FromRgb(64, 152, 230),
                    Color.FromRgb(24, 90, 188),
                    0.68);
                edge.Strength = 1;
                edge.Phase = edges[i, 0];
                edge.Speed = edges[i, 1];
            }
        }

        void UpdateAtomMachine(double progress)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.55);
            var points = new Point3D[8];
            for (int node = 0; node < 8; node++)
                points[node] = RotatedCubePoint(node, progress * Math.PI * 2, envelope);

            for (int i = 0; i < 8; i++)
            {
                EffectVisual node = _effectVisuals[i];
                SetMove(node, points[i]);
                node.Scale.ScaleX = node.Scale.ScaleY = node.Scale.ScaleZ = Math.Max(0.001, envelope);
            }

            for (int i = 8; i < _effectVisuals.Count; i++)
            {
                EffectVisual edge = _effectVisuals[i];
                Point3D a = points[(int)edge.Phase];
                Point3D b = points[(int)edge.Speed];
                SetMove(edge, LerpPoint(a, b, 0.5));
                Vector3D direction = b - a;
                OrientAlongX(edge, direction);
                edge.Scale.ScaleX = Math.Max(0.001, envelope * direction.Length / 1.03);
                edge.Scale.ScaleY = edge.Scale.ScaleZ = Math.Max(0.001, envelope);
            }

            double baseScale = 1 - envelope * 0.22;
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = baseScale;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = baseScale;
        }

        static Point3D RotatedCubePoint(int index, double angle, double envelope)
        {
            double x = (index & 1) == 0 ? -0.66 : 0.66;
            double y = (index & 2) == 0 ? -0.66 : 0.66;
            double z = (index & 4) == 0 ? -0.66 : 0.66;
            double cosY = Math.Cos(angle);
            double sinY = Math.Sin(angle);
            double rotatedX = x * cosY + z * sinY;
            double rotatedZ = -x * sinY + z * cosY;
            double pitch = angle * 0.63;
            double cosX = Math.Cos(pitch);
            double sinX = Math.Sin(pitch);
            double rotatedY = y * cosX - rotatedZ * sinX;
            rotatedZ = y * sinX + rotatedZ * cosX;
            return new Point3D(rotatedX * envelope, rotatedY * envelope + 0.18, rotatedZ * envelope);
        }

        void SetupHypercube()
        {
            MeshGeometry3D nodeGeometry = CreateSphere(0.085, 8, 6);
            MeshGeometry3D edgeGeometry = CreateCapsule(0.018, 0.72, 8, 3);
            for (int node = 0; node < 16; node++)
            {
                EffectVisual visual = AddEffectVisual(
                    nodeGeometry,
                    node < 8 ? Color.FromRgb(68, 156, 235) : Color.FromRgb(142, 218, 255),
                    Color.FromRgb(35, 118, 225),
                    0.94);
                visual.Strength = 0;
                visual.Phase = node;
            }

            for (int node = 0; node < 16; node++)
            {
                for (int dimension = 0; dimension < 4; dimension++)
                {
                    int other = node ^ (1 << dimension);
                    if (node >= other)
                        continue;
                    EffectVisual edge = AddEffectVisual(
                        edgeGeometry,
                        dimension % 2 == 0 ? Color.FromRgb(52, 138, 226) : Color.FromRgb(112, 200, 250),
                        Color.FromRgb(28, 102, 208),
                        0.58);
                    edge.Strength = 1;
                    edge.Phase = node;
                    edge.Speed = other;
                }
            }
        }

        void UpdateHypercube(double progress)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.50);
            double angle = progress * Math.PI * 2;
            var points = new Point3D[16];
            for (int node = 0; node < 16; node++)
                points[node] = ProjectTesseractPoint(node, angle, envelope);

            for (int i = 0; i < 16; i++)
            {
                EffectVisual node = _effectVisuals[i];
                SetMove(node, points[i]);
                node.Scale.ScaleX = node.Scale.ScaleY = node.Scale.ScaleZ = Math.Max(0.001, envelope);
            }

            for (int i = 16; i < _effectVisuals.Count; i++)
            {
                EffectVisual edge = _effectVisuals[i];
                Point3D a = points[(int)edge.Phase];
                Point3D b = points[(int)edge.Speed];
                Vector3D direction = b - a;
                SetMove(edge, LerpPoint(a, b, 0.5));
                OrientAlongX(edge, direction);
                edge.Scale.ScaleX = Math.Max(0.001, envelope * direction.Length / 0.97);
                edge.Scale.ScaleY = edge.Scale.ScaleZ = Math.Max(0.001, envelope);
            }

            double baseScale = 1 - envelope * 0.42;
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = baseScale;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = baseScale;
            _groupRoll.Angle = envelope * Math.Sin(angle) * 12;
            _camera.FieldOfView = 30 + envelope * 10;
        }

        static Point3D ProjectTesseractPoint(int index, double angle, double envelope)
        {
            double x = (index & 1) == 0 ? -1 : 1;
            double y = (index & 2) == 0 ? -1 : 1;
            double z = (index & 4) == 0 ? -1 : 1;
            double w = (index & 8) == 0 ? -1 : 1;

            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);
            double rotatedX = x * cosA - w * sinA;
            double rotatedW = x * sinA + w * cosA;
            double angleB = angle * 0.73;
            double cosB = Math.Cos(angleB);
            double sinB = Math.Sin(angleB);
            double rotatedY = y * cosB - z * sinB;
            double rotatedZ = y * sinB + z * cosB;
            double perspective = 1.45 / (2.65 - rotatedW * 0.42);

            return new Point3D(
                rotatedX * perspective * envelope,
                rotatedY * perspective * envelope + 0.20,
                rotatedZ * perspective * envelope);
        }

        static void OrientAlongX(EffectVisual visual, Vector3D direction)
        {
            if (direction.LengthSquared < 0.000001)
            {
                visual.Rotation.Axis = new Vector3D(0, 1, 0);
                visual.Rotation.Angle = 0;
                return;
            }

            Vector3D normalized = direction;
            normalized.Normalize();
            Vector3D xAxis = new Vector3D(1, 0, 0);
            Vector3D axis = Vector3D.CrossProduct(xAxis, normalized);
            double dot = Math.Max(-1, Math.Min(1, Vector3D.DotProduct(xAxis, normalized)));
            if (axis.LengthSquared < 0.000001)
                axis = new Vector3D(0, 1, 0);
            else
                axis.Normalize();
            visual.Rotation.Axis = axis;
            visual.Rotation.Angle = Math.Acos(dot) * 180 / Math.PI;
        }

        static void UpdateRoleSwapTube(MeshGeometry3D mesh, double lineAmount)
        {
            UpdateGeneratedTube(mesh, lineAmount, 0);
        }

        static void UpdatePolygonRing(MeshGeometry3D mesh, double progress)
        {
            UpdateGeneratedTube(mesh, progress, 1);
        }

        static void UpdateGeneratedTube(MeshGeometry3D mesh, double parameter, int mode)
        {
            const int aroundSegments = 48;
            const int tubeSegments = 16;
            const double epsilon = 0.001;
            var positions = new Point3DCollection((aroundSegments + 1) * (tubeSegments + 1));
            var normals = new Vector3DCollection((aroundSegments + 1) * (tubeSegments + 1));

            for (int i = 0; i <= aroundSegments; i++)
            {
                double theta = 2 * Math.PI * i / aroundSegments;
                Point3D center = GeneratedCenter(theta, parameter, mode);
                Point3D before = GeneratedCenter(theta - epsilon, parameter, mode);
                Point3D after = GeneratedCenter(theta + epsilon, parameter, mode);
                Vector3D tangent = after - before;
                if (tangent.LengthSquared < 0.000001)
                    tangent = new Vector3D(1, 0, 0);
                tangent.Normalize();
                Vector3D normalA = Vector3D.CrossProduct(new Vector3D(0, 0, 1), tangent);
                if (normalA.LengthSquared < 0.001)
                    normalA = new Vector3D(0, 1, 0);
                normalA.Normalize();
                Vector3D normalB = Vector3D.CrossProduct(tangent, normalA);
                normalB.Normalize();

                for (int j = 0; j <= tubeSegments; j++)
                {
                    double phi = 2 * Math.PI * j / tubeSegments;
                    Vector3D normal = normalA * Math.Cos(phi) + normalB * Math.Sin(phi);
                    positions.Add(center + normal * RingMinor);
                    normals.Add(normal);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
        }

        static Point3D GeneratedCenter(double theta, double parameter, int mode)
        {
            if (mode == 0)
            {
                Point3D circle = new Point3D(Math.Cos(theta), Math.Sin(theta), 0);
                double normalized = theta / (2 * Math.PI);
                Point3D line = new Point3D(Lerp(-0.92, 0.92, normalized), BarRestY, 0);
                return LerpPoint(circle, line, parameter);
            }

            double sequence = Clamp01(parameter) * 5;
            int shape = Math.Min(4, (int)Math.Floor(sequence));
            double blend = EaseInOut(sequence - shape);
            double radiusA = ShapeRadius(shape, theta);
            double radiusB = ShapeRadius(shape + 1, theta);
            double radius = Lerp(radiusA, radiusB, blend);
            return new Point3D(Math.Cos(theta) * radius, Math.Sin(theta) * radius, 0);
        }

        static double ShapeRadius(int shape, double theta)
        {
            switch (shape)
            {
                case 1:
                    return PolygonRadius(4, theta + Math.PI / 4);
                case 2:
                    return PolygonRadius(3, theta + Math.PI / 2);
                case 3:
                    return PolygonRadius(6, theta);
                case 4:
                    return 0.82 + Math.Cos(theta * 5) * 0.18;
                default:
                    return 1;
            }
        }

        static double PolygonRadius(int sides, double theta)
        {
            double sector = 2 * Math.PI / sides;
            double local = theta % sector;
            if (local < 0)
                local += sector;
            local -= sector / 2;
            return Math.Cos(Math.PI / sides) / Math.Cos(local);
        }
    }
}

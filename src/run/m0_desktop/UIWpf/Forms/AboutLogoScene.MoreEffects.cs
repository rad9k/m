using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace m0.UIWpf.Forms
{
    sealed partial class AboutLogoScene
    {
        void SetupTesla()
        {
            for (int i = 0; i < 5; i++)
            {
                EffectVisual bolt = AddEffectVisual(
                    CreateDynamicLightningRibbon(9),
                    i % 2 == 0 ? Color.FromRgb(115, 205, 255) : Color.FromRgb(48, 132, 245),
                    i % 2 == 0 ? Color.FromRgb(64, 170, 255) : Color.FromRgb(25, 92, 225),
                    0.82);
                bolt.Phase = i * 2.37;
            }
        }

        void UpdateTesla(double progress, double time)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.48);
            double step = Math.Floor(time * 24);

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual bolt = _effectVisuals[i];
                double barX = -0.72 + i * 0.36;
                double angle = time * 1.8 + i * Math.PI * 0.4;
                Point3D start = new Point3D(barX, BarRestY - 0.08, 0.05);
                Point3D end = new Point3D(
                    Math.Cos(angle) * RingMajor,
                    Math.Sin(angle) * RingMajor,
                    0.05);
                UpdateLightningRibbon(
                    (MeshGeometry3D)bolt.Model.Geometry,
                    start,
                    end,
                    9,
                    step + bolt.Phase,
                    0.018 + envelope * 0.018);
                bolt.Diffuse.Opacity = envelope * (0.52 + 0.42 * PseudoRandom(step + i));
                bolt.Emissive.Opacity = envelope;
            }

            double surge = envelope * (0.55 + 0.45 * Math.Sin(time * 14));
            _ringSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(42, 142, 255), surge);
            _barSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(75, 186, 255), surge);
            _point.Color = LerpColor(Color.FromRgb(176, 198, 228), Color.FromRgb(102, 192, 255), surge);
            _groupMove.OffsetX = (PseudoRandom(step * 1.73) * 2 - 1) * envelope * 0.022;
        }

        void SetupIris()
        {
            const int segmentCount = 18;
            for (int i = 0; i < segmentCount; i++)
            {
                double start = 2 * Math.PI * i / segmentCount;
                double end = 2 * Math.PI * (i + 1) / segmentCount;
                Point3D center;
                EffectVisual segment = AddEffectVisual(
                    CreateTorusArc(start, end, 5, 14, out center),
                    i % 2 == 0 ? Color.FromRgb(48, 118, 205) : Color.FromRgb(86, 164, 236),
                    Color.FromRgb(14, 48, 104),
                    1);
                segment.Home = center;
                segment.Phase = start;
                SetMove(segment, center);
            }
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = 0.001;
        }

        void UpdateIris(double progress)
        {
            double aperture = Math.Pow(Math.Sin(progress * Math.PI), 0.72);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual segment = _effectVisuals[i];
                Vector3D inward = new Vector3D(-segment.Home.X, -segment.Home.Y, 0);
                inward.Normalize();
                segment.Move.OffsetX = segment.Home.X + inward.X * aperture * 0.34;
                segment.Move.OffsetY = segment.Home.Y + inward.Y * aperture * 0.34;
                segment.Move.OffsetZ = Math.Sin(segment.Phase * 3) * aperture * 0.13;
                segment.Rotation.Axis = new Vector3D(0, 0, 1);
                segment.Rotation.Angle = aperture * (i % 2 == 0 ? 42 : -42);
                segment.Scale.ScaleX = 1 + aperture * 0.08;
                segment.Scale.ScaleY = 1 - aperture * 0.18;
            }

            _barHeroMove.OffsetY = aperture * 0.20;
            _barHeroRotation.Axis = new Vector3D(0, 1, 0);
            _barHeroRotation.Angle = aperture * 120;
            _camera.Position = new Point3D(0, 0.2, 5.25 - aperture * 0.42);
        }

        void SetupCrystal()
        {
            const int crystalCount = 24;
            MeshGeometry3D crystalGeometry = CreateCrystalGeometry();
            for (int i = 0; i < crystalCount; i++)
            {
                double angle = 2 * Math.PI * i / crystalCount;
                EffectVisual crystal = AddEffectVisual(
                    crystalGeometry,
                    i % 3 == 0
                        ? Color.FromRgb(155, 222, 255)
                        : i % 3 == 1
                            ? Color.FromRgb(70, 158, 242)
                            : Color.FromRgb(34, 98, 195),
                    Color.FromRgb(35, 112, 220),
                    0.92);
                crystal.Home = new Point3D(
                    Math.Cos(angle) * (RingMajor + RingMinor * 0.35),
                    Math.Sin(angle) * (RingMajor + RingMinor * 0.35),
                    0);
                crystal.Direction = new Vector3D(Math.Cos(angle), Math.Sin(angle), 0);
                crystal.Rotation.Axis = new Vector3D(0, 0, 1);
                crystal.Speed = angle * 180 / Math.PI;
                crystal.Rotation.Angle = crystal.Speed;
                crystal.Phase = i * 0.71;
                crystal.Strength = 0.65 + _random.NextDouble() * 0.8;
                SetMove(crystal, crystal.Home);
                crystal.Scale.ScaleX = crystal.Scale.ScaleY = crystal.Scale.ScaleZ = 0.001;
            }
        }

        void UpdateCrystal(double progress, double time)
        {
            double growth = Clamp01(progress / 0.34);
            double hold = progress < 0.62 ? 1 : 1 - EaseInOut((progress - 0.62) / 0.38);
            double crystalAmount = EaseOutCubic(growth) * hold;
            double fracture = Clamp01((progress - 0.62) / 0.38);

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual crystal = _effectVisuals[i];
                double flicker = 0.82 + 0.18 * Math.Sin(time * 7 + crystal.Phase);
                double size = crystalAmount * crystal.Strength * flicker;
                crystal.Scale.ScaleX = Math.Max(0.001, size);
                crystal.Scale.ScaleY = crystal.Scale.ScaleZ = Math.Max(0.001, crystalAmount * 0.75);
                crystal.Move.OffsetX = crystal.Home.X + crystal.Direction.X * fracture * 0.48;
                crystal.Move.OffsetY = crystal.Home.Y + crystal.Direction.Y * fracture * 0.48 - fracture * fracture * 0.38;
                crystal.Move.OffsetZ = Math.Sin(crystal.Phase) * fracture * 0.38;
                crystal.Rotation.Angle =
                    crystal.Speed + fracture * Math.Sin(crystal.Phase) * 240;
            }

            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ =
                1 - crystalAmount * 0.045;
            _point.Color = LerpColor(
                Color.FromRgb(176, 198, 228),
                Color.FromRgb(132, 215, 255),
                crystalAmount);
        }

        void SetupUnwind()
        {
            _dynamicRingGeometry = CreateDynamicRingGeometry(48, 16);
            _ringModel.Geometry = _dynamicRingGeometry;
        }

        void UpdateUnwind(double progress)
        {
            double amount = Math.Pow(Math.Sin(progress * Math.PI), 0.82);
            UpdateUnwindRingGeometry(_dynamicRingGeometry, amount);
            _barHeroMove.OffsetX = Math.Sin(progress * Math.PI * 2) * amount * 0.52;
            _barHeroMove.OffsetY = amount * 0.12;
            _barHeroRotation.Axis = new Vector3D(0.15, 1, 0.1);
            _barHeroRotation.Angle = progress * amount * 540;
            _camera.FieldOfView = 30 + amount * 9;
        }

        void SetupPortalTunnel()
        {
            MeshGeometry3D torus = CreateTorus(RingMajor, RingMinor * 0.78, 64, 16);
            for (int i = 0; i < 11; i++)
            {
                EffectVisual portal = AddEffectVisual(
                    torus,
                    i % 2 == 0 ? Color.FromRgb(48, 132, 225) : Color.FromRgb(116, 198, 248),
                    Color.FromRgb(22, 92, 205),
                    0.50);
                portal.Phase = i / 11.0;
            }
        }

        void UpdatePortalTunnel(double progress)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.62);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual portal = _effectVisuals[i];
                double travel = (progress * 1.75 + portal.Phase) % 1;
                double scale = 0.18 + travel * 1.12;
                portal.Scale.ScaleX = portal.Scale.ScaleY = portal.Scale.ScaleZ = scale;
                portal.Move.OffsetZ = -3.5 + travel * 4.25;
                portal.Rotation.Axis = new Vector3D(0, 0, 1);
                portal.Rotation.Angle = (i % 2 == 0 ? 1 : -1) * progress * 220;
                double opacity = envelope * Math.Sin(travel * Math.PI) * 0.58;
                portal.Diffuse.Opacity = opacity;
                portal.Emissive.Opacity = opacity;
            }

            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ =
                1 + envelope * 0.08;
            _camera.Position = new Point3D(0, 0.2, 5.25 - envelope * 0.72);
            _glint.Color = Color.FromRgb(118, 205, 255);
        }

        void SetupShockwave()
        {
            MeshGeometry3D torus = CreateTorus(RingMajor, RingMinor * 0.40, 64, 12);
            for (int i = 0; i < 7; i++)
            {
                EffectVisual wave = AddEffectVisual(
                    torus,
                    i % 2 == 0 ? Color.FromRgb(74, 164, 240) : Color.FromRgb(142, 216, 255),
                    Color.FromRgb(38, 128, 232),
                    0);
                wave.Phase = i * 0.11;
            }
        }

        void UpdateShockwave(double progress)
        {
            double impact = Math.Exp(-Math.Pow((progress - 0.18) / 0.09, 2));
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual wave = _effectVisuals[i];
                double local = (progress - wave.Phase) / 0.52;
                if (local < 0 || local > 1)
                {
                    wave.Scale.ScaleX = wave.Scale.ScaleY = wave.Scale.ScaleZ = 0.001;
                    wave.Diffuse.Opacity = 0;
                    wave.Emissive.Opacity = 0;
                    continue;
                }

                double scale = 1 + EaseOutCubic(local) * 1.65;
                wave.Scale.ScaleX = wave.Scale.ScaleY = wave.Scale.ScaleZ = scale;
                double opacity = Math.Sin(local * Math.PI) * 0.55;
                wave.Diffuse.Opacity = opacity;
                wave.Emissive.Opacity = opacity;
            }

            double rebound = Math.Sin(progress * Math.PI * 4) * Math.Exp(-progress * 3.8);
            _groupScale.ScaleX = 1 + impact * 0.18 + rebound * 0.04;
            _groupScale.ScaleY = 1 - impact * 0.15 - rebound * 0.03;
            _groupScale.ScaleZ = 1 + impact * 0.08;
            _camera.Position = new Point3D(0, 0.2, 5.25 + impact * 0.28);
        }

        void SetupHologramScan()
        {
            const int ringBands = 16;
            for (int i = 0; i < ringBands; i++)
            {
                double start = 2 * Math.PI * i / ringBands;
                double end = 2 * Math.PI * (i + 1) / ringBands;
                Point3D center;
                EffectVisual band = AddEffectVisual(
                    CreateTorusArc(start, end, 4, 10, out center),
                    i % 2 == 0 ? Color.FromRgb(48, 138, 235) : Color.FromRgb(128, 214, 255),
                    Color.FromRgb(34, 118, 235),
                    0.62);
                band.Home = center;
                band.Strength = 0;
                band.Phase = i * 0.61;
                SetMove(band, center);
            }

            for (int i = 0; i < 5; i++)
            {
                EffectVisual band = AddEffectVisual(
                    CreateCapsule(0.12, 0.17, 16, 4),
                    i % 2 == 0 ? Color.FromRgb(54, 146, 240) : Color.FromRgb(138, 220, 255),
                    Color.FromRgb(32, 112, 220),
                    0.60);
                band.Home = new Point3D(-0.82 + i * 0.41, BarRestY, 0);
                band.Strength = 1;
                band.Phase = 20 + i;
                SetMove(band, band.Home);
            }

            var scannerGeometry = new MeshGeometry3D();
            AddBox(scannerGeometry, new Point3D(), new Vector3D(2.65, 0.035, 0.10));
            scannerGeometry.Freeze();
            EffectVisual scanner = AddEffectVisual(
                scannerGeometry,
                Color.FromRgb(150, 226, 255),
                Color.FromRgb(74, 182, 255),
                0.80);
            scanner.Strength = 999;
        }

        void UpdateHologramScan(double progress, double time)
        {
            double scanY = Lerp(-1.42, 1.78, EaseInOut(progress));
            for (int i = 0; i < _effectVisuals.Count - 1; i++)
            {
                EffectVisual band = _effectVisuals[i];
                double distance = scanY - band.Home.Y;
                double wake = distance > 0
                    ? Math.Sin(Math.Min(1, distance * 2.4) * Math.PI) * Math.Exp(-distance * 1.8)
                    : 0;
                band.Move.OffsetX = band.Home.X + Math.Sin(time * 10 + band.Phase) * wake * 0.12;
                band.Move.OffsetY = band.Home.Y;
                band.Move.OffsetZ = wake * 0.34;
                band.Scale.ScaleX = 1 + wake * 0.12;
                band.Diffuse.Opacity = 0.22 + wake * 0.70;
                band.Emissive.Opacity = 0.28 + wake * 0.72;
            }

            EffectVisual scanner = _effectVisuals[_effectVisuals.Count - 1];
            scanner.Move.OffsetY = scanY;
            scanner.Move.OffsetZ = 0.42;
            double edgeFade = Math.Sin(progress * Math.PI);
            scanner.Scale.ScaleX = edgeFade;
            scanner.Scale.ScaleY = 1;
            scanner.Scale.ScaleZ = 1;
            _point.Color = Color.FromRgb(106, 196, 255);
        }

        void SetupComets()
        {
            MeshGeometry3D sphere = CreateSphere(0.09, 9, 7);
            for (int comet = 0; comet < 3; comet++)
            {
                for (int trail = 0; trail < 7; trail++)
                {
                    double opacity = trail == 0 ? 1 : 0.66 - trail * 0.075;
                    EffectVisual particle = AddEffectVisual(
                        sphere,
                        comet == 0
                            ? Color.FromRgb(155, 224, 255)
                            : comet == 1
                                ? Color.FromRgb(72, 160, 242)
                                : Color.FromRgb(35, 105, 215),
                        Color.FromRgb(45, 140, 245),
                        opacity);
                    particle.Strength = comet;
                    particle.Phase = trail * 0.032;
                    particle.Speed = trail;
                }
            }
        }

        void UpdateComets(double progress)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.45);
            Point3D leading = new Point3D();
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual particle = _effectVisuals[i];
                int comet = (int)particle.Strength;
                double local = progress - particle.Phase;
                if (local < 0)
                {
                    particle.Scale.ScaleX =
                        particle.Scale.ScaleY =
                        particle.Scale.ScaleZ = 0.001;
                    continue;
                }

                Point3D point = CometPoint(comet, local);
                SetMove(particle, point);
                double size = envelope * (particle.Speed == 0 ? 1.25 : 0.80 - particle.Speed * 0.075);
                particle.Scale.ScaleX = particle.Scale.ScaleY = particle.Scale.ScaleZ = Math.Max(0.001, size);
                if (comet == 0 && particle.Speed == 0)
                    leading = point;
            }

            _glint.Position = leading + new Vector3D(0, 0, 0.45);
            _glint.Color = Color.FromRgb(132, 216, 255);
            _ringHeroRotation.Axis = new Vector3D(0.2, 1, 0.15);
            _ringHeroRotation.Angle = Math.Sin(progress * Math.PI) * 75;
        }

        static Point3D CometPoint(int comet, double progress)
        {
            double angle = progress * Math.PI * (3.8 + comet * 0.55) + comet * Math.PI * 0.66;
            double radius = 1.25 + 0.18 * Math.Sin(progress * Math.PI * 3 + comet);
            return new Point3D(
                Math.Cos(angle) * radius,
                0.24 + Math.Sin(angle) * radius * 0.82,
                Math.Sin(angle * 0.72 + comet) * 0.62);
        }

        void SetupMirrorDimension()
        {
            EffectVisual ringCopy = AddEffectVisual(
                CreateTorus(RingMajor, RingMinor, 72, 20),
                Color.FromRgb(74, 160, 232),
                Color.FromRgb(32, 105, 205),
                0.48);
            ringCopy.Strength = 0;

            EffectVisual barCopy = AddEffectVisual(
                CreateCapsule(BarRadius, BarCylinderLength, 24, 6),
                Color.FromRgb(112, 198, 248),
                Color.FromRgb(45, 126, 220),
                0.48);
            barCopy.Strength = 1;
            barCopy.Home = new Point3D(0, BarRestY, 0);
        }

        void UpdateMirrorDimension(double progress)
        {
            double envelope = Math.Sin(progress * Math.PI);
            double cross = Math.Sin(progress * Math.PI * 2);
            double depth = -1.45 + envelope * 2.30;
            double copyX = cross * 0.62;

            EffectVisual ringCopy = _effectVisuals[0];
            EffectVisual barCopy = _effectVisuals[1];
            SetMove(ringCopy, new Point3D(copyX, 0, depth));
            SetMove(barCopy, new Point3D(copyX, BarRestY, depth));
            double copyScale = Math.Max(0.001, envelope * (0.72 + 0.28 * envelope));
            ringCopy.Scale.ScaleX = ringCopy.Scale.ScaleY = ringCopy.Scale.ScaleZ = copyScale;
            barCopy.Scale.ScaleX = barCopy.Scale.ScaleY = barCopy.Scale.ScaleZ = copyScale;
            ringCopy.Rotation.Axis = new Vector3D(0.15, 1, 0.1);
            barCopy.Rotation.Axis = new Vector3D(0.15, 1, 0.1);
            ringCopy.Rotation.Angle = progress * 360;
            barCopy.Rotation.Angle = progress * 360;

            _ringHeroMove.OffsetX = -copyX * 0.34;
            _barHeroMove.OffsetX = -copyX * 0.34;
            _ringHeroMove.OffsetZ = envelope * -0.26;
            _barHeroMove.OffsetZ = envelope * -0.26;
            _groupRoll.Angle = cross * envelope * 7;
            _camera.FieldOfView = 30 + envelope * 7;
        }

        static MeshGeometry3D CreateDynamicLightningRibbon(int segments)
        {
            var mesh = new MeshGeometry3D();
            for (int i = 0; i <= segments; i++)
            {
                mesh.Positions.Add(new Point3D());
                mesh.Positions.Add(new Point3D());
                mesh.Normals.Add(new Vector3D(0, 0, 1));
                mesh.Normals.Add(new Vector3D(0, 0, 1));
            }
            for (int i = 0; i < segments; i++)
            {
                int first = i * 2;
                mesh.TriangleIndices.Add(first);
                mesh.TriangleIndices.Add(first + 2);
                mesh.TriangleIndices.Add(first + 1);
                mesh.TriangleIndices.Add(first + 1);
                mesh.TriangleIndices.Add(first + 2);
                mesh.TriangleIndices.Add(first + 3);
            }
            return mesh;
        }

        static void UpdateLightningRibbon(
            MeshGeometry3D mesh,
            Point3D start,
            Point3D end,
            int segments,
            double seed,
            double width)
        {
            var positions = new Point3DCollection((segments + 1) * 2);
            Vector3D direction = end - start;
            Vector3D perpendicular = new Vector3D(-direction.Y, direction.X, 0);
            if (perpendicular.LengthSquared < 0.001)
                perpendicular = new Vector3D(1, 0, 0);
            perpendicular.Normalize();

            for (int i = 0; i <= segments; i++)
            {
                double amount = i / (double)segments;
                Point3D point = LerpPoint(start, end, amount);
                if (i > 0 && i < segments)
                {
                    double taper = Math.Sin(amount * Math.PI);
                    double jitter = (PseudoRandom(seed + i * 3.17) * 2 - 1) * 0.15 * taper;
                    point += perpendicular * jitter;
                    point.Z += (PseudoRandom(seed * 2.1 + i) * 2 - 1) * 0.09 * taper;
                }
                positions.Add(point + new Vector3D(0, 0, width));
                positions.Add(point - new Vector3D(0, 0, width));
            }
            mesh.Positions = positions;
        }

        static MeshGeometry3D CreateCrystalGeometry()
        {
            var mesh = new MeshGeometry3D();
            Point3D baseA = new Point3D(0, -0.11, -0.08);
            Point3D baseB = new Point3D(0, 0.11, -0.08);
            Point3D baseC = new Point3D(0, 0, 0.13);
            Point3D tip = new Point3D(0.72, 0, 0);
            AddTriangle(mesh, baseA, baseB, tip);
            AddTriangle(mesh, baseB, baseC, tip);
            AddTriangle(mesh, baseC, baseA, tip);
            AddTriangle(mesh, baseC, baseB, baseA);
            mesh.Freeze();
            return mesh;
        }

        static void AddTriangle(MeshGeometry3D mesh, Point3D a, Point3D b, Point3D c)
        {
            Vector3D normal = Vector3D.CrossProduct(b - a, c - a);
            normal.Normalize();
            int first = mesh.Positions.Count;
            mesh.Positions.Add(a);
            mesh.Positions.Add(b);
            mesh.Positions.Add(c);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.TriangleIndices.Add(first);
            mesh.TriangleIndices.Add(first + 1);
            mesh.TriangleIndices.Add(first + 2);
        }

        static void UpdateUnwindRingGeometry(MeshGeometry3D mesh, double amount)
        {
            const int aroundSegments = 48;
            const int tubeSegments = 16;
            const double epsilon = 0.001;
            var positions = new Point3DCollection((aroundSegments + 1) * (tubeSegments + 1));
            var normals = new Vector3DCollection((aroundSegments + 1) * (tubeSegments + 1));

            for (int i = 0; i <= aroundSegments; i++)
            {
                double theta = 2 * Math.PI * i / aroundSegments;
                Point3D center = UnwindCenter(theta, amount);
                Point3D before = UnwindCenter(theta - epsilon, amount);
                Point3D after = UnwindCenter(theta + epsilon, amount);
                Vector3D tangent = after - before;
                tangent.Normalize();
                Vector3D normalA = Vector3D.CrossProduct(new Vector3D(0, 0, 1), tangent);
                if (normalA.LengthSquared < 0.001)
                    normalA = new Vector3D(1, 0, 0);
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

        static Point3D UnwindCenter(double theta, double amount)
        {
            Point3D circle = new Point3D(Math.Cos(theta), Math.Sin(theta), 0);
            double normalized = theta / (2 * Math.PI);
            Point3D ribbon = new Point3D(
                Lerp(-1.28, 1.28, normalized),
                Math.Sin(theta * 1.5) * 0.64,
                Math.Cos(theta * 2) * 0.44);
            return LerpPoint(circle, ribbon, amount);
        }
    }
}

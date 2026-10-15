using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace m0.UIWpf.Forms
{
    sealed class AboutLogoScene : IDisposable
    {
        const double RingMajor = 1.0;
        const double RingMinor = 0.18;
        const double BarRadius = 0.155;
        const double BarCylinderLength = 1.72;
        const double BarRestY = 1.43;

        static readonly Color RestGlow = Color.FromRgb(16, 58, 120);
        static readonly Color SpinGlow = Color.FromRgb(78, 160, 240);

        readonly Stopwatch _clock = new Stopwatch();
        readonly Random _random = new Random();
        ScaleTransform3D _groupScale;
        AxisAngleRotation3D _groupPitch;
        AxisAngleRotation3D _groupYaw;
        TranslateTransform3D _barMove;
        PointLight _point;
        PointLight _glint;
        PerspectiveCamera _camera;
        PieceSpin _ringSpin;
        PieceSpin _barSpin;
        double _heartAt = 3.4;
        bool _heartOn;
        double _heartStart;
        double _punchUntil = -1;
        double _pointerX = 0.5;
        double _pointerY = 0.34;
        double _yaw;
        double _pitch = 16;
        double _lightX;
        double _lightY = 0.7;
        bool _disposed;

        public void SetPointer(double x, double y)
        {
            _pointerX = Clamp01(x);
            _pointerY = Clamp01(y);
        }

        public void Attach(Viewport3D view)
        {
            var blue = Color.FromRgb(80, 165, 255);
            var diffuse = new DiffuseMaterial(new SolidColorBrush(blue));
            var specular = new SpecularMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 255)), 72);

            _groupScale = new ScaleTransform3D(0.9, 0.9, 0.9);
            _groupPitch = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 16);
            _groupYaw = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            var markTransform = new Transform3DGroup();
            markTransform.Children.Add(_groupScale);
            markTransform.Children.Add(new RotateTransform3D(_groupPitch));
            markTransform.Children.Add(new RotateTransform3D(_groupYaw));

            _ringSpin = CreatePieceSpin();
            var ringMaterial = PieceMaterial(diffuse, specular, _ringSpin.Emissive);
            var ringTransform = new Transform3DGroup();
            ringTransform.Children.Add(_ringSpin.Scale);
            ringTransform.Children.Add(new RotateTransform3D(_ringSpin.Rotation));
            var ring = new GeometryModel3D(CreateTorus(RingMajor, RingMinor, 96, 32), ringMaterial)
            {
                BackMaterial = ringMaterial,
                Transform = ringTransform
            };
            Arm(_ringSpin, 0, true);

            _barSpin = CreatePieceSpin();
            var barMaterial = PieceMaterial(diffuse, specular, _barSpin.Emissive);
            _barMove = new TranslateTransform3D(0, BarRestY, 0);
            var barTransform = new Transform3DGroup();
            barTransform.Children.Add(_barSpin.Scale);
            barTransform.Children.Add(new RotateTransform3D(_barSpin.Rotation));
            barTransform.Children.Add(_barMove);
            var bar = new GeometryModel3D(CreateCapsule(BarRadius, BarCylinderLength, 28, 8), barMaterial)
            {
                BackMaterial = barMaterial,
                Transform = barTransform
            };
            Arm(_barSpin, 0, true);

            var mark = new Model3DGroup { Transform = markTransform };
            mark.Children.Add(ring);
            mark.Children.Add(bar);

            _point = new PointLight(Color.FromRgb(255, 255, 255), new Point3D(0, 0.7, 2.15))
            {
                Range = 14,
                ConstantAttenuation = 0.25,
                LinearAttenuation = 0.05
            };

            _glint = new PointLight(Color.FromRgb(210, 236, 255), new Point3D(1.6, 0.3, 0.8))
            {
                Range = 8,
                ConstantAttenuation = 0.85,
                LinearAttenuation = 0.22
            };

            var lights = new Model3DGroup();
            lights.Children.Add(new AmbientLight(Color.FromRgb(72, 92, 124)));
            lights.Children.Add(new DirectionalLight(Color.FromRgb(150, 176, 214), new Vector3D(0.35, -0.55, -1)));
            lights.Children.Add(_point);
            lights.Children.Add(_glint);

            _camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0.2, 5.25),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 30,
                NearPlaneDistance = 0.05,
                FarPlaneDistance = 30
            };
            view.Camera = _camera;
            view.Children.Clear();
            view.Children.Add(new ModelVisual3D { Content = lights });
            view.Children.Add(new ModelVisual3D { Content = mark });

            Apply(0);
            _clock.Start();
            CompositionTarget.Rendering += OnRendering;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            CompositionTarget.Rendering -= OnRendering;
            _clock.Stop();
        }

        void OnRendering(object sender, EventArgs e)
        {
            if (_disposed)
                return;

            Apply(_clock.Elapsed.TotalSeconds);
        }

        void Apply(double t)
        {
            double settle = EaseOutCubic(Clamp01(t / 0.45));
            double scale = Lerp(0.92, 1, settle);
            _groupScale.ScaleX = scale;
            _groupScale.ScaleY = scale;
            _groupScale.ScaleZ = scale;

            double aimYaw = (_pointerX - 0.5) * -16;
            double aimPitch = (_pointerY - 0.5) * 10;
            double swayYaw = Math.Sin(t * 0.45) * 5;
            double swayPitch = Math.Cos(t * 0.33) * 2.5;
            _yaw += (aimYaw + swayYaw - _yaw) * 0.07;
            _pitch += (14 + aimPitch + swayPitch - _pitch) * 0.07;
            _groupYaw.Angle = _yaw;
            _groupPitch.Angle = _pitch;

            _barMove.OffsetY = BarRestY + Math.Sin(t * 1.15) * 0.02;
            if (Advance(_ringSpin, t) || Advance(_barSpin, t))
                _punchUntil = t + 0.42;
            UpdateHeart(t);

            double targetX = (_pointerX - 0.5) * 3.6;
            double targetY = (0.4 - _pointerY) * 2.8 + 0.35;
            _lightX += (targetX - _lightX) * 0.16;
            _lightY += (targetY - _lightY) * 0.16;
            _point.Position = new Point3D(_lightX, _lightY, 2.05);

            double glint = t * 1.15;
            _glint.Position = new Point3D(
                Math.Cos(glint) * 1.65,
                0.28 + Math.Sin(glint * 2) * 0.55,
                Math.Sin(glint) * 1.25 + 0.55);

            double punch = 0;
            if (t < _punchUntil)
                punch = Math.Sin(Clamp01(1 - (_punchUntil - t) / 0.42) * Math.PI);
            _camera.Position = new Point3D(0, 0.2, 5.25 - punch * 0.55);
        }

        void UpdateHeart(double t)
        {
            if (_ringSpin.Going)
            {
                _heartOn = false;
                if (t >= _heartAt)
                    _heartAt = t + 0.5;
                return;
            }

            if (!_heartOn)
            {
                if (t < _heartAt)
                    return;
                _heartOn = true;
                _heartStart = t;
            }

            double u = (t - _heartStart) / 0.64;
            if (u >= 1)
            {
                _heartOn = false;
                _heartAt = t + 6 + _random.NextDouble() * 6;
                _ringSpin.Scale.ScaleX = _ringSpin.Scale.ScaleY = _ringSpin.Scale.ScaleZ = 1;
                return;
            }

            double lub = Math.Exp(-Math.Pow((u - 0.22) / 0.09, 2));
            double dub = Math.Exp(-Math.Pow((u - 0.48) / 0.07, 2));
            double scale = 1 + 0.12 * lub + 0.07 * dub;
            _ringSpin.Scale.ScaleX = _ringSpin.Scale.ScaleY = _ringSpin.Scale.ScaleZ = scale;
        }

        static PieceSpin CreatePieceSpin()
        {
            return new PieceSpin
            {
                Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0),
                Scale = new ScaleTransform3D(1, 1, 1),
                Emissive = new SolidColorBrush(RestGlow)
            };
        }

        static MaterialGroup PieceMaterial(DiffuseMaterial diffuse, SpecularMaterial specular, SolidColorBrush emissive)
        {
            var material = new MaterialGroup();
            material.Children.Add(diffuse);
            material.Children.Add(specular);
            material.Children.Add(new EmissiveMaterial(emissive));
            return material;
        }

        void Arm(PieceSpin spin, double now, bool first)
        {
            spin.Going = false;
            spin.Rotation.Angle = 0;
            spin.Scale.ScaleX = spin.Scale.ScaleY = spin.Scale.ScaleZ = 1;
            spin.Emissive.Color = RestGlow;
            double wait = first
                ? 1.1 + _random.NextDouble() * 1.8
                : 4 + _random.NextDouble() * 10;
            if (spin == _barSpin && first)
                wait += 1.6;
            spin.NextAt = now + wait;
        }

        bool Advance(PieceSpin spin, double t)
        {
            if (!spin.Going)
            {
                if (t < spin.NextAt)
                    return false;

                spin.Going = true;
                spin.StartedAt = t;
                spin.Turns = _random.NextDouble() < 0.2 ? 2 : 1;
                spin.Direction = _random.NextDouble() < 0.5 ? -1 : 1;
                spin.Duration = (0.62 + _random.NextDouble() * 0.7) * spin.Turns;
                spin.Rotation.Axis = RandomAxis();
                spin.Rotation.Angle = 0;
            }

            double u = (t - spin.StartedAt) / spin.Duration;
            if (u >= 1)
            {
                Arm(spin, t, false);
                return true;
            }

            double eased = EaseInOut(Clamp01(u));
            spin.Rotation.Angle = spin.Direction * 360 * spin.Turns * eased;
            double pulse = Math.Sin(u * Math.PI);
            double scale = 1 + pulse * 0.16;
            spin.Scale.ScaleX = spin.Scale.ScaleY = spin.Scale.ScaleZ = scale;
            spin.Emissive.Color = LerpColor(RestGlow, SpinGlow, pulse);
            return false;
        }

        Vector3D RandomAxis()
        {
            var axis = new Vector3D(
                _random.NextDouble() * 2 - 1,
                _random.NextDouble() * 2 - 1,
                _random.NextDouble() * 2 - 1);
            if (axis.LengthSquared < 0.04)
                axis = new Vector3D(0.2, 1, 0.15);
            axis.Normalize();
            return axis;
        }

        static MeshGeometry3D CreateTorus(double major, double minor, int thetaSeg, int phiSeg)
        {
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= thetaSeg; i++)
            {
                double theta = 2 * Math.PI * i / thetaSeg;
                double ct = Math.Cos(theta);
                double st = Math.Sin(theta);

                for (int j = 0; j <= phiSeg; j++)
                {
                    double phi = 2 * Math.PI * j / phiSeg;
                    double cp = Math.Cos(phi);
                    double sp = Math.Sin(phi);
                    mesh.Positions.Add(new Point3D((major + minor * cp) * ct, (major + minor * cp) * st, minor * sp));
                    mesh.Normals.Add(new Vector3D(cp * ct, cp * st, sp));
                }
            }

            int stride = phiSeg + 1;
            for (int i = 0; i < thetaSeg; i++)
            {
                for (int j = 0; j < phiSeg; j++)
                {
                    int a = i * stride + j;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;
                    mesh.TriangleIndices.Add(a);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(b);
                    mesh.TriangleIndices.Add(c);
                    mesh.TriangleIndices.Add(d);
                }
            }

            mesh.Freeze();
            return mesh;
        }

        static MeshGeometry3D CreateCapsule(double radius, double cylinderLength, int around, int capRings)
        {
            var mesh = new MeshGeometry3D();
            double half = cylinderLength / 2.0;

            for (int i = 0; i <= around; i++)
            {
                double ang = 2 * Math.PI * i / around;
                double cy = Math.Cos(ang);
                double cz = Math.Sin(ang);
                var normal = new Vector3D(0, cy, cz);
                mesh.Positions.Add(new Point3D(-half, radius * cy, radius * cz));
                mesh.Normals.Add(normal);
                mesh.Positions.Add(new Point3D(half, radius * cy, radius * cz));
                mesh.Normals.Add(normal);
            }

            for (int i = 0; i < around; i++)
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

            AddHemisphere(mesh, radius, half, around, capRings, true);
            AddHemisphere(mesh, radius, half, around, capRings, false);
            mesh.Freeze();
            return mesh;
        }

        static void AddHemisphere(MeshGeometry3D mesh, double radius, double half, int around, int capRings, bool positive)
        {
            int start = mesh.Positions.Count;
            int stride = around + 1;
            double sign = positive ? 1.0 : -1.0;

            for (int i = 0; i <= capRings; i++)
            {
                double phi = (Math.PI / 2.0) * i / capRings;
                double sp = Math.Sin(phi);
                double cp = Math.Cos(phi);

                for (int j = 0; j <= around; j++)
                {
                    double theta = 2 * Math.PI * j / around;
                    double ct = Math.Cos(theta);
                    double st = Math.Sin(theta);
                    mesh.Normals.Add(new Vector3D(sign * sp, cp * ct, cp * st));
                    mesh.Positions.Add(new Point3D(sign * (half + radius * sp), radius * cp * ct, radius * cp * st));
                }
            }

            for (int i = 0; i < capRings; i++)
            {
                for (int j = 0; j < around; j++)
                {
                    int a = start + i * stride + j;
                    int thetaN = a + 1;
                    int phiN = a + stride;
                    int both = phiN + 1;

                    if (positive)
                    {
                        mesh.TriangleIndices.Add(a);
                        mesh.TriangleIndices.Add(thetaN);
                        mesh.TriangleIndices.Add(phiN);
                        mesh.TriangleIndices.Add(thetaN);
                        mesh.TriangleIndices.Add(both);
                        mesh.TriangleIndices.Add(phiN);
                    }
                    else
                    {
                        mesh.TriangleIndices.Add(a);
                        mesh.TriangleIndices.Add(phiN);
                        mesh.TriangleIndices.Add(thetaN);
                        mesh.TriangleIndices.Add(phiN);
                        mesh.TriangleIndices.Add(both);
                        mesh.TriangleIndices.Add(thetaN);
                    }
                }
            }
        }

        static double Clamp01(double value)
        {
            if (value < 0)
                return 0;
            if (value > 1)
                return 1;
            return value;
        }

        static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        static double EaseOutCubic(double t)
        {
            double u = 1 - t;
            return 1 - u * u * u;
        }

        static double EaseInOut(double t)
        {
            if (t < 0.5)
                return 4 * t * t * t;
            double u = -2 * t + 2;
            return 1 - u * u * u / 2;
        }

        static Color LerpColor(Color a, Color b, double t)
        {
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }

        sealed class PieceSpin
        {
            public AxisAngleRotation3D Rotation;
            public ScaleTransform3D Scale;
            public SolidColorBrush Emissive;
            public double NextAt;
            public double StartedAt;
            public double Duration;
            public double Turns;
            public double Direction;
            public bool Going;
        }
    }
}

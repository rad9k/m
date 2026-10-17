using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace m0.UIWpf.Forms
{
    sealed partial class AboutLogoScene
    {
        enum HeroEffectKind
        {
            None,
            Shatter,
            Spikes,
            Morph,
            BarOrbit,
            Plane,
            Droplets,
            Glitch,
            CameraPortal,
            Tesla,
            Iris,
            Crystal,
            Unwind,
            PortalTunnel,
            Shockwave,
            HologramScan,
            Comets,
            MirrorDimension,
            RoleSwap,
            PolygonForge,
            FractalLogos,
            Mitosis,
            Dna,
            Voxels,
            AtomMachine,
            Hypercube,
            SecretFace
        }

        sealed class EffectVisual
        {
            public GeometryModel3D Model;
            public ScaleTransform3D Scale;
            public AxisAngleRotation3D Rotation;
            public TranslateTransform3D Move;
            public SolidColorBrush Diffuse;
            public SolidColorBrush Emissive;
            public Point3D Home;
            public Vector3D Direction;
            public Vector3D SpinAxis;
            public double Phase;
            public double Speed;
            public double Strength;
        }

        readonly List<HeroEffectKind> _effectBag = new List<HeroEffectKind>();
        readonly List<EffectVisual> _effectVisuals = new List<EffectVisual>();
        HeroEffectKind _activeHeroEffect;
        int _effectBagIndex;
        double _heroStartedAt;
        double _heroDuration;
        double _nextHeroAt;
        double _secretFaceAt;
        int _nextEffectRequests;
        MeshGeometry3D _dynamicRingGeometry;

        bool HeroEffectActive
        {
            get { return _activeHeroEffect != HeroEffectKind.None; }
        }

        public void TriggerNextEffect()
        {
            _nextEffectRequests++;
        }

        void InitializeHeroDirector()
        {
            RefillEffectBag();
            _nextHeroAt = 1.5;
            _secretFaceAt = 34 + _random.NextDouble() * 22;
        }

        void DisposeHeroDirector()
        {
            if (_heroRoot != null)
                _heroRoot.Children.Clear();
            _effectVisuals.Clear();
        }

        void UpdateHeroDirector(double time)
        {
            if (_nextEffectRequests > 0)
            {
                _nextEffectRequests--;
                if (_activeHeroEffect != HeroEffectKind.None)
                {
                    ResetHeroState();
                    _activeHeroEffect = HeroEffectKind.None;
                }
                BeginHeroEffect(NextHeroEffect(), time);
            }
            else if (_activeHeroEffect == HeroEffectKind.None)
            {
                if (_ringSpin.Going || _barSpin.Going || _heartOn || time < _punchUntil)
                {
                    if (time >= _nextHeroAt)
                        _nextHeroAt = time + 0.2;
                    return;
                }

                if (time >= _secretFaceAt)
                    BeginHeroEffect(HeroEffectKind.SecretFace, time);
                else if (time >= _nextHeroAt)
                    BeginHeroEffect(NextHeroEffect(), time);
                else
                    return;
            }

            double progress = (time - _heroStartedAt) / _heroDuration;
            if (progress >= 1)
            {
                FinishHeroEffect(time);
                return;
            }

            progress = Clamp01(progress);
            switch (_activeHeroEffect)
            {
                case HeroEffectKind.Shatter:
                    UpdateShatter(progress, time);
                    break;
                case HeroEffectKind.Spikes:
                    UpdateSpikes(progress, time);
                    break;
                case HeroEffectKind.Morph:
                    UpdateMorph(progress);
                    break;
                case HeroEffectKind.BarOrbit:
                    UpdateBarOrbit(progress);
                    break;
                case HeroEffectKind.Plane:
                    UpdatePlane(progress);
                    break;
                case HeroEffectKind.Droplets:
                    UpdateDroplets(progress, time);
                    break;
                case HeroEffectKind.Glitch:
                    UpdateGlitch(progress, time);
                    break;
                case HeroEffectKind.CameraPortal:
                    UpdateCameraPortal(progress);
                    break;
                case HeroEffectKind.Tesla:
                    UpdateTesla(progress, time);
                    break;
                case HeroEffectKind.Iris:
                    UpdateIris(progress);
                    break;
                case HeroEffectKind.Crystal:
                    UpdateCrystal(progress, time);
                    break;
                case HeroEffectKind.Unwind:
                    UpdateUnwind(progress);
                    break;
                case HeroEffectKind.PortalTunnel:
                    UpdatePortalTunnel(progress);
                    break;
                case HeroEffectKind.Shockwave:
                    UpdateShockwave(progress);
                    break;
                case HeroEffectKind.HologramScan:
                    UpdateHologramScan(progress, time);
                    break;
                case HeroEffectKind.Comets:
                    UpdateComets(progress);
                    break;
                case HeroEffectKind.MirrorDimension:
                    UpdateMirrorDimension(progress);
                    break;
                case HeroEffectKind.RoleSwap:
                    UpdateRoleSwap(progress);
                    break;
                case HeroEffectKind.PolygonForge:
                    UpdatePolygonForge(progress);
                    break;
                case HeroEffectKind.FractalLogos:
                    UpdateFractalLogos(progress, time);
                    break;
                case HeroEffectKind.Mitosis:
                    UpdateMitosis(progress);
                    break;
                case HeroEffectKind.Dna:
                    UpdateDna(progress, time);
                    break;
                case HeroEffectKind.Voxels:
                    UpdateVoxels(progress);
                    break;
                case HeroEffectKind.AtomMachine:
                    UpdateAtomMachine(progress);
                    break;
                case HeroEffectKind.Hypercube:
                    UpdateHypercube(progress);
                    break;
                case HeroEffectKind.SecretFace:
                    UpdateSecretFace(progress);
                    break;
            }
        }

        void RefillEffectBag()
        {
            _effectBag.Clear();
            _effectBag.Add(HeroEffectKind.Shatter);
            _effectBag.Add(HeroEffectKind.Spikes);
            _effectBag.Add(HeroEffectKind.Morph);
            _effectBag.Add(HeroEffectKind.BarOrbit);
            _effectBag.Add(HeroEffectKind.Plane);
            _effectBag.Add(HeroEffectKind.Droplets);
            _effectBag.Add(HeroEffectKind.Glitch);
            _effectBag.Add(HeroEffectKind.CameraPortal);
            _effectBag.Add(HeroEffectKind.Tesla);
            _effectBag.Add(HeroEffectKind.Iris);
            _effectBag.Add(HeroEffectKind.Crystal);
            _effectBag.Add(HeroEffectKind.Unwind);
            _effectBag.Add(HeroEffectKind.PortalTunnel);
            _effectBag.Add(HeroEffectKind.Shockwave);
            _effectBag.Add(HeroEffectKind.HologramScan);
            _effectBag.Add(HeroEffectKind.Comets);
            _effectBag.Add(HeroEffectKind.MirrorDimension);
            _effectBag.Add(HeroEffectKind.RoleSwap);
            _effectBag.Add(HeroEffectKind.PolygonForge);
            _effectBag.Add(HeroEffectKind.FractalLogos);
            _effectBag.Add(HeroEffectKind.Mitosis);
            _effectBag.Add(HeroEffectKind.Dna);
            _effectBag.Add(HeroEffectKind.Voxels);
            _effectBag.Add(HeroEffectKind.AtomMachine);
            _effectBag.Add(HeroEffectKind.Hypercube);

            for (int i = _effectBag.Count - 1; i > 0; i--)
            {
                int other = _random.Next(i + 1);
                HeroEffectKind temporary = _effectBag[i];
                _effectBag[i] = _effectBag[other];
                _effectBag[other] = temporary;
            }

            _effectBagIndex = 0;
        }

        HeroEffectKind NextHeroEffect()
        {
            if (_effectBagIndex >= _effectBag.Count)
                RefillEffectBag();
            return _effectBag[_effectBagIndex++];
        }

        void BeginHeroEffect(HeroEffectKind effect, double time)
        {
            ResetHeroState();
            Arm(_ringSpin, time, false);
            Arm(_barSpin, time, false);
            _activeHeroEffect = effect;
            _heroStartedAt = time;
            _heartOn = false;

            switch (effect)
            {
                case HeroEffectKind.Shatter:
                    _heroDuration = 2.75;
                    SetupShatter();
                    break;
                case HeroEffectKind.Spikes:
                    _heroDuration = 2.35;
                    SetupSpikes();
                    break;
                case HeroEffectKind.Morph:
                    _heroDuration = 3.2;
                    SetupMorph();
                    break;
                case HeroEffectKind.BarOrbit:
                    _heroDuration = 2.65;
                    SetupBarOrbit();
                    break;
                case HeroEffectKind.Plane:
                    _heroDuration = 2.6;
                    SetupPlane();
                    break;
                case HeroEffectKind.Droplets:
                    _heroDuration = 2.85;
                    SetupDroplets();
                    break;
                case HeroEffectKind.Glitch:
                    _heroDuration = 1.65;
                    SetupGlitch();
                    break;
                case HeroEffectKind.CameraPortal:
                    _heroDuration = 2.9;
                    break;
                case HeroEffectKind.Tesla:
                    _heroDuration = 2.3;
                    SetupTesla();
                    break;
                case HeroEffectKind.Iris:
                    _heroDuration = 2.55;
                    SetupIris();
                    break;
                case HeroEffectKind.Crystal:
                    _heroDuration = 2.85;
                    SetupCrystal();
                    break;
                case HeroEffectKind.Unwind:
                    _heroDuration = 3.15;
                    SetupUnwind();
                    break;
                case HeroEffectKind.PortalTunnel:
                    _heroDuration = 2.8;
                    SetupPortalTunnel();
                    break;
                case HeroEffectKind.Shockwave:
                    _heroDuration = 2.35;
                    SetupShockwave();
                    break;
                case HeroEffectKind.HologramScan:
                    _heroDuration = 2.75;
                    SetupHologramScan();
                    break;
                case HeroEffectKind.Comets:
                    _heroDuration = 3.0;
                    SetupComets();
                    break;
                case HeroEffectKind.MirrorDimension:
                    _heroDuration = 2.9;
                    SetupMirrorDimension();
                    break;
                case HeroEffectKind.RoleSwap:
                    _heroDuration = 3.0;
                    SetupRoleSwap();
                    break;
                case HeroEffectKind.PolygonForge:
                    _heroDuration = 3.35;
                    SetupPolygonForge();
                    break;
                case HeroEffectKind.FractalLogos:
                    _heroDuration = 3.1;
                    SetupFractalLogos();
                    break;
                case HeroEffectKind.Mitosis:
                    _heroDuration = 3.0;
                    SetupMitosis();
                    break;
                case HeroEffectKind.Dna:
                    _heroDuration = 3.25;
                    SetupDna();
                    break;
                case HeroEffectKind.Voxels:
                    _heroDuration = 3.25;
                    SetupVoxels();
                    break;
                case HeroEffectKind.AtomMachine:
                    _heroDuration = 3.2;
                    SetupAtomMachine();
                    break;
                case HeroEffectKind.Hypercube:
                    _heroDuration = 3.45;
                    SetupHypercube();
                    break;
                case HeroEffectKind.SecretFace:
                    _heroDuration = 3.1;
                    SetupSecretFace();
                    break;
            }
        }

        void FinishHeroEffect(double time)
        {
            bool wasSecret = _activeHeroEffect == HeroEffectKind.SecretFace;
            ResetHeroState();
            _activeHeroEffect = HeroEffectKind.None;
            _nextHeroAt = time + 2.4 + _random.NextDouble() * 3.4;
            if (wasSecret)
                _secretFaceAt = time + 42 + _random.NextDouble() * 35;
        }

        void ResetHeroState()
        {
            if (_heroRoot != null)
                _heroRoot.Children.Clear();
            _effectVisuals.Clear();
            _dynamicRingGeometry = null;

            if (_ringModel != null)
                _ringModel.Geometry = _baseRingGeometry;

            ResetTransform(_ringHeroScale, _ringHeroRotation, _ringHeroMove);
            ResetTransform(_barHeroScale, _barHeroRotation, _barHeroMove);

            if (_groupRoll != null)
                _groupRoll.Angle = 0;
            if (_groupMove != null)
            {
                _groupMove.OffsetX = 0;
                _groupMove.OffsetY = 0;
                _groupMove.OffsetZ = 0;
            }

            if (_point != null)
                _point.Color = Color.FromRgb(176, 198, 228);
            if (_glint != null)
                _glint.Color = Color.FromRgb(88, 155, 210);
            if (_ringSpin != null)
                _ringSpin.Emissive.Color = RestGlow;
            if (_barSpin != null)
                _barSpin.Emissive.Color = RestGlow;
        }

        static void ResetTransform(ScaleTransform3D scale, AxisAngleRotation3D rotation, TranslateTransform3D move)
        {
            if (scale != null)
                scale.ScaleX = scale.ScaleY = scale.ScaleZ = 1;
            if (rotation != null)
            {
                rotation.Axis = new Vector3D(0, 1, 0);
                rotation.Angle = 0;
            }
            if (move != null)
            {
                move.OffsetX = 0;
                move.OffsetY = 0;
                move.OffsetZ = 0;
            }
        }

        EffectVisual AddEffectVisual(
            MeshGeometry3D geometry,
            Color color,
            Color emissiveColor,
            double opacity)
        {
            var diffuseBrush = new SolidColorBrush(color) { Opacity = opacity };
            var emissiveBrush = new SolidColorBrush(emissiveColor) { Opacity = opacity };
            var material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(diffuseBrush));
            material.Children.Add(new SpecularMaterial(
                new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), 225, 238, 255)), 64));
            material.Children.Add(new EmissiveMaterial(emissiveBrush));

            var visual = new EffectVisual
            {
                Scale = new ScaleTransform3D(1, 1, 1),
                Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0),
                Move = new TranslateTransform3D(),
                Diffuse = diffuseBrush,
                Emissive = emissiveBrush
            };

            var transform = new Transform3DGroup();
            transform.Children.Add(visual.Scale);
            transform.Children.Add(new RotateTransform3D(visual.Rotation));
            transform.Children.Add(visual.Move);

            visual.Model = new GeometryModel3D(geometry, material)
            {
                BackMaterial = material,
                Transform = transform
            };
            _heroRoot.Children.Add(visual.Model);
            _effectVisuals.Add(visual);
            return visual;
        }

        void SetupShatter()
        {
            const int ringParts = 20;
            for (int i = 0; i < ringParts; i++)
            {
                double start = 2 * Math.PI * i / ringParts;
                double end = 2 * Math.PI * (i + 1) / ringParts;
                Point3D center;
                MeshGeometry3D geometry = CreateTorusArc(start, end, 5, 14, out center);
                EffectVisual fragment = AddEffectVisual(
                    geometry,
                    Color.FromRgb(68, 132, 200),
                    Color.FromRgb(14, 38, 82),
                    1);
                fragment.Home = center;
                fragment.Direction = RandomBurstDirection(center);
                fragment.SpinAxis = RandomAxis();
                fragment.Speed = 150 + _random.NextDouble() * 310;
                fragment.Strength = 0.55 + _random.NextDouble() * 0.8;
                fragment.Phase = _random.NextDouble() * Math.PI * 2;
                SetMove(fragment, center);
            }

            const int barParts = 5;
            for (int i = 0; i < barParts; i++)
            {
                double x = -0.82 + i * 0.41;
                EffectVisual fragment = AddEffectVisual(
                    CreateCapsule(0.13, 0.17, 18, 5),
                    Color.FromRgb(68, 132, 200),
                    Color.FromRgb(14, 38, 82),
                    1);
                fragment.Home = new Point3D(x, BarRestY, 0);
                fragment.Direction = RandomBurstDirection(fragment.Home);
                fragment.SpinAxis = RandomAxis();
                fragment.Speed = 180 + _random.NextDouble() * 360;
                fragment.Strength = 0.65 + _random.NextDouble() * 0.9;
                fragment.Phase = _random.NextDouble() * Math.PI * 2;
                SetMove(fragment, fragment.Home);
            }
        }

        void UpdateShatter(double progress, double time)
        {
            double explosion;
            if (progress < 0.26)
                explosion = EaseOutCubic(progress / 0.26);
            else if (progress < 0.54)
                explosion = 1;
            else
                explosion = 1 - EaseInOut((progress - 0.54) / 0.46);

            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = 0.001;
            _barHeroScale.ScaleX = _barHeroScale.ScaleY = _barHeroScale.ScaleZ = 0.001;

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual fragment = _effectVisuals[i];
                double hover = progress > 0.26 && progress < 0.56
                    ? Math.Sin(time * 4.5 + fragment.Phase) * 0.08
                    : 0;
                Vector3D offset = fragment.Direction * (fragment.Strength * explosion + hover);
                fragment.Move.OffsetX = fragment.Home.X + offset.X;
                fragment.Move.OffsetY = fragment.Home.Y + offset.Y;
                fragment.Move.OffsetZ = fragment.Home.Z + offset.Z;
                fragment.Rotation.Axis = fragment.SpinAxis;
                fragment.Rotation.Angle = fragment.Speed * explosion;
                double fragmentScale = 0.78 + 0.22 * (1 - explosion);
                fragment.Scale.ScaleX = fragment.Scale.ScaleY = fragment.Scale.ScaleZ = fragmentScale;
            }

            double flash = Math.Sin(Clamp01(progress / 0.26) * Math.PI);
            _glint.Color = LerpColor(Color.FromRgb(88, 155, 210), Color.FromRgb(220, 242, 255), flash);
            _camera.Position = new Point3D(0, 0.2, 5.25 - flash * 0.38);
        }

        void SetupSpikes()
        {
            const int spikeCount = 28;
            for (int i = 0; i < spikeCount; i++)
            {
                double angle = 2 * Math.PI * i / spikeCount;
                EffectVisual spike = AddEffectVisual(
                    CreateCone(0.105, 0.58, 12),
                    i % 3 == 0
                        ? Color.FromRgb(36, 92, 168)
                        : i % 3 == 1
                            ? Color.FromRgb(66, 142, 226)
                            : Color.FromRgb(132, 202, 255),
                    i % 2 == 0 ? Color.FromRgb(18, 68, 150) : Color.FromRgb(40, 116, 218),
                    0.96);
                spike.Home = new Point3D(
                    Math.Cos(angle) * (RingMajor + RingMinor * 0.7),
                    Math.Sin(angle) * (RingMajor + RingMinor * 0.7),
                    0);
                spike.Rotation.Axis = new Vector3D(0, 0, 1);
                spike.Rotation.Angle = angle * 180 / Math.PI;
                spike.Phase = i / (double)spikeCount;
                SetMove(spike, spike.Home);
                spike.Scale.ScaleX = 0.001;
                spike.Scale.ScaleY = spike.Scale.ScaleZ = 0.55;
            }
        }

        void UpdateSpikes(double progress, double time)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.65);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual spike = _effectVisuals[i];
                double wave = 0.5 + 0.5 * Math.Sin(time * 8.5 - spike.Phase * Math.PI * 6);
                double length = envelope * (0.28 + wave * 0.92);
                spike.Scale.ScaleX = Math.Max(0.001, length);
                spike.Scale.ScaleY = spike.Scale.ScaleZ = 0.45 + envelope * 0.45;
                spike.Emissive.Opacity = 0.45 + wave * 0.55;
            }

            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ =
                1 + envelope * (0.035 + Math.Sin(time * 8) * 0.025);
            _barHeroMove.OffsetY = envelope * 0.11;
            _glint.Color = LerpColor(
                Color.FromRgb(88, 155, 210),
                Color.FromRgb(150, 218, 255),
                envelope * (0.5 + 0.5 * Math.Sin(time * 4)));
        }

        void SetupMorph()
        {
            _dynamicRingGeometry = CreateDynamicRingGeometry(48, 16);
            _ringModel.Geometry = _dynamicRingGeometry;
        }

        void UpdateMorph(double progress)
        {
            double strength = Math.Sin(progress * Math.PI);
            double targetBlend = EaseInOut(Clamp01((progress - 0.30) / 0.42));
            UpdateMorphRingGeometry(_dynamicRingGeometry, strength, targetBlend);
            _barHeroScale.ScaleX = 1 - strength * 0.18;
            _barHeroScale.ScaleY = 1 + strength * 0.32;
            _barHeroScale.ScaleZ = 1 + strength * 0.32;
            _barHeroRotation.Axis = new Vector3D(0, 1, 0);
            _barHeroRotation.Angle = Math.Sin(progress * Math.PI * 2) * strength * 28;
            _ringSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(74, 24, 142), strength);
        }

        void SetupBarOrbit()
        {
            MeshGeometry3D geometry = CreateCapsule(BarRadius * 0.9, BarCylinderLength, 24, 6);
            for (int i = 0; i < 7; i++)
            {
                EffectVisual ghost = AddEffectVisual(
                    geometry,
                    Color.FromRgb(62, 128, 205),
                    Color.FromRgb(20, 56, 112),
                    0.30 - i * 0.032);
                ghost.Phase = (i + 1) * 0.085;
            }
        }

        void UpdateBarOrbit(double progress)
        {
            double eased = EaseInOut(progress);
            double angle = Math.PI / 2 + eased * Math.PI * 2;
            SetOrbitingBar(_barHeroMove, _barHeroRotation, angle, eased);

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual ghost = _effectVisuals[i];
                double ghostProgress = Math.Max(0, eased - ghost.Phase);
                double ghostAngle = Math.PI / 2 + ghostProgress * Math.PI * 2;
                Point3D point = OrbitPoint(ghostAngle);
                SetMove(ghost, point);
                ghost.Rotation.Axis = new Vector3D(0.25, 1, 0.15);
                ghost.Rotation.Angle = ghostProgress * 720;
                double appearance = Math.Sin(progress * Math.PI);
                ghost.Scale.ScaleX = ghost.Scale.ScaleY = ghost.Scale.ScaleZ = appearance;
            }

            _ringHeroRotation.Axis = new Vector3D(0.35, 1, 0.2);
            _ringHeroRotation.Angle = Math.Sin(progress * Math.PI) * 85;
        }

        static Point3D OrbitPoint(double angle)
        {
            return new Point3D(
                Math.Cos(angle) * 1.25,
                0.25 + Math.Sin(angle) * 1.18,
                Math.Sin(angle * 2) * 0.46);
        }

        static void SetOrbitingBar(
            TranslateTransform3D move,
            AxisAngleRotation3D rotation,
            double angle,
            double progress)
        {
            Point3D point = OrbitPoint(angle);
            move.OffsetX = point.X;
            move.OffsetY = point.Y - BarRestY;
            move.OffsetZ = point.Z;
            rotation.Axis = new Vector3D(0.25, 1, 0.15);
            rotation.Angle = progress * 720;
        }

        void SetupPlane()
        {
            EffectVisual plane = AddEffectVisual(
                CreatePlaneGeometry(),
                Color.FromRgb(235, 242, 250),
                Color.FromRgb(38, 82, 132),
                1);
            plane.Scale.ScaleX = plane.Scale.ScaleY = plane.Scale.ScaleZ = 0.72;
            plane.Strength = 999;

            MeshGeometry3D particleGeometry = CreateSphere(0.075, 8, 6);
            for (int i = 0; i < 14; i++)
            {
                EffectVisual particle = AddEffectVisual(
                    particleGeometry,
                    Color.FromRgb(96, 182, 255),
                    Color.FromRgb(32, 108, 218),
                    0.62 - i * 0.032);
                particle.Phase = (i + 1) * 0.035;
            }

            _dynamicRingGeometry = CreateDynamicRingGeometry(48, 16);
            _ringModel.Geometry = _dynamicRingGeometry;
        }

        void UpdatePlane(double progress)
        {
            EffectVisual plane = _effectVisuals[0];
            Point3D current = PlanePoint(progress);
            Point3D ahead = PlanePoint(Math.Min(1, progress + 0.012));
            SetMove(plane, current);
            plane.Rotation.Axis = new Vector3D(0, 0, 1);
            plane.Rotation.Angle = Math.Atan2(ahead.Y - current.Y, ahead.X - current.X) * 180 / Math.PI;

            for (int i = 1; i < _effectVisuals.Count; i++)
            {
                EffectVisual particle = _effectVisuals[i];
                if (progress <= particle.Phase)
                {
                    particle.Scale.ScaleX =
                        particle.Scale.ScaleY =
                        particle.Scale.ScaleZ = 0.001;
                    continue;
                }

                double particleProgress = Math.Max(0, progress - particle.Phase);
                Point3D point = PlanePoint(particleProgress);
                point.Y += Math.Sin((progress + i) * 12) * 0.035;
                point.Z += Math.Cos((progress + i) * 9) * 0.05;
                SetMove(particle, point);
                double age = Clamp01(particle.Phase / 0.52);
                double size = Math.Max(0.03, (1 - age) * Math.Sin(progress * Math.PI));
                particle.Scale.ScaleX = particle.Scale.ScaleY = particle.Scale.ScaleZ = size;
            }

            double wake = Math.Exp(-Math.Pow((progress - 0.51) / 0.12, 2));
            UpdateRippleRingGeometry(_dynamicRingGeometry, wake * 0.18, progress * Math.PI * 8);
            _ringSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(36, 90, 165), wake);
        }

        static Point3D PlanePoint(double progress)
        {
            double x = Lerp(-3.05, 3.05, progress);
            double y = 0.75 - 0.55 * progress + Math.Sin(progress * Math.PI) * 0.32;
            double z = 1.1 - 2.0 * progress;
            return new Point3D(x, y, z);
        }

        void SetupDroplets()
        {
            MeshGeometry3D sphere = CreateSphere(0.125, 10, 8);
            const int droplets = 34;
            for (int i = 0; i < droplets; i++)
            {
                double angle = 2 * Math.PI * i / droplets;
                EffectVisual droplet = AddEffectVisual(
                    sphere,
                    Color.FromRgb(66, 138, 215),
                    Color.FromRgb(18, 58, 122),
                    1);
                droplet.Home = new Point3D(
                    Math.Cos(angle) * RingMajor,
                    Math.Sin(angle) * RingMajor,
                    0);
                droplet.Direction = RandomBurstDirection(droplet.Home);
                droplet.Phase = _random.NextDouble() * Math.PI * 2;
                droplet.Strength = 0.25 + _random.NextDouble() * 0.65;
                SetMove(droplet, droplet.Home);
            }
            _ringHeroScale.ScaleX = _ringHeroScale.ScaleY = _ringHeroScale.ScaleZ = 0.001;
        }

        void UpdateDroplets(double progress, double time)
        {
            double scatter = Math.Pow(Math.Sin(progress * Math.PI), 0.72);
            double pointerWorldX = (_pointerX - 0.5) * 2.6;
            double pointerWorldY = (0.5 - _pointerY) * 2.6;

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual droplet = _effectVisuals[i];
                Vector3D fromPointer = droplet.Home - new Point3D(pointerWorldX, pointerWorldY, 0);
                double rawLength = fromPointer.Length;
                if (rawLength < 0.0001)
                    fromPointer = droplet.Direction;
                else
                    fromPointer /= rawLength;
                double length = Math.Max(0.28, rawLength);
                double repel = Math.Min(0.7, 0.34 / length);
                Vector3D offset = droplet.Direction * (droplet.Strength * scatter);
                offset += fromPointer * (repel * scatter);
                offset.Z += Math.Sin(time * 3.2 + droplet.Phase) * 0.28 * scatter;
                droplet.Move.OffsetX = droplet.Home.X + offset.X;
                droplet.Move.OffsetY = droplet.Home.Y + offset.Y;
                droplet.Move.OffsetZ = droplet.Home.Z + offset.Z;
                double size = 0.72 + 0.34 * Math.Sin(time * 4 + droplet.Phase);
                droplet.Scale.ScaleX = droplet.Scale.ScaleY = droplet.Scale.ScaleZ = size;
            }

            _barHeroMove.OffsetY = Math.Sin(progress * Math.PI) * 0.12;
            _barHeroRotation.Axis = new Vector3D(0.2, 1, 0);
            _barHeroRotation.Angle = Math.Sin(progress * Math.PI) * 35;
        }

        void SetupGlitch()
        {
            const int bands = 11;
            for (int i = 0; i < bands; i++)
            {
                double start = 2 * Math.PI * i / bands;
                double end = 2 * Math.PI * (i + 1) / bands;
                Point3D center;
                MeshGeometry3D arc = CreateTorusArc(start, end, 4, 10, out center);
                Color color = i % 2 == 0
                    ? Color.FromRgb(32, 108, 225)
                    : Color.FromRgb(132, 216, 255);
                EffectVisual band = AddEffectVisual(arc, color, color, 0.58);
                band.Home = center;
                band.Phase = i * 2.17;
                SetMove(band, center);
            }

            for (int i = 0; i < 4; i++)
            {
                Color color = i % 2 == 0
                    ? Color.FromRgb(25, 88, 190)
                    : Color.FromRgb(115, 205, 255);
                EffectVisual barGhost = AddEffectVisual(
                    CreateCapsule(BarRadius, BarCylinderLength, 20, 5),
                    color,
                    color,
                    0.34);
                barGhost.Home = new Point3D(0, BarRestY, 0);
                barGhost.Phase = 20 + i * 3.1;
                SetMove(barGhost, barGhost.Home);
            }
        }

        void UpdateGlitch(double progress, double time)
        {
            double envelope = Math.Pow(Math.Sin(progress * Math.PI), 0.45);
            double step = Math.Floor(time * 20);
            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual visual = _effectVisuals[i];
                double randomX = PseudoRandom(step + visual.Phase) * 2 - 1;
                double randomY = PseudoRandom(step * 1.7 + visual.Phase * 2.3) * 2 - 1;
                visual.Move.OffsetX = visual.Home.X + randomX * envelope * 0.28;
                visual.Move.OffsetY = visual.Home.Y + randomY * envelope * 0.09;
                visual.Move.OffsetZ = visual.Home.Z + randomX * envelope * 0.12;
                visual.Scale.ScaleX = 1 + randomX * envelope * 0.12;
            }

            _groupMove.OffsetX = (PseudoRandom(step * 2.9) * 2 - 1) * envelope * 0.08;
            _groupRoll.Angle = (PseudoRandom(step * 4.1) * 2 - 1) * envelope * 4;
            _ringHeroMove.OffsetX = Math.Sin(step) * envelope * 0.05;
            _barHeroMove.OffsetX = -Math.Sin(step * 1.3) * envelope * 0.07;
            _point.Color = Color.FromRgb(62, 142, 245);
            _glint.Color = Color.FromRgb(155, 224, 255);
            _camera.FieldOfView = 30 + envelope * (PseudoRandom(step) * 5);
        }

        void UpdateCameraPortal(double progress)
        {
            double eased = EaseInOut(progress);
            double angle = eased * Math.PI * 2;
            double radius = 5.25 - Math.Sin(progress * Math.PI) * 2.35;
            Point3D position = new Point3D(
                Math.Sin(angle) * radius,
                0.25 + Math.Sin(angle * 2) * 0.72,
                Math.Cos(angle) * radius);
            Point3D target = new Point3D(0, 0.36, 0);
            _camera.Position = position;
            _camera.LookDirection = target - position;
            _camera.UpDirection = new Vector3D(
                Math.Sin(progress * Math.PI * 2) * 0.25,
                1,
                Math.Sin(progress * Math.PI) * 0.12);
            _camera.FieldOfView = 30 + Math.Sin(progress * Math.PI) * 18;
            _groupRoll.Angle = -Math.Sin(angle) * 14;
            _ringSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(24, 90, 170), Math.Sin(progress * Math.PI));
        }

        void SetupSecretFace()
        {
            MeshGeometry3D eyeGeometry = CreateSphere(0.14, 12, 8);
            EffectVisual leftEye = AddEffectVisual(
                eyeGeometry,
                Color.FromRgb(175, 228, 255),
                Color.FromRgb(75, 180, 255),
                1);
            leftEye.Home = new Point3D(-0.32, 0.22, -0.32);

            EffectVisual rightEye = AddEffectVisual(
                eyeGeometry,
                Color.FromRgb(175, 228, 255),
                Color.FromRgb(75, 180, 255),
                1);
            rightEye.Home = new Point3D(0.32, 0.22, -0.32);

            EffectVisual mouth = AddEffectVisual(
                CreateCapsule(0.07, 0.42, 16, 4),
                Color.FromRgb(128, 205, 255),
                Color.FromRgb(48, 152, 255),
                1);
            mouth.Home = new Point3D(0, -0.30, -0.32);

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                SetMove(_effectVisuals[i], _effectVisuals[i].Home);
                _effectVisuals[i].Scale.ScaleX =
                    _effectVisuals[i].Scale.ScaleY =
                    _effectVisuals[i].Scale.ScaleZ = 0.001;
            }
        }

        void UpdateSecretFace(double progress)
        {
            double flip;
            if (progress < 0.28)
                flip = EaseInOut(progress / 0.28);
            else if (progress < 0.72)
                flip = 1;
            else
                flip = 1 - EaseInOut((progress - 0.72) / 0.28);

            _groupYaw.Angle = _yaw + flip * 180;
            double faceEnvelope = Clamp01((flip - 0.75) * 4);
            double blink = 1;
            if (progress > 0.48 && progress < 0.56)
                blink = 0.10 + Math.Abs(progress - 0.52) / 0.04 * 0.90;

            for (int i = 0; i < _effectVisuals.Count; i++)
            {
                EffectVisual visual = _effectVisuals[i];
                visual.Scale.ScaleX = faceEnvelope;
                visual.Scale.ScaleY = faceEnvelope * (i < 2 ? blink : 1);
                visual.Scale.ScaleZ = faceEnvelope;
            }

            _ringSpin.Emissive.Color = LerpColor(RestGlow, Color.FromRgb(24, 82, 154), faceEnvelope);
            _point.Color = LerpColor(Color.FromRgb(176, 198, 228), Color.FromRgb(105, 190, 255), faceEnvelope);
        }

        MeshGeometry3D CreateTorusArc(
            double start,
            double end,
            int alongSegments,
            int tubeSegments,
            out Point3D center)
        {
            double middle = (start + end) / 2;
            center = new Point3D(RingMajor * Math.Cos(middle), RingMajor * Math.Sin(middle), 0);
            var mesh = new MeshGeometry3D();

            for (int i = 0; i <= alongSegments; i++)
            {
                double theta = Lerp(start, end, i / (double)alongSegments);
                double ct = Math.Cos(theta);
                double st = Math.Sin(theta);
                for (int j = 0; j <= tubeSegments; j++)
                {
                    double phi = 2 * Math.PI * j / tubeSegments;
                    double cp = Math.Cos(phi);
                    double sp = Math.Sin(phi);
                    mesh.Positions.Add(new Point3D(
                        (RingMajor + RingMinor * cp) * ct - center.X,
                        (RingMajor + RingMinor * cp) * st - center.Y,
                        RingMinor * sp));
                    mesh.Normals.Add(new Vector3D(cp * ct, cp * st, sp));
                }
            }

            AddGridTriangles(mesh, alongSegments, tubeSegments);
            mesh.Freeze();
            return mesh;
        }

        static MeshGeometry3D CreateCone(double radius, double length, int sides)
        {
            var mesh = new MeshGeometry3D();
            for (int i = 0; i <= sides; i++)
            {
                double angle = 2 * Math.PI * i / sides;
                double y = Math.Cos(angle) * radius;
                double z = Math.Sin(angle) * radius;
                Vector3D normal = new Vector3D(radius, y * length / radius, z * length / radius);
                normal.Normalize();
                mesh.Positions.Add(new Point3D(0, y, z));
                mesh.Normals.Add(normal);
                mesh.Positions.Add(new Point3D(length, 0, 0));
                mesh.Normals.Add(normal);
            }

            for (int i = 0; i < sides; i++)
            {
                int index = i * 2;
                mesh.TriangleIndices.Add(index);
                mesh.TriangleIndices.Add(index + 2);
                mesh.TriangleIndices.Add(index + 1);
            }
            mesh.Freeze();
            return mesh;
        }

        static MeshGeometry3D CreateSphere(double radius, int longitudeSegments, int latitudeSegments)
        {
            var mesh = new MeshGeometry3D();
            for (int latitude = 0; latitude <= latitudeSegments; latitude++)
            {
                double phi = Math.PI * latitude / latitudeSegments;
                double y = Math.Cos(phi);
                double ring = Math.Sin(phi);
                for (int longitude = 0; longitude <= longitudeSegments; longitude++)
                {
                    double theta = 2 * Math.PI * longitude / longitudeSegments;
                    Vector3D normal = new Vector3D(
                        ring * Math.Cos(theta),
                        y,
                        ring * Math.Sin(theta));
                    mesh.Positions.Add(new Point3D(
                        normal.X * radius,
                        normal.Y * radius,
                        normal.Z * radius));
                    mesh.Normals.Add(normal);
                }
            }
            AddGridTriangles(mesh, latitudeSegments, longitudeSegments);
            mesh.Freeze();
            return mesh;
        }

        static MeshGeometry3D CreatePlaneGeometry()
        {
            var mesh = new MeshGeometry3D();
            AddBox(mesh, new Point3D(0, 0, 0), new Vector3D(0.92, 0.14, 0.14));
            AddBox(mesh, new Point3D(-0.02, 0, 0), new Vector3D(0.25, 0.055, 1.12));
            AddBox(mesh, new Point3D(-0.36, 0.16, 0), new Vector3D(0.20, 0.38, 0.06));
            AddBox(mesh, new Point3D(-0.35, 0, 0), new Vector3D(0.22, 0.045, 0.48));
            mesh.Freeze();
            return mesh;
        }

        static void AddBox(MeshGeometry3D mesh, Point3D center, Vector3D size)
        {
            double x0 = center.X - size.X / 2;
            double x1 = center.X + size.X / 2;
            double y0 = center.Y - size.Y / 2;
            double y1 = center.Y + size.Y / 2;
            double z0 = center.Z - size.Z / 2;
            double z1 = center.Z + size.Z / 2;

            AddQuad(mesh, new Point3D(x1, y0, z0), new Point3D(x1, y1, z0), new Point3D(x1, y1, z1), new Point3D(x1, y0, z1), new Vector3D(1, 0, 0));
            AddQuad(mesh, new Point3D(x0, y0, z1), new Point3D(x0, y1, z1), new Point3D(x0, y1, z0), new Point3D(x0, y0, z0), new Vector3D(-1, 0, 0));
            AddQuad(mesh, new Point3D(x0, y1, z0), new Point3D(x0, y1, z1), new Point3D(x1, y1, z1), new Point3D(x1, y1, z0), new Vector3D(0, 1, 0));
            AddQuad(mesh, new Point3D(x0, y0, z1), new Point3D(x0, y0, z0), new Point3D(x1, y0, z0), new Point3D(x1, y0, z1), new Vector3D(0, -1, 0));
            AddQuad(mesh, new Point3D(x0, y0, z1), new Point3D(x1, y0, z1), new Point3D(x1, y1, z1), new Point3D(x0, y1, z1), new Vector3D(0, 0, 1));
            AddQuad(mesh, new Point3D(x1, y0, z0), new Point3D(x0, y0, z0), new Point3D(x0, y1, z0), new Point3D(x1, y1, z0), new Vector3D(0, 0, -1));
        }

        static void AddQuad(
            MeshGeometry3D mesh,
            Point3D a,
            Point3D b,
            Point3D c,
            Point3D d,
            Vector3D normal)
        {
            int first = mesh.Positions.Count;
            mesh.Positions.Add(a);
            mesh.Positions.Add(b);
            mesh.Positions.Add(c);
            mesh.Positions.Add(d);
            for (int i = 0; i < 4; i++)
                mesh.Normals.Add(normal);
            mesh.TriangleIndices.Add(first);
            mesh.TriangleIndices.Add(first + 1);
            mesh.TriangleIndices.Add(first + 2);
            mesh.TriangleIndices.Add(first);
            mesh.TriangleIndices.Add(first + 2);
            mesh.TriangleIndices.Add(first + 3);
        }

        static MeshGeometry3D CreateDynamicRingGeometry(int aroundSegments, int tubeSegments)
        {
            var mesh = new MeshGeometry3D();
            for (int i = 0; i <= aroundSegments; i++)
            {
                for (int j = 0; j <= tubeSegments; j++)
                {
                    mesh.Positions.Add(new Point3D());
                    mesh.Normals.Add(new Vector3D(0, 0, 1));
                }
            }
            AddGridTriangles(mesh, aroundSegments, tubeSegments);
            return mesh;
        }

        static void AddGridTriangles(MeshGeometry3D mesh, int rows, int columns)
        {
            int stride = columns + 1;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int a = row * stride + column;
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
        }

        static void UpdateMorphRingGeometry(MeshGeometry3D mesh, double strength, double targetBlend)
        {
            const int aroundSegments = 48;
            const int tubeSegments = 16;
            const double epsilon = 0.001;
            var positions = new Point3DCollection((aroundSegments + 1) * (tubeSegments + 1));
            var normals = new Vector3DCollection((aroundSegments + 1) * (tubeSegments + 1));

            for (int i = 0; i <= aroundSegments; i++)
            {
                double theta = 2 * Math.PI * i / aroundSegments;
                Point3D center = MorphCenter(theta, strength, targetBlend);
                Point3D before = MorphCenter(theta - epsilon, strength, targetBlend);
                Point3D after = MorphCenter(theta + epsilon, strength, targetBlend);
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

        static Point3D MorphCenter(double theta, double strength, double targetBlend)
        {
            Point3D circle = new Point3D(Math.Cos(theta), Math.Sin(theta), 0);
            Point3D infinity = new Point3D(
                Math.Sin(theta) * 1.18,
                Math.Sin(theta * 2) * 0.62,
                Math.Cos(theta) * 0.18);
            Point3D twistedRing = new Point3D(
                Math.Cos(theta) * 0.95,
                Math.Sin(theta) * 0.86,
                Math.Sin(theta * 2) * 0.52);
            Point3D target = LerpPoint(infinity, twistedRing, targetBlend);
            return LerpPoint(circle, target, strength);
        }

        static void UpdateRippleRingGeometry(MeshGeometry3D mesh, double amplitude, double phase)
        {
            const int aroundSegments = 48;
            const int tubeSegments = 16;
            var positions = new Point3DCollection((aroundSegments + 1) * (tubeSegments + 1));
            var normals = new Vector3DCollection((aroundSegments + 1) * (tubeSegments + 1));
            for (int i = 0; i <= aroundSegments; i++)
            {
                double theta = 2 * Math.PI * i / aroundSegments;
                double ripple = amplitude * Math.Sin(theta * 5 - phase);
                double major = RingMajor + ripple;
                double ct = Math.Cos(theta);
                double st = Math.Sin(theta);
                for (int j = 0; j <= tubeSegments; j++)
                {
                    double phi = 2 * Math.PI * j / tubeSegments;
                    double cp = Math.Cos(phi);
                    double sp = Math.Sin(phi);
                    Vector3D normal = new Vector3D(cp * ct, cp * st, sp);
                    positions.Add(new Point3D(
                        (major + RingMinor * cp) * ct,
                        (major + RingMinor * cp) * st,
                        RingMinor * sp + ripple * 0.32));
                    normals.Add(normal);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
        }

        Vector3D RandomBurstDirection(Point3D home)
        {
            var direction = new Vector3D(
                home.X + (_random.NextDouble() * 2 - 1) * 0.45,
                home.Y + (_random.NextDouble() * 2 - 1) * 0.45,
                _random.NextDouble() * 2 - 1);
            if (direction.LengthSquared < 0.04)
                direction = RandomAxis();
            direction.Normalize();
            return direction;
        }

        static void SetMove(EffectVisual visual, Point3D point)
        {
            visual.Move.OffsetX = point.X;
            visual.Move.OffsetY = point.Y;
            visual.Move.OffsetZ = point.Z;
        }

        static Point3D LerpPoint(Point3D a, Point3D b, double amount)
        {
            return new Point3D(
                Lerp(a.X, b.X, amount),
                Lerp(a.Y, b.Y, amount),
                Lerp(a.Z, b.Z, amount));
        }

        static double PseudoRandom(double value)
        {
            double result = Math.Sin(value * 12.9898 + 78.233) * 43758.5453;
            return result - Math.Floor(result);
        }
    }
}

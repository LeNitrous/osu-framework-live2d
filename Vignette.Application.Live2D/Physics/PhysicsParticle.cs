// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using osuTK;
using Vignette.Application.Live2D.Utils;

namespace Vignette.Application.Live2D.Physics
{
    public class PhysicsParticle
    {
        public CubismVector2 InitialPosition { get; set; }

        public float Mobility { get; set; }

        public float Delay { get; set; }

        public float Acceleration { get; set; }

        public float Radius { get; set; }

        public CubismVector2 Position { get; set; }

        public CubismVector2 LastPosition { get; set; }

        public CubismVector2 LastGravity { get; set; }

        public CubismVector2 Force { get; set; }

        public CubismVector2 Velocity { get; set; }
    }
}

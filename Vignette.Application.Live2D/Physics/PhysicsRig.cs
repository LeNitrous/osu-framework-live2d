// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using osuTK;
using Vignette.Application.Live2D.Utils;

namespace Vignette.Application.Live2D.Physics
{
    public class PhysicsRig
    {
        public int SubRigCount { get; set; }

        public PhysicsSubRig[] Settings { get; set; }

        public PhysicsInput[] Inputs { get; set; }

        public PhysicsOutput[] Outputs { get; set; }

        public PhysicsParticle[] Particles { get; set; }

        public CubismVector2 Gravity { get; set; }

        public CubismVector2 Wind { get; set; }
    }
}

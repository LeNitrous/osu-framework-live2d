// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System.Collections.Generic;
using Vignette.Application.Live2D.Utils;

namespace Vignette.Application.Live2D.Physics
{
    public class PhysicsOutput
    {
        public PhysicsParameter Destination { get; set; }

        public int DestinationParameterIndex { get; set; }

        public int VertexIndex { get; set; }

        public CubismVector2 TranslationScale { get; set; }

        public float AngleScale { get; set; }

        public float Weight { get; set; }

        public PhysicsSource Type { get; set; }

        public bool Reflect { get; set; }

        public float ValueBelowMinimum { get; set; }

        public float ValueExceededMaximum { get; set; }

        public PhysicsValueGetter GetValue { get; set; }

        public PhysicsScaleGetter GetScale { get; set; }

        public delegate float PhysicsValueGetter(
            CubismVector2 translation,
            List<PhysicsParticle> particles,
            int particleIndex,
            bool isInverted,
            CubismVector2 parentGravity
        );

        public delegate float PhysicsScaleGetter(
            CubismVector2 translationScale,
            float angleScale
        );
    }
}

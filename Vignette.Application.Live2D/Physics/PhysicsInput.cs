// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using osuTK;
using Vignette.Application.Live2D.Model;
using Vignette.Application.Live2D.Utils;

namespace Vignette.Application.Live2D.Physics
{
    public class PhysicsInput
    {
        public PhysicsParameter Source { get; set; }

        public int SourceParameterIndex { get; set; }

        public float Weight { get; set; }

        public PhysicsSource Type { get; set; }

        public bool Reflect { get; set; }

        public NormalizedPhysicsParameterValueGetter GetNormalizedPhysicsParameterValue { get; set; }

        public delegate void NormalizedPhysicsParameterValueGetter(
            ref CubismVector2 TargetTranslation,
            ref float TargetAngle,
            CubismParameter parameter,
            PhysicsNormalization normalizationPosition,
            PhysicsNormalization normalizationAngle,
            bool isInverted,
            float weight
        );
    }
}

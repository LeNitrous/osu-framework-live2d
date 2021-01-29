// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using Vignette.Application.Live2D.Graphics;
using Vignette.Application.Live2D.Json;
using Vignette.Application.Live2D.Model;
using Vignette.Application.Live2D.Motion;
using Vignette.Application.Live2D.Physics;
using Vignette.Application.Live2D.Tests.Resources;

namespace Vignette.Application.Live2D.Tests.Visual
{
    public class TestScenePhysics : TestSceneCubismSprite
    {
        private readonly SpriteText statusText;

        public TestScenePhysics()
        {
            statusText = new SpriteText { Margin = new MarginPadding(10) };

            Add(statusText);
        }

        protected override void Update()
        {
            base.Update();

            statusText.Text = $"{Clock.ElapsedFrameTime / 1000}";
        }

        protected override CubismSprite CreateSprite(CubismModel model, IEnumerable<Texture> textures) => new CubismSpriteWithPhysics(model, textures)
        {
            ScaleAdjust = 6f,
            PositionAdjust = new Vector2(0, 1400),
        };

        private class CubismSpriteWithPhysics : CubismSprite
        {
            public CubismMotion Motion { get; private set; }

            public CubismPhysics Physics { get; private set; }

            public CubismSpriteWithPhysics(CubismModel model, IEnumerable<Texture> textures, bool disposeModel = false)
                : base(model, textures, disposeModel)
            {
                using var physicsReader = new StreamReader(TestResources.GetModelResource(@"Hiyori.physics3.json"));
                var physicsSetting = JsonSerializer.Deserialize<CubismPhysicsSetting>(physicsReader.ReadToEnd());
                Physics = new CubismPhysics(model, physicsSetting) { Gravity = new Utils.CubismVector2(0, -1) };

                using var motionReader = new StreamReader(TestResources.GetModelResource(@"motions/Hiyori_m02.motion3.json"));
                var motionSetting = JsonSerializer.Deserialize<CubismMotionSetting>(motionReader.ReadToEnd());

                Motion = new CubismMotion(model, motionSetting)
                {
                    Weight = 1,
                    LoopFading = true,
                    GlobalFadeInSeconds = 0.5,
                    GlobalFadeOutSeconds = 0.5,
                };
            }

            protected override void Update()
            {
                base.Update();

                Motion.Update(Clock.CurrentTime / 1000, true);
                Physics.Update(Clock.ElapsedFrameTime / 1000);
            }
        }
    }
}

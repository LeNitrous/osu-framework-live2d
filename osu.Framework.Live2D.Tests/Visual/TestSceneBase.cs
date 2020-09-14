// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CubismFramework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cubism;
using osu.Framework.Graphics.Cubism.Renderer;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osuTK;

namespace osu.Framework.Live2D.Tests.Visual
{
    public abstract class TestSceneBase : TestScene
    {
        protected Container<Drawable> Container;
        protected CubismSprite Sprite;
        protected static string[] Parameters => typeof(CubismDefaultParameterId)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Select(f => f.GetValue(null) as string).ToArray();
        protected virtual Colour4 BackgroundColour => Colour4.Transparent;

        [BackgroundDependencyLoader]
        private void load(CubismAssetStore cubismAssets)
        {
            Add(Container = new Container<Drawable>
            {
                Size = new Vector2(684),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = BackgroundColour
                    },
                    Sprite = new CubismSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Asset = cubismAssets.Get("Hiyori.Hiyori.model3.json"),
                        Renderer = new CubismRenderer
                        {
                            Scale = new Vector2(2),
                            Y = 384
                        }
                    }
                }
            });
        }

        protected class ParameterMonitor : FillFlowContainer
        {
            private CubismSprite sprite;
            private Dictionary<string, Box> bars = new Dictionary<string, Box>();

            public ParameterMonitor(CubismSprite sprite, string[] parameters)
            {
                Width = 350;
                RelativeSizeAxes = Axes.Y;
                Direction = FillDirection.Full;

                this.sprite = sprite;
                foreach (var param in parameters)
                {
                    if (sprite.Asset.Model.GetParameter(param) == null)
                        continue;

                    var bar = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Colour = Colour4.White,
                    };

                    bars.Add(param, bar);

                    Add(new FillFlowContainer
                    {
                        Direction = FillDirection.Vertical,
                        Width = 150,
                        Height = 30,
                        Margin = new MarginPadding(5),
                        Children = new Drawable[]
                        {
                            new SpriteText { Text = param, Scale = new Vector2(0.75f) },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 5,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colour4.White.Opacity(0.25f),
                                    },
                                    bar
                                }
                            }
                        }
                    });
                }
            }

            protected override void Update()
            {
                base.Update();

                foreach (var (name, bar) in bars)
                {
                    var param = map((float)sprite.GetParameterValue(name), 0, 1, -0.5f, 0.5f);
                    bar.Width = Math.Clamp(param, -0.5f, 0.5f);
                    bar.Colour = param > 0 ? Colour4.White : Colour4.Red;
                }

                float map(float value, float fromSource, float toSource, float fromDestination, float toDestination) =>
                    (value - fromSource) / (toSource - fromSource) * (toDestination - fromDestination) + fromDestination;
            }
        }
    }
}
// Copyright 2020 - 2021 Vignette Project
// Licensed under MIT. See LICENSE for details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;
using osuTK;
using Vignette.Application.Live2D.Json;
using Vignette.Application.Live2D.Model;

namespace Vignette.Application.Live2D.Physics
{
    public class CubismPhysics
    {
        public static float AirResistance => 5.0f;

        public static float MaximumWeight => 100.0f;

        public static float MovementThreshold => 0.001f;

        public bool UseAngleCorrection { get; set; } = true;

        private PhysicsRig physicsRig;

        private CubismModel model;

        public CubismPhysics(CubismModel model, CubismPhysicsSetting setting)
        {
            this.model = model;

            physicsRig = new PhysicsRig
            {
                Gravity = setting.Meta.EffectiveForces.Gravity,
                Wind = setting.Meta.EffectiveForces.Wind,
                SubRigs = new PhysicsSubRig[setting.Meta.PhysicsSettingCount],
            };

            for (int i = 0; i < physicsRig.SubRigs.Length; ++i)
            {
                physicsRig.SubRigs[i] = new PhysicsSubRig
                {
                    Gravity = physicsRig.Gravity,
                    Wind = physicsRig.Wind,
                    Input = readInputs(setting.PhysicsSettings[i].Input.ToArray()),
                    Output = readOutputs(setting.PhysicsSettings[i].Output.ToArray()),
                    Particles = readParticles(setting.PhysicsSettings[i].Vertices.ToArray()),
                    Normalization = readNormalization(setting.PhysicsSettings[i].Normalization),
                };
            }

            physicsRig.Initialize();
        }

        public void Update(double delta)
        {
            physicsRig.Update(delta);
        }

        private PhysicsInput[] readInputs(CubismPhysicsSetting.PhysicsSetting.InputSetting[] settings)
        {
            var data = new PhysicsInput[settings.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new PhysicsInput
                {
                    Source = model.Parameters.Get(settings[i].Source.Id),
                    SourceId = settings[i].Source.Id,
                    AngleScale = 0.0f,
                    ScaleOfTranslation = Vector2.Zero,
                    Weight = settings[i].Weight,
                    SourceComponent = Enum.Parse<PhysicsSource>(settings[i].Type),
                    IsInverted = settings[i].Reflect,
                };
            }

            return data;
        }

        private PhysicsOutput[] readOutputs(CubismPhysicsSetting.PhysicsSetting.OutputSetting[] settings)
        {
            var data = new PhysicsOutput[settings.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new PhysicsOutput
                {
                    Destination = model.Parameters.Get(settings[i].Destination.Id),
                    DestinationId = settings[i].Destination.Id,
                    ParticleIndex = settings[i].VertexIndex,
                    TranslationScale = Vector2.Zero,
                    AngleScale = settings[i].Scale,
                    Weight = settings[i].Weight,
                    SourceComponent = Enum.Parse<PhysicsSource>(settings[i].Type),
                    IsInverted = settings[i].Reflect,
                    ValueBelowMinimum = 0.0f,
                    ValueExceededMaximum = 0.0f,
                    UseAngleCorrection = UseAngleCorrection,
                };
            }

            return data;
        }

        private PhysicsParticle[] readParticles(CubismPhysicsSetting.PhysicsSetting.VertexSetting[] settings)
        {
            var data = new PhysicsParticle[settings.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new PhysicsParticle
                {
                    InitialPosition = settings[i].Position,
                    Mobility = settings[i].Mobility,
                    Delay = settings[i].Delay,
                    Acceleration = settings[i].Acceleration,
                    Radius = settings[i].Radius,
                    Position = Vector2.Zero,
                    LastPosition = Vector2.Zero,
                    LastGravity = Vector2.Zero,
                    Force = Vector2.Zero,
                    Velocity = Vector2.Zero,
                };
            }

            return data;
        }

        private PhysicsNormalization readNormalization(CubismPhysicsSetting.PhysicsSetting.NormalizationSetting setting)
        {
            return new PhysicsNormalization
            {
                Position = new PhysicsNormalizationTuplet
                {
                    Maximum = setting.Position.Maximum,
                    Minimum = setting.Position.Minimum,
                    Default = setting.Position.Default,
                },
                Angle = new PhysicsNormalizationTuplet
                {
                    Maximum = setting.Angle.Maximum,
                    Minimum = setting.Angle.Minimum,
                    Default = setting.Angle.Default,
                }
            };
        }
    }
}

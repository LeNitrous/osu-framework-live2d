// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;
using System.Collections.Generic;
using System.Linq;
using osuTK;
using Vignette.Application.Live2D.Json;
using Vignette.Application.Live2D.Model;
using Vignette.Application.Live2D.Utils;

namespace Vignette.Application.Live2D.Physics
{
    public class CubismPhysics
    {
        public CubismVector2 Gravity { get; set; }

        public CubismVector2 Wind { get; set; }

        private const float air_resistance = 5.0f;

        private const float maximum_weight = 100.0f;

        private const float movement_threshold = 0.001f;

        private readonly PhysicsRig physicsRig = new PhysicsRig();

        public CubismPhysics(CubismModel model, CubismPhysicsSetting setting)
        {
            var gravity = setting.Meta.EffectiveForces.Gravity;
            var wind = setting.Meta.EffectiveForces.Wind;

            physicsRig.Gravity = new CubismVector2(gravity.X, gravity.Y);
            physicsRig.Wind = new CubismVector2(wind.X, wind.Y);
            physicsRig.SubRigCount = setting.Meta.PhysicsSettingCount;

            physicsRig.Settings = new PhysicsSubRig[physicsRig.SubRigCount];
            physicsRig.Inputs = new PhysicsInput[setting.Meta.TotalInputCount];
            physicsRig.Outputs = new PhysicsOutput[setting.Meta.TotalOutputCount];
            physicsRig.Particles = new PhysicsParticle[setting.Meta.VertexCount];

            int inputIndex = 0;
            int outputIndex = 0;
            int particleIndex = 0;
            for (int i = 0; i < physicsRig.SubRigCount; i++)
            {
                physicsRig.Settings[i] = new PhysicsSubRig
                {
                    NormalizationPosition = new PhysicsNormalization
                    {
                        Minimum = setting.PhysicsSettings[i].Normalization.Position.Minimum,
                        Maximum = setting.PhysicsSettings[i].Normalization.Position.Maximum,
                        Default = setting.PhysicsSettings[i].Normalization.Position.Default,
                    },

                    InputCount = setting.PhysicsSettings[i].Input.Count,
                    BaseInputIndex = inputIndex
                };

                for (int j = 0; j < physicsRig.Settings[i].InputCount; j++)
                {
                    physicsRig.Inputs[inputIndex + j] = new PhysicsInput
                    {
                        SourceParameterIndex = -1,
                        Weight = setting.PhysicsSettings[i].Input[j].Weight,
                        Reflect = setting.PhysicsSettings[i].Input[j].Reflect,
                        Source = new PhysicsParameter
                        {
                            TargetType = PhysicsTarget.Parameter,
                            Parameter = model.Parameters.Get(setting.PhysicsSettings[i].Input[j].Source.Id),
                        }
                    };

                    switch (setting.PhysicsSettings[i].Input[j].Type)
                    {
                        case "X":
                            physicsRig.Inputs[inputIndex + j].Type = PhysicsSource.X;
                            physicsRig.Inputs[inputIndex + j].GetNormalizedPhysicsParameterValue = getInputTranslationXFromNormalizedParameterValue;
                            break;

                        case "Y":
                            physicsRig.Inputs[inputIndex + j].Type = PhysicsSource.Y;
                            physicsRig.Inputs[inputIndex + j].GetNormalizedPhysicsParameterValue = getInputTranslationYFromNormalizedParameterValue;
                            break;

                        case "Angle":
                            physicsRig.Inputs[inputIndex + j].Type = PhysicsSource.Angle;
                            physicsRig.Inputs[inputIndex + j].GetNormalizedPhysicsParameterValue = getInputAngleFromNormalizedParameterValue;
                            break;
                    }
                }

                inputIndex += physicsRig.Settings[i].InputCount;

                physicsRig.Settings[i].OutputCount = setting.PhysicsSettings[i].Output.Count;
                physicsRig.Settings[i].BaseOutputIndex = outputIndex;
                for (int j = 0; j < physicsRig.Settings[i].OutputCount; j++)
                {
                    physicsRig.Outputs[outputIndex + j] = new PhysicsOutput
                    {
                        DestinationParameterIndex = -1,
                        VertexIndex = setting.PhysicsSettings[i].Output[j].VertexIndex,
                        AngleScale = setting.PhysicsSettings[i].Output[j].Scale,
                        Weight = setting.PhysicsSettings[i].Output[j].Weight,
                        Reflect = setting.PhysicsSettings[i].Output[j].Reflect,
                        Destination = new PhysicsParameter
                        {
                            TargetType = PhysicsTarget.Parameter,
                            Parameter = model.Parameters.Get(setting.PhysicsSettings[i].Output[j].Destination.Id),
                        }
                    };

                    switch (setting.PhysicsSettings[i].Output[j].Type)
                    {
                        case "X":
                            physicsRig.Outputs[outputIndex + j].Type = PhysicsSource.X;
                            physicsRig.Outputs[outputIndex + j].GetValue = getOutputTranslationX;
                            physicsRig.Outputs[outputIndex + j].GetScale = getOutputScaleTranslationX;
                            break;

                        case "Y":
                            physicsRig.Outputs[outputIndex + j].Type = PhysicsSource.Y;
                            physicsRig.Outputs[outputIndex + j].GetValue = getOutputTranslationY;
                            physicsRig.Outputs[outputIndex + j].GetScale = getOutputScaleTranslationY;
                            break;

                        case "Angle":
                            physicsRig.Outputs[outputIndex + j].Type = PhysicsSource.Angle;
                            physicsRig.Outputs[outputIndex + j].GetValue = getOutputAngle;
                            physicsRig.Outputs[outputIndex + j].GetScale = getOutputScaleAngle;
                            break;
                    }
                }

                outputIndex += physicsRig.Settings[i].OutputCount;

                physicsRig.Settings[i].ParticleCount = setting.PhysicsSettings[i].Vertices.Count;
                physicsRig.Settings[i].BaseParticleIndex = particleIndex;
                for (int j = 0; j < physicsRig.Settings[i].ParticleCount; j++)
                {
                    var position = setting.PhysicsSettings[i].Vertices[j].Position;
                    physicsRig.Particles[particleIndex + j] = new PhysicsParticle
                    {
                        Mobility = setting.PhysicsSettings[i].Vertices[j].Mobility,
                        Delay = setting.PhysicsSettings[i].Vertices[j].Delay,
                        Acceleration = setting.PhysicsSettings[i].Vertices[j].Acceleration,
                        Radius = setting.PhysicsSettings[i].Vertices[j].Radius,
                        Position = new CubismVector2(position.X, position.Y),
                    };
                }

                particleIndex += physicsRig.Settings[i].ParticleCount;
            }

            initialize();
        }

        public void Update(double delta)
        {
            for (int i = 0; i < physicsRig.SubRigCount; i++)
            {
                var totalTranslation = CubismVector2.Zero;
                float totalAngle = 0;

                var currentSetting = physicsRig.Settings[i];
                var currentInput = physicsRig.Inputs.Skip(currentSetting.BaseInputIndex).ToList();
                var currentOutput = physicsRig.Outputs.Skip(currentSetting.BaseOutputIndex).ToList();
                var currentParticles = physicsRig.Particles.Skip(currentSetting.BaseParticleIndex).ToList();

                for (int j = 0; j < currentSetting.InputCount; j++)
                {
                    float weight = currentInput[j].Weight / maximum_weight;

                    if (currentInput[j].SourceParameterIndex == -1)
                        currentInput[j].SourceParameterIndex = currentInput[j].Source.Parameter.Id;

                    currentInput[j].GetNormalizedPhysicsParameterValue(
                        ref totalTranslation,
                        ref totalAngle,
                        currentInput[j].Source.Parameter,
                        currentSetting.NormalizationPosition,
                        currentSetting.NormalizationAngle,
                        currentInput[j].Reflect,
                        weight
                    );
                }

                float radAngle = CubismMath.DegreesToRadian(-totalAngle);
                float totalTranslationX = (totalTranslation.X * MathF.Cos(radAngle)) - (totalTranslation.Y * MathF.Sin(radAngle));
                float totalTranslationY = (totalTranslation.X * MathF.Cos(radAngle)) + (totalTranslation.Y * MathF.Sin(radAngle));
                totalTranslation = new CubismVector2(totalTranslationX, totalTranslationY);

                updateParticles(
                    currentParticles,
                    currentSetting.ParticleCount,
                    totalTranslation,
                    totalAngle,
                    Wind,
                    movement_threshold * currentSetting.NormalizationPosition.Maximum,
                    (float)delta,
                    air_resistance
                );

                for (int j = 0; j < currentSetting.OutputCount; j++)
                {
                    int particleIndex = currentOutput[j].VertexIndex;

                    if (particleIndex < 1 || particleIndex >= currentSetting.ParticleCount)
                        break;

                    if (currentOutput[j].DestinationParameterIndex == -1)
                        currentOutput[j].DestinationParameterIndex = currentOutput[j].Destination.Parameter.Id;

                    var translation = currentParticles[particleIndex].Position - currentParticles[particleIndex - 1].Position;

                    float outputValue = currentOutput[j].GetValue(
                        translation,
                        currentParticles,
                        particleIndex,
                        currentOutput[j].Reflect,
                        Gravity
                    );

                    updateOutputParameterValue(currentOutput[j].Destination.Parameter, outputValue, currentOutput[j]);
                }
            }
        }

        private void initialize()
        {
            for (int i = 0; i < physicsRig.SubRigCount; i++)
            {
                var currentSetting = physicsRig.Settings[i];

                var top = physicsRig.Particles[currentSetting.BaseParticleIndex];
                top.InitialPosition = CubismVector2.Zero;
                top.LastPosition = new CubismVector2(top.InitialPosition.X, top.InitialPosition.Y);
                top.LastGravity = new CubismVector2(0, -1);
                top.LastGravity *= new CubismVector2(0, -1);
                top.Velocity = CubismVector2.Zero;
                top.Force = CubismVector2.Zero;

                for (int j = 1; j < currentSetting.ParticleCount; j++)
                {
                    var particle = physicsRig.Particles[currentSetting.BaseParticleIndex + j];
                    var radius = new CubismVector2(0, particle.Radius);
                    particle.InitialPosition = physicsRig.Particles[currentSetting.BaseParticleIndex + j - 1].InitialPosition + radius;
                    particle.Position = particle.InitialPosition;
                    particle.LastPosition = particle.InitialPosition;
                    particle.LastGravity = new CubismVector2(0, -1);
                    particle.LastGravity *= new CubismVector2(0, -1);
                    particle.Velocity = CubismVector2.Zero;
                    particle.Force = CubismVector2.Zero;
                }
            }
        }

        private void updateParticles(
            List<PhysicsParticle> strand,
            int strandCount,
            CubismVector2 totalTranslation,
            float totalAngle,
            CubismVector2 windDirection,
            float thresholdValue,
            float deltaTimeSeconds,
            float airResistance
        )
        {
            strand[0].Position = totalTranslation;

            float totalRadian = CubismMath.DegreesToRadian(totalAngle);

            var currentGravity = CubismMath.RadianToDirection(totalRadian);
            currentGravity.Normalize();

            for (int i = 1; i < strandCount; i++)
            {
                strand[i].Force = (currentGravity * strand[i].Acceleration) + windDirection;
                strand[i].LastPosition = strand[i].Position;
                float delay = strand[i].Delay * deltaTimeSeconds * 30.0f;

                var direction = strand[i].Position - strand[i - 1].Position;
                float radian = CubismMath.DirectionToRadian(strand[i].LastGravity, currentGravity) / airResistance;

                float directionX = (MathF.Cos(radian) * direction.X) - (direction.Y * MathF.Sin(radian));
                float directionY = (MathF.Cos(radian) * direction.X) + (direction.Y * MathF.Sin(radian));
                direction = new CubismVector2(directionX, directionY);

                strand[i].Position = strand[i - 1].Position + direction;

                var velocity = strand[i].Velocity * delay;
                var force = strand[i].Force * delay * delay;

                strand[i].Position = strand[i].Position + velocity + force;

                var newDirection = strand[i].Position - strand[i - 1].Position;
                newDirection.Normalize();

                strand[i].Position = strand[i - 1].Position + (newDirection * strand[i].Radius);

                if (MathF.Abs(strand[i].Position.X) < thresholdValue)
                    strand[i].Position = new CubismVector2(0, strand[i].Position.Y);

                if (delay != 0)
                {
                    strand[i].Velocity = strand[i].Position - strand[i].LastPosition;
                    strand[i].Velocity /= delay;
                    strand[i].Velocity *= strand[i].Mobility;
                }

                strand[i].Force = CubismVector2.Zero;
                strand[i].LastGravity = currentGravity;
            }
        }

        private void updateOutputParameterValue(CubismParameter parameter, float translation, PhysicsOutput output)
        {
            float outputScale = output.GetScale(output.TranslationScale, output.AngleScale);
            float value = translation * outputScale;

            if (value < parameter.Minimum)
            {
                if (value < output.ValueBelowMinimum)
                    output.ValueBelowMinimum = value;

                value = parameter.Minimum;
            }
            else if (value > parameter.Maximum)
            {
                if (value > output.ValueExceededMaximum)
                    output.ValueExceededMaximum = value;

                value = parameter.Maximum;
            }

            float weight = output.Weight / maximum_weight;
            if (weight >= 1)
                parameter.Value = value;
            else
            {
                value = (parameter.Value * (1 * weight)) + (value * weight);
                parameter.Value = value;
            }
        }

        private float getRangeValue(float min, float max)
        {
            float maxValue = CubismMath.Maximum(min, max);
            float minValue = CubismMath.Minimum(min, max);

            return MathF.Abs(maxValue - minValue);
        }

        private float getDefaultValue(float min, float max)
        {
            float minValue = CubismMath.Minimum(min, max);
            return minValue + getRangeValue(min, max) / 2.0f;
        }

        private float normalizeParameterValue(
            float value,
            float paramMin,
            float paramMax,
            float paramDef,
            float normalizedMin,
            float normalizedMax,
            float normalizedDef,
            bool isInverted
        )
        {
            float result = 0;

            float maxValue = CubismMath.Maximum(paramMax, paramMin);
            if (maxValue < value)
                value = maxValue;

            float minValue = CubismMath.Minimum(paramMax, paramMin);
            if (minValue > value)
                value = minValue;

            float minNormValue = CubismMath.Minimum(normalizedMin, normalizedMax);
            float maxNormValue = CubismMath.Maximum(normalizedMin, normalizedMax);
            float midNormValue = normalizedDef;

            float midValue = getDefaultValue(minValue, maxValue);
            float paramValue = value - midValue;

            switch (MathF.Sign(paramValue))
            {
                case 1:
                {
                    float nLength = maxNormValue - midNormValue;
                    float pLength = maxValue - midValue;

                    if (pLength != 0.0f)
                    {
                        result = paramValue * (nLength / pLength);
                        result += midNormValue;
                    }

                    break;
                }

                case -1:
                {
                    float nLength = minNormValue - midNormValue;
                    float pLength = minValue - midValue;

                    if (pLength != 0)
                    {
                        result = paramValue * (nLength / pLength);
                        result += midNormValue;
                    }

                    break;
                }

                case 0:
                    result = midNormValue;
                    break;
            }

            return !isInverted ? result : result * -1.0f;
        }

        private void getInputTranslationXFromNormalizedParameterValue(
            ref CubismVector2 targetTranslation,
            ref float targetAngle,
            CubismParameter parameter,
            PhysicsNormalization normalizationPosition,
            PhysicsNormalization normalizationAngle,
            bool isInverted,
            float weight
        )
        {
            targetTranslation.X += normalizeParameterValue(
                parameter.Value,
                parameter.Minimum,
                parameter.Maximum,
                parameter.Default,
                normalizationPosition.Minimum,
                normalizationPosition.Maximum,
                normalizationPosition.Default,
                isInverted
            ) * weight;
        }

        private void getInputTranslationYFromNormalizedParameterValue(
            ref CubismVector2 targetTranslation,
            ref float targetAngle,
            CubismParameter parameter,
            PhysicsNormalization normalizationPosition,
            PhysicsNormalization normalizationAngle,
            bool isInverted,
            float weight
        )
        {
            targetTranslation.Y += normalizeParameterValue(
                parameter.Value,
                parameter.Minimum,
                parameter.Maximum,
                parameter.Default,
                normalizationPosition.Minimum,
                normalizationPosition.Maximum,
                normalizationPosition.Default,
                isInverted
            ) * weight;
        }

        private void getInputAngleFromNormalizedParameterValue(
            ref CubismVector2 targetTranslation,
            ref float targetAngle,
            CubismParameter parameter,
            PhysicsNormalization normalizationPosition,
            PhysicsNormalization normalizationAngle,
            bool isInverted,
            float weight
        )
        {
            targetAngle += normalizeParameterValue(
                parameter.Value,
                parameter.Minimum,
                parameter.Maximum,
                parameter.Default,
                normalizationAngle.Minimum,
                normalizationAngle.Maximum,
                normalizationAngle.Default,
                isInverted
           ) * weight;
        }

        private float getOutputTranslationX(
            CubismVector2 translation,
            List<PhysicsParticle> particles,
            int particleIndex,
            bool isInverted,
            CubismVector2 parentGravity
        )
        {
            float outputValue = parentGravity.X;

            if (isInverted)
                outputValue *= -1.0f;

            return outputValue;
        }

        private float getOutputTranslationY(
            CubismVector2 translation,
            List<PhysicsParticle> particles,
            int particleIndex,
            bool isInverted,
            CubismVector2 parentGravity
        )
        {
            float outputValue = parentGravity.Y;

            if (isInverted)
                outputValue *= -1.0f;

            return outputValue;
        }

        private float getOutputAngle(
            CubismVector2 translation,
            List<PhysicsParticle> particles,
            int particleIndex,
            bool isInverted,
            CubismVector2 parentGravity
        )
        {
            float outputValue;

            if (particleIndex >= 2)
                parentGravity = particles[particleIndex - 1].Position - particles[particleIndex - 2].Position;
            else
                parentGravity *= -1.0f;

            outputValue = CubismMath.DirectionToRadian(parentGravity, translation);

            if (isInverted)
                outputValue *= -1.0f;

            return outputValue;
        }

        private float getOutputScaleTranslationX(CubismVector2 translationScale, float angleScale)
        {
            return translationScale.X;
        }

        private float getOutputScaleTranslationY(CubismVector2 translationScale, float angleScale)
        {
            return translationScale.Y;
        }

        private float getOutputScaleAngle(CubismVector2 translationScale, float angleScale)
        {
            return angleScale;
        }
    }
}

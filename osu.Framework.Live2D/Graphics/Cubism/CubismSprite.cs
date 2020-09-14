// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using CubismFramework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Cubism.Renderer;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES30;
using osuTK.Input;

namespace osu.Framework.Graphics.Cubism
{
    public partial class CubismSprite : Drawable, IBufferedDrawable
    {
        public CubismRenderer Renderer = new CubismRenderer();
        public CubismRenderingManager RenderingManager { get; private set; }

        private CubismAsset asset;
        public CubismAsset Asset
        {
            get => asset;
            set
            {
                if (value == asset) return;

                RenderingManager?.Dispose();
                Asset?.Dispose();
                asset = value;

                if (IsLoaded)
                    initialize();
            }
        }

        public bool IsMoving
        {
            get
            {
                if (Voice?.Playing ?? false)
                    return true;

                if (CanBreathe || CanEyeBlink)
                    return true;

                if ((baseMotionQueue?.IsActive ?? false) || (expressionQueue?.IsActive ?? false) || (effectQueue?.IsActive ?? false))
                    return true;

                return false;
            }
        }

        public int BaseMotionsQueued => baseMotionQueue?.Queued ?? 0;
        public int ExpressionsQueued => expressionQueue?.Queued ?? 0;
        public int EffectsQueued => effectQueue?.Queued ?? 0;

        public SampleChannel Voice;
        private CubismMotionQueueEntry eyeBlinkMotion;
        private CubismMotionQueueEntry breatheMotion;
        private CubismMotionQueue baseMotionQueue;
        private CubismMotionQueue expressionQueue;
        private CubismMotionQueue effectQueue;
        public static string[] PARAMS_EYE = new[] { "ParamEyeLOpen", "ParamEyeROpen" };
        public static string[] PARAMS_LOOK = new[] { "ParamAngleX", "ParamAngleY", "ParamAngleZ", "ParamBodyAngleX" };
        public static string[] PARAMS_BREATH = PARAMS_LOOK.Concat(new[] { "ParamBreath" }).ToArray();

        private bool canBreathe;
        public bool CanBreathe
        {
            get => canBreathe;
            set
            {
                if (canBreathe == value) return;

                canBreathe = value;
                breatheMotion?.Terminate(0);
                ResetParameters(PARAMS_BREATH);

                if (CanBreathe && IsLoaded)
                    initializeBreathing();
            }
        }

        private bool canEyeBlink;
        public bool CanEyeBlink
        {
            get => canEyeBlink;
            set
            {
                if (canEyeBlink == value) return;

                canEyeBlink = value;
                eyeBlinkMotion?.Terminate(0);
                ResetParameters(PARAMS_EYE);

                if (CanEyeBlink && IsLoaded)
                    initializeEyeBlink();
            }
        }

        public CubismLookType LookType = CubismLookType.None;

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaderManager)
        {
            Renderer.ShaderManager = new CubismShaderManager(shaderManager);

            TextureShader = shaderManager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
            RoundedTextureShader = shaderManager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);

            if (Asset != null)
                initialize();
        }

        /// <summary>
        /// Starts a motion from a motion group as defined in the model file.
        /// </summary>
        /// <param name="force">Clears the queue and immediately plays the motion.</param>
        /// <param name="fadeOutTime">If force is true, smoothly fade out.</param>
        /// <returns>If the motion was successfully enqueued.</returns>
        public bool StartMotion(string group, int index, MotionType type = MotionType.Base, bool force = false, bool loop = false, double fadeOutTime = 0)
        {
            if (!Asset.MotionGroups.ContainsKey(group)) return false;

            var motionGroup = Asset.MotionGroups[group];
            if ((index < 0) || (index > motionGroup.Length - 1)) return false;

            var motionQueue = getMotionQueue(type);
            if (force) motionQueue.Stop(fadeOutTime);

            motionQueue.Add(motionGroup[index], loop);
            return true;
        }

        /// <summary>
        /// Ends the current motion and plays the next in queue. This will do nothing if there's no other motions in queue.
        /// </summary>
        public void NextMotion(double fadeOutTime = 0, MotionType type = MotionType.Base) => getMotionQueue(type).Next(fadeOutTime);

        /// <summary>
        /// Pauses or unpauses the current motion.
        /// </summary>
        public void SuspendMotion(bool suspend = true, MotionType type = MotionType.Base) => getMotionQueue(type).Suspend(suspend);

        /// <summary>
        /// Ends the current motion and clears the queue.
        /// </summary>
        public void StopMotion(double fadeOutTime = 0, MotionType type = MotionType.Base) => getMotionQueue(type).Stop(fadeOutTime);

        public bool HasParameter(string name) => Asset.Model.GetParameter(name) != null;

        /// <summary>
        /// Get the current value of a model's parameter.
        /// </summary>
        public double GetParameterValue(string name)
        {
            var parameter = Asset.Model.GetParameter(name);
            if (parameter == null)
                throw new ArgumentException($"Model does not have a parameter named '{name}'.");

            return parameter.Value / parameter.Maximum;
        }

        /// <summary>
        /// Modify a model's parameter.
        /// </summary>
        public void SetParameterValue(string name, double value) => modifyParameterValue(name, (current) => value );

        /// <summary>
        /// Add value to a model's parameter.
        /// </summary>
        public void AddParameterValue(string name, double value) => modifyParameterValue(name, (current) => current += value);

        /// <summary>
        /// Reduce value to a model's parameter.
        /// </summary>
        public void SubtractParameterValue(string name, double value) => modifyParameterValue(name, (current) => current -= value);

        /// <summary>
        /// Resets all model parameters to its defaults.
        /// </summary>
        public void ResetParameters()
        {
            Asset.Model.RestoreDefaultParameters();
            Asset.Model.SaveParameters();

            if (!IsMoving)
                Invalidate(Invalidation.DrawNode);
        }

        /// <summary>
        /// Resets a model's parameter to its default.
        /// </summary>
        public void ResetParameters(IEnumerable<string> parameters)
        {
            Asset.Model.RestoreSavedParameters();
            foreach (string name in parameters)
            {
                var param = Asset.Model.GetParameter(name);
                if (param == null) continue;
                param.Value = param.Default;
            }
            Asset.Model.SaveParameters();
        }

        private void initialize()
        {
            if (CanBreathe)
                initializeBreathing();

            if (CanEyeBlink)
                initializeEyeBlink();

            baseMotionQueue = new CubismMotionQueue(Asset, MotionType.Base);
            expressionQueue = new CubismMotionQueue(Asset, MotionType.Expression);
            effectQueue = new CubismMotionQueue(Asset, MotionType.Effect);

            RenderingManager = new CubismRenderingManager(Renderer, Asset);
        }

        private void initializeBreathing()
        {
            var breathController = new CubismBreath();
            breathController.SetParameter(new CubismBreathParameter(Asset.Model.GetParameter("ParamAngleX"), 0.0, 15.0, 6.5345, 0.5));
            breathController.SetParameter(new CubismBreathParameter(Asset.Model.GetParameter("ParamAngleY"), 0.0, 8.0, 3.5345, 0.5));
            breathController.SetParameter(new CubismBreathParameter(Asset.Model.GetParameter("ParamAngleZ"), 0.0, 10.0, 5.5345, 0.5));
            breathController.SetParameter(new CubismBreathParameter(Asset.Model.GetParameter("ParamBodyAngleX"), 0.0, 4.0, 15.5345, 0.5));
            breathController.SetParameter(new CubismBreathParameter(Asset.Model.GetParameter("ParamBreath"), 0.5, 0.5, 3.2345, 0.5));
            breatheMotion = Asset.StartMotion(MotionType.Effect, breathController);
        }

        private void initializeEyeBlink()
        {
            var eyeBlinkController = new CubismEyeBlink(Asset.ParameterGroups["EyeBlink"]);
            eyeBlinkMotion = Asset.StartMotion(MotionType.Effect, eyeBlinkController);
        }

        private CubismMotionQueue getMotionQueue(MotionType type)
        {
            switch (type)
            {
                case MotionType.Base:
                    return baseMotionQueue;
                case MotionType.Expression:
                    return expressionQueue;
                case MotionType.Effect:
                    return effectQueue;
                default:
                    throw new ArgumentException();
            }
        }

        private void modifyParameterValue(string name, Func<double, double> mod)
        {
            var parameter = Asset.Model.GetParameter(name);
            if (parameter == null)
                throw new ArgumentException($"Model does not have a parameter named '{name}'.");

            Asset.Model.RestoreSavedParameters();
            parameter.Value = parameter.Maximum * mod.Invoke(parameter.Value / parameter.Maximum);
            Asset.Model.SaveParameters();

            // Only invalidate when we're not moving to avoid the calls getting doubled
            if (!IsMoving)
                Invalidate(Invalidation.DrawNode);
        }

        private void lookAtPoint(Vector2 point)
        {
            double dragX = ((point.X - (DrawWidth / 2)) / DrawWidth) * 2;
            double dragY = ((point.Y - (DrawHeight / 2)) / DrawHeight) * 2;
            AddParameterValue("ParamAngleX", dragX * 30);
            AddParameterValue("ParamAngleY", dragY * -30);
            AddParameterValue("ParamAngleZ", dragX * dragY * -3);
            AddParameterValue("ParamBodyAngleX", dragX * 10);

            if (!IsMoving)
                Invalidate(Invalidation.DrawNode);
        }

        protected override void Update()
        {
            base.Update();

            SetParameterValue("ParamMouthOpenY", Voice?.CurrentAmplitudes.Average ?? 0);

            Renderer.DrawSize = DrawSize;
            Asset.Update(Clock.ElapsedFrameTime / 1000);

            baseMotionQueue.Update();
            expressionQueue.Update();
            effectQueue.Update();

            // We just need to invalidate the draw node when we do want to animate
            if (IsMoving)
                Invalidate(Invalidation.DrawNode);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if ((LookType == CubismLookType.Drag && e.IsPressed(MouseButton.Left)) || (LookType == CubismLookType.Hover))
                lookAtPoint(e.MousePosition);
            return base.OnMouseMove(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            if (LookType != CubismLookType.None)
                ResetParameters(PARAMS_LOOK);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (LookType == CubismLookType.Drag && !e.IsPressed(MouseButton.Left))
                ResetParameters(PARAMS_LOOK);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        public Color4 BackgroundColour => new Color4(0, 0, 0, 0);
        public DrawColourInfo? FrameBufferDrawColour => base.DrawColourInfo;
        public Vector2 FrameBufferScale => Vector2.One;
        public IShader TextureShader { get; private set; }
        public IShader RoundedTextureShader { get; private set; }
        private readonly BufferedDrawNodeSharedData sharedData = new BufferedDrawNodeSharedData(new[] { RenderbufferInternalFormat.DepthComponent16 });
        protected override DrawNode CreateDrawNode() => new BufferedDrawNode(this, new CubismSpriteDrawNode(this), sharedData);
    }
}
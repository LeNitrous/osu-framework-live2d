// Copyright 2020 - 2021 Vignette Project
// Licensed under MIT. See LICENSE for details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using Vignette.Game.Live2D.Extensions;
using Vignette.Game.Live2D.Graphics.Controllers;
using Vignette.Game.Live2D.IO;
using Vignette.Game.Live2D.IO.Serialization;
using Vignette.Game.Live2D.Model;

namespace Vignette.Game.Live2D.Graphics
{
    /// <summary>
    /// A drawable that renders a Live2D model.
    /// </summary>
    [Cached]
    [Cached(typeof(ICubismResourceProvider))]
    public unsafe class CubismModel : CompositeDrawable, ICubismResourceProvider
    {
        private readonly CubismMoc moc;
        private readonly IntPtr buffer;
        private readonly IntPtr handle;

        private readonly List<CubismParameter> parameters = new List<CubismParameter>();
        private readonly List<CubismPart> parts = new List<CubismPart>();

        /// <summary>
        /// The moc file version of this model. See <see cref="CubismMocVersion"/> for valid values.
        /// </summary>
        public CubismMocVersion Version => moc.Version;

        /// <summary>
        /// A collection of drawables used to draw this model.
        /// </summary>
        public IEnumerable<CubismDrawable> Drawables => InternalChildren.OfType<CubismDrawable>();

        /// <summary>
        /// A collection of parameters used to manipulate the model.
        /// </summary>
        public IReadOnlyList<CubismParameter> Parameters => parameters;

        /// <summary>
        /// A collection of parts used to manipulate groups of the model.
        /// </summary>
        public IReadOnlyList<CubismPart> Parts => parts;

        public CubismModel(CubismMoc moc)
        {
            this.moc = moc;

            int size = CubismCore.csmGetSizeofModel(moc.Handle);
            buffer = Marshal.AllocHGlobal(size + CubismCore.ALIGN_OF_MODEL - 1);
            handle = CubismCore.csmInitializeModelInPlace(moc.Handle, buffer.Align(CubismCore.ALIGN_OF_MODEL), size);
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            if (resources != null)
                largeTextureStore = new LargeTextureStore(host.CreateTextureLoaderStore(resources));

            initializeDrawables();
        }

        #region Drawables

        private int* vertexCounts;

        private void initializeDrawables()
        {
            vertexCounts = CubismCore.csmGetDrawableVertexCounts(handle);

            int count = CubismCore.csmGetDrawableCount(handle);
            var names = CubismCore.csmGetDrawableIds(handle);

            var vertexUvs = CubismCore.csmGetDrawableVertexUvs(handle);
            var maskCounts = CubismCore.csmGetDrawableMaskCounts(handle);
            var masks = CubismCore.csmGetDrawableMasks(handle);
            var indexCounts = CubismCore.csmGetDrawableIndexCounts(handle);
            var indices = CubismCore.csmGetDrawableIndices(handle);
            var textureIds = CubismCore.csmGetDrawableTextureIndices(handle);
            var constantFlags = CubismCore.csmGetDrawableConstantFlags(handle);

            for (int i = 0; i < count; i++)
            {
                var flags = constantFlags[i];
                var drawable = new CubismDrawable
                {
                    ID = i,
                    Name = Marshal.PtrToStringAnsi((IntPtr)names[i]),
                    Masks = pointerToArray<int>(masks[i], maskCounts[i]),
                    Texture = largeTextureStore?.Get(modelSetting?.FileReferences.Textures[textureIds[i]] ?? string.Empty),
                    Indices = pointerToArray<short>(indices[i], indexCounts[i]),
                    IsDoubleSided = flags.HasFlagFast(CubismConstantFlags.IsDoubleSided),
                    IsInvertedMask = flags.HasFlagFast(CubismConstantFlags.IsInvertedMask),
                    TexturePositions = pointerToArray<Vector2>(vertexUvs[i], vertexCounts[i]),
                };

                drawable.RenderOrderChanged += (drawable, depth) => ChangeInternalChildDepth(drawable, depth);

                if (flags.HasFlagFast(CubismConstantFlags.BlendAdditive))
                {
                    drawable.Blending = new BlendingParameters
                    {
                        Source = BlendingType.One,
                        Destination = BlendingType.One,
                        SourceAlpha = BlendingType.Zero,
                        DestinationAlpha = BlendingType.One,
                    };
                }
                else if (flags.HasFlagFast(CubismConstantFlags.BlendMultiplicative))
                {
                    drawable.Blending = new BlendingParameters
                    {
                        Source = BlendingType.DstColor,
                        Destination = BlendingType.OneMinusSrcAlpha,
                        SourceAlpha = BlendingType.Zero,
                        DestinationAlpha = BlendingType.One,
                    };
                }
                else if (flags.HasFlagFast(CubismConstantFlags.BlendNormal))
                {
                    drawable.Blending = new BlendingParameters
                    {
                        Source = BlendingType.One,
                        Destination = BlendingType.OneMinusSrcAlpha,
                        SourceAlpha = BlendingType.One,
                        DestinationAlpha = BlendingType.OneMinusSrcAlpha,
                    };
                }

                AddInternal(drawable);
            }
        }

        private void updateDrawables()
        {
            var vertices = CubismCore.csmGetDrawableVertexPositions(handle);
            var opacities = CubismCore.csmGetDrawableOpacities(handle);
            var renderOrders = CubismCore.csmGetDrawableRenderOrders(handle);

            foreach (var drawable in Drawables.ToArray())
            {
                int i = drawable.ID;
                drawable.Alpha = opacities[i];
                drawable.RenderOrder = renderOrders[i];
                drawable.VertexPositions = pointerToArray<Vector2>(vertices[i], vertexCounts[i]);
            }
        }

        #endregion

        protected override void Update()
        {
            base.Update();
            CubismCore.csmUpdateModel(handle);
            updateDrawables();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                Marshal.FreeHGlobal(buffer);

            base.Dispose(isDisposing);
        }

        private static T[] pointerToArray<T>(void* pointer, int length)
        {
            var type = typeof(T);
            var output = new T[length];

            int size = Marshal.SizeOf<T>();

            if (type.IsPrimitive)
            {
                var handle = GCHandle.Alloc(output, GCHandleType.Pinned);
                var byteLength = length * size;

                var destination = (byte*)handle.AddrOfPinnedObject().ToPointer();

                for (int i = 0; i < byteLength; i++)
                    destination[i] = ((byte*)pointer)[i];

                handle.Free();
            }
            else if (type.IsValueType)
            {
                if (!type.IsLayoutSequential && !type.IsExplicitLayout)
                    throw new ArgumentException($"{type} does not define a StructLayout attribute.");

                for (int i = 0; i < length; i++)
                {
                    var offset = IntPtr.Add((IntPtr)pointer, i * size);
                    output[i] = Marshal.PtrToStructure<T>(offset);
                }
            }
            else
            {
                throw new ArgumentException($"{type} is not a supported type.");
            }

            return output;
        }

        #region Controllers

        /// <summary>
        /// Adds a controller to this <see cref="CubismModel"/>.
        /// </summary>
        public void Add(CubismController controller) => AddInternal(controller);

        /// <summary>
        /// Adds a range of controllers to this <see cref="CubismModel"/>.
        /// </summary>
        public void AddRange(IEnumerable<CubismController> controllers) => AddRangeInternal(controllers);

        /// <summary>
        /// Removes a given controller in this <see cref="CubismModel"/>.
        /// </summary>
        /// <returns>False if the controller is not present on this <see cref="CubismModel"/>and true otherwise.</returns>
        public bool Remove(CubismController controller) => RemoveInternal(controller);

        /// <summary>
        /// The list of controllers this <see cref="CubismModel"/> has.
        /// Assigning to this property will dispose all existing controllers.
        /// </summary>
        public IEnumerable<CubismController> Children
        {
            get => InternalChildren.OfType<CubismController>();
            set
            {
                foreach (var controller in Children)
                    Remove(controller);

                AddRange(value);
            }
        }

        #endregion

        #region ICubismResourceProvider

        private LargeTextureStore largeTextureStore;
        private readonly CubismModelSetting modelSetting;
        private readonly IResourceStore<byte[]> resources;

        CubismModelSetting ICubismResourceProvider.Settings => modelSetting;

        LargeTextureStore ICubismResourceProvider.Textures => largeTextureStore;

        IResourceStore<byte[]> ICubismResourceProvider.Resources => resources;

        #endregion
    }
}

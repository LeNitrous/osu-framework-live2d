// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Text.RegularExpressions;
using CubismFramework;
using osu.Framework.IO.Stores;

namespace osu.Framework.Graphics.Cubism
{
    public class CubismAssetStore : ResourceStore<byte[]>
    {
        public CubismAssetStore(IResourceStore<byte[]> store = null)
            : base(store)
        {
        }

        /// <summary>
        /// Loads a <see cref="CubismAsset"/> used for <see cref="CubismSprite"/>.
        /// </summary>
        /// <param name="name">The path to the model json file.</param>
        /// <returns>The loaded CubismAsset</returns>
        public new CubismAsset Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            try
            {
                var baseDir = name.Split('.', 2)[0];
                var mdlPath = name.Substring(baseDir.Length + 1, (name.Length - baseDir.Length) - 1);
                return new CubismAsset($"{baseDir}/{mdlPath}", Retrieve);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// The loader method for <see cref="CubismAsset"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        protected virtual Stream Retrieve(string path)
        {
            // CubismFileLoader internally uses System.IO.Path
            path = path.Replace(Path.DirectorySeparatorChar, '.');
            path = path.Replace(Path.AltDirectorySeparatorChar, '.');

            // osu!framework prepends an underscore to numbers as file names
            var matches = new Regex(@"\.(\d+)").Matches(path);
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                var start = groups[1].Index;
                var end = start + groups[1].Length;
                path = path.Substring(0, start) + $"_{groups[1].Value}" + path.Substring(end, path.Length - end);
            }

            return GetStream(path);
        }
    }
}
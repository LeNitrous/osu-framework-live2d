// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using osu.Framework;
using osu.Framework.Platform;

namespace Vignette.Application.Live2D.Tests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using GameHost host = Host.GetSuitableHost(@"Vignette");
            host.Run(new VisualTestGame());
        }
    }
}

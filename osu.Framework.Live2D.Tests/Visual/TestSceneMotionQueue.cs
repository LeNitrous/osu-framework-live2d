// Copyright (c) Nitrous <n20gaming2000@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Reflection;
using CubismFramework;
using osu.Framework.Allocation;

namespace osu.Framework.Live2D.Tests.Visual
{
    [System.ComponentModel.Description("Add or remove motions in queue")]
    public class TestSceneMotionQueue : TestSceneBase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddLabel("Basic Motion");
            AddStep("enqueue motion", () => Sprite.StartMotion("Idle", 0, MotionType.Base));
            AddAssert("check motion", () => Sprite.IsMoving);
            AddStep("stop queue", () => Sprite.StopMotion());

            // The first motion is popped off from the queue and is played in the next update frame
            AddLabel("Queued Motions");
            AddRepeatStep("enqueue motions", () => Sprite.StartMotion("Idle", 0, MotionType.Base), 5);
            AddAssert("check queue count", () => Sprite.BaseMotionsQueued  == 4);
            AddRepeatStep("skip motions", () => Sprite.NextMotion(), 4);
            AddAssert("check queue if empty", () => Sprite.BaseMotionsQueued == 0);

            AddLabel("Forced Motion");
            AddRepeatStep("enqueue motions", () => Sprite.StartMotion("Idle", 0, MotionType.Base), 5);
            AddStep("enqueue forced motion", () => Sprite.StartMotion("Idle", 1, MotionType.Base, true));
            AddAssert("check queue if empty", () => Sprite.BaseMotionsQueued == 0);

            AddLabel("Looping Motion");
            AddStep("enqueue looping motion", () => Sprite.StartMotion("Idle", 2, MotionType.Base, true, true));
            AddWaitStep("wait", 30);
            AddAssert("check motion", () => Sprite.IsMoving);
            AddStep("stop motion", () => Sprite.StopMotion());
            AddAssert("check if stopped", () => !Sprite.IsMoving);

            Add(new ParameterMonitor(Sprite, Parameters));
        }
    }
}
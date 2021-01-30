// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;
using osuTK;

namespace Vignette.Application.Live2D.Utils
{
    public static class CubismMath
    {
        public static float Maximum(float left, float right) => (left > right) ? left : right;

        public static float Minimum(float left, float right) => (left < right) ? left : right;

        public static double EaseSine(double t) => Math.Clamp(0.5f - 0.5f * Math.Cos(Math.PI * t), 0.0f, 1.0f);

        public static float DegreesToRadian(float degrees) => (degrees / 180.0f) * MathF.PI;

        public static float RadianToDegrees(float radians) => (radians * 180.0f) * MathF.PI;

        public static float DirectionToRadian(CubismVector2 from, CubismVector2 to)
        {
            float q1, q2, ret;

            q1 = MathF.Atan2(to.Y, to.X);
            q2 = MathF.Atan2(from.Y, from.X);
            ret = q1 - q2;

            while (ret < -MathF.PI)
                ret += MathF.PI * 2.0f;

            while (ret > MathF.PI)
                ret -= MathF.PI * 2.0f;

            return ret;
        }

        public static float DirectionToDegrees(CubismVector2 from, CubismVector2 to)
        {
            float radian, degree;

            radian = DirectionToRadian(from, to);
            degree = RadianToDegrees(radian);

            if ((to.X - from.X) > 0)
                degree = -degree;

            return degree;
        }

        public static CubismVector2 RadianToDirection(float totalAngle) => new CubismVector2(MathF.Sin(totalAngle), MathF.Cos(totalAngle));
    }
}

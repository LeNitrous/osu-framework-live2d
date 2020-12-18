﻿// Copyright 2020 - 2021 Vignette Project
// Licensed under MIT. See LICENSE for details.

using System;
using osuTK;

namespace Vignette.Application.Live2D.Utils
{
    public static class CubismMath
    {
        public static float EaseSine(float t) => Math.Clamp(0.5f - 0.5f * MathF.Cos(MathF.PI * t), 0, 1);

        public static float DegreesToRadian(float degrees) => (degrees / 180.0f) * MathF.PI;

        public static float RadianToDegrees(float radians) => (radians * 180.0f) * MathF.PI;

        public static float DirectionToRadian(Vector2 from, Vector2 to)
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

        public static float DirectionToDegrees(Vector2 from, Vector2 to)
        {
            float radian, degree;

            radian = DirectionToRadian(from, to);
            degree = RadianToDegrees(radian);

            if ((to.X - from.X) > 0)
                degree = -degree;

            return degree;
        }

        public static Vector2 RadianToDirection(float totalAngle) => new Vector2(MathF.Sin(totalAngle), MathF.Cos(totalAngle));
    }
}

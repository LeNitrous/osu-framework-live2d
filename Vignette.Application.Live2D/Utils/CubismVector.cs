// Copyright 2020 - 2021 Vignette Project
// Licensed under MIT. See LICENSE for details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;

namespace Vignette.Application.Live2D.Utils
{
    /// <summary>
    /// Implements CubismVector2 API from CubismNative.
    /// </summary>
    public class CubismVector2
    {
        internal float X, Y;

        public CubismVector2(float x, float y)
        { }

        #region Operator Overloads
        // HACK: As C# doesn't allow compound overloads, we'll just implement all the possible operators that will be used.
        public static CubismVector2 operator +(CubismVector2 a, CubismVector2 b) => new(a.X + b.X, a.Y + b.Y);

        public static CubismVector2 operator -(CubismVector2 a, CubismVector2 b) => new(a.X - b.X, a.Y - b.Y);

        public static CubismVector2 operator *(CubismVector2 a, CubismVector2 b) => new(a.X * b.X, a.Y * b.Y);

        public static CubismVector2 operator /(CubismVector2 a, CubismVector2 b) => new(a.X / b.X, a.Y / b.Y);

        public static bool operator ==(CubismVector2 a, CubismVector2 b) => (a.X == b.X) && (a.Y == b.Y);

        public static bool operator !=(CubismVector2 a, CubismVector2 b) => !(a == b);

        public static CubismVector2 operator *(CubismVector2 vector, float scalar) => new(vector.X * scalar, vector.Y * scalar);

        public static CubismVector2 operator *(float scalar, CubismVector2 vector) => new(vector.X * scalar, vector.Y * scalar);

        public static CubismVector2 operator /(CubismVector2 vector, float scalar) => new(vector.X / scalar, vector.Y / scalar);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Methods
        public float GetLength()
        {
            return MathF.Sqrt(X * X + Y * Y);
        }
        public float GetDistanceWith(CubismVector2 a)
        {
            // Weird PEMDAS goes here
            // I'm losing my mind please help me.
            return MathF.Sqrt(((X - a.X) * (X - a.X) ) + ((Y - a.Y) * (Y - a.Y)));
        }

        public float Dot(CubismVector2 a)
        {
            return (X * a.X) + (Y * a.Y);
        }

        public void Normalize()
        {
            float length = MathF.Pow((X * X) + (Y * Y), 0.5f);

            X /= length;
            Y /= length;
        }
        #endregion
    }
}

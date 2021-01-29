// Copyright 2020 - 2021 Vignette Project
// Licensed under MIT. See LICENSE for details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

using System;

namespace Vignette.Application.Live2D.Utils
{
    /// <summary>
    /// Implements CubismVector2 API from CubismNative. Vector2 is 2D Vector that contains two single precision floating point numbers.
    /// </summary>
    public class CubismVector2
    {
        public float X, Y;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="value">The value that will initialize this instance.</param>
        public CubismVector2(float value)
        {
            X = value;
            Y = value;
        }

        /// <summary>
        /// Constructs a new Vector2.
        /// </summary>
        /// <param name="x">The x coordinate of the net Vector2.</param>
        /// <param name="y">The y coordinate of the net Vector2.</param>
        public CubismVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

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

        #region Fields
        /// <summary>
        /// Defines an instance with all components set to 1.
        /// </summary>
        public static readonly CubismVector2 Zero = new CubismVector2(0, 0);

        /// <summary>
        /// Defines an instance with all components set to 1.
        /// </summary>
        public static readonly CubismVector2 One = new CubismVector2(1, 1);

        /// <summary>
        /// Defines a unit-length Vector2 that points towards the X-axis.
        /// </summary>
        public static readonly CubismVector2 UnitX = new CubismVector2(1, 0);

        /// <summary>
        /// Defines a zero-length Vector2.
        /// </summary>
        public static readonly CubismVector2 UnitY = new CubismVector2(0, 1);
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct Matrix3x3 : IEquatable<Matrix3x3>
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;

    private static readonly Matrix3x3 identityMatrix = new(1, -0, 0, -0, 1, 0, 0, 0, 1);

    public static Matrix3x3 identity
    {
        get => Matrix3x3.identityMatrix;
    }

    public Matrix3x3(float f0, float f1, float f2, float f3, float f4, float f5, float f6, float f7, float f8)
    {
        m00 = f0;
        m01 = f1;
        m02 = f2;

        m10 = f3;
        m11 = f4;
        m12 = f5;

        m20 = f6;
        m21 = f7;
        m22 = f8;
    }


    public static Vector3 operator *(Matrix3x3 lhs, Vector3 vector)
    {
        Vector3 vector3;
        vector3.x = (float) ((double) lhs.m00 * (double) vector.x + (double) lhs.m01 * (double) vector.y + (double) lhs.m02 * (double) vector.z );
        vector3.y = (float) ((double) lhs.m10 * (double) vector.x + (double) lhs.m11 * (double) vector.y + (double) lhs.m12 * (double) vector.z );
        vector3.z = (float) ((double) lhs.m20 * (double) vector.x + (double) lhs.m21 * (double) vector.y + (double) lhs.m22 * (double) vector.z );
        return vector3;
    }


    // PSD文件中使用右乘矩阵
    public static Vector3 operator *(Vector3 vector, Matrix3x3 lhs)
    {
        Vector3 vector3;
        vector3.x = (float) ((double) lhs.m00 * (double) vector.x + (double) lhs.m10 * (double) vector.y + (double) lhs.m20 * (double) vector.z );
        vector3.y = (float) ((double) lhs.m01 * (double) vector.x + (double) lhs.m11 * (double) vector.y + (double) lhs.m21 * (double) vector.z );
        vector3.z = (float) ((double) lhs.m02 * (double) vector.x + (double) lhs.m12 * (double) vector.y + (double) lhs.m22 * (double) vector.z );
        return vector3;
    }

    public bool Equals(Matrix3x3 other)
    {
        return m00.Equals(other.m00) && m01.Equals(other.m01) && m02.Equals(other.m02) &&
               m10.Equals(other.m10) && m11.Equals(other.m11) && m12.Equals(other.m12) &&
               m20.Equals(other.m20) && m21.Equals(other.m21) && m22.Equals(other.m22);
    }

    public override bool Equals(object obj)
    {
        return obj is Matrix3x3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(m00);
        hashCode.Add(m01);
        hashCode.Add(m02);
        hashCode.Add(m10);
        hashCode.Add(m11);
        hashCode.Add(m12);
        hashCode.Add(m20);
        hashCode.Add(m21);
        hashCode.Add(m22);
        return hashCode.ToHashCode();
    }
}
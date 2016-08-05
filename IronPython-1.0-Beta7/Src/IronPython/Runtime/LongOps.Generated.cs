/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronMath;

namespace IronPython.Runtime {
    public static partial class LongOps {
        #region Generated LongOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(BigInteger x, object other) {
            if (other is int) return x + ((int)other);
            if (other is Complex64) return x + ((Complex64)other);
            if (other is double) return x + ((double)other);
            if (other is BigInteger) return x + ((BigInteger)other);
            if (other is bool) return x + ((bool) other ? 1 : 0);
            if (other is long) return x + ((long)other);
            if (other is ExtensibleInt) return x + (((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return x + (((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return x + ((ExtensibleComplex)other).value;
            if (other is byte) return x + (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(BigInteger x, object other) {
            if (other is int) return x - ((int)other);
            if (other is Complex64) return x - ((Complex64)other);
            if (other is double) return x - ((double)other);
            if (other is BigInteger) return x - ((BigInteger)other);
            if (other is bool) return x - ((bool) other ? 1 : 0);
            if (other is long) return x - ((long)other);
            if (other is ExtensibleInt) return x - (((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return x - (((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return x - ((ExtensibleComplex)other).value;
            if (other is byte) return x - (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(BigInteger x, object other) {
            if (other is int) return Power(x, (int)other);
            if (other is BigInteger) return Power(x, (BigInteger)other);
            if (other is double) return Power(x, (double)other);
            if (other is Complex64) return ComplexOps.Power(x, (Complex64)other);
            if (other is bool) return Power(x, (bool)other ? 1 : 0);
            if (other is long) return Power(x, (long)other);
            if (other is ExtensibleInt) return Power(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return Power(x, ((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return Power(x, ((ExtensibleComplex)other).value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(BigInteger x, object other) {
            if (other is int) return x * ((int)other);
            if (other is Complex64) return x * ((Complex64)other);
            if (other is double) return x * ((double)other);
            if (other is BigInteger) return x * ((BigInteger)other);
            if (other is bool) return x * ((bool) other ? 1 : 0);
            if (other is long) return x * ((long)other);
            if (other is ExtensibleInt) return x * (((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return x * (((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return x * ((ExtensibleComplex)other).value;
            if (other is byte) return x * (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(BigInteger x, object other) {
            if (other is int) return Divide(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.Divide(x, (double)other);
            if (other is bool) return Divide(x, (bool)other ? 1 : 0);
            if (other is long) return Divide(x, (long)other);
            if (other is BigInteger) return Divide(x, (BigInteger)other);
            if (other is ExtensibleInt) return Divide(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            if (other is ExtensibleFloat) return FloatOps.Divide(x, ((ExtensibleFloat)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__rdiv__")]
        public static object ReverseDivide(BigInteger x, object other) {
            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Divide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if (other is BigInteger) return Divide((BigInteger)other, x);
            if (other is ExtensibleInt) return Divide(((ExtensibleInt)other).value, x);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            if (other is ExtensibleFloat) return FloatOps.Divide(((ExtensibleFloat)other).value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__floordiv__")]
        public static object FloorDivide(BigInteger x, object other) {
            if (other is int) return Divide(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.FloorDivide(x, (double)other);
            if (other is bool) return Divide(x, (bool)other ? 1 : 0);
            if (other is long) return Divide(x, (long)other);
            if (other is BigInteger) return Divide(x, (BigInteger)other);
            if (other is ExtensibleInt) return Divide(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Divide(x, (int)((byte)other));
            if (other is ExtensibleFloat) return FloatOps.FloorDivide(x, ((ExtensibleFloat)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__rfloordiv__")]
        public static object ReverseFloorDivide(BigInteger x, object other) {
            if (other is int) return IntOps.Divide((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.FloorDivide((double)other, x);
            if (other is bool) return Divide((bool)other ? 1 : 0, x);
            if (other is long) return Divide((long)other, x);
            if (other is BigInteger) return Divide((BigInteger)other, x);
            if (other is ExtensibleInt) return Divide(((ExtensibleInt)other).value, x);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Divide((int)((byte)other), x);
            if (other is ExtensibleFloat) return FloatOps.FloorDivide(((ExtensibleFloat)other).value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__truediv__")]
        public static object TrueDivide(BigInteger x, object other) {
            if (other is int) return TrueDivide(x, (int)other);
            if (other is BigInteger) return TrueDivide(x, (BigInteger)other);
            if (other is double) return TrueDivide(x, (double)other);
            if (other is Complex64) return ComplexOps.TrueDivide(x, (Complex64)other);
            if (other is bool) return TrueDivide(x, (bool)other ? 1 : 0);
            if (other is long) return TrueDivide(x, (long)other);
            if (other is ExtensibleInt) return TrueDivide(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleFloat) return TrueDivide(x, ((ExtensibleFloat)other).value);
            if (other is ExtensibleComplex) return TrueDivide(x, ((ExtensibleComplex)other).value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(BigInteger x, object other) {
            if (other is int) return Mod(x, (int)other);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is double) return FloatOps.Mod(x, (double)other);
            if (other is bool) return Mod(x, (bool)other ? 1 : 0);
            if (other is long) return Mod(x, (long)other);
            if (other is BigInteger) return Mod(x, (BigInteger)other);
            if (other is ExtensibleInt) return Mod(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is byte) return Mod(x, (int)((byte)other));
            if (other is ExtensibleFloat) return FloatOps.Mod(x, ((ExtensibleFloat)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__rmod__")]
        public static object ReverseMod(BigInteger x, object other) {
            if (other is int) return IntOps.Mod((int)other, x);
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is double) return FloatOps.Mod((double)other, x);
            if (other is bool) return Mod((bool)other ? 1 : 0, x);
            if (other is long) return Mod((long)other, x);
            if (other is BigInteger) return Mod((BigInteger)other, x);
            if (other is ExtensibleInt) return Mod(((ExtensibleInt)other).value, x);
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(y, Complex64.MakeReal(x));
            }
            if (other is byte) return IntOps.Mod((int)((byte)other), x);
            if (other is ExtensibleFloat) return FloatOps.Mod(((ExtensibleFloat)other).value, x);
            return Ops.NotImplemented;
        }



        [PythonName("__and__")]
        public static object BitwiseAnd(BigInteger x, object other) {
            if (other is BigInteger) return x & (BigInteger)other;
            if (other is long) return x & (long)other;
            if (other is int) return x & (int)other;
            if (other is bool) return x & ((bool)other ? 1 : 0);
            if (other is ExtensibleInt) return x & ((ExtensibleInt)other).value;
            if (other is byte) return x & (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(BigInteger x, object other) {
            if (other is BigInteger) return x | (BigInteger)other;
            if (other is long) return x | (long)other;
            if (other is int) return x | (int)other;
            if (other is bool) return x | ((bool)other ? 1 : 0);
            if (other is ExtensibleInt) return x | ((ExtensibleInt)other).value;
            if (other is byte) return x | (int)((byte)other);
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(BigInteger x, object other) {
            if (other is BigInteger) return x ^ (BigInteger)other;
            if (other is long) return x ^ (long)other;
            if (other is int) return x ^ (int)other;
            if (other is bool) return x ^ ((bool)other ? 1 : 0);
            if (other is ExtensibleInt) return x ^ ((ExtensibleInt)other).value;
            if (other is byte) return x ^ (int)((byte)other);
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}

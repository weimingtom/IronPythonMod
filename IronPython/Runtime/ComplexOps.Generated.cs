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
    public static partial class ComplexOps {
        #region Generated ComplexOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(Complex64 x, object other) {
            if (other is int) {
                return x + (int)other;
            } else if (other is Complex64) {
                return x + (Complex64)other;
            } else if (other is double) {
                return x + (double)other;
            } else if (other is BigInteger) {
                return x + (BigInteger)other;
            } else if (other is long) {
                return x + (long)other;
            } else if (other is ExtensibleComplex) {
                return x + ((ExtensibleComplex)other).value;
            } else if (other is ExtensibleInt) {
                return x + ((ExtensibleInt)other).value;
            } else if (other is ExtensibleFloat) {
                return x + ((ExtensibleFloat)other).value;
            } else if(other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(Complex64 x, object other) {
            if (other is int) {
                return x - (int)other;
            } else if (other is Complex64) {
                return x - (Complex64)other;
            } else if (other is double) {
                return x - (double)other;
            } else if (other is BigInteger) {
                return x - (BigInteger)other;
            } else if (other is long) {
                return x - (long)other;
            } else if (other is ExtensibleComplex) {
                return x - ((ExtensibleComplex)other).value;
            } else if (other is ExtensibleInt) {
                return x - ((ExtensibleInt)other).value;
            } else if (other is ExtensibleFloat) {
                return x - ((ExtensibleFloat)other).value;
            } else if(other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__pow__")]
        public static object Power(Complex64 x, object other) {
            if (other is int) return Power(x, (Complex64) ((int)other));
            if (other is Complex64) return Power(x, (Complex64) other);
            if (other is double) return Power(x, (Complex64) ((double) other));
            if (other is BigInteger) return Power(x, (Complex64) ((BigInteger) other));
            if (other is bool) return Power(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return Power(x, (Complex64)((long) other));
            if (other is ExtensibleComplex) return Power(x, ((ExtensibleComplex)other).value);
            if (other is ExtensibleFloat) return Power(x, (Complex64)((ExtensibleFloat)other).value);
            if (other is ExtensibleInt) return Power(x, (Complex64)((ExtensibleInt)other).value);
            if (other is byte) return Power(x, (Complex64) (int)((byte)other));
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(Complex64 x, object other) {
            if (other is int) {
                return x * (int)other;
            } else if (other is Complex64) {
                return x * (Complex64)other;
            } else if (other is double) {
                return x * (double)other;
            } else if (other is BigInteger) {
                return x * (BigInteger)other;
            } else if (other is long) {
                return x * (long)other;
            } else if (other is ExtensibleComplex) {
                return x * ((ExtensibleComplex)other).value;
            } else if (other is ExtensibleInt) {
                return x * ((ExtensibleInt)other).value;
            } else if (other is ExtensibleFloat) {
                return x * ((ExtensibleFloat)other).value;
            } else if(other is string) {
                return Ops.NotImplemented;
            } else if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(Complex64 x, object other) {
            if (other is int) return TrueDivide(x, (Complex64) ((int)other));
            if (other is Complex64) return TrueDivide(x, (Complex64) other);
            if (other is double) return TrueDivide(x, (Complex64) ((double) other));
            if (other is BigInteger) return TrueDivide(x, (Complex64) ((BigInteger) other));
            if (other is bool) return TrueDivide(x, (Complex64)((bool)other ? 1 : 0));
            if (other is long) return TrueDivide(x, (Complex64)((long) other));
            if (other is ExtensibleComplex) return TrueDivide(x, ((ExtensibleComplex)other).value);
            if (other is ExtensibleFloat) return TrueDivide(x, (Complex64)((ExtensibleFloat)other).value);
            if (other is ExtensibleInt) return TrueDivide(x, (Complex64)((ExtensibleInt)other).value);
            if (other is byte) return TrueDivide(x, (Complex64) (int)((byte)other));
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion
    }
}

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
    public static partial class FloatOps {
        #region Generated FloatOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(double x, object other) {
            if (other is double) return x + ((double)other);
            if (other is int) return x + ((int)other);
            if (other is Complex64) return ComplexOps.Add(Complex64.MakeReal(x), (Complex64)other);
            if (other is BigInteger) return x + ((BigInteger)other);
            if (other is float) return x + ((float)other);
            if (other is ExtensibleFloat) return x + ((ExtensibleFloat)other).value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x + y;
            }
            if (other is long) return x + ((long)other);
            if (other is ExtensibleInt) return x + ((ExtensibleInt)other).value;
            if (other is ExtensibleComplex) return ComplexOps.Add(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(double x, object other) {
            if (other is double) return x - ((double)other);
            if (other is int) return x - ((int)other);
            if (other is Complex64) return ComplexOps.Subtract(Complex64.MakeReal(x), (Complex64)other);
            if (other is BigInteger) return x - ((BigInteger)other);
            if (other is float) return x - ((float)other);
            if (other is ExtensibleFloat) return x - ((ExtensibleFloat)other).value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x - y;
            }
            if (other is long) return x - ((long)other);
            if (other is ExtensibleInt) return x - ((ExtensibleInt)other).value;
            if (other is ExtensibleComplex) return ComplexOps.Subtract(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(double x, object other) {
            if (other is double) return x * ((double)other);
            if (other is int) return x * ((int)other);
            if (other is Complex64) return ComplexOps.Multiply(Complex64.MakeReal(x), (Complex64)other);
            if (other is BigInteger) return x * ((BigInteger)other);
            if (other is float) return x * ((float)other);
            if (other is ExtensibleFloat) return x * ((ExtensibleFloat)other).value;
            if (other is string) return Ops.NotImplemented;
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                return x * y;
            }
            if (other is long) return x * ((long)other);
            if (other is ExtensibleInt) return x * ((ExtensibleInt)other).value;
            if (other is ExtensibleComplex) return ComplexOps.Multiply(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(double x, object other) {
            if (other is double) return TrueDivide(x, ((double)other));
            if (other is int) return TrueDivide(x, ((int)other));
            if (other is Complex64) return ComplexOps.TrueDivide(Complex64.MakeReal(x), (Complex64)other);
            if (other is BigInteger) return TrueDivide(x, ((BigInteger)other));
            if (other is bool) return TrueDivide(x, (bool)other ? 1.0 : 0.0);
            if (other is float) return TrueDivide(x, ((float)other));
            if (other is ExtensibleFloat) return TrueDivide(x, ((ExtensibleFloat)other).value);
            if (other is long) return TrueDivide(x, ((long)other));
            if (other is ExtensibleComplex) return ComplexOps.TrueDivide(Complex64.MakeReal(x), ((ExtensibleComplex)other).value);
            if (other is ExtensibleInt) return TrueDivide(x, ((ExtensibleInt)other).value);
            if (other is byte) return TrueDivide(x, (int)((byte)other));
           return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}

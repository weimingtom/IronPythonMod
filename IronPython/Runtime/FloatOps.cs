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
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using IronMath;

namespace IronPython.Runtime {
    public class ExtensibleFloat : IRichComparable, IComparable {
        public double value;

        public ExtensibleFloat() { this.value = 0; }
        public ExtensibleFloat(double value) { this.value = value; }

        public override string ToString() {
            return value.ToString();
        }

        [PythonName("__cmp__")]
        public virtual object Compare(object other) {
            Conversion conv;
            double val = Converter.TryConvertToDouble(other, out conv);
            if (conv == Conversion.None) return Ops.NotImplemented;
            if (val == value) return 0;
            if (value < val) return -1;
            return 1;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj) {
            object res = Compare(obj);
            if (res == Ops.NotImplemented) throw Ops.TypeError("cannot compare {0} to float", Ops.GetDynamicType(obj).__name__);
            return (int)res;
        }

        #endregion

        #region IRichComparable Members

        public object CompareTo(object other) {
            if (other == null) return 1;

            double otherDbl;
            Conversion conv;

            if (other is float) {
                return FloatOps.Compare(value, (int)other);
            } else if (other is ExtensibleFloat) {
                return FloatOps.Compare(value, ((ExtensibleFloat)other).value);
            } else if (other is bool) {
                return FloatOps.Compare(value, ((bool)other) ? 1 : 0);
            } else if (other is int) {
                return FloatOps.Compare(value, (double)((int)other));
            } else if (other is ExtensibleInt) {
                return FloatOps.Compare(value, (double)((ExtensibleInt)other).value);
            } else {
                otherDbl = Converter.TryConvertToDouble(other, out conv);
                if (conv != Conversion.None) return FloatOps.Compare(value, otherDbl);
            }

            return Ops.NotImplemented;
        }

        public object GreaterThan(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) > 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThan(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) < 0;
            }
            return Ops.NotImplemented;
        }

        public object GreaterThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) >= 0;
            }
            return Ops.NotImplemented;
        }

        public object LessThanOrEqual(object other) {
            object res = CompareTo(other);
            if (res is int) {
                return ((int)res) <= 0;
            }
            return Ops.NotImplemented;
        }

        #endregion

        #region IRichComparable

        [PythonName("__hash__")]
        public object RichGetHashCode() {
            return Ops.NotImplemented;
        }

        [PythonName("__eq__")]
        public object RichEquals(object other) {
            ExtensibleFloat ei = other as ExtensibleFloat;
            if (ei != null) return Ops.Bool2Object(value == ei.value);
            if (other is double) return Ops.Bool2Object(value == (double)other);
            if (other is float) return Ops.Bool2Object(value == (float)other);

            return Ops.NotImplemented;
        }

        [PythonName("__ne__")]
        public object RichNotEquals(object other) {
            object res = RichEquals(other);
            if (res != Ops.NotImplemented) return Ops.Not(res);

            return Ops.NotImplemented;
        }

        #endregion

    }

    public static partial class FloatOps {
        static ReflectedType FloatType;
        public static ReflectedType MakeDynamicType() {
            if (FloatType == null) {
                ReflectedType res = new OpsReflectedType("float", typeof(double), typeof(FloatOps), typeof(ExtensibleFloat));
                if (Interlocked.CompareExchange<ReflectedType>(ref FloatType, res, null) == null)
                    return res;
            }
            return FloatType;
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls) {
            Debug.Assert(cls != FloatType);
            return cls.ctor.Call(cls);
        }

        [PythonName("__new__")]
        public static object Make(PythonType cls, object o) {
            if (cls == FloatType) {
                if (o is string) {
                    try {
                        return LiteralParser.ParseFloat((string)o);
                    } catch (FormatException) {
                        throw Ops.ValueError("invalid literal for float()");
                    }
                }
                Conversion conv;
                object d = Converter.TryConvertToDouble(o, out conv);
                if (conv != Conversion.None) return d;

                if (o is Complex64) throw Ops.TypeError("can't convert complex to float; use abs(z)");

                d = Ops.Call(Ops.GetAttr(DefaultContext.Default, o, SymbolTable.ConvertToFloat));
                if (d is double) return d;
                throw Ops.TypeError("__float__ returned non-float (type %s)", Ops.GetDynamicType(d));
            } else {
                return cls.ctor.Call(cls, o);
            }
        }

        private static object TrueDivide(double x, long y) {
            if (y == 0) throw Ops.ZeroDivisionError();
            return x / y;
        }

        private static object TrueDivide(double x, double y) {
            if (y == 0) throw Ops.ZeroDivisionError();
            return x / y;
        }

        private static object TrueDivide(double x, BigInteger y) {
            if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
            return x / y;
        }

        private static object TrueDivide(double x, Complex64 y) {
            if (y.IsZero) throw Ops.ZeroDivisionError();
            return x / y;
        }

        [PythonName("__abs__")]
        public static object Abs(double x) {
            return Math.Abs(x);
        }

        [PythonName("__pow__")]
        public static object Power(double x, double y) {
            if (x == 0.0 && y < 0.0)
                throw Ops.ZeroDivisionError("0.0 cannot be raised to a negative power");
            if (x < 0 && (Math.Floor(y) != y)) {
                throw Ops.ValueError("negative number cannot be raised to fraction");
            }
            double result = Math.Pow(x, y);
            if (double.IsInfinity(result)) {
                throw Ops.OverflowError("result too large");
            }
            return result;
        }

        [PythonName("__div__")]
        public static object Divide(double x, object other) {
            return TrueDivide(x, other);
        }

        [PythonName("__eq__")]
        public static object Equals(double x, object other) {
            if (other == null) return false;

            if (other is int) return Ops.Bool2Object(x == (int)other);
            else if (other is double) return Ops.Bool2Object(x == (double)other);
            else if (other is BigInteger) return Ops.Bool2Object(x == (BigInteger)other);
            else if (other is long) return Ops.Bool2Object(x == (long)other);
            else if (other is Complex64) return Ops.Bool2Object(x == (Complex64)other);
            else if (other is float) return Ops.Bool2Object(x == (float)other);
            else if (other is ExtensibleFloat) return Ops.Bool2Object(x == ((ExtensibleFloat)other).value);
            else if (other is bool) return Ops.Bool2Object((bool)other ? x == 1 : x == 0);
            else if (other is decimal) return Ops.Bool2Object(x == (double)(decimal)other);

            Conversion conversion;
            double y = Converter.TryConvertToInt64(other, out conversion);
            if (conversion != Conversion.None) return Ops.Bool2Object(x == y);

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.Equal(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.NotImplemented;
        }

        [PythonName("__eq__")]
        public static object Equals(float x, object other) {
            // need to not promote to double, as that may throw
            // off our number...  instead demote double to float, and
            // the compare.
            if (other is double) return x == (float)(double)other;

            return Equals((double)x, other);
        }

        public static bool EqualsRetBool(double x, object other) {
            if (other is int) return x == (int)other;
            else if (other is double) return x == (double)other;
            else if (other is BigInteger) return x == (BigInteger)other;
            else if (other is long) return x == (long)other;
            else if (other is Complex64) return x == (Complex64)other;
            else if (other is float) return x == (float)other;
            else if (other is ExtensibleFloat) return x == ((ExtensibleFloat)other).value;
            else if (other is bool) return (bool)other ? x == 1 : x == 0;
            else if (other is decimal) return x == (double)(decimal)other;

            Conversion conversion;
            double y = Converter.TryConvertToInt64(other, out conversion);
            if (conversion != Conversion.None) return x == y;

            object res = Ops.GetDynamicType(other).Coerce(other, x);
            if (res != Ops.NotImplemented && !(res is OldInstance)) {
                return Ops.EqualRetBool(((Tuple)res)[1], ((Tuple)res)[0]);
            }

            return Ops.DynamicEqualRetBool(x, other);
        }

        public static bool EqualsRetBool(float x, object other) {
            return EqualsRetBool((double)x, other);
        }

        [PythonName("__neg__")]
        public static object Negate(double x) {
            return -x;
        }

        #region ToString

        public static string ToString(double x) {
            StringFormatter sf = new StringFormatter("%.12g", x);
            sf.TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        [PythonName("__str__")]
        public static string ToString(double x, IFormatProvider provider) {
            return x.ToString(provider);
        }
        [PythonName("__str__")]
        public static string ToString(double x, string format) {
            return x.ToString(format);
        }
        [PythonName("__str__")]
        public static string ToString(double x, string format, IFormatProvider provider) {
            return x.ToString(format, provider);
        }

        public static string ToString(float x) {
            // Python does not natively support System.Single. However, we try to provide
            // formatting consistent with System.Double.
            StringFormatter sf = new StringFormatter("%.6g", x);
            sf.TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        #endregion

        [PythonName("__coerce__")]
        public static double Coerce(double x, object o) {
            double d = (double)Make(FloatType, o);

            if (Double.IsInfinity(d)) {
                throw Ops.OverflowError("number too big");
            }

            return d;
        }

        [PythonName("__pow__")]
        public static object Power(double x, object other) {
            if (other is int) return Power(x, ((int)other));
            if (other is long) return Power(x, ((long)other));
            if (other is Complex64) return ComplexOps.Power(x, ((Complex64)other));
            if (other is double) return Power(x, ((double)other));
            if (other is BigInteger) return Power(x, ((BigInteger)other));
            if (other is bool) return Power(x, (bool)other ? 1 : 0);
            if (other is float) return Power(x, ((float)other));
            if (other is ExtensibleFloat) return Power(x, ((ExtensibleFloat)other).value);
            if (other is ExtensibleInt) return Power(x, ((ExtensibleInt)other).value);
            if (other is ExtensibleComplex) return ComplexOps.Power(x, ((ExtensibleComplex)other).value);
            if (other is byte) return Power(x, (int)((byte)other));
            return Ops.NotImplemented;
        }

        [PythonName("__floordiv__")]
        public static object FloorDivide(double x, object other) {
            if (other is int) {
                int y = (int)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is long) {
                long y = (long)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            if (other is double) {
                double y = (double)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is BigInteger) {
                BigInteger y = (BigInteger)other;
                if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is ExtensibleFloat) {
                ExtensibleFloat y = (ExtensibleFloat)other;
                if (y.value == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y.value);
            }
            if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }

            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                if (y == 0) throw Ops.ZeroDivisionError();
                return Math.Floor(x / y);
            }
            return Ops.NotImplemented;
        }

        private static double Modulo(double x, double y) {
            double r = x % y;
            if (r > 0 && y < 0) {
                r = r + y;
            } else if (r < 0 && y > 0) {
                r = r + y;
            }
            return r;
        }

        [PythonName("__mod__")]
        public static object Mod(double x, object other) {
            if (other is int) {
                int y = (int)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is long) {
                long y = (long)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is double) {
                double y = (double)other;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is BigInteger) {
                BigInteger y = (BigInteger)other;
                if (y == BigInteger.Zero) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is ExtensibleFloat) {
                ExtensibleFloat y = (ExtensibleFloat)other;
                if (y.value == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y.value);
            }
            if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }
            if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if (y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            if (other is IConvertible) {
                double y = ((IConvertible)other).ToDouble(null);
                if (y == 0) throw Ops.ZeroDivisionError();
                return Modulo(x, y);
            }

            return Ops.NotImplemented;
        }

        [PythonName("__int__")]
        public static object ToInteger(double d) {
            if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                return (int)d;
            } else if (Int64.MinValue <= d && d <= Int64.MaxValue) {
                return (long)d;
            } else {
                return BigInteger.Create(d);
            }
        }

        [PythonName("__cmp__")]
        public static object Compare(double self, object other) {
            if (other == null) return 1;

            Conversion conv;
            double val = Converter.TryConvertToDouble(other, out conv);
            if (conv == Conversion.None) {
                object res = Ops.GetDynamicType(other).Coerce(other, self);
                if (res != Ops.NotImplemented && !(res is OldInstance)) {
                    return Ops.Compare(((Tuple)res)[1], ((Tuple)res)[0]);
                }

                Complex64 c64 = Converter.TryConvertToComplex64(other, out conv);
                if (conv != Conversion.None) {
                    return ComplexOps.TrueCompare(c64, new Complex64(self)) * -1;
                }
                
                return Ops.NotImplemented;
            }


            if (val == self) return 0;
            if (self < val) return -1;
            return 1;
        }

    }
}

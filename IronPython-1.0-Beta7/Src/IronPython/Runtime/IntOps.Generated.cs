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
    public static partial class IntOps {
        #region Generated IntOps

        // *** BEGIN GENERATED CODE ***


        [PythonName("__add__")]
        public static object Add(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) + (BigInteger)other;
            } else if (other is double) {
                return x + (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Add(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x + (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x + y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is float) {
                return x + (float)other;
            } else if (other is byte) {
                return x + (byte)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            } else if (other is ExtensibleFloat) {
                return x + ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Add(Complex64.MakeReal(x), (ExtensibleComplex)other);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x + y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) + y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__sub__")]
        public static object Subtract(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) - (BigInteger)other;
            } else if (other is double) {
                return x - (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Subtract(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x - (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x - y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is float) {
                return x - (float)other;
            } else if (other is byte) {
                return x - (byte)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            } else if (other is ExtensibleFloat) {
                return x - ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Subtract(Complex64.MakeReal(x), (ExtensibleComplex)other);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x - y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) - y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mul__")]
        public static object Multiply(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is BigInteger) {
                return BigInteger.Create(x) * (BigInteger)other;
            } else if (other is double) {
                return x * (double)other;
            } else if (other is Complex64) {
                return ComplexOps.Multiply(Complex64.MakeReal(x), other);
            } else if (other is bool) {
                bool b = (bool)other;
                return x * (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return checked(x * y);
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is float) {
                return x * (float)other;
            } else if (other is byte) {
                return x * (byte)other;
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            } else if (other is ExtensibleFloat) {
                return x * ((ExtensibleFloat)other).value;
            } else if (other is ExtensibleComplex) {
                return ComplexOps.Multiply(Complex64.MakeReal(x), (ExtensibleComplex)other);
            } else if (other is byte) {
                int y = (byte)other;
                try {
                    return Ops.Int2Object(checked(x * y));
                } catch (OverflowException) {
                    return BigInteger.Create(x) * y;
                }
            }
            return Ops.NotImplemented;
        }


        [PythonName("__div__")]
        public static object Divide(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x) , y);
                }
            } else if (other is BigInteger) {
                return LongOps.Divide(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.Divide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x / (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.Divide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Divide(x, (int)((byte)other)));
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.Divide(BigInteger.Create(x) , y);
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.Divide(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Divide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__floordiv__")]
        public static object FloorDivide(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x) , y);
                }
            } else if (other is BigInteger) {
                return LongOps.FloorDivide(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.FloorDivide(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x / (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Divide(x, y);
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.FloorDivide(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Divide(x, (int)((byte)other)));
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(Divide(x, y));
                } catch (OverflowException) {
                    return LongOps.FloorDivide(BigInteger.Create(x) , y);
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.FloorDivide(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.FloorDivide(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__truediv__")]
        public static object TrueDivide(int x, object other) {
            if (other is int) {
                return TrueDivide(x, (int)other);
            } else if (other is double) {
                return TrueDivide(x, (double)other);
            } else if (other is long) {
                return TrueDivide(x, (long)other);
            } else if (other is BigInteger) {
                return TrueDivide(x, (BigInteger)other);
            } else if (other is bool) {
                return TrueDivide(x, (bool)other ? 1 : 0);
            } else if (other is Complex64) {
                return TrueDivide(x, (Complex64)other);
            } else if (other is ExtensibleInt) {
                return TrueDivide(x, ((ExtensibleInt)other).value);
            } else if (other is ExtensibleFloat) {
                return TrueDivide(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                return TrueDivide(x, ((ExtensibleComplex)other).value);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__mod__")]
        public static object Mod(int x, object other) {
            if (other is int) {
                int y = (int)other;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x) , y);
                }
            } else if (other is BigInteger) {
                return LongOps.Mod(BigInteger.Create(x), (BigInteger)other);
            } else if (other is double) {
                return FloatOps.Mod(x, (double)other);
            } else if (other is Complex64) {
                Complex64 y = (Complex64)other;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            } else if (other is bool) {
                bool b = (bool)other;
                return x % (b ? 1 : 0);
            } else if (other is long) {
                long y = (long)other;
                try {
                    return Mod(x, y);
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x), y);
                }
            } else if (other is float) {
                return FloatOps.Mod(x, (float)other);
            } else if (other is byte) {
                return Ops.Int2Object(Mod(x, (int)((byte)other)));
            } else if (other is ExtensibleInt) {
                int y = ((ExtensibleInt)other).value;
                try {
                    return Ops.Int2Object(Mod(x, y));
                } catch (OverflowException) {
                    return LongOps.Mod(BigInteger.Create(x) , y);
                }
            } else if (other is ExtensibleFloat) {
                return FloatOps.Mod(x, ((ExtensibleFloat)other).value);
            } else if (other is ExtensibleComplex) {
                Complex64 y = ((ExtensibleComplex)other).value;
                if(y.IsZero) throw Ops.ZeroDivisionError();
                return ComplexOps.Mod(Complex64.MakeReal(x), y);
            }
            return Ops.NotImplemented;
        }


        [PythonName("__and__")]
        public static object BitwiseAnd(int x, object other) {
            if (other is int) {
                return Ops.Int2Object(x & (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx & (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) & (BigInteger)other;
            } else if (other is bool) {
                return Ops.Int2Object(x & ((bool)other ? 1 : 0));
            } else if (other is ExtensibleInt) {
                return Ops.Int2Object(x & ((ExtensibleInt)other).value);
            } else if (other is byte) {
                return Ops.Int2Object(x & (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__or__")]
        public static object BitwiseOr(int x, object other) {
            if (other is int) {
                return Ops.Int2Object(x | (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx | (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) | (BigInteger)other;
            } else if (other is bool) {
                return Ops.Int2Object(x | ((bool)other ? 1 : 0));
            } else if (other is ExtensibleInt) {
                return Ops.Int2Object(x | ((ExtensibleInt)other).value);
            } else if (other is byte) {
                return Ops.Int2Object(x | (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        [PythonName("__xor__")]
        public static object Xor(int x, object other) {
            if (other is int) {
                return Ops.Int2Object(x ^ (int)other);
            } else if (other is long) {
                long lx = (long)x;
                return lx ^ (long)other;
            } else if (other is BigInteger) {
                return BigInteger.Create(x) ^ (BigInteger)other;
            } else if (other is bool) {
                return Ops.Int2Object(x ^ ((bool)other ? 1 : 0));
            } else if (other is ExtensibleInt) {
                return Ops.Int2Object(x ^ ((ExtensibleInt)other).value);
            } else if (other is byte) {
                return Ops.Int2Object(x ^ (int)((byte)other));
            }
            return Ops.NotImplemented;
        }


        // *** END GENERATED CODE ***

        #endregion

    }
}
